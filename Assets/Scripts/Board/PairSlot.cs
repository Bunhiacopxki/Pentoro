public class PairSlot
{
    public readonly int IndexA;
    public readonly int IndexB;

    public PairSlot(int indexA, int indexB)
    {
        IndexA = indexA;
        IndexB = indexB;
    }

    public bool IsPair(int a, int b)
    {
        return (IndexA == a && IndexB == b) || (IndexA == b && IndexB == a);
    }
}