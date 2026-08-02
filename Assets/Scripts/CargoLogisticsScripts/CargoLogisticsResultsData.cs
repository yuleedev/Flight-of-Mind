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
    public int ruleViolations;
    public float errorIndex;
    public float creditFraction;
    public float itemDifficultyLogits;
    public float initialThinkingSeconds;
    public float subsequentThinkingSeconds;
    public float animationSeconds;
    public float totalSeconds;
    public float deliberationCostSeconds;
    public float expectedDeliberationSeconds;
    public float paceRatio;
    public float logPaceDeviation;
    public float planningRatio;
    public int ageGroupIndex;
    public string ageGroupLabel;
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
                              int ruleViolations, float initialThinking, float subsequentThinking,
                              float animationSeconds, float totalSeconds)
    {
        float expected = CargoLogisticsScoring.ExpectedDeliberation(optimalMoves);
        float cost = CargoLogisticsScoring.DeliberationCost(initialThinking, subsequentThinking);
        float pace = CargoLogisticsScoring.PaceRatio(cost, expected);

        Trials.Add(new TrialResult
        {
            problemIndex = problemIndex,
            isPractice = isPractice,
            movesTaken = movesTaken,
            optimalMoves = optimalMoves,
            excessMoves = Mathf.Max(0, movesTaken - optimalMoves),
            ruleViolations = ruleViolations,
            errorIndex = CargoLogisticsScoring.ErrorIndex(optimalMoves, movesTaken),
            creditFraction = CargoLogisticsScoring.CreditFraction(optimalMoves, movesTaken, ruleViolations),
            itemDifficultyLogits = CargoLogisticsScoring.ItemDifficulty(optimalMoves),
            initialThinkingSeconds = initialThinking,
            subsequentThinkingSeconds = subsequentThinking,
            animationSeconds = animationSeconds,
            totalSeconds = totalSeconds,
            deliberationCostSeconds = cost,
            expectedDeliberationSeconds = expected,
            paceRatio = pace,
            logPaceDeviation = Mathf.Log(pace),
            planningRatio = CargoLogisticsScoring.PlanningRatio(initialThinking, subsequentThinking),
            ageGroupIndex = CargoLogisticsNorms.CurrentGroupIndex,
            ageGroupLabel = CargoLogisticsNorms.CurrentLabel
        });

        CargoLogisticsScoring.LogTrial(Trials[Trials.Count - 1]);
    }
}

public static class CargoLogisticsScoring
{
public static float InitialThinkingWeight = 1.00f;
    public static float SubsequentThinkingWeight = 2.33f;
    public static float PaceTolerance = 0.60f;
    public static float FastPaceMultiplier = 1.55f;
    public static float PaceDeviationLimit = 1.40f;
    public static float ExpectedTimeMultiplier = 1.00f;
    public const float ExcessMoveDecay = 1.60f;
    public const float ViolationDecay = 0.25f;
    public const float LogitsPerMove = 1.10f;
    public const float AbilityPriorSd = 1.50f;
    public const float ScorePerLogit = 20f;
    public const float ScoreMidpoint = 50f;
    private const float MinCostSeconds = 0.25f;

    public static void ConfigurePacing(float expectedTimeMultiplier, float paceTolerance,
                                       float initialThinkingWeight, float subsequentThinkingWeight)
    {
        ExpectedTimeMultiplier = Mathf.Max(0.05f, expectedTimeMultiplier);
        PaceTolerance = Mathf.Max(0.05f, paceTolerance);
        InitialThinkingWeight = Mathf.Max(0f, initialThinkingWeight);
        SubsequentThinkingWeight = Mathf.Max(0f, subsequentThinkingWeight);
    }

    public static int logicalReasoningScore;
    public static int thinkingTimeScore;
    public static float rawReasoningScore;
    public static float reasoningStandardError;
    public static float abilityLogits;
    public static float paceRmsDeviation;
    public static float paceMeanLogDeviation;
    public static bool AtCeiling;
    public static bool AtFloor;
    public static bool FinalScoresReady;

    public static void ResetFinalScores()
    {
        logicalReasoningScore = 0;
        thinkingTimeScore = 0;
        rawReasoningScore = 0f;
        reasoningStandardError = 0f;
        abilityLogits = 0f;
        paceRmsDeviation = 0f;
        paceMeanLogDeviation = 0f;
        AtCeiling = false;
        AtFloor = false;
        FinalScoresReady = false;
    }

    public static float ErrorIndex(int optimalMoves, int movesTaken)
    {
        if (optimalMoves <= 0) return 0f;
        return Mathf.Max(0, movesTaken - optimalMoves) / (float)optimalMoves;
    }

    public static float CreditFraction(int optimalMoves, int movesTaken, int ruleViolations)
    {
        if (optimalMoves <= 0 || movesTaken <= 0) return 0f;

        float penalty = ExcessMoveDecay * ErrorIndex(optimalMoves, movesTaken)
                      + ViolationDecay * Mathf.Max(0, ruleViolations);
        return Mathf.Clamp01(Mathf.Exp(-penalty));
    }

