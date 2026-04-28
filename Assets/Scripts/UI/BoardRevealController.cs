using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardRevealController
{
    private readonly MonoBehaviour _runner;
    private readonly List<CellView> _views;
    private readonly float _revealInterval;

    private Coroutine _revealCoroutine;

    public bool IsRevealing { get; private set; }

    public BoardRevealController(
        MonoBehaviour runner,
        List<CellView> views,
        float revealInterval
    )
    {
        _runner = runner;
        _views = views;
        _revealInterval = revealInterval;
    }

    public void Stop()
    {
        if (_revealCoroutine != null)
        {
            _runner.StopCoroutine(_revealCoroutine);
            _revealCoroutine = null;
        }

        IsRevealing = false;
    }

    public void Reveal(IReadOnlyList<int> indexes)
    {
        Stop();
        _revealCoroutine = _runner.StartCoroutine(CoRevealViews(indexes));
    }

    private IEnumerator CoRevealViews(IReadOnlyList<int> indexes)
    {
        IsRevealing = true;

        for (int i = 0; i < indexes.Count; i++)
        {
            int index = indexes[i];
            if (index < 0 || index >= _views.Count) continue;

            _views[index].gameObject.SetActive(true);
            _views[index].ScaleDownAnim();

            yield return new WaitForSeconds(_revealInterval);
        }

        IsRevealing = false;
        _revealCoroutine = null;
    }
}