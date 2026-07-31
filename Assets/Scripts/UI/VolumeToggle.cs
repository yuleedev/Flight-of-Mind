using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class VolumeToggle : MonoBehaviour
{
    public enum Channel { Sfx, Music }

    [SerializeField] private Channel channel = Channel.Sfx;
    [SerializeField] private AudioClip previewClip;

    private Toggle toggle;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
        toggle.SetIsOnWithoutNotify(Level > 0.01f);
        toggle.onValueChanged.AddListener(OnChanged);
    }

    private void OnDestroy()
    {
        if (toggle != null)
            toggle.onValueChanged.RemoveListener(OnChanged);
    }

    private float Level => channel == Channel.Sfx ? VolumeSettings.Sfx : VolumeSettings.Music;

    private void OnChanged(bool on)
    {
        if (channel == Channel.Music)
        {
            VolumeSettings.Music = on ? 1f : 0f;
            return;
        }

        VolumeSettings.Sfx = on ? 1f : 0f;

        if (on)
            Sfx.Play(previewClip);
    }
}
