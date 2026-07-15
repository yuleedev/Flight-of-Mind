using UnityEngine;
using TMPro;

public class MoveCount : MonoBehaviour
{
    private TMP_Text text;

    void Awake()
    {
        text = GetComponent<TMP_Text>();
        SetMoves(0);
    }

    public void SetMoves(int moves)
    {
        text.text = $"{moves}";
    }
}