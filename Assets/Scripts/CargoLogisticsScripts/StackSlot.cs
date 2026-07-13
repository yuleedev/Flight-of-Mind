using UnityEngine;
using System.Collections.Generic;

public class StackSlot : MonoBehaviour
{
    public static List<StackSlot> All = new List<StackSlot>();
    public int capacity = 3; //ts a lie



    //Helps format how the cargo is stacked
    [Header("Stacking layout")]
    public float itemHeight = 90f;   
    public float bottomPadding = 10f; 

    private RectTransform rectTransform;

    void Awake() => rectTransform = GetComponent<RectTransform>();
    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    public Transform Top => transform.childCount > 0 ? transform.GetChild(0) : null;
    public bool IsFull => transform.childCount >= capacity;
    //If the cargo is touching the stack then it is a valid drop, when that valid drop happens it gets stacked
    public float OverlapArea(RectTransform item)
    {
        Rect slotRect = GetWorldRect(rectTransform);
        Rect itemRect = GetWorldRect(item);

        float xOverlap = Mathf.Max(0, Mathf.Min(slotRect.xMax, itemRect.xMax) - Mathf.Max(slotRect.xMin, itemRect.xMin));
        float yOverlap = Mathf.Max(0, Mathf.Min(slotRect.yMax, itemRect.yMax) - Mathf.Max(slotRect.yMin, itemRect.yMin));
        return xOverlap * yOverlap;
    }
    //Helps with that collosion detection of the Cargo and the Stack.
    private Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        return new Rect(corners[0].x, corners[0].y,
                        corners[2].x - corners[0].x,
                        corners[2].y - corners[0].y);
    }

    // Positions items resting on this slots edge, stacking upwards, just like the normal game
    public void RestackItems()
    {
        
        float halfHeight = rectTransform.rect.height * 0.5f;
        float pivotOffset = (0.5f - rectTransform.pivot.y) * rectTransform.rect.height;
        float bottomEdge = -halfHeight + pivotOffset;

        int count = transform.childCount;
        for (int i = 0; i < count; i++)
        {
            RectTransform child = transform.GetChild(i).GetComponent<RectTransform>();
            int heightFromBottom = (count - 1) - i; 

           
            float y = bottomEdge + bottomPadding + (child.rect.height * 0.5f) + heightFromBottom * itemHeight;
            child.anchoredPosition = new Vector2(0f, y);
        }
    }
}