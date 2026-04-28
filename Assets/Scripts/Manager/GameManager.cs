using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private BoardManager boardManager;
    [SerializeField] private StageManager stageManager;
    [SerializeField] private AddManager   addManager;
    [SerializeField] private AudioManager audioManager;

    public BoardManager BoardManager => boardManager;
    public StageManager StageManager => stageManager;
    public AddManager AddManager => addManager;
    public AudioManager AudioManager => audioManager;

    public event System.Action<GameObject> OnShowCanvas;
    public event System.Action<GameObject> OnHideCanvas;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        stageManager.ResetStage();
        boardManager.GenerateInitialBoard();
    }

    public void showObject(GameObject go)
    {
        OnShowCanvas.Invoke(go);
    }

    public void hideObject(GameObject go) { 
        OnHideCanvas.Invoke(go); 
    }
}