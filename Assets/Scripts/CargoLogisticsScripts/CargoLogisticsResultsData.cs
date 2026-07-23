using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TrialResult
{
    public int problemIndex;
    public bool isPractice;
    public int movesTaken;
    public int optimalMoves;
    public int excessMoves;
    public float thinkingTimeSeconds;
    public float expectedThinkingTimeSeconds;
    public float efficiencyScore;
    public float timeScore;
}

public static class CargoLogisticsResults
{
    public static readonly List<TrialResult> Trials = new List<TrialResult>();

    public static void Clear()
    {
        Trials.Clear();
        CargoLogisticsScoring.ResetFinalScores();
    }

    public static void Record(int problemIndex, bool isPractice, int movesTaken, int optimalMoves, float thinkingTimeSeconds)
    {
        float expectedTime = CargoLogisticsScoring.ExpectedThinkingTime(optimalMoves);
        float efficiency = CargoLogisticsScoring.EfficiencyScore(optimalMoves, movesTaken);
        float time = CargoLogisticsScoring.TimeScore(thinkingTimeSeconds, optimalMoves);

        Trials.Add(new TrialResult
        {
            problemIndex = problemIndex,
            isPractice = isPractice,
            movesTaken = movesTaken,
            optimalMoves = optimalMoves,
            excessMoves = movesTaken - optimalMoves,
            thinkingTimeSeconds = thinkingTimeSeconds,
            expectedThinkingTimeSeconds = expectedTime,
            efficiencyScore = efficiency,
            timeScore = time
        });
    }
}

public static class CargoLogisticsScoring
{

    public static float BaseThinkingSeconds = 3f;
    public static float SecondsPerOptimalMove = 4f;

    public static void Configure(float baseThinkingSeconds, float secondsPerOptimalMove)
    {
        BaseThinkingSeconds = baseThinkingSeconds;
        SecondsPerOptimalMove = secondsPerOptimalMove;
    }

    public static int thinkingTimeScore;
    public static int logicalReasoningScore;

    public static void ResetFinalScores()
    {
        thinkingTimeScore = 0;
        logicalReasoningScore = 0;
    }

    public static float ExpectedThinkingTime(int optimalMoves)
    {
        return BaseThinkingSeconds + SecondsPerOptimalMove * optimalMoves;
    }

    public static float EfficiencyScore(int optimalMoves, int movesTaken)
    {
        if (movesTaken <= 0 || optimalMoves <= 0) return 0f;
        float ratio = (float)optimalMoves / movesTaken;
        return Mathf.Clamp01(ratio) * 100f;
    }

    public static float TimeScore(float actualSeconds, int optimalMoves)
    {
        float expected = ExpectedThinkingTime(optimalMoves);
        float safeActual = Mathf.Max(actualSeconds, 0.01f);
        float ratio = expected / safeActual;
        return Mathf.Clamp01(ratio) * 100f;
    }

    public static int ScoredTrialCount()
    {
        int n = 0;
        foreach (var t in CargoLogisticsResults.Trials)
            if (!t.isPractice) n++;
        return n;
    }

    public static void ComputeFinalScores()
    {
        int n = 0;
        float efficiencySum = 0f;
        float timeSum = 0f;

        foreach (var t in CargoLogisticsResults.Trials)
        {
            if (t.isPractice) continue;
            efficiencySum += t.efficiencyScore;
            timeSum += t.timeScore;
            n++;
        }

        logicalReasoningScore = n > 0 ? Mathf.RoundToInt(efficiencySum / n) : 0;
        thinkingTimeScore = n > 0 ? Mathf.RoundToInt(timeSum / n) : 0;

        LogFinalResults();
    }

    private static void LogFinalResults()
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine("===== CARGO LOGISTICS - PER-TRIAL RAW DATA =====");
        foreach (var t in CargoLogisticsResults.Trials)
        {
            string tag = t.isPractice ? "practice" : "scored";
            sb.AppendLine(
                $"[{tag}] problem {t.problemIndex}: moves {t.movesTaken}/{t.optimalMoves} (excess {t.excessMoves}) " +
                $"-> efficiency {t.efficiencyScore:F1}/100 | " +
                $"thinking {t.thinkingTimeSeconds:F2}s vs expected {t.expectedThinkingTimeSeconds:F2}s " +
                $"-> time {t.timeScore:F1}/100");
        }

        sb.AppendLine("===== CARGO LOGISTICS - FINAL SCORES (practice trial excluded) =====");
        sb.AppendLine($"Scored trials: {ScoredTrialCount()}");
        sb.AppendLine($"logicalReasoningScore = {logicalReasoningScore} / 100");
        sb.AppendLine($"thinkingTimeScore     = {thinkingTimeScore} / 100");

        Debug.Log(sb.ToString());
    }
}