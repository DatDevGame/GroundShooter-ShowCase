using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class Weapon : MonoBehaviour
{
    protected int starLevel = 0;
    protected WeaponSO weaponSO;
    [ShowInInspector, ReadOnly]
    protected UnitStats unitStats;
    protected Unit unit;

    public int StarLevel => starLevel;
    public WeaponType WeaponType => WeaponSO.WeaponType;
    public WeaponSO WeaponSO => weaponSO;

    public virtual void Init(WeaponSO weaponSO, Unit unit)
    {
        this.weaponSO = weaponSO;
        this.unit = unit;
        unitStats = new UnitStats();
        unitStats.Init(WeaponSO.GetInitialStatDictionary());
        if (unitStats.GetStatValue(StatType.MaxHealth) > 0)
            unit.Stats.AddModifier(StatType.MaxHealth, new StatModifier(unitStats.GetStatValue(StatType.MaxHealth), StatModifierType.Flat, this));
    }

    // Increase star level (used by in-game power-ups)
    [Button]
    public virtual void IncreaseStarLevel(int amount)
    {
        for (int i = starLevel; i < starLevel + amount; i++)
        {
            var battleSkillStage = WeaponSO.WeaponBattleSkillStages[i];
            if (battleSkillStage.IsModifyStat)
            {
                foreach (var statModifier in battleSkillStage.StatModifiers)
                {
                    unitStats.AddModifier(statModifier.statType, new StatModifier(statModifier.value, statModifier.modType, this));
                }
            }
        }
        starLevel += amount;
    }

    // Reset start level
    [Button]
    public virtual void ResetStarLevel()
    {
        starLevel = 0;
    }

    public virtual void ActivePreview()
    {
        this.enabled = false;
    }
}