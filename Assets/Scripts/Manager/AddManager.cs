using UnityEngine;

public class AddManager : MonoBehaviour
{
    [SerializeField] private AddView _addView;
    [SerializeField] private int _timesOfAdd = 10;

    private int _defaultTimesOfAdd;

    public int TimesOfAdd => _timesOfAdd;
    public bool HasAddTurnsRemaining => _timesOfAdd > 0;

    private void Awake()
    {
        _defaultTimesOfAdd = _timesOfAdd;
        _addView.ChangeTimesValue(_timesOfAdd);
    }

    private void OnEnable()
    {
        _addView.AddButton.onClick.AddListener(AddNumHandle);
    }

    private void OnDisable()
    {
        _addView.AddButton.onClick.RemoveListener(AddNumHandle);
    }

    private void AddNumHandle()
    {
        if (_timesOfAdd <= 0 || GameManager.Instance.BoardManager.IsReveal) return;

        _addView.PlayClickFeedback();
        _addView.ChangeTimesValue(--_timesOfAdd);
        GameManager.Instance.BoardManager.AddNumbers();
        GameManager.Instance.BoardManager.EvaluateBoardState();
    }

    public void ResetAddTurns()
    {
        _timesOfAdd = _defaultTimesOfAdd;
        _addView.ChangeTimesValue(_timesOfAdd);
    }

    public void SetRemainingTurns(int turns)
    {
        _timesOfAdd = turns;
        _addView.ChangeTimesValue(_timesOfAdd);
    }
}
