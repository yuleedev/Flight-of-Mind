using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class RadarPivot : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 90f;

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

        pressedThisRotation = false;
        warningActive = false;
        waitingForNextRotation = false;

        if (warningTriangle != null)
        {
            warningStartColor = warningTriangle.color;
        }

        HideFlash();

        if (warningSymbol != null)
        {
            warningSymbol.SetActive(false);
        }
    }

    private void Start()
    {
        StartNewRotation();
    }

    private void Update()
    {
        UpdateFlash();

        if (waitingForNextRotation)
        {
            if (flashTimer <= 0f)
            {
                waitingForNextRotation = false;
                StartNewRotation();
            }

            return;
        }

        float rotationAmount = rotationSpeed * Time.deltaTime;

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
            float reactionTime = Time.time - rotationStartTime;

            FlashScreen(Color.green);

            if (passFailCounter != null)
            {
                passFailCounter.AddPass(reactionTime);
            }
        }
    }

    private void FinishRotation()
    {
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

        waitingForNextRotation = true;
    }

    private void StartNewRotation()
    {
        pressedThisRotation = false;
        warningActive = Random.value < warningChance;
        rotationStartTime = Time.time;

        if (warningSymbol != null)
        {
            warningSymbol.SetActive(warningActive);
        }

        if (!warningActive)
        {
            return;
        }

        if (warningTriangle != null)
        {
            warningTriangle.color = warningStartColor;
        }

        if (warningSymbol != null)
        {
            Vector2 randomPosition =
                Random.insideUnitCircle * warningRadius;

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