using System.Collections;
using DG.Tweening;
using UnityEngine;

public class Bullet : BaseBullet
{
    protected Vector3 _lastPosition;
    protected RaycastHit[] _hits = new RaycastHit[1];

    protected virtual void OnEnable()
    {
        _lastPosition = transform.position;
    }

    protected override IEnumerator ShootCoroutine()
    {
        _lastPosition = transform.position;
        // bulletVFX.gameObject.SetActive(false);
        yield return null;
        // bulletVFX.gameObject.SetActive(true);

        transform.DOKill();
        transform.DOMove(transform.position + transform.forward * gun.maxDistance, gun.bulletSpeed).SetSpeedBased().SetEase(Ease.Linear).OnUpdate(() =>
        {
            var delta = transform.position - _lastPosition;
            // Raycast from previous position to new position
            if (Physics.SphereCastNonAlloc(_lastPosition, gun.bulletRadius, transform.forward, _hits, delta.magnitude, gun.hitLayers, QueryTriggerInteraction.Collide) > 0)
            {
                OnHit(_hits[0]);
                return;
            }
            _lastPosition = transform.position;
        }).OnComplete(() =>
        {
            // Check if the bullet is still active
            if (gameObject.activeSelf)
            {
                Despawn();
            }
        });
    }

    protected override void Despawn()
    {
        transform.DOKill();
        base.Despawn();
    }
}
