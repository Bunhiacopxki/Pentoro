using System.Collections.Generic;

public class BoardFallbackBaker
{
    private readonly int _rows;
    private readonly int _columns;

    private readonly DigitDistributionPlanner _distributionPlanner;
    private readonly BoardFiller _boardFiller;
    private readonly BoardValidator _boardValidator;

    public BoardFallbackBaker(int rows, int columns)
    {
        _rows = rows;
        _columns = columns;
        _distributionPlanner = new DigitDistributionPlanner();
        _boardFiller = new BoardFiller(_columns);
        _boardValidator = new BoardValidator(_columns);
    }

    public List<IntBoardData> BakeBoards(int stage, int requiredPairCount, int targetCount, int maxAttempts, List<IntBoardData> existingBoards = null)
    {
        List<IntBoardData> result = new List<IntBoardData>();
        HashSet<string> signatures = new HashSet<string>();

        if (existingBoards != null)
        {
            for (int i = 0; i < existingBoards.Count; i++)
            {
                IntBoardData board = existingBoards[i];

                if (board == null || board.values == null)
                    continue;

                string oldSignature = string.Join(",", board.values);

                if (signatures.Contains(oldSignature))
                    continue;

                signatures.Add(oldSignature);

                result.Add(new IntBoardData
                {
                    name = board.name,
                    values = CloneArray(board.values)
                });
            }
        }

        int attempts = 0;

        while (result.Count < targetCount && attempts < maxAttempts)
        {
            attempts++;

            BoardGenerationContext context = new BoardGenerationContext(_columns, _rows);
            context.DigitCounts = _distributionPlanner.CreateBalancedDigitCounts(context.TotalCells, stage);

            PairLayoutPlanner pairLayoutPlanner = new PairLayoutPlanner(_columns, _rows);
            context.ForcedPairs = pairLayoutPlanner.CreatePairSlots(requiredPairCount, context.TotalCells);

            bool filled = _boardFiller.Fill(context);
            if (!filled)
                continue;

            bool valid = _boardValidator.Validate(context, requiredPairCount);
            if (!valid)
                continue;

            string signature = string.Join(",", context.Values);
            if (signatures.Contains(signature))
                continue;

            signatures.Add(signature);

            result.Add(new IntBoardData
            {
                name = $"Pair{requiredPairCount}_Board_{result.Count + 1}",
                values = CloneArray(context.Values)
            });
        }

        return result;
    }

    private int[] CloneArray(int[] source)
    {
        int[] clone = new int[source.Length];
        System.Array.Copy(source, clone, source.Length);
        return clone;
    }
}