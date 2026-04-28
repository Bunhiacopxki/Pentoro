using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchPairEffect : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform lineRect; // line để hiển thị hiệu ứng

    [Header("Line")]
    [SerializeField] private float thickness = 8f; // Độ dày của line.
    [SerializeField] private float duration = 0.35f; // Thời gian hiệu ứng line tồn tại trước khi biến mất.
    [SerializeField] private AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0); // Curve điều khiển alpha của line theo thời gian.

    private readonly List<RectTransform> _segments = new List<RectTransform>(); // Danh sách các đoạn line đang được quản lý.
    private Coroutine _runningCoroutine;

    private void Awake()
    {
        if (lineRect != null)
        {
            lineRect.gameObject.SetActive(false);
            _segments.Add(lineRect);
        }
    }

    private void Start()
    {
        if (GameManager.Instance == null || GameManager.Instance.BoardManager == null) return;
        GameManager.Instance.BoardManager.OnPairMatched += HandlePairMatched;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.BoardManager != null)
        {
            GameManager.Instance.BoardManager.OnPairMatched -= HandlePairMatched;
        }
    }

    private void HandlePairMatched(int indexA, int indexB)
    {
        if (lineRect == null)
            return;

        // Lấy CellView tương ứng với hai index để biết vị trí thật trên UI.
        CellView a = GameManager.Instance.BoardManager.GetCellView(indexA);
        CellView b = GameManager.Instance.BoardManager.GetCellView(indexB);

        if (a == null || b == null)
            return;

        if (_runningCoroutine != null)
            StopCoroutine(_runningCoroutine);

        _runningCoroutine = StartCoroutine(CoPlay(indexA, indexB, a, b));
    }

    /// <summary>
    /// Phát hiệu ứng line.
    /// </summary>
    private IEnumerator CoPlay(int indexA, int indexB, CellView a, CellView b)
    {
        // Ẩn toàn bộ line cũ.
        HideAllSegments();

        BoardManager board = GameManager.Instance.BoardManager;
        int columns = board.Columns;

        if (IsStraightOrDiagonal(indexA, indexB, columns))
        {
            // Nếu là ngang / dọc / chéo thì chỉ cần một line nối tâm hai ô.
            ShowSegment(0, a.GetWorldCenter(), b.GetWorldCenter());
        }
        else
        {
            // Nếu match theo mảng 1 chiều qua nhiều hàng, vẽ thành 2 đoạn: cuối hàng trên và đầu hàng dưới.
            ShowWrappedSegments(indexA, indexB, columns);
        }

        // Fade line theo alphaCurve.
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            SetAllAlpha(alphaCurve.Evaluate(t));
            yield return null;
        }

        HideAllSegments();
        _runningCoroutine = null;
    }

    /// <summary>
    /// Kiểm tra hai index có thuộc case nối thẳng hay không.
    /// </summary>
    private bool IsStraightOrDiagonal(int indexA, int indexB, int columns)
    {
        int rowA = indexA / columns;
        int colA = indexA % columns;

        int rowB = indexB / columns;
        int colB = indexB % columns;

        if (rowA == rowB) return true;
        if (colA == colB) return true;
        if (Mathf.Abs(rowA - rowB) == Mathf.Abs(colA - colB)) return true;

        return false;
    }

    /// <summary>
    /// Hiển thị line cho trường hợp match theo mảng 1 chiều.
    /// </summary>
    private void ShowWrappedSegments(int indexA, int indexB, int columns)
    {
        BoardManager board = GameManager.Instance.BoardManager;

        // Xử lý theo chiều index tăng dần
        int firstIndex = Mathf.Min(indexA, indexB);
        int secondIndex = Mathf.Max(indexA, indexB);

        CellView first = board.GetCellView(firstIndex);
        CellView second = board.GetCellView(secondIndex);

        if (first == null || second == null)
            return;

        int firstRow = firstIndex / columns;
        int secondRow = secondIndex / columns;

        // Cell cuối cùng của hàng chứa firstIndex.
        int firstRowRightIndex = firstRow * columns + (columns - 1);
        // Cell đầu tiên của hàng chứa secondIndex.
        int secondRowLeftIndex = secondRow * columns;

        CellView firstRowRightCell = board.GetCellView(firstRowRightIndex);
        CellView secondRowLeftCell = board.GetCellView(secondRowLeftIndex);

        if (firstRowRightCell == null || secondRowLeftCell == null)
            return;

        // Từ tâm ô đầu tiên đến mép phải của hàng đó.
        Vector3 firstStart = first.GetWorldCenter();
        Vector3 firstEnd = firstRowRightCell.GetWorldRightCenter();
        // Từ mép trái của hàng dưới đến tâm ô thứ hai.
        Vector3 secondStart = secondRowLeftCell.GetWorldLeftCenter();
        Vector3 secondEnd = second.GetWorldCenter();

        ShowSegment(0, firstStart, firstEnd);
        ShowSegment(1, secondStart, secondEnd);
    }

    /// <summary>
    /// Hiển thị một đoạn line từ start đến end.
    /// </summary>
    private void ShowSegment(int segmentIndex, Vector3 start, Vector3 end)
    {
        RectTransform segment = GetSegment(segmentIndex);

        Vector2 dir = end - start;
        float length = dir.magnitude;

        segment.gameObject.SetActive(true);

        // Đặt line nằm giữa hai điểm.
        segment.position = (start + end) * 0.5f;
        segment.sizeDelta = new Vector2(length, thickness);

        // Tính góc xoay để line hướng từ start đến end.
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        segment.rotation = Quaternion.Euler(0f, 0f, angle);

        // Set alpha ban đầu theo curve tại thời điểm 0.
        SetAlpha(segment, alphaCurve.Evaluate(0f));
    }

    /// <summary>
    /// Lấy segment theo index.
    /// </summary>
    private RectTransform GetSegment(int index)
    {
        while (_segments.Count <= index)
        {
            RectTransform newSegment = Instantiate(lineRect, lineRect.parent);
            newSegment.gameObject.SetActive(false);
            _segments.Add(newSegment);
        }

        return _segments[index];
    }

    /// <summary>
    /// Set alpha cho toàn bộ line segment hiện có.
    /// </summary>
    private void SetAllAlpha(float alpha)
    {
        for (int i = 0; i < _segments.Count; i++)
        {
            SetAlpha(_segments[i], alpha);
        }
    }

    /// <summary>
    /// Set alpha cho một line segment.
    /// </summary>
    private void SetAlpha(RectTransform segment, float alpha)
    {
        Image image = segment.GetComponent<Image>();
        if (image == null) return;

        Color c = image.color;
        c.a = alpha;
        image.color = c;
    }

    /// <summary>
    /// Ẩn toàn bộ line segment.
    /// </summary>
    private void HideAllSegments()
    {
        for (int i = 0; i < _segments.Count; i++)
        {
            if (_segments[i] != null)
                _segments[i].gameObject.SetActive(false);
        }
    }
}