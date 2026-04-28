using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AddView : MonoBehaviour
{
    [Header("Add Num Config")]
    [SerializeField] private TMP_Text _times;
    [SerializeField] private Button _addButton;
    [SerializeField] private Image _background;

    [Header("Click Feedback")]
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _pressedColor = Color.gray;
    [SerializeField] private float _pressedScale = 0.9f;
    [SerializeField] private float _pressTime = 0.08f;
    [SerializeField] private float _returnTime = 0.08f;

    private Vector3 _defaultScale;
    private Coroutine _feedbackCoroutine;

    public Button AddButton => _addButton;

    private void Awake()
    {
        _defaultScale = transform.localScale;
        _background.color = _normalColor;
    }

    public void ChangeTimesValue(int times)
    {
        _times.text = times.ToString();
    }

    public void PlayClickFeedback()
    {
        if (_feedbackCoroutine != null)
            StopCoroutine(_feedbackCoroutine);

        _feedbackCoroutine = StartCoroutine(CoPlayClickFeedback());
    }

    private IEnumerator CoPlayClickFeedback()
    {
        Vector3 pressedScale = _defaultScale * _pressedScale;

        _background.color = _pressedColor;
        yield return ScaleTo(pressedScale, _pressTime);

        _background.color = _normalColor;
        yield return ScaleTo(_defaultScale, _returnTime);

        _feedbackCoroutine = null;
    }

    private IEnumerator ScaleTo(Vector3 targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        float time = 0f;

        if (duration <= 0f)
        {
            transform.localScale = targetScale;
            yield break;
        }

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        transform.localScale = targetScale;
    }
}