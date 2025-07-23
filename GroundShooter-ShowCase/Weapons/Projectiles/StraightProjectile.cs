using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class StraightProjectile : BaseProjectile
{
    protected readonly static RaycastHit[] CachedHitInfo = new RaycastHit[1];

    HashSet<IDamageable> damageableHashSet = new HashSet<IDamageable>();

    public override void Launch(UnitStats unitStats, LayerMask hitLayers)
    {
        base.Launch(unitStats, hitLayers);
        float projectileSpeed = unitStats.GetStatValue(StatType.ProjectileSpeed);
        float projectileRadius = unitStats.GetStatValue(StatType.ProjectileRadius);
        float projectileMaxDistance = unitStats.GetInitialStatValue(StatType.ProjectileMaxDistance);
        int projectileMaxPenetration = (int)unitStats.GetInitialStatValue(StatType.ProjectileMaxPenetration) + 1;
        damageableHashSet.Clear();
        Vector3 lastPosition = transform.position;
        transform.DOKill(true);
        transform.DOMove(transform.position + transform.forward * projectileMaxDistance, projectileSpeed)
            .SetSpeedBased()
            .SetEase(Ease.Linear)
            .OnUpdate(() =>
            {
                if (projectileMaxPenetration <= 0)
                    return;
                var delta = transform.position - lastPosition;
                // Raycast from previous position to new position
                if (Physics.SphereCastNonAlloc(lastPosition, projectileRadius, transform.forward, CachedHitInfo, delta.magnitude, hitLayers, QueryTriggerInteraction.UseGlobal) > 0 && CachedHitInfo[0].collider.TryGetComponent(out IDamageable damageableObject) && !damageableHashSet.Contains(damageableObject))
                {
                    damageableHashSet.Add(damageableObject);
                    projectileMaxPenetration--;
                    NotifyEventTargetHit(damageableObject, CachedHitInfo[0]);
                    if (projectileMaxPenetration <= 0)
                    {
                        transform.DOKill();
                        NotifyEventLifetimeEnded();
                    }
                }
                lastPosition = transform.position;
            })
            .OnComplete(NotifyEventLifetimeEnded);
        projectileParticleVFX.Play();
    }
}