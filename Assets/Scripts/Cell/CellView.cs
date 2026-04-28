using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// View của một cell: hiển thị số, xử lý click, animation, hiển thị gem.
/// </summary>
public class CellView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button button;              // Button để click
    [SerializeField] private Image background;           // Ảnh nền để chạy anim
    [SerializeField] private Image valueImage;           // Ảnh hiển thị số

    [Header("Data")]
    [SerializeField] private NumberSpriteLibrary numberSpriteLibrary; // Thư viện sprite số

    [Header("Animation")]
    [SerializeField] private CellRemoveAnim removeAnim; // Animation khi xóa
    [SerializeField] private CellMoveAnim moveAnim;     // Animation di chuyển

    [Header("Background Colors")]
    [SerializeField] private Color normalBackgroundColor = Color.white;
    [SerializeField] private Color selectedBackgroundColor;

    [Header("Text Colors")]
    [SerializeField] private Color normalTextColor = Color.white;
    [SerializeField] private Color removedTextColor;

    [Header("Gem")]
    [SerializeField] private Image gemImage;            // Ảnh gem hiển thị trên cell
    [SerializeField] private GemSpriteLibrary gemSpriteLibrary;

    [Header("Add Number Preview")]
    [SerializeField] private Image _circleHighlight;    // Animation preview khi thêm số
    [SerializeField] private float _drawTime = 0.25f;   // Thời gian vẽ circle
    [SerializeField] private float _holdTime = 0.15f;   // Thời gian giữ circle
    [SerializeField] private float _fadeTime = 0.12f;   // Thời gian fade out

    private int _index;
    private InputHandler _inputHandler;

    private bool _isSelected;
    private bool _isRemoved;
    private bool _isGem;

    private Coroutine _clickCoroutine;

    public RectTransform RectTransform => transform as RectTransform;

    private void OnEnable()
    {
        if (GameManager.Instance != null && GameManager.Instance.BoardManager != null)
        {
            GameManager.Instance.BoardManager.OnCellRemoved += HandleCellRemoved;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null && GameManager.Instance.BoardManager != null)
        {
            GameManager.Instance.BoardManager.OnCellRemoved -= HandleCellRemoved;
        }
    }

    private void Awake()
    {
        HideAddPreviewCircleInstant();
    }

    /// <summary>
    /// Ẩn preview circle ngay lập tức.
    /// </summary>
    public void HideAddPreviewCircleInstant()
    {
        if (_circleHighlight == null) return;

        _circleHighlight.gameObject.SetActive(false);
        _circleHighlight.fillAmount = 0f;

        Color c = _circleHighlight.color;
        c.a = 1f;
        _circleHighlight.color = c;
    }

    /// <summary>
    /// Setup view với dữ liệu cell.
    /// </summary>
    public void Setup(CellData data, int index, InputHandler inputHandler)
    {
        _index = index;
        _inputHandler = inputHandler;
        _isRemoved = data.IsRemoved;
        _isSelected = false;
        _isGem = data.GemType != GemType.None;

        RefreshValueSprite(data.Value);
        RefreshGem(data);

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
            button.interactable = !_isRemoved;
        }

        RefreshVisual();
        RefreshRemoveStateInstant();
    }

    /// <summary>
    /// Xử lý khi click vào cell
    /// </summary>
    private void OnClick()
    {
        if (_inputHandler == null) return;
        _inputHandler.OnCellClicked(_index);
    }

    /// <summary>
    /// Set trạng thái selected cho cell.
    /// </summary>
    public void SetSelected(bool selected)
    {
        if (_isRemoved) return;

        _isSelected = selected;
        if (selected)
        {
            if (_clickCoroutine != null) StopCoroutine( _clickCoroutine );
            _clickCoroutine = StartCoroutine(ScaleUpAnim());
        }
        else RefreshVisual();
    }

    /// <summary>
    /// Animation thu nhỏ khi bị xóa.
    /// </summary>
    public void ScaleDownAnim()
    {
        if (removeAnim != null)
            removeAnim.PlayRemove();
    }

    /// <summary>
    /// Animation phóng to khi được chọn.
    /// </summary>
    public IEnumerator ScaleUpAnim()
    {
        if (removeAnim != null)
            removeAnim.PlayClick();

        yield return new WaitUntil(() => GameManager.Instance.BoardManager.IsClicking == false);
        RefreshVisual();
    }

    /// <summary>
    /// Xử lý khi cell bị xóa
    /// </summary>
    private void HandleCellRemoved(int removedIndex)
    {
        if (removedIndex != _index) return;

        _isRemoved = true;
        _isSelected = false;
        _isGem = false;

        if (gemImage != null)
            gemImage.gameObject.SetActive(false);

        RefreshVisual();

        if (button != null)
            button.interactable = false;

        ScaleDownAnim();
    }

    /// <summary>
    /// Cập nhật sprite hiển thị số.
    /// </summary>
    private void RefreshValueSprite(int value)
    {
        if (valueImage == null || numberSpriteLibrary == null)
            return;

        Sprite sprite = numberSpriteLibrary.GetSprite(value);
        valueImage.sprite = sprite;
        valueImage.enabled = sprite != null;
    }

    /// <summary>
    /// Cập nhật visual dựa trên trạng thái
    /// </summary>
    private void RefreshVisual()
    {
        if (background != null)
        {
            if (_isSelected)
            {
                if (_isGem)
                {
                    Color color = gemImage.color;
                    color.a = 0f;
                    gemImage.color = color;
                }
                background.color = selectedBackgroundColor;
            }
            else
            {
                if (_isGem)
                {
                    Color color = gemImage.color;
                    color.a = 1f;
                    gemImage.color = color;
                }
                background.color = normalBackgroundColor;
            }
        }

        if (valueImage != null)
        {
            if (_isSelected) valueImage.color = normalTextColor;
            else if (_isGem) valueImage.color = Color.white;
            else if (_isRemoved) valueImage.color = removedTextColor;
            else valueImage.color = normalTextColor;
        }
    }

    /// <summary>
    /// Cập nhật trạng thái removed
    /// </summary>
    private void RefreshRemoveStateInstant()
    {
        if (removeAnim == null) return;

        if (_isRemoved)
            removeAnim.SetHiddenInstant();
    }

    /// <summary>
    /// Cập nhật hiển thị gem.
    /// </summary>
    private void RefreshGem(CellData data)
    {
        if (gemImage == null) return;

        if (data.HasGem == false || data.GemType == GemType.None)
        {
            gemImage.gameObject.SetActive(false);
            return;
        }

        gemImage.gameObject.SetActive(true);
        gemImage.sprite = gemSpriteLibrary.GetSprite(data.GemType);
    }

    /// <summary>
    /// Set vị trí ngay lập tức.
    /// </summary>
    public void SetPositionInstant(Vector2 anchoredPosition)
    {
        if (moveAnim != null)
            moveAnim.SetPositionInstant(anchoredPosition);
        else if (RectTransform != null)
            RectTransform.anchoredPosition = anchoredPosition;
    }

    /// <summary>
    /// Di chuyển đến vị trí mới với animation.
    /// </summary>
    public void PlayMoveTo(Vector2 anchoredPosition, float duration)
    {
        if (moveAnim != null)
            moveAnim.MoveTo(anchoredPosition, duration);
        else if (RectTransform != null)
            RectTransform.anchoredPosition = anchoredPosition;
    }

    /// <summary>
    /// Lấy tọa độ world center của cell.
    /// </summary>
    public Vector3 GetWorldCenter()
    {
        RectTransform rt = transform as RectTransform;
        return rt.TransformPoint(rt.rect.center);
    }

    /// <summary>
    /// Chạy animation preview circle khi thêm số.
    /// </summary>
    public IEnumerator PlayAddPreviewCircle()
    {
        if (_circleHighlight == null)
            yield break;

        _circleHighlight.gameObject.SetActive(true);
        _circleHighlight.fillAmount = 0f;

        Color c = _circleHighlight.color;
        c.a = 1f;
        _circleHighlight.color = c;

        float timer = 0f;

        while (timer < _drawTime)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / _drawTime);

            _circleHighlight.fillAmount = t;

            yield return null;
        }

        _circleHighlight.fillAmount = 1f;

        yield return new WaitForSeconds(_holdTime);

        timer = 0f;

        while (timer < _fadeTime)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / _fadeTime);

            Color fadeColor = _circleHighlight.color;
            fadeColor.a = Mathf.Lerp(1f, 0f, t);
            _circleHighlight.color = fadeColor;

            yield return null;
        }

        HideAddPreviewCircleInstant();
    }
}