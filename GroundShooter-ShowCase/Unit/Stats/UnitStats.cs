using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[Serializable]
public class UnitStats
{
    private readonly static Dictionary<UnitSO, UnitStats> CachedUnitStatsDictionary = new Dictionary<UnitSO, UnitStats>();

    [ShowInInspector, ReadOnly]
    private Dictionary<StatType, Stat> initialStatDictionary;
    [ShowInInspector, ReadOnly]
    private Dictionary<StatType, Stat> runtimeStatDictionary;

    public void Init(Dictionary<StatType, Stat> initialStatDictionary, bool deepClone = false)
    {
        if (!deepClone)
        {
            this.initialStatDictionary = initialStatDictionary;
            runtimeStatDictionary = initialStatDictionary;
        }
        else
        {
            this.initialStatDictionary = initialStatDictionary;
            runtimeStatDictionary = new Dictionary<StatType, Stat>();
            foreach (var kvp in initialStatDictionary)
            {
                runtimeStatDictionary.Add(kvp.Key, kvp.Value.Clone());
            }
        }
    }

    public Stat GetInitialStat(StatType statType)
    {
        return initialStatDictionary.Get(statType);
    }

    public Stat GetStat(StatType statType)
    {
        return runtimeStatDictionary.Get(statType);
    }

    public float GetInitialStatValue(StatType statType, float defaultValue = 0)
    {
        if (initialStatDictionary.TryGetValue(statType, out Stat stat))
        {
            return stat.Value;
        }
        return defaultValue;
    }

    public float GetStatValue(StatType statType, float defaultValue = 0)
    {
        if (runtimeStatDictionary.TryGetValue(statType, out Stat stat))
        {
            return stat.Value;
        }
        return defaultValue;
    }

    public void AddModifier(StatType statType, StatModifier modifier)
    {
        if (runtimeStatDictionary.ContainsKey(statType))
            runtimeStatDictionary[statType].AddModifier(modifier);
    }

    public void RemoveLastModifierFromSource(object source)
    {
        foreach (var stat in runtimeStatDictionary.Values)
        {
            stat.RemoveLastModifierFromSource(source);
        }
    }

    public void RemoveAllModifiersFromSource(object source)
    {
        foreach (var stat in runtimeStatDictionary.Values)
            stat.RemoveAllModifiersFromSource(source);
    }

    public static UnitStats GetOrCreateUnitStats(Unit unit)
    {
        if (unit == null || unit.UnitSO == null)
            return null;
        UnitStats unitStats = default;
        if (unit.IsPlayer())
        {
            unitStats = new UnitStats();
            unitStats.Init(unit.UnitSO.InitialStatDictionary, true);
        }
        else if (!CachedUnitStatsDictionary.TryGetValue(unit.UnitSO, out unitStats))
        {
            unitStats = new UnitStats();
            unitStats.Init(unit.UnitSO.InitialStatDictionary, false);
            CachedUnitStatsDictionary.Add(unit.UnitSO, unitStats);
        }
        return unitStats;
    }
}