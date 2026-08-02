using UnityEngine;

public struct TrailMakingScores
{
    public bool hasSpeedScore;
    public bool hasFlexibilityScore;

    public int processingSpeedScore;
    public int cognitiveFlexibilityScore;

    public float effectivePartASeconds;
    public float effectivePartBSeconds;

    public float parPartASeconds;
    public float paceRatio;
    public float switchCostRatio;
}

public static class TrailMakingScoring
{
    public static float ParSecondsPerHop = 1.50f;
    public static float PaceHalvingPoint = 1.20f;
    public static float SwitchCostPar = 1.80f;
    public static float SwitchCostHalvingPoint = 1.10f;
    public static float ErrorCostInHops = 0.4f;
    public static int TargetsPerRoute = 25;

    private const float MinValidSeconds = 1f;

    public static int Hops => Mathf.Max(1, TargetsPerRoute - 1);

    public static float ParPartASeconds => ParSecondsPerHop * Hops;

    public static void Configure(float parSecondsPerHop, float paceHalvingPoint,
                                 float switchCostPar, float switchCostHalvingPoint,
                                 float errorCostInHops)
    {
        ParSecondsPerHop = Mathf.Max(0.05f, parSecondsPerHop);
        PaceHalvingPoint = Mathf.Max(0.05f, paceHalvingPoint);
        SwitchCostPar = Mathf.Max(1f, switchCostPar);
        SwitchCostHalvingPoint = Mathf.Max(0.05f, switchCostHalvingPoint);
        ErrorCostInHops = Mathf.Max(0f, errorCostInHops);
    }

    public static float EffectiveSeconds(float seconds, int errors)
    {
        float penalty = ErrorCostInHops * Mathf.Max(0, errors) / Hops;
        return Mathf.Max(0f, seconds) * (1f + penalty);
    }

    public static int HalvingScore(float excess, float halvingPoint)
    {
        if (excess <= 0f)
            return 100;

        float value = 100f * Mathf.Pow(0.5f, excess / Mathf.Max(0.05f, halvingPoint));
        return Mathf.Clamp(Mathf.RoundToInt(value), 0, 100);
    }

    public static TrailMakingScores Compute()
    {
        TrailMakingScores scores = new TrailMakingScores();
        scores.parPartASeconds = ParPartASeconds;

        TrailMakingResult a = TrailMakingResults.Get("A");
        TrailMakingResult b = TrailMakingResults.Get("B");

        if (a == null || a.timeSeconds < MinValidSeconds)
            return scores;

        scores.effectivePartASeconds = EffectiveSeconds(a.timeSeconds, a.errors);
        scores.paceRatio = scores.effectivePartASeconds / scores.parPartASeconds;
        scores.processingSpeedScore = HalvingScore(scores.paceRatio - 1f, PaceHalvingPoint);
        scores.hasSpeedScore = true;

        if (b == null || b.timeSeconds < MinValidSeconds)
            return scores;

        scores.effectivePartBSeconds = EffectiveSeconds(b.timeSeconds, b.errors);
        scores.switchCostRatio = scores.effectivePartBSeconds / scores.effectivePartASeconds;
        scores.cognitiveFlexibilityScore =
            HalvingScore(scores.switchCostRatio - SwitchCostPar, SwitchCostHalvingPoint);
        scores.hasFlexibilityScore = true;

        return scores;
    }

    public static void LogScores()
    {
        TrailMakingScores s = Compute();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("===== LANDING ROUTES - GAME SCORES =====");

        if (!s.hasSpeedScore)
        {
            sb.AppendLine("  no usable Part A time, scores not computed");
            Debug.Log(sb.ToString());
            return;
        }

        sb.AppendLine($"  processing speed {s.processingSpeedScore}/100 - effective A " +
                      $"{s.effectivePartASeconds:F1}s against par {s.parPartASeconds:F1}s " +
                      $"({Hops} hops at {ParSecondsPerHop:F2}s), pace {s.paceRatio:F2}x par, " +
                      $"every +{PaceHalvingPoint:P0} over par halves the score");

        if (s.hasFlexibilityScore)
        {
            sb.AppendLine($"  cognitive flexibility {s.cognitiveFlexibilityScore}/100 - switch cost " +
                          $"{s.switchCostRatio:F2}x against par {SwitchCostPar:F2}x, " +
                          $"every +{SwitchCostHalvingPoint:F2} beyond par halves the score");
        }
        else
        {
            sb.AppendLine("  cognitive flexibility not available, Part B was not completed");
        }

        Debug.Log(sb.ToString());
    }
}
