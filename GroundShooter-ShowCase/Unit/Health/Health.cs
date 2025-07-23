using System;
using HyrphusQ.GUI;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class Health
{
    [SerializeField]
    private ProgressBar healthBar;
    private RangeProgress<float> healthProgress;
    [ShowInInspector, HideInEditorMode]
    public float CurrentHealth
    {
        get
        {
#if UNITY_EDITOR
            if (healthProgress == null)
                return -1;
#endif
            return healthProgress.value;
        }
        set
        {
            healthProgress.value = Mathf.Clamp(value, healthProgress.minValue, healthProgress.maxValue);
        }
    }
    [ShowInInspector, HideInEditorMode]
    public float MaxHealth
    {
        get
        {
#if UNITY_EDITOR
            if (healthProgress == null)
                return -1;
#endif
            return healthProgress.maxValue;
        }
        set
        {
            healthProgress.maxValue = value;
        }
    }
    public ProgressBar HealthBar => healthBar;

    public void Init(RangeValue<float> maxHealthRange)
    {
        healthProgress = new RangeProgress<float>(maxHealthRange, maxHealthRange.maxValue);
        if (healthBar != null)
            healthBar.Init(healthProgress);
    }
}