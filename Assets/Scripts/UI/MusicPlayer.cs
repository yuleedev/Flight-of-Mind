using System.Collections;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    private static MusicPlayer instance;

    private AudioSource source;
    private AudioClip currentClip;
    private float targetVolume;
    private Coroutine fade;

    public static void Request(AudioClip clip, float volume, float fadeIn, float fadeOut)
    {
        if (clip == null)
            return;

        Ensure();
        instance.targetVolume = volume;

        if (instance.currentClip == clip)
        {
            if (!instance.source.isPlaying)
                instance.source.Play();

            return;
        }

        instance.currentClip = clip;

        if (instance.fade != null)
            instance.StopCoroutine(instance.fade);

        instance.fade = instance.StartCoroutine(instance.Switch(clip, fadeIn, fadeOut));
    }

    public static void FadeOut(float seconds)
    {
        if (instance == null || instance.source == null)
            return;

        instance.currentClip = null;

        if (instance.fade != null)
            instance.StopCoroutine(instance.fade);

        instance.fade = instance.StartCoroutine(instance.FadeOutAndStop(seconds));
    }

    private static void Ensure()
    {
        if (instance != null)
            return;

        GameObject host = new GameObject("[Music]");
        DontDestroyOnLoad(host);

        instance = host.AddComponent<MusicPlayer>();
        instance.source = host.AddComponent<AudioSource>();
        instance.source.loop = true;
        instance.source.playOnAwake = false;
        instance.source.volume = 0f;
    }

    private void Update()
    {
        if (fade == null && source.isPlaying)
            source.volume = targetVolume * VolumeSettings.Music;
    }

    private IEnumerator Switch(AudioClip clip, float fadeIn, float fadeOut)
    {
        if (source.isPlaying)
            yield return Ramp(0f, fadeOut);

        source.clip = clip;
        source.volume = 0f;
        source.Play();

        yield return Ramp(targetVolume * VolumeSettings.Music, fadeIn);
        fade = null;
    }

    private IEnumerator FadeOutAndStop(float seconds)
    {
        yield return Ramp(0f, seconds);
        source.Stop();
        fade = null;
    }

    private IEnumerator Ramp(float target, float seconds)
    {
        float from = source.volume;
        float elapsed = 0f;

        while (elapsed < seconds)
        {
            elapsed += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(from, target, Mathf.Clamp01(elapsed / seconds));
            yield return null;
        }

        source.volume = target;
    }
}
