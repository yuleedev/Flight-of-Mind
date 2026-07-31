using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreDial : MonoBehaviour
{
    public const float CardHeight = 146f;

    private static readonly Color InkColor = new Color(0.173f, 0.204f, 0.341f, 1f);
    private static readonly Color CardColor = new Color(1f, 1f, 1f, 0.85f);
    private static readonly Color FaceColor = new Color(0.945f, 0.957f, 0.984f, 1f);
    private static readonly Color TrackColor = new Color(0.173f, 0.204f, 0.341f, 0.10f);

    private static readonly Color LowColor = new Color(0.45f, 0.50f, 0.72f, 1f);
    private static readonly Color MidColor = new Color(0.29f, 0.45f, 0.83f, 1f);
    private static readonly Color HighColor = new Color(0.09f, 0.66f, 0.55f, 1f);

    private const float RingSize = 118f;
    private const float RingInset = 16f;
    private const float SweepSeconds = 0.8f;

    private static Sprite ringSprite;
    private static Sprite discSprite;
    private static Sprite cardSprite;

    private Image fillImage;
    private TMP_Text valueText;
    private TMP_Text labelText;
    private TMP_Text captionText;

    private Coroutine sweep;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Prewarm()
    {
        RingSprite();
        DiscSprite();
        CardSprite();
    }

    public static ScoreDial Create(RectTransform parent, TMP_FontAsset font)
    {
        GameObject root = NewUIObject("ScoreDial", parent);
        ScoreDial dial = root.AddComponent<ScoreDial>();

        RectTransform rootRect = root.GetComponent<RectTransform>();

        Image card = root.AddComponent<Image>();
        card.sprite = CardSprite();
        card.type = Image.Type.Sliced;
        card.color = CardColor;
        card.raycastTarget = false;

        LayoutElement layout = root.AddComponent<LayoutElement>();
        layout.preferredHeight = CardHeight;
        layout.minHeight = CardHeight;
        layout.flexibleWidth = 1f;

        RectTransform ring = NewUIObject("Ring", rootRect).GetComponent<RectTransform>();
        ring.anchorMin = new Vector2(0f, 0.5f);
        ring.anchorMax = new Vector2(0f, 0.5f);
        ring.pivot = new Vector2(0f, 0.5f);
        ring.anchoredPosition = new Vector2(RingInset, 0f);
        ring.sizeDelta = new Vector2(RingSize, RingSize);

        AddStretchedImage(ring, "Face", DiscSprite(), FaceColor);
        AddStretchedImage(ring, "Track", RingSprite(), TrackColor);

        dial.fillImage = AddStretchedImage(ring, "Fill", RingSprite(), MidColor);
        dial.fillImage.type = Image.Type.Filled;
        dial.fillImage.fillMethod = Image.FillMethod.Radial360;
        dial.fillImage.fillOrigin = (int)Image.Origin360.Top;
        dial.fillImage.fillClockwise = true;
        dial.fillImage.fillAmount = 0f;

        dial.valueText = AddText(ring, "Value", font, 44f, InkColor);
        Stretch(dial.valueText.rectTransform);
        dial.valueText.lineSpacing = -26f;

        float textLeft = RingInset + RingSize + 22f;

        dial.labelText = AddText(rootRect, "Label", font, 31f, InkColor);
        AnchorBanner(dial.labelText.rectTransform, textLeft, 11f, 42f);
        dial.labelText.alignment = TextAlignmentOptions.BottomLeft;
        dial.labelText.characterSpacing = 3f;
        dial.labelText.enableAutoSizing = true;
        dial.labelText.fontSizeMin = 19f;
        dial.labelText.fontSizeMax = 31f;

        dial.captionText = AddText(rootRect, "Caption", font, 25f,
                                   new Color(InkColor.r, InkColor.g, InkColor.b, 0.62f));
        AnchorBanner(dial.captionText.rectTransform, textLeft, -36f, 36f);
        dial.captionText.alignment = TextAlignmentOptions.TopLeft;
        dial.captionText.enableAutoSizing = true;
        dial.captionText.fontSizeMin = 17f;
        dial.captionText.fontSizeMax = 25f;

        return dial;
    }

    public void SetScore(int score, string label, string caption, bool animate)
    {
        StopSweep();

        labelText.text = label.ToUpperInvariant();
        captionText.text = caption;

        float target = Mathf.Clamp01(score / 100f);
        fillImage.color = ColorForScore(score);

        if (!animate || !isActiveAndEnabled)
        {
            fillImage.fillAmount = target;
            SetValueText(score);
            return;
        }

        sweep = StartCoroutine(Sweep(score, target));
    }

    public void SetUnavailable(string label, string caption)
    {
        StopSweep();

        labelText.text = label.ToUpperInvariant();
        captionText.text = caption;
        fillImage.fillAmount = 0f;
        valueText.text = "<alpha=#55><size=38>--";
    }

    private void StopSweep()
    {
        if (sweep == null)
            return;

        StopCoroutine(sweep);
        sweep = null;
    }

    private IEnumerator Sweep(int score, float target)
    {
        while (SceneTransition.IsBusy)
            yield return null;

        float elapsed = 0f;

        while (elapsed < SweepSeconds)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / SweepSeconds);
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            fillImage.fillAmount = target * eased;
            SetValueText(Mathf.RoundToInt(score * eased));

            yield return null;
        }

        fillImage.fillAmount = target;
        SetValueText(score);
        sweep = null;
    }

    private void SetValueText(int score)
    {
        valueText.text = score + "\n<size=16><alpha=#88>/100";
    }

    private static Color ColorForScore(int score)
    {
        float t = Mathf.Clamp01(score / 100f);

        return t < 0.5f
            ? Color.Lerp(LowColor, MidColor, t / 0.5f)
            : Color.Lerp(MidColor, HighColor, (t - 0.5f) / 0.5f);
    }

    private static GameObject NewUIObject(string name, RectTransform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = parent.gameObject.layer;
        go.GetComponent<RectTransform>().SetParent(parent, false);
        return go;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void AnchorBanner(RectTransform rect, float left, float centreOffset, float height)
    {
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.offsetMin = new Vector2(left, centreOffset - height * 0.5f);
        rect.offsetMax = new Vector2(-18f, centreOffset + height * 0.5f);
    }

    private static Image AddStretchedImage(RectTransform parent, string name, Sprite sprite, Color color)
    {
        RectTransform rect = NewUIObject(name, parent).GetComponent<RectTransform>();
        Stretch(rect);

        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static TMP_Text AddText(RectTransform parent, string name, TMP_FontAsset font,
                                    float size, Color color)
    {
        TextMeshProUGUI text = NewUIObject(name, parent).AddComponent<TextMeshProUGUI>();

        if (font != null)
            text.font = font;

        text.fontSize = size;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        return text;
    }

    private static Sprite RingSprite()
    {
        if (ringSprite == null)
            ringSprite = BuildRadialSprite("ScoreDialRing", 0.5f - 0.105f);

        return ringSprite;
    }

    private static Sprite DiscSprite()
    {
        if (discSprite == null)
            discSprite = BuildRadialSprite("ScoreDialDisc", 0f);

        return discSprite;
    }

    private static Sprite CardSprite()
    {
        if (cardSprite == null)
            cardSprite = BuildRoundedRectSprite("ScoreDialCard", 64, 18f);

        return cardSprite;
    }

    private static Texture2D NewTexture(string name, int size)
    {
        return new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = name,
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.HideAndDontSave
        };
    }

    private static Sprite BuildRadialSprite(string name, float innerRadiusFraction)
    {
        const int size = 256;

        Texture2D texture = NewTexture(name, size);

        float centre = (size - 1) * 0.5f;
        float outer = size * 0.5f - 1.5f;
        float inner = size * innerRadiusFraction;

        Color32[] pixels = new Color32[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - centre;
                float dy = y - centre;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);

                float alpha = Mathf.Clamp01(outer - distance);

                if (inner > 0f)
                    alpha = Mathf.Min(alpha, Mathf.Clamp01(distance - inner));

                pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        return Finish(texture, name, Vector4.zero);
    }

    private static Sprite BuildRoundedRectSprite(string name, int size, float radius)
    {
        Texture2D texture = NewTexture(name, size);

        Color32[] pixels = new Color32[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(radius - x - 0.5f, (x + 0.5f) - (size - radius), 0f);
                float dy = Mathf.Max(radius - y - 0.5f, (y + 0.5f) - (size - radius), 0f);

                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01(radius - distance);

                pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        float border = radius + 2f;
        return Finish(texture, name, new Vector4(border, border, border, border));
    }

    private static Sprite Finish(Texture2D texture, string name, Vector4 border)
    {
        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                                      new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
        sprite.name = name;
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }
}
