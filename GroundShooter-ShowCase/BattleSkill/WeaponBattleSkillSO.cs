using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BattleSkillSO_Weapon", menuName = Const.String.AssetMenuName + "BattleSkillSystem/BattleSkillSO/Weapon")]
public class WeaponBattleSkillSO : BattleSkillSO
{
    [SerializeField]
    private WeaponManagerSO weaponManagerSO;

    public override int MaxStack => weaponManagerSO.currentItemInUse != null && weaponManagerSO.currentItemInUse.IsUnlocked() ? weaponManagerSO.currentItemInUse.Cast<WeaponSO>().WeaponBattleSkillStages.Count : 0;
    public override string Title
    {
        get
        {
            Weapon weapon = WeaponManager.Instance.GetWeapon(weaponManagerSO.WeaponType);
            return weapon.WeaponSO.WeaponBattleSkillStages[Mathf.Min(weapon.StarLevel, weapon.WeaponSO.WeaponBattleSkillStages.Count - 1)].Title;
        }
    }
    public override string Description
    {
        get
        {
            Weapon weapon = WeaponManager.Instance.GetWeapon(weaponManagerSO.WeaponType);
            return weapon.WeaponSO.WeaponBattleSkillStages[Mathf.Min(weapon.StarLevel, weapon.WeaponSO.WeaponBattleSkillStages.Count - 1)].Description;
        }
    }
    public override Sprite Icon
    {
        get
        {
            Weapon weapon = WeaponManager.Instance.GetWeapon(weaponManagerSO.WeaponType);
            return weapon.WeaponSO.WeaponBattleSkillStages[Mathf.Min(weapon.StarLevel, weapon.WeaponSO.WeaponBattleSkillStages.Count - 1)].Icon;
        }
    }
    public override Color Color
    {
        get
        {
            Weapon weapon = WeaponManager.Instance.GetWeapon(weaponManagerSO.WeaponType);
            return weapon.WeaponSO.WeaponBattleSkillStages[Mathf.Min(weapon.StarLevel, weapon.WeaponSO.WeaponBattleSkillStages.Count - 1)].Color;
        }
    }

    public override void Apply(Unit unit)
    {
        Weapon weapon = WeaponManager.Instance.GetWeapon(weaponManagerSO.WeaponType);
        if (weapon == null)
            return;
        weapon.IncreaseStarLevel(1);
    }

    public override void Remove(Unit unit)
    {
        // Do nothing
    }

    public virtual Sprite GetLastIcon()
    {
        Weapon weapon = WeaponManager.Instance.GetWeapon(weaponManagerSO.WeaponType);
        return weapon.WeaponSO.WeaponBattleSkillStages[weapon.StarLevel - 1].Icon;
    }
}