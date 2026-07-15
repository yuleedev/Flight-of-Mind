using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class SettingsPanelAnimator : MonoBehaviour
{
    public RectTransform window;      // the inner box to scale
    public float duration = 0.2f;
    public float startScale = 0.85f;  // how small it starts before popping in

    CanvasGroup cg;
    Coroutine anim;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        cg.alpha = 0f;                 // start hidden
        cg.interactable = false;
        cg.blocksRaycasts = false;
        if (window) window.localScale = Vector3.one * startScale;
    }

    public void Show() => Run(true);
    public void Hide() => Run(false);

    void Run(bool show)
    {
        if (anim != null) StopCoroutine(anim);
        anim = StartCoroutine(Animate(show));
    }

    IEnumerator Animate(bool show)
    {
        cg.interactable = show;
        cg.blocksRaycasts = show;
        float a0 = cg.alpha, a1 = show ? 1f : 0f;
        float s0 = window ? window.localScale.x : 1f, s1 = show ? 1f : startScale;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / duration);
            cg.alpha = Mathf.Lerp(a0, a1, k);
            if (window) window.localScale = Vector3.one * Mathf.Lerp(s0, s1, k);
            yield return null;
        }
        cg.alpha = a1;
        if (window) window.localScale = Vector3.one * s1;
    }
}