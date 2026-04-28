using System.Collections.Generic;

/// <summary>
/// Context lưu trữ dữ liệu trong quá trình generate board: giá trị cells, số lượng mỗi số, các cặp forced.
/// </summary>
public class BoardGenerationContext
{
    public int Rows { get; }
    public int Columns { get; }
    public int TotalCells => Columns * Rows;

    public int[] Values { get; }           // Giá trị của từng cell (0 = chưa fill)
    public bool[] LockedPairCells { get; }  // Cell đã thuộc forced pair

    public Dictionary<int, int> DigitCounts { get; set; }  // Số lượng còn lại của mỗi digit (1-9)
    public List<PairSlot> ForcedPairs { get; set; }         // Các cặp đã được xác định trước

    public BoardGenerationContext(int columns, int rows)
    {
        Rows = rows;
        Columns = columns;

        Values = new int[columns * rows];
        LockedPairCells = new bool[columns * rows];
        DigitCounts = new Dictionary<int, int>(9);
        ForcedPairs = new List<PairSlot>();
    }
}