using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultsScreenUI : MonoBehaviour
{
    public TMP_Text landingRoutesText;
    public TMP_Text cargoLogisticsText;
    public TMP_Text radarWatchText;
    public bool showScoreDials = true;
    public string continueScene = "Main Menu";

    struct DialPair
    {
        public ScoreDial first;
        public ScoreDial second;

        public bool Ready => first != null && second != null;
    }

    DialPair landingDials;
    DialPair cargoDials;
    DialPair radarDials;
    bool dialsBuilt;

    void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        BuildDials();

        if (landingRoutesText != null)
            landingRoutesText.text = BuildLandingRoutesText();

        if (cargoLogisticsText != null)
            cargoLogisticsText.text = BuildCargoLogisticsText();

        if (radarWatchText != null)
            radarWatchText.text = BuildRadarWatchText();

        RefreshLandingRoutesDials();
        RefreshCargoLogisticsDials();
        RefreshRadarWatchDials();
    }

    void BuildDials()
    {
        if (dialsBuilt || !showScoreDials)
            return;

        dialsBuilt = true;

        landingDials = BuildDialPair(landingRoutesText);
        cargoDials = BuildDialPair(cargoLogisticsText);
        radarDials = BuildDialPair(radarWatchText);
    }

    DialPair BuildDialPair(TMP_Text bodyText)
    {
        DialPair pair = new DialPair();

        if (bodyText == null)
            return pair;

        RectTransform section = bodyText.transform.parent as RectTransform;
        if (section == null)
            return pair;

        GameObject row = new GameObject("ScoreDials", typeof(RectTransform));
        row.layer = section.gameObject.layer;

        RectTransform rowRect = row.GetComponent<RectTransform>();
        rowRect.SetParent(section, false);
        rowRect.SetSiblingIndex(Mathf.Min(1, section.childCount - 1));

        VerticalLayoutGroup layout = row.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 14;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        float rowHeight = ScoreDial.CardHeight * 2f + layout.spacing;

        LayoutElement rowLayout = row.AddComponent<LayoutElement>();
        rowLayout.preferredHeight = rowHeight;
        rowLayout.minHeight = rowHeight;

        pair.first = ScoreDial.Create(rowRect, bodyText.font);
        pair.second = ScoreDial.Create(rowRect, bodyText.font);
        return pair;
    }

    void RefreshLandingRoutesDials()
    {
        if (!landingDials.Ready)
            return;

        TrailMakingScores scores = TrailMakingScoring.Compute();
        TrailMakingResult a = TrailMakingResults.Get("A");

        if (scores.hasFlexibilityScore)
        {
            landingDials.first.SetScore(
                scores.cognitiveFlexibilityScore,
                "Cognitive Flexibility",
                "part B took " + scores.switchCostRatio.ToString("F2") + "x part A",
                true);
        }
        else
        {
            landingDials.first.SetUnavailable("Cognitive Flexibility", "part B not completed");
        }

        if (scores.hasSpeedScore && a != null)
        {
            landingDials.second.SetScore(
                scores.processingSpeedScore,
                "Processing Speed",
                a.timeSeconds.ToString("F1") + " s across " + TrailMakingScoring.TargetsPerRoute + " targets",
                true);
        }
        else
        {
            landingDials.second.SetUnavailable("Processing Speed", "part A not completed");
        }
    }

    void RefreshCargoLogisticsDials()
    {
        if (!cargoDials.Ready)
            return;

        if (!CargoLogisticsScoring.FinalScoresReady)
        {
            cargoDials.first.SetUnavailable("Logical Reasoning", "not completed");
            cargoDials.second.SetUnavailable("Thinking Time", "not completed");
            return;
        }

        SummariseCargoTrials(out int problems, out int excessMoves, out int violations, out float thinking);

        string moveWord = excessMoves == 1 ? " extra move" : " extra moves";

        cargoDials.first.SetScore(
            CargoLogisticsScoring.logicalReasoningScore,
            "Logical Reasoning",
            problems + " problems  ·  " + excessMoves + moveWord,
            true);

        cargoDials.second.SetScore(
            CargoLogisticsScoring.thinkingTimeScore,
            "Thinking Time",
            thinking.ToString("F1") + " s planning per problem",
            true);
    }

    void RefreshRadarWatchDials()
    {
        if (!radarDials.Ready)
            return;

        if (!RadarWatchResults.HasResults)
        {
            radarDials.first.SetUnavailable("Watch Accuracy", "not completed");
            radarDials.second.SetUnavailable("Reaction Time", "not completed");
            return;
        }

        int sweeps = RadarWatchResults.Passes + RadarWatchResults.Fails;

        radarDials.first.SetScore(
            RadarWatchResults.FinalScore,
            "Watch Accuracy",
            RadarWatchResults.Passes + " of " + sweeps + " sweeps correct",
            true);

        radarDials.second.SetScore(
            RadarWatchResults.ReactionTimeScore,
            "Reaction Time",
            RadarWatchResults.HasReactionTime
                ? RadarWatchResults.AverageReactionTime.ToString("F3") + " s average"
                : "no timed presses",
            true);
    }

    static void SummariseCargoTrials(out int problems, out int excessMoves, out int violations,
                                     out float thinkingPerProblem)
    {
        problems = 0;
        excessMoves = 0;
        violations = 0;

        float thinkingTotal = 0f;

        foreach (var t in CargoLogisticsResults.Trials)
        {
            if (t.isPractice) continue;

            problems++;
            excessMoves += t.excessMoves;
            violations += t.ruleViolations;
            thinkingTotal += t.initialThinkingSeconds + t.subsequentThinkingSeconds;
        }

        thinkingPerProblem = problems > 0 ? thinkingTotal / problems : 0f;
    }

    static string Row(string label, string value)
    {
        return "<b>" + label + ":</b> " + value;
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
            sb.AppendLine(Row("B - A", difference.ToString("F1") + " s"));
            sb.Append(Row("B / A", ratio.ToString("F2") + " s"));
        }

        return sb.ToString();
    }

    string FormatPart(string label, TrailMakingResult result)
    {
        if (result == null)
            return Row(label, "not completed");

        string errorWord = result.errors == 1 ? " error" : " errors";
        return Row(label, result.timeSeconds.ToString("F1") + " s, " + result.errors + errorWord);
    }

    string BuildCargoLogisticsText()
    {
        if (!CargoLogisticsScoring.FinalScoresReady)
            return "No results recorded this session.";

        SummariseCargoTrials(out int problems, out int excessMoves, out int violations, out float thinking);

        StringBuilder sb = new StringBuilder();

        sb.AppendLine(Row("Problems", problems.ToString()));
        sb.AppendLine(Row("Extra moves", excessMoves.ToString()));
        sb.AppendLine(Row("Rule slips", violations.ToString()));
        sb.Append(Row("Planning", thinking.ToString("F1") + " s per problem"));

        return sb.ToString();
    }

    string BuildRadarWatchText()
    {
        if (!RadarWatchResults.HasResults)
            return "No results recorded this session.";

        string averageReaction = RadarWatchResults.HasReactionTime
            ? RadarWatchResults.AverageReactionTime.ToString("F3") + " s"
            : "--";

        StringBuilder sb = new StringBuilder();

        sb.AppendLine(Row("Correct", RadarWatchResults.Passes.ToString()));
        sb.AppendLine(Row("Incorrect", RadarWatchResults.Fails.ToString()));
        sb.AppendLine(Row("False alarms", RadarWatchResults.FalsePositives.ToString()));
        sb.AppendLine(Row("Missed presses", RadarWatchResults.FalseNegatives.ToString()));
        sb.Append(Row("Average reaction", averageReaction));

        return sb.ToString();
    }

    public void OnContinueClicked()
    {
        SceneManager.LoadScene(continueScene);
    }
}
