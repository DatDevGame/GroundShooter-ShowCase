using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HyrphusQ.Helpers;
using Random = UnityEngine.Random;
using HyrphusQ.Events;

public class WeaponGridUI : MonoBehaviour
{
    [SerializeField] private GridLayoutGroup itemsGrid;
    [SerializeField] private Button addWeaponButton;
    [SerializeField] private GameObject weaponItemPrefab;

    private void Awake()
    {
        if (addWeaponButton != null)
        {
            addWeaponButton.onClick.AddListener(OnAddWeaponClicked);
        }
        if (PlayerInventoryManager.Instance != null)
        {
            PlayerInventoryManager.OnWeaponAdded += AddWeapon;
            PlayerInventoryManager.WeaponManagerSOs.ForEach(w => w.CurrentWeaponInUseVariable.onValueChanged += OnWeaponChanged);
        }
        GenerateGridView();
    }

    private void OnDestroy()
    {
        if (addWeaponButton != null)
        {
            addWeaponButton.onClick.RemoveListener(OnAddWeaponClicked);
        }
        if (PlayerInventoryManager.Instance != null)
        {
            PlayerInventoryManager.OnWeaponAdded -= AddWeapon;
            PlayerInventoryManager.WeaponManagerSOs.ForEach(w => w.CurrentWeaponInUseVariable.onValueChanged -= OnWeaponChanged);
        }
    }

    private void OnWeaponChanged(ValueDataChanged<ItemSO> eventData)
    {
        ClearGrid();
        GenerateGridView();
    }

    private void OnAddWeaponClicked()
    {
        PlayerInventoryManager.AddWeapon(PlayerInventoryManager.GetWeaponManagerSO((WeaponType)Random.Range(0, 5)).initialValue.GetRandom().Cast<WeaponSO>(), 1, (RarityType)Random.Range(0, 6));
    }

    private void GenerateGridView()
    {
        foreach (var weaponSO in PlayerInventoryManager.WeaponSOs)
        {
            if (PlayerInventoryManager.IsEquipped(weaponSO))
                continue;
            AddWeapon(weaponSO);
        }
    }

    // Method to add a weapon to the grid
    public void AddWeapon(WeaponSO weaponSO)
    {
        if (weaponItemPrefab != null && itemsGrid != null)
        {
            GameObject newItem = Instantiate(weaponItemPrefab, itemsGrid.transform);
            WeaponItem weaponItem = newItem.GetComponent<WeaponItem>();

            if (weaponItem != null)
            {
                weaponItem.SetData(weaponSO);
                LGDebug.Log($"Added new weapon to grid: {weaponSO}");
            }
        }
        else
        {
            LGDebug.Log("Failed to add weapon to grid. Weapon item prefab or grid is null.");
        }
    }

    // Method to clear all items from the grid
    public void ClearGrid()
    {
        if (itemsGrid != null)
        {
            foreach (Transform child in itemsGrid.transform)
            {
                Destroy(child.gameObject);
            }
            LGDebug.Log("Grid cleared!");
        }
    }

}