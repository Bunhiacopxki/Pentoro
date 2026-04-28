using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Quản lý toàn bộ trạng thái board game: tạo board, xử lý matching, collapse, thêm số,
/// kiểm tra win/lose, và quản lý gem goals.
/// </summary>
public class BoardManager : MonoBehaviour
{
    [Header("Board Config")]
    [SerializeField] private int columns = 9;           // Số cột của board
    [SerializeField] private int visibleRows = 9;       // Số hàng hiển thị (dùng cho layout)
    [SerializeField] private RectTransform boardParent; // Container chính của các cell
    [SerializeField] private CellView cellPrefab;       // Prefab để instantiate cell

    [Header("Input")]
    [SerializeField] private InputHandler inputHandler;

    [Header("UI")]
    [SerializeField] private GameObject lossCanvas;
    [SerializeField] private GameObject winCanvas;
    [SerializeField] private TMP_Text winText;

    [Header("Animation")]
    [SerializeField] private float waitAfterRemove = 0.38f;      // Thời gian chờ sau khi xóa cặp match
    [SerializeField] private float collapseMoveDuration = 0.25f; // Thời gian di chuyển khi collapse
    [SerializeField] private AddNumAnim _addNumAnim;            // Animation preview khi thêm số

    [Header("Spawn Board")]
    [SerializeField] private float revealInterval = 0.04f;      // Khoảng cách thời gian giữa các lần reveal cell
    [SerializeField] private BoardFallbackLibrary fallbackLibrary; // Thư viện fallback khi generate board

    [Header("Gem")]
    [SerializeField] private GemGoalLibrary gemGoalLibrary; // Thư viện chứa các gem goal cho từng stage

    private readonly List<CellData> _cells = new List<CellData>();   // Dữ liệu logic của tất cả cell
    private readonly List<CellView> _views = new List<CellView>();   // View tương ứng với từng cell

    private bool _isResolving;    // Đang xử lý matching/collapse (không cho click)
    private bool _isClicking;       // Cờ tạm để block input
    private bool _isAddingNumber;   // Đang trong quá trình thêm số mới

    private Coroutine _removeCoroutine;  // Reference đến coroutine xử lý match hiện tại

    private BoardGenerator _boardGenerator;           // Khởi tạo board
    private BoardAddNumberService _addNumberService;  // Logic thêm số
    private BoardCollapseService _collapseService;    // Logic xóa hàng khi match hết
    private GemSpawnService _gemSpawnService;         // Gắn gem vào các cell

    private BoardGridLayout _layout;              // Tính toán vị trí anchor cho cell view
    private BoardViewPool _viewPool;                   // Quản lý việc tạo/tái sử dụng cell view
    private BoardRevealController _revealController;   // Điều khiển animation reveal cell
    private BoardPairResolveController _pairResolveController; // Xử lý logic match + collapse
    private BoardStageSnapshotService _snapshotService;      // Lưu/trả lại trạng thái board

    private GemSpawnContext _gemContext;     // Context chứa thông tin gem goals của stage hiện tại
    private StageSnapshot _stageStartSnapshot; // Snapshot lưu trạng thái ban đầu của stage (dùng cho retry)

    public event System.Action<int> OnCellRemoved;              // Event khi một cell bị xóa
    public event System.Action<int, int> OnPairMatched;           // Event khi một 2 cell match thành công
    public event System.Action<BoardCollapsePlan> OnCollapseStarted; // Event khi bắt đầu collapse
    public event System.Action<GemType, int> OnUpdateGem;          // Event khi gem được thu thập

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
        // Khởi tạo các service objects
        _boardGenerator = new BoardGenerator(fallbackLibrary);
        _addNumberService = new BoardAddNumberService();
        _collapseService = new BoardCollapseService();
        _gemSpawnService = new GemSpawnService();

        // Khởi tạo layout và view pool
        _layout = new BoardGridLayout(boardParent, columns, visibleRows);

        _viewPool = new BoardViewPool(
            _views,
            cellPrefab,
            boardParent,
            inputHandler,
            _layout
        );

        // Khởi tạo controllers
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
        
        // Setup audio cho input handler
        GameManager.Instance.AudioManager.Binder.SetUp(inputHandler);

