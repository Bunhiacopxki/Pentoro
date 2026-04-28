public class PairSlot
{
    public readonly int IndexA;  // Index của cell thứ nhất
    public readonly int IndexB;  // Index của cell thứ hai

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