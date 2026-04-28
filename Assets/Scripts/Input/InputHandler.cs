using System.Collections;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    private float _chooseDelay = 0.15f;
    private int _firstSelected = -1;
    private int _secondSelected = -1;
    private Coroutine _matchCoroutine;
    public event System.Action OnChooseNumber;

    public void OnCellClicked(int index)
    {
        if (!GameManager.Instance.BoardManager.IsValidSelectable(index)) return;
        OnChooseNumber?.Invoke();
        if (_firstSelected == index)
        {
            GameManager.Instance.BoardManager.SetCellSelected(index, false);
            _firstSelected = -1;
            return;
        }

        if (_secondSelected == index)
        {
            GameManager.Instance.BoardManager.SetCellSelected(index, false);
            _secondSelected = -1;
            return;
        }

        if (_firstSelected == -1)
        {
            _firstSelected = index;
            GameManager.Instance.BoardManager.SetCellSelected(index, true);
        }
        else if (_secondSelected == -1)
        {
            _secondSelected = index;
            GameManager.Instance.BoardManager.SetCellSelected(index, true);
            if (_matchCoroutine != null) StopCoroutine( _matchCoroutine );
            _matchCoroutine = StartCoroutine(TryMatch());
        }
    }

    private IEnumerator TryMatch()
    {
        yield return new WaitForSeconds(_chooseDelay);

        if (_firstSelected >= 0 && _secondSelected >= 0)
        {
            bool matched = GameManager.Instance.BoardManager.TryMatch(_firstSelected, _secondSelected);

            if (!matched)
            {
                GameManager.Instance.BoardManager.SetCellSelected(_firstSelected, false);
                GameManager.Instance.BoardManager.SetCellSelected(_secondSelected, false);
            }
        }

        ResetSelectionState();
    }

    public void ResetSelectionState()
    {
        _firstSelected = -1;
        _secondSelected = -1;
    }
}