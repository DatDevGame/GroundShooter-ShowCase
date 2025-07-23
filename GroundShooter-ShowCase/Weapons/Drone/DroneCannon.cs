using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using HyrphusQ.Helpers;
using LatteGames.PoolManagement;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class DroneCannon : DroneWeapon
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
        public StraightProjectile projectilePrefab;
        public ParticleSystem hitExplosionVFXPrefab;
        public ParticleSystem muzzleFlashVFX, muzzleFlashVFX2;
        public List<BurstConfig> burstWaveConfig;
    }

    [SerializeField]
    protected Transform bulletSpawnPoint, bulletSpawnPoint2;
    [SerializeField]
    protected List<BurstWaveConfig> burstWaveConfigLevels;

    [SerializeField, BoxGroup("Preview")] protected float m_FrequencyPreview = 1;
    [SerializeField, BoxGroup("Preview")] protected float m_amplitudePreview = 0.05f;

    protected float attackCooldown;
    protected Vector3 bulletSpawnPointLocalPosition;
    protected Vector3 bulletSpawnPoint2LocalPosition;

    protected float AttackSpeed => unit.Stats.GetStatValue(StatType.AttackSpeed);
    protected float FireRate => unitStats.GetStatValue(StatType.FireRate);
    protected StraightProjectile ProjectilePrefab => burstWaveConfigLevels[starLevel].projectilePrefab;
    protected ParticleSystem HitExplosionVFXPrefab => burstWaveConfigLevels[starLevel].hitExplosionVFXPrefab;
    protected ParticleSystem MuzzleFlashVFX => burstWaveConfigLevels[starLevel].muzzleFlashVFX;
    protected ParticleSystem MuzzleFlashVFX2 => burstWaveConfigLevels[starLevel].muzzleFlashVFX2;
    protected bool m_PreviewMainScene = false;

    protected virtual void Start()
    {
        attackCooldown = 1f / FireRate;
        bulletSpawnPointLocalPosition = bulletSpawnPoint.localPosition;
        bulletSpawnPoint2LocalPosition = bulletSpawnPoint2.localPosition;
        bulletSpawnPoint.transform.parent = unit.transform.parent;
        bulletSpawnPoint2.transform.parent = unit.transform.parent;
    }

    protected virtual void OnDestroy()
    {
        foreach (var burstWaveConfigLevel in burstWaveConfigLevels)
        {
            if (PoolManager.Instance != null)
                PoolManager.Clear(burstWaveConfigLevel.projectilePrefab.GetInstanceID().ToString());
            if (PoolManager.Instance != null)
                PoolManager.Clear(burstWaveConfigLevel.hitExplosionVFXPrefab.GetInstanceID().ToString());
        }

        if (m_PreviewMainScene)
        {
            Destroy(bulletSpawnPoint.gameObject);
            Destroy(bulletSpawnPoint2.gameObject);
        }
    }

    protected virtual void Update()
    {
        attackCooldown = Mathf.Max(attackCooldown - Time.deltaTime * AttackSpeed, 0f);
        if (attackCooldown <= 0f)
        {
            Fire();
        }
        float frequency = 2f;
        float amplitude = 0.1f;
        float originalSpeed = 25f;
        float speed = originalSpeed * Random.Range(0.75f, 1f);
        float speed2 = originalSpeed * Random.Range(0.75f, 1f);
        Vector3 localPos = bulletSpawnPointLocalPosition;
        localPos.y = Mathf.Sin(Time.time * frequency * Mathf.PI) * amplitude;
        Vector3 localPos2 = bulletSpawnPoint2LocalPosition;
        localPos2.y = Mathf.Sin(Time.time * frequency * Mathf.PI) * amplitude;

        if (m_PreviewMainScene)
        {
            frequency = m_FrequencyPreview;
            amplitude = m_amplitudePreview;
            localPos.y = Mathf.Sin(Time.time * frequency * Mathf.PI) * amplitude;
            localPos2.y = Mathf.Sin(Time.time * frequency * Mathf.PI) * amplitude;
        }

        // Add a little rotation to make it look more like a drone
        float rotFrequency = 1f;
        float rotAmplitude = 10f;
        float rotZ = Mathf.Sin(Time.time * rotFrequency) * rotAmplitude;
        float rotZ2 = Mathf.Sin(Time.time * rotFrequency + Mathf.PI) * rotAmplitude;
        bulletSpawnPoint.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, rotZ);
        bulletSpawnPoint2.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, rotZ2);

        bulletSpawnPoint.position = Vector3.MoveTowards(bulletSpawnPoint.position, transform.TransformPoint(localPos), Time.deltaTime * speed);
        bulletSpawnPoint2.position = Vector3.MoveTowards(bulletSpawnPoint2.position, transform.TransformPoint(localPos2), Time.deltaTime * speed2);
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
            Vector3 position2 = bulletSpawnPoint2.TransformPoint(Quaternion.Euler(0f, burstConfig.angleOffset, 0f) * Vector3.forward * burstConfig.positionOffset);
            Gizmos.color = i % 2 == 0 ? Color.red : Color.blue;
            Gizmos.DrawWireSphere(position, 0.15f);
            Gizmos.DrawWireSphere(position2, 0.15f);
        }
    }

    protected virtual void LaunchProjectile(BurstConfig burstConfig, Transform bulletSpawnPoint, ParticleSystem muzzleFlashVFX)
    {
        Vector3 position = bulletSpawnPoint.TransformPoint(Quaternion.Euler(0f, burstConfig.angleOffset, 0f) * Vector3.forward * burstConfig.positionOffset);
        Quaternion rotation = bulletSpawnPoint.rotation * Quaternion.Euler(0f, burstConfig.rotation, 0f);
        var hitExplosionVFXPrefab = HitExplosionVFXPrefab;
        var projectilePool = PoolManager.GetOrCreatePool(ProjectilePrefab);
        var projectile = projectilePool.Get();
        projectile.transform.SetPositionAndRotation(position, rotation);
        projectile.Launch(unitStats, hitLayers);
        projectile.OnTargetHit += OnTargetHit;
        projectile.OnLifetimeEnded += OnLifetimeEnded;
        muzzleFlashVFX.Play();

        void OnTargetHit(BaseProjectile projectile, IDamageable damageableObject, RaycastHit hitInfo)
        {
            var hitExplosionVFX = PoolManager.GetOrCreatePool(hitExplosionVFXPrefab).Get();
            hitExplosionVFX.transform.position = hitInfo.point == Vector3.zero ? new Vector3(hitInfo.collider.transform.position.x, projectile.transform.position.y, hitInfo.collider.transform.position.z) : hitInfo.point;
            hitExplosionVFX.Play();
            hitExplosionVFX.Release(hitExplosionVFXPrefab, hitExplosionVFX.main.duration);

            damageableObject.TakeDamage(this);
        }

        void OnLifetimeEnded(BaseProjectile projectile)
        {
            projectile.OnTargetHit -= OnTargetHit;
            projectile.OnLifetimeEnded -= OnLifetimeEnded;

            //projectilePool.Release(projectile);
            Destroy(projectile.gameObject);
        }
    }

    protected virtual IEnumerator LaunchProjectile_CR(BurstConfig burstConfig)
    {
        float delay = burstConfig.delayRandomRange.RandomRange();
        if (delay > 0f)
        {
            yield return Yielders.Get(delay);
        }
        LaunchProjectile(burstConfig, bulletSpawnPoint, MuzzleFlashVFX);
        LaunchProjectile(burstConfig, bulletSpawnPoint2, MuzzleFlashVFX2);
    }

    protected virtual void PlayRecoilAnim()
    {
        if (DOTween.IsTweening(transform))
            return;
        transform
            .DOPunchPosition(transform.InverseTransformDirection(-transform.forward) * 0.1f, 0.2f, 10, 1)
            .SetEase(Ease.Linear)
            .SetUpdate(UpdateType.Late);
    }

    public void Fire()
    {
        if (m_PreviewMainScene)
            return;

        attackCooldown = 1f / FireRate;
        var burstWaveConfigLevel = burstWaveConfigLevels[starLevel];
        foreach (var burstConfig in burstWaveConfigLevel.burstWaveConfig)
        {
            StartCoroutine(LaunchProjectile_CR(burstConfig));
        }
        PlayRecoilAnim();
    }

    public void EnablePreview()
    {
        m_PreviewMainScene = true;
    }
}