        // Ẩn canvas win/loss ban đầu
        lossCanvas.SetActive(false);
        winCanvas.SetActive(false);
    }

    /// <summary>
    /// Tạo board ban đầu cho level mới: sinh cell data, gắn gem, reveal lần lượt.
    /// </summary>
    public void GenerateInitialBoard()
    {
        _revealController.Stop();

        _cells.Clear();
        _isResolving = false;

        // Lấy gem context cho stage hiện tại
        int stage = GameManager.Instance.StageManager.CurrentStage;
        _gemContext = gemGoalLibrary.CreateContext(stage);

        // Sinh dữ liệu các cell
        List<CellData> generatedCells = _boardGenerator.Generate(_gemContext);
        _cells.AddRange(generatedCells);

        RefreshIndexes();

        // Tạo danh sách index để gắn gem (gắn cho tất cả cell ban đầu)
        List<int> indexes = CreateIndexList(0, _cells.Count);

        _gemSpawnService.AttachGems(_cells, indexes, _gemContext);

        // Lưu snapshot để có thể retry
        SaveStageStartSnapshot();

        // Chuẩn bị view và reveal
        _viewPool.PrepareRevealView(_cells);
        _revealController.Reveal(indexes);
    }

    /// <summary>
    /// Kiểm tra xem một cell có thể được chọn hay không.
    /// </summary>
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

    /// <summary>
    /// Kiểm tra điều kiện match: valid, khác nhau, cùng giá trị và có đường đi không bị chắn.
    /// Nếu thành công, bắt đầu coroutine xử lý match + collapse.
    /// </summary>
    public bool TryMatch(int indexA, int indexB)
    {
        if (_isResolving) return false;

        // Kiểm tra valid
        if (!IsValidSelectable(indexA) ||
            !IsValidSelectable(indexB) ||
            indexA == indexB)
        {
            return false;
        }

        CellData a = _cells[indexA];
        CellData b = _cells[indexB];

        // Kiểm tra giá trị có match không
        if (!BoardRules.IsMatchValue(a.Value, b.Value))
            return false;

        // Kiểm tra có đường đi giữa hai cell không
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

        // Bắt đầu xử lý match
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

    /// <summary>
    /// Thêm các số mới vào board.
    /// Chỉ thêm khi không đang resolving và không đang thêm số.
    /// </summary>
    public void AddNumbers()
    {
        if (_isResolving || _isAddingNumber) return;

        StartCoroutine(CoAddNumbers());
    }

    private IEnumerator CoAddNumbers()
    {
        _isAddingNumber = true;

        // Chạy animation preview cho các cell còn sống trước khi thêm
        if (_addNumAnim != null)
        {
            yield return _addNumAnim.PlayAliveCellsPreview(
                _cells,
                _views
            );
        }

        int startIndex = _cells.Count;

        // Tạo các cell mới
        List<CellData> appendedCells =
            _addNumberService.CreateAppendedCells(_cells, startIndex);

        if (appendedCells.Count == 0)
        {
            _isAddingNumber = false;
            yield break;
        }

        _cells.AddRange(appendedCells);
        RefreshIndexes();

        // Gắn gem cho các cell mới
        List<int> appendedIndexes = CreateIndexList(startIndex, _cells.Count);

        _gemSpawnService.AttachGems(_cells, appendedIndexes, _gemContext);

        // Tạo view và reveal
        _viewPool.PrepareNewViews(_cells, startIndex);
        _revealController.Reveal(appendedIndexes);

        _isAddingNumber = false;
    }

    /// <summary>
    /// Đánh giá trạng thái board: kiểm tra win/lose.
    /// </summary>
    public void EvaluateBoardState()
    {
        // Kiểm tra win: thu thập đủ gem
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
        // Kiểm tra lose: không còn cặp match nào và không còn lượt thêm số
        else if (CheckLoseCondition())
        {
            GameManager.Instance.showObject(lossCanvas);
            Debug.Log("Loss");
        }
    }

    /// <summary>
    /// Thu thập gem khi match cell.
    /// Cập nhật số lượng gem đã thu và giảm số lượng gem cần spawn.
    /// </summary>
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

            // Cập nhật số lượng đã thu thập
            if (goal.collectedCount < goal.targetCount)
            {
                goal.collectedCount++;

                // Thông báo số gem còn lại cần thu
                OnUpdateGem?.Invoke(
                    cell.GemType,
                    goal.targetCount - goal.collectedCount
                );

                // Giảm số gem cần spawn (vì đã thu được một gem)
                if (goal.spawnedCount > 0)
                    goal.spawnedCount--;

                cell.HasGem = false;
                cell.GemType = GemType.None;
            }

            break;
        }
    }

    /// <summary>
    /// Kiểm tra điều kiện thua: không còn cặp match nào và không còn lượt thêm số.
    /// </summary>
    private bool CheckLoseCondition()
    {
        // Nếu đã win rồi thì không thua
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

    /// <summary>
    /// Cập nhật lại index cho tất cả cell (sau khi thêm/xóa cell).
    /// </summary>
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

    /// <summary>
    /// Lưu snapshot trạng thái ban đầu của stage để có thể retry.
    /// </summary>
    private void SaveStageStartSnapshot()
    {
        _stageStartSnapshot = _snapshotService.Save(
            _cells,
            _gemContext,
            GameManager.Instance.StageManager.CurrentStage,
            GameManager.Instance.AddManager.TimesOfAdd
        );
    }

    /// <summary>
    /// Retry stage hiện tại: khôi phục lại trạng thái từ snapshot đã lưu.
    /// </summary>
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

        // Khôi phục dữ liệu
        _snapshotService.RestoreCells(_stageStartSnapshot, _cells);
        _gemContext = _snapshotService.RestoreGemContext(_stageStartSnapshot);

        GameManager.Instance.AddManager.SetRemainingTurns(
            _stageStartSnapshot.remainingAddTurns
        );

        // Reset stage về ban đầu
        GameManager.Instance.StageManager.StageUp.Invoke(
            _stageStartSnapshot.stage
        );

        if (inputHandler != null)
            inputHandler.ResetSelectionState();

        RefreshIndexes();

        // Reveal lại tất cả
        List<int> indexes = CreateIndexList(0, _cells.Count);

        _viewPool.PrepareRevealView(_cells);

        GameManager.Instance.hideObject(lossCanvas);

        _revealController.Reveal(indexes);
    }

    /// <summary>
    /// Chuyển sang level tiếp theo hoặc reset về level 1 nếu đã hoàn thành tất cả các level.
    /// </summary>
    public void HandleNextLevel()
    {
        // Reset số lượt thêm số
        GameManager.Instance.AddManager.ResetAddTurns();
        
        // Chuyển stage hoặc reset về stage 1
        if (GameManager.Instance.StageManager.CurrentStage < 3)
        {
            GameManager.Instance.StageManager.AdvanceStage();
        }
        else
        {
            GameManager.Instance.StageManager.ResetStage();
        }
        GameManager.Instance.hideObject(winCanvas);

        // Tạo board mới
        GenerateInitialBoard();
    }

    /// <summary>
    /// Tạo danh sách index liên tiếp từ start đến end-1.
    /// </summary>
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