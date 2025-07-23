using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using HyrphusQ.Helpers;
using LatteGames.PoolManagement;
using LatteGames.Template;
using UnityEngine;

public class AssaultGun : MainGunWeapon
{
    [Serializable]
    public struct BurstConfig
    {
        public Range<float> delayRandomRange;
        public float angleOffset;
        public float positionOffset;
        public float rotation;
    }

    [Serializable]
    public struct BurstWaveConfig
    {
        public List<BurstConfig> burstWaveConfig;
    }

    public GameObject[] models;
    [SerializeField]
    protected Transform bulletSpawnPoint;
    [SerializeField]
    protected StraightProjectile projectilePrefab;
    [SerializeField]
    protected ParticleSystem hitExplosionVFXPrefab;
    [SerializeField]
    protected ParticleSystem muzzleFlashVFX;
    [SerializeField]
    protected List<BurstWaveConfig> burstWaveConfigLevels;

    protected float attackCooldown;

    protected float AttackSpeed => unit.Stats.GetStatValue(StatType.AttackSpeed);
    protected float FireRate => unitStats.GetStatValue(StatType.FireRate);

    private void OnDestroy()
    {
        if (PoolManager.Instance != null)
            PoolManager.Clear(projectilePrefab.GetInstanceID().ToString());
    }

    protected virtual void Update()
    {
        attackCooldown = Mathf.Max(attackCooldown - Time.deltaTime * AttackSpeed, 0f);
        if (attackCooldown <= 0f)
        {
            Fire();
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
        if (burstWaveConfigLevels.Count <= 0)
            return;
        int level = 0;
        BurstWaveConfig burstWaveConfig = burstWaveConfigLevels[level];
        for (int i = 0; i < burstWaveConfig.burstWaveConfig.Count; i++)
        {
            BurstConfig burstConfig = burstWaveConfig.burstWaveConfig[i];
            Vector3 position = bulletSpawnPoint.TransformPoint(Quaternion.Euler(0f, burstConfig.angleOffset, 0f) * Vector3.forward * burstConfig.positionOffset);
            Gizmos.color = i % 2 == 0 ? Color.red : Color.blue;
            Gizmos.DrawWireSphere(position, 0.15f);
        }
    }

    protected virtual void LaunchProjectile(BurstConfig burstConfig)
    {
        Vector3 position = bulletSpawnPoint.TransformPoint(Quaternion.Euler(0f, burstConfig.angleOffset, 0f) * Vector3.forward * burstConfig.positionOffset);
        Quaternion rotation = bulletSpawnPoint.rotation * Quaternion.Euler(0f, burstConfig.rotation, 0f);
        var projectile = PoolManager.GetOrCreatePool(projectilePrefab).Get();
        projectile.transform.SetPositionAndRotation(position, rotation);
        projectile.Launch(unitStats, hitLayers);
        projectile.OnTargetHit += OnTargetHit;
        projectile.OnLifetimeEnded += OnLifetimeEnded;
        muzzleFlashVFX.Play();
    }

    protected virtual void OnTargetHit(BaseProjectile projectile, IDamageable damageableObject, RaycastHit hitInfo)
    {
        var hitExplosionVFX = PoolManager.GetOrCreatePool(hitExplosionVFXPrefab).Get();
        hitExplosionVFX.transform.position = hitInfo.point == Vector3.zero ? new Vector3(hitInfo.collider.transform.position.x, projectile.transform.position.y, hitInfo.collider.transform.position.z) : hitInfo.point;
        hitExplosionVFX.Play();
        hitExplosionVFX.Release(hitExplosionVFXPrefab, hitExplosionVFX.main.duration);

        damageableObject.TakeDamage(this);
    }

    protected virtual void OnLifetimeEnded(BaseProjectile projectile)
    {
        projectile.OnTargetHit -= OnTargetHit;
        projectile.OnLifetimeEnded -= OnLifetimeEnded;
        Destroy(projectile.gameObject);

        //PoolManager.Release(projectilePrefab, projectile);
    }

    protected virtual IEnumerator LaunchProjectile_CR(BurstConfig burstConfig)
    {
        float delay = burstConfig.delayRandomRange.RandomRange();
        if (delay > 0f)
        {
            yield return Yielders.Get(delay);
        }
        LaunchProjectile(burstConfig);
    }

    protected virtual void PlayRecoilAnim()
    {
        if (DOTween.IsTweening(transform))
            return;
        transform
            .DOPunchPosition(transform.InverseTransformDirection(-transform.forward) * 0.15f, 0.2f, 10, 1)
            .SetEase(Ease.Linear)
            .SetUpdate(UpdateType.Late);
    }

    public void Fire()
    {
        attackCooldown = 1f / FireRate;
        var burstWaveConfigLevel = burstWaveConfigLevels[starLevel];
        foreach (var burstConfig in burstWaveConfigLevel.burstWaveConfig)
        {
            StartCoroutine(LaunchProjectile_CR(burstConfig));
        }
        PlayRecoilAnim();
        SoundManager.Instance.PlaySFX(SFX.AssaultRifle, 0.08f);
    }
}