using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class Unit : MonoBehaviour, IDamageable
{
    public event Action<Unit> OnUnitDead = delegate { };
    public event Action<Unit, float, IAttackable> OnUnitDamageTaken = delegate { };

    [SerializeField]
    protected UnitSO unitSO;
    [SerializeField]
    protected Health health;
    [ShowInInspector, ReadOnly]
    protected UnitStats stats;

    public UnitSO UnitSO
    {
        get => unitSO;
        set
        {
            unitSO = value;
            Init();
        }
    }

    public Health Health => health;
    public UnitStats Stats => stats;

    protected virtual void Awake()
    {
        Init();
    }

    protected virtual void Start()
    {

    }

    protected virtual void OnDestroy()
    {

    }

    protected virtual void NotifyEventDead()
    {
        OnUnitDead.Invoke(this);
        LGDebug.Log($"Unit {name} dead", nameof(Unit), context: this);
    }

    protected virtual void NotifyEventDamageTaken(float damage, IAttackable damageSource)
    {
        OnUnitDamageTaken.Invoke(this, damage, damageSource);
        LGDebug.Log($"Unit {name} took {damage} damage from {damageSource}", nameof(Unit), context: this);
    }

    protected virtual void Init()
    {
        stats = UnitStats.GetOrCreateUnitStats(this);
        health.Init(CreateHealthProgress());
    }

    protected virtual RangeValue<float> CreateHealthProgress()
    {
        return new RangeFloatValue(0, Stats.GetInitialStatValue(StatType.MaxHealth));
    }

    public virtual bool IsPlayer()
    {
        return false;
    }

    public virtual void TakeDamage(IAttackable damageSource)
    {
        if (IsDead())
            return;
        float dodgeChance = Stats.GetStatValue(StatType.DodgeChance);
        bool isDodge = dodgeChance > 0f && Random.value <= dodgeChance;
        if (isDodge)
        {
            // TODO: Show dodge text popup
            return;
        }
        float instantKillChance = damageSource.GetInstantKillChance();
        bool isInstantKill = instantKillChance > 0f && Random.value <= instantKillChance;
        float critChance = damageSource.GetCriticalHitChance();
        float critDamageMultiplier = damageSource.GetCriticalDamageMultiplier();
        float baseDamage = damageSource.GetAttackDamage();
        bool isCrit = critChance > 0f && Random.value <= critChance;
        float damage = isInstantKill ? GetMaxHealth() : (isCrit ? baseDamage * critDamageMultiplier : baseDamage);
        float damageReduction = Stats.GetStatValue(StatType.DamageReduction);
        if (damageReduction > 0f && !isInstantKill)
        {
            // Ensure minimum damage reduction & prevent negative damage
            damage = Mathf.Max(damage - Mathf.Max(damage * damageReduction, 1f), 0f);
        }
        Health.CurrentHealth -= damage;
        NotifyEventDamageTaken(damage, damageSource);
        if (Health.CurrentHealth <= 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        gameObject.SetActive(false);
        NotifyEventDead();
    }

    public virtual float GetCurrentHealth()
    {
        return Health.CurrentHealth;
    }

    public virtual float GetMaxHealth()
    {
        return Health.MaxHealth;
    }

    public virtual bool IsDead()
    {
        return GetCurrentHealth() <= 0f;
    }

    public virtual Transform GetTransform()
    {
        return transform;
    }

#if UNITY_EDITOR
    [Button]
    public void Dead()
    {
        HitAttack hitAttack = new HitAttack(999999);
        TakeDamage(hitAttack);
    }
#endif
}