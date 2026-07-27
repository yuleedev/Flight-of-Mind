using UnityEngine;

[System.Serializable]
public struct AgeNorm
{
    public string label;
    public int maxAge;
    public float baseSeconds;
    public float secondsPerMove;
    public float errorDecay;
}

public static class CargoLogisticsNorms
{
    public static readonly AgeNorm[] Groups =
    {
        new AgeNorm { label = "6-8",   maxAge = 8,   baseSeconds = 6.30f, secondsPerMove = 8.40f, errorDecay = 0.80f },
        new AgeNorm { label = "9-11",  maxAge = 11,  baseSeconds = 5.25f, secondsPerMove = 7.00f, errorDecay = 0.95f },
        new AgeNorm { label = "12-14", maxAge = 14,  baseSeconds = 4.35f, secondsPerMove = 5.80f, errorDecay = 1.15f },
        new AgeNorm { label = "15-17", maxAge = 17,  baseSeconds = 3.60f, secondsPerMove = 4.80f, errorDecay = 1.40f },
        new AgeNorm { label = "18-24", maxAge = 24,  baseSeconds = 3.15f, secondsPerMove = 4.20f, errorDecay = 1.55f },
        new AgeNorm { label = "25-34", maxAge = 34,  baseSeconds = 3.00f, secondsPerMove = 4.00f, errorDecay = 1.60f },
        new AgeNorm { label = "35-44", maxAge = 44,  baseSeconds = 3.15f, secondsPerMove = 4.20f, errorDecay = 1.55f },
        new AgeNorm { label = "45-54", maxAge = 54,  baseSeconds = 3.45f, secondsPerMove = 4.60f, errorDecay = 1.45f },
        new AgeNorm { label = "55-64", maxAge = 64,  baseSeconds = 4.05f, secondsPerMove = 5.40f, errorDecay = 1.30f },
        new AgeNorm { label = "65+",   maxAge = 120, baseSeconds = 4.95f, secondsPerMove = 6.60f, errorDecay = 1.10f },
    };

    public const int ReferenceGroupIndex = 5;

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