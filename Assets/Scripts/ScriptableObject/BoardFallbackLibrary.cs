using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "NumMatch/Board Fallback Library")]
public class BoardFallbackLibrary : ScriptableObject
{
    [Header("Bake Settings")]
    public int bakeStage = 1;
    public int bakeRows = 3;
    public int bakeColumns = 9;
    public int bakeTargetCount = 5;
    public int bakeMaxAttempts = 2000;

    public List<IntBoardData> pair3Boards = new List<IntBoardData>();
    public List<IntBoardData> pair2Boards = new List<IntBoardData>();
    public List<IntBoardData> pair1Boards = new List<IntBoardData>();
}

[Serializable]
public class IntBoardData
{
    public string name;
    public int[] values;
}