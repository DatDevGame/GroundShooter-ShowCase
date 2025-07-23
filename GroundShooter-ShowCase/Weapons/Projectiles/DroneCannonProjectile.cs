using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LatteGames;
using Sirenix.OdinInspector;
using UnityEngine;

public class DroneCannonProjectile : StraightProjectile
{
    private const float MaxLength = 25f;

    [SerializeField]
    private bool absClamp;
    [SerializeField]
    private float frequency = 2f;
    [SerializeField]
    private int pointCount = 100;
    [SerializeField]
    private float rotationSpeed = 0f;
    [SerializeField]
    private AnimationCurve amplitudeCurve;
    [SerializeField]
    private LineRenderer[] lineRenderers = new LineRenderer[0];

    private bool isClone;
    private float length;
    private float previousLength;
    private Coroutine increaseTrailLengthCoroutine;

    private void Update()
    {
        GeneratePoints(length);
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }

    [Button]
    private void GeneratePoints(float length)
    {
        if (previousLength == length)
            return;
        previousLength = length;
        foreach (var lineRenderer in lineRenderers)
        {
            lineRenderer.positionCount = length <= 0 ? 0 : pointCount;
        }
        if (length <= 0)
            return;
        for (int i = 0; i < pointCount; i++)
        {
            float t = (float)i / (pointCount - 1);
            float x = Mathf.Sin((t + Time.time * 5f) * frequency * Mathf.PI) * amplitudeCurve.Evaluate(t);
            x = Mathf.Abs(x);
            if (absClamp)
            {
                x = Mathf.Abs(x);
            }
            float y = t * length;
            foreach (var lineRenderer in lineRenderers)
            {
                lineRenderer.SetPosition(i, new Vector3(x, y, 0f));
            }
        }
    }

    protected override void NotifyEventLifetimeEnded()
    {
        base.NotifyEventLifetimeEnded();
        if (increaseTrailLengthCoroutine != null)
            StopCoroutine(increaseTrailLengthCoroutine);
    }

    public override void OnReturnToPool()
    {
        isClone = false;
        base.OnReturnToPool();
        foreach (var lineRenderer in lineRenderers)
        {
            lineRenderer.enabled = false;
        }
    }

    public override void Launch(UnitStats unitStats, LayerMask hitLayers)
    {
        base.Launch(unitStats, hitLayers);
        foreach (var lineRenderer in lineRenderers)
        {
            lineRenderer.enabled = true;
            lineRenderer.positionCount = 0;
        }
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0f);
        if (!isClone)
        {
            length = 0f;
            increaseTrailLengthCoroutine = StartCoroutine(CommonCoroutine.LerpFactor(1f, t =>
            {
                length = t * MaxLength;
            }));
        }
    }

    public override BaseProjectile Clone(Vector3 position, Quaternion rotation)
    {
        var clone = base.Clone(position, rotation) as DroneCannonProjectile;
        clone.length = length;
        clone.isClone = true;
        return clone;
    }
}