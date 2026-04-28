using System.Collections.Generic;

public class BoardGenerationContext
{
    public int Rows { get; }
    public int Columns { get; }
    public int TotalCells => Columns * Rows;

    public int[] Values { get; }
    public bool[] LockedPairCells { get; }

    public Dictionary<int, int> DigitCounts { get; set; }
    public List<PairSlot> ForcedPairs { get; set; }

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