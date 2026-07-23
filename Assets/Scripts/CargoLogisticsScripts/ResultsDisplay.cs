using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultDisplay
{
    private static GameObject activePanel;

    public static void Show(Canvas canvas, string message)
    {
        BuildPanel(canvas, message, 80f, 32f);
    }

    private static void BuildPanel(Canvas canvas, string message, float height, float fontSize)
    {
        if (activePanel != null) Object.Destroy(activePanel);

        GameObject panel = new GameObject("ResultPanel", typeof(RectTransform));
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.SetParent(canvas.transform, false);
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 30f);
        rt.sizeDelta = new Vector2(700f, height);

        panel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);

        GameObject textGo = new GameObject("ResultText", typeof(RectTransform));
        RectTransform textRt = textGo.GetComponent<RectTransform>();
        textRt.SetParent(rt, false);
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        var text = textGo.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.text = message;

        activePanel = panel;

        if (FontManager.Instance != null)
            FontManager.Instance.SetDyslexia(FontManager.Instance.IsOn);
    }

    public static void Hide()
    {
        if (activePanel != null) Object.Destroy(activePanel);
        activePanel = null;
    }
}