using System;

public enum StatModifierType
{
    Flat,      // +10
    AdditivePercent, // +20% (stackable)
    MultiplicativePercent // +20% multiplicative (non-stackable or layered)
}

[Serializable]
public class StatModifier
{
    public StatModifier(float value, StatModifierType modifierType, object source)
    {
        this.modifierType = modifierType;
        this.value = value;
        this.source = source;
    }
    
    public StatModifierType modifierType;
    public float value;
    public object source;
}