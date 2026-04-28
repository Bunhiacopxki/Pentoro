#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(BoardFallbackLibrary))]
public class BoardFallbackLibraryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BoardFallbackLibrary library = (BoardFallbackLibrary)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Bake Fallback Boards"))
        {
            BakeLibrary(library);
        }

        if (GUILayout.Button("Clear All Boards"))
        {
            library.pair3Boards.Clear();
            library.pair2Boards.Clear();
            library.pair1Boards.Clear();

            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
            Debug.Log("Đã clear fallback library.");
        }
    }

    private void BakeLibrary(BoardFallbackLibrary library)
    {
        int stage = Mathf.Max(1, library.bakeStage);
        int rows = Mathf.Max(1, library.bakeRows);
        int columns = Mathf.Max(1, library.bakeColumns);
        int targetCount = Mathf.Max(1, library.bakeTargetCount);
        int maxAttempts = Mathf.Max(targetCount, library.bakeMaxAttempts);

        int requiredPairCount = GetRequiredPairCountByStage(stage);
        BoardFallbackBaker baker = new BoardFallbackBaker(rows, columns);
        List<IntBoardData> existingBoards = GetExistingBoards(library, requiredPairCount);
        List<IntBoardData> bakedBoards = baker.BakeBoards(stage, requiredPairCount, targetCount, maxAttempts, existingBoards);

        SetBoards(library, requiredPairCount, bakedBoards);

        EditorUtility.SetDirty(library);
        AssetDatabase.SaveAssets();

        Debug.Log($"Bake xong ở stage={stage}. ");
    }

    private List<IntBoardData> GetExistingBoards(BoardFallbackLibrary library, int requiredPairCount)
    {
        if (requiredPairCount == 3)
            return library.pair3Boards;

        if (requiredPairCount == 2)
            return library.pair2Boards;

        return library.pair1Boards;
    }

    private void SetBoards(BoardFallbackLibrary library, int requiredPairCount, List<IntBoardData> boards)
    {
        if (requiredPairCount == 3)
            library.pair3Boards = boards;
        else if (requiredPairCount == 2)
            library.pair2Boards = boards;
        else
            library.pair1Boards = boards;
    }


    private int GetRequiredPairCountByStage(int stage)
    {
        if (stage <= 1) return 3;
        if (stage == 2) return 2;
        return 1;
    }
}
#endif