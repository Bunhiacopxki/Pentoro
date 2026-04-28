using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class GemMoveSolverRunner
{
    private const int Columns = 9;

    [MenuItem("Tools/Gem Solver/Run Solver")]
    public static void RunSolver()
    {
        string inputPath = Path.Combine(Application.dataPath, "input.txt");
        string outputPath = Path.Combine(Application.dataPath, "output.txt");

        if (!File.Exists(inputPath))
        {
            Debug.LogError($"Cannot find input.txt at: {inputPath}");
            return;
        }

        string input = File.ReadAllText(inputPath).Trim();

        GemMoveSolver solver = new GemMoveSolver();
        GemSolveResult result = solver.Solve(input);

        WriteOutput(outputPath, result);

        Debug.Log("Gem Solver completed.");
        Debug.Log($"Total gem count: {result.TotalGemCount}");
        Debug.Log($"Required gem count: {result.RequiredGemCount}");
        Debug.Log($"Status: {result.Status}");
        Debug.Log($"Best move count: {result.BestMoveCount}");
        Debug.Log($"Solution count: {result.Solutions.Count}");
        Debug.Log($"Output path: {outputPath}");

        AssetDatabase.Refresh();
    }

    private static void WriteOutput(string outputPath, GemSolveResult result)
    {
        if (result.Status == GemSolveStatus.NoSolution)
        {
            File.WriteAllText(outputPath, "NO_SOLUTION");
            return;
        }

        List<string> lines = new List<string>();

        foreach (List<MatchMove> solution in result.Solutions)
        {
            string line = string.Join(
                "|",
                solution.Select(move => move.ToOutputString(Columns))
            );

            lines.Add(line);
        }

        File.WriteAllLines(outputPath, lines);
    }
}