using System.Collections.Generic;
using UnityEngine;

public class BoardViewPool
{
    private readonly List<CellView> _views;
    private readonly CellView _cellPrefab;
    private readonly RectTransform _boardParent;
    private readonly InputHandler _inputHandler;
    private readonly BoardGridLayout _layout;

    public BoardViewPool(
        List<CellView> views,
        CellView cellPrefab,
        RectTransform boardParent,
        InputHandler inputHandler,
        BoardGridLayout layout
    )
    {
        _views = views;
        _cellPrefab = cellPrefab;
        _boardParent = boardParent;
        _inputHandler = inputHandler;
        _layout = layout;
    }

    public void EnsureViewPoolSize(int targetCount)
    {
        while (_views.Count < targetCount)
        {
            CellView view = Object.Instantiate(_cellPrefab, _boardParent);
            RectTransform rt = view.transform as RectTransform;

            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = _layout.CellSize;

            _views.Add(view);
        }
    }

    public void PrepareRevealView(IReadOnlyList<CellData> cells)
    {
        EnsureViewPoolSize(cells.Count);

        for (int i = 0; i < _views.Count; i++)
        {
            bool active = i < cells.Count;

            if (!active)
            {
                _views[i].gameObject.SetActive(false);
                continue;
            }

            SetupSingleView(cells[i], i);
            _views[i].gameObject.SetActive(false);
        }
    }

    public void RebuildView(IReadOnlyList<CellData> cells)
    {
        EnsureViewPoolSize(cells.Count);

        for (int i = 0; i < _views.Count; i++)
        {
            bool active = i < cells.Count;
            _views[i].gameObject.SetActive(active);

            if (!active)
                continue;

            SetupSingleView(cells[i], i);
        }
    }

    public void PrepareNewViews(IReadOnlyList<CellData> cells, int startIndex)
    {
        EnsureViewPoolSize(cells.Count);

        for (int i = startIndex; i < cells.Count; i++)
        {
            SetupSingleView(cells[i], i);
            _views[i].gameObject.SetActive(false);
        }
    }

    public void SetupSingleView(CellData cell, int index)
    {
        _views[index].Setup(cell, index, _inputHandler);

        RectTransform rt = _views[index].transform as RectTransform;
        rt.sizeDelta = _layout.CellSize;

        _views[index].SetPositionInstant(_layout.GetAnchoredPositionByIndex(index));
    }

    public CellView GetCellView(int index)
    {
        if (index < 0 || index >= _views.Count) return null;
        return _views[index];
    }

    public void SetSelected(int index, bool selected)
    {
        if (index < 0 || index >= _views.Count) return;
        _views[index].SetSelected(selected);
    }
}