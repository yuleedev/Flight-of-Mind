using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CargoLogisticsManager : MonoBehaviour
{
    public static CargoLogisticsManager Instance;

    [SerializeField] private StackSlot[] slots;
    [SerializeField] private MoveCount moveCounter;
    [SerializeField] private TMPro.TextMeshProUGUI progressText;

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
    [Tooltip("Font for the result banner. Leave empty to use the TextMeshPro default.")]
    [SerializeField] private TMPro.TMP_FontAsset resultFont;

    [Header("Participant")]
    [Tooltip("Fallback only. Ignored if an earlier scene already called CargoLogisticsNorms.SetAge.")]
    [SerializeField] private int participantAge = 30;

    [Header("Thinking Time Tuning")]
    [Tooltip("Scales every age band's expected deliberation time. Set this to the 'typical pace' figure printed in the final log to centre the score.")]
    [SerializeField, Range(0.1f, 3f)] private float expectedTimeMultiplier = 1.0f;
    [Tooltip("How far off the expected pace a player can drift before the score falls. 0.60 means 2x too slow or 2x too fast scores about 51/100. Raise to be more forgiving.")]
    [SerializeField, Range(0.2f, 1.5f)] private float paceTolerance = 0.60f;
    [Tooltip("Cost multiplier on planning before the first move.")]
    [SerializeField, Range(0f, 3f)] private float initialThinkingWeight = 1.00f;
    [Tooltip("Cost multiplier on hesitation between moves.")]
    [SerializeField, Range(0f, 3f)] private float subsequentThinkingWeight = 2.33f;

    [Header("Audio")]
    [Tooltip("Plays every time a problem is solved.")]
    [SerializeField] private AudioClip solvedSound;
    [SerializeField, Range(0f, 1f)] private float solvedVolume = 0.6f;

    [Header("Result Panel Timing")]
    [Tooltip("How long the result panel stays up between problems.")]
    [SerializeField] private float resultSeconds = 2f;
    [Tooltip("How long the final result panel stays up before the Radar scene loads. Raise this if the last problem's result flashes past.")]
    [SerializeField] private float finalResultSeconds = 4f;
    [Tooltip("Height in canvas pixels of the final panel, which carries an extra line of text.")]
    [SerializeField] private float finalPanelHeight = 130f;

    private Canvas canvas;
    private int moveCount = 0;
    private int ruleViolations = 0;
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

        if (!CargoLogisticsNorms.AgeSet)
            CargoLogisticsNorms.SetAge(participantAge);

        ResultDisplay.SetFont(resultFont);
        ApplyPacingSettings();
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
        SceneMusic.StartGameMusic();
        LoadProblem(0);
    }

    private void LoadProblem(int index)
    {
        currentProblemIndex = index;
        problemSolved = false;
        UpdateProgressDisplay();

        TowerOfLondonProblem problem = ProblemLibrary.Sequence[index];
        goalState = problem.goal;
        currentOptimalMoves = problem.optimalMoves;
        moveCount = 0;
        ruleViolations = 0;

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

    public void RegisterRuleViolation()
    {
        if (sessionComplete || problemSolved) return;
        ruleViolations++;
    }

    public void RegisterMove()
    {
        if (sessionComplete || problemSolved) return;

        moveCount++;
        moveCounter.SetMoves(moveCount);

        if (thinkingTime.Instance != null)
        {
            CargoLogisticsScoring.LogMove(
                currentProblemIndex,
                ProblemLibrary.Sequence[currentProblemIndex].isPractice,
                moveCount,
                currentOptimalMoves,
                ruleViolations,
                thinkingTime.Instance.InitialThinkingSeconds,
                thinkingTime.Instance.LiveSubsequentThinkingSeconds);
        }

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

        if (thinkingTime.Instance != null) thinkingTime.Instance.OnTrialSolved();

        float initial = thinkingTime.Instance != null ? thinkingTime.Instance.InitialThinkingSeconds : 0f;
        float subsequent = thinkingTime.Instance != null ? thinkingTime.Instance.SubsequentThinkingSeconds : 0f;
        float animation = thinkingTime.Instance != null ? thinkingTime.Instance.AnimationSeconds : 0f;
        float total = thinkingTime.Instance != null ? thinkingTime.Instance.TotalSeconds : 0f;

        CargoLogisticsResults.Record(currentProblemIndex, problem.isPractice, moveCount, currentOptimalMoves,
                                     ruleViolations, initial, subsequent, animation, total);

        Sfx.Play(solvedSound, solvedVolume);

        Time.timeScale = 0f;

        if (isLastProblem)
        {
            CargoLogisticsScoring.ComputeFinalScores();

            ResultDisplay.Show(canvas,
                $"Solved in {moveCount} moves (optimal was {currentOptimalMoves}).\nAll problems complete!",
                finalPanelHeight, 32f);

            StartCoroutine(AdvanceAfterDelay(finalResultSeconds, true));
            return;
        }

        string message;
        if (problem.isPractice)
            message = "Practice complete! Starting the timed problems...";
        else
            message = $"Solved in {moveCount} moves (optimal was {currentOptimalMoves}).";

        ResultDisplay.Show(canvas, message);
        StartCoroutine(AdvanceAfterDelay(resultSeconds, false));
    }

    private IEnumerator AdvanceAfterDelay(float seconds, bool isLastProblem)
    {
        yield return new WaitForSecondsRealtime(seconds);
        Time.timeScale = 1f;
        ResultDisplay.Hide();

        if (isLastProblem)
        {
            sessionComplete = true;
            SceneTransition.LoadScene("Radar");
        }
        else
        {
            LoadProblem(currentProblemIndex + 1);
        }
    }
    private void ApplyPacingSettings()
    {
        CargoLogisticsScoring.ConfigurePacing(expectedTimeMultiplier, paceTolerance,
                                              initialThinkingWeight, subsequentThinkingWeight);
    }

    void OnValidate()
    {
        ApplyPacingSettings();
    }

    private void UpdateProgressDisplay()
    {
        if (progressText != null)
            progressText.text = $"{currentProblemIndex + 1}/{ProblemLibrary.Sequence.Count}";
    }
}