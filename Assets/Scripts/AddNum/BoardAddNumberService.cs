using System.Collections.Generic;

/// <summary>
/// Service tạo các cell mới khi người chơi thêm số vào board.
/// </summary>
public class BoardAddNumberService
{
    // Số cell tối đa trên board
    private int _maxCell => GameManager.Instance.BoardManager.Columns * GameManager.Instance.BoardManager.Rows;

    /// <summary>
    /// Đếm số cell còn sống (chưa bị xóa).
    /// </summary>
    private int AliveCellsCount(IReadOnlyList<CellData> sourceCells)
    {
        int res = 0;
        for (int i = 0; i < sourceCells.Count; i++)
        {
            if (sourceCells[i].IsRemoved == false) res++;
        }
        return res;
    }
    
    /// <summary>
    /// Tạo danh sách cell mới bằng số lượng cell còn sống.
    /// Mỗi cell sống tạo ra một cell mới có cùng giá trị.
    /// </summary>
    public List<CellData> CreateAppendedCells(IReadOnlyList<CellData> sourceCells, int startIndex)
    {
        List<CellData> appended = new List<CellData>();

        // Số cell cần thêm
        int addCellsQuantity = AliveCellsCount(sourceCells);

        // Không vượt quá giới hạn board
        if (sourceCells.Count + addCellsQuantity > _maxCell) addCellsQuantity = _maxCell - sourceCells.Count;

        int remainsCount = 0;
        for (int i = 0; i < sourceCells.Count; i++)
        {
            if (remainsCount >= addCellsQuantity) break;
            CellData cell = sourceCells[i];
            if (cell.IsRemoved) continue;

            // Clone cell còn sống với index mới
            int newIndex = startIndex + appended.Count;
            CellData clone = new CellData(cell.Value, newIndex, false, GemType.None);
            appended.Add(clone);
            remainsCount++;
        }

        return appended;
    }
}