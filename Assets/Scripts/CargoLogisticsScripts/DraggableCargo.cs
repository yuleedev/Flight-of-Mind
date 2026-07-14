using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableCargo : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private StackSlot startSlot;
    private Vector2 startPosition;
    private bool isValidDrag;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }
// Tracks the position of the cargo if it is draggable, otherwise it refuses to move
   public void OnBeginDrag(PointerEventData eventData)
    {
       if (CargoLogisticsManager.Instance.IsGameOver) { isValidDrag = false; return; }

        startSlot = GetComponentInParent<StackSlot>();
        startSlot = GetComponentInParent<StackSlot>();
        isValidDrag = startSlot != null && startSlot.Top == transform;

        if (!isValidDrag)
        {
            Debug.Log($"{name}: can't drag — not the top item of its slot.");
            return;
        }

        startPosition = rectTransform.anchoredPosition;
        Debug.Log($"{name}: begin drag from {startSlot.name}");
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(canvas.transform, true);
    }
// Moves the cargo with the mouse while dragging
    public void OnDrag(PointerEventData eventData)
    {
        if (!isValidDrag) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }
// Handles the logic for when the cargo is dropped, checking if it is a valid drop and either stacking it or returning it to its original position
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isValidDrag) return;
        canvasGroup.blocksRaycasts = true;

        StackSlot targetSlot = GetMostOverlappingSlot();

        bool isValidDrop =
            targetSlot != null &&
            targetSlot != startSlot &&
            !targetSlot.IsFull;

        if (isValidDrop)
        {
            Debug.Log($"{name}: moved from {startSlot.name} → {targetSlot.name}");
            transform.SetParent(targetSlot.transform);
            transform.SetAsFirstSibling();
            targetSlot.RestackItems();
            startSlot.RestackItems();
            CargoLogisticsManager.Instance.RegisterMove();
            //return;
        }
        else
        {
            if (targetSlot == null)
                Debug.Log($"{name}: not touching any slot — returning to {startSlot.name}");
            else if (targetSlot == startSlot)
                Debug.Log($"{name}: still on original slot — returning");
            else
                Debug.Log($"{name}: {targetSlot.name} is full — returning to {startSlot.name}");

            ReturnToStart();
        }
    }

    private void ReturnToStart()
    {
        transform.SetParent(startSlot.transform);
        transform.SetAsFirstSibling();
        rectTransform.anchoredPosition = startPosition;
    }

    // Finds the slot whose rectangle this block overlaps most at release time
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
        return best;   // null if the block is touching nothing
    }
}