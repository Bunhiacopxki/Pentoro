using System.Collections.Generic;

/// <summary>
/// Sinh dữ liệu các cell cho board game từ fallback values.
/// </summary>
public class BoardGenerator
{
    private BoardFallbackProvider _fallbackProvider;

    public BoardGenerator(BoardFallbackLibrary fallbackLibrary)
    {
        _fallbackProvider = new BoardFallbackProvider(fallbackLibrary);
    }

    /// <summary>
    /// Sinh danh sách cell cho board mới.
    /// </summary>
    public List<CellData> Generate(GemSpawnContext gemContext)
    {
        // Lấy danh sách giá trị ngẫu nhiên từ fallback library
        int[] fallbackValues = _fallbackProvider.GetRandomFallback(GameManager.Instance.StageManager.RequiredInitialPairCount);
        return BuildCells(fallbackValues, gemContext);
    }

    /// <summary>
    /// Tạo danh sách CellData từ mảng giá trị.
    /// </summary>
    private List<CellData> BuildCells(int[] values, GemSpawnContext gemContext)
    {
        List<CellData> cells = new List<CellData>(values.Length);

        for (int i = 0; i < values.Length; i++)
        {
            cells.Add(new CellData(values[i], i));
        }

        return cells;
    }
}