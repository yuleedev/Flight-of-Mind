using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class ScaleContentToFit : MonoBehaviour
{
    [SerializeField] private float padding = 0f;

    private RectTransform self;
    private RectTransform canvasRect;
    private bool applying;

    private void OnEnable()
    {
        Apply();
    }

    private void Start()
    {
        Apply();
    }

    private void OnRectTransformDimensionsChange()
    {
        Apply();
    }

    private bool Resolve()
    {
        if (self == null)
        {
            self = (RectTransform)transform;
        }

        if (canvasRect == null)
        {
            Canvas canvas = GetComponentInParent<Canvas>();

            if (canvas != null)
            {
                canvasRect = (RectTransform)canvas.rootCanvas.transform;
            }
        }

        return self != null && canvasRect != null;
    }

    private bool MeasureContent(out float width, out float height)
    {
        width = 0f;
        height = 0f;

        Vector3[] corners = new Vector3[4];
        bool any = false;

        for (int i = 0; i < self.childCount; i++)
        {
            RectTransform child = self.GetChild(i) as RectTransform;

            if (child == null || !child.gameObject.activeSelf)
            {
                continue;
            }

            child.GetLocalCorners(corners);

            for (int c = 0; c < 4; c++)
            {
                Vector3 local = self.InverseTransformPoint(child.TransformPoint(corners[c]));

                width = Mathf.Max(width, Mathf.Abs(local.x) * 2f);
                height = Mathf.Max(height, Mathf.Abs(local.y) * 2f);
                any = true;
            }
        }

        return any;
    }

    private void Apply()
    {
        if (applying || !Resolve())
        {
            return;
        }

        Vector2 canvasSize = canvasRect.rect.size;

        if (canvasSize.x <= 0f || canvasSize.y <= 0f)
        {
            return;
        }

        applying = true;

        self.localScale = Vector3.one;

        float contentWidth;
        float contentHeight;
        bool measured = MeasureContent(out contentWidth, out contentHeight);

        float scale = 1f;

        if (measured)
        {
            float available = Mathf.Max(1f, canvasSize.x - padding * 2f);
            float availableHeight = Mathf.Max(1f, canvasSize.y - padding * 2f);

            if (contentWidth > 0f)
            {
                scale = Mathf.Min(scale, available / contentWidth);
            }

            if (contentHeight > 0f)
            {
                scale = Mathf.Min(scale, availableHeight / contentHeight);
            }
        }

        scale = Mathf.Clamp(scale, 0.05f, 1f);

        self.localScale = new Vector3(scale, scale, 1f);

        bool stretched =
            Mathf.Approximately(self.anchorMin.x, 0f) &&
            Mathf.Approximately(self.anchorMin.y, 0f) &&
            Mathf.Approximately(self.anchorMax.x, 1f) &&
            Mathf.Approximately(self.anchorMax.y, 1f);

        if (stretched)
        {
            Vector2 target = canvasSize * (1f / scale - 1f);

            if ((self.sizeDelta - target).sqrMagnitude > 0.01f)
            {
                self.sizeDelta = target;
            }
        }

        applying = false;
    }
}
