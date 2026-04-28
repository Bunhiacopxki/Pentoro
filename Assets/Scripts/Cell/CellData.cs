using System;

/// <summary>
/// Loại gem có thể xuất hiện trên cell.
/// </summary>
public enum GemType
{
    None = 0,
    Pink = 1,
    Orange = 2,
    Purple = 3,
}

/// <summary>
/// Dữ liệu logic của một cell
/// </summary>
[Serializable]
public class CellData
{
    public int Value;          // Giá trị số của cell (1-9)
    public bool IsRemoved;     // Cell đã bị xóa chưa
    public bool HasGem;        // Cell có gem không
    public int Index;          // Vị trí index trong danh sách cells
    public GemType GemType;    // Loại gem (nếu có)

    public CellData(int value, int index, bool hasGem = false, GemType gemType = GemType.None)
    {
        Value = value;
        Index = index;
        HasGem = hasGem;
        GemType = hasGem ? gemType : GemType.None;
        IsRemoved = false;
    }

    /// <summary>
    /// Tạo bản sao (clone) của cell data.
    /// </summary>
    public CellData Clone()
    {
        return new CellData(Value, Index, HasGem, GemType)
        {
            IsRemoved = IsRemoved
        };
    }
}