    public static float ItemDifficulty(int optimalMoves)
    {
        return LogitsPerMove * (Mathf.Max(1, optimalMoves) - CargoLogisticsNorms.Current.planningMidpoint);
    }

    public static float ExpectedDeliberation(int optimalMoves)
    {
        AgeNorm norm = CargoLogisticsNorms.Current;
        return (norm.baseSeconds + norm.secondsPerMove * Mathf.Max(1, optimalMoves)) * ExpectedTimeMultiplier;
    }

    public static float DeliberationCost(float initialThinking, float subsequentThinking)
    {
        return InitialThinkingWeight * Mathf.Max(0f, initialThinking)
             + SubsequentThinkingWeight * Mathf.Max(0f, subsequentThinking);
    }

    public static float PaceRatio(float cost, float expected)
    {
        if (expected <= 0f) return 1f;
        return Mathf.Max(cost, MinCostSeconds) / expected;
    }

    public static float PaceRatio(float initialThinking, float subsequentThinking, int optimalMoves)
    {
        return PaceRatio(DeliberationCost(initialThinking, subsequentThinking),
                         ExpectedDeliberation(optimalMoves));
    }

    public static float PlanningRatio(float initialThinking, float subsequentThinking)
    {
        float sum = Mathf.Max(0f, initialThinking) + Mathf.Max(0f, subsequentThinking);
        return sum <= 0f ? 0f : Mathf.Clamp01(initialThinking / sum);
    }

    public static float ScaledPaceDeviation(float logDeviation)
    {
        float limited = Mathf.Clamp(logDeviation, -PaceDeviationLimit, PaceDeviationLimit);
        float tolerance = limited < 0f
            ? PaceTolerance * Mathf.Max(1f, FastPaceMultiplier)
            : PaceTolerance;

        return limited / Mathf.Max(0.05f, tolerance);
    }

    public static float PaceScoreFromDeviation(float logDeviation)
    {
        float scaled = ScaledPaceDeviation(logDeviation);
        return 100f * Mathf.Exp(-scaled * scaled / 2f);
    }

    private static float Logistic(float x)
    {
        return 1f / (1f + Mathf.Exp(-x));
    }

    public static int ScoredTrialCount()
    {
        int n = 0;
        foreach (var t in CargoLogisticsResults.Trials)
            if (!t.isPractice) n++;
        return n;
    }

    private static void EstimateAbility(out float theta, out float standardError)
    {
        float priorPrecision = 1f / (AbilityPriorSd * AbilityPriorSd);
        theta = 0f;

        if (ScoredTrialCount() == 0)
        {
            standardError = AbilityPriorSd;
            return;
        }

        for (int iteration = 0; iteration < 40; iteration++)
        {
            float gradient = -theta * priorPrecision;
            float information = priorPrecision;

            foreach (var t in CargoLogisticsResults.Trials)
            {
                if (t.isPractice) continue;
                float p = Logistic(theta - t.itemDifficultyLogits);
                gradient += t.creditFraction - p;
                information += p * (1f - p);
            }

            float step = gradient / information;
            theta = Mathf.Clamp(theta + step, -6f, 6f);
            if (Mathf.Abs(step) < 0.00001f) break;
        }

        float finalInformation = priorPrecision;
        foreach (var t in CargoLogisticsResults.Trials)
        {
            if (t.isPractice) continue;
            float p = Logistic(theta - t.itemDifficultyLogits);
            finalInformation += p * (1f - p);
        }

        standardError = 1f / Mathf.Sqrt(finalInformation);
    }

    private static float EstimatePaceScore(out float rmsDeviation, out float meanDeviation)
    {
        float weightSum = 0f;
        float squaredSum = 0f;
        float rawSquaredSum = 0f;
        float linearSum = 0f;

        foreach (var t in CargoLogisticsResults.Trials)
        {
            if (t.isPractice) continue;
            float w = Mathf.Max(0.01f, t.expectedDeliberationSeconds);
            float scaled = ScaledPaceDeviation(t.logPaceDeviation);
            weightSum += w;
            squaredSum += w * scaled * scaled;
            rawSquaredSum += w * t.logPaceDeviation * t.logPaceDeviation;
            linearSum += w * t.logPaceDeviation;
        }

        if (weightSum <= 0f)
        {
            rmsDeviation = 0f;
            meanDeviation = 0f;
            return 0f;
        }

        float meanSquare = squaredSum / weightSum;
        rmsDeviation = Mathf.Sqrt(rawSquaredSum / weightSum);
        meanDeviation = linearSum / weightSum;
        return 100f * Mathf.Exp(-meanSquare / 2f);
    }

    public static void ComputeFinalScores()
    {
        EstimateAbility(out abilityLogits, out float standardErrorLogits);

        rawReasoningScore = ScoreMidpoint + ScorePerLogit * abilityLogits;
        logicalReasoningScore = Mathf.Clamp(Mathf.RoundToInt(rawReasoningScore), 0, 100);
        reasoningStandardError = ScorePerLogit * standardErrorLogits;

        thinkingTimeScore = Mathf.Clamp(
            Mathf.RoundToInt(EstimatePaceScore(out paceRmsDeviation, out paceMeanLogDeviation)), 0, 100);

        AtCeiling = rawReasoningScore >= 99.5f;
        AtFloor = rawReasoningScore <= 0.5f;
        FinalScoresReady = ScoredTrialCount() > 0;

        LogFinalResults();
        
    }

