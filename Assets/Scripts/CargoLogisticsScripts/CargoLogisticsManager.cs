using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CargoLogisticsManager : MonoBehaviour
{
    public static CargoLogisticsManager Instance;

    [SerializeField] private StackSlot[] slots;
    [SerializeField] private MoveCount moveCounter;
    [SerializeField] private GameObject redCargo;
    [SerializeField] private GameObject blueCargo;
    [SerializeField] private GameObject greenCargo;

    private Canvas canvas;
    private int moveCount = 0;
    private int optimalMoves = 0;
    private bool gameOver = false;
    private List<List<string>> goalState;

    public bool IsGameOver => gameOver;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        canvas = FindFirstObjectByType<Canvas>();
    }

    void Start()
    {
        TowerOfLondonProblem problem = ProblemLibrary.PickRandom();
        goalState = problem.goal;
        optimalMoves = problem.optimalMoves;

        ApplyArrangement(problem.start);   // Randomized problem with the set arrangement.
        GoalPreview.Build(canvas, problem.goal);
        moveCounter.SetMoves(0);

        foreach (var slot in slots)
            slot.RestackItems();
    }

    private void ApplyArrangement(List<List<string>> arrangement)
    {
        Debug.Log($"Applying arrangement: {string.Join(" | ", arrangement.Select(s => string.Join(",", s)))}");
        var lookup = new Dictionary<string, GameObject>
        {
            { "RedCargo", redCargo },
            { "BlueCargo", blueCargo },
            { "GreenCargo", greenCargo },
        };

        for (int s = 0; s < arrangement.Count && s < slots.Length; s++)
        {
            for (int i = arrangement[s].Count - 1; i >= 0; i--)
            {
                GameObject item = lookup[arrangement[s][i]];
                item.transform.SetParent(slots[s].transform);
                item.transform.SetAsFirstSibling();
            }
        }
    }

    public void RegisterMove()
    {
        if (gameOver) return;

        moveCount++;
        moveCounter.SetMoves(moveCount);

        if (IsSolved())
            EndGame();
    }

    private bool IsSolved()
    {
        Debug.Log($"CHECKING — actual vs goal at solve time");
        for (int i = 0; i < slots.Length; i++)
        {
            var actual = slots[i].transform.Cast<Transform>().Select(t => t.name).ToList();
            if (!actual.SequenceEqual(goalState[i])) return false;
        }
        return true;
    }

    private void EndGame()
    {
        gameOver = true;
        ResultDisplay.Show(canvas, moveCount, optimalMoves);
        Time.timeScale = 0f;
    }
}