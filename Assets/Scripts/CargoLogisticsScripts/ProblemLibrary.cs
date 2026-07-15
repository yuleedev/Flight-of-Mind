using System.Collections.Generic;
using UnityEngine;

public class TowerOfLondonProblem
{
    public List<List<string>> goal;
    public int optimalMoves;
    public bool isPractice;

    public TowerOfLondonProblem(List<List<string>> goal, int optimalMoves, bool isPractice = false)
    {
        this.goal = goal;
        this.optimalMoves = optimalMoves;
        this.isPractice = isPractice;
    }
}

public static class ProblemLibrary
{

    public static readonly List<List<string>> FixedStart = new List<List<string>>
    {
        new(){ "OrangeCargo", "WhiteCargo" },
        new(){ "BlueCargo" },
        new(){ }
    };

    // 1 practice trial (2 moves) + 12 scored trials (2,2,3,3,4,4,4,4,5,5,5,5),
    //A:3 B:2 C:1
    public static readonly List<TowerOfLondonProblem> Sequence = new List<TowerOfLondonProblem>
    {
        new(new(){ new(){}, new(){"OrangeCargo","BlueCargo"}, new(){"WhiteCargo"} }, 2, isPractice: true),

        new(new(){ new(){}, new(){"WhiteCargo","BlueCargo"}, new(){"OrangeCargo"} }, 2),
        new(new(){ new(){"BlueCargo","WhiteCargo"}, new(){}, new(){"OrangeCargo"} }, 2),

        new(new(){ new(){"OrangeCargo"}, new(){"BlueCargo"}, new(){"WhiteCargo"} }, 3),
        new(new(){ new(){"OrangeCargo"}, new(){"WhiteCargo","BlueCargo"}, new(){} }, 3),

        new(new(){ new(){"BlueCargo","OrangeCargo"}, new(){}, new(){"WhiteCargo"} }, 4),
        new(new(){ new(){"WhiteCargo","OrangeCargo"}, new(){"BlueCargo"}, new(){} }, 4),
        new(new(){ new(){"BlueCargo"}, new(){"WhiteCargo","OrangeCargo"}, new(){} }, 4),
        new(new(){ new(){}, new(){"BlueCargo","OrangeCargo"}, new(){"WhiteCargo"} }, 4),

        new(new(){ new(){"WhiteCargo","BlueCargo","OrangeCargo"}, new(){}, new(){} }, 5),
        new(new(){ new(){"BlueCargo","OrangeCargo"}, new(){"WhiteCargo"}, new(){} }, 5),
        new(new(){ new(){"BlueCargo","WhiteCargo","OrangeCargo"}, new(){}, new(){} }, 5),
        new(new(){ new(){"WhiteCargo","OrangeCargo"}, new(){}, new(){"BlueCargo"} }, 5),
    };
}