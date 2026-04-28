using System.Collections.Generic;
using UnityEngine;

public class BoardCollapseService
{
    public BoardCollapsePlan BuildCollapsePlan(IReadOnlyList<CellData> cells, int columns)
    {
        BoardCollapsePlan plan = new BoardCollapsePlan();

        int rowCount = Mathf.CeilToInt((float)cells.Count / columns);

        for (int row = 0; row < rowCount; row++)
        {
            int start = row * columns;
            int end = Mathf.Min(start + columns, cells.Count);

            if (end - start < columns)
                continue;

            bool allRemoved = true;
            for (int i = start; i < end; i++)
            {
                if (!cells[i].IsRemoved)
                {
                    allRemoved = false;
                    break;
                }
            }

            if (allRemoved)
                plan.CompletedRows.Add(row);
        }

        if (plan.CompletedRows.Count == 0)
            return plan;

        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i].IsRemoved)
                continue;

            BoardRules.IndexToRowCol(i, columns, out int oldRow, out int col);

            int shift = 0;
            for (int j = 0; j < plan.CompletedRows.Count; j++)
            {
                if (plan.CompletedRows[j] < oldRow)
                    shift++;
            }

            if (shift == 0)
                continue;

            int newRow = oldRow - shift;
            int newIndex = newRow * columns + col;

            plan.Moves.Add(new CellCollapseMove
            {
                OldIndex = i,
                NewIndex = newIndex,
                Column = col,
                OldRow = oldRow,
                NewRow = newRow
            });
        }

        return plan;
    }

    public void RemoveCompletedRows(List<CellData> cells, List<int> completedRows, int columns)
    {
        for (int i = completedRows.Count - 1; i >= 0; i--)
        {
            int row = completedRows[i];
            int start = row * columns;
            cells.RemoveRange(start, columns);
        }
    }
}

public class BoardCollapsePlan
{
    public List<int> CompletedRows = new List<int>();
    public List<CellCollapseMove> Moves = new List<CellCollapseMove>();

    public bool HasCompletedRows => CompletedRows.Count > 0;
    public bool HasAnyMove => Moves.Count > 0;
}

public class CellCollapseMove
{
    public int OldIndex;
    public int NewIndex;
    public int Column;
    public int OldRow;
    public int NewRow;
}
