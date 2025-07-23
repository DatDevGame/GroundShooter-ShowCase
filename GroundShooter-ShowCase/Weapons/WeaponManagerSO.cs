using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HyrphusQ.Events;
using Sirenix.OdinInspector;
using UnityEngine;

[EventCode]
public enum WeaponManagementEventCode
{
    OnWeaponUsed,
    OnWeaponSelected,
    OnWeaponUnlocked
}
[CreateAssetMenu(fileName = "WeaponManagerSO", menuName = "GroundShooter/Weapons/WeaponManagerSO")]
public class WeaponManagerSO : ItemManagerSO<WeaponSO>
{
    #region Fields
    // Event Code Fields
    [SerializeField, BoxGroup("Event Code")]
    protected EventCode itemCardChangedEventCode;
    [SerializeField, BoxGroup("Event Code")]
    protected EventCode itemUpgradedEventCode;
    [SerializeField, BoxGroup("Event Code")]
    protected EventCode itemNewStateChangedEventCode;
    [SerializeField]
    private WeaponType weaponType;
    [SerializeField]
    private WeaponDataStorageSO weaponDataStorageSO;
    #endregion

    #region Properties
    public override List<ItemSO> value
    {
        get
        {
            if (!Application.isPlaying)
                return m_InitialValue;
            if (m_RuntimeValue == null)
                m_RuntimeValue = weaponDataStorageSO.Weapons.Select(weapon => weapon.WeaponSO as ItemSO).ToList();
            return m_RuntimeValue;
        }
        set => base.value = value;
    }
    public WeaponType WeaponType => weaponType;
    public WeaponDataStorageSO WeaponDataStorageSO => weaponDataStorageSO;
    public PPrefItemSOVariable CurrentWeaponInUseVariable => m_CurrentItemInUse;
    #endregion

    #region Private & Protected Methods
    protected override void OnEnable()
    {
        base.OnEnable();
        foreach (var item in items)
        {
            if (item.TryGetModule(out CardItemModule cardModule))
            {
                cardModule.onNumOfCardsChanged += OnItemCardChanged;
            }
            if (item.TryGetModule(out UpgradableItemModule upgradableModule))
            {
                upgradableModule.onUpgradeLevelChanged += OnItemUpgradeLevelChanged;
            }
            if (item.TryGetModule(out NewItemModule newItemModule))
            {
                newItemModule.onNewStateChanged += OnItemNewStateChanged;
            }
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        foreach (var item in items)
        {
            if (item.TryGetModule(out CardItemModule cardModule))
            {
                cardModule.onNumOfCardsChanged -= OnItemCardChanged;
            }
            if (item.TryGetModule(out UpgradableItemModule upgradableModule))
            {
                upgradableModule.onUpgradeLevelChanged -= OnItemUpgradeLevelChanged;
            }
            if (item.TryGetModule(out NewItemModule newItemModule))
            {
                newItemModule.onNewStateChanged -= OnItemNewStateChanged;
            }
        }
    }

    // Notify Events
    protected override void NotifyEventItemUsed(params object[] eventData)
    {
        if (itemUsedEventCode == null || itemUsedEventCode.eventCode == null)
            return;
        base.NotifyEventItemUsed(eventData);
    }
    
    protected override void NotifyEventItemSelected(params object[] eventData)
    {
        if (itemSelectedEventCode == null || itemSelectedEventCode.eventCode == null)
            return;
        base.NotifyEventItemSelected(eventData);
    }

    protected virtual void NotifyEventItemCardChanged(params object[] eventData)
    {
        if (itemCardChangedEventCode == null || itemCardChangedEventCode.eventCode == null)
            return;
        GameEventHandler.Invoke(itemCardChangedEventCode, eventData);
    }

    protected virtual void NotifyEventItemUpgraded(params object[] eventData)
    {
        if (itemUpgradedEventCode == null || itemUpgradedEventCode.eventCode == null)
            return;
        GameEventHandler.Invoke(itemUpgradedEventCode, eventData);
    }

    protected virtual void NotifyEventItemNewStateChanged(params object[] eventData)
    {
        if (itemNewStateChangedEventCode == null || itemNewStateChangedEventCode.eventCode == null)
            return;
        GameEventHandler.Invoke(itemNewStateChangedEventCode, eventData);
    }

    // Listen Events
    protected virtual void OnItemCardChanged(CardItemModule cardModule)
    {
        // Notify event
        NotifyEventItemCardChanged(this, cardModule.itemSO, cardModule.numOfCards);
    }

    protected virtual void OnItemUpgradeLevelChanged(UpgradableItemModule upgradableModule)
    {
        // Notify event
        NotifyEventItemUpgraded(this, upgradableModule.itemSO, upgradableModule.upgradeLevel);
    }

    protected virtual void OnItemNewStateChanged(NewItemModule newItemModule)
    {
        // Notify event
        NotifyEventItemNewStateChanged(this, newItemModule.itemSO, newItemModule.isNew);
    }
    #endregion

    public override void Add(ItemSO item)
    {
        if (item.TryGetModule(out UnlockableItemModule unlockableModule))
        {
            unlockableModule.onItemUnlocked += OnItemUnlocked;
        }
        if (item.TryGetModule(out CardItemModule cardModule))
        {
            cardModule.onNumOfCardsChanged += OnItemCardChanged;
        }
        if (item.TryGetModule(out UpgradableItemModule upgradableModule))
        {
            upgradableModule.onUpgradeLevelChanged += OnItemUpgradeLevelChanged;
        }
        if (item.TryGetModule(out NewItemModule newItemModule))
        {
            newItemModule.onNewStateChanged += OnItemNewStateChanged;
        }
        base.Add(item);
    }

    public override bool Remove(ItemSO item)
    {
        if (item.TryGetModule(out UnlockableItemModule unlockableModule))
        {
            unlockableModule.onItemUnlocked -= OnItemUnlocked;
        }
        if (item.TryGetModule(out CardItemModule cardModule))
        {
            cardModule.onNumOfCardsChanged -= OnItemCardChanged;
        }
        if (item.TryGetModule(out UpgradableItemModule upgradableModule))
        {
            upgradableModule.onUpgradeLevelChanged -= OnItemUpgradeLevelChanged;
        }
        if (item.TryGetModule(out NewItemModule newItemModule))
        {
            newItemModule.onNewStateChanged -= OnItemNewStateChanged;
        }
        return base.Remove(item);
    }
}