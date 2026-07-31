using System.Collections;
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

    public static void PlayClipped(AudioClip clip, float volume, float pitch,
                                   float duration, float fadeIn, float fadeOut)
    {
        if (clip == null || instance == null)
            return;

        instance.StartCoroutine(instance.RunClipped(clip, volume * VolumeSettings.Sfx, pitch,
                                                    duration, fadeIn, fadeOut));
    }

    private IEnumerator RunClipped(AudioClip clip, float volume, float pitch,
                                   float duration, float fadeIn, float fadeOut)
    {
        AudioSource voice = gameObject.AddComponent<AudioSource>();
        voice.clip = clip;
        voice.pitch = pitch;
        voice.volume = 0f;
        voice.playOnAwake = false;
        voice.Play();

        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;

            float up = fadeIn > 0f ? Mathf.Clamp01(t / fadeIn) : 1f;
            float down = fadeOut > 0f ? Mathf.Clamp01((duration - t) / fadeOut) : 1f;
            voice.volume = volume * Mathf.Min(up, down);

            yield return null;
        }

        voice.Stop();
        Destroy(voice);
    }

    public static void Play(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null || instance == null)
            return;

        instance.source.pitch = pitch;
        instance.source.PlayOneShot(clip, volume * VolumeSettings.Sfx);
    }
}
