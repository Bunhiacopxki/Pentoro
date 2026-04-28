using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Service gắn gem vào các cell
/// </summary>
public class GemSpawnService
{
    /// <summary>
    /// Gắn gem vào các cell mới.
    /// </summary>
    public void AttachGems(List<CellData> cells, IReadOnlyList<int> newIndexes, GemSpawnContext context)
    {
        if (cells == null || newIndexes == null || newIndexes.Count == 0 || context == null)
            return;

        // Lọc ra các cell có thể đặt gem
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

    /// <summary>
    /// Thử spawn gem vào các cell candidate.
    /// </summary>
    private void TrySpawnGems(List<CellData> cells, List<int> candidateIndexes, GemSpawnContext context)
    {
        int maxSpawnThisTurn = context.GemRemaining();
        if (maxSpawnThisTurn <= 0) return;
        if (candidateIndexes.Count == 0) return;

        // Tính toán các thông số spawn
        int randomPercent = Random.Range(context.MinPercent, context.MaxPercent + 1);
        int y = Mathf.CeilToInt((candidateIndexes.Count + 1) / 2f); // Khoảng cách tối đa giữa các gem

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

            // Force spawn sau y cell không có gem
            bool forceSpawn = sinceLastGem >= y - 1;
            // Random spawn theo tỷ lệ phần trăm
            bool randomSpawn = Random.Range(0, 100) < randomPercent;

            if (forceSpawn || randomSpawn)
            {
                // Kiểm tra xem có thể đặt gem ở đây không
                if (CanPlaceGemHere(cells, cellIndex, selectedIndexes))
                {
                    selectedIndexes.Add(cellIndex);
                    sinceLastGem = 0;
                }
                else sinceLastGem++;
            }
            else sinceLastGem++;
        }

        // Gắn gem vào các cell đã chọn
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

    /// <summary>
    /// Kiểm tra xem có thể đặt gem vào cell candidate
    /// </summary>
    private bool CanPlaceGemHere(List<CellData> cells, int candidateIndex, List<int> selectedIndexes)
    {
        for (int i = 0; i < selectedIndexes.Count; i++)
        {
            int otherIndex = selectedIndexes[i];

            // Nếu gem mới không match với các gem đang có thì không sao
            if (!BoardRules.IsMatchValue(cells[candidateIndex].Value, cells[otherIndex].Value))
                continue;

            // Nếu có thể match với gem đã đặt trước đó → không được đặt ở đây
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