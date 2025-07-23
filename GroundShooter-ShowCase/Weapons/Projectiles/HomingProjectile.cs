using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class HomingProjectile : BaseProjectile
{
    private static readonly RaycastHit[] CachedHitInfo = new RaycastHit[1];
    private static readonly Collider[] CachedOverlapColliders = new Collider[100];

    [SerializeField]
    private float homingRadius = 5f;
    [SerializeField]
    private float rotationSpeed = 10f;
    [SerializeField]
    private float lifeTime = 10f;
    [SerializeField]
    private TrailRenderer trailRenderer;

    [ShowInInspector, ReadOnly]
    private IDamageable target;

    private IEnumerator Fire_CR(UnitStats unitStats, LayerMask hitLayers)
    {
        float lifeTime = this.lifeTime;
        float projectileSpeed = unitStats.GetStatValue(StatType.ProjectileSpeed);
        float projectileRadius = unitStats.GetStatValue(StatType.ProjectileRadius);
        Vector3 lastPosition = transform.position;
        projectileParticleVFX.Play();
        while (true)
        {
            if (target != null && !target.IsDead())
            {
                RotateTowardsTarget();
            }
            else
            {
                FindTarget(hitLayers);
            }
            MoveForward(projectileSpeed);
            var delta = transform.position - lastPosition;
            // Raycast from previous position to new position
            if (Physics.SphereCastNonAlloc(lastPosition, projectileRadius, transform.forward, CachedHitInfo, delta.magnitude, hitLayers, QueryTriggerInteraction.Collide) > 0 && CachedHitInfo[0].collider.TryGetComponent(out IDamageable damageableObject))
            {
                NotifyEventTargetHit(damageableObject, CachedHitInfo[0]);
                NotifyEventLifetimeEnded();
                yield break;
            }
            lastPosition = transform.position;

            lifeTime -= Time.deltaTime;
            if (lifeTime <= 0)
            {
                NotifyEventLifetimeEnded();
                yield break;
            }
            yield return null;
        }
    }

    void MoveForward(float bulletSpeed)
    {
        transform.position += transform.forward * bulletSpeed * Time.deltaTime;
    }

    void FindTarget(LayerMask hitLayers)
    {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, homingRadius, CachedOverlapColliders, hitLayers);
        if (hitCount > 0)
        {
            float closestDist = Mathf.Infinity;
            Transform closest = null;

            for (int i = 0; i < hitCount; i++)
            {
                var hit = CachedOverlapColliders[i];
                if (hit == null)
                {
                    continue;
                }
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = hit.transform;
                }
            }

            if (closest != null)
            {
                target = closest.GetComponent<IDamageable>();
            }
        }
    }

    void RotateTowardsTarget()
    {
        Vector3 dir = (target.GetTransform().position - transform.position).normalized;
        dir.y = 0; // Keep the bullet level
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, homingRadius);
    }

    public override void Launch(UnitStats unitStats, LayerMask hitLayers)
    {
        base.Launch(unitStats, hitLayers);
        if (trailRenderer != null)
            trailRenderer.Clear();
        StartCoroutine(Fire_CR(unitStats, hitLayers));
    }
}