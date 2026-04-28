using UnityEngine;

public class UIController : MonoBehaviour
{
    private void Start()
    {
        GameManager.Instance.OnShowCanvas += ShowCanvas;
        GameManager.Instance.OnHideCanvas += HideCanvas;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnShowCanvas -= ShowCanvas;
        GameManager.Instance.OnHideCanvas -= HideCanvas;
    }

    public void ShowCanvas(GameObject gameObject)
    {
        gameObject.SetActive(true);
    }

    public void HideCanvas(GameObject gameObject)
    {
        gameObject.SetActive(false);
    }
}
