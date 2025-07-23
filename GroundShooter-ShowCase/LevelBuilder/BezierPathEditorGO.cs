using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using static UnityEngine.Rendering.DebugUI;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class BezierPathEditorGO : MonoBehaviour
{
#if UNITY_EDITOR
    [ShowInInspector] public BezierPathSO PathData 
    {
        get => m_PathData;
        set
        {
            if (value != m_PathData)
            {
                ClearAllChild();
                m_PathData = value;
                for (int i = 0; i < m_PathData.Points.Count; i++)
                    CreatePointObject(i, m_PathData.Points[i].Position);
                EditorUtility.SetDirty(PathData);
            }
        }
    }
    private BezierPathSO m_PathData;

    private void ClearAllChild()
    {
        var childrenToRemove = new List<GameObject>();
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("[Point_"))
            {
                childrenToRemove.Add(child.gameObject);
            }
        }

        foreach (var go in childrenToRemove)
        {
            if (Application.isEditor)
                DestroyImmediate(go);
            else
                Destroy(go);
        }
    }

    [Button("Create New Path")]
    private void CreateNewPathAsset()
    {
        string folderPath = "Assets/_GroundShooter/ScriptableObjects/Levels/BezierPathSO";

        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            System.IO.Directory.CreateDirectory(folderPath);
            AssetDatabase.Refresh();
        }

        string baseName = "NewBezierPath";
        string path = $"{folderPath}/{baseName}.asset";
        int index = 1;
        while (System.IO.File.Exists(path))
        {
            path = $"{folderPath}/{baseName}_{index}.asset";
            index++;
        }

        var newPath = ScriptableObject.CreateInstance<BezierPathSO>();
        AssetDatabase.CreateAsset(newPath, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        PathData = newPath;
        Debug.Log($"✅ Created BezierPathSO at: {path}");
    }

    [Button("Remove Point At")]
    private void ClearPointAt(int index)
    {
        if (!EditorUtility.DisplayDialog("Confirm Deletion",
            $"Are you sure you want to delete the point at index {index}?",
            "Yes", "Cancel"))
        {
            return;
        }

        if (index <= PathData.Points.Count - 1)
        {
            DestroyImmediate(transform.GetChild(index).gameObject);
            PathData.Points.RemoveAt(index);
            PathData.Points = PathData.Points.Where(v => v != null).ToList();

            ClearAllChild();
            for (int i = 0; i < PathData.Points.Count; i++)
                CreatePointObject(i, PathData.Points[i].Position);
            EditorUtility.SetDirty(PathData);
        }
    }

    [Button("Clear All")]
    private void ClearHandles()
    {
        if (!EditorUtility.DisplayDialog("Confirm Deletion",
            $"Are you sure you want to delete all Point?",
            "Yes", "Cancel"))
        {
            return;
        }

        var childrenToRemove = new List<GameObject>();
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("[Point_"))
            {
                childrenToRemove.Add(child.gameObject);
            }
        }

        foreach (var go in childrenToRemove)
        {
            if (Application.isEditor)
                DestroyImmediate(go);
            else
                Destroy(go);
        }
        PathData.Points.Clear();
    }

    [Button("Add Point")]
    private void AddPoint()
    {
        if (PathData == null) return;

        Vector3 lastLocal = PathData.Points.Count > 0
            ? PathData.Points[^1].Position
            : Vector3.zero;

        Vector3 newLocal = lastLocal + Vector3.forward * 2f;

        PathData.Points.Add(new BezierPathSO.BezierPoint
        {
            Position = newLocal,
            CurveToNext = CurveType.CatmullRom
        });

        ClearAllChild();
        for (int i = 0; i < PathData.Points.Count; i++)
            CreatePointObject(i, PathData.Points[i].Position);
        EditorUtility.SetDirty(PathData);
    }

    private void Update()
    {
        if (!Application.isPlaying && PathData != null)
        {
            bool changed = false;
            for (int i = 0; i < PathData.Points.Count; i++)
            {
                string expectedName = $"[Point_{i}]";
                Transform pointObj = transform.Find(expectedName);
                if (pointObj != null)
                {
                    Vector3 localPos = transform.InverseTransformPoint(pointObj.position);
                    if (PathData.Points[i].Position != localPos)
                    {
                        PathData.Points[i].Position = localPos;
                        changed = true;
                    }
                }
            }
            if (changed)
            {
                EditorUtility.SetDirty(PathData);
            }
        }
    }

    private void CreatePointObject(int index, Vector3 localPosition)
    {
        GameObject go = new GameObject($"[Point_{index}]");
        go.transform.SetParent(transform);
        go.transform.localPosition = localPosition;
    }

    [Button("Disable Editor")]
    private void DisableEdit()
    {
        PathData = null;
    }

    private void OnDrawGizmos()
    {
        if (PathData == null || PathData.Points.Count < 2) return;

        for (int i = 0; i < PathData.Points.Count - 1; i++)
        {
            var p0 = PathData.Points[i];
            var p1 = PathData.Points[i + 1];

            Vector3 wp0 = transform.TransformPoint(p0.Position);
            Vector3 wp1 = transform.TransformPoint(p1.Position);

            switch (p0.CurveToNext)
            {
                case CurveType.Linear:
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(wp0, wp1);
                    break;

                case CurveType.CatmullRom:
                    Gizmos.color = Color.cyan;
                    Vector3 prev = EvaluateCatmull(i, 0f);
                    for (int j = 1; j <= 20; j++)
                    {
                        float t = j / 20f;
                        Vector3 next = EvaluateCatmull(i, t);
                        Gizmos.DrawLine(prev, next);
                        prev = next;
                    }
                    break;

                case CurveType.Bezier:
                    Gizmos.color = Color.yellow;
                    Vector3 c1 = transform.TransformPoint(p0.Position + p0.ControlPointOut);
                    Vector3 c2 = wp1;
                    Vector3 bPrev = wp0;
                    for (int j = 1; j <= 20; j++)
                    {
                        float t = j / 20f;
                        Vector3 bNext = EvaluateBezier(wp0, c1, c2, wp1, t);
                        Gizmos.DrawLine(bPrev, bNext);
                        bPrev = bNext;
                    }
                    break;
            }

            Gizmos.color = Color.white;
            Gizmos.DrawSphere(wp0, 0.1f);
            Handles.Label(wp0 + Vector3.up * 0.1f, $"P{i}");

        }

        var last = PathData.Points[^1];
        Vector3 lastWorld = transform.TransformPoint(last.Position);
        Gizmos.DrawSphere(lastWorld, 0.1f);

        Handles.Label(lastWorld + Vector3.up * 0.1f, $"P{PathData.Points.Count - 1}");

    }

    private Vector3 EvaluateCatmull(int i, float t)
    {
        Vector3 p0 = i > 0 ? PathData.Points[i - 1].Position : PathData.Points[i].Position;
        Vector3 p1 = PathData.Points[i].Position;
        Vector3 p2 = PathData.Points[i + 1].Position;
        Vector3 p3 = i + 2 < PathData.Points.Count ? PathData.Points[i + 2].Position : p2;

        return transform.TransformPoint(0.5f * (
            2f * p1 +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
        ));
    }

    private Vector3 EvaluateBezier(Vector3 p0, Vector3 c1, Vector3 c2, Vector3 p1, float t)
    {
        float u = 1 - t;
        return u * u * u * p0 +
               3 * u * u * t * c1 +
               3 * u * t * t * c2 +
               t * t * t * p1;
    }

#endif
}
