using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using HyrphusQ.Events;

[Serializable]
public class Stat
{
    public Stat(float baseValue)
    {
        this.baseValue = baseValue;
    }

    public event Action<ValueDataChanged<float>> OnValueChanged = delegate { };

    [SerializeField]
    private float baseValue;
    [NonSerialized, ShowInInspector, ReadOnly]
    private List<StatModifier> modifiers = new();

    public float BaseValue
    {
        get => baseValue;
        set
        {
            float oldValue = Value;
            BaseValue = value;
            float newValue = Value;
            NotifyValueChanged(new ValueDataChanged<float>(oldValue, newValue));
        }
    }
    [ShowInInspector]
    public float Value
    {
        get
        {
            float flatSum = 0f;
            float additivePercent = 0f;
            float multiplicativePercent = 1f;

            if (modifiers != null)
            {
                foreach (var modifier in modifiers)
                {
                    if (modifier.modifierType == StatModifierType.Flat)
                        flatSum += modifier.value;
                    else if (modifier.modifierType == StatModifierType.AdditivePercent)
                        additivePercent += modifier.value;
                    else if (modifier.modifierType == StatModifierType.MultiplicativePercent)
                        multiplicativePercent *= 1f + modifier.value;
                }
            }

            float finalValue = (BaseValue + flatSum) * (1 + additivePercent) * multiplicativePercent;
            return finalValue;
        }
    }

    private void NotifyValueChanged(ValueDataChanged<float> eventData)
    {
        OnValueChanged.Invoke(eventData);
    }

    public void AddModifier(StatModifier modifier)
    {
        float oldValue = Value;
        modifiers.Add(modifier);
        float newValue = Value;
        NotifyValueChanged(new ValueDataChanged<float>(oldValue, newValue));
    }

    public void RemoveModifier(StatModifier modifier)
    {
        float oldValue = Value;
        modifiers.Remove(modifier);
        float newValue = Value;
        NotifyValueChanged(new ValueDataChanged<float>(oldValue, newValue));
    }

    public void RemoveLastModifierFromSource(object source)
    {
        for (int i = modifiers.Count - 1; i >= 0; i--)
        {
            if (modifiers[i].source == source)
            {
                RemoveModifier(modifiers[i]);
                break;
            }
        }
    }
    public void RemoveAllModifiersFromSource(object source)
    {
        float oldValue = Value;
        modifiers.RemoveAll(m => m.source == source);
        float newValue = Value;
        NotifyValueChanged(new ValueDataChanged<float>(oldValue, newValue));
    }

    public Stat Clone()
    {
        Stat instance = (Stat)MemberwiseClone();
        if (instance.OnValueChanged == null)
            instance.OnValueChanged = delegate { };
        if (instance.modifiers == null)
            instance.modifiers = new List<StatModifier>();
        return instance;
    }
}