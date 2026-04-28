using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Điền giá trị số vào các cell trống trên bảng.
/// Đảm bảo không tạo ra các cặp ghép không mong muốn.
/// Sử dụng thuật toán backtracking để tìm cách điền hợp lệ.
/// </summary>
public class BoardFiller
{
    private readonly int _columns;

    public BoardFiller(int columns)
    {
        _columns = columns;
    }

    private bool AssignForcedPairs(BoardGenerationContext context)
    {
        for (int i = 0; i < context.ForcedPairs.Count; i++)
        {
            PairSlot slot = context.ForcedPairs[i];
            List<PairValueCandidate> candidates = BuildPairCandidates(context, slot);

            if (candidates.Count == 0) return false;

            Shuffle(candidates);
            PairValueCandidate chosen = candidates[0];

            context.Values[slot.IndexA] = chosen.ValueA;
            context.Values[slot.IndexB] = chosen.ValueB;

            context.LockedPairCells[slot.IndexA] = true;
            context.LockedPairCells[slot.IndexB] = true;

            context.DigitCounts[chosen.ValueA]--;
            context.DigitCounts[chosen.ValueB]--;
        }

        return true;
    }

    private List<PairValueCandidate> BuildPairCandidates(BoardGenerationContext context, PairSlot slot)
    {
        List<PairValueCandidate> result = new List<PairValueCandidate>();

        for (int a = 1; a <= 9; a++)
        {
            for (int b = 1; b <= 9; b++)
            {
                if (!BoardRules.IsMatchValue(a, b))
                    continue;

                if (!HasEnoughCount(context.DigitCounts, a, b))
                    continue;

                if (UnexpectedPair(context, slot.IndexA, a, slot.IndexB))
                    continue;

                if (UnexpectedPair(context, slot.IndexB, b, slot.IndexA))
                    continue;

                result.Add(new PairValueCandidate(a, b));
            }
        }

        return result;
    }

    private bool HasEnoughCount(Dictionary<int, int> digitCounts, int a, int b)
    {
        if (a == b) return digitCounts[a] >= 2;
        return digitCounts[a] > 0 && digitCounts[b] > 0;
    }

    private bool UnexpectedPair(
        BoardGenerationContext context,
        int currentIndex,
        int testValue,
        int pairedIndex)
    {
        for (int i = 0; i < context.Values.Length; i++)
        {
            if (i == currentIndex || i == pairedIndex)
                continue;

            if (context.Values[i] == 0)
                continue;

            if (!BoardRules.IsMatchValue(testValue, context.Values[i]))
                continue;

            if (BoardRules.IsPathClear(
                currentIndex,
                i,
                _columns,
                context.Values.Length,
                idx => context.Values[idx] != 0
            ))
                return true;
        }

        return false;
    }

    private List<int> BuildFillOrder(int totalCells, bool[] lockedPairCells)
    {
        List<int> order = new List<int>(totalCells);

        for (int i = 0; i < totalCells; i++)
        {
            if (!lockedPairCells[i])
                order.Add(i);
        }

        return order;
    }

    /// <summary>
    /// Điền giá trị vào tất cả các cell trống trong context.
    /// Thứ tự ưu tiên: forced pairs trước, sau đó các ô còn lại.
    /// </summary>
    public bool Fill(BoardGenerationContext context)
    {
        if (!AssignForcedPairs(context))
            return false;

        List<int> fillOrder = BuildFillOrder(context.Values.Length, context.LockedPairCells);
        return FillRemainingCells(context, fillOrder, 0);
    }

    /// <summary>
    /// Hàm đệ quy điền các cell còn lại theo thứ tự fillOrder.
    /// Sử dụng backtracking: thử từng candidate, nếu không được thì quay lại.
    /// </summary>
    private bool FillRemainingCells(BoardGenerationContext context, List<int> order, int orderIndex)
    {
        if (orderIndex >= order.Count)
            return true;

        int index = order[orderIndex];
        List<int> candidates = BuildSafeCandidates(context, index);

        for (int i = 0; i < candidates.Count; i++)
        {
            int digit = candidates[i];

            context.Values[index] = digit;
            context.DigitCounts[digit]--;

            if (FillRemainingCells(context, order, orderIndex + 1))
                return true;

            context.Values[index] = 0;
            context.DigitCounts[digit]++;
        }

        return false;
    }

    /// <summary>
    /// Xây dựng danh sách các số có thể điền vào một ô mà không tạo cặp không mong muốn.
    /// Loại bỏ các số đã hết số lượng hoặc tạo ra các cặp không mong muốn
    /// </summary>
    private List<int> BuildSafeCandidates(BoardGenerationContext context, int index)
    {
        List<int> result = new List<int>();

        for (int digit = 1; digit <= 9; digit++)
        {
            if (context.DigitCounts[digit] <= 0)
                continue;

            if (CreateUnexpectedPair(context, index, digit))
                continue;

            result.Add(digit);
        }

        Shuffle(result);
        return result;
    }

    private bool CreateUnexpectedPair(BoardGenerationContext context, int index, int testValue)
    {
        for (int i = 0; i < context.Values.Length; i++)
        {
            if (i == index)
                continue;

            if (context.Values[i] == 0)
                continue;

            if (!BoardRules.IsMatchValue(testValue, context.Values[i]))
                continue;

            if (!BoardRules.IsPathClear(
                index,
                i,
                _columns,
                context.Values.Length,
                idx => context.Values[idx] != 0
            ))
                continue;

            if (IsForcedPair(context.ForcedPairs, index, i))
                continue;

            return true;
        }

        return false;
    }

    private bool IsForcedPair(List<PairSlot> forcedPairs, int a, int b)
    {
        for (int i = 0; i < forcedPairs.Count; i++)
        {
            if (forcedPairs[i].IsPair(a, b))
                return true;
        }

        return false;
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

public class PairValueCandidate
{
    public readonly int ValueA;
    public readonly int ValueB;

    public PairValueCandidate(int valueA, int valueB)
    {
        ValueA = valueA;
        ValueB = valueB;
    }
}