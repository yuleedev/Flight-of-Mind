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

    private void UpdateDisplay()
    {
        if (passText != null)
        {
            passText.text = "Passes: " + passes;
        }

        if (failText != null)
        {
            failText.text = "Fails: " + fails;
        }

        if (falsePositiveText != null)
        {
            falsePositiveText.text = "False Positives: " + falsePositives;
        }

        if (falseNegativeText != null)
        {
            falseNegativeText.text = "False Negatives: " + falseNegatives;
        }

        if (averageReactionTimeText != null)
        {
            if (reactionTimeCount == 0)
            {
                averageReactionTimeText.text = "Average Reaction: --";
            }
            else
            {
                float averageReactionTime =
                    totalReactionTime / reactionTimeCount;

                averageReactionTimeText.text =
                    "Average Reaction: " +
                    averageReactionTime.ToString("F3") +
                    " s";
            }
        }
    }
}