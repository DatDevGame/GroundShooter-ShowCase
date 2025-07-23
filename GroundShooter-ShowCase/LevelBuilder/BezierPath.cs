using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BezierPath : MonoBehaviour
{
    public List<Vector3> controlPoints = new()
    {
        new Vector3(-2, 0, 0),
        new Vector3(-1, 0, 2),
        new Vector3(1, 0, 2),
        new Vector3(2, 0, 0)
    };

    public Vector3 GetPoint(float t)
    {
        var pts = GetExtendedPoints();
        int numSections = pts.Count - 3;
        float scaledT = t * numSections;
        int i = Mathf.Min(Mathf.FloorToInt(scaledT), numSections - 1);
        float localT = scaledT - i;

        return CatmullRom(pts[i], pts[i + 1], pts[i + 2], pts[i + 3], localT);
    }

    private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        return 0.5f * (
            2f * p1 +
            (-p0 + p2) * t +
            t * t * (2f * p0 - 5f * p1 + 4f * p2 - p3) +
            t * t * t * (-p0 + 3f * p1 - 3f * p2 + p3)
        );
    }

    private List<Vector3> GetExtendedPoints()
    {
        List<Vector3> pts = new(controlPoints);
        if (pts.Count < 2) return pts;
        pts.Insert(0, pts[0] - (pts[1] - pts[0]));
        pts.Add(pts[pts.Count - 1] + (pts[pts.Count - 1] - pts[pts.Count - 2]));
        return pts;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 prev = GetPoint(0f);
        for (int i = 1; i <= 50; i++)
        {
            float t = i / 50f;
            Vector3 next = GetPoint(t);
            Gizmos.DrawLine(prev + transform.position, next + transform.position);
            prev = next;
        }
    }
#endif
}
