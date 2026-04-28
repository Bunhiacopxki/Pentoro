using System;
using TMPro;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    [SerializeField] private int currentStage = 1;
    [SerializeField] private TMP_Text stageValue;
    [SerializeField] private TMP_Text stageLoss;
    [SerializeField] private TMP_Text stageWin;

    public Action<int> StageUp;

    private void OnEnable()
    {
        StageUp += ChangeStageValue;
    }

    private void OnDisable()
    {
        StageUp -= ChangeStageValue;
    }

    public void ChangeStageValue(int stage)
    {
        if (stageValue != null && stageLoss != null)
        {
            stageValue.text = stage.ToString();
            stageLoss.text = stage.ToString();
            stageWin.text = stage.ToString();
        }
    }

    public int CurrentStage => currentStage;

    public int RequiredInitialPairCount
    {
        get
        {
            if (CurrentStage <= 1) return 3;
            if (CurrentStage == 2) return 2;
            return 1;
        }
    }

    public void ResetStage()
    {
        currentStage = 1;
        StageUp?.Invoke(currentStage);
    }

    public void AdvanceStage()
    {
        currentStage++;

        StageUp?.Invoke(currentStage);
    }
}