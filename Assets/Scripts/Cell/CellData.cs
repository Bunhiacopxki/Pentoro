using System;

public enum GemType
{
    None = 0,
    Pink = 1,
    Orange = 2,
    Purple = 3,
}

[Serializable]
public class CellData
{
    public int Value;
    public bool IsRemoved;
    public bool HasGem;
    public int Index;
    public GemType GemType;

    public CellData(int value, int index, bool hasGem = false, GemType gemType = GemType.None)
    {
        Value = value;
        Index = index;
        HasGem = hasGem;
        GemType = hasGem ? gemType : GemType.None;
        IsRemoved = false;
    }

    public CellData Clone()
    {
        return new CellData(Value, Index, HasGem, GemType)
        {
            IsRemoved = IsRemoved
        };
    }
}