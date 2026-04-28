using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static GemGoalLibrary;

public class GemUI : MonoBehaviour
{
    [Header("MainCanvas")]
    [SerializeField] private TMP_Text _pinkValue;
    [SerializeField] private TMP_Text _orangeValue;
    [SerializeField] private TMP_Text _purpleValue;
    [Header("LossCanvas")]
    [SerializeField] private TMP_Text _remainPink;
    [SerializeField] private TMP_Text _remainOrange;
    [SerializeField] private TMP_Text _remainPurple;

    private void Start()
    {
        GameManager.Instance.StageManager.StageUp += UpdateUI;
        GameManager.Instance.BoardManager.OnUpdateGem += UpdateTextValue;
    }

    private void OnDestroy()
    {
        GameManager.Instance.StageManager.StageUp -= UpdateUI;
        GameManager.Instance.BoardManager.OnUpdateGem -= UpdateTextValue;

    }

    public void UpdateUI(int stage)
    {
        List<GemGoalConfigData> gemGoal = GameManager.Instance.BoardManager.GemLibrary.GetGemGoal(stage);
        if (gemGoal == null) return;
        for (int i = 0; i < gemGoal.Count; i++) 
        {
            GemGoalConfigData currentGem = gemGoal[i];
            UpdateTextValue(currentGem.gemType, currentGem.targetCount);
        }
    }

    private void UpdateTextValue(GemType type, int count)
    {
        switch (type)
        {
            case GemType.Pink:
                {
                    _pinkValue.text = count.ToString();
                    _remainPink.text = count.ToString();
                    break;
                }
            case GemType.Orange:
                {
                    _orangeValue.text = count.ToString();
                    _remainOrange.text = count.ToString();
                    break;
                }
            case GemType.Purple:
                {
                    _purpleValue.text = count.ToString();
                    _remainPurple.text = count.ToString();
                    break;
                }
            default:
                break;
        }
    }
}
