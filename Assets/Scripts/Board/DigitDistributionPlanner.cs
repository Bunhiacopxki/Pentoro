using System.Collections.Generic;

public class DigitDistributionPlanner
{
    private static readonly int[] SpreadOrder = { 0, 2, 3, 5, 6, 8, 1, 4, 7 };

    public Dictionary<int, int> CreateBalancedDigitCounts(int totalCells, int stage)
    {
        Dictionary<int, int> counts = new Dictionary<int, int>(9);

        int baseCount = totalCells / 9;
        int remainder = totalCells % 9;

        for (int digit = 1; digit <= 9; digit++)
            counts[digit] = baseCount;

        int rotateStart = ((stage - 1) % 9 + 9) % 9;

        for (int i = 0; i < remainder; i++)
        {
            int spreadIndex = SpreadOrder[(rotateStart + i) % 9];
            int digit = spreadIndex + 1;
            counts[digit]++;
        }

        return counts;
    }
}