using System;
using System.Collections.Generic;
using System.Linq;
using HyrphusQ.Helpers;
using Sirenix.OdinInspector;
using UnityEngine;
using static WeaponDataStorage;

[Serializable]
public struct WeaponSlot
{
    public WeaponManagerSO weaponManagerSO;
    public Transform mountPoint;
}
public class WeaponManager : Singleton<WeaponManager>
{
    [SerializeField]
    private List<WeaponSlot> weaponSlots = new List<WeaponSlot>();

    [ShowInInspector, ReadOnly]
    private List<Weapon> weapons = new List<Weapon>();

    [SerializeField]
    private bool m_IsPreview = false;

    private void Start()
    {
        if (m_IsPreview)
            return;
        InitializeDefaultWeapons();
    }

    [Button]
    public void InitializeDefaultWeapons()
    {
        foreach (var weaponSlot in weaponSlots)
        {
            WeaponSO currentWeaponSO = weaponSlot.weaponManagerSO.currentGenericItemInUse;
            if (currentWeaponSO != null && currentWeaponSO.IsUnlocked())
            {
                var weapon = Instantiate(currentWeaponSO.GetModelPrefab<Weapon>(), weaponSlot.mountPoint);
                if (m_IsPreview)
                    weapon.ActivePreview();
                weapon.transform.localPosition = Vector3.zero;
                weapon.transform.localRotation = Quaternion.identity;
                weapon.Init(currentWeaponSO, PlayerUnit.Instance);
                weapons.Add(weapon);
            }
        }
    }

    public Weapon GetWeapon(WeaponType weaponType)
    {
        foreach (var weapon in weapons)
        {
            if (weapon.WeaponType == weaponType)
                return weapon;
        }

        LGDebug.LogWarning($"Weapon for type {weaponType} not found!");
        return null;
    }

    [Button]
    public void InitPreviewWeapons()
    {
        InitializeDefaultWeapons();

        weapons
            .Where(v => v is DroneCannon).ToList()
            .ForEach((v) => 
            {
                v.enabled = true;
                if(v is DroneCannon DroneCannon)
                    DroneCannon.EnablePreview();
            });
    }
    [Button]
    public void Clear()
    {
        weapons.ForEach((v) => 
        {
            Destroy(v.gameObject);
        });
        weapons.Clear();
    }

}