using UnityEngine;
using System.Collections.Generic;

public class PairLayoutPlanner
{
    private readonly int _columns;
    private readonly int _rows;

    public PairLayoutPlanner(int columns, int rows)
    {
        _columns = columns;
        _rows = rows;
    }

    public List<PairSlot> CreatePairSlots(int targetPairCount, int totalCells)
    {
        List<PairSlot> allCandidates = BuildPairSlots(totalCells);
        bool[] used = new bool[totalCells];
        List<PairSlot> selected = new List<PairSlot>(targetPairCount);

        for (int i = 0; i < allCandidates.Count; i++)
        {
            PairSlot pair = allCandidates[i];

            if (used[pair.IndexA] || used[pair.IndexB])
                continue;

            selected.Add(pair);
            used[pair.IndexA] = true;
            used[pair.IndexB] = true;

            if (selected.Count >= targetPairCount)
                break;
        }

        return selected;
    }

    private List<PairSlot> BuildPairSlots(int totalCells)
    {
        List<PairSlot> result = new List<PairSlot>();

        for (int row = 0; row < _rows; row++)
        {
            for (int col = 0; col < _columns; col++)
            {
                int a = row * _columns + col;
                if (a < 0 || a >= totalCells)
                    continue;

                AddPairIfValid(result, a, row, col + 1, totalCells);     
                AddPairIfValid(result, a, row + 1, col, totalCells);     
                AddPairIfValid(result, a, row + 1, col + 1, totalCells); 
                AddPairIfValid(result, a, row + 1, col - 1, totalCells);
            }
        }

        Shuffle(result);
        return result;
    }

    private void AddPairIfValid(List<PairSlot> result, int a, int rowB, int colB, int totalCells)
    {
        if (rowB < 0 || rowB >= _rows)
            return;

        if (colB < 0 || colB >= _columns)
            return;

        int b = rowB * _columns + colB;
        if (b < 0 || b >= totalCells)
            return;

        result.Add(new PairSlot(a, b));
    }

    public void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}