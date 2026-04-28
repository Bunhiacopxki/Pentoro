using System.Collections;
using UnityEngine;

/// <summary>
/// Xử lý input từ người chơi: click chọn cell, quản lý trạng thái chọn 2 cell để match.
/// </summary>
public class InputHandler : MonoBehaviour
{
    private float _chooseDelay = 0.15f;  // Thời gian chờ trước khi thử match
    private int _firstSelected = -1;     // Index của cell chọn đầu tiên
    private int _secondSelected = -1;   // Index của cell chọn thứ hai
    private Coroutine _matchCoroutine;
    public event System.Action OnChooseNumber;

    /// <summary>
    /// Được gọi khi người chơi click vào một cell.
    /// </summary>
    public void OnCellClicked(int index)
    {
        // Kiểm tra cell có hợp lệ không
        if (!GameManager.Instance.BoardManager.IsValidSelectable(index)) return;
        
        // Event cho audio
        OnChooseNumber?.Invoke();
        
        // Bỏ chọn nếu click lại cell đang được chọn
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

        // Chọn cell đầu tiên
        if (_firstSelected == -1)
        {
            _firstSelected = index;
            GameManager.Instance.BoardManager.SetCellSelected(index, true);
        }
        // Chọn cell thứ hai → thử match
        else if (_secondSelected == -1)
        {
            _secondSelected = index;
            GameManager.Instance.BoardManager.SetCellSelected(index, true);
            
            if (_matchCoroutine != null) StopCoroutine( _matchCoroutine );
            _matchCoroutine = StartCoroutine(TryMatch());
        }
    }

    /// <summary>
    /// Match hai cell đã chọn
    /// </summary>
    private IEnumerator TryMatch()
    {
        yield return new WaitForSeconds(_chooseDelay);

        if (_firstSelected >= 0 && _secondSelected >= 0)
        {
            bool matched = GameManager.Instance.BoardManager.TryMatch(_firstSelected, _secondSelected);

            // Nếu không match, bỏ chọn cả hai
            if (!matched)
            {
                GameManager.Instance.BoardManager.SetCellSelected(_firstSelected, false);
                GameManager.Instance.BoardManager.SetCellSelected(_secondSelected, false);
            }
        }

        ResetSelectionState();
    }

    /// <summary>
    /// Reset trạng thái chọn về ban đầu.
    /// </summary>
    public void ResetSelectionState()
    {
        _firstSelected = -1;
        _secondSelected = -1;
    }
}