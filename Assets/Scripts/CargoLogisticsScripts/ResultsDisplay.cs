using UnityEngine;
using UnityEngine.UI;

public class ResultDisplay
{
    // Creates the bottom result banner from code when the game ends.
    public static void Show(Canvas canvas, int movesTaken, int optimalMoves)
    {
        GameObject panel = new GameObject("ResultPanel", typeof(RectTransform));
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.SetParent(canvas.transform, false);
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 30f);
        rt.sizeDelta = new Vector2(700f, 80f);

        panel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);

        GameObject textGo = new GameObject("ResultText", typeof(RectTransform));
        RectTransform textRt = textGo.GetComponent<RectTransform>();
        textRt.SetParent(rt, false);
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        var text = textGo.AddComponent<TMPro.TextMeshProUGUI>();
        text.fontSize = 36f;
        text.alignment = TMPro.TextAlignmentOptions.Center;
        text.text = movesTaken == optimalMoves
            ? $"Solved in {movesTaken} moves -- optimal!"
            : $"Solved in {movesTaken} moves (optimal was {optimalMoves})";
    }
}