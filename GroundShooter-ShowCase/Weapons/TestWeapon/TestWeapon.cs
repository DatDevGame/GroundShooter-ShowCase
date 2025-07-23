using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class TestWeapon : MonoBehaviour
{
    public WeaponSO originalWeaponSO;
    public RarityType rarityType;
    public int level;
    public List<WeaponSO> weapons;

    private void Start()
    {
        FetchCurrentInventory();
    }

    [Button]
    private void FetchCurrentInventory()
    {
        weapons = PlayerInventoryManager.WeaponSOs;
    }

    [Button]
    private void TestAddWeapon()
    {
        PlayerInventoryManager.AddWeapon(originalWeaponSO, level, rarityType);
    }

    [Button]
    private void TestRemoveWeapon()
    {
        PlayerInventoryManager.RemoveWeapon(weapons[0]);
    }
}