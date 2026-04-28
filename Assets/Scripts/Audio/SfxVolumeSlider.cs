using UnityEngine;
using UnityEngine.UI;

public class SfxVolumeSlider : MonoBehaviour
{
    [SerializeField] private SfxType sfxType;
    [SerializeField] private bool playPreviewWhenChanged = true;
    [SerializeField] private Slider _slider;

    private void Awake()
    {
        _slider.minValue = 0f;
        _slider.maxValue = 1f;
    }

    private void Start()
    {
        _slider.SetValueWithoutNotify(
            GameManager.Instance.AudioManager.GetSfxVolume(sfxType)
        );
    }

    private void OnEnable()
    {
        _slider.onValueChanged.AddListener(HandleValueChanged);
    }

    private void OnDisable()
    {
        if (_slider != null)
            _slider.onValueChanged.RemoveListener(HandleValueChanged);
    }

    private void HandleValueChanged(float value)
    {
        GameManager.Instance.AudioManager.SetSfxVolume(sfxType, value);

        if (playPreviewWhenChanged)
            GameManager.Instance.AudioManager.PlaySfx(sfxType);
    }
}