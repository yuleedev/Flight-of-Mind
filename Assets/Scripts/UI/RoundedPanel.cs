using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class RoundedPanel : MonoBehaviour
{
    [SerializeField] private float cornerRadius = 26f;
    [SerializeField] private float borderThickness = 5f;
    [SerializeField] private Color borderColor = new Color(0.282f, 0.306f, 0.408f, 1f);
    [SerializeField] private Color fillColor = new Color(0.204f, 0.204f, 0.314f, 1f);
    [SerializeField] private Color accentColor = new Color(0.09f, 0.66f, 0.55f, 1f);
    [SerializeField] private float accentWidth = 0f;
    [SerializeField] private bool dropShadow = true;

    private static Sprite roundedSprite;

    private void Awake()
    {
        Image border = GetComponent<Image>();
        border.sprite = RoundedSprite();
        border.type = Image.Type.Sliced;
        border.color = borderColor;
        border.raycastTarget = false;

        RectTransform fill = NewChild("Fill");
        fill.offsetMin = new Vector2(borderThickness, borderThickness);
        fill.offsetMax = new Vector2(-borderThickness, -borderThickness);

        Image fillImage = fill.gameObject.AddComponent<Image>();
        fillImage.sprite = RoundedSprite();
        fillImage.type = Image.Type.Sliced;
        fillImage.color = fillColor;
        fillImage.raycastTarget = false;

        if (accentWidth > 0f)
        {
            RectTransform accent = NewChild("Accent");
            accent.anchorMin = new Vector2(0f, 0f);
            accent.anchorMax = new Vector2(0f, 1f);
            accent.pivot = new Vector2(0f, 0.5f);
            accent.offsetMin = new Vector2(borderThickness, borderThickness + cornerRadius * 0.5f);
            accent.offsetMax = new Vector2(borderThickness + accentWidth, -(borderThickness + cornerRadius * 0.5f));

            Image accentImage = accent.gameObject.AddComponent<Image>();
            accentImage.color = accentColor;
            accentImage.raycastTarget = false;
        }

        if (dropShadow)
        {
            Shadow shadow = gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.09f, 0.10f, 0.18f, 0.35f);
            shadow.effectDistance = new Vector2(0f, -7f);
        }
    }

    private RectTransform NewChild(string name)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = gameObject.layer;

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(transform, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return rect;
    }

    private Sprite RoundedSprite()
    {
        if (roundedSprite != null)
            return roundedSprite;

        int size = 64;
        float radius = Mathf.Clamp(cornerRadius, 2f, size * 0.5f - 1f);

        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "RoundedPanel",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.HideAndDontSave
        };

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
        roundedSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f),
                                      100f, 0, SpriteMeshType.FullRect,
                                      new Vector4(border, border, border, border));
        roundedSprite.name = "RoundedPanel";
        roundedSprite.hideFlags = HideFlags.HideAndDontSave;
        return roundedSprite;
    }
}
