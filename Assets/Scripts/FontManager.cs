using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FontManager : MonoBehaviour
{
    public static FontManager Instance;
    public TMP_FontAsset dyslexiaFont;
    public float dyslexiaScale = 0.8f;

    public bool IsOn { get; private set; }

    readonly Dictionary<TMP_Text, TMP_FontAsset> originals = new();
    readonly Dictionary<TMP_Text, float> originalSizes = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        Instance = null;
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        IsOn = false;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance == this) Instance = null;
    }

    void Start() => Apply(IsOn);

    void OnSceneLoaded(Scene s, LoadSceneMode m) => Apply(IsOn);

    public void SetDyslexia(bool on)
    {
        IsOn = on;
        Apply(on);
    }

    void Apply(bool on)
    {
        var texts = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include);

        foreach (var t in texts)
        {
            if (t == null) continue;
            if (!originals.ContainsKey(t)) originals[t] = t.font;
            if (!originalSizes.ContainsKey(t)) originalSizes[t] = t.fontSize;

            t.font = on ? dyslexiaFont : originals[t];
            t.fontSize = on ? originalSizes[t] * dyslexiaScale : originalSizes[t];
        }
    }
}