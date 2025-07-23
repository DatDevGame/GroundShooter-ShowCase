using UnityEngine;
using LatteGames.PoolManagement;
using Sirenix.OdinInspector;

public class TwinBarrelTurret : BaseTurret
{
    [SerializeField, BoxGroup("Resource")] private Transform[] m_FirePoints;

    protected override void Attack()
    {
        if (m_CurrentTarget == null) return;

        foreach (var firePoint in m_FirePoints)
        {
            if (firePoint == null) continue;

            BaseMissile missile = PoolManager.GetOrCreatePool(m_BaseMissilePrefab, initialCapacity: 1).Get();
            if (missile != null)
            {
                missile.transform.position = firePoint.position;
                missile.transform.rotation = firePoint.rotation;
                missile.SetTarget(m_CurrentTarget, this);
            }
        }
    }
}
