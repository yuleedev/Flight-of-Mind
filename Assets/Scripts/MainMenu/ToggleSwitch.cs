using UnityEngine;
using UnityEngine.UI;

public class ToggleSwitch : MonoBehaviour
{
    public Toggle toggle;
    public RectTransform knob;
    public Image track;
    public float onX = 35f, offX = -35f, speed = 12f;
    public Color onColor = new Color(0.7f, 0.7f, 0.7f);
    public Color offColor = new Color(0.84f, 0.84f, 0.84f);

    float targetX;

    void Start()
    {
        toggle.onValueChanged.AddListener(OnChanged);
        Apply(toggle.isOn, instant: true);
    }

    void OnChanged(bool isOn) => Apply(isOn, instant: false);

    void Apply(bool isOn, bool instant)
    {
        targetX = isOn ? onX : offX;
        if (track) track.color = isOn ? onColor : offColor;
        if (instant) knob.anchoredPosition = new Vector2(targetX, knob.anchoredPosition.y);
    }

    void Update()
    {
        var p = knob.anchoredPosition;
        p.x = Mathf.Lerp(p.x, targetX, Time.unscaledDeltaTime * speed);
        knob.anchoredPosition = p;
    }
}