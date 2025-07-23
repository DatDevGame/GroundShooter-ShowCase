using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LatteGames;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponDetailUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI detailText;
    [SerializeField] private Button equipButton, upgradeButton;
    [SerializeField] private CanvasGroupVisibility canvasGroupVisibility;

    private WeaponSO currentWeaponSO;
    private WeaponManagerSO currentWeaponManagerSO;

    private void Awake()
    {
        equipButton.onClick.AddListener(OnEquipButtonClicked);
        upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
    }

    private void OnUpgradeButtonClicked()
    {
        currentWeaponSO.TryUpgradeIgnoreRequirement();
        SetData(currentWeaponSO, currentWeaponManagerSO);
    }

    private void OnEquipButtonClicked()
    {
        if (currentWeaponManagerSO.currentItemInUse == currentWeaponSO)
        {
            currentWeaponManagerSO.CurrentWeaponInUseVariable.value = null;
        }
        else
        {
            currentWeaponManagerSO.Use(currentWeaponSO);
        }
        SetData(currentWeaponSO, currentWeaponManagerSO);
    }

    private string GetDetailText(WeaponSO weaponSO)
    {
        string text = $"Name: {weaponSO.GetDisplayName()}\n" +
                $"Weapon Type: {weaponSO.WeaponType}\n" +
                "Rarity: " + weaponSO.GetRarityType() + "\n" +
                "Level: " + weaponSO.GetCurrentUpgradeLevel() + "\n" +
                "Upgrade Cost: " + (weaponSO.GetCurrentUpgradeRequirements().FirstOrDefault(requirement => requirement is Requirement_Currency requirement_Currency && requirement_Currency.currencyType == CurrencyType.Standard) as Requirement_Currency).requiredAmountOfCurrency + "\n" +
                "Upgrade Token: " + (weaponSO.GetCurrentUpgradeRequirements().FirstOrDefault(requirement => requirement is Requirement_Currency requirement_Currency && requirement_Currency.currencyType == weaponSO.WeaponType.ConvertToCurrencyType()) as Requirement_Currency).requiredAmountOfCurrency + $"/{CurrencyManager.Instance.GetCurrencySO(weaponSO.WeaponType.ConvertToCurrencyType()).value}" + "\n";
        foreach (var stat in weaponSO.WeaponStats)
        {
            text += $"{stat.StatType}: {GetStatValue(stat)}\n";
        }
        return text;

        string GetStatValue(WeaponStat stat)
        {
            if (stat.StatType == StatType.CritChance)
            {
                return $"{stat.GetValueAtLevel(weaponSO.GetCurrentUpgradeLevel()) * 100}% -> {stat.GetValueAtLevel(weaponSO.GetCurrentUpgradeLevel() + 1) * 100}%";
            }
            return $"{stat.GetValueAtLevel(weaponSO.GetCurrentUpgradeLevel())} -> {stat.GetValueAtLevel(weaponSO.GetCurrentUpgradeLevel() + 1)}";
        }
    }

    public void SetData(WeaponSO weaponSO, WeaponManagerSO weaponManagerSO)
    {
        currentWeaponSO = weaponSO;
        currentWeaponManagerSO = weaponManagerSO;
        detailText.text = GetDetailText(weaponSO);
        canvasGroupVisibility.Show();
        equipButton.GetComponentInChildren<TextMeshProUGUI>().text = currentWeaponManagerSO.currentItemInUse == weaponSO ? "Unequip" : "Equip";
    }

}