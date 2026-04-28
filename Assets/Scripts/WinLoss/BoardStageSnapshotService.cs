using System.Collections.Generic;

public class BoardStageSnapshotService
{
    public StageSnapshot Save(
        IReadOnlyList<CellData> cells,
        GemSpawnContext gemContext,
        int stage,
        int remainingAddTurns
    )
    {
        StageSnapshot snapshot = new StageSnapshot
        {
            stage = stage,
            remainingAddTurns = remainingAddTurns,
            cells = new List<CellData>(),
            goals = new List<GemGoalEntry>()
        };

        for (int i = 0; i < cells.Count; i++)
        {
            snapshot.cells.Add(cells[i].Clone());
        }

        if (gemContext != null && gemContext.Goals != null)
        {
            for (int i = 0; i < gemContext.Goals.Count; i++)
            {
                snapshot.goals.Add(gemContext.Goals[i].Clone());
            }
        }

        return snapshot;
    }

    public void RestoreCells(StageSnapshot snapshot, List<CellData> targetCells)
    {
        targetCells.Clear();

        for (int i = 0; i < snapshot.cells.Count; i++)
        {
            targetCells.Add(snapshot.cells[i].Clone());
        }
    }

    public GemSpawnContext RestoreGemContext(StageSnapshot snapshot)
    {
        GemSpawnContext context = new GemSpawnContext();
        context.Goals = new List<GemGoalEntry>();

        for (int i = 0; i < snapshot.goals.Count; i++)
        {
            context.Goals.Add(snapshot.goals[i].Clone());
        }

        return context;
    }
}