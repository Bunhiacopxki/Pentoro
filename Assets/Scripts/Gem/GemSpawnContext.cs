using System.Collections.Generic;

[System.Serializable]
public class GemGoalEntry
{
    public GemType gemType;
    public int targetCount;
    public int collectedCount;
    public int spawnedCount;

    public GemGoalEntry Clone()
    {
        return new GemGoalEntry
        {
            gemType = gemType,
            targetCount = targetCount,
            collectedCount = collectedCount,
            spawnedCount = spawnedCount
        };
    }
}

public class GemSpawnContext
{
    public int MinPercent = 5;
    public int MaxPercent = 7;

    public List<GemGoalEntry> Goals;

    public int GemRemaining()
    {
        int count = 0;

        for (int i = 0; i < Goals.Count; i++)
        {
            GemGoalEntry goal = Goals[i];
            if (goal.collectedCount + goal.spawnedCount < goal.targetCount)
                count++;
        }

        return count;
    }

    public GemGoalEntry GetAvailableGoal()
    {
        for (int i = 0; i < Goals.Count; i++)
        {
            GemGoalEntry goal = Goals[i];
            if (goal.collectedCount + goal.spawnedCount < goal.targetCount)
                return goal;
        }

        return null;
    }

    public bool IsCompleted()
    {
        for (int i = 0; i < Goals.Count; i++)
        {
            if (Goals[i].collectedCount < Goals[i].targetCount)
                return false;
        }

        return true;
    }
}