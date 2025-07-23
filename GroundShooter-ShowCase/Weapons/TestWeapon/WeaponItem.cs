using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HyrphusQ.SerializedDataStructure;
using HyrphusQ.Events;
using System;

public class WeaponItem : MonoBehaviour
{
    [SerializeField] private PPrefItemSOVariable currentWeaponInUse;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image weaponIconImage;
    [SerializeField] private TextMeshProUGUI weaponText, levelText;
    [SerializeField] private Button button;
    [SerializeField] private SerializedDictionary<RarityType, Color> rarityColorDictionary;

    WeaponSO weaponSO;

    private void Awake()
    {
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
        if (currentWeaponInUse != null)
        {
            currentWeaponInUse.onValueChanged += OnCurrentWeaponInUseChanged;
            if (currentWeaponInUse.value != null)
            {
                SetData(currentWeaponInUse.value as WeaponSO);
            }
            else
            {
                backgroundImage.color = Color.black;
                weaponText.gameObject.SetActive(false);
                weaponIconImage.gameObject.SetActive(false);
            }
        }

    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnClick);
        }
        if (currentWeaponInUse != null)
            currentWeaponInUse.onValueChanged -= OnCurrentWeaponInUseChanged;
    }

    private void OnCurrentWeaponInUseChanged(ValueDataChanged<ItemSO> eventData)
    {
        if (eventData.newValue != null)
        {
            SetData(eventData.newValue as WeaponSO);
        }
        else
        {
            backgroundImage.color = Color.black;
            weaponText.gameObject.SetActive(false);
            weaponIconImage.gameObject.SetActive(false);
        }
    }

    private void OnClick()
    {
        if (weaponSO == null)
            return;
        FindObjectOfType<WeaponDetailUI>().SetData(weaponSO, PlayerInventoryManager.GetWeaponManagerSO(weaponSO.WeaponType));
    }

    public void SetData(WeaponSO weaponSO)
    {
        if (this.weaponSO != null)
        {
            if (this.weaponSO.TryGetModule(out WeaponUpgradableModule upgradeModule))
            {
                upgradeModule.onUpgradeLevelChanged -= OnUpgradeLevelChanged;
            }
        }
        this.weaponSO = weaponSO;
        weaponText.gameObject.SetActive(true);
        if (weaponText != null)
        {
            weaponText.text = weaponSO.GetDisplayName();
        }

        weaponIconImage.gameObject.SetActive(true);
        if (weaponIconImage != null && weaponSO.GetThumbnailImage() != null)
        {
            weaponIconImage.sprite = weaponSO.GetThumbnailImage();
        }

        backgroundImage.color = rarityColorDictionary.Get(weaponSO.GetRarityType());
        levelText.SetText(weaponSO.GetCurrentUpgradeLevel().ToString());
        if (weaponSO.TryGetModule(out WeaponUpgradableModule upgradeModule2))
        {
            upgradeModule2.onUpgradeLevelChanged += OnUpgradeLevelChanged;
        }
    }

    private void OnUpgradeLevelChanged(UpgradableItemModule module)
    {
        SetData(weaponSO);
    }
}