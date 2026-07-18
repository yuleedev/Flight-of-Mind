using TMPro;
using UnityEngine;

public class PassFailCounter : MonoBehaviour
{
    [SerializeField] private TMP_Text passText;
    [SerializeField] private TMP_Text failText;
    [SerializeField] private TMP_Text falsePositiveText;
    [SerializeField] private TMP_Text falseNegativeText;
    [SerializeField] private TMP_Text averageReactionTimeText;

    private int passes;
    private int fails;
    private int falsePositives;
    private int falseNegatives;

    private float totalReactionTime;
    private int reactionTimeCount;

    public int Passes => passes;
    public int Fails => fails;
    public int FalsePositives => falsePositives;
    public int FalseNegatives => falseNegatives;

    public bool HasReactionTime =>
        reactionTimeCount > 0;

    public float AverageReactionTime =>
        reactionTimeCount > 0
            ? totalReactionTime /
              reactionTimeCount
            : 0f;

    private void Start()
    {
        UpdateDisplay();
    }

    public void AddPass()
    {
        passes++;
        UpdateDisplay();
    }

    public void AddPass(float reactionTime)
    {
        passes++;
        totalReactionTime += reactionTime;
        reactionTimeCount++;
        UpdateDisplay();
    }

    public void AddFalsePositive()
    {
        fails++;
        falsePositives++;
        UpdateDisplay();
    }

    public void AddFalseNegative()
    {
        fails++;
        falseNegatives++;
        UpdateDisplay();
    }

    public void ResetCounter()
    {
        passes = 0;
        fails = 0;
        falsePositives = 0;
        falseNegatives = 0;
        totalReactionTime = 0f;
        reactionTimeCount = 0;

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (passText != null)
        {
            passText.text =
                "Passes: " + passes;
        }

        if (failText != null)
        {
            failText.text =
                "Fails: " + fails;
        }

        if (falsePositiveText != null)
        {
            falsePositiveText.text =
                "False Positives: " +
                falsePositives;
        }

        if (falseNegativeText != null)
        {
            falseNegativeText.text =
                "False Negatives: " +
                falseNegatives;
        }

        if (averageReactionTimeText != null)
        {
            if (!HasReactionTime)
            {
                averageReactionTimeText.text =
                    "Average Reaction: --";
            }
            else
            {
                averageReactionTimeText.text =
                    "Average Reaction: " +
                    AverageReactionTime
                        .ToString("F3") +
                    " s";
            }
        }
    }
}