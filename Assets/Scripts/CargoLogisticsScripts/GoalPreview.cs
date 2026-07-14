using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GoalPreview
{
    //takes whichever block is in a specific position at the scene and then constructs it's own on an small display image on the top left side of the screendd.
    public static void Build(Canvas canvas, List<List<string>> goalStacks, Dictionary<string, Sprite> itemSprites)
    {
    
        GameObject root = new GameObject("GoalPreview", typeof(RectTransform));
        RectTransform rootRt = root.GetComponent<RectTransform>();
        rootRt.SetParent(canvas.transform, false);
        rootRt.anchorMin = new Vector2(0f, 1f);
        rootRt.anchorMax = new Vector2(0f, 1f);
        rootRt.pivot = new Vector2(0f, 1f);
        rootRt.anchoredPosition = new Vector2(20f, -20f);
        rootRt.sizeDelta = new Vector2(300f, 230f);

        GameObject labelGo = new GameObject("Label", typeof(RectTransform));
        RectTransform labelRt = labelGo.GetComponent<RectTransform>();
        labelRt.SetParent(rootRt, false);
        labelRt.anchorMin = new Vector2(0f, 1f);
        labelRt.anchorMax = new Vector2(1f, 1f);
        labelRt.pivot = new Vector2(0.5f, 1f);
        labelRt.anchoredPosition = Vector2.zero;
        labelRt.sizeDelta = new Vector2(0f, 30f);
        var label = labelGo.AddComponent<TMPro.TextMeshProUGUI>();
        label.text = "Goal:";
        label.fontSize = 24f;
        label.alignment = TMPro.TextAlignmentOptions.Left;

        float swatchWidth = 40f;
        float swatchHeight = 40f;
        float pegSpacing = 90f;
        float baselineY = -190f;
        int[] capacities = { 3, 2, 1 };
        string[] pegLabels = { "A", "B", "C" };

        for (int s = 0; s < goalStacks.Count && s < capacities.Length; s++)
        {
            float x = 50f + s * pegSpacing;
            float pegHeight = capacities[s] * (swatchHeight + 4f) + 10f;

            GameObject peg = new GameObject($"Peg_{s}", typeof(RectTransform));
            RectTransform pegRt = peg.GetComponent<RectTransform>();
            pegRt.SetParent(rootRt, false);
            pegRt.anchorMin = new Vector2(0f, 1f);
            pegRt.anchorMax = new Vector2(0f, 1f);
            pegRt.pivot = new Vector2(0.5f, 0f);
            pegRt.sizeDelta = new Vector2(8f, pegHeight);
            pegRt.anchoredPosition = new Vector2(x, baselineY);
            peg.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.25f);

            GameObject pegLabelGo = new GameObject($"PegLabel_{s}", typeof(RectTransform));
            RectTransform pegLabelRt = pegLabelGo.GetComponent<RectTransform>();
            pegLabelRt.SetParent(rootRt, false);
            pegLabelRt.anchorMin = new Vector2(0f, 1f);
            pegLabelRt.anchorMax = new Vector2(0f, 1f);
            pegLabelRt.pivot = new Vector2(0.5f, 1f);
            pegLabelRt.sizeDelta = new Vector2(40f, 20f);
            pegLabelRt.anchoredPosition = new Vector2(x, baselineY - 4f);
            var pegLabelText = pegLabelGo.AddComponent<TMPro.TextMeshProUGUI>();
            pegLabelText.text = pegLabels[s];
            pegLabelText.fontSize = 16f;
            pegLabelText.alignment = TMPro.TextAlignmentOptions.Center;
            pegLabelText.color = new Color(1f, 1f, 1f, 0.6f);

            for (int i = 0; i < goalStacks[s].Count; i++)
            {
                GameObject swatch = new GameObject($"Swatch_{s}_{i}", typeof(RectTransform));
                RectTransform rt = swatch.GetComponent<RectTransform>();
                rt.SetParent(rootRt, false);
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(swatchWidth, swatchHeight);

                int heightFromBottom = (goalStacks[s].Count - 1) - i;
                float y = baselineY + 5f + (swatchHeight * 0.5f) + heightFromBottom * (swatchHeight + 4f);
                rt.anchoredPosition = new Vector2(x, y);

                var img = swatch.AddComponent<Image>();
                string itemName = goalStacks[s][i];

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
    }
}