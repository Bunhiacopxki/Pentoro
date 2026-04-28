using System.Collections.Generic;

[System.Serializable]
public class StageSnapshot
{
    public int stage;
    public int remainingAddTurns;
    public List<CellData> cells = new List<CellData>();
    public List<GemGoalEntry> goals = new List<GemGoalEntry>();
}