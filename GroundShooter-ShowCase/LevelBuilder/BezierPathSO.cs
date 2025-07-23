using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BezierPath", menuName = "Game/Simple Bezier Path")]
public class BezierPathSO : SerializedScriptableObject
{
    [System.Serializable]
    public class BezierPoint
    {
        public Vector3 Position;

        [LabelText("To Next")]
        public CurveType CurveToNext = CurveType.CatmullRom;

        [ShowIf("@CurveToNext == CurveType.Bezier")]
        [LabelText("Control Out")]
        public Vector3 ControlPointOut;
    }

    [ListDrawerSettings(ShowIndexLabels = true)]
    public List<BezierPoint> Points = new();

    private List<(float t, float distance)> m_DistanceTable;

    // ================== RUNTIME API ==================

    public Vector3 GetPoint(float t)
    {
        int segmentCount = Points.Count - 1;
        if (segmentCount <= 0)
            return Points.Count > 0 ? Points[0].Position : Vector3.zero;

        float totalT = Mathf.Clamp01(t) * segmentCount;
        int i = Mathf.Min(Mathf.FloorToInt(totalT), segmentCount - 1);
        float localT = totalT - i;

        return EvaluateSegment(i, localT);
    }

    public Vector3 GetPoint(float t, Transform origin)
    {
        Vector3 local = GetPoint(t);
        return origin != null ? origin.TransformPoint(local) : local;
    }

    public void BuildDistanceTable(int resolution = 200)
    {
        m_DistanceTable = new();
        float totalDistance = 0f;
        Vector3 prev = GetPoint(0f);
        m_DistanceTable.Add((0f, 0f));

        for (int i = 1; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            Vector3 point = GetPoint(t);
            totalDistance += Vector3.Distance(prev, point);
            m_DistanceTable.Add((t, totalDistance));
            prev = point;
        }
    }

    public Vector3 GetPointAtDistance(float distance)
    {
        if (m_DistanceTable == null || m_DistanceTable.Count == 0)
            BuildDistanceTable(200);

        float total = m_DistanceTable[^1].distance;
        distance = Mathf.Clamp(distance, 0f, total);

        for (int i = 0; i < m_DistanceTable.Count - 1; i++)
        {
            var a = m_DistanceTable[i];
            var b = m_DistanceTable[i + 1];

            if (b.distance >= distance)
            {
                float segmentLength = b.distance - a.distance;
                float segmentT = segmentLength > 0f ? (distance - a.distance) / segmentLength : 0f;
                float t = Mathf.Lerp(a.t, b.t, segmentT);
                return GetPoint(t);
            }
        }

        return GetPoint(1f);
    }

    public float GetTotalLength(int resolution = 100)
    {
        float length = 0f;
        Vector3 prev = GetPoint(0f);
        for (int i = 1; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            Vector3 current = GetPoint(t);
            length += Vector3.Distance(prev, current);
            prev = current;
        }
        return length;
    }

    // ================== EVALUATION ==================

    private Vector3 EvaluateSegment(int i, float t)
    {
        BezierPoint p0 = Points[i];
        BezierPoint p1 = Points[i + 1];

        return p0.CurveToNext switch
        {
            CurveType.Linear => Vector3.Lerp(p0.Position, p1.Position, t),
            CurveType.CatmullRom => CatmullRom(
                i > 0 ? Points[i - 1].Position : p0.Position,
                p0.Position,
                p1.Position,
                i + 2 < Points.Count ? Points[i + 2].Position : p1.Position,
                t),
            CurveType.Bezier => Bezier(
                p0.Position,
                p0.Position + p0.ControlPointOut,
                p1.Position,
                t),
            _ => Vector3.Lerp(p0.Position, p1.Position, t),
        };
    }

    private Vector3 Bezier(Vector3 p0, Vector3 control, Vector3 p1, float t)
    {
        float u = 1 - t;
        return u * u * p0 + 2 * u * t * control + t * t * p1;
    }

    private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        return 0.5f * (
            2f * p1 +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
        );
    }
}

public enum CurveType
{
    Linear,
    CatmullRom,
    Bezier
}
