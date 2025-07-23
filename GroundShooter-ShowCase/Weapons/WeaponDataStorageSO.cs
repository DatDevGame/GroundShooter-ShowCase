using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class WeaponDataStorage : SavedData
{
    [Serializable]
    public class Weapon
    {
        public Weapon(WeaponSO originalWeaponSO, int level, RarityType rarityType)
        {
            id = Guid.NewGuid().ToString();
            originalId = originalWeaponSO.guid;
            this.level = level;
            this.rarityType = rarityType;
            this.originalWeaponSO = originalWeaponSO;
        }

        [SerializeField]
        private string id;
        [SerializeField]
        private string originalId;
        [SerializeField]
        private int level;
        [SerializeField]
        private RarityType rarityType;

        [ShowInInspector]
        private WeaponSO weaponSO;
        [ShowInInspector]
        private WeaponSO originalWeaponSO;

        public int Level => level;
        public string Id => id;
        public string OriginalId => originalId;
        public RarityType RarityType => rarityType;
        public WeaponSO OriginalWeaponSO
        {
            get => originalWeaponSO;
            set => originalWeaponSO = value;
        }
        public WeaponSO WeaponSO
        {
            get
            {
                if (weaponSO == null)
                {
                    weaponSO = Object.Instantiate(originalWeaponSO);
                    weaponSO.AssignId(id);
                    weaponSO.name = $"{originalWeaponSO.name} (Clone)";
                    if (weaponSO.TryGetModule(out RarityItemModule rarityModule))
                    {
                        rarityModule.rarityType = rarityType;
                        rarityModule.onRarityChanged += OnRarityChanged;
                    }
                    if (weaponSO.TryGetModule(out WeaponUpgradableModule upgradeModule))
                    {
                        upgradeModule.SetUpgradeLevel(level);
                        upgradeModule.onUpgradeLevelChanged += OnUpgradeLevelChanged;
                    }
                }
                return weaponSO;
            }
        }

        private void OnRarityChanged(RarityItemModule rarityModule)
        {
            rarityType = rarityModule.rarityType;
        }

        private void OnUpgradeLevelChanged(UpgradableItemModule upgradableModule)
        {
            level = upgradableModule.upgradeLevel;
        }

        public void Dispose()
        {
            // if (weaponSO != null)
            // {
            //     Object.Destroy(weaponSO);
            // }
        }
    }

    [SerializeField]
    private List<Weapon> weapons = new List<Weapon>();

    public List<Weapon> Weapons => weapons;
}
[CreateAssetMenu(fileName = "WeaponDataStorageSO", menuName = "GroundShooter/Weapons/WeaponDataStorageSO")]
public class WeaponDataStorageSO : SavedDataSO<WeaponDataStorage>
{
    [SerializeField]
    private WeaponManagerSO weaponManagerSO;

    public WeaponType WeaponType => weaponManagerSO.WeaponType;
    public WeaponManagerSO WeaponManagerSO => weaponManagerSO;
    [ShowInInspector, Title("DEBUG")]
    public List<WeaponDataStorage.Weapon> Weapons => data.Weapons;
    [ShowInInspector]
    public WeaponDataStorage InspectorData => data;

    public WeaponSO FindOriginalWeaponSO(string originalId)
    {
        foreach (var weaponSO in weaponManagerSO.initialValue)
        {
            if (weaponSO.guid == originalId)
                return weaponSO as WeaponSO;
        }
        throw new Exception($"WeaponSO with id {originalId} not found");
    }

    public override void Load()
    {
        if (m_Data != null)
            return;
        ES3Settings settings = new ES3Settings(m_SaveFileName, m_EncryptionType, k_EncryptionPassword);
        m_Data = ES3.Load(m_Key, defaultData, settings);
        for (int i = m_Data.Weapons.Count - 1; i >= 0; i--)
        {
            var originalWeaponSO = FindOriginalWeaponSO(m_Data.Weapons[i].OriginalId);
            if (originalWeaponSO == null)
            {
                // Handle the case where the original weapon is deleted by design
                m_Data.Weapons.RemoveAt(i);
                continue;
            }
            m_Data.Weapons[i].OriginalWeaponSO = originalWeaponSO;
        }
        NotifyEventDataLoaded();
    }

    public WeaponDataStorage.Weapon CloneWeapon(WeaponSO originalWeaponSO, int level, RarityType rarityType)
    {
        var weapon = new WeaponDataStorage.Weapon(originalWeaponSO, level, rarityType);
        return weapon;
    }

    public void AddWeapon(WeaponDataStorage.Weapon weapon)
    {
        data.Weapons.Add(weapon);
        weaponManagerSO.Add(weapon.WeaponSO);
    }

    public void RemoveWeapon(WeaponDataStorage.Weapon weapon)
    {
        data.Weapons.Remove(weapon);
        weaponManagerSO.Remove(weapon.WeaponSO);
        weapon.Dispose();
    }
}