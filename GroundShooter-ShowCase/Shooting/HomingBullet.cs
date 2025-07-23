using System.Collections;
using DG.Tweening;
using UnityEngine;

public class HomingBullet : BaseBullet
{
    public float homingRadius = 5f;
    public float rotationSpeed = 10f;
    public float lifeTime = 10f;

    private IDamageable target;
    private Vector3 _lastPosition;
    RaycastHit[] _hits = new RaycastHit[1];
    Collider[] findTargetHits = new Collider[5];
    float _lifeTime;

    protected override IEnumerator ShootCoroutine()
    {
        _lastPosition = transform.position;
        _lifeTime = lifeTime;

        while (true)
        {
            if (target != null && !target.IsDead())
            {
                RotateTowardsTarget();
            }
            else
            {
                FindTarget();
            }
            MoveForward();
            var delta = transform.position - _lastPosition;
            // Raycast from previous position to new position
            if (Physics.SphereCastNonAlloc(_lastPosition, gun.bulletRadius, transform.forward, _hits, delta.magnitude, gun.hitLayers, QueryTriggerInteraction.Collide) > 0)
            {
                OnHit(_hits[0]);
                yield break;
            }
            _lastPosition = transform.position;

            _lifeTime -= Time.deltaTime;
            if (_lifeTime <= 0)
            {
                Despawn();
                yield break;
            }
            yield return null;
        }
    }

    protected override void Despawn()
    {
        target = null;
        base.Despawn();
    }

    void MoveForward()
    {
        transform.position += transform.forward * gun.bulletSpeed * Time.deltaTime;
    }

    void FindTarget()
    {
        if (Physics.OverlapSphereNonAlloc(transform.position, homingRadius, findTargetHits, gun.hitLayers) > 0)
        {
            float closestDist = Mathf.Infinity;
            Transform closest = null;

            foreach (var hit in findTargetHits)
            {
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
}
