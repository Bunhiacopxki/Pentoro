using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSfx : MonoBehaviour
{
    [SerializeField] private SfxType sfxType = SfxType.Pop2;

    [SerializeField] private Button _button;

    private void OnEnable()
    {
        _button.onClick.AddListener(PlayButtonSfx);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(PlayButtonSfx);
    }

    private void PlayButtonSfx()
    {
        GameManager.Instance.AudioManager.PlaySfx(sfxType);
    }
}