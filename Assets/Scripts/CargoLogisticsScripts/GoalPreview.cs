using System.Collections.Generic;
using UnityEngine;

public class GoalPreview
{
    public static void Build(GoalSlotDisplay[] goalSlots, List<List<string>> goalStacks, Dictionary<string, Sprite> itemSprites)
    {
        for (int s = 0; s < goalStacks.Count && s < goalSlots.Length; s++)
            goalSlots[s].ShowStack(goalStacks[s], itemSprites);
    }
}