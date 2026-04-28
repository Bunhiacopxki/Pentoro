public struct MatchMove
{
    public int A;
    public int B;

    public MatchMove(int a, int b)
    {
        A = a;
        B = b;
    }

    public string ToOutputString(int columns)
    {
        int rowA = A / columns;
        int colA = A % columns;

        int rowB = B / columns;
        int colB = B % columns;

        return $"{rowA},{colA},{rowB},{colB}";
    }
}

public enum GemSolveStatus
{
    Solved,
    NoSolution
}