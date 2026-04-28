using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardPairResolveController
{
    private readonly float _waitAfterRemove;
    private readonly float _collapseMoveDuration;
    private readonly int _columns;
    private readonly BoardCollapseService _collapseService;

    public BoardPairResolveController(
        float waitAfterRemove,
        float collapseMoveDuration,
        int columns,
        BoardCollapseService collapseService
    )
    {
        _waitAfterRemove = waitAfterRemove;
        _collapseMoveDuration = collapseMoveDuration;
        _columns = columns;
        _collapseService = collapseService;
    }

    public IEnumerator ResolvePair(
        List<CellData> cells,
        int indexA,
        int indexB,
        Action<int, int> onPairMatched,
        Action<CellData> collectGemIfAny,
        Action<int> onCellRemoved,
        Action<BoardCollapsePlan> onCollapseStarted,
        Action refreshIndexes,
        Action rebuildView,
        Action evaluateBoardState,
        Action onCompleted
    )
    {
        onPairMatched?.Invoke(indexA, indexB);

        collectGemIfAny?.Invoke(cells[indexA]);
        collectGemIfAny?.Invoke(cells[indexB]);

        cells[indexA].IsRemoved = true;
        cells[indexB].IsRemoved = true;

        onCellRemoved?.Invoke(indexA);
        onCellRemoved?.Invoke(indexB);

        yield return new WaitForSeconds(_waitAfterRemove);

        BoardCollapsePlan collapsePlan = _collapseService.BuildCollapsePlan(cells, _columns);

        if (collapsePlan.HasCompletedRows)
        {
            onCollapseStarted?.Invoke(collapsePlan);

            if (collapsePlan.HasAnyMove)
                yield return new WaitForSeconds(_collapseMoveDuration);

            _collapseService.RemoveCompletedRows(cells, collapsePlan.CompletedRows, _columns);
            refreshIndexes?.Invoke();
        }

        rebuildView?.Invoke();
        evaluateBoardState?.Invoke();

        onCompleted?.Invoke();
    }
}