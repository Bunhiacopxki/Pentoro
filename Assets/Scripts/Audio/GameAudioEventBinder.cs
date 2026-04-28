using UnityEngine;

public class GameAudioEventBinder : MonoBehaviour
{
    private InputHandler _inputHandler;

    private void Start()
    {
        GameManager.Instance.BoardManager.OnPairMatched += HandlePairMatched;
        GameManager.Instance.BoardManager.OnCollapseStarted += HandleCollapseStarted;
    }

    private void OnDestroy()
    {
        GameManager.Instance.BoardManager.OnPairMatched -= HandlePairMatched;
        GameManager.Instance.BoardManager.OnCollapseStarted -= HandleCollapseStarted;

        if (_inputHandler != null)
        {
            _inputHandler.OnChooseNumber -= HandleChooseNumber;
        }
    }

    public void SetUp(InputHandler inputHandler)
    {
        _inputHandler = inputHandler;
        _inputHandler.OnChooseNumber += HandleChooseNumber;
    }

    private void HandleChooseNumber()
    {
        GameManager.Instance.AudioManager.PlaySfx(SfxType.ChooseNumber);
    }

    private void HandlePairMatched(int indexA, int indexB)
    {
        GameManager.Instance.AudioManager.PlaySfx(SfxType.PairClear);
    }

    private void HandleCollapseStarted(BoardCollapsePlan plan)
    {
        if (plan == null) return;
        if (!plan.HasCompletedRows) return;

        GameManager.Instance.AudioManager.PlaySfx(SfxType.RowClear);
    }
}