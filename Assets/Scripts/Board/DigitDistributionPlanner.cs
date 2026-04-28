using System.Collections.Generic;

/// <summary>
/// Lập kế hoạch phân bổ số lượng các chữ số (1-9) cho board một cách cân bằng.
/// </summary>
public class DigitDistributionPlanner
{
    // Thứ tự spread để đảm bảo các số được phân bổ đều
    private static readonly int[] SpreadOrder = { 0, 2, 3, 5, 6, 8, 1, 4, 7 };

    /// <summary>
    /// Tạo dictionary số lượng các chữ số sao cho cân bằng nhất có thể.
    /// </summary>
    public Dictionary<int, int> CreateBalancedDigitCounts(int totalCells, int stage)
    {
        Dictionary<int, int> counts = new Dictionary<int, int>(9);

        // Số lượng cơ bản của mỗi digit
        int baseCount = totalCells / 9;
        int remainder = totalCells % 9;

        for (int digit = 1; digit <= 9; digit++)
            counts[digit] = baseCount;

        // Xoay vị trí bắt đầu theo stage để tạo variety
        int rotateStart = ((stage - 1) % 9 + 9) % 9;

        // Phân bổ remainder theo thứ tự spread
        for (int i = 0; i < remainder; i++)
        {
            int spreadIndex = SpreadOrder[(rotateStart + i) % 9];
            int digit = spreadIndex + 1;
            counts[digit]++;
        }

        return counts;
    }
}