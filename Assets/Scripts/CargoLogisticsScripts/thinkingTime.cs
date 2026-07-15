using UnityEngine;

public class thinkingTime : MonoBehaviour
{
    public static thinkingTime Instance;

    public float ThinkingTimeSeconds { get; private set; }
    public bool HasStoppedTiming { get; private set; }

    private float startTime;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ResetTimer();
    }

    public void ResetTimer()
    {
        startTime = Time.time;
        HasStoppedTiming = false;
        ThinkingTimeSeconds = 0f;
    }

    public void StopTiming()
    {
        if (HasStoppedTiming) return;

        HasStoppedTiming = true;
        ThinkingTimeSeconds = Time.time - startTime;
        Debug.Log($"thinking_time: {ThinkingTimeSeconds:F2}s");
    }
}