using System;
using UnityEngine;

/// <summary>
/// Các quy tắc và hàm tiện ích cho board game: kiểm tra match, chuyển đổi index ↔ row/col, kiểm tra đường đi.
/// </summary>
public static class BoardRules
{
    /// <summary>
    /// Kiểm tra hai giá trị có match được với nhau không.
    /// Match khi: cùng giá trị, hoặc tổng bằng 10 (ví dụ: 3+7, 4+6).
    /// </summary>
    public static bool IsMatchValue(int a, int b)
    {
        return a == b || (a + b == 10);
    }

    /// <summary>
    /// Chuyển đổi index tuyến tính sang tọa độ hàng/cột.
    /// </summary>
    public static void IndexToRowCol(int index, int columns, out int row, out int col)
    {
        row = index / columns;
        col = index % columns;
    }

    /// <summary>
    /// Chuyển đổi tọa độ hàng/cột sang index tuyến tính.
    /// Trả về -1 nếu tọa độ không hợp lệ.
    /// </summary>
    public static int RowColToIndex(int row, int col, int columns, int cellCount)
    {
        if (row < 0 || col < 0 || col >= columns) return -1;

        int index = row * columns + col;
        if (index < 0 || index >= cellCount) return -1;

        return index;
    }

    /// <summary>
    /// Kiểm tra xem có đường đi giữa hai cell mà không bị chắn.
    /// </summary>
    public static bool IsPathClear(
        int indexA,
        int indexB,
        int columns,
        int cellCount,
        Func<int, bool> isBlocked
    )
    {
        IndexToRowCol(indexA, columns, out int rowA, out int colA);
        IndexToRowCol(indexB, columns, out int rowB, out int colB);

        int dRow = rowB - rowA;
        int dCol = colB - colA;

        // Cùng hàng → kiểm tra ngang
        if (rowA == rowB)
        {
            int step = colA < colB ? 1 : -1;
            for (int c = colA + step; c != colB; c += step)
            {
                int idx = RowColToIndex(rowA, c, columns, cellCount);
                if (idx >= 0 && isBlocked(idx))
                    return false;
            }
            return true;
        }

        // Cùng cột → kiểm tra dọc
        if (colA == colB)
        {
            int step = rowA < rowB ? 1 : -1;
            for (int r = rowA + step; r != rowB; r += step)
            {
                int idx = RowColToIndex(r, colA, columns, cellCount);
                if (idx >= 0 && isBlocked(idx))
                    return false;
            }
            return true;
        }

        // Đường chéo (hàng và cột cách nhau bằng nhau)
        if (Mathf.Abs(dRow) == Mathf.Abs(dCol))
        {
            int stepRow = dRow > 0 ? 1 : -1;
            int stepCol = dCol > 0 ? 1 : -1;

            int r = rowA + stepRow;
            int c = colA + stepCol;

            while (r != rowB && c != colB)
            {
                int idx = RowColToIndex(r, c, columns, cellCount);
                if (idx >= 0 && isBlocked(idx))
                    return false;

                r += stepRow;
                c += stepCol;
            }

            return true;
        }

        // Nếu không có num nào ở giữa
        int from = Mathf.Min(indexA, indexB) + 1;
        int to = Mathf.Max(indexA, indexB);

        for (int i = from; i < to; i++)
        {
            if (isBlocked(i))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Kiểm tra xem board còn cặp match nào không.
    /// Dùng để xác định điều kiện thua.
    /// </summary>
    public static bool HasAnyMatch(System.Collections.Generic.IReadOnlyList<CellData> cells, int columns)
    {
        if (cells == null || cells.Count <= 1) return false;

        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i].IsRemoved) continue;

            for (int j = i + 1; j < cells.Count; j++)
            {
                if (cells[j].IsRemoved) continue;

                if (!IsMatchValue(cells[i].Value, cells[j].Value))
                    continue;

                bool isPathClear = IsPathClear(
                    i,
                    j,
                    columns,
                    cells.Count,
                    idx => !cells[idx].IsRemoved
                );

                if (isPathClear)
                    return true;
            }
        }

        return false;
    }
}