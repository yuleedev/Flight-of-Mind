using UnityEngine;

public static class VolumeSettings
{
    public const float Default = 0.5f;

    private const string SfxKey = "sfxVolume";
    private const string MusicKey = "musicVolume";

    private static float sfx = -1f;
    private static float music = -1f;

    public static float Sfx
    {
        get
        {
            if (sfx < 0f)
                sfx = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxKey, Default));

            return sfx;
        }
        set
        {
            sfx = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(SfxKey, sfx);
        }
    }

    public static float Music
    {
        get
        {
            if (music < 0f)
                music = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicKey, Default));

            return music;
        }
        set
        {
            music = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(MusicKey, music);
        }
    }
}
