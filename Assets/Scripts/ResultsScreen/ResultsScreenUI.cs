using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultsScreenUI : MonoBehaviour
{
    public TMP_Text landingRoutesText;
    public string continueScene = "Main Menu";

    void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (landingRoutesText != null)
            landingRoutesText.text = BuildLandingRoutesText();
    }

    string BuildLandingRoutesText()
    {
        TrailMakingResult a = TrailMakingResults.Get("A");
        TrailMakingResult b = TrailMakingResults.Get("B");

        if (a == null && b == null)
            return "No results recorded this session.";

        StringBuilder sb = new StringBuilder();

        sb.AppendLine(FormatPart("Part A", a));
        sb.AppendLine(FormatPart("Part B", b));

        if (a != null && b != null)
        {
            float difference = b.timeSeconds - a.timeSeconds;
            float ratio = a.timeSeconds > 0f ? b.timeSeconds / a.timeSeconds : 0f;

            sb.AppendLine();
            sb.AppendLine("B - A     " + difference.ToString("F1") + " s");
            sb.Append("B / A     " + ratio.ToString("F2"));
        }

        return sb.ToString();
    }

    string FormatPart(string label, TrailMakingResult result)
    {
        if (result == null)
            return label + "     not completed";

        string errorWord = result.errors == 1 ? " error" : " errors";
        return label + "     " + result.timeSeconds.ToString("F1") + " s     " + result.errors + errorWord;
    }

    public void OnContinueClicked()
    {
        SceneManager.LoadScene(continueScene);
    }
}
