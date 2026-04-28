using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingUI : MonoBehaviour
{
    [SerializeField] private Button _settingButton;
    [SerializeField] private GameObject _settingCanvas;
    [SerializeField] private Button _closeSetting;

    private void Awake()
    {
        _settingCanvas.SetActive(false);
    }

    private void OnEnable()
    {
        _settingButton?.onClick.AddListener(Open);
        _closeSetting?.onClick.AddListener(Close);
    }

    private void OnDisable()
    {
        _settingButton?.onClick.RemoveListener(Open);
        _closeSetting?.onClick.RemoveListener(Close);
    }

    private void Open()
    {
        GameManager.Instance.showObject(_settingCanvas);
    }

    private void Close()
    {
        GameManager.Instance.hideObject(_settingCanvas);
    }
}
