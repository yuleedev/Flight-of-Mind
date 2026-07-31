using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
                                               IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float restScale = 0.97f;
    [SerializeField] private float hoverScale = 1.03f;
    [SerializeField] private float pressScale = 0.99f;
    [SerializeField] private float popSeconds = 0.18f;
    [SerializeField] private float settleSeconds = 0.12f;

    private Vector3 baseScale = Vector3.one;
    private Coroutine animation;
    private bool hovering;

    private void Awake()
    {
        baseScale = transform.localScale;
        transform.localScale = baseScale * restScale;
    }

    private void OnDisable()
    {
        animation = null;
        hovering = false;
        transform.localScale = baseScale * restScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
        ScaleTo(hoverScale, popSeconds, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
        ScaleTo(restScale, settleSeconds, false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ScaleTo(pressScale, 0.06f, false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ScaleTo(hovering ? hoverScale : restScale, popSeconds, hovering);
    }

    private void ScaleTo(float target, float seconds, bool overshoot)
    {
        if (!isActiveAndEnabled)
            return;

        if (animation != null)
            StopCoroutine(animation);

        animation = StartCoroutine(Animate(target, seconds, overshoot));
    }

    private IEnumerator Animate(float target, float seconds, bool overshoot)
    {
        Vector3 from = transform.localScale;
        Vector3 to = baseScale * target;
        float elapsed = 0f;

        while (elapsed < seconds)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / seconds);
            float eased = overshoot ? EaseOutBack(t) : t * t * (3f - 2f * t);

            transform.localScale = Vector3.LerpUnclamped(from, to, eased);
            yield return null;
        }

        transform.localScale = to;
        animation = null;
    }

    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;

        float p = t - 1f;
        return 1f + c3 * p * p * p + c1 * p * p;
    }
}
