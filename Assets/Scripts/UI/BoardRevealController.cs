using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Điều khiển animation reveal cell: lần lượt hiển thị các cell với khoảng cách thời gian.
/// </summary>
public class BoardRevealController
{
    private readonly MonoBehaviour _runner;      // Đối tượng để chạy coroutine
    private readonly List<CellView> _views;      // Danh sách view
    private readonly float _revealInterval;      // Khoảng cách thời gian giữa các lần reveal

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

    /// <summary>
    /// Dừng reveal đang chạy
    /// </summary>
    public void Stop()
    {
        if (_revealCoroutine != null)
        {
            _runner.StopCoroutine(_revealCoroutine);
            _revealCoroutine = null;
        }

        IsRevealing = false;
    }

    /// <summary>
    /// Bắt đầu reveal các cell theo danh sách index.
    /// </summary>
    public void Reveal(IReadOnlyList<int> indexes)
    {
        Stop();
        _revealCoroutine = _runner.StartCoroutine(CoRevealViews(indexes));
    }

    /// <summary>
    /// Lần lượt active và chạy animation cho từng cell.
    /// </summary>
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