using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class WeaponBattleSkillStage
{
    [Serializable]
    public class StatModifier
    {
        public StatType statType;
        public StatModifierType modType;
        public float value;
    }

    [SerializeField]
    private string title;
    [SerializeField]
    private string description;
    [SerializeField]
    private Color color = Color.white;
    [SerializeField]
    private Sprite icon;
    [SerializeField]
    private StatModifier[] statModifiers;

    public bool IsModifyStat => StatModifiers.Length > 0;
    public Color Color => color;
    public string Title => title;
    public string Description => description;
    public Sprite Icon => icon;
    public StatModifier[] StatModifiers => statModifiers;
}

[CreateAssetMenu(fileName = "WeaponSO", menuName = Const.String.AssetMenuName + "Weapons/WeaponSO")]
public class WeaponSO : ItemSO
{
    [SerializeField]
    private WeaponType weaponType;
    [SerializeField, TextArea]
    private string description;
    [SerializeField]
    private List<WeaponStat> weaponStats = new List<WeaponStat>();
    [SerializeField]
    private List<WeaponBattleSkillStage> weaponBattleSkillStages = new List<WeaponBattleSkillStage>();

    public WeaponType WeaponType => weaponType;
    public string Description => description;
    public List<WeaponStat> WeaponStats => weaponStats;
    public List<WeaponBattleSkillStage> WeaponBattleSkillStages => weaponBattleSkillStages;

#if UNITY_EDITOR
    [ButtonGroup, Button(SdfIconType.Plus, "Add 1 Card"), PropertyOrder(-1)]
    private void Add1Card()
    {
        this.UpdateNumOfCards(this.GetNumOfCards() + 10);
    }
    [ButtonGroup, Button(SdfIconType.Plus, "Add 10 Card"), PropertyOrder(-1)]
    private void Add10Card()
    {
        this.UpdateNumOfCards(this.GetNumOfCards() + 10);
    }
    [ButtonGroup, Button(SdfIconType.Plus, "Add 100 Card"), PropertyOrder(-1)]
    private void Add100Card()
    {
        this.UpdateNumOfCards(this.GetNumOfCards() + 100);
    }
    [ButtonGroup, Button(SdfIconType.ArrowUp, "Update 1 level"), PropertyOrder(-1)]
    private void Upgrade1Level()
    {
        this.TryUpgradeIgnoreRequirement();
    }
    [ButtonGroup("Group1"), Button(SdfIconType.ArrowDownCircle, "Reset level"), PropertyOrder(-1)]
    private void ResetLevelToDefault()
    {
        this.ResetUpgradeLevelToDefault();
    }
    [ButtonGroup("Group1"), Button(SdfIconType.Unlock, "Unlock Item"), PropertyOrder(-1)]
    private void UnlockItem()
    {
        this.TryUnlockIgnoreRequirement();
    }
    [ButtonGroup("Group1"), Button(SdfIconType.Lock, "Reset unlock"), PropertyOrder(-1)]
    private void ResetUnlockToDefault()
    {
        this.ResetUnlockToDefault();
    }
    [ButtonGroup("Group1"), Button(SdfIconType.Newspaper, "Set New Item"), PropertyOrder(-1)]
    private void SetItemNew()
    {
        this.SetNewItem(true);
    }
    [OnInspectorGUI, Title("DEBUG"), ReadOnly]
    private void OnInspectorGUI()
    {
        GUI.enabled = false;
        EditorGUILayout.TextField("UpgradeLevel", this.GetCurrentUpgradeLevel().ToString());
        foreach (var stat in weaponStats)
        {
            EditorGUILayout.TextField(stat.StatType.ToString(), stat.GetValueAtLevel(this.GetCurrentUpgradeLevel()).ToString());
        }
        GUI.enabled = true;
    }
    [Button]
    private void Test()
    {
        LGDebug.Log($"Rarity: {this.GetRarityType()} - Max Upgrade Level: {this.GetMaxUpgradeLevel()}");
        for (int i = 1; i <= this.GetMaxUpgradeLevel(); i++)
        {
            LGDebug.Log($"==================Lv{i}==================");
            LGDebug.Log($"Damage-Lv{i}: {this.GetStatValue(StatType.AttackDamage, i)}");
            var requirements = this.GetUpgradeRequirementsOfLevel(i);
            for (int j = 0; j < requirements.Count; j++)
            {
                if (requirements[j] is Requirement_Currency requirement)
                {
                    if (requirement.currencyType == CurrencyType.Standard)
                    {
                        LGDebug.Log($"Upgrade Cost-Lv{i}: {requirement.requiredAmountOfCurrency}");
                    }
                    else
                    {
                        LGDebug.Log($"Upgrade Tokens-Lv{i}: {requirement.requiredAmountOfCurrency}");
                    }
                }
            }
        }
    }
#endif

    public void AssignId(string guid)
    {
        m_guid = guid;
    }

    public float GetStatValue(StatType statType)
    {
        int currentLevel = this.GetCurrentUpgradeLevel();
        return GetStatValue(statType, currentLevel);
    }

    public float GetStatValue(StatType statType, int level)
    {
        WeaponStat stat = GetStat(statType);
        if (stat != null)
        {
            return stat.GetValueAtLevel(level);
        }

        return 0f;
    }

    public WeaponStat GetStat(StatType statType)
    {
        foreach (var stat in weaponStats)
        {
            if (stat.StatType == statType)
            {
                return stat;
            }
        }

        return null;
    }

    public Dictionary<StatType, Stat> GetInitialStatDictionary()
    {
        var initialStatDictionary = new Dictionary<StatType, Stat>();
        foreach (var weaponStat in WeaponStats)
        {
            initialStatDictionary.Add(weaponStat.StatType, new Stat(weaponStat.GetValueAtLevel(this.GetCurrentUpgradeLevel())));
        }
        return initialStatDictionary;
    }
}