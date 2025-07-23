using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WeaponUpgradableModule : UpgradableItemModule
{
    [SerializeField]
    private AnimationCurve tokensRequiredCurve = AnimationCurve.Linear(0, 5, 50, 5);
    private int _upgradeLevel = 1;

    public override int upgradeLevel
    {
        get => _upgradeLevel;
        protected set
        {
            _upgradeLevel = value;
        }
    }

    public override int maxUpgradeLevel
    {
        get
        {
            RarityType rarity = m_ItemSO.GetRarityType();
            return ((int)rarity + 1) * 30;
        }
    }

    // Calculate tokens required of level
    private int GetTokensRequiredOfLevel(int level)
    {
        return Mathf.RoundToInt(tokensRequiredCurve.Evaluate(level));
    }

    // Calculate upgrade cost of level
    private int GetUpgradeCostOfLevel(int level)
    {
        return 100 + (level - 1) * 20;
    }

    public virtual void SetUpgradeLevel(int level)
    {
        upgradeLevel = level;
    }

    public override List<Requirement> GetUpgradeRequirementsOfLevel(int level)
    {
        var tokenRequirement = new Requirement_Currency
        {
            currencyType = itemSO.Cast<WeaponSO>().WeaponType.ConvertToCurrencyType(),
            requiredAmountOfCurrency = GetTokensRequiredOfLevel(level)
        };
        var costRequirement = new Requirement_Currency
        {
            currencyType = CurrencyType.Standard,
            requiredAmountOfCurrency = GetUpgradeCostOfLevel(level)
        };

        return new List<Requirement> { tokenRequirement, costRequirement };
    }
} 