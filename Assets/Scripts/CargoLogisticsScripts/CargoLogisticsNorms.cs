using UnityEngine;

[System.Serializable]
public struct AgeNorm
{
    public string label;
    public int maxAge;
    public float planningMidpoint;
    public float baseSeconds;
    public float secondsPerMove;
}

public static class CargoLogisticsNorms
{
    public static readonly AgeNorm[] Groups =
    {
        new AgeNorm { label = "5-8",   maxAge = 8,   planningMidpoint = 2.20f, baseSeconds = 6.41f, secondsPerMove = 8.55f },
        new AgeNorm { label = "9-11",  maxAge = 11,  planningMidpoint = 2.80f, baseSeconds = 5.34f, secondsPerMove = 7.12f },
        new AgeNorm { label = "12-14", maxAge = 14,  planningMidpoint = 3.40f, baseSeconds = 4.42f, secondsPerMove = 5.90f },
        new AgeNorm { label = "15-19", maxAge = 19,  planningMidpoint = 3.95f, baseSeconds = 3.60f, secondsPerMove = 4.80f },
        new AgeNorm { label = "20-34", maxAge = 34,  planningMidpoint = 4.27f, baseSeconds = 3.05f, secondsPerMove = 4.07f },
        new AgeNorm { label = "35-49", maxAge = 49,  planningMidpoint = 4.13f, baseSeconds = 3.23f, secondsPerMove = 4.31f },
        new AgeNorm { label = "50-64", maxAge = 64,  planningMidpoint = 3.80f, baseSeconds = 3.72f, secondsPerMove = 4.97f },
        new AgeNorm { label = "65-79", maxAge = 79,  planningMidpoint = 3.40f, baseSeconds = 4.58f, secondsPerMove = 6.11f },
        new AgeNorm { label = "80+",   maxAge = 120, planningMidpoint = 2.90f, baseSeconds = 5.64f, secondsPerMove = 7.53f },
    };

    public const int ReferenceGroupIndex = 4;

    public static int CurrentGroupIndex { get; private set; } = ReferenceGroupIndex;
    public static bool AgeSet { get; private set; }
    public static int Age { get; private set; }

    public static AgeNorm Current => Groups[CurrentGroupIndex];
    public static AgeNorm Reference => Groups[ReferenceGroupIndex];

    public static string CurrentLabel =>
        AgeSet ? $"{Age}y [{Current.label}]" : $"{Current.label} (default, age not set)";

    public static void SetAge(int years)
    {
        Age = Mathf.Clamp(years, 0, 120);
        AgeSet = true;

        for (int i = 0; i < Groups.Length; i++)
        {
            if (Age <= Groups[i].maxAge)
            {
                CurrentGroupIndex = i;
                return;
            }
        }

        CurrentGroupIndex = Groups.Length - 1;
    }

    public static void SetGroupIndex(int index)
    {
        CurrentGroupIndex = Mathf.Clamp(index, 0, Groups.Length - 1);
        Age = Groups[CurrentGroupIndex].maxAge;
        AgeSet = true;
    }

    public static void Reset()
    {
        CurrentGroupIndex = ReferenceGroupIndex;
        Age = 0;
        AgeSet = false;
    }
}