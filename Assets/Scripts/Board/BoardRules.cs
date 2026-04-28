using System;
using UnityEngine;

public static class BoardRules
{
    public static bool IsMatchValue(int a, int b)
    {
        return a == b || (a + b == 10);
    }

    public static void IndexToRowCol(int index, int columns, out int row, out int col)
    {
        row = index / columns;
        col = index % columns;
    }

    public static int RowColToIndex(int row, int col, int columns, int cellCount)
    {
        if (row < 0 || col < 0 || col >= columns) return -1;

        int index = row * columns + col;
        if (index < 0 || index >= cellCount) return -1;

        return index;
    }

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

        return false;
    }

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