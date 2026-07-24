using System.Collections.Generic;
using UnityEngine;

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

    public static TrailMakingResult Get(string part)
    {
        foreach (var t in Trials)
            if (t.part == part) return t;
        return null;
    }

    public static void LogResults()
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine("===== LANDING ROUTES (TRAIL MAKING) - PER-PART RAW DATA =====");
        foreach (var t in Trials)
        {
            sb.AppendLine($"Part {t.part}: {t.timeSeconds:F1}s, {t.errors} errors");
        }

        var a = Get("A");
        var b = Get("B");
        if (a != null && b != null)
        {
            float difference = b.timeSeconds - a.timeSeconds;
            float ratio = a.timeSeconds > 0f ? b.timeSeconds / a.timeSeconds : 0f;

            sb.AppendLine("===== LANDING ROUTES - DERIVED SCORES =====");
            sb.AppendLine($"B - A difference = {difference:F1}s");
            sb.AppendLine($"B / A ratio      = {ratio:F2}");
        }

        Debug.Log(sb.ToString());
    }
}
