using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

    [Header("Goal Preview Icons")]
    [SerializeField] private Sprite orangeGoalIcon;
    [SerializeField] private Sprite blueGoalIcon;
    [SerializeField] private Sprite whiteGoalIcon;

    [Header("Goal Preview Slots")]
    [SerializeField] private GoalSlotDisplay[] goalSlots;

    [Header("Instructions")]
    [SerializeField] private GameObject startPanel;

    private Canvas canvas;
    private int moveCount = 0;
    private int currentProblemIndex = -1;
    private bool problemSolved = false;
    private bool sessionComplete = false;
    private List<List<string>> goalState;
    private int currentOptimalMoves;

    public bool IsGameOver => sessionComplete;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        canvas = FindAnyObjectByType<Canvas>();
    }

    void Start()
    {
        if (startPanel != null) startPanel.SetActive(true);
        else LoadProblem(0);
    }

    public void OnStartClicked()
    {
        CargoLogisticsResults.Clear();

        if (startPanel != null) startPanel.SetActive(false);
        LoadProblem(0);
    }

    private void LoadProblem(int index)
    {
        currentProblemIndex = index;
        problemSolved = false;

        TowerOfLondonProblem problem = ProblemLibrary.Sequence[index];
        goalState = problem.goal;
        currentOptimalMoves = problem.optimalMoves;
        moveCount = 0;

        ApplyArrangement(ProblemLibrary.FixedStart);

        var itemSprites = new Dictionary<string, Sprite>
        {
            { "OrangeCargo", orangeGoalIcon },
            { "BlueCargo",   blueGoalIcon },
            { "WhiteCargo",  whiteGoalIcon },
        };
        GoalPreview.Build(goalSlots, problem.goal, itemSprites);

        moveCounter.SetMoves(0);

        foreach (var slot in slots)
            slot.RestackItems();

        if (thinkingTime.Instance != null)
            thinkingTime.Instance.ResetTimer();
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
            for (int i = 0; i < arrangement[s].Count; i++)
            {
                GameObject item = lookup[arrangement[s][i]];
                item.transform.SetParent(slots[s].transform, false);
                item.transform.SetAsFirstSibling();
            }
        }
    }

    public void RegisterMove()
    {
        if (sessionComplete || problemSolved) return;

        moveCount++;
        moveCounter.SetMoves(moveCount);

        if (IsSolved())
        {
            problemSolved = true;
            OnProblemSolved();
        }
    }

    private bool IsSolved()
    {
        for (int i = 0; i < slots.Length && i < goalState.Count; i++)
        {

            var actual = slots[i].transform.Cast<Transform>().Select(t => t.name).ToList();
            var expected = Enumerable.Reverse(goalState[i]).ToList();
            if (!actual.SequenceEqual(expected)) return false;
        }
        return true;
    }

    private void OnProblemSolved()
    {
        TowerOfLondonProblem problem = ProblemLibrary.Sequence[currentProblemIndex];
        bool isLastProblem = currentProblemIndex == ProblemLibrary.Sequence.Count - 1;

        float thinkTime = thinkingTime.Instance != null ? thinkingTime.Instance.ThinkingTimeSeconds : 0f;
        CargoLogisticsResults.Record(currentProblemIndex, problem.isPractice, moveCount, currentOptimalMoves, thinkTime);

        Debug.Log($"Trial {currentProblemIndex}: {moveCount} moves ({moveCount - currentOptimalMoves} excess), {thinkTime:F2}s to first move");

        string message;
        if (problem.isPractice)
            message = "Practice complete! Starting the timed problems...";
        else if (isLastProblem)
            message = $"All problems complete! Last one solved in {moveCount} moves (optimal was {currentOptimalMoves}).";
        else
            message = $"Solved in {moveCount} moves (optimal was {currentOptimalMoves}).";

        ResultDisplay.Show(canvas, message);
        Time.timeScale = 0f;

        StartCoroutine(AdvanceAfterDelay(2f, isLastProblem));
    }

    private IEnumerator AdvanceAfterDelay(float seconds, bool isLastProblem)
    {
        yield return new WaitForSecondsRealtime(seconds);
        Time.timeScale = 1f;
        ResultDisplay.Hide();

        if (isLastProblem)
        {
            sessionComplete = true;
            SceneManager.LoadScene("Radar");
        }
        else
        {
            LoadProblem(currentProblemIndex + 1);
        }
    }
}