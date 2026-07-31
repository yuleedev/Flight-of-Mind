using UnityEngine;

public class Sfx : MonoBehaviour
{
    private static Sfx instance;
    private AudioSource source;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        if (instance != null)
            return;

        GameObject host = new GameObject("[Sfx]");
        DontDestroyOnLoad(host);

        instance = host.AddComponent<Sfx>();
        instance.source = host.AddComponent<AudioSource>();
        instance.source.playOnAwake = false;
    }

    public static void Play(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null || instance == null)
            return;

        instance.source.pitch = pitch;
        instance.source.PlayOneShot(clip, volume * VolumeSettings.Sfx);
    }
}
