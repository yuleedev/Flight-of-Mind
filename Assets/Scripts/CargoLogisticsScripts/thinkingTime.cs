using UnityEngine;

public class thinkingTime : MonoBehaviour
{
    public static thinkingTime Instance;

    public float ThinkingTimeSeconds { get; private set; }// amount of time in seconds the person spent thinking.
    public bool HasStoppedTiming { get; private set; }

    private float startTime;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        startTime = Time.time;
    }

    public void StopTiming()
    {
        if (HasStoppedTiming) return;

        HasStoppedTiming = true;
        ThinkingTimeSeconds = Time.time - startTime;
        Debug.Log($"thinking_time: {ThinkingTimeSeconds:F2}s");
    }
}