using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;
using UnityEditor;

[Serializable]
public class EnemyGroupData
{
    // ==================== PATH SETTINGS ====================
    [Title("Path Settings")]
    public BezierPathSO Path;

    [LabelText("Start Delay Group")]
    public float StartDelayGroup = 0.5f;

    [Min(0.1f)]
    public float Speed = 5f;

    public Ease EaseType = Ease.Linear;

    [LabelText("Loop Path")]
    public bool LoopPath = false;

    [LabelText("Wait For Group Death")]
    public bool WaitForDeath = true;

    [LabelText("Look Forward On Path")]
    public bool LookForward = true;

    [ShowIf("@!LookForward")]
    public FacingDirection FacingDirection = FacingDirection.Forward;

    // ==================== GRID SETTINGS ====================
    [Title("Grid Setup")]
    [MinValue(1), OnValueChanged("ResizeGrid")] public int Rows = 3;
    [MinValue(1), OnValueChanged("ResizeGrid")] public int Columns = 3;

    [LabelText("Offset (X = Column, Y = Row)")]
    public Vector2 Offset = new(1f, 1f);

    // ==================== FILL SETTINGS ====================
    [Title("Fill Settings")]
    public GameObject PrefabToFill;
    public UnitSO UnitStatsToFill;

    public GridFillMode FillMode = GridFillMode.FillAll;
    public int TargetRow;
    public int TargetColumn;

    [SerializeField, HideInInspector]
    private List<EnemySpawnInfo> flatGrid = new();

    public EnemySpawnInfo this[int r, int c]
    {
        get
        {
            int index = r * Columns + c;
            if (index < 0 || index >= flatGrid.Count) return new EnemySpawnInfo();
            return flatGrid[index];
        }
        set
        {
            int index = r * Columns + c;
            if (index < 0) return;
            while (flatGrid.Count <= index)
                flatGrid.Add(new EnemySpawnInfo());
            flatGrid[index] = value;
        }
    }

    public void Resize(int newRows, int newCols)
    {
        Rows = newRows;
        Columns = newCols;
        int total = Rows * Columns;
        while (flatGrid.Count < total)
            flatGrid.Add(new EnemySpawnInfo());
        while (flatGrid.Count > total)
            flatGrid.RemoveAt(flatGrid.Count - 1);
    }

    private void ResizeGrid() => Resize(Rows, Columns);

    [Button("Fill Grid (by Mode)")]
    private void FillGrid()
    {
        Resize(Rows, Columns);
        System.Random rng = new();

        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Columns; c++)
            {
                bool shouldFill = FillMode switch
                {
                    GridFillMode.FillAll => true,
                    GridFillMode.FirstRow => r == 0,
                    GridFillMode.FirstColumn => c == 0,
                    GridFillMode.DiagonalTLBR => r == c,
                    GridFillMode.DiagonalTRBL => r + c == Columns - 1,
                    GridFillMode.Checkerboard => (r + c) % 2 == 0,
                    GridFillMode.Random => rng.NextDouble() < 0.5,
                    _ => false
                };

                if (shouldFill)
                    this[r, c] = new EnemySpawnInfo { EnemyPrefab = PrefabToFill, UnitStats = UnitStatsToFill };
            }
        }
    }

    [Button("Fill Row")]
    private void FillRow()
    {
        if (TargetRow < 0 || TargetRow >= Rows) return;
        for (int c = 0; c < Columns; c++)
            this[TargetRow, c] = new EnemySpawnInfo { EnemyPrefab = PrefabToFill, UnitStats = UnitStatsToFill };
    }

    [Button("Fill Column")]
    private void FillColumn()
    {
        if (TargetColumn < 0 || TargetColumn >= Columns) return;
        for (int r = 0; r < Rows; r++)
            this[r, TargetColumn] = new EnemySpawnInfo { EnemyPrefab = PrefabToFill, UnitStats = UnitStatsToFill };
    }

    [Button("Clear Grid")]
    private void ClearGrid()
    {
        Resize(Rows, Columns);
        for (int i = 0; i < flatGrid.Count; i++)
            flatGrid[i] = new EnemySpawnInfo();
    }

#if UNITY_EDITOR
    [ShowInInspector]
    [TableMatrix(
        DrawElementMethod = nameof(DrawCell),
        SquareCells = true,
        ResizableColumns = false,
        HideRowIndices = false,
        HideColumnIndices = false,
        RowHeight = 70
    )]
    private EnemySpawnInfo[,] Matrix
    {
        get
        {
            Resize(Rows, Columns);
            var matrix = new EnemySpawnInfo[Columns, Rows];
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Columns; c++)
                    matrix[c, r] = this[r, c];
            return matrix;
        }
        set
        {
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Columns; c++)
                    this[r, c] = value[c, r];
        }
    }

    private static EnemySpawnInfo DrawCell(Rect rect, EnemySpawnInfo value)
    {
        if (value == null) value = new EnemySpawnInfo();

        const float targetWidth = 80f;
        const float targetHeight = 60f;
        const float padding = 2f;

        float offsetX = (rect.width - targetWidth) / 2f;
        float offsetY = (rect.height - targetHeight) / 2f;
        Rect cellRect = new Rect(rect.x + offsetX, rect.y + offsetY, targetWidth, targetHeight);

        EditorGUI.DrawRect(cellRect, new Color(0.15f, 0.15f, 0.15f));
        EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y, cellRect.width, 1), Color.gray);
        EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.yMax - 1, cellRect.width, 1), Color.gray);
        EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y, 1, cellRect.height), Color.gray);
        EditorGUI.DrawRect(new Rect(cellRect.xMax - 1, cellRect.y, 1, cellRect.height), Color.gray);

        float half = (cellRect.height - padding * 3) / 2f;
        Rect prefabRect = new Rect(cellRect.x + padding, cellRect.y + padding, cellRect.width - 2 * padding, half);
        Rect statsRect = new Rect(cellRect.x + padding, cellRect.y + 2 * padding + half, cellRect.width - 2 * padding, half);

        value.EnemyPrefab = (GameObject)EditorGUI.ObjectField(prefabRect, GUIContent.none, value.EnemyPrefab, typeof(GameObject), false);
        value.UnitStats = (UnitSO)EditorGUI.ObjectField(statsRect, GUIContent.none, value.UnitStats, typeof(UnitSO), false);

        return value;
    }
#endif

    [Serializable]
    public class EnemySpawnInfo
    {
        public GameObject EnemyPrefab;
        public UnitSO UnitStats;
    }

    public enum GridFillMode
    {
        FillAll,
        FirstRow,
        FirstColumn,
        DiagonalTLBR,
        DiagonalTRBL,
        Checkerboard,
        Random
    }
}
