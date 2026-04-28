using System.Collections.Generic;
using UnityEngine;

public class GemSpawnService
{
    public void AttachGems(List<CellData> cells, IReadOnlyList<int> newIndexes, GemSpawnContext context)
    {
        if (cells == null || newIndexes == null || newIndexes.Count == 0 || context == null)
            return;

        List<int> candidateIndexes = new List<int>();

        for (int i = 0; i < newIndexes.Count; i++)
        {
            int index = newIndexes[i];

            if (index < 0 || index >= cells.Count)
                continue;

            if (!cells[index].IsRemoved && !cells[index].HasGem)
                candidateIndexes.Add(index);
        }

        TrySpawnGems(cells, candidateIndexes, context);
    }

    private void TrySpawnGems(List<CellData> cells, List<int> candidateIndexes, GemSpawnContext context)
    {
        int maxSpawnThisTurn = context.GemRemaining();
        if (maxSpawnThisTurn <= 0) return;
        if (candidateIndexes.Count == 0) return;

        int randomPercent = Random.Range(context.MinPercent, context.MaxPercent + 1);
        int y = Mathf.CeilToInt((candidateIndexes.Count + 1) / 2f);

        List<int> selectedIndexes = new List<int>();
        int sinceLastGem = 0;

        for (int i = 0; i < candidateIndexes.Count; i++)
        {
            if (selectedIndexes.Count >= maxSpawnThisTurn)
                break;

            int cellIndex = candidateIndexes[i];
            CellData cell = cells[cellIndex];

            if (cell.HasGem)
            {
                sinceLastGem = 0;
                continue;
            }

            // Không còn loại gem nào có thể spawn
            if (context.GetAvailableGoal() == null)
                break;

            bool forceSpawn = sinceLastGem >= y - 1;
            bool randomSpawn = Random.Range(0, 100) < randomPercent;

            if (forceSpawn || randomSpawn)
            {
                if (CanPlaceGemHere(cells, cellIndex, selectedIndexes))
                {
                    selectedIndexes.Add(cellIndex);
                    sinceLastGem = 0;
                }
                else sinceLastGem++;
            }
            else sinceLastGem++;
        }

        for (int i = 0; i < selectedIndexes.Count; i++)
        {
            GemGoalEntry goal = context.GetAvailableGoal();
            if (goal == null) break;

            int index = selectedIndexes[i];

            cells[index].HasGem = true;
            cells[index].GemType = goal.gemType;
            goal.spawnedCount++;
        }
    }

    private bool CanPlaceGemHere(List<CellData> cells, int candidateIndex, List<int> selectedIndexes)
    {
        for (int i = 0; i < selectedIndexes.Count; i++)
        {
            int otherIndex = selectedIndexes[i];

            if (!BoardRules.IsMatchValue(cells[candidateIndex].Value, cells[otherIndex].Value))
                continue;

            bool canMatch = BoardRules.IsPathClear(
                candidateIndex,
                otherIndex,
                GameManager.Instance.BoardManager.Columns,
                cells.Count,
                idx => !cells[idx].IsRemoved
            );

            if (canMatch)
                return false;
        }

        return true;
    }
}