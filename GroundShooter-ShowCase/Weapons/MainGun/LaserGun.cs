using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Coffee.UIParticleExtensions;
using DG.Tweening;
using LatteGames;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class LaserGun : MainGunWeapon
{
    protected readonly static RaycastHit[] RaycastHits = new RaycastHit[100];

    [Serializable]
    public struct LaserConfig
    {
        public float innerThicknessMultiplier;
        public float outerThicknessMultiplier;
        public bool hyperChargeLaser;
        public Material innerLaserMaterial;
        public Material outerLasermaterial;
        public ParticleSystem hitParticleVFX;
        public ParticleSystem energyChargeVFX;
    }

    [SerializeField]
    protected float superLaserReloadTime = 5f;
    [SerializeField]
    protected float superLaserDuration = 3f;
    [SerializeField]
    protected float superLaserRadius = 1f;
    [SerializeField]
    protected float superLaserAttackDamageMultiplier = 2f;
    [SerializeField]
    protected float refractionLaserWidthMultiplier = 1.25f;
    [SerializeField]
    protected float refractionMaxDistance = 10f;
    [SerializeField]
    protected float distancePerSegment = 5f;
    [SerializeField]
    protected Transform bulletSpawnPoint;
    [SerializeField]
    protected LineRenderer innerLaserLineRenderer, outerLaserLineRenderer, superLaserLineRenderer;
    [SerializeField]
    protected AnimationCurve segmentDistanceToSpeedCurve;
    [SerializeField]
    protected ParticleSystem superLaserEnergyChargeVFX;
    [SerializeField]
    protected List<LaserConfig> laserConfigLevels;

    protected float dealDamageCooldown;
    protected float superLaserCooldown;
    protected ParticleSystem hitParticleVFX;
    protected ParticleSystem energyChargeVFX;
    protected Vector3 superLaserEnergyChargeOriginalScale;
    protected List<Vector3> segmentPoints = new List<Vector3>();
    protected List<IDamageable> excludeDamageableObjects = new List<IDamageable>();

    protected float AttackSpeed
    {
        get
        {
            return unit.Stats.GetStatValue(StatType.AttackSpeed, 1f);
        }
    }
    protected ParticleSystem HitParticleVFX
    {
        get => hitParticleVFX;
        set
        {
            if (hitParticleVFX != null && hitParticleVFX.isPlaying)
                hitParticleVFX.Stop();
            hitParticleVFX = value;
        }
    }
    protected ParticleSystem EnergyChargeVFX
    {
        get => energyChargeVFX;
        set
        {
            if (energyChargeVFX != null && energyChargeVFX.isPlaying)
                energyChargeVFX.Stop();
            energyChargeVFX = value;
            energyChargeVFX.Play();
        }
    }

    protected virtual void Start()
    {
        superLaserCooldown = superLaserReloadTime + superLaserDuration;
        superLaserEnergyChargeOriginalScale = superLaserEnergyChargeVFX.transform.localScale;
        UpdateVisual();
    }

    protected virtual void Update()
    {
        HandleBasicLaserAttack();
        HandleSuperLaserAttack();
    }

    protected virtual void HandleBasicLaserAttack()
    {
        dealDamageCooldown = Mathf.Max(dealDamageCooldown - Time.deltaTime * AttackSpeed, 0f);

        bool isHit = Physics.SphereCast(bulletSpawnPoint.position, unitStats.GetStatValue(StatType.ProjectileRadius), bulletSpawnPoint.forward, out RaycastHit hitInfo, unitStats.GetStatValue(StatType.ProjectileMaxDistance), hitLayers, QueryTriggerInteraction.Ignore);
        if (isHit)
        {
            if (HitParticleVFX.isStopped)
                HitParticleVFX.Play();
            HitParticleVFX.transform.position = hitInfo.point;
            RenderLaser(hitInfo.point);
            if (dealDamageCooldown <= 0f && hitInfo.collider.TryGetComponent(out IDamageable damageableObject))
            {
                dealDamageCooldown = 1f / unitStats.GetStatValue(StatType.FireRate);
                List<IDamageable> excludeDamageableObjects = CollectionPool<List<IDamageable>, IDamageable>.Get();
                DealDamageRecursive(damageableObject, (int)unitStats.GetStatValue(StatType.ProjectileMaxRefraction), excludeDamageableObjects, () =>
                {
                    CollectionPool<List<IDamageable>, IDamageable>.Release(excludeDamageableObjects);
                });
            }
        }
        else
        {
            if (!HitParticleVFX.isStopped)
                HitParticleVFX.Stop();
            RenderLaser(bulletSpawnPoint.position + bulletSpawnPoint.forward * unitStats.GetStatValue(StatType.ProjectileMaxDistance));
        }
    }

    protected virtual void HandleSuperLaserAttack()
    {
        LaserConfig laserConfig = laserConfigLevels[starLevel];
        if (laserConfig.hyperChargeLaser)
        {
            superLaserCooldown = Mathf.Max(superLaserCooldown - Time.deltaTime, 0f);
            if (superLaserCooldown <= 1f && !superLaserEnergyChargeVFX.isPlaying)
            {
                superLaserEnergyChargeVFX.Play();
                superLaserEnergyChargeVFX.transform.localScale = Vector3.zero;
                superLaserEnergyChargeVFX.transform.DOScale(superLaserEnergyChargeOriginalScale, 0.5f);
            }
            if (superLaserCooldown <= 0f)
            {
                StartCoroutine(LaunchSuperLaser_CR());
            }
        }
    }

    protected virtual IEnumerator LaunchSuperLaser_CR()
    {
        superLaserCooldown = superLaserReloadTime + superLaserDuration;
        superLaserLineRenderer.enabled = true;
        StartCoroutine(CommonCoroutine.LerpFactor(0.2f, t =>
        {
            superLaserLineRenderer.SetPosition(1, new Vector3(0f, 0f, 75f * t));
        }));
        float t = 0f;
        float dealDamageCooldown = 0f;
        while (t < superLaserDuration)
        {
            float deltaTime = Time.deltaTime;
            t += deltaTime;
            dealDamageCooldown = Mathf.Max(dealDamageCooldown - deltaTime * AttackSpeed, 0f);
            if (dealDamageCooldown <= 0f)
            {
                dealDamageCooldown = 1f / unitStats.GetStatValue(StatType.FireRate);
                int hitCount = Physics.SphereCastNonAlloc(bulletSpawnPoint.position, superLaserRadius, bulletSpawnPoint.forward, RaycastHits, 75f, hitLayers, QueryTriggerInteraction.Ignore);
                for (int i = 0; i < hitCount; i++)
                {
                    if (RaycastHits[i].collider.TryGetComponent(out IDamageable damageableObject))
                    {
                        damageableObject.TakeDamage(new HitAttack(GetAttackDamage() * superLaserAttackDamageMultiplier, GetCriticalHitChance(), GetCriticalDamageMultiplier(), GetInstantKillChance()));
                    }
                }
            }
            yield return null;
        }
        yield return CommonCoroutine.LerpFactor(0.1f, t =>
        {
            superLaserLineRenderer.SetPosition(1, new Vector3(0f, 0f, 75f * (1f - t)));
        });
        superLaserLineRenderer.enabled = false;
        superLaserEnergyChargeVFX.Stop();
    }

    protected virtual void DealDamageRecursive(IDamageable damageableObject, int maxRefraction, List<IDamageable> excludeDamageableObjects, Action onCompleted)
    {
        excludeDamageableObjects.Add(damageableObject);
        damageableObject.TakeDamage(this);
        if (maxRefraction <= 0)
        {
            onCompleted.Invoke();
            return;
        }
        var closestEnemyUnit = EnemyUnit.ActiveEnemies
            .Except(excludeDamageableObjects)
            .Where(item => Vector3.Distance(item.GetTransform().position, damageableObject.GetTransform().position) < refractionMaxDistance)
            .OrderBy(item => Vector3.Distance(item.GetTransform().position, damageableObject.GetTransform().position))
            .Take(1)
            .FirstOrDefault();
        if (!closestEnemyUnit.IsUnityNull())
        {
            float delayTime = excludeDamageableObjects.Count * 0.05f;
            CreateRefractionLineRenderer(damageableObject.GetTransform(), closestEnemyUnit.GetTransform());
            StartCoroutine(CommonCoroutine.Delay(delayTime, false, () =>
            {
                DealDamageRecursive(closestEnemyUnit, maxRefraction - 1, excludeDamageableObjects, onCompleted);
            }));
        }
        else
        {
            onCompleted.Invoke();
        }
    }

    protected virtual void CreateRefractionLineRenderer(Transform from, Transform to)
    {
        var innerLaserLineRenderer = Instantiate(this.innerLaserLineRenderer, transform);
        innerLaserLineRenderer.enabled = true;
        innerLaserLineRenderer.textureScale = new Vector2(innerLaserLineRenderer.textureScale.x, 0.6f);
        var outerLaserLineRenderer = Instantiate(this.outerLaserLineRenderer, transform);
        outerLaserLineRenderer.enabled = true;
        outerLaserLineRenderer.textureScale = new Vector2(outerLaserLineRenderer.textureScale.x, 0.4f);
        Vector3 fromPos = new Vector3(from.transform.position.x, 1f, from.transform.position.z);
        Vector3 toPos = new Vector3(to.transform.position.x, 1f, to.transform.position.z);
        Vector3 hitPos = toPos + (fromPos - toPos).normalized * 0.75f;
        Vector3 originalLocalScale = HitParticleVFX.transform.localScale;
        var hitParticleVFX = Instantiate(HitParticleVFX, transform);
        hitParticleVFX.transform.SetPositionAndRotation(hitPos, Quaternion.identity);
        hitParticleVFX.Play();
        StartCoroutine(CommonCoroutine.LerpFactor(0.1f, t =>
        {
            innerLaserLineRenderer.positionCount = 2;
            innerLaserLineRenderer.SetPosition(0, fromPos);
            innerLaserLineRenderer.SetPosition(1, toPos);
            innerLaserLineRenderer.widthMultiplier = (1f - t) * refractionLaserWidthMultiplier;
            outerLaserLineRenderer.positionCount = 2;
            outerLaserLineRenderer.SetPosition(0, fromPos);
            outerLaserLineRenderer.SetPosition(1, toPos);
            outerLaserLineRenderer.widthMultiplier = (1f - t) * refractionLaserWidthMultiplier;
            hitParticleVFX.transform.localScale = (1f - t) * originalLocalScale;
            if (t >= 1f)
            {
                Destroy(innerLaserLineRenderer.gameObject);
                Destroy(outerLaserLineRenderer.gameObject);
                Destroy(hitParticleVFX.gameObject);
            }
        }));
    }

    protected virtual void RenderLaser(Vector3 endPoint)
    {
        float distanceToEndPoint = Mathf.Round(Vector3.Distance(endPoint, bulletSpawnPoint.position));
        int segmentCount = Mathf.CeilToInt(distanceToEndPoint / distancePerSegment);
        for (int i = 0; i < segmentCount; i++)
        {
            Vector3 targetPoint = bulletSpawnPoint.position + i * distancePerSegment * bulletSpawnPoint.forward;
            Vector3 currentPoint = i < segmentPoints.Count ? segmentPoints[i] : targetPoint;
            Vector3 point = Vector3.MoveTowards(currentPoint, targetPoint, segmentDistanceToSpeedCurve.Evaluate(i * distancePerSegment / distanceToEndPoint) * 50f * Time.deltaTime);
            if (i < segmentPoints.Count)
            {
                segmentPoints[i] = point;
            }
            else
            {
                if (segmentPoints.Count > 0)
                    point.x = segmentPoints[^1].x;
                segmentPoints.Add(point);
            }
        }
        segmentPoints.RemoveRange(segmentCount, Mathf.Max(segmentPoints.Count - segmentCount, 0));
        Vector3[] segmentPointArr = segmentPoints.ToArray();
        innerLaserLineRenderer.positionCount = segmentPoints.Count;
        innerLaserLineRenderer.SetPositions(segmentPointArr);
        outerLaserLineRenderer.positionCount = segmentPoints.Count;
        outerLaserLineRenderer.SetPositions(segmentPointArr);
    }

    protected virtual void UpdateVisual()
    {
        LaserConfig laserConfig = laserConfigLevels[starLevel];
        innerLaserLineRenderer.textureScale = new Vector2(1f, 1.5f / laserConfig.innerThicknessMultiplier);
        outerLaserLineRenderer.textureScale = new Vector2(1f, 0.5f / laserConfig.outerThicknessMultiplier);
        innerLaserLineRenderer.material = laserConfig.innerLaserMaterial;
        outerLaserLineRenderer.material = laserConfig.outerLasermaterial;
        HitParticleVFX = laserConfig.hitParticleVFX;
        EnergyChargeVFX = laserConfig.energyChargeVFX;
    }

    public override void IncreaseStarLevel(int amount)
    {
        base.IncreaseStarLevel(amount);
        UpdateVisual();
    }
    public override void ActivePreview()
    {
        base.ActivePreview();
        bulletSpawnPoint.gameObject.SetActive(false);
    }
}