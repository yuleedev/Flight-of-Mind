using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultDisplay
{
    private static readonly Color InkColor = new Color(0.996f, 0.984f, 0.855f, 1f);

    private static GameObject activePanel;
    private static TMP_FontAsset font;

    public static void SetFont(TMP_FontAsset fontAsset)
    {
        font = fontAsset;
    }

    public static void Show(Canvas canvas, string message)
    {
        BuildPanel(canvas, message, 96f, 34f);
    }

    public static void Show(Canvas canvas, string message, float height, float fontSize)
    {
        BuildPanel(canvas, message, height, fontSize);
    }

    private static void BuildPanel(Canvas canvas, string message, float height, float fontSize)
    {
        if (activePanel != null) Object.Destroy(activePanel);
        if (canvas == null) return;

        GameObject panel = new GameObject("ResultPanel", typeof(RectTransform));
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.SetParent(canvas.transform, false);
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 40f);
        rt.sizeDelta = new Vector2(760f, height);

        panel.AddComponent<Image>();
        panel.AddComponent<RoundedPanel>();

        GameObject textGo = new GameObject("ResultText", typeof(RectTransform));
        RectTransform textRt = textGo.GetComponent<RectTransform>();
        textRt.SetParent(rt, false);
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        var text = textGo.AddComponent<TextMeshProUGUI>();

        if (font != null)
            text.font = font;

        text.fontSize = fontSize;
        text.color = InkColor;
        text.alignment = TextAlignmentOptions.Center;
        text.margin = new Vector4(30f, 14f, 30f, 14f);
        text.enableAutoSizing = true;
        text.fontSizeMin = 18f;
        text.fontSizeMax = fontSize;
        text.raycastTarget = false;
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
