using System.Collections.Generic;
using UnityEngine;

public class BoardValidator
{
    private readonly int _columns;

    public BoardValidator(int columns)
    {
        _columns = columns;
    }

    public bool Validate(BoardGenerationContext context, int requiredPairCount)
    {
        if (!HasNoEmptyCells(context.Values))
        {
            Debug.LogError("Board còn ô chưa được fill.");
            return false;
        }

        if (!ContainsAllDigits(context.Values))
        {
            Debug.LogError("Board không chứa đủ các số từ 1 tới 9.");
            return false;
        }

        int pairCount = CountMaximumIndependentPairs(context.Values);

        if (pairCount != requiredPairCount)
        {
            Debug.LogError("Board có " + pairCount + " cặp, yêu cầu " + requiredPairCount + " cặp.");
            return false;
        }

        return true;
    }

    private bool ContainsAllDigits(int[] values)
    {
        bool[] seen = new bool[10];

        for (int i = 0; i < values.Length; i++)
        {
            int value = values[i];
            if (value >= 1 && value <= 9)
                seen[value] = true;
        }

        for (int digit = 1; digit <= 9; digit++)
        {
            if (!seen[digit])
                return false;
        }

        return true;
    }

    private bool HasNoEmptyCells(int[] values)
    {
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] == 0)
                return false;
        }

        return true;
    }

    private int CountMaximumIndependentPairs(int[] values)
    {
        List<PairSlot> allPairs = CollectAllPairs(values);
        if (allPairs.Count == 0)
            return 0;

        Dictionary<int, List<int>> adjacency = BuildAdjacency(values.Length, allPairs);

        bool[] used = new bool[values.Length];
        Dictionary<string, int> memo = new Dictionary<string, int>();
        return SolveMaxIndependentPairsArray(adjacency, used, memo);
    }

    private List<PairSlot> CollectAllPairs(int[] values)
    {
        List<PairSlot> result = new List<PairSlot>();

        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] == 0)
                continue;

            for (int j = i + 1; j < values.Length; j++)
            {
                if (values[j] == 0)
                    continue;

                if (!BoardRules.IsMatchValue(values[i], values[j]))
                    continue;

                if (!BoardRules.IsPathClear(
                    i,
                    j,
                    _columns,
                    values.Length,
                    idx => idx != i && idx != j && values[idx] != 0
                ))
                    continue;

                result.Add(new PairSlot(i, j));
            }
        }

        return result;
    }

    private Dictionary<int, List<int>> BuildAdjacency(int cellCount, List<PairSlot> pairs)
    {
        Dictionary<int, List<int>> adjacency = new Dictionary<int, List<int>>(cellCount);

        for (int i = 0; i < cellCount; i++)
            adjacency[i] = new List<int>();

        for (int i = 0; i < pairs.Count; i++)
        {
            PairSlot pair = pairs[i];
            adjacency[pair.IndexA].Add(pair.IndexB);
            adjacency[pair.IndexB].Add(pair.IndexA);
        }

        return adjacency;
    }

    private int SolveMaxIndependentPairsArray(
        Dictionary<int, List<int>> adjacency,
        bool[] used,
        Dictionary<string, int> memo)
    {
        string key = BuildUsedKey(used);
        if (memo.TryGetValue(key, out int cached))
            return cached;

        int firstUnused = -1;
        for (int i = 0; i < used.Length; i++)
        {
            if (!used[i])
            {
                firstUnused = i;
                break;
            }
        }

        if (firstUnused == -1)
            return 0;

        used[firstUnused] = true;
        int best = SolveMaxIndependentPairsArray(adjacency, used, memo);
        used[firstUnused] = false;

        List<int> neighbors = adjacency[firstUnused];
        for (int i = 0; i < neighbors.Count; i++)
        {
            int neighbor = neighbors[i];
            if (used[neighbor])
                continue;

            used[firstUnused] = true;
            used[neighbor] = true;

            int candidate = 1 + SolveMaxIndependentPairsArray(adjacency, used, memo);
            if (candidate > best)
                best = candidate;

            used[firstUnused] = false;
            used[neighbor] = false;
        }

        memo[key] = best;
        return best;
    }

    private string BuildUsedKey(bool[] used)
    {
        char[] chars = new char[used.Length];
        for (int i = 0; i < used.Length; i++)
            chars[i] = used[i] ? '1' : '0';

        return new string(chars);
    }
}