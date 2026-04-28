using System;
using System.Collections.Generic;

/// <summary>
/// Cung cấp các mảng giá trị fallback (board có sẵn) dựa trên các yêu cầu.
/// </summary>
public class BoardFallbackProvider
{
    private readonly BoardFallbackLibrary _library;

    public BoardFallbackProvider(BoardFallbackLibrary library)
    {
        _library = library;
    }

    /// <summary>
    /// Lấy một mảng giá trị ngẫu nhiên từ pool fallback tương ứng với số cặp yêu cầu.
    /// </summary>
    public int[] GetRandomFallback(int requiredPairCount)
    {
        List<IntBoardData> pool = GetPool(requiredPairCount);

        if (pool == null || pool.Count == 0)
            throw new Exception($"Không có fallback cho requiredPairCount = {requiredPairCount}");

        int index = UnityEngine.Random.Range(0, pool.Count);
        return CloneArray(pool[index].values);
    }

    private List<IntBoardData> GetPool(int requiredPairCount)
    {
        if (requiredPairCount == 3) return _library.pair3Boards;
        if (requiredPairCount == 2) return _library.pair2Boards;
        return _library.pair1Boards;
    }

    private int[] CloneArray(int[] source)
    {
        int[] clone = new int[source.Length];
        Array.Copy(source, clone, source.Length);
        return clone;
    }
}