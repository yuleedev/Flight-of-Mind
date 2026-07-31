using TMPro;
using UnityEngine;

public class PassFailCounter : MonoBehaviour
{
    [SerializeField] private TMP_Text passText;
    [SerializeField] private TMP_Text failText;
    [SerializeField] private TMP_Text falsePositiveText;
    [SerializeField] private TMP_Text falseNegativeText;
    [SerializeField] private TMP_Text averageReactionTimeText;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip incorrectSound;

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
        PlaySound(correctSound);
        UpdateDisplay();
    }

    public void AddPass(float reactionTime)
    {
        passes++;
        totalReactionTime += reactionTime;
        reactionTimeCount++;
        PlaySound(correctSound);
        UpdateDisplay();
    }

    public void AddFalsePositive()
    {
        fails++;
        falsePositives++;
        UpdateDisplay();
    }

    public void PlayIncorrect()
    {
        PlaySound(incorrectSound);
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

    private void PlaySound(AudioClip clip)
    {
        Sfx.Play(clip);
    }

    private static string Row(string label, string value)
    {
        return "<b>" + label + ":</b> " + value;
    }

    private void UpdateDisplay()
    {
        if (passText != null)
            passText.text = Row("Correct", passes.ToString());

        if (failText != null)
            failText.text = Row("Total errors", fails.ToString());

        if (falsePositiveText != null)
            falsePositiveText.text = Row("Wrong press", falsePositives.ToString());

        if (falseNegativeText != null)
            falseNegativeText.text = Row("Missed sweep", falseNegatives.ToString());

        if (averageReactionTimeText != null)
        {
            averageReactionTimeText.text = Row("Reaction",
                HasReactionTime ? AverageReactionTime.ToString("F2") + " s" : "--");
        }
    }
}