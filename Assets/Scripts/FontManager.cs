using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FontManager : MonoBehaviour
{
    public static FontManager Instance;
    public TMP_FontAsset dyslexiaFont;
    public float dyslexiaScale = 0.8f;

    const string Key = "DyslexiaFont";
    public bool IsOn { get; private set; }

    readonly Dictionary<TMP_Text, TMP_FontAsset> originals = new();
    readonly Dictionary<TMP_Text, float> originalSizes = new();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        IsOn = PlayerPrefs.GetInt(Key, 0) == 1;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start() => Apply(IsOn);

    void OnSceneLoaded(Scene s, LoadSceneMode m) => Apply(IsOn);

    public void SetDyslexia(bool on)
    {
        IsOn = on;
        PlayerPrefs.SetInt(Key, on ? 1 : 0);
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