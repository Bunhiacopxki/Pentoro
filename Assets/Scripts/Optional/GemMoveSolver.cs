using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GemMoveSolver
{
    private const int Columns = 9;
    private const int MaxSolutions = 10;

    private int[] _values;
    private bool[] _removed;

    private int _totalGemCount;
    private int _requiredGemCount;
    private int _currentDepthLimit;

    private readonly List<MatchMove> _currentPath = new List<MatchMove>();
    private readonly List<List<MatchMove>> _solutions = new List<List<MatchMove>>();
    private readonly HashSet<string> _solutionKeys = new HashSet<string>();
    private readonly Dictionary<string, int> _visited = new Dictionary<string, int>();

    public GemSolveResult Solve(string input)
    {
        Init(input);

        if (_requiredGemCount == 0)
        {
            List<List<MatchMove>> emptySolutions = new List<List<MatchMove>>
            {
                new List<MatchMove>()
            };

            return new GemSolveResult(
                GemSolveStatus.Solved,
                emptySolutions,
                _requiredGemCount,
                _totalGemCount,
                0
            );
        }

        int lowerBound = Mathf.CeilToInt(_requiredGemCount / 2f);
        int maxDepth = _values.Length / 2;

        for (int depthLimit = lowerBound; depthLimit <= maxDepth; depthLimit++)
        {
            _currentDepthLimit = depthLimit;

            _currentPath.Clear();
            _solutions.Clear();
            _solutionKeys.Clear();
            _visited.Clear();

            ClearRemoved();

            DepthLimitedDFS();

            if (_solutions.Count > 0)
            {
                return new GemSolveResult(
                    GemSolveStatus.Solved,
                    _solutions,
                    _requiredGemCount,
                    _totalGemCount,
                    depthLimit
                );
            }
        }

        return new GemSolveResult(
            GemSolveStatus.NoSolution,
            _solutions,
            _requiredGemCount,
            _totalGemCount,
            -1
        );
    }

    private void Init(string input)
    {
        _values = input
            .Where(char.IsDigit)
            .Select(c => c - '0')
            .ToArray();

        _removed = new bool[_values.Length];

        _totalGemCount = 0;

        for (int i = 0; i < _values.Length; i++)
        {
            if (_values[i] == 5)
                _totalGemCount++;
        }

        _requiredGemCount = (_totalGemCount / 2) * 2;

        _currentPath.Clear();
        _solutions.Clear();
        _solutionKeys.Clear();
        _visited.Clear();

        ClearRemoved();
    }

    private void ClearRemoved()
    {
        for (int i = 0; i < _removed.Length; i++)
            _removed[i] = false;
    }

    private void DepthLimitedDFS()
    {
        int collectedGemCount = CountCollectedGems();

        if (collectedGemCount >= _requiredGemCount)
        {
            SaveSolution();
            return;
        }

        int currentDepth = _currentPath.Count;
        int remainingDepth = _currentDepthLimit - currentDepth;

        if (remainingDepth <= 0)
            return;

        int remainingGemNeed = _requiredGemCount - collectedGemCount;
        int minExtraMovesNeeded = Mathf.CeilToInt(remainingGemNeed / 2f);

        if (minExtraMovesNeeded > remainingDepth)
            return;

        string stateKey = CreateStateKey();

        if (_visited.TryGetValue(stateKey, out int bestRemainingDepth))
        {
            if (bestRemainingDepth >= remainingDepth)
                return;

            _visited[stateKey] = remainingDepth;
        }
        else
        {
            _visited.Add(stateKey, remainingDepth);
        }

        List<MatchMove> legalMoves = GetAllLegalMoves();

        if (legalMoves.Count == 0)
            return;

        legalMoves = legalMoves
            .OrderByDescending(move => GetMoveScore(move))
            .ToList();

        foreach (MatchMove move in legalMoves)
        {
            ApplyMove(move);

            DepthLimitedDFS();

            UndoMove(move);

            if (_solutions.Count >= MaxSolutions)
                return;
        }
    }

    private List<MatchMove> GetAllLegalMoves()
    {
        List<MatchMove> result = new List<MatchMove>();

        for (int i = 0; i < _values.Length; i++)
        {
            if (_removed[i])
                continue;

            for (int j = i + 1; j < _values.Length; j++)
            {
                if (_removed[j])
                    continue;

                if (!BoardRules.IsMatchValue(_values[i], _values[j]))
                    continue;

                bool isPathClear = BoardRules.IsPathClear(
                    i,
                    j,
                    Columns,
                    _values.Length,
                    idx => !_removed[idx]
                );

                if (!isPathClear)
                    continue;

                result.Add(new MatchMove(i, j));
            }
        }

        return result;
    }

    private void SaveSolution()
    {
        List<MatchMove> solution = new List<MatchMove>(_currentPath);
        string key = CreateCanonicalSolutionKey(solution);

        if (_solutionKeys.Contains(key))
            return;

        if (_solutions.Count >= MaxSolutions)
            return;

        _solutions.Add(solution);
        _solutionKeys.Add(key);
    }

    private string CreateCanonicalSolutionKey(List<MatchMove> solution)
    {
        List<string> moveKeys = new List<string>();

        foreach (MatchMove move in solution)
        {
            int a = Mathf.Min(move.A, move.B);
            int b = Mathf.Max(move.A, move.B);

            moveKeys.Add($"{a}-{b}");
        }

        moveKeys.Sort();

        return string.Join("|", moveKeys);
    }

    private string CreateStateKey()
    {
        char[] chars = new char[_removed.Length];

        for (int i = 0; i < _removed.Length; i++)
        {
            chars[i] = _removed[i] ? '1' : '0';
        }

        return new string(chars);
    }

    private void ApplyMove(MatchMove move)
    {
        _removed[move.A] = true;
        _removed[move.B] = true;

        _currentPath.Add(move);
    }

    private void UndoMove(MatchMove move)
    {
        _currentPath.RemoveAt(_currentPath.Count - 1);

        _removed[move.A] = false;
        _removed[move.B] = false;
    }

    private int CountCollectedGems()
    {
        int count = 0;

        for (int i = 0; i < _values.Length; i++)
        {
            if (_values[i] == 5 && _removed[i])
                count++;
        }

        return count;
    }

    private int GetMoveScore(MatchMove move)
    {
        int score = 0;

        if (_values[move.A] == 5)
            score += 100;

        if (_values[move.B] == 5)
            score += 100;

        int distance = Mathf.Abs(move.A - move.B);
        score -= distance;

        return score;
    }
}