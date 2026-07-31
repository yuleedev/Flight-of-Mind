using UnityEngine;

public class SceneMusic : MonoBehaviour
{
    public static SceneMusic Instance;

    [Tooltip("Plays as soon as the scene loads, which covers the instruction screen.")]
    [SerializeField] private AudioClip instructionsClip;
    [Tooltip("Takes over once the game itself starts. Leave empty to keep the instructions track.")]
    [SerializeField] private AudioClip gameClip;
    [SerializeField, Range(0f, 1f)] private float volume = 0.12f;
    [SerializeField] private float fadeInSeconds = 2f;
    [SerializeField] private float fadeOutSeconds = 0.8f;
    [Tooltip("Fade for the game track specifically. 0 makes it start at full volume.")]
    [SerializeField] private float gameFadeInSeconds = 0f;

    private void Awake()
    {
        Instance = this;
        MusicPlayer.Request(instructionsClip, volume, fadeInSeconds, fadeOutSeconds);
    }

    public static void StartGameMusic()
    {
        if (Instance != null)
            Instance.PlayGameMusic();
    }

    public void PlayGameMusic()
    {
        MusicPlayer.Request(gameClip, volume, gameFadeInSeconds, fadeOutSeconds);
    }
}
