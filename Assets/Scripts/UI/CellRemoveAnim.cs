using System.Collections;
using UnityEngine;

public class CellRemoveAnim : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private RectTransform visualRoot;

    [Header("Animation")]
    [SerializeField] private float removeDuration = 0.5f;
    [SerializeField] private float clickDuration = 0.5f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine _runningCoroutine;

    public void PlayRemove()
    {
        if (visualRoot == null) return;

        if (_runningCoroutine != null)
            StopCoroutine(_runningCoroutine);

        _runningCoroutine = StartCoroutine(CoPlayRemove());
    }

    public void SetHiddenInstant()
    {
        if (visualRoot == null) return;

        if (_runningCoroutine != null)
        {
            StopCoroutine(_runningCoroutine);
            _runningCoroutine = null;
        }

        visualRoot.localScale = Vector3.zero;
        visualRoot.gameObject.SetActive(false);
    }

    private IEnumerator CoPlayRemove()
    {
        visualRoot.gameObject.SetActive(true);

        Vector3 startScale = Vector3.one;
        Vector3 endScale = Vector3.zero;

        float elapsed = 0f;

        while (elapsed < removeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / removeDuration);
            float curveT = scaleCurve.Evaluate(t);

            visualRoot.localScale = Vector3.LerpUnclamped(startScale, endScale, curveT);
            yield return null;
        }

        SetHiddenInstant();
    }

    public void PlayClick()
    {
        if (visualRoot == null) return;

        if (_runningCoroutine != null)
            StopCoroutine(_runningCoroutine);

        _runningCoroutine = StartCoroutine(CoPlayClick());
    }

    private IEnumerator CoPlayClick()
    {
        GameManager.Instance.BoardManager.IsClicking = true;
        visualRoot.gameObject.SetActive(true);

        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one;

        float elapsed = 0f;

        while (elapsed < clickDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / clickDuration);
            float curveT = scaleCurve.Evaluate(t);

            visualRoot.localScale = Vector3.LerpUnclamped(startScale, endScale, curveT);
            yield return null;
        }
        GameManager.Instance.BoardManager.IsClicking = false;
        SetHiddenInstant();
    }
}