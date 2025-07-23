using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerInventoryManager : Singleton<PlayerInventoryManager>
{
    public static event Action<WeaponSO> OnWeaponAdded = delegate { };
    public static event Action<WeaponSO> OnWeaponRemoved = delegate { };
    public static event Action<WeaponSO> OnWeaponMerged = delegate { };

    [SerializeField]
    private List<WeaponDataStorageSO> weaponDataStorageSOs;

    public static List<WeaponSO> WeaponSOs => Instance.weaponDataStorageSOs.SelectMany(w => w.WeaponManagerSO.genericItems).ToList();
    public static List<WeaponManagerSO> WeaponManagerSOs => Instance.weaponDataStorageSOs.Select(w => w.WeaponManagerSO).ToList();

    public static WeaponManagerSO GetWeaponManagerSO(WeaponType weaponType)
    {
        return Instance.weaponDataStorageSOs.Find(w => w.WeaponType == weaponType).WeaponManagerSO;
    }

    public static WeaponDataStorageSO GetWeaponDataStorageSO(WeaponType weaponType)
    {
        return Instance.weaponDataStorageSOs.Find(w => w.WeaponType == weaponType);
    }

    public static void AddWeapon(WeaponSO originalWeaponSO, int level, RarityType rarityType)
    {
        var weaponDataStorageSO = GetWeaponDataStorageSO(originalWeaponSO.WeaponType);
        var weapon = weaponDataStorageSO.CloneWeapon(originalWeaponSO, level, rarityType);
        weaponDataStorageSO.AddWeapon(weapon);
        OnWeaponAdded(weapon.WeaponSO);
        LGDebug.Log($"Add weapon: {weapon.WeaponSO.name} from {originalWeaponSO} - {level} - {rarityType}");
    }

    public static void RemoveWeapon(WeaponSO weaponSO)
    {
        if (weaponSO == null)
            return;
        var weaponDataStorageSO = GetWeaponDataStorageSO(weaponSO.WeaponType);
        weaponDataStorageSO.RemoveWeapon(weaponDataStorageSO.Weapons.Find(w => w.WeaponSO == weaponSO));
        OnWeaponRemoved(weaponSO);
        LGDebug.Log($"Remove weapon: {weaponSO.name}");
    }

    public static void MergeWeapon(WeaponDataStorage.Weapon[] weapons)
    {
        if (weapons == null || weapons == null)
            return;
    }

    public static bool IsEquipped(WeaponSO weaponSO)
    {
        return WeaponManagerSOs.Any(w => w.CurrentWeaponInUseVariable.value == weaponSO);
    }
}