using System.Collections.Generic;

[System.Serializable]
public class TrialResult
{
    public int problemIndex;
    public bool isPractice;
    public int movesTaken;
    public int optimalMoves;
    public int excessMoves;
    public float thinkingTimeSeconds;
}

public static class CargoLogisticsResults
{
    public static readonly List<TrialResult> Trials = new List<TrialResult>();

    public static void Clear() => Trials.Clear();

    public static void Record(int problemIndex, bool isPractice, int movesTaken, int optimalMoves, float thinkingTimeSeconds)
    {
        Trials.Add(new TrialResult
        {
            problemIndex = problemIndex,
            isPractice = isPractice,
            movesTaken = movesTaken,
            optimalMoves = optimalMoves,
            excessMoves = movesTaken - optimalMoves,
            thinkingTimeSeconds = thinkingTimeSeconds
        });
    }
}