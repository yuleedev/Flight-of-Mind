using UnityEngine;

public static class VolumeSettings
{
    public const float Default = 1f;

    private const string SfxKey = "sfxVolume";
    private const string MusicKey = "musicVolume";
    private const string VersionKey = "volumePrefsVersion";
    private const int Version = 2;

    private static float sfx = -1f;
    private static float music = -1f;
    private static bool migrated;

    private static void Migrate()
    {
        if (migrated)
            return;

        migrated = true;

        if (PlayerPrefs.GetInt(VersionKey, 0) >= Version)
            return;

        PlayerPrefs.DeleteKey(SfxKey);
        PlayerPrefs.DeleteKey(MusicKey);
        PlayerPrefs.SetInt(VersionKey, Version);
    }

    public static float Sfx
    {
        get
        {
            if (sfx < 0f)
            {
                Migrate();
                sfx = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxKey, Default));
            }

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
            {
                Migrate();
                music = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicKey, Default));
            }

            return music;
        }
        set
        {
            music = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(MusicKey, music);
        }
    }
}
