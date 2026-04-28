using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GemGoalLibrary", menuName = "Gem/Goal Library")]
public class GemGoalLibrary : ScriptableObject
{
    [System.Serializable]
    public class GemGoalConfigData
    {
        public GemType gemType;
        public int targetCount;
    }

    [System.Serializable]
    public class StageGemGoalData
    {
        public int stage;
        public List<GemGoalConfigData> goals = new List<GemGoalConfigData>();
    }

    [SerializeField] private List<StageGemGoalData> stages = new List<StageGemGoalData>();

    public GemSpawnContext CreateContext(int stage)
    {
        StageGemGoalData stageData = null;

        for (int i = 0; i < stages.Count; i++)
        {
            if (stages[i].stage == stage)
            {
                stageData = stages[i];
                break;
            }
        }

        if (stageData == null && stages.Count > 0)
        {
            stageData = stages[stages.Count - 1];
        }

        GemSpawnContext context = new GemSpawnContext();
        context.Goals = new List<GemGoalEntry>();

        if (stageData != null)
        {
            for (int i = 0; i < stageData.goals.Count; i++)
            {
                GemGoalConfigData src = stageData.goals[i];
                GemGoalEntry goal = new GemGoalEntry
                {
                    gemType = src.gemType,
                    targetCount = src.targetCount,
                    collectedCount = 0,
                    spawnedCount = 0
                };

                context.Goals.Add(goal);
            }
        }

        return context;
    }

    public List<GemGoalConfigData> GetGemGoal(int stage)
    {
        for (int i = 0; i < stages.Count; i++)
        {
            if (stages[i].stage == stage)
            {
                return stages[i].goals;
            }
        }
        return null;
    }
}