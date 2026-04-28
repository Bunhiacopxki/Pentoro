using System.Collections.Generic;

public class BoardGenerator
{
    private BoardFallbackProvider _fallbackProvider;

    public BoardGenerator(BoardFallbackLibrary fallbackLibrary)
    {
        _fallbackProvider = new BoardFallbackProvider(fallbackLibrary);
    }

    public List<CellData> Generate(GemSpawnContext gemContext)
    {
        int[] fallbackValues = _fallbackProvider.GetRandomFallback(GameManager.Instance.StageManager.RequiredInitialPairCount);
        return BuildCells(fallbackValues, gemContext);
    }

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