using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Service tính toán kế hoạch collapse: xác định các hàng đã full và tính toán vị trí mới cho cell.
/// </summary>
public class BoardCollapseService
{
    /// <summary>
    /// Xây dựng kế hoạch collapse: tìm các hàng đã bị xóa hoàn toàn và tính vị trí mới cho các cell còn lại.
    /// </summary>
    public BoardCollapsePlan BuildCollapsePlan(IReadOnlyList<CellData> cells, int columns)
    {
        BoardCollapsePlan plan = new BoardCollapsePlan();

        int rowCount = Mathf.CeilToInt((float)cells.Count / columns);

        // Tìm các hàng đã bị xóa hoàn toàn
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

        // Tính vị trí mới cho mỗi cell còn sống
        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i].IsRemoved)
                continue;

            BoardRules.IndexToRowCol(i, columns, out int oldRow, out int col);

            // Đếm số hàng completed nằm trên hàng hiện tại
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

    /// <summary>
    /// Xóa các hàng đã completed khỏi danh sách cells.
    /// </summary>
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

/// <summary>
/// Kế hoạch collapse: chứa danh sách hàng cần xóa và danh sách di chuyển cell.
/// </summary>
public class BoardCollapsePlan
{
    public List<int> CompletedRows = new List<int>();
    public List<CellCollapseMove> Moves = new List<CellCollapseMove>();

    public bool HasCompletedRows => CompletedRows.Count > 0;
    public bool HasAnyMove => Moves.Count > 0;
}

/// <summary>
/// Thông tin di chuyển của một cell trong collapse.
/// </summary>
public class CellCollapseMove
{
    public int OldIndex;
    public int NewIndex;
    public int Column;
    public int OldRow;
    public int NewRow;
}
