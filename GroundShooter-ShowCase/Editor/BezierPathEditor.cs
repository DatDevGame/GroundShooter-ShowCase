using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BezierPath))]
public class BezierPathEditor : Editor
{
    private BezierPath path;
    private const float GridSize = 1f; // Snap grid spacing
    private const float HandleSize = 0.5f;

    private void OnEnable()
    {
        path = (BezierPath)target;
    }

    private void OnSceneGUI()
    {
        Event e = Event.current;
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        Undo.RecordObject(path, "Edit Bezier Path");

        DrawGrid();

        for (int i = 0; i < path.controlPoints.Count; i++)
        {
            Vector3 worldPos = path.transform.TransformPoint(path.controlPoints[i]);
            Vector3 newWorldPos = Handles.FreeMoveHandle(
                worldPos,
                HandleSize,
                Vector3.zero,
                Handles.SphereHandleCap
            );

            if (worldPos != newWorldPos)
            {
                Vector3 local = path.transform.InverseTransformPoint(newWorldPos);
                local.y = 0f;
                local = Snap(local, GridSize);
                path.controlPoints[i] = local;
                EditorUtility.SetDirty(path);
            }

            Handles.Label(worldPos + Vector3.up * 0.7f, $"P{i}");
        }

        // Draw lines
        Handles.color = Color.yellow;
        for (int i = 0; i < path.controlPoints.Count - 1; i++)
        {
            Handles.DrawLine(
                path.transform.TransformPoint(path.controlPoints[i]),
                path.transform.TransformPoint(path.controlPoints[i + 1])
            );
        }

        HandleMouseAndHotkeys(e);
    }

    private void HandleMouseAndHotkeys(Event e)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        Plane ground = new Plane(Vector3.up, Vector3.zero);

        if (ground.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            hitPoint.y = 0;

            if (e.type == EventType.MouseDown && e.shift)
            {
                path.controlPoints.Add(Snap(path.transform.InverseTransformPoint(hitPoint), GridSize));
                EditorUtility.SetDirty(path);
                e.Use();
            }

            if (e.type == EventType.MouseDown && e.control && path.controlPoints.Count > 0)
            {
                int index = GetClosestPointIndex(hitPoint);
                if (index >= 0)
                {
                    path.controlPoints.RemoveAt(index);
                    EditorUtility.SetDirty(path);
                    e.Use();
                }
            }

            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.A)
            {
                AlignPath();
                EditorUtility.SetDirty(path);
                e.Use();
            }
        }
    }

    private Vector3 Snap(Vector3 pos, float grid)
    {
        pos.x = Mathf.Round(pos.x / grid) * grid;
        pos.z = Mathf.Round(pos.z / grid) * grid;
        pos.y = 0f;
        return pos;
    }

    private int GetClosestPointIndex(Vector3 worldPoint)
    {
        float minDist = float.MaxValue;
        int closest = -1;
        for (int i = 0; i < path.controlPoints.Count; i++)
        {
            Vector3 wp = path.transform.TransformPoint(path.controlPoints[i]);
            float dist = Vector3.Distance(wp, worldPoint);
            if (dist < minDist)
            {
                minDist = dist;
                closest = i;
            }
        }
        return closest;
    }

    private void AlignPath()
    {
        if (path.controlPoints.Count < 2) return;

        // Align with the closer axis
        bool alignX = Mathf.Abs(path.controlPoints[^1].x - path.controlPoints[0].x) >
                      Mathf.Abs(path.controlPoints[^1].z - path.controlPoints[0].z);

        for (int i = 1; i < path.controlPoints.Count - 1; i++)
        {
            Vector3 p = path.controlPoints[i];
            p = alignX
                ? new Vector3(path.controlPoints[0].x, 0, p.z)
                : new Vector3(p.x, 0, path.controlPoints[0].z);
            path.controlPoints[i] = p;
        }
    }

    private void DrawGrid()
    {
        Handles.color = new Color(0.3f, 0.3f, 0.3f, 0.4f);
        int halfGrid = 20;

        for (int x = -halfGrid; x <= halfGrid; x++)
        {
            Handles.DrawLine(new Vector3(x, 0, -halfGrid), new Vector3(x, 0, halfGrid));
        }

        for (int z = -halfGrid; z <= halfGrid; z++)
        {
            Handles.DrawLine(new Vector3(-halfGrid, 0, z), new Vector3(halfGrid, 0, z));
        }
    }
}
