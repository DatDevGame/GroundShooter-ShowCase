using System.Collections.Generic;
using UnityEngine;

public static class BezierUtility
{
    public static Vector3 Evaluate(List<Vector3> points, float t)
    {
        while (points.Count > 1)
        {
            List<Vector3> next = new();
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector3 p0 = points[i];
                Vector3 p1 = points[i + 1];
                next.Add(Vector3.Lerp(p0, p1, t));
            }
            points = next;
        }
        return points[0];
    }

    public static float EstimateLength(List<Vector3> points, int resolution = 20)
    {
        float length = 0f;
        Vector3 prev = Evaluate(points, 0f);
        for (int i = 1; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            Vector3 point = Evaluate(points, t);
            length += Vector3.Distance(prev, point);
            prev = point;
        }
        return length;
    }
}
