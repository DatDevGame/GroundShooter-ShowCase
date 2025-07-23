using System.Collections;
using System.Collections.Generic;
using LatteGames.PoolManagement;
using UnityEngine;

public class BaseBullet : MonoBehaviour
{
    public Gun gun;
    public ParticleSystem bulletVFX;

    protected Coroutine shootCoroutine;

    public virtual void Shoot()
    {
        gameObject.SetActive(true);
        if (shootCoroutine != null)
        {
            StopCoroutine(shootCoroutine);
        }
        shootCoroutine = StartCoroutine(ShootCoroutine());
    }

    protected virtual IEnumerator ShootCoroutine()
    {
        yield return null;
    }

    protected virtual void OnHit(RaycastHit hit)
    {
        if (hit.collider.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(gun);
        }

        Despawn();
    }

    protected virtual void Despawn()
    {
        if (shootCoroutine != null)
        {
            StopCoroutine(shootCoroutine);
        }
        gameObject.SetActive(false);
        PoolManager.Release(gun.prefabBullet, this);
    }
}
