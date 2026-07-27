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
    public float errorIndex;
    public float initialThinkingSeconds;
    public float subsequentThinkingSeconds;
    public float animationSeconds;
    public float totalSeconds;
    public float deliberationCostSeconds;
    public float expectedDeliberationSeconds;
    public float planningRatio;
    public float accuracyScore;
    public float deliberationScore;
    public float difficultyWeight;
    public int ageGroupIndex;
    public string ageGroupLabel;
    public float referenceAccuracyScore;
    public float referenceDeliberationScore;
}

public static class CargoLogisticsResults
{
    public static readonly List<TrialResult> Trials = new List<TrialResult>();

    public static void Clear()
    {
        Trials.Clear();
        CargoLogisticsScoring.ResetFinalScores();
    }

    public static void Record(int problemIndex, bool isPractice, int movesTaken, int optimalMoves,
                              float initialThinking, float subsequentThinking,
                              float animationSeconds, float totalSeconds)
    {
        Trials.Add(new TrialResult
        {
            problemIndex = problemIndex,
            isPractice = isPractice,
            movesTaken = movesTaken,
            optimalMoves = optimalMoves,
            excessMoves = Mathf.Max(0, movesTaken - optimalMoves),
            errorIndex = CargoLogisticsScoring.ErrorIndex(optimalMoves, movesTaken),
            initialThinkingSeconds = initialThinking,
            subsequentThinkingSeconds = subsequentThinking,
            animationSeconds = animationSeconds,
            totalSeconds = totalSeconds,
            deliberationCostSeconds = CargoLogisticsScoring.DeliberationCost(initialThinking, subsequentThinking),
            expectedDeliberationSeconds = CargoLogisticsScoring.ExpectedDeliberation(optimalMoves),
            planningRatio = CargoLogisticsScoring.PlanningRatio(initialThinking, subsequentThinking),
            accuracyScore = CargoLogisticsScoring.AccuracyScore(optimalMoves, movesTaken),
            deliberationScore = CargoLogisticsScoring.DeliberationScore(initialThinking, subsequentThinking,
                                                                        optimalMoves, movesTaken),
            difficultyWeight = CargoLogisticsScoring.DifficultyWeight(optimalMoves),
            ageGroupIndex = CargoLogisticsNorms.CurrentGroupIndex,
            ageGroupLabel = CargoLogisticsNorms.CurrentLabel,
            referenceAccuracyScore = CargoLogisticsScoring.ReferenceAccuracyScore(optimalMoves, movesTaken),
            referenceDeliberationScore = CargoLogisticsScoring.ReferenceDeliberationScore(
                initialThinking, subsequentThinking, optimalMoves, movesTaken)
        });

        CargoLogisticsScoring.LogTrial(Trials[Trials.Count - 1]);
    }
}

public static class CargoLogisticsScoring
{
    public const float InitialThinkingWeight = 1.00f;
    public const float SubsequentThinkingWeight = 2.33f;
    public const float PaceSensitivity = 1.1f;
    private const float MinCostSeconds = 0.25f;

    public static int thinkingTimeScore;
    public static int logicalReasoningScore;

    public static void ResetFinalScores()
    {
        thinkingTimeScore = 0;
        logicalReasoningScore = 0;
    }

    public static float DifficultyWeight(int optimalMoves)
    {
        return Mathf.Max(1, optimalMoves);
    }

    public static float ErrorIndex(int optimalMoves, int movesTaken)
    {
        if (optimalMoves <= 0) return 0f;
        return Mathf.Max(0, movesTaken - optimalMoves) / (float)optimalMoves;
    }

    public static float ExpectedDeliberation(int optimalMoves)
    {
        return ExpectedDeliberation(optimalMoves, CargoLogisticsNorms.Current);
    }

    public static float ReferenceExpectedDeliberation(int optimalMoves)
    {
        return ExpectedDeliberation(optimalMoves, CargoLogisticsNorms.Reference);
    }

    private static float ExpectedDeliberation(int optimalMoves, AgeNorm norm)
    {
        return norm.baseSeconds + norm.secondsPerMove * Mathf.Max(1, optimalMoves);
    }

    public static float AccuracyScore(int optimalMoves, int movesTaken)
    {
        return AccuracyScore(optimalMoves, movesTaken, CargoLogisticsNorms.Current);
    }

    public static float ReferenceAccuracyScore(int optimalMoves, int movesTaken)
    {
        return AccuracyScore(optimalMoves, movesTaken, CargoLogisticsNorms.Reference);
    }

    private static float AccuracyScore(int optimalMoves, int movesTaken, AgeNorm norm)
    {
        if (optimalMoves <= 0 || movesTaken <= 0) return 0f;
        return 100f * Mathf.Exp(-norm.errorDecay * ErrorIndex(optimalMoves, movesTaken));
    }

    public static float DeliberationCost(float initialThinking, float subsequentThinking)
    {
        return InitialThinkingWeight * Mathf.Max(0f, initialThinking)
             + SubsequentThinkingWeight * Mathf.Max(0f, subsequentThinking);
    }

    public static float PlanningRatio(float initialThinking, float subsequentThinking)
    {
        float sum = Mathf.Max(0f, initialThinking) + Mathf.Max(0f, subsequentThinking);
        return sum <= 0f ? 0f : Mathf.Clamp01(initialThinking / sum);
    }

