using System.Collections.Generic;
using UnityEngine;

public class TowerOfLondonProblem
{
    public List<List<string>> start;
    public List<List<string>> goal;
    public int optimalMoves;

    public TowerOfLondonProblem(List<List<string>> start, List<List<string>> goal, int optimalMoves)
    {
        this.start = start;
        this.goal = goal;
        this.optimalMoves = optimalMoves;
    }
}

public static class ProblemLibrary
{
    // Every list is top-to-bottom. Capacities must be Stack_A=3, Stack_B=2, Stack_C=1.
    public static readonly List<TowerOfLondonProblem> All = new List<TowerOfLondonProblem>
    {
        new(new(){new(){"BlueCargo","OrangeCargo"}, new(){"WhiteCargo"}, new(){}},
            new(){new(){"OrangeCargo"}, new(){"BlueCargo"}, new(){"WhiteCargo"}}, 2),
        new(new(){new(){"BlueCargo"}, new(){"OrangeCargo","WhiteCargo"}, new(){}},
            new(){new(){"OrangeCargo","WhiteCargo","BlueCargo"}, new(){}, new(){}}, 3),
        new(new(){new(){"BlueCargo","OrangeCargo","WhiteCargo"}, new(){}, new(){}},
            new(){new(){"BlueCargo","WhiteCargo"}, new(){}, new(){"OrangeCargo"}}, 3),
        new(new(){new(){"OrangeCargo"}, new(){"BlueCargo"}, new(){"WhiteCargo"}},
            new(){new(){}, new(){"OrangeCargo","WhiteCargo"}, new(){"BlueCargo"}}, 4),
        new(new(){new(){"BlueCargo"}, new(){"OrangeCargo","WhiteCargo"}, new(){}},
            new(){new(){"BlueCargo"}, new(){"WhiteCargo","OrangeCargo"}, new(){}}, 4),
        new(new(){new(){"OrangeCargo","BlueCargo","WhiteCargo"}, new(){}, new(){}},
            new(){new(){"OrangeCargo"}, new(){"BlueCargo"}, new(){"WhiteCargo"}}, 5),
        new(new(){new(){"OrangeCargo","BlueCargo","WhiteCargo"}, new(){}, new(){}},
            new(){new(){"WhiteCargo","OrangeCargo"}, new(){"BlueCargo"}, new(){}}, 5),
        new(new(){new(){"BlueCargo"}, new(){"WhiteCargo"}, new(){"OrangeCargo"}},
            new(){new(){"BlueCargo","WhiteCargo","OrangeCargo"}, new(){}, new(){}}, 5),
        new(new(){new(){"BlueCargo","WhiteCargo"}, new(){"OrangeCargo"}, new(){}},
            new(){new(){"WhiteCargo","OrangeCargo"}, new(){}, new(){"BlueCargo"}}, 6),
        new(new(){new(){"WhiteCargo"}, new(){"BlueCargo","OrangeCargo"}, new(){}},
            new(){new(){}, new(){"BlueCargo","WhiteCargo"}, new(){"OrangeCargo"}}, 6),
        new(new(){new(){"OrangeCargo","WhiteCargo"}, new(){"BlueCargo"}, new(){}},
            new(){new(){}, new(){"BlueCargo","WhiteCargo"}, new(){"OrangeCargo"}}, 7),
        new(new(){new(){"BlueCargo","WhiteCargo"}, new(){}, new(){"OrangeCargo"}},
            new(){new(){"BlueCargo"}, new(){"OrangeCargo"}, new(){"WhiteCargo"}}, 4),
    };

    public static TowerOfLondonProblem PickRandom() => All[Random.Range(0, All.Count)];
}