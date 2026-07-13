using UnityEngine;
using TMPro;

public class Waypoint : MonoBehaviour
{
    public TMP_Text label;
    public SpriteRenderer icon;
    public Color normalColor = Color.white;
    public Color visitedColor = Color.green;

    public int order = -1;
    public string labelText;

    public void SetLabel(string text)
    {
        labelText = text;
        label.text = text;
    }

    public void MarkVisited()
    {
        icon.color = visitedColor;
    }

    public void ResetColor()
    {
        icon.color = normalColor;
    }
}