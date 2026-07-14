using UnityEngine;
using System.Collections.Generic;

public class StackSlot : MonoBehaviour
{
    public static List<StackSlot> All = new List<StackSlot>();
    public int capacity = 3;

    [Header("Stacking layout")]
    public float spacingGap = 0f;    
    public float bottomPadding = 0f;  

    private RectTransform rectTransform;

    void Awake() => rectTransform = GetComponent<RectTransform>();
    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    public Transform Top => transform.childCount > 0 ? transform.GetChild(0) : null;
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

    // Positions items resting on this slot's floor, stacking upward. Uses each
    // item's own actual height is used.
    public void RestackItems()
    {
        float halfHeight = rectTransform.rect.height * 0.5f;
        float pivotOffset = (0.5f - rectTransform.pivot.y) * rectTransform.rect.height;
        float bottomEdge = -halfHeight + pivotOffset;

        int count = transform.childCount;
        float runningY = bottomEdge + bottomPadding;

        // Bottom most item updards
        for (int i = count - 1; i >= 0; i--)
        {
            RectTransform child = transform.GetChild(i).GetComponent<RectTransform>();
            float half = child.rect.height * 0.5f;
            child.anchoredPosition = new Vector2(0f, runningY + half);
            runningY += child.rect.height + spacingGap;
        }
    }
}