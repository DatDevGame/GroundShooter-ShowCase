using System;
using LatteGames.PoolManagement;
using UnityEngine;

public abstract class BaseProjectile : MonoBehaviour, IPoolEventListener
{
    public event Action<BaseProjectile, IDamageable, RaycastHit> OnTargetHit = delegate { };
    public event Action<BaseProjectile> OnLifetimeEnded = delegate { };

    [SerializeField]
    protected ParticleSystem projectileParticleVFX;
    public UnitStats UnitStats { get; set; }
    public LayerMask HitLayers { get; set; }

    protected static Transform projectileContainer;
    protected static Transform ProjectileContainer
    {
        get
        {
            if (projectileContainer == null)
            {
                projectileContainer = new GameObject("ProjectileContainer").transform;
            }
            return projectileContainer;
        }
    }

    protected virtual void NotifyEventTargetHit(IDamageable damageableObject, RaycastHit hitInfo)
    {
        OnTargetHit.Invoke(this, damageableObject, hitInfo);
    }

    protected virtual void NotifyEventLifetimeEnded()
    {
        OnLifetimeEnded.Invoke(this);
    }

    public virtual void OnCreate()
    {
        transform.parent = ProjectileContainer;
    }

    public virtual void OnDispose()
    {

    }

    public virtual void OnReturnToPool()
    {
        gameObject.SetActive(false);
        projectileParticleVFX.Stop();
    }

    public virtual void OnTakeFromPool()
    {
        gameObject.SetActive(true);
    }

    public virtual void Launch(UnitStats unitStats, LayerMask hitLayers)
    {
        UnitStats = unitStats;
        HitLayers = hitLayers;
    }

    public virtual BaseProjectile Clone(Vector3 position, Quaternion rotation)
    {
        BaseProjectile clone = Instantiate(this, position, rotation, ProjectileContainer);
        clone.name = name;
        clone.UnitStats = UnitStats;
        clone.HitLayers = HitLayers;
        clone.OnTargetHit = OnTargetHit;
        clone.OnLifetimeEnded = OnLifetimeEnded;
        return clone;
    }
}