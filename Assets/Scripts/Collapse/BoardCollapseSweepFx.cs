using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BoardCollapseSweepFx : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform effectRoot;
    [SerializeField] private SweepFxView sweepFxPrefab;

    [Header("Anim")]
    [SerializeField] private float sweepDuration = 0.5f;
    [SerializeField] private float delayPerRow = 0.01f;

    [Header("Trail")]
    [SerializeField] private float trailStartLength = 10f;
    [SerializeField] private float trailFinalLength = 90f;
    [SerializeField] private float trailThickness = 24f;
    [SerializeField] private float trailGrowDuration = 0.08f;
    [SerializeField] private float trailAlpha = 0.75f;

    private BoardManager _boardManager;

    private void Start()
    {
        _boardManager = GameManager.Instance.BoardManager;
        if (_boardManager != null)
            _boardManager.OnCollapseStarted += HandleCollapseStarted;
    }

    private void OnDestroy()
    {
        if (_boardManager != null)
            _boardManager.OnCollapseStarted -= HandleCollapseStarted;
    }

    private void HandleCollapseStarted(BoardCollapsePlan plan)
    {
        for (int i = 0; i < plan.CompletedRows.Count; i++)
        {
            StartCoroutine(CoPlaySweep(plan.CompletedRows[i], i * delayPerRow));
        }
    }

    private IEnumerator CoPlaySweep(int row, float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        if (_boardManager == null || sweepFxPrefab == null || effectRoot == null)
            yield break;

        int columns = _boardManager.Columns;
        int startIndex = row * columns;
        int endIndex = startIndex + columns - 1;

        Vector2 from = _boardManager.GetAnchoredPositionByIndex(startIndex);
        Vector2 to = _boardManager.GetAnchoredPositionByIndex(endIndex);

        SweepFxView fx = Instantiate(sweepFxPrefab, effectRoot);

        RectTransform fxRoot = fx.Root;
        Image head = fx.HeadImage;
        Image trail = fx.TrailImage;
        RectTransform trailRt = fx.TrailRect;

        if (fxRoot == null || head == null || trail == null || trailRt == null)
        {
            Destroy(fx.gameObject);
            yield break;
        }

        fxRoot.anchorMin = new Vector2(0.5f, 0.5f);
        fxRoot.anchorMax = new Vector2(0.5f, 0.5f);
        fxRoot.pivot = new Vector2(0.5f, 0.5f);
        fxRoot.localScale = Vector3.one;
        fxRoot.anchoredPosition = from;
        fxRoot.SetAsLastSibling();

        trailRt.anchorMin = new Vector2(0.5f, 0.5f);
        trailRt.anchorMax = new Vector2(0.5f, 0.5f);
        trailRt.pivot = new Vector2(1f, 0.5f);

        trailRt.sizeDelta = new Vector2(trailStartLength, trailThickness);

        Color headColor = head.color;
        headColor.a = 1f;
        head.color = headColor;

        Color trailColor = trail.color;
        trailColor.a = trailAlpha;
        trail.color = trailColor;

        float time = 0f;

        while (time < sweepDuration)
        {
            time += Time.deltaTime;

            float moveT = Mathf.Clamp01(time / sweepDuration);
            moveT = Mathf.SmoothStep(0f, 1f, moveT);
            fxRoot.anchoredPosition = Vector2.Lerp(from, to, moveT);

            float currentTrailLength;
            if (time < trailGrowDuration)
            {
                float growT = Mathf.Clamp01(time / trailGrowDuration);
                growT = Mathf.SmoothStep(0f, 1f, growT);
                currentTrailLength = Mathf.Lerp(trailStartLength, trailFinalLength, growT);
            }
            else
            {
                currentTrailLength = trailFinalLength;
            }

            trailRt.sizeDelta = new Vector2(currentTrailLength, trailThickness);

            float fadeT = Mathf.InverseLerp(sweepDuration * 0.7f, sweepDuration, time);

            Color hc = head.color;
            hc.a = Mathf.Lerp(1f, 0f, fadeT);
            head.color = hc;

            Color tc = trail.color;
            tc.a = Mathf.Lerp(trailAlpha, 0f, fadeT);
            trail.color = tc;

            yield return null;
        }

        fxRoot.anchoredPosition = to;
        Destroy(fx.gameObject);
    }
}