    public static void LogMove(int problemIndex, bool isPractice, int movesSoFar, int optimalMoves,
                               int ruleViolations, float initialThinking, float subsequentThinking)
    {
        float cost = DeliberationCost(initialThinking, subsequentThinking);
        float expected = ExpectedDeliberation(optimalMoves);
        float pace = PaceRatio(cost, expected);
        float credit = CreditFraction(optimalMoves, movesSoFar, ruleViolations);

        Debug.Log(
            $"[{(isPractice ? "practice" : "scored")}] problem {problemIndex} move {movesSoFar}/{optimalMoves} -> " +
            $"credit {credit:F3} | violations {ruleViolations} | " +
            $"cost {cost:F2}s vs expected {expected:F2}s (pace {pace:F2}x, " +
            $"pace score {PaceScoreFromDeviation(Mathf.Log(pace)):F0}/100) | " +
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
            EstimateAbility(out float theta, out float se);
            float raw = ScoreMidpoint + ScorePerLogit * theta;
            int running = Mathf.Clamp(Mathf.RoundToInt(raw), 0, 100);
            float runningPace = EstimatePaceScore(out float rms, out float meanDev);

            sb.AppendLine($"    running after {ScoredTrialCount()} scored trial(s): " +
                          $"logical reasoning {running}/100 (uncapped {raw:F0}, +/- {ScorePerLogit * se:F0}) | " +
                          $"thinking time {runningPace:F0}/100 (typical pace {Mathf.Exp(meanDev):F2}x, " +
                          $"spread {Mathf.Exp(rms):F2}x)");
        }

        Debug.Log(sb.ToString());
    }

    private static string FormatTrial(TrialResult t)
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine(
            $"    moves {t.movesTaken}/{t.optimalMoves} (excess {t.excessMoves}, error index {t.errorIndex:F2}) | " +
            $"violations {t.ruleViolations} -> credit {t.creditFraction:F3} " +
            $"(item difficulty {t.itemDifficultyLogits:F2} logits)");
        sb.AppendLine(
            $"    initial {t.initialThinkingSeconds:F2}s | subsequent {t.subsequentThinkingSeconds:F2}s | " +
            $"animation {t.animationSeconds:F2}s | total {t.totalSeconds:F2}s | " +
            $"planning ratio {t.planningRatio:F2}");
        sb.AppendLine(
            $"    weighted cost {t.deliberationCostSeconds:F2}s vs expected {t.expectedDeliberationSeconds:F2}s " +
            $"-> pace {t.paceRatio:F2}x (log dev {t.logPaceDeviation:F2}) | age band {t.ageGroupLabel}");

        return sb.ToString();
    }

    private static void LogFinalResults()
    {
        var raw = new System.Text.StringBuilder();

        raw.AppendLine("===== CARGO LOGISTICS - PER-TRIAL RAW DATA =====");
        foreach (var t in CargoLogisticsResults.Trials)
        {
            raw.AppendLine($"[{(t.isPractice ? "practice" : "scored")}] problem {t.problemIndex}:");
            raw.Append(FormatTrial(t));
        }

        Debug.Log(raw.ToString());

        var final = new System.Text.StringBuilder();

        final.AppendLine($"CARGO LOGISTICS FINAL - logical reasoning {logicalReasoningScore}/100 | " +
                         $"thinking time {thinkingTimeScore}/100");
        final.AppendLine($"  logical reasoning {logicalReasoningScore}/100 - 50/100 is average for the age band, " +
                         $"plausible range {Mathf.Max(0f, rawReasoningScore - reasoningStandardError):F0}-" +
                         $"{Mathf.Min(100f, rawReasoningScore + reasoningStandardError):F0}");
        final.AppendLine($"  thinking time {thinkingTimeScore}/100 - 100/100 means paced as expected, " +
                         $"score falls if much faster OR much slower");
        final.AppendLine($"  typical pace was {Mathf.Exp(paceMeanLogDeviation):F2}x the expected time " +
                         $"(consistency spread {Mathf.Exp(paceRmsDeviation):F2}x)");
        final.AppendLine($"  age band {CargoLogisticsNorms.CurrentLabel} | scored trials {ScoredTrialCount()} " +
                         $"(practice excluded) | ability {abilityLogits:F2} logits");

        if (AtCeiling)
            final.AppendLine($"  CEILING: uncapped estimate was {rawReasoningScore:F0} - the score is a lower " +
                             $"bound, not a point estimate. These items are too easy to measure this player.");
        if (AtFloor)
            final.AppendLine($"  FLOOR: uncapped estimate was {rawReasoningScore:F0} - the score is an upper " +
                             $"bound. These items are too hard to measure this player.");

        Debug.Log(final.ToString());
        Debug.Log($"Thinking Time: {thinkingTimeScore}/100\nLogical Reasoning: {logicalReasoningScore}/100");
    }
}