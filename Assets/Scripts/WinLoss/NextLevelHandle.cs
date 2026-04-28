using UnityEngine;
using UnityEngine.UI;

public class NextLevelHandle : MonoBehaviour
{
    [SerializeField] private Button NextLevelButton;

    private void Start()
    {
        NextLevelButton.onClick.AddListener(GameManager.Instance.BoardManager.HandleNextLevel);
    }

    private void OnDestroy()
    {
        NextLevelButton.onClick.RemoveListener(GameManager.Instance.BoardManager.HandleNextLevel);
    }
}
