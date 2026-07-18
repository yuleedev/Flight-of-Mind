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
    [Tooltip("How tall this crate's VISIBLE art is, in canvas pixels. The stack reserves exactly this much vertical space for the crate. Each crate needs its own value. Requires the crate's Scale to be (1,1,1).")]
    [SerializeField] private float visualHeight = 180f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private Image image;
    private StackSlot startSlot;
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