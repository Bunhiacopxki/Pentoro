using System.Collections.Generic;

public class BoardAddNumberService
{
    private int _maxCell => GameManager.Instance.BoardManager.Columns * GameManager.Instance.BoardManager.Rows;

    private int AliveCellsCount(IReadOnlyList<CellData> sourceCells)
    {
        int res = 0;
        for (int i = 0; i < sourceCells.Count; i++)
        {
            if (sourceCells[i].IsRemoved == false) res++;
        }
        return res;
    }
    
    public List<CellData> CreateAppendedCells(IReadOnlyList<CellData> sourceCells, int startIndex)
    {
        List<CellData> appended = new List<CellData>();

        int addCellsQuantity = AliveCellsCount(sourceCells);

        if (sourceCells.Count + addCellsQuantity > _maxCell) addCellsQuantity = _maxCell - sourceCells.Count;

        int remainsCount = 0;
        for (int i = 0; i < sourceCells.Count; i++)
        {
            if (remainsCount >= addCellsQuantity) break;
            CellData cell = sourceCells[i];
            if (cell.IsRemoved) continue;

            int newIndex = startIndex + appended.Count;
            CellData clone = new CellData(cell.Value, newIndex, false, GemType.None);
            appended.Add(clone);
            remainsCount++;
        }

        return appended;
    }
}