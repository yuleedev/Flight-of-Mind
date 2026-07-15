using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GoalSlotDisplay : MonoBehaviour
{
    [SerializeField] private float swatchSize = 40f;
    [SerializeField] private float spacingGap = 6f;
    [SerializeField] private float bottomPadding = 10f;

    public void ShowStack(List<string> items, Dictionary<string, Sprite> itemSprites)
    {
        Clear();

        RectTransform rt = GetComponent<RectTransform>();
        float halfHeight = rt.rect.height * 0.5f;
        float pivotOffset = (0.5f - rt.pivot.y) * rt.rect.height;
        float bottomEdge = -halfHeight + pivotOffset;
        float runningY = bottomEdge + bottomPadding;

        for (int i = items.Count - 1; i >= 0; i--)
        {
            GameObject swatch = new GameObject($"GoalItem_{i}", typeof(RectTransform));
            RectTransform swatchRt = swatch.GetComponent<RectTransform>();
            swatchRt.SetParent(transform, false);
            swatchRt.anchorMin = new Vector2(0.5f, 0f);
            swatchRt.anchorMax = new Vector2(0.5f, 0f);
            swatchRt.pivot = new Vector2(0.5f, 0.5f);
            swatchRt.sizeDelta = new Vector2(swatchSize, swatchSize);
            swatchRt.anchoredPosition = new Vector2(0f, runningY + swatchSize * 0.5f);
            runningY += swatchSize + spacingGap;

            var img = swatch.AddComponent<Image>();
            string itemName = items[i];

            if (itemSprites.TryGetValue(itemName, out Sprite sprite) && sprite != null)
            {
                img.sprite = sprite;
                img.color = Color.white;
                img.preserveAspect = true;
            }
            else
            {
                img.color = Color.gray;
            }
        }
    }

    public void Clear()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
    }
}