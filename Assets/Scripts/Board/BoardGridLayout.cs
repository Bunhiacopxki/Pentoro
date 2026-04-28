using UnityEngine;

public class BoardGridLayout
{
    private readonly RectTransform _boardParent;
    private readonly int _columns;
    private readonly int _rows;

    public BoardGridLayout(RectTransform boardParent, int columns, int visibleRows)
    {
        _boardParent = boardParent;
        _columns = columns;
        _rows = visibleRows;
    }

    public float CellWidth => _boardParent.rect.width / _columns;
    public float CellHeight => _boardParent.rect.height / _rows;

    public Vector2 CellSize => new Vector2(CellWidth, CellHeight);

    public Vector2 GetAnchoredPositionByIndex(int index)
    {
        int row = index / _columns;
        int col = index % _columns;

        float boardWidth = _boardParent.rect.width;
        float boardHeight = _boardParent.rect.height;

        float startX = -boardWidth * 0.5f + CellWidth * 0.5f;
        float startY = boardHeight * 0.5f - CellHeight * 0.5f;

        float x = startX + col * CellWidth;
        float y = startY - row * CellHeight;

        return new Vector2(x, y);
    }
}