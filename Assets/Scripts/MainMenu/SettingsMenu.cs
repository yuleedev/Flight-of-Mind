using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the Settings pop-up: opening/closing it, the dyslexia-friendly
/// font toggle, and (placeholder) music / SFX sliders.
/// Settings are saved with PlayerPrefs so they persist between sessions.
/// </summary>
public class SettingsMenu : MonoBehaviour
{
    [Header("Pop-up")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Dyslexia Font")]
    [SerializeField] private Toggle dyslexiaToggle;
    [SerializeField] private TMP_FontAsset defaultFont;   // e.g. LiberationSans SDF
    [SerializeField] private TMP_FontAsset dyslexiaFont;  // e.g. AtkinsonHyperlegible SDF

    [Header("Audio (placeholders for now)")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private const string DyslexiaKey = "dyslexiaFont";
    private const string MusicKey = "musicVolume";
    private const string SfxKey = "sfxVolume";

    private void Start()
    {
        // Load saved values (default: off / full volume).
        bool dyslexiaOn = PlayerPrefs.GetInt(DyslexiaKey, 0) == 1;

        if (dyslexiaToggle != null)
            dyslexiaToggle.SetIsOnWithoutNotify(dyslexiaOn);

        if (musicSlider != null)
            musicSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(MusicKey, 1f));

        if (sfxSlider != null)
            sfxSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(SfxKey, 1f));

        ApplyFont(dyslexiaOn);

        // Make sure the panel starts hidden.
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    // Hook this to the Settings button's OnClick.
    public void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    // Hook this to the OK button's OnClick.
    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        PlayerPrefs.Save();
    }

    // Hook this to the Toggle's OnValueChanged (passes the bool automatically).
    public void OnDyslexiaToggle(bool isOn)
    {
        PlayerPrefs.SetInt(DyslexiaKey, isOn ? 1 : 0);
        ApplyFont(isOn);
    }

    // Hook these to the sliders' OnValueChanged when you add audio later.
    public void OnMusicChanged(float value)
    {
        PlayerPrefs.SetFloat(MusicKey, value);
        // TODO: route to your AudioMixer / AudioSource volume.
    }

    public void OnSfxChanged(float value)
    {
        PlayerPrefs.SetFloat(SfxKey, value);
        // TODO: route to your AudioMixer / AudioSource volume.
    }

    /// <summary>
    /// Swaps the font on every TMP text in the current scene, including
    /// inactive ones so the pop-up's own labels change too.
    /// </summary>
    private void ApplyFont(bool dyslexiaOn)
    {
        TMP_FontAsset target = dyslexiaOn ? dyslexiaFont : defaultFont;
        if (target == null)
            return;

        TMP_Text[] texts = Resources.FindObjectsOfTypeAll<TMP_Text>();
        foreach (TMP_Text t in texts)
        {
            // Skip prefab assets that aren't part of a loaded scene.
            if (t.gameObject.scene.IsValid())
                t.font = target;
        }
    }
}
