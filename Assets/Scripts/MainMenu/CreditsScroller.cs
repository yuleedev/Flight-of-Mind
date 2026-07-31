using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class CreditsScroller : MonoBehaviour
{
    [Tooltip("Fallback cap. A LayoutElement on this object wins, so the scroll view keeps the height the panel already reserves.")]
    [SerializeField] private float maxHeight = 260f;
    [SerializeField] private float scrollbarWidth = 8f;
    [SerializeField] private float scrollSensitivity = 24f;

    private static readonly Color TrackColor = new Color(0.173f, 0.204f, 0.341f, 0.12f);
    private static readonly Color HandleColor = new Color(0.173f, 0.204f, 0.341f, 0.45f);

    private bool built;

    private void OnEnable()
    {
        if (!built)
            StartCoroutine(BuildIfOverflowing());
    }

    private IEnumerator BuildIfOverflowing()
    {
        yield return null;

        TMP_Text text = GetComponent<TMP_Text>();
        if (text == null)
            yield break;

        RectTransform body = (RectTransform)transform;

        float width = 0f;

        for (int frame = 0; frame < 30 && width <= 1f; frame++)
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(body);
            width = body.rect.width;

            if (width <= 1f)
                yield return null;
        }

        if (width <= 1f)
        {
            Debug.LogWarning("[CreditsScroller] credits body never got a width, scrolling not set up.");
            yield break;
        }

        float needed = text.GetPreferredValues(text.text, width, 0f).y;

        LayoutElement reserved = GetComponent<LayoutElement>();
        float cap = reserved != null && reserved.preferredHeight > 0f
            ? reserved.preferredHeight
            : maxHeight;

        Debug.Log($"[CreditsScroller] text needs {needed:F0}px, panel reserves {cap:F0}px at width {width:F0}.");

        if (needed <= cap + 1f)
            yield break;

        built = true;
        Build(body, needed, cap, reserved);
    }

    private void Build(RectTransform body, float contentHeight, float cap, LayoutElement reserved)
    {
        Transform parent = body.parent;
        int siblingIndex = body.GetSiblingIndex();

        if (reserved != null)
        {
            reserved.enabled = false;
            Destroy(reserved);
        }

        RectTransform viewport = NewChild("CreditsViewport", parent as RectTransform);
        viewport.SetSiblingIndex(siblingIndex);

        Image catcher = viewport.gameObject.AddComponent<Image>();
        catcher.color = new Color(0f, 0f, 0f, 0f);
        catcher.raycastTarget = true;

        viewport.gameObject.AddComponent<RectMask2D>();

        LayoutElement layout = viewport.gameObject.AddComponent<LayoutElement>();
        layout.preferredHeight = cap;
        layout.minHeight = cap;
        layout.flexibleWidth = 1f;

        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = scrollSensitivity;
        scroll.viewport = viewport;

        body.SetParent(viewport, false);
        body.anchorMin = new Vector2(0f, 1f);
        body.anchorMax = new Vector2(1f, 1f);
        body.pivot = new Vector2(0.5f, 1f);
        body.anchoredPosition = Vector2.zero;
        body.offsetMin = new Vector2(0f, body.offsetMin.y);
        body.offsetMax = new Vector2(-(scrollbarWidth + 6f), 0f);
        body.sizeDelta = new Vector2(body.sizeDelta.x, contentHeight);

        ContentSizeFitter fitter = body.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.content = body;
        scroll.verticalScrollbar = BuildScrollbar(viewport);
        scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;

        scroll.verticalNormalizedPosition = 1f;
    }

    private Scrollbar BuildScrollbar(RectTransform viewport)
    {
        RectTransform bar = NewChild("Scrollbar", viewport);
        bar.anchorMin = new Vector2(1f, 0f);
        bar.anchorMax = new Vector2(1f, 1f);
        bar.pivot = new Vector2(1f, 1f);
        bar.anchoredPosition = Vector2.zero;
        bar.sizeDelta = new Vector2(scrollbarWidth, 0f);

        Image track = bar.gameObject.AddComponent<Image>();
        track.color = TrackColor;
        track.raycastTarget = true;

        Scrollbar scrollbar = bar.gameObject.AddComponent<Scrollbar>();
        scrollbar.direction = Scrollbar.Direction.BottomToTop;

        RectTransform area = NewChild("Sliding Area", bar);
        Stretch(area);

        RectTransform handle = NewChild("Handle", area);
        Stretch(handle);

        Image handleImage = handle.gameObject.AddComponent<Image>();
        handleImage.color = HandleColor;

        scrollbar.handleRect = handle;
        scrollbar.targetGraphic = handleImage;
        return scrollbar;
    }

    private RectTransform NewChild(string name, RectTransform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = gameObject.layer;

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
