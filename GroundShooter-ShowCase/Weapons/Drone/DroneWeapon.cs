using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class DroneWeapon : Weapon, IAttackable
{
    [SerializeField]
    protected LayerMask hitLayers;

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
}