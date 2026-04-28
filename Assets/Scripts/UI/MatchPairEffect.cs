using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MatchPairEffect : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform lineRect;
    [SerializeField] private Image lineImage;

    [Header("Line")]
    [SerializeField] private float thickness = 8f;
    [SerializeField] private float duration = 0.35f;
    [SerializeField] private AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private Coroutine _runningCoroutine;

    private void Awake()
    {
        HideLine();
    }

    private void Start()
    {
        if (GameManager.Instance == null || GameManager.Instance.BoardManager == null) return;
        GameManager.Instance.BoardManager.OnPairMatched += HandlePairMatched;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance.BoardManager != null)
        {
            GameManager.Instance.BoardManager.OnPairMatched -= HandlePairMatched;
        }
    }

    private void HandlePairMatched(int indexA, int indexB)
    {
        if (lineRect == null || lineImage == null)
            return;

        CellView a = GameManager.Instance.BoardManager.GetCellView(indexA);
        CellView b = GameManager.Instance.BoardManager.GetCellView(indexB);

        if (a == null || b == null)
            return;

        if (_runningCoroutine != null)
            StopCoroutine(_runningCoroutine);

        _runningCoroutine = StartCoroutine(CoPlay(a, b));
    }

    private IEnumerator CoPlay(CellView a, CellView b)
    {
        Vector3 start = a.GetWorldCenter();
        Vector3 end = b.GetWorldCenter();

        ShowLine(start, end);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            SetAlpha(alphaCurve.Evaluate(t));
            yield return null;
        }

        HideLine();
        _runningCoroutine = null;
    }

    private void ShowLine(Vector3 start, Vector3 end)
    {
        Vector2 dir = end - start;
        float length = dir.magnitude;

        lineRect.gameObject.SetActive(true);
        lineRect.position = (start + end) * 0.5f;
        lineRect.sizeDelta = new Vector2(length, thickness);

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        lineRect.rotation = Quaternion.Euler(0f, 0f, angle);

        SetAlpha(alphaCurve.Evaluate(0f));
    }

    private void SetAlpha(float alpha)
    {
        Color c = lineImage.color;
        c.a = alpha;
        lineImage.color = c;
    }

    private void HideLine()
    {
        if (lineRect != null)
            lineRect.gameObject.SetActive(false);
    }
}