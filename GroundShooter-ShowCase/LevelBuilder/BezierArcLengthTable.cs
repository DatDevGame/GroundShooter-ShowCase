using System.Collections.Generic;
using UnityEngine;

public class BezierArcLengthTable
{
    public struct Sample
    {
        public float t;
        public float distance;
    }

    public List<Sample> samples;
    public float totalLength;

    public BezierArcLengthTable(List<Vector3> controlPoints, int resolution = 100)
    {
        samples = new();
        totalLength = 0f;

        Vector3 prev = BezierUtility.Evaluate(controlPoints, 0f);
        samples.Add(new Sample { t = 0f, distance = 0f });

        for (int i = 1; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            Vector3 point = BezierUtility.Evaluate(controlPoints, t);
            float dist = Vector3.Distance(prev, point);
            totalLength += dist;
            samples.Add(new Sample { t = t, distance = totalLength });
            prev = point;
        }
    }

    // Convert form distance → t (interpolated)
    public float GetTForDistance(float d)
    {
        if (d <= 0f) return 0f;
        if (d >= totalLength) return 1f;

        for (int i = 1; i < samples.Count; i++)
        {
            if (d <= samples[i].distance)
            {
                float d0 = samples[i - 1].distance;
                float d1 = samples[i].distance;
                float t0 = samples[i - 1].t;
                float t1 = samples[i].t;

                float ratio = (d - d0) / (d1 - d0);
                return Mathf.Lerp(t0, t1, ratio);
            }
        }

        return 1f;
    }
}
