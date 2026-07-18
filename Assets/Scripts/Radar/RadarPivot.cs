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

    [SerializeField] private GameObject instructionsPanel;

    [SerializeField] private TMP_Text phaseText;
    [SerializeField] private TMP_Text tutorialInstructionText;
    [SerializeField] private TMP_Text timerText;

    [SerializeField] private GameObject completionPanel;
    [SerializeField] private TMP_Text completionText;

    [SerializeField] private Image greenFlash;
    [SerializeField] private float flashDuration = 0.2f;
    [SerializeField, Range(0f, 1f)] private float flashOpacity = 0.35f;

    [SerializeField] private GameObject warningSymbol;
    [SerializeField] private SpriteRenderer warningTriangle;
    [SerializeField, Range(0f, 1f)] private float warningChance = 0.3f;
    [SerializeField] private float warningRadius = 2f;

    [SerializeField] private PassFailCounter passFailCounter;

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

        gameStarted = false;
        gameFinished = false;
        tutorialActive = false;
        scoredGameActive = false;
        countdownActive = false;
        pressedThisRotation = false;
        warningActive = false;
        waitingForNextRotation = false;

        if (warningTriangle != null)
        {
            warningStartColor = warningTriangle.color;
        }

        HideFlash();
        SetWarningVisible(false);
        SetPhaseText("READY");
        HideInstructionText();
        UpdateTimerDisplay();

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

        pressedThisRotation = false;
        warningActive = false;
        waitingForNextRotation = false;

        transform.localRotation = Quaternion.identity;

        if (passFailCounter != null)
        {
            passFailCounter.ResetCounter();
        }

        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
        }

        if (completionPanel != null)
        {
            completionPanel.SetActive(false);
        }

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
                "TUTORIAL: Incorrect\nDo not press SPACE when there is a warning."
            );
        }
        else
        {
            FlashScreen(Color.green);

            SetInstructionText(
                "TUTORIAL: Correct\nYou pressed SPACE with no warning."
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

        waitingForNextRotation = true;
        transitionTimer = flashDuration;
    }

    private void FinishTutorialRotation()
    {
        bool correctResponse =
            warningActive
                ? !pressedThisRotation
                : pressedThisRotation;

        if (correctResponse)
        {
            if (warningActive &&
                warningTriangle != null)
            {
                warningTriangle.color = Color.green;
            }

            FlashScreen(Color.green);

            if (warningActive)
            {
                SetInstructionText(
                    "TUTORIAL: Correct\nYou did not press SPACE."
                );
            }
            else
            {
                SetInstructionText(
                    "TUTORIAL: Correct\nYou pressed SPACE."
                );
            }
        }
        else
        {
            if (warningActive &&
                warningTriangle != null)
            {
                warningTriangle.color = Color.red;
            }

            FlashScreen(Color.red);

            if (warningActive)
            {
                SetInstructionText(
                    "TUTORIAL: Incorrect\nDo not press SPACE when there is a warning."
                );
            }
            else
            {
                SetInstructionText(
                    "TUTORIAL: Missed\nPress SPACE when there is no warning."
                );
            }
        }

        SetWarningVisible(false);

        tutorialSweepIndex++;
        waitingForNextRotation = true;
        transitionTimer = tutorialFeedbackDuration;
    }

    private void StartNewRotation()
    {
        pressedThisRotation = false;
        rotationStartTime = Time.time;

        if (warningTriangle != null)
        {
            warningTriangle.color = warningStartColor;
        }

        if (tutorialActive)
        {
            warningActive = tutorialSweepIndex % 2 == 1;

            SetPhaseText(
                "TUTORIAL " +
                (tutorialSweepIndex + 1) +
                "/" +
                tutorialSweeps
            );

            if (warningActive)
            {
                SetInstructionText(
                    "TUTORIAL: do not press SPACE (Warning)"
                );
            }
            else
            {
                SetInstructionText(
                    "TUTORIAL: press SPACE (no warning)"
                );
            }
        }
        else
        {
            warningActive =
                Random.value < warningChance;

            SetPhaseText("RADAR WATCH");
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

        for (int number = countdownSeconds; number >= 1; number--)
        {
            SetInstructionText(
                "Ready? The real game will start in " +
                countdownSeconds +
                " seconds.\n" +
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
        transitionTimer = 0f;

        transform.localRotation = Quaternion.identity;

        SetWarningVisible(false);
        SetPhaseText("RADAR WATCH");
        SetInstructionText("Radar Watch begins (Fast)");

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
            completionPanel.SetActive(true);
        }

        if (completionText == null)
        {
            return;
        }

        if (passFailCounter == null)
        {
            completionText.text =
                "Radar Watch Complete";

            return;
        }

        string averageReaction =
            passFailCounter.HasReactionTime
                ? passFailCounter
                    .AverageReactionTime
                    .ToString("F3") + " s"
                : "--";

        completionText.text =
            "Radar Watch Complete\n\n" +
            "Passes: " +
            passFailCounter.Passes +
            "\nFails: " +
            passFailCounter.Fails +
            "\nFalse Positives: " +
            passFailCounter.FalsePositives +
            "\nFalse Negatives: " +
            passFailCounter.FalseNegatives +
            "\nAverage Reaction: " +
            averageReaction;
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
    }

    private void HideInstructionText()
    {
        if (tutorialInstructionText != null)
        {
            tutorialInstructionText.gameObject.SetActive(false);
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null)
        {
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