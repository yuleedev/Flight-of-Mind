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

    [Header("Stacking size")]
    [Tooltip("Leave at 0. The crate's real rendered height is measured automatically and " +
             "accounts for Preserve Aspect letterboxing. Only set this above 0 if the PNG " +
             "itself has baked-in transparent padding that measuring can't see.")]
    [SerializeField] private float visualHeightOverride = 0f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private Image image;
    private StackSlot startSlot;
    private bool isValidDrag;
    private bool isFalling;

    public bool IsFalling => isFalling;


    public float VisualHeight => visualHeightOverride > 0f ? visualHeightOverride : MeasureRenderedHeight();

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();

        NormalizeAnchors();
        StripPhysicsComponents();
    }

    private void NormalizeAnchors()
    {
        Vector2 renderedSize = rectTransform.rect.size; // capture BEFORE touching anchors
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot     = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = renderedSize;
    }


    private void StripPhysicsComponents()
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Debug.LogWarning($"{name}: removed Rigidbody2D — this is Canvas UI positioned via RectTransform, not Physics2D.");
            Destroy(rb);
        }
        foreach (var col in GetComponents<Collider2D>())
        {
            Debug.LogWarning($"{name}: removed {col.GetType().Name} — drop detection runs via StackSlot.OverlapArea(), not Collider2D.");
            Destroy(col);
        }
    }

 
    private float MeasureRenderedHeight()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        float boxWidth  = rectTransform.rect.width;
        float boxHeight = rectTransform.rect.height;

        if (image == null) image = GetComponent<Image>();
        if (image == null || image.sprite == null || !image.preserveAspect) return boxHeight;

        Rect spriteRect = image.sprite.rect;
        if (spriteRect.height <= 0f || boxHeight <= 0f) return boxHeight;

        float spriteAspect = spriteRect.width / spriteRect.height;
        float boxAspect    = boxWidth / boxHeight;


        if (spriteAspect > boxAspect) return boxWidth / spriteAspect;

        return boxHeight;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (thinkingTime.Instance != null) thinkingTime.Instance.StopTiming();

        if (CargoLogisticsManager.Instance.IsGameOver) { isValidDrag = false; return; }
        if (isFalling) { isValidDrag = false; return; }

        startSlot = GetComponentInParent<StackSlot>();
        isValidDrag = startSlot != null && startSlot.Top == transform;

        if (!isValidDrag)
        {
            Debug.Log($"{name}: can't drag — not the top item of its slot.");
            return;
        }

        canvasGroup.blocksRaycasts = false;
        transform.SetParent(canvas.transform, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isValidDrag) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isValidDrag) return;
        isValidDrag = false;
        canvasGroup.blocksRaycasts = true;

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

        // worldPositionStays: true on purpose — the crate must stay visually where the player
        // released it, then fall from there. Passing false would teleport it.
        transform.SetParent(targetSlot.transform, true);
        transform.SetAsLastSibling(); // last sibling == top of the stack

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
        transform.SetAsLastSibling(); // it was the top item, it goes back on top

        rectTransform.anchoredPosition = new Vector2(0f, rectTransform.anchoredPosition.y);
        float targetY = startSlot.GetRestingYForTopItem();

        StartCoroutine(FallToPosition(targetY, () => startSlot.RestackItems()));
    }

    private IEnumerator FallToPosition(float targetY, Action onLanded)
    {
        isFalling = true;
        float velocity = fallStartVelocity;

        while (rectTransform.anchoredPosition.y > targetY)
        {
            velocity += fallGravity * Time.deltaTime;
            float newY = rectTransform.anchoredPosition.y - velocity * Time.deltaTime;
            if (newY <= targetY) newY = targetY;
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, newY);
            yield return null;
        }

        rectTransform.anchoredPosition = new Vector2(0f, targetY);
        isFalling = false;
        onLanded?.Invoke();
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