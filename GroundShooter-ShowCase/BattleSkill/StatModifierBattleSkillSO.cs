using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BattleSkillSO_StatModifier", menuName = Const.String.AssetMenuName + "BattleSkillSystem/BattleSkillSO/StatModifier")]
public class StatModifierBattleSkillSO : BattleSkillSO
{
    [SerializeField]
    private int maxStack = 4;
    [SerializeField]
    private string title;
    [SerializeField]
    private string description;
    [SerializeField]
    private Sprite icon;
    [SerializeField]
    private StatType statType;
    [SerializeField]
    private StatModifierType modifierType;
    [SerializeField]
    private float value;

    public override int MaxStack => maxStack;
    public override string Title => title;
    public override string Description => description;
    public override Sprite Icon => icon;
    public override Color Color => new Color(0.190603f, 0.8679245f, 0f, 1f);

    public override void Apply(Unit unit)
    {
        unit.Stats.AddModifier(statType, new StatModifier(value, modifierType, this));
    }

    public override void Remove(Unit unit)
    {
        unit.Stats.RemoveLastModifierFromSource(this);
    }
}