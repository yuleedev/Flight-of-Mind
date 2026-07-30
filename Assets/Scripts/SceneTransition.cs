using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    public static float CoverSeconds = 0.26f;
    public static float HoldSeconds = 0.16f;
    public static float RevealSeconds = 0.32f;

    private static readonly Color CoverColor = new Color(0.129f, 0.145f, 0.259f, 1f);

    private const float MaxFrameStep = 1f / 20f;
    private const float SettleFrameTime = 1f / 30f;
    private const float MaxSettleSeconds = 0.5f;

    private static SceneTransition instance;
    private static Sprite blockSprite;

    private Image cover;
    private RectTransform coverRect;

    private bool busy;

    public static bool IsBusy => instance != null && instance.busy;

    public static void LoadScene(string sceneName)
    {
        if (instance == null)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
            return;
        }

        instance.Begin(sceneName, -1);
    }

    public static void LoadScene(int buildIndex)
    {
        if (instance == null)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(buildIndex);
            return;
        }

        instance.Begin(null, buildIndex);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        if (instance != null)
            return;

        GameObject host = new GameObject("[SceneTransition]");
        DontDestroyOnLoad(host);

        instance = host.AddComponent<SceneTransition>();
        instance.Build();
    }

    private void Build()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        GameObject go = new GameObject("Cover", typeof(RectTransform));
        go.layer = gameObject.layer;

        coverRect = go.GetComponent<RectTransform>();
        coverRect.SetParent(canvas.GetComponent<RectTransform>(), false);
        coverRect.anchorMin = Vector2.zero;
        coverRect.anchorMax = Vector2.one;
        coverRect.offsetMin = Vector2.zero;
        coverRect.offsetMax = Vector2.zero;

        cover = go.AddComponent<Image>();
        cover.sprite = BlockSprite();
        cover.color = CoverColor;
        cover.raycastTarget = false;
        cover.type = Image.Type.Filled;
        cover.fillMethod = Image.FillMethod.Vertical;
        cover.fillOrigin = (int)Image.OriginVertical.Top;
        cover.fillAmount = 0f;

        coverRect.gameObject.SetActive(false);
    }

    private void Begin(string sceneName, int buildIndex)
    {
        if (busy)
            return;

        busy = true;
        Time.timeScale = 1f;
        StartCoroutine(Run(sceneName, buildIndex));
    }

    private IEnumerator Run(string sceneName, int buildIndex)
    {
        coverRect.gameObject.SetActive(true);

        cover.fillOrigin = (int)Image.OriginVertical.Top;
        yield return Wipe(0f, 1f, CoverSeconds);

        cover.fillAmount = 1f;

        AsyncOperation load = sceneName != null
            ? SceneManager.LoadSceneAsync(sceneName)
            : SceneManager.LoadSceneAsync(buildIndex);

        load.allowSceneActivation = false;

        float held = 0f;

        while (held < HoldSeconds || load.progress < 0.9f)
        {
            held += Time.unscaledDeltaTime;
            yield return null;
        }

        load.allowSceneActivation = true;

        while (!load.isDone)
            yield return null;

        yield return Settle();

        cover.fillOrigin = (int)Image.OriginVertical.Bottom;
        yield return Wipe(1f, 0f, RevealSeconds);

        coverRect.gameObject.SetActive(false);
        busy = false;
    }

    private IEnumerator Settle()
    {
        yield return null;
        yield return null;

        float waited = 0f;

        while (Time.unscaledDeltaTime > SettleFrameTime && waited < MaxSettleSeconds)
        {
            waited += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private IEnumerator Wipe(float from, float to, float seconds)
    {
        float elapsed = 0f;

        while (elapsed < seconds)
        {
            elapsed += Mathf.Min(Time.unscaledDeltaTime, MaxFrameStep);

            float t = Mathf.Clamp01(elapsed / seconds);
            float eased = t * t * (3f - 2f * t);

            cover.fillAmount = Mathf.Lerp(from, to, eased);

            yield return null;
        }

        cover.fillAmount = to;
    }

    private static Sprite BlockSprite()
    {
        if (blockSprite != null)
            return blockSprite;

        const int size = 8;

        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "SceneTransitionBlock",
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };

        Color32[] pixels = new Color32[size * size];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = new Color32(255, 255, 255, 255);

        texture.SetPixels32(pixels);
        texture.Apply();

        blockSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f));
        blockSprite.name = "SceneTransitionBlock";
        blockSprite.hideFlags = HideFlags.HideAndDontSave;
        return blockSprite;
    }
}
