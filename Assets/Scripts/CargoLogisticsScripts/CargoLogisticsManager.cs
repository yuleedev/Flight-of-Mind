using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class CargoLogisticsManager : MonoBehaviour
{
    public static CargoLogisticsManager Instance;

    [SerializeField] private StackSlot[] slots;
    [SerializeField] private MoveCount moveCounter;

    [FormerlySerializedAs("redCargo")]
    [SerializeField] private GameObject orangeCargo;
    [SerializeField] private GameObject blueCargo;
    [FormerlySerializedAs("greenCargo")]
    [SerializeField] private GameObject whiteCargo;

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

        ApplyArrangement(problem.start);

        var itemSprites = new Dictionary<string, Sprite>
        {
            { "OrangeCargo", orangeCargo.GetComponent<Image>().sprite },
            { "BlueCargo",   blueCargo.GetComponent<Image>().sprite },
            { "WhiteCargo",  whiteCargo.GetComponent<Image>().sprite },
        };
        GoalPreview.Build(canvas, problem.goal, itemSprites);

        moveCounter.SetMoves(0);

        foreach (var slot in slots)
            slot.RestackItems();
    }

    private void ApplyArrangement(List<List<string>> arrangement)
    {
        var lookup = new Dictionary<string, GameObject>
        {
            { "OrangeCargo", orangeCargo },
            { "BlueCargo", blueCargo },
            { "WhiteCargo", whiteCargo },
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