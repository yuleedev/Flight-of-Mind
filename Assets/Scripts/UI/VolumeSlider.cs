using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class VolumeSlider : MonoBehaviour
{
    public enum Channel { Sfx, Music }

    [SerializeField] private Channel channel = Channel.Sfx;
    [Tooltip("Blip played while dragging an SFX slider so the level can be heard.")]
    [SerializeField] private AudioClip previewClip;
    [SerializeField] private float previewInterval = 0.15f;

    private Slider slider;
    private float lastPreview;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;

        slider.SetValueWithoutNotify(channel == Channel.Sfx ? VolumeSettings.Sfx : VolumeSettings.Music);
        slider.onValueChanged.AddListener(OnChanged);
    }

    private void OnDestroy()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnChanged);
    }

    private void OnChanged(float value)
    {
        if (channel == Channel.Music)
        {
            VolumeSettings.Music = value;
            return;
        }

        VolumeSettings.Sfx = value;

        if (previewClip == null || Time.unscaledTime - lastPreview < previewInterval)
            return;

        lastPreview = Time.unscaledTime;
        Sfx.Play(previewClip);
    }
}
