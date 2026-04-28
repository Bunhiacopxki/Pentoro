using UnityEngine;

public class BoardCollapseAnimator : MonoBehaviour
{
    [SerializeField] private float collapseMoveDuration = 0.25f;

    private BoardManager _boardManager;

    private void OnEnable()
    {
        if (GameManager.Instance != null && GameManager.Instance.BoardManager != null)
        {
            _boardManager = GameManager.Instance.BoardManager;
            _boardManager.OnCollapseStarted += HandleCollapseStarted;
        }
    }

    private void OnDisable()
    {
        if (_boardManager != null)
            _boardManager.OnCollapseStarted -= HandleCollapseStarted;
    }

    private void HandleCollapseStarted(BoardCollapsePlan plan)
    {
        for (int i = 0; i < plan.Moves.Count; i++)
        {
            CellCollapseMove move = plan.Moves[i];
            CellView view = _boardManager.GetCellView(move.OldIndex);
            if (view == null) continue;

            Vector2 targetPos = _boardManager.GetAnchoredPositionByIndex(move.NewIndex);
            view.PlayMoveTo(targetPos, collapseMoveDuration);
        }
    }
}