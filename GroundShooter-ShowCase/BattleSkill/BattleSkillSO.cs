using System.Collections.Generic;
using UnityEngine;

public abstract class BattleSkillSO : ScriptableObject
{
    public abstract int MaxStack { get; }
    public abstract string Title { get; }
    public abstract string Description { get; }
    public abstract Color Color { get; }
    public abstract Sprite Icon { get; }

    public abstract void Apply(Unit unit);
    public abstract void Remove(Unit unit);
}