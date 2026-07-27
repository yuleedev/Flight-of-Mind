using UnityEngine;

public class thinkingTime : MonoBehaviour
{
    public static thinkingTime Instance;

    public float InitialThinkingSeconds { get; private set; }
    public float SubsequentThinkingSeconds { get; private set; }
    public float AnimationSeconds { get; private set; }
    public float TotalSeconds { get; private set; }
    public bool HasStoppedTiming { get; private set; }

    public float ThinkingTimeSeconds => InitialThinkingSeconds;

    private float trialStart;
    private float animationStart;
    private int animationsRunning;
    public float LiveTotalSeconds => Time.unscaledTime - trialStart;

    public float LiveSubsequentThinkingSeconds
    {
        get
        {
            if (!HasStoppedTiming) return 0f;
            float animationSoFar = AnimationSeconds +
                                   (animationsRunning > 0 ? Time.unscaledTime - animationStart : 0f);
            return Mathf.Max(0f, LiveTotalSeconds - InitialThinkingSeconds - animationSoFar);
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        ResetTimer();
    }

    public void ResetTimer()
    {
        trialStart = Time.unscaledTime;
        InitialThinkingSeconds = 0f;
        SubsequentThinkingSeconds = 0f;
        AnimationSeconds = 0f;
        TotalSeconds = 0f;
        HasStoppedTiming = false;
        animationsRunning = 0;
    }

    public void StopTiming()
    {
        if (HasStoppedTiming) return;

        HasStoppedTiming = true;
        InitialThinkingSeconds = Time.unscaledTime - trialStart;
        Debug.Log($"initial_thinking_time: {InitialThinkingSeconds:F2}s");
    }

    public void OnAnimationStarted()
    {
        if (animationsRunning == 0) animationStart = Time.unscaledTime;
        animationsRunning++;
    }

    public void OnAnimationEnded()
    {
        if (animationsRunning == 0) return;

        animationsRunning--;
        if (animationsRunning == 0) AnimationSeconds += Time.unscaledTime - animationStart;
    }

    public void OnTrialSolved()
    {
        StopTiming();
        TotalSeconds = Time.unscaledTime - trialStart;
        SubsequentThinkingSeconds = Mathf.Max(0f, TotalSeconds - InitialThinkingSeconds - AnimationSeconds);
    }
}