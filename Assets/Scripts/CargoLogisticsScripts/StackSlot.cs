using UnityEngine;
using System.Collections.Generic;

public class StackSlot : MonoBehaviour
{
    public static List<StackSlot> All = new List<StackSlot>();

    [Tooltip("3 / 2 / 1 for Slot_A / Slot_B / Slot_C. Changing these silently invalidates " +
             "every optimalMoves value baked into ProblemLibrary.")]
    public int capacity = 3;

    [Header("Stacking layout")]
    [Tooltip("Extra space between crates. Keep at 0 for crates that physically touch.")]
    public float spacingGap = 0f;
    [Tooltip("Distance from the slot's bottom edge up to the floor the bottom crate rests on.")]
    public float bottomPadding = 0f;

    private RectTransform rectTransform;


    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Destroy(rb);
        }
        foreach (var col in GetComponents<Collider2D>())
        {
            Destroy(col);
        }
    }

    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    public Transform Top => transform.childCount > 0 ? transform.GetChild(transform.childCount - 1) : null;
    public bool IsFull => transform.childCount >= capacity;

    public float OverlapArea(RectTransform item)
    {
        Rect slotRect = GetWorldRect(rectTransform);
        Rect itemRect = GetWorldRect(item);

        float xOverlap = Mathf.Max(0, Mathf.Min(slotRect.xMax, itemRect.xMax) - Mathf.Max(slotRect.xMin, itemRect.xMin));
        float yOverlap = Mathf.Max(0, Mathf.Min(slotRect.yMax, itemRect.yMax) - Mathf.Max(slotRect.yMin, itemRect.yMin));
        return xOverlap * yOverlap;
    }

    private Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        return new Rect(corners[0].x, corners[0].y,
                        corners[2].x - corners[0].x,
                        corners[2].y - corners[0].y);
    }

    private float GetItemHeight(Transform item)
    {
        DraggableCargo cargo = item.GetComponent<DraggableCargo>();
        if (cargo != null) return cargo.VisualHeight;

        RectTransform rt = item.GetComponent<RectTransform>();
        return rt != null ? rt.rect.height : 0f;
    }


    private float FloorY()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        return -rectTransform.rect.height * 0.5f + bottomPadding;
    }


    public void RestackItems()
    {
        float runningY = FloorY();
        int count = transform.childCount;

        for (int i = 0; i < count; i++)
        {
            Transform child = transform.GetChild(i);
            RectTransform childRt = child.GetComponent<RectTransform>();
            if (childRt == null) continue;

            float height = GetItemHeight(child);
            childRt.anchoredPosition = new Vector2(0f, runningY + height * 0.5f);
            runningY += height + spacingGap;
        }
    }


    public float GetRestingYForTopItem()
    {
        int count = transform.childCount;
        float runningY = FloorY();
        if (count == 0) return runningY;

        for (int i = 0; i < count - 1; i++)
            runningY += GetItemHeight(transform.GetChild(i)) + spacingGap;

        return runningY + GetItemHeight(transform.GetChild(count - 1)) * 0.5f;
    }
}