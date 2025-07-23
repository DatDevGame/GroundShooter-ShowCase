using LatteGames.PoolManagement;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TrackingMissile : BaseMissile
{
    [SerializeField, BoxGroup("Config")] private float m_TurnSpeed = 90f;
    [SerializeField, BoxGroup("Config")] private float m_DetectAngle = 45f;
    [SerializeField, BoxGroup("Config")] private float m_DetectRadius = 10f;

    protected override void MoveMissile()
    {
        if (m_Target == null)
        {
            OnExpire();
            return;
        }

        Vector3 targetPos = m_Target.position + Vector3.up;
        Vector3 directionToTarget = (targetPos - transform.position).normalized;

        float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
        if (angleToTarget <= m_DetectAngle / 2f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, m_TurnSpeed * Time.deltaTime);
        }

        transform.position += transform.forward * m_Speed * Time.deltaTime;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        var explosionVFX = PoolManager.GetOrCreatePool(m_ExplosionVFXPrefab).Get();
        explosionVFX.transform.position = m_PointExplosion.position;
        explosionVFX.Play();
        explosionVFX.Release(m_ExplosionVFXPrefab, explosionVFX.main.duration);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector3 forward = transform.forward;

        Vector3 leftBoundary = Quaternion.Euler(0, -m_DetectAngle / 2f, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, m_DetectAngle / 2f, 0) * forward;

        Gizmos.DrawRay(transform.position, leftBoundary * m_DetectRadius);
        Gizmos.DrawRay(transform.position, rightBoundary * m_DetectRadius);

        int segments = 30;
        float deltaAngle = m_DetectAngle / segments;

        Vector3 prevPoint = transform.position + (Quaternion.Euler(0, -m_DetectAngle / 2f, 0) * forward) * m_DetectRadius;

        for (int i = 1; i <= segments; i++)
        {
            float angle = -m_DetectAngle / 2f + deltaAngle * i;
            Vector3 nextPoint = transform.position + (Quaternion.Euler(0, angle, 0) * forward) * m_DetectRadius;
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }
#endif
}
