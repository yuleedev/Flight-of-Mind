using System.Collections.Generic;

[System.Serializable]
public class TrailMakingResult
{
    public string part;
    public float timeSeconds;
    public int errors;
}

public static class TrailMakingResults
{
    public static readonly List<TrailMakingResult> Trials = new List<TrailMakingResult>();

    public static void Clear() => Trials.Clear();

    public static void Record(string part, float timeSeconds, int errors)
    {
        Trials.Add(new TrailMakingResult
        {
            part = part,
            timeSeconds = timeSeconds,
            errors = errors
        });
    }
}
