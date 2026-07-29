public static class RadarWatchResults
{
    public static int FinalScore { get; private set; }

    public static int Passes { get; private set; }
    public static int Fails { get; private set; }
    public static int FalsePositives { get; private set; }
    public static int FalseNegatives { get; private set; }

    public static bool HasReactionTime { get; private set; }
    public static float AverageReactionTime { get; private set; }

    public static int ReactionTimeScore { get; private set; }

    public static bool HasResults { get; private set; }

    public static void Record(
        int finalScore,
        int passes,
        int fails,
        int falsePositives,
        int falseNegatives,
        bool hasReactionTime,
        float averageReactionTime)
    {
        FinalScore = finalScore;
        Passes = passes;
        Fails = fails;
        FalsePositives = falsePositives;
        FalseNegatives = falseNegatives;
        HasReactionTime = hasReactionTime;
        AverageReactionTime = averageReactionTime;
        HasResults = true;
    }

    public static void SetReactionTimeScore(int score)
    {
        ReactionTimeScore = score;
    }

    public static void Clear()
    {
        FinalScore = 0;
        Passes = 0;
        Fails = 0;
        FalsePositives = 0;
        FalseNegatives = 0;
        HasReactionTime = false;
        AverageReactionTime = 0f;
        ReactionTimeScore = 0;
        HasResults = false;
    }
}