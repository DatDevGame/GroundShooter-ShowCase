using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainGunWeapon : Weapon, IAttackable
{
    [SerializeField]
    protected LayerMask hitLayers;
    [SerializeField]
    protected Transform rightHandTarget, rightHandHint;
    [SerializeField]
    protected Transform leftHandTarget, leftHandHint;

    public virtual float GetAttackDamage()
    {
        return unit.Stats.GetStatValue(StatType.AttackDamage) + unitStats.GetStatValue(StatType.AttackDamage);
    }

    public virtual float GetCriticalDamageMultiplier()
    {
        return unit.Stats.GetStatValue(StatType.CritDamageMultiplier, 2f);
    }

    public virtual float GetCriticalHitChance()
    {
        return unit.Stats.GetStatValue(StatType.CritChance);
    }

    public virtual float GetInstantKillChance()
    {
        return unit.Stats.GetStatValue(StatType.InstantKillChance);
    }

    public override void Init(WeaponSO weaponSO, Unit unit)
    {
        base.Init(weaponSO, unit);
        if (unit is PlayerUnit playerUnit)
            playerUnit.SetupIKConstraints(rightHandTarget, rightHandHint, leftHandTarget, leftHandHint);
    }
}