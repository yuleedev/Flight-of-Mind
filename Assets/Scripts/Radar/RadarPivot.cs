using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class RadarPivot : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 90f;

    [SerializeField] private Image greenFlash;
    [SerializeField] private GameObject warningSymbol;
    [SerializeField] private SpriteRenderer warningTriangle;

    [SerializeField] private PassFailCounter passFailCounter;

    [SerializeField] private float flashDuration = 0.2f;
    [SerializeField, Range(0f, 1f)] private float flashOpacity = 0.35f;

    [SerializeField, Range(0f, 1f)] private float warningChance = 0.3f;
    [SerializeField] private float warningRadius = 2f;

    private float degreesRotated;
    private float flashTimer;

    private bool pressedThisRotation;
    private bool warningActive;
    private bool waitingForNextRotation;

    private Color warningStartColor;

    private void Start()
    {
        warningStartColor = warningTriangle.color;

        Color color = greenFlash.color;
        color.a = 0f;
        greenFlash.color = color;

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
            // The player pressed when they should not have pressed.
            warningTriangle.color = Color.red;
            FlashScreen(Color.red);
            passFailCounter.AddFail();
        }
        else
        {
            FlashScreen(Color.green);
            passFailCounter.AddPass();
        }
    }

    private void FinishRotation()
    {
        if (!pressedThisRotation)
        {
            if (warningActive)
            {
                warningTriangle.color = Color.green;
                FlashScreen(Color.green);
                passFailCounter.AddPass();
            }
            else
            {
                FlashScreen(Color.red);
                passFailCounter.AddFail();
            }

            waitingForNextRotation = true;
        }
        else
        {
            StartNewRotation();
        }
    }

    private void StartNewRotation()
    {
        pressedThisRotation = false;
        warningActive = Random.value < warningChance;

        warningSymbol.SetActive(warningActive);

        if (warningActive)
        {
            warningTriangle.color = warningStartColor;

            Vector2 randomPosition =
                Random.insideUnitCircle * warningRadius;

            warningSymbol.transform.localPosition =
                new Vector3(
                    randomPosition.x,
                    randomPosition.y,
                    warningSymbol.transform.localPosition.z
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
            Color color = greenFlash.color;
            color.a = 0f;
            greenFlash.color = color;
        }
    }

    private void FlashScreen(Color flashColor)
    {
        flashColor.a = flashOpacity;
        greenFlash.color = flashColor;
        flashTimer = flashDuration;
    }
}