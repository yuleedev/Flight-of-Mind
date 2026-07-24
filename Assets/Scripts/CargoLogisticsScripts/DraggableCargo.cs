using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class DraggableCargo : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Simulated fall physics")]
    [SerializeField] private float fallGravity = 2500f;
    [SerializeField] private float fallStartVelocity = 0f;

    [Header("Bounce physics")]
    [Tooltip("Coefficient of restitution: fraction of impact speed kept after each bounce. Real wood-on-wood crates are not very elastic - 0.2 to 0.4 looks right. 0 = no bounce (old behavior). 1 = bounces forever (unrealistic, avoid).")]
    [SerializeField, Range(0f, 1f)] private float bounciness = 0.35f;
    [Tooltip("Once the rebound speed after an impact would drop below this (pixels/second), the crate is considered settled and stops bouncing instead of doing an imperceptible micro-bounce.")]
    [SerializeField] private float minBounceSpeed = 60f;
    [Tooltip("Hard safety cap on bounce count, in case of an unusually large starting velocity.")]
    [SerializeField] private int maxBounces = 8;

    [Header("Stacking size")]
    [Tooltip("How tall this crate's VISIBLE art is, in canvas pixels. The stack reserves exactly this much vertical space for the crate. Each crate needs its own value. Requires the crate's Scale to be (1,1,1).")]
    [SerializeField] private float visualHeight = 180f;

    [Header("Sound Effects")]
    [Tooltip("Short whoosh/drop sound played once when the crate is released and starts falling. Leave empty for no sound - the game works fine either way.")]
    [SerializeField] private AudioClip fallSound;
    [Tooltip("Wood-on-wood impact sound played on landing and again on every bounce after it. Leave empty for no sound.")]
    [SerializeField] private AudioClip collisionSound;
    [Tooltip("Impact speed (pixels/second) at or above which the collision sound plays at Max Collision Volume. Softer impacts (later, smaller bounces) scale down from this automatically.")]
    [SerializeField] private float referenceImpactSpeed = 1400f;
    [Tooltip("Volume the collision sound plays at when impact speed >= Reference Impact Speed.")]
    [SerializeField, Range(0f, 1f)] private float maxCollisionVolume = 0.85f;
    [Tooltip("Volume the fall/whoosh sound plays at.")]
    [SerializeField, Range(0f, 1f)] private float fallSoundVolume = 0.6f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private Image image;
    private StackSlot startSlot;
    private AudioSource audioSource;
    private bool isValidDrag;
    private bool isFalling;

    public bool IsFalling => isFalling;
    public float VisualHeight => visualHeight;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D UI sound, not positioned in world space

        NormalizeAnchors();
        StripPhysicsComponents();
    }

    private void NormalizeAnchors()
    {
        Vector2 renderedSize = rectTransform.rect.size;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = renderedSize;
    }

    private void StripPhysicsComponents()
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) Destroy(rb);

        foreach (var col in GetComponents<Collider2D>())
            Destroy(col);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (thinkingTime.Instance != null) thinkingTime.Instance.StopTiming();

        StackSlot.ClearAllHighlights();

        if (CargoLogisticsManager.Instance.IsGameOver) { isValidDrag = false; return; }
        if (isFalling) { isValidDrag = false; return; }

        startSlot = GetComponentInParent<StackSlot>();
        isValidDrag = startSlot != null && startSlot.Top == transform;

        if (!isValidDrag) return;

        canvasGroup.blocksRaycasts = false;
        transform.SetParent(canvas.transform, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isValidDrag) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        UpdateDropHighlight();
    }

    private void UpdateDropHighlight()
    {
        StackSlot hovered = GetMostOverlappingSlot();

        for (int i = 0; i < StackSlot.All.Count; i++)
        {
            StackSlot slot = StackSlot.All[i];

            if (slot != hovered || slot == startSlot)
            {
                slot.SetHighlight(false, false);
                continue;
            }

            slot.SetHighlight(true, !slot.IsFull);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isValidDrag) return;
        isValidDrag = false;
        canvasGroup.blocksRaycasts = true;
        StackSlot.ClearAllHighlights();

        StackSlot targetSlot = GetMostOverlappingSlot();

        bool isValidDrop =
            targetSlot != null &&
            targetSlot != startSlot &&
            !targetSlot.IsFull;

        if (!isValidDrop)
        {
            ReturnToStart();
            return;
        }

        StackSlot fromSlot = startSlot;

        transform.SetParent(targetSlot.transform, true);
        transform.SetAsLastSibling();

        rectTransform.anchoredPosition = new Vector2(0f, rectTransform.anchoredPosition.y);
        float targetY = targetSlot.GetRestingYForTopItem();

        StartCoroutine(FallToPosition(targetY, () =>
        {
            targetSlot.RestackItems();
            fromSlot.RestackItems();
            CargoLogisticsManager.Instance.RegisterMove();
        }));
    }

    private void ReturnToStart()
    {
        transform.SetParent(startSlot.transform, true);
        transform.SetAsLastSibling();

        rectTransform.anchoredPosition = new Vector2(0f, rectTransform.anchoredPosition.y);
        float targetY = startSlot.GetRestingYForTopItem();

        StartCoroutine(FallToPosition(targetY, () => startSlot.RestackItems()));
    }
    private IEnumerator FallToPosition(float targetY, Action onLanded)
    {
        isFalling = true;

        PlaySound(fallSound, fallSoundVolume);

        float y = rectTransform.anchoredPosition.y;
        float velocityY = -Mathf.Abs(fallStartVelocity);
        int bounceCount = 0;

        while (true)
        {
            velocityY -= fallGravity * Time.deltaTime;
            y += velocityY * Time.deltaTime;

            if (y <= targetY)
            {
                y = targetY;
                float impactSpeed = -velocityY; 

                bool canStillBounce = bounceCount < maxBounces && impactSpeed * bounciness > minBounceSpeed;

                PlaySound(collisionSound, ImpactVolume(impactSpeed));

                if (canStillBounce)
                {
                    velocityY = impactSpeed * bounciness;
                    bounceCount++;
                }
                else
                {
                    velocityY = 0f;
                    rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, y);
                    break;
                }
            }

            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, y);
            yield return null;
        }

        isFalling = false;
        onLanded?.Invoke();
    }

    private float ImpactVolume(float impactSpeed)
    {
        float t = Mathf.Clamp01(impactSpeed / Mathf.Max(1f, referenceImpactSpeed));
        return t * maxCollisionVolume;
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip, volume);
    }

    private StackSlot GetMostOverlappingSlot()
    {
        StackSlot best = null;
        float bestArea = 0f;

        foreach (var slot in StackSlot.All)
        {
            float area = slot.OverlapArea(rectTransform);
            if (area > bestArea)
            {
                bestArea = area;
                best = slot;
            }
        }
        return best;
    }
}