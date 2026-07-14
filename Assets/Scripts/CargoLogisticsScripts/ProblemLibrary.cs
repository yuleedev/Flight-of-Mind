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
    // Every list is top-to-bottom. Capacities must be Stack_A=3, Stack_B=2, Stack_C=1
    // (matching your tall/medium/short pegs) — these move counts are only valid under that setup.
    public static readonly List<TowerOfLondonProblem> All = new List<TowerOfLondonProblem>
    {
        new(new(){new(){"BlueCargo","RedCargo"}, new(){"GreenCargo"}, new(){}},
            new(){new(){"RedCargo"}, new(){"BlueCargo"}, new(){"GreenCargo"}}, 2),
        new(new(){new(){"BlueCargo"}, new(){"RedCargo","GreenCargo"}, new(){}},
            new(){new(){"RedCargo","GreenCargo","BlueCargo"}, new(){}, new(){}}, 3),
        new(new(){new(){"BlueCargo","RedCargo","GreenCargo"}, new(){}, new(){}},
            new(){new(){"BlueCargo","GreenCargo"}, new(){}, new(){"RedCargo"}}, 3),
        new(new(){new(){"RedCargo"}, new(){"BlueCargo"}, new(){"GreenCargo"}},
            new(){new(){}, new(){"RedCargo","GreenCargo"}, new(){"BlueCargo"}}, 4),
        new(new(){new(){"BlueCargo"}, new(){"RedCargo","GreenCargo"}, new(){}},
            new(){new(){"BlueCargo"}, new(){"GreenCargo","RedCargo"}, new(){}}, 4),
        new(new(){new(){"RedCargo","BlueCargo","GreenCargo"}, new(){}, new(){}},
            new(){new(){"RedCargo"}, new(){"BlueCargo"}, new(){"GreenCargo"}}, 5),
        new(new(){new(){"RedCargo","BlueCargo","GreenCargo"}, new(){}, new(){}},
            new(){new(){"GreenCargo","RedCargo"}, new(){"BlueCargo"}, new(){}}, 5),
        new(new(){new(){"BlueCargo"}, new(){"GreenCargo"}, new(){"RedCargo"}},
            new(){new(){"BlueCargo","GreenCargo","RedCargo"}, new(){}, new(){}}, 5),
        new(new(){new(){"BlueCargo","GreenCargo"}, new(){"RedCargo"}, new(){}},
            new(){new(){"GreenCargo","RedCargo"}, new(){}, new(){"BlueCargo"}}, 6),
        new(new(){new(){"GreenCargo"}, new(){"BlueCargo","RedCargo"}, new(){}},
            new(){new(){}, new(){"BlueCargo","GreenCargo"}, new(){"RedCargo"}}, 6),
        new(new(){new(){"RedCargo","GreenCargo"}, new(){"BlueCargo"}, new(){}},
            new(){new(){}, new(){"BlueCargo","GreenCargo"}, new(){"RedCargo"}}, 7),
        new(new(){new(){"BlueCargo","GreenCargo"}, new(){}, new(){"RedCargo"}},
            new(){new(){"BlueCargo"}, new(){"RedCargo"}, new(){"GreenCargo"}}, 4),
    };

    public static TowerOfLondonProblem PickRandom() => All[Random.Range(0, All.Count)];
}