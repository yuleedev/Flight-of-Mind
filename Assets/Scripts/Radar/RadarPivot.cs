using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RadarPivot : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float tutorialRotationSpeed = 20f;
    [SerializeField] private float levelDuration = 150f;
    [SerializeField] private int tutorialSweeps = 2;
    [SerializeField] private float tutorialFeedbackDuration = 1.5f;
    [SerializeField] private float scoredStartMessageDuration = 2f;
    [SerializeField] private int countdownSeconds = 3;
    [Tooltip("Pause with the sweep line parked at the start before each scored sweep begins.")]
    [SerializeField] private float sweepStartDelay = 0.5f;
    [Tooltip("Longer version of the same pause during practice.")]
    [SerializeField] private float tutorialSweepStartDelay = 1.2f;

    [SerializeField] private GameObject instructionsPanel;
    [Tooltip("The readout block on the blue panel. Hidden until the run starts.")]
    [SerializeField] private GameObject panelHud;
    [Tooltip("Backing plate behind the instruction line at the top of the screen.")]
    [SerializeField] private GameObject tutorialBanner;

    [SerializeField] private TMP_Text phaseText;
    [SerializeField] private TMP_Text tutorialInstructionText;
    [SerializeField] private TMP_Text timerText;

    [SerializeField] private GameObject completionPanel;

    [SerializeField] private string resultsSceneName = "ResultsScreen";
    [SerializeField] private float resultsTransitionDelay = 0.75f;

    [SerializeField] private Image greenFlash;
    [SerializeField] private float flashDuration = 0.2f;
    [SerializeField, Range(0f, 1f)] private float flashOpacity = 0.35f;

    [SerializeField] private GameObject warningSymbol;
    [SerializeField] private SpriteRenderer warningTriangle;
    [SerializeField, Range(0f, 1f)] private float warningChance = 0.3f;
    [SerializeField] private float warningRadius = 2f;

    [SerializeField] private PassFailCounter passFailCounter;

    [Header("Scoring")]
    [SerializeField, Range(0f, 1f)] private float accuracyWeight = 0.60f;
    [SerializeField, Range(0f, 1f)] private float falsePositiveWeight = 0.8f;
    [SerializeField, Range(0f, 1f)] private float falseNegativeWeight = 0.5f;
    [SerializeField, Range(0f, 1f)] private float reactionTimeWeight = 0.10f;
    [SerializeField] private float excellentReactionTime = 0.35f;
    [SerializeField] private float poorReactionTime = 1.50f;

    public int FinalScore { get; private set; }
    public int ReactionTimeScore { get; private set; }

    private float degreesRotated;
    private float flashTimer;
    private float rotationStartTime;
    private float elapsedGameTime;
    private float transitionTimer;
    private float temporaryMessageTimer;

    private int tutorialSweepIndex;

    private bool gameStarted;
    private bool gameFinished;
    private bool tutorialActive;
    private bool scoredGameActive;
    private bool countdownActive;
    private bool pressedThisRotation;
    private bool warningActive;
    private bool waitingForNextRotation;
    private bool waitingToStartSweep;
    private float sweepStartTimer;

    private Color warningStartColor;

    private void Awake()
    {
        Time.timeScale = 1f;

        degreesRotated = 0f;
        flashTimer = 0f;
        rotationStartTime = 0f;
        elapsedGameTime = 0f;
        transitionTimer = 0f;
        temporaryMessageTimer = 0f;
        tutorialSweepIndex = 0;
        FinalScore = 0;
        ReactionTimeScore = 0;

        gameStarted = false;
        gameFinished = false;
        tutorialActive = false;
        scoredGameActive = false;
        countdownActive = false;
        pressedThisRotation = false;
        warningActive = false;
        waitingForNextRotation = false;
        waitingToStartSweep = false;

        if (warningTriangle != null)
        {
            warningStartColor = warningTriangle.color;
        }

        HideFlash();
        SetWarningVisible(false);
        SetPhaseText("READY");
        HideInstructionText();
        SetTimerVisible(false);
        SetPanelHudVisible(false);

        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(true);
        }

        if (completionPanel != null)
        {
            completionPanel.SetActive(false);
        }
    }

    public void StartGame()
    {
        if (gameStarted)
        {
            return;
        }

        gameStarted = true;
        gameFinished = false;
        tutorialActive = tutorialSweeps > 0;
        scoredGameActive = false;
        countdownActive = false;

        degreesRotated = 0f;
        flashTimer = 0f;
        rotationStartTime = 0f;
        elapsedGameTime = 0f;
        transitionTimer = 0f;
        temporaryMessageTimer = 0f;
        tutorialSweepIndex = 0;
        FinalScore = 0;
        ReactionTimeScore = 0;

        pressedThisRotation = false;
        warningActive = false;
        waitingForNextRotation = false;
        waitingToStartSweep = false;

        transform.localRotation = Quaternion.identity;

        if (passFailCounter != null)
        {
            passFailCounter.ResetCounter();
        }

        RadarWatchResults.Clear();

        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
        }

        if (completionPanel != null)
        {
            completionPanel.SetActive(false);
        }

        SceneMusic.StartGameMusic();

        SetPanelHudVisible(true);
        SetTimerVisible(true);
        UpdateTimerDisplay();

        if (tutorialActive)
        {
            StartNewRotation();
        }
        else
        {
            StartCoroutine(BeginScoredGameCountdown());
        }
    }

    private void Update()
    {
        if (!gameStarted || gameFinished)
        {
            return;
        }

        UpdateFlash();
        UpdateTemporaryMessage();

        if (countdownActive)
        {
            return;
        }

        if (scoredGameActive)
        {
            elapsedGameTime += Time.deltaTime;
            UpdateTimerDisplay();

            if (elapsedGameTime >= levelDuration)
            {
                FinishGame();
                return;
            }
        }

        if (!tutorialActive && !scoredGameActive)
        {
            return;
        }

        if (waitingForNextRotation)
        {
            transitionTimer -= Time.deltaTime;

            if (transitionTimer <= 0f)
            {
                waitingForNextRotation = false;

                if (tutorialActive &&
                    tutorialSweepIndex >= tutorialSweeps)
                {
                    StartCoroutine(BeginScoredGameCountdown());
                }
                else
                {
                    StartNewRotation();
                }
            }

            return;
        }

        if (waitingToStartSweep)
        {
            sweepStartTimer -= Time.deltaTime;

            if (sweepStartTimer <= 0f)
            {
                BeginSweep();
            }

            return;
        }

        float currentRotationSpeed =
            tutorialActive
                ? tutorialRotationSpeed
                : rotationSpeed;

        float rotationAmount =
            currentRotationSpeed * Time.deltaTime;

        transform.Rotate(0f, 0f, -rotationAmount);
        degreesRotated += rotationAmount;

        CheckForPlayerInput();

        if (degreesRotated >= 360f)
        {
            degreesRotated -= 360f;
            FinishRotation();
        }
    }

    private void CheckForPlayerInput()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (!Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            return;
        }

        if (pressedThisRotation)
        {
            return;
        }

        pressedThisRotation = true;

        if (tutorialActive)
        {
            HandleTutorialInput();
            return;
        }

        if (warningActive)
        {
            if (warningTriangle != null)
            {
                warningTriangle.color = Color.red;
            }

            FlashScreen(Color.red);

            if (passFailCounter != null)
            {
                passFailCounter.AddFalsePositive();
            }
        }
        else
        {
            float reactionTime =
                Time.time - rotationStartTime;

            FlashScreen(Color.green);

            if (passFailCounter != null)
            {
                passFailCounter.AddPass(reactionTime);
            }
        }
    }

    private void HandleTutorialInput()
    {
        if (warningActive)
        {
            if (warningTriangle != null)
            {
                warningTriangle.color = Color.red;
            }

            FlashScreen(Color.red);

            SetInstructionText(
                "<b>Not that one.</b> A warning was showing, so hold off."
            );
        }
        else
        {
            FlashScreen(Color.green);

            SetInstructionText(
                "<b>Correct.</b> Clear sweep, pressed in time."
            );
        }
    }

    private void FinishRotation()
    {
        if (tutorialActive)
        {
            FinishTutorialRotation();
            return;
        }

        if (pressedThisRotation)
        {
            LogReactionTimeScore();
            StartNewRotation();
            return;
        }

        if (warningActive)
        {
            if (warningTriangle != null)
            {
                warningTriangle.color = Color.green;
            }

            FlashScreen(Color.green);

            if (passFailCounter != null)
            {
                passFailCounter.AddPass();
            }
        }
        else
        {
            FlashScreen(Color.red);

            if (passFailCounter != null)
            {
                passFailCounter.AddFalseNegative();
            }
        }

        SetWarningVisible(false);

        LogReactionTimeScore();

        waitingForNextRotation = true;
        transitionTimer = flashDuration;
    }

    private void LogReactionTimeScore()
    {
        ReactionTimeScore = Mathf.RoundToInt(CalculateReactionTimeScore());
        RadarWatchResults.SetReactionTimeScore(ReactionTimeScore);

        Debug.Log("Radar Watch reaction time score: " + ReactionTimeScore + "/100");
    }

    private void FinishTutorialRotation()
    {
        bool correctResponse =
            warningActive
                ? !pressedThisRotation
                : pressedThisRotation;

        bool alreadyShownFeedback = pressedThisRotation;

        if (correctResponse)
        {
            if (warningActive &&
                warningTriangle != null)
            {
                warningTriangle.color = Color.green;
            }

            if (!alreadyShownFeedback)
            {
                FlashScreen(Color.green);
            }

            if (warningActive)
            {
                SetInstructionText(
                    "<b>Correct.</b> Warning showing, so you held off."
                );
            }
            else
            {
                SetInstructionText(
                    "<b>Correct.</b> Clear sweep, pressed in time."
                );
            }

            SetWarningVisible(false);

            tutorialSweepIndex++;
            waitingForNextRotation = true;
            transitionTimer = tutorialFeedbackDuration;
        }
        else
        {
            if (warningActive &&
                warningTriangle != null)
            {
                warningTriangle.color = Color.red;
            }

            if (!alreadyShownFeedback)
            {
                FlashScreen(Color.red);
            }

            if (warningActive)
            {
                SetInstructionText(
                    "<b>Not that one.</b> That sweep had a warning. Practice restarts."
                );
            }
            else
            {
                SetInstructionText(
                    "<b>Too slow.</b> That sweep was clear. Practice restarts."
                );
            }

            SetWarningVisible(false);

            tutorialSweepIndex = 0;

            if (passFailCounter != null)
            {
                passFailCounter.ResetCounter();
            }

            waitingForNextRotation = true;
            transitionTimer = tutorialFeedbackDuration;
        }
    }

    private void StartNewRotation()
    {
        pressedThisRotation = false;
        degreesRotated = 0f;
        transform.localRotation = Quaternion.identity;

        UpdateTimerDisplay();
        SetWarningVisible(false);

        if (warningTriangle != null)
        {
            warningTriangle.color = warningStartColor;
        }

        if (tutorialActive)
        {
            warningActive = tutorialSweepIndex % 2 == 1;

            SetPhaseText(
                "PRACTICE " +
                (tutorialSweepIndex + 1) +
                "/" +
                tutorialSweeps
            );

            SetInstructionText("<b>Get ready.</b> Watch where the sweep starts.");
        }
        else
        {
            warningActive =
                Random.value < warningChance;

            SetPhaseText("RADAR WATCH");
        }

        float delay =
            tutorialActive
                ? tutorialSweepStartDelay
                : sweepStartDelay;

        if (delay > 0f)
        {
            waitingToStartSweep = true;
            sweepStartTimer = delay;
            return;
        }

        BeginSweep();
    }

    private void BeginSweep()
    {
        waitingToStartSweep = false;
        rotationStartTime = Time.time;

        if (tutorialActive)
        {
            if (warningActive)
            {
                SetInstructionText(
                    "<b>Warning showing.</b> Do not press."
                );
            }
            else
            {
                SetInstructionText(
                    "<b>Clear sweep.</b> Press SPACE before it comes around."
                );
            }
        }

        SetWarningVisible(warningActive);

        if (!warningActive)
        {
            return;
        }

        if (warningSymbol != null)
        {
            Vector2 randomPosition =
                Random.insideUnitCircle *
                warningRadius;

            Vector3 currentPosition =
                warningSymbol.transform.localPosition;

            warningSymbol.transform.localPosition =
                new Vector3(
                    randomPosition.x,
                    randomPosition.y,
                    currentPosition.z
                );
        }
    }

    private IEnumerator BeginScoredGameCountdown()
    {
        countdownActive = true;
        tutorialActive = false;
        scoredGameActive = false;
        waitingForNextRotation = false;
        warningActive = false;
        pressedThisRotation = false;
        degreesRotated = 0f;

        transform.localRotation = Quaternion.identity;

        SetWarningVisible(false);
        HideFlash();
        SetPhaseText("GET READY");
        UpdateTimerDisplay();

        for (int number = countdownSeconds; number >= 1; number--)
        {
            SetInstructionText(
                "<b>Practice done.</b> Real run starts in " +
                number
            );

            yield return new WaitForSeconds(1f);
        }

        countdownActive = false;
        BeginScoredGame();
    }

    private void BeginScoredGame()
    {
        tutorialActive = false;
        scoredGameActive = true;
        countdownActive = false;

        elapsedGameTime = 0f;
        degreesRotated = 0f;
        pressedThisRotation = false;
        warningActive = false;
        waitingForNextRotation = false;
        waitingToStartSweep = false;
        transitionTimer = 0f;

        transform.localRotation = Quaternion.identity;

        SetWarningVisible(false);
        SetPhaseText("RADAR WATCH");
        SetInstructionText("<b>Go.</b> Press on clear sweeps, hold off when a warning shows.");

        temporaryMessageTimer =
            scoredStartMessageDuration;

        UpdateTimerDisplay();
        StartNewRotation();
    }

    private void FinishGame()
    {
        gameFinished = true;
        gameStarted = false;
        tutorialActive = false;
        scoredGameActive = false;
        countdownActive = false;
        waitingForNextRotation = false;
        elapsedGameTime = levelDuration;

        SetWarningVisible(false);
        HideFlash();
        HideInstructionText();
        SetPhaseText("COMPLETE");
        UpdateTimerDisplay();

        if (completionPanel != null)
        {
            completionPanel.SetActive(false);
        }

        if (passFailCounter != null)
        {
            FinalScore = CalculateFinalScore();

            RadarWatchResults.Record(
                FinalScore,
                passFailCounter.Passes,
                passFailCounter.Fails,
                passFailCounter.FalsePositives,
                passFailCounter.FalseNegatives,
                passFailCounter.HasReactionTime,
                passFailCounter.AverageReactionTime
            );
        }

        StartCoroutine(LoadResultsScreen());
    }

    private IEnumerator LoadResultsScreen()
    {
        if (resultsTransitionDelay > 0f)
        {
            yield return new WaitForSeconds(resultsTransitionDelay);
        }

        SceneTransition.LoadScene(resultsSceneName);
    }

    private int CalculateFinalScore()
    {
        if (passFailCounter == null)
        {
            return 0;
        }

        int totalRounds =
            passFailCounter.Passes +
            passFailCounter.Fails;

        if (totalRounds <= 0)
        {
            return 0;
        }

        float accuracyScore =
            (float)passFailCounter.Passes /
            totalRounds * 100f;

        float falsePositiveScore =
            100f -
            ((float)passFailCounter.FalsePositives /
            totalRounds * 100f);

        float falseNegativeScore =
            100f -
            ((float)passFailCounter.FalseNegatives /
            totalRounds * 100f);

        float reactionScore = CalculateReactionTimeScore();

        float totalWeight =
            accuracyWeight +
            falsePositiveWeight +
            falseNegativeWeight +
            reactionTimeWeight;

        if (totalWeight <= 0f)
        {
            return 0;
        }

        float weightedScore =
            (accuracyScore * accuracyWeight +
            falsePositiveScore * falsePositiveWeight +
            falseNegativeScore * falseNegativeWeight +
            reactionScore * reactionTimeWeight) /
            totalWeight;

        return Mathf.RoundToInt(
            Mathf.Clamp(weightedScore, 0f, 100f)
        );
    }

    private float CalculateReactionTimeScore()
    {
        if (passFailCounter == null || !passFailCounter.HasReactionTime)
        {
            return 0f;
        }

        float reactionRange =
            Mathf.Max(
                0.01f,
                poorReactionTime - excellentReactionTime
            );

        return Mathf.Clamp01(
            (poorReactionTime -
            passFailCounter.AverageReactionTime) /
            reactionRange
        ) * 100f;
    }

    private void UpdateTemporaryMessage()
    {
        if (!scoredGameActive ||
            temporaryMessageTimer <= 0f)
        {
            return;
        }

        temporaryMessageTimer -=
            Time.deltaTime;

        if (temporaryMessageTimer <= 0f)
        {
            HideInstructionText();
        }
    }

    private void SetWarningVisible(bool visible)
    {
        if (warningSymbol != null)
        {
            warningSymbol.SetActive(visible);
        }
    }

    private void SetPhaseText(string message)
    {
        if (phaseText != null)
        {
            phaseText.text = message;
        }
    }

    private void SetInstructionText(string message)
    {
        if (tutorialInstructionText == null)
        {
            return;
        }

        tutorialInstructionText.gameObject.SetActive(true);
        tutorialInstructionText.text = message;

        if (tutorialBanner != null)
        {
            tutorialBanner.SetActive(true);
        }
    }

    private void HideInstructionText()
    {
        if (tutorialInstructionText != null)
        {
            tutorialInstructionText.gameObject.SetActive(false);
        }

        if (tutorialBanner != null)
        {
            tutorialBanner.SetActive(false);
        }
    }

    private void SetPanelHudVisible(bool visible)
    {
        if (panelHud != null)
        {
            panelHud.SetActive(visible);
        }
    }

    private void SetTimerVisible(bool visible)
    {
        if (timerText != null)
        {
            timerText.gameObject.SetActive(visible);
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null)
        {
            return;
        }

        if (tutorialActive)
        {
            int currentSweep =
                Mathf.Min(
                    tutorialSweepIndex + 1,
                    tutorialSweeps
                );

            timerText.text =
                "<b>Practice</b> " +
                currentSweep +
                " of " +
                tutorialSweeps;

            return;
        }

        float remainingTime =
            Mathf.Max(
                0f,
                levelDuration - elapsedGameTime
            );

        int totalSeconds =
            Mathf.CeilToInt(remainingTime);

        int minutes =
            totalSeconds / 60;

        int seconds =
            totalSeconds % 60;

        timerText.text =
            "<b>Time left</b> " +
            minutes +
            ":" +
            seconds.ToString("00");
    }


    private void UpdateFlash()
    {
        if (flashTimer <= 0f)
        {
            return;
        }

        flashTimer -= Time.deltaTime;

        if (flashTimer <= 0f)
        {
            flashTimer = 0f;
            HideFlash();
        }
    }

    private void FlashScreen(Color flashColor)
    {
        if (passFailCounter != null)
        {
            if (flashColor == Color.red)
            {
                passFailCounter.PlayIncorrect();
            }
            else if (flashColor == Color.green)
            {
                passFailCounter.PlayCorrect();
            }
        }

        if (greenFlash == null)
        {
            return;
        }

        flashColor.a = flashOpacity;
        greenFlash.color = flashColor;
        flashTimer = flashDuration;
    }

    private void HideFlash()
    {
        if (greenFlash == null)
        {
            return;
        }

        Color color = greenFlash.color;
        color.a = 0f;
        greenFlash.color = color;
    }
}