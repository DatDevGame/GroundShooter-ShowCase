using LatteGames.PoolManagement;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class BaseMissile : MonoBehaviour
{
    [SerializeField, BoxGroup("Ref")] protected Transform m_PointExplosion;

    [SerializeField, BoxGroup("Config")] protected float m_Speed = 5f;
    [SerializeField, BoxGroup("Config")] protected float m_LifeTime = 10f;

    [SerializeField, BoxGroup("Resource")] protected ParticleSystem m_ExplosionVFXPrefab;

    protected Transform m_Target;
    protected float m_Timer;
    protected BaseTurret m_BaseTurret;
    protected BaseMissile m_BaseMissilePrefab;

    public virtual void SetTarget(Transform target, BaseTurret baseTurret)
    {
        m_Timer = 0;
        m_Target = target;
        m_BaseTurret = baseTurret;
        m_BaseMissilePrefab = m_BaseTurret.BaseMissilePrefab;
    }

    public virtual void SetBaseTurret(BaseTurret baseTurret)
    {
        m_BaseTurret = baseTurret;
        m_BaseMissilePrefab = m_BaseTurret.BaseMissilePrefab;
    }

    protected virtual void Update()
    {
        m_Timer += Time.deltaTime;
        if (m_Timer >= m_LifeTime)
        {
            OnExpire();
            return;
        }

        MoveMissile();
    }

    protected abstract void MoveMissile();

    protected virtual void OnExpire()
    {
        Despawn();
    }
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IDamageable damageable))
        {
            if(m_BaseTurret != null)
                damageable.TakeDamage(m_BaseTurret);
            Despawn();

            var explosionVFX = PoolManager.GetOrCreatePool(m_ExplosionVFXPrefab).Get();
            explosionVFX.transform.position = m_PointExplosion.position;
            explosionVFX.Play();
            explosionVFX.Release(m_ExplosionVFXPrefab, explosionVFX.main.duration);
        }
    }

    protected virtual void Despawn()
    {
        gameObject.SetActive(false);
        if(PoolManager.Instance != null)
            PoolManager.Release(m_BaseMissilePrefab, this);
    }
}
