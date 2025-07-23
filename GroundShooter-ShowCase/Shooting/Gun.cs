using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour, IAttackable
{
    [Header("Refs")]
    public Unit unit;
    public UnitStats unitStats;
    public BaseBullet prefabBullet;

    [Header("General")]
    public bool canFire = true;
    public float bulletRadius = 1f;
    public float fireRate = 1f;
    public float bulletSpeed = 5f;
    public LayerMask hitLayers;
    public float maxDistance = 50f;

    [Header("Patterns")]
    public GunPattern[] patterns;
    public bool cyclePatterns = true;       // Rotate through patterns?
    public bool drawRuntimeGizmos = false;  // Toggle in play mode

    protected int patternIndex;
    protected Coroutine loop;

    protected virtual GunPattern Current => patterns != null && patterns.Length > 0
                      ? patterns[patternIndex]
                      : null;

    protected virtual void OnEnable() => loop = StartCoroutine(FireLoop());
    protected virtual void OnDisable() { if (loop != null) StopCoroutine(loop); }

    protected virtual void Start()
    {

    }

#if UNITY_EDITOR   // Draw even in edit mode
    protected virtual void OnDrawGizmos()
    {
        if (!Application.isPlaying) PreviewEditGizmos();
    }

    protected virtual void OnDrawGizmosSelected()
    {
        if (Application.isPlaying && drawRuntimeGizmos && Current != null)
            Current.DrawGizmos(this);
    }

    protected virtual void PreviewEditGizmos()
    {
        if (Current != null)
            Current.DrawGizmos(this);
    }
#endif

    protected virtual IEnumerator FireLoop()
    {
        // First frame safety
        yield return null;

        while (true)
        {
            if (!canFire)
            {
                yield return new WaitUntil(() => canFire);
            }
            float wait = 1f / (unit.Stats.GetStatValue(StatType.AttackSpeed) * fireRate);
            yield return new WaitForSeconds(wait);

            if (Current != null)
                yield return Current.Execute(this);

            if (cyclePatterns)
                patternIndex = (patternIndex + 1) % patterns.Length;
        }
    }

    public virtual void Init(Unit unit, UnitStats unitStats)
    {
        this.unit = unit;
        this.unitStats = unitStats;
        fireRate = unitStats.GetStatValue(StatType.FireRate, fireRate);
        bulletSpeed = unitStats.GetStatValue(StatType.ProjectileSpeed, bulletSpeed);
    }

    public float GetInstantKillChance()
    {
        return unit.Stats.GetStatValue(StatType.AttackDamage, 0f);
    }

    public float GetCriticalHitChance()
    {
        return unit.Stats.GetStatValue(StatType.CritChance, 0f);
    }

    public float GetCriticalDamageMultiplier()
    {
        return unit.Stats.GetStatValue(StatType.CritDamageMultiplier, 2f);
    }

    public float GetAttackDamage()
    {
        return unit.Stats.GetStatValue(StatType.AttackDamage, 0f);
    }
}