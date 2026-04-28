using System.Collections;
using UnityEngine;

public class CellMoveAnim : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private float moveDuration = 0.25f;
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine _runningCoroutine;

    private void Reset()
    {
        rectTransform = transform as RectTransform;
    }

    public void SetPositionInstant(Vector2 anchoredPosition)
    {
        if (_runningCoroutine != null)
        {
            StopCoroutine(_runningCoroutine);
            _runningCoroutine = null;
        }

        rectTransform.anchoredPosition = anchoredPosition;
    }

    public void MoveTo(Vector2 targetAnchoredPos, float durationOverride = -1f)
    {
        if (_runningCoroutine != null) StopCoroutine(_runningCoroutine);
        _runningCoroutine = StartCoroutine(CoMoveTo(targetAnchoredPos, durationOverride));
    }

    private IEnumerator CoMoveTo(Vector2 targetAnchoredPos, float durationOverride)
    {
        float duration = durationOverride > 0f ? durationOverride : moveDuration;

        Vector2 start = rectTransform.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curveT = moveCurve.Evaluate(t);

            rectTransform.anchoredPosition = Vector2.LerpUnclamped(start, targetAnchoredPos, curveT);
            yield return null;
        }

        rectTransform.anchoredPosition = targetAnchoredPos;
        _runningCoroutine = null;
    }
}