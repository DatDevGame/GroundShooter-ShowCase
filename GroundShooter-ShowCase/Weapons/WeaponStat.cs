using UnityEngine;
using Sirenix.OdinInspector;
using System;

[Serializable]
public class WeaponStat
{
    public enum StatScalingMethod
    {
        Curve,    // Use an animation curve to map level to stat value
        Formula   // Use a mathematical formula to calculate stat at level X
    }

    [SerializeField]
    private StatType statType;
    [SerializeField, Tooltip("Base value of the stat")]
    private float baseValue;
    [SerializeField]
    private StatScalingMethod statScalingMethod;
    [SerializeField, Tooltip("Value added per level"), ShowIf("@statScalingMethod == StatScalingMethod.Formula")]
    private float valuePerLevel;
    [SerializeField, ShowIf("@statScalingMethod == StatScalingMethod.Curve")]
    private AnimationCurve valuePerLevelCurve = AnimationCurve.Linear(1, 1, 50, 50);

    public StatType StatType => statType;

    // Get the value of this stat at a specific level
    public float GetValueAtLevel(int level)
    {
        if (statScalingMethod == StatScalingMethod.Curve)
        {
            if (level <= 1)
            {
                return baseValue;
            }
            return GetValueAtLevel(level - 1) + valuePerLevelCurve.Evaluate(level);
        }
        else
        {
            return baseValue + (valuePerLevel * (level - 1));
        }
    }
}