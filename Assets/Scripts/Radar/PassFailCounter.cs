using TMPro;
using UnityEngine;

public class PassFailCounter : MonoBehaviour
{
    [SerializeField] private TMP_Text passText;
    [SerializeField] private TMP_Text failText;

    private int passes;
    private int fails;

    private void Start()
    {
        UpdateDisplay();
    }

    public void AddPass()
    {
        passes++;
        UpdateDisplay();
    }

    public void AddFail()
    {
        fails++;
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
    }
}