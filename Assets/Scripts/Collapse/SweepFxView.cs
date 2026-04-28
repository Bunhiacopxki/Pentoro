using UnityEngine;
using UnityEngine.UI;

public class SweepFxView : MonoBehaviour
{
    [SerializeField] private RectTransform root;
    [SerializeField] private Image headImage;
    [SerializeField] private Image trailImage;
    [SerializeField] private RectTransform trailRect;

    public RectTransform Root => root;
    public Image HeadImage => headImage;
    public Image TrailImage => trailImage;
    public RectTransform TrailRect => trailRect;
}