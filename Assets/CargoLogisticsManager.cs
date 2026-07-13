using UnityEngine;
using System.Linq;

public class CargoLogisticsManager : MonoBehaviour
{
    public static CargoLogisticsManager Instance;

    [SerializeField] private StackSlot[] slots;
    [SerializeField] private GoalStack[] goalOrderPerSlot;
    [SerializeField] private int optimalMoves;

    private int moveCount = 0;
    private bool hasPlanningStarted = false;
    private float sceneStartTime;

    void Awake()
    {
        Instance = this;
        sceneStartTime = Time.time;
    }

    public void OnDragStarted()
    {
        if (hasPlanningStarted) return;
        hasPlanningStarted = true;
        Debug.Log($"Pre-planning time: {Time.time - sceneStartTime:F2}s");
    }

    public void RegisterMove()
    {
        moveCount++;
        Debug.Log($"Move {moveCount} (optimal: {optimalMoves})");
        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            var actual = slots[i].transform.Cast<Transform>().Select(t => t.name).ToArray();
            if (!actual.SequenceEqual(goalOrderPerSlot[i].items)) return;
        }
        Debug.Log($"Solved in {moveCount} moves (optimal was {optimalMoves}).");
    }
}

[System.Serializable]
public class GoalStack
{
    public string[] items;
}