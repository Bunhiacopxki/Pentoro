using UnityEngine;
using UnityEngine.UI;

public class RetryHandle : MonoBehaviour
{
    [SerializeField] private Button RetryButton;

    private void Start()
    {
        RetryButton.onClick.AddListener(GameManager.Instance.BoardManager.RetryStage);
    }

    private void OnDestroy()
    {
        RetryButton.onClick.RemoveListener(GameManager.Instance.BoardManager.RetryStage);
    }
}
