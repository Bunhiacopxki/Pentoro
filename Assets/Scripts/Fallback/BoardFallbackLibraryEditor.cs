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

        List<IntBoardData> bakedBoards = baker.BakeBoards(stage, requiredPairCount, targetCount, maxAttempts);

        if (requiredPairCount == 3)
            library.pair3Boards = bakedBoards;
        else if (requiredPairCount == 2)
            library.pair2Boards = bakedBoards;
        else
            library.pair1Boards = bakedBoards;

        EditorUtility.SetDirty(library);
        AssetDatabase.SaveAssets();

        Debug.Log($"Bake xong ở stage={stage}. ");
    }

    private int GetRequiredPairCountByStage(int stage)
    {
        if (stage <= 1) return 3;
        if (stage == 2) return 2;
        return 1;
    }
}
#endif