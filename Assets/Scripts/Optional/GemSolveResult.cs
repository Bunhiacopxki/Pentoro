using System.Collections.Generic;

/// <summary>
/// Lưu trữ kết quả của bài toán thu thập gem.
/// Chứa trạng thái, danh sách các giải pháp, và các thông số thống kê.
/// </summary>
public class GemSolveResult
{
    public GemSolveStatus Status;
    public List<List<MatchMove>> Solutions;
    public int RequiredGemCount;
    public int TotalGemCount;
    public int BestMoveCount;

    public GemSolveResult(
        GemSolveStatus status,
        List<List<MatchMove>> solutions,
        int requiredGemCount,
        int totalGemCount,
        int bestMoveCount
    )
    {
        Status = status;
        Solutions = solutions;
        RequiredGemCount = requiredGemCount;
        TotalGemCount = totalGemCount;
        BestMoveCount = bestMoveCount;
    }
}