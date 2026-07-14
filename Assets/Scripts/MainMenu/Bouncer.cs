using UnityEngine;

public class Bouncer : MonoBehaviour
{
    public float bounceHeight = 50f;
    public float speed = 5f;

    private RectTransform rectTransform;
    private Vector2 startPos;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        startPos = rectTransform.anchoredPosition;
    }

    private void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * speed) * bounceHeight;
        rectTransform.anchoredPosition = new Vector2(startPos.x, newY);
    }
}