    public static float DeliberationScore(float initialThinking, float subsequentThinking,
                                          int optimalMoves, int movesTaken)
    {
        return DeliberationScore(initialThinking, subsequentThinking, optimalMoves, movesTaken,
                                 CargoLogisticsNorms.Current);
    }

    public static float ReferenceDeliberationScore(float initialThinking, float subsequentThinking,
                                                   int optimalMoves, int movesTaken)
    {
        return DeliberationScore(initialThinking, subsequentThinking, optimalMoves, movesTaken,
                                 CargoLogisticsNorms.Reference);
    }

    private static float DeliberationScore(float initialThinking, float subsequentThinking,
                                           int optimalMoves, int movesTaken, AgeNorm norm)
    {
        float cost = Mathf.Max(DeliberationCost(initialThinking, subsequentThinking), MinCostSeconds);
        float accuracyFraction = AccuracyScore(optimalMoves, movesTaken, norm) / 100f;
        float rate = accuracyFraction * ExpectedDeliberation(optimalMoves, norm) / cost;
        return 100f * (float)System.Math.Tanh(PaceSensitivity * rate);
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
        ComputeWeightedScores(out logicalReasoningScore, out thinkingTimeScore);
        LogFinalResults();
    }

    private static void ComputeWeightedScores(out int reasoning, out int thinking)
    {
        float weightSum = 0f;
        float accuracySum = 0f;
        float deliberationSum = 0f;

        foreach (var t in CargoLogisticsResults.Trials)
        {
            if (t.isPractice) continue;
            weightSum += t.difficultyWeight;
            accuracySum += t.accuracyScore * t.difficultyWeight;
            deliberationSum += t.deliberationScore * t.difficultyWeight;
        }

        reasoning = weightSum > 0f ? Mathf.RoundToInt(accuracySum / weightSum) : 0;
        thinking = weightSum > 0f ? Mathf.RoundToInt(deliberationSum / weightSum) : 0;
    }

    public static void LogMove(int problemIndex, bool isPractice, int movesSoFar, int optimalMoves,
                               float initialThinking, float subsequentThinking)
    {
        float thinking = DeliberationScore(initialThinking, subsequentThinking, optimalMoves, movesSoFar);
        float accuracy = AccuracyScore(optimalMoves, movesSoFar);
        float cost = DeliberationCost(initialThinking, subsequentThinking);

        Debug.Log(
            $"[{(isPractice ? "practice" : "scored")}] problem {problemIndex} move {movesSoFar}/{optimalMoves} -> " +
            $"thinkingTimeScore {thinking:F1}/100 | accuracy {accuracy:F1}/100 | " +
            $"initial {initialThinking:F2}s, subsequent {subsequentThinking:F2}s, " +
            $"cost {cost:F2}s vs expected {ExpectedDeliberation(optimalMoves):F2}s | " +
            $"age band {CargoLogisticsNorms.CurrentLabel}");
    }

    public static void LogTrial(TrialResult t)
    {
        var sb = new System.Text.StringBuilder();
        string tag = t.isPractice ? "practice" : "scored";

        sb.AppendLine($"===== CARGO LOGISTICS - TRIAL {t.problemIndex} ({tag}) =====");
        sb.Append(FormatTrial(t));

        if (!t.isPractice)
        {
            ComputeWeightedScores(out int reasoning, out int thinking);
            sb.AppendLine($"    running after {ScoredTrialCount()} scored trial(s): " +
                          $"logicalReasoningScore = {reasoning} / 100 | thinkingTimeScore = {thinking} / 100");
        }

        Debug.Log(sb.ToString());
    }

    private static string FormatTrial(TrialResult t)
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine(
            $"    moves {t.movesTaken}/{t.optimalMoves} (excess {t.excessMoves}, error index {t.errorIndex:F2}) " +
            $"-> accuracy {t.accuracyScore:F1}/100");
        sb.AppendLine(
            $"    initial {t.initialThinkingSeconds:F2}s | subsequent {t.subsequentThinkingSeconds:F2}s | " +
            $"animation {t.animationSeconds:F2}s | total {t.totalSeconds:F2}s");
        sb.AppendLine(
            $"    planning ratio {t.planningRatio:F2} | weighted cost {t.deliberationCostSeconds:F2}s " +
            $"vs expected {t.expectedDeliberationSeconds:F2}s -> deliberation {t.deliberationScore:F1}/100 " +
            $"(weight {t.difficultyWeight:F0})");
        sb.AppendLine(
            $"    age band {t.ageGroupLabel} | reference-band accuracy {t.referenceAccuracyScore:F1} | " +
            $"reference-band deliberation {t.referenceDeliberationScore:F1}");

        return sb.ToString();
    }

    private static void LogFinalResults()
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine("===== CARGO LOGISTICS - PER-TRIAL RAW DATA =====");
        foreach (var t in CargoLogisticsResults.Trials)
        {
            sb.AppendLine($"[{(t.isPractice ? "practice" : "scored")}] problem {t.problemIndex}:");
            sb.Append(FormatTrial(t));
        }

        sb.AppendLine("===== CARGO LOGISTICS - FINAL SCORES (practice trial excluded) =====");
        sb.AppendLine($"Age band: {CargoLogisticsNorms.CurrentLabel}");
        sb.AppendLine($"Scored trials: {ScoredTrialCount()}");
        sb.AppendLine($"logicalReasoningScore = {logicalReasoningScore} / 100");
        sb.AppendLine($"thinkingTimeScore     = {thinkingTimeScore} / 100");

        Debug.Log(sb.ToString());
    }
}