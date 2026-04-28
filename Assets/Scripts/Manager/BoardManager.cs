using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Board Config")]
    [SerializeField] private int columns = 9;
    [SerializeField] private int visibleRows = 9;
    [SerializeField] private RectTransform boardParent;
    [SerializeField] private CellView cellPrefab;

    [Header("Input")]
    [SerializeField] private InputHandler inputHandler;

    [Header("UI")]
    [SerializeField] private GameObject lossCanvas;
    [SerializeField] private GameObject winCanvas;
    [SerializeField] private TMP_Text winText;

    [Header("Animation")]
    [SerializeField] private float waitAfterRemove = 0.38f;
    [SerializeField] private float collapseMoveDuration = 0.25f;
    [SerializeField] private AddNumAnim _addNumAnim;

    [Header("Spawn Board")]
    [SerializeField] private float revealInterval = 0.04f;
    [SerializeField] private BoardFallbackLibrary fallbackLibrary;

    [Header("Gem")]
    [SerializeField] private GemGoalLibrary gemGoalLibrary;

    private readonly List<CellData> _cells = new List<CellData>();
    private readonly List<CellView> _views = new List<CellView>();

    private bool _isResolving;
    private bool _isClicking;
    private bool _isAddingNumber;

    private Coroutine _removeCoroutine;

    private BoardGenerator _boardGenerator;
    private BoardAddNumberService _addNumberService;
    private BoardCollapseService _collapseService;
    private GemSpawnService _gemSpawnService;

    private BoardGridLayout _layout;
    private BoardViewPool _viewPool;
    private BoardRevealController _revealController;
    private BoardPairResolveController _pairResolveController;
    private BoardStageSnapshotService _snapshotService;

    private GemSpawnContext _gemContext;
    private StageSnapshot _stageStartSnapshot;

    public event System.Action<int> OnCellRemoved;
    public event System.Action<int, int> OnPairMatched;
    public event System.Action<BoardCollapsePlan> OnCollapseStarted;
    public event System.Action<GemType, int> OnUpdateGem;

    public int Columns => columns;
    public int Rows => visibleRows;
    public int CellCount => _cells.Count;

    public bool IsResolving => _isResolving;
    public bool IsReveal => _revealController != null && _revealController.IsRevealing;

    public bool IsClicking
    {
        get { return _isClicking; }
        set { _isClicking = value; }
    }

    public GemGoalLibrary GemLibrary => gemGoalLibrary;

    private void Awake()
    {
        _boardGenerator = new BoardGenerator(fallbackLibrary);
        _addNumberService = new BoardAddNumberService();
        _collapseService = new BoardCollapseService();
        _gemSpawnService = new GemSpawnService();

        _layout = new BoardGridLayout(boardParent, columns, visibleRows);

        _viewPool = new BoardViewPool(
            _views,
            cellPrefab,
            boardParent,
            inputHandler,
            _layout
        );

        _revealController = new BoardRevealController(
            this,
            _views,
            revealInterval
        );

        _pairResolveController = new BoardPairResolveController(
            waitAfterRemove,
            collapseMoveDuration,
            columns,
            _collapseService
        );

        _snapshotService = new BoardStageSnapshotService();
        GameManager.Instance.AudioManager.Binder.SetUp(inputHandler);

        lossCanvas.SetActive(false);
        winCanvas.SetActive(false);
    }

    public void GenerateInitialBoard()
    {
        _revealController.Stop();

        _cells.Clear();
        _isResolving = false;

        int stage = GameManager.Instance.StageManager.CurrentStage;
        _gemContext = gemGoalLibrary.CreateContext(stage);

        List<CellData> generatedCells = _boardGenerator.Generate(_gemContext);
        _cells.AddRange(generatedCells);

        RefreshIndexes();

        List<int> indexes = CreateIndexList(0, _cells.Count);

        _gemSpawnService.AttachGems(_cells, indexes, _gemContext);

        SaveStageStartSnapshot();

        _viewPool.PrepareRevealView(_cells);
        _revealController.Reveal(indexes);
    }

    public bool IsValidSelectable(int index)
    {
        return index >= 0 &&
               index < _cells.Count &&
               !_cells[index].IsRemoved;
    }

    public void SetCellSelected(int index, bool selected)
    {
        _viewPool.SetSelected(index, selected);
    }

    public bool TryMatch(int indexA, int indexB)
    {
        if (_isResolving) return false;

        if (!IsValidSelectable(indexA) ||
            !IsValidSelectable(indexB) ||
            indexA == indexB)
        {
            return false;
        }

        CellData a = _cells[indexA];
        CellData b = _cells[indexB];

        if (!BoardRules.IsMatchValue(a.Value, b.Value))
            return false;

        bool pathClear = BoardRules.IsPathClear(
            indexA,
            indexB,
            columns,
            _cells.Count,
            idx => !_cells[idx].IsRemoved
        );

        if (!pathClear)
            return false;

        if (_removeCoroutine != null)
            StopCoroutine(_removeCoroutine);

        _isResolving = true;

        _removeCoroutine = StartCoroutine(
            _pairResolveController.ResolvePair(
                _cells,
                indexA,
                indexB,
                OnPairMatched,
                CollectGemIfAny,
                OnCellRemoved,
                OnCollapseStarted,
                RefreshIndexes,
                RebuildView,
                EvaluateBoardState,
                OnResolveCompleted
            )
        );

        return true;
    }

    private void OnResolveCompleted()
    {
        _isResolving = false;
        _removeCoroutine = null;
    }

    public void AddNumbers()
    {
        if (_isResolving || _isAddingNumber) return;

        StartCoroutine(CoAddNumbers());
    }

    private IEnumerator CoAddNumbers()
    {
        _isAddingNumber = true;

        if (_addNumAnim != null)
        {
            yield return _addNumAnim.PlayAliveCellsPreview(
                _cells,
                _views
            );
        }

        int startIndex = _cells.Count;

        List<CellData> appendedCells =
            _addNumberService.CreateAppendedCells(_cells, startIndex);

        if (appendedCells.Count == 0)
        {
            _isAddingNumber = false;
            yield break;
        }

        _cells.AddRange(appendedCells);
        RefreshIndexes();

        List<int> appendedIndexes = CreateIndexList(startIndex, _cells.Count);

        _gemSpawnService.AttachGems(_cells, appendedIndexes, _gemContext);

        _viewPool.PrepareNewViews(_cells, startIndex);
        _revealController.Reveal(appendedIndexes);

        _isAddingNumber = false;
    }

    public void EvaluateBoardState()
    {
        if (_gemContext != null && _gemContext.IsCompleted())
        {
            if (GameManager.Instance.StageManager.CurrentStage < 3)
            {
                winText.text = "NEXT LEVEL";
            }
            else
            {
                winText.text = "PLAY AGAIN";
            }
            GameManager.Instance.showObject(winCanvas);
            Debug.Log("Win");
        }
        else if (CheckLoseCondition())
        {
            GameManager.Instance.showObject(lossCanvas);
            Debug.Log("Loss");
        }
    }

    private void CollectGemIfAny(CellData cell)
    {
        if (cell == null) return;
        if (!cell.HasGem) return;
        if (_gemContext == null || _gemContext.Goals == null) return;

        for (int i = 0; i < _gemContext.Goals.Count; i++)
        {
            GemGoalEntry goal = _gemContext.Goals[i];

            if (goal.gemType != cell.GemType)
                continue;

            if (goal.collectedCount < goal.targetCount)
            {
                goal.collectedCount++;

                OnUpdateGem?.Invoke(
                    cell.GemType,
                    goal.targetCount - goal.collectedCount
                );

                if (goal.spawnedCount > 0)
                    goal.spawnedCount--;

                cell.HasGem = false;
                cell.GemType = GemType.None;
            }

            break;
        }
    }

    private bool CheckLoseCondition()
    {
        if (_gemContext != null && _gemContext.IsCompleted())
            return false;

        bool hasAnyMatch = BoardRules.HasAnyMatch(_cells, columns);
        bool hasAddMove = GameManager.Instance.AddManager.HasAddTurnsRemaining;

        return !hasAnyMatch && !hasAddMove;
    }

    public Vector2 GetAnchoredPositionByIndex(int index)
    {
        return _layout.GetAnchoredPositionByIndex(index);
    }

    public CellView GetCellView(int index)
    {
        return _viewPool.GetCellView(index);
    }

    private void RefreshIndexes()
    {
        for (int i = 0; i < _cells.Count; i++)
        {
            _cells[i].Index = i;
        }
    }

    private void RebuildView()
    {
        _viewPool.RebuildView(_cells);
    }

    private void SaveStageStartSnapshot()
    {
        _stageStartSnapshot = _snapshotService.Save(
            _cells,
            _gemContext,
            GameManager.Instance.StageManager.CurrentStage,
            GameManager.Instance.AddManager.TimesOfAdd
        );
    }

    public void RetryStage()
    {
        if (_stageStartSnapshot == null)
            return;

        _revealController.Stop();

        if (_removeCoroutine != null)
        {
            StopCoroutine(_removeCoroutine);
            _removeCoroutine = null;
        }

        _isResolving = false;

        _snapshotService.RestoreCells(_stageStartSnapshot, _cells);
        _gemContext = _snapshotService.RestoreGemContext(_stageStartSnapshot);

        GameManager.Instance.AddManager.SetRemainingTurns(
            _stageStartSnapshot.remainingAddTurns
        );

        GameManager.Instance.StageManager.StageUp.Invoke(
            _stageStartSnapshot.stage
        );

        if (inputHandler != null)
            inputHandler.ResetSelectionState();

        RefreshIndexes();

        List<int> indexes = CreateIndexList(0, _cells.Count);

        _viewPool.PrepareRevealView(_cells);

        GameManager.Instance.hideObject(lossCanvas);

        _revealController.Reveal(indexes);
    }

    public void HandleNextLevel()
    {
        GameManager.Instance.AddManager.ResetAddTurns();
        if (GameManager.Instance.StageManager.CurrentStage < 3)
        {
            GameManager.Instance.StageManager.AdvanceStage();
        }
        else
        {
            GameManager.Instance.StageManager.ResetStage();
        }
        GameManager.Instance.hideObject(winCanvas);

        GenerateInitialBoard();
    }

    private List<int> CreateIndexList(int startInclusive, int endExclusive)
    {
        List<int> indexes = new List<int>();

        for (int i = startInclusive; i < endExclusive; i++)
        {
            indexes.Add(i);
        }

        return indexes;
    }
}