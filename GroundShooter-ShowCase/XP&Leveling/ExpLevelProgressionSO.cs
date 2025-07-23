using HyrphusQ.Events;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ExpLevelProgressionSO", menuName = Const.String.AssetMenuName + "ExpLevelingSystem/ExpLevelProgressionSO")]
public class ExpLevelProgressionSO : ScriptableObject
{
    private const string DebugTag = "Default";
    public event Action<ValueDataChanged<int>> OnLevelChanged = delegate { };
    public event Action<float> OnExpGained = delegate { };

    [SerializeField]
    protected float[] expRequiredPerLevel = new float[]
    {
        0,      // Level 1
        100,    // Level 2
        200,    // Level 3
        400,    // Level 4
        800,    // Level 5
        1600,   // Level 6
        3200,   // Level 7
        6400,   // Level 8
        12800,  // Level 9
        25600,  // Level 10
    };
    [SerializeField, InfoBox("Optional")]
    protected IntVariable levelVariableSO;
    [SerializeField, InfoBox("Optional")]
    protected RangeProgressSO<float> expProgressSO;
    [NonSerialized]
    protected RangeProgress<float> currentExpProgress;

    [Title("DEBUG")]
    [ShowInInspector]
    public virtual bool IsMaxLevel => CurrentLevel >= MaxLevel;
    [ShowInInspector]
    public virtual int InitialLevel => 1;
    [ShowInInspector]
    public virtual int CurrentLevel => CalculateLevelByExp(CurrentExp);
    [ShowInInspector]
    public virtual int MaxLevel => expRequiredPerLevel.Length;
    [ShowInInspector]
    public virtual float CurrentExp => currentExpProgress == null ? 0f : currentExpProgress.value;
    [ShowInInspector]
    public virtual float ExpToNextLevel => currentExpProgress == null ? 0f : currentExpProgress.maxValue;
    [ShowInInspector]
    public virtual float RemainingExpToNextLevel => currentExpProgress == null ? 0f : currentExpProgress.maxValue - currentExpProgress.value;
#if UNITY_EDITOR
    [ShowInInspector]
    private AnimationCurve LevelToExpCurve
    {
        get
        {
            var levelToExpCurve = new AnimationCurve();

            // Clear existing keys
            while (levelToExpCurve.keys.Length > 0)
            {
                levelToExpCurve.RemoveKey(0);
            }

            // Add key frame for each level
            for (int i = 0; i < expRequiredPerLevel.Length; i++)
            {
                // Set both inTangent and outTangent to 1 for linear interpolation
                Keyframe keyFrame = new Keyframe(i, expRequiredPerLevel[i], 1f, 1f)
                {
                    weightedMode = WeightedMode.None
                };
                levelToExpCurve.AddKey(keyFrame);
            }

            // Set tangent mode to Linear for all keys
            for (int i = 0; i < levelToExpCurve.length; i++)
            {
                UnityEditor.AnimationUtility.SetKeyLeftTangentMode(levelToExpCurve, i, UnityEditor.AnimationUtility.TangentMode.Linear);
                UnityEditor.AnimationUtility.SetKeyRightTangentMode(levelToExpCurve, i, UnityEditor.AnimationUtility.TangentMode.Linear);
            }

            return levelToExpCurve;
        }
        set
        {
            // Do nothing
        }
    }
#endif

    protected virtual void OnEnable()
    {
        Initialize();
    }

    protected virtual void OnValidate()
    {
        if (expRequiredPerLevel == null || expRequiredPerLevel.Length == 0)
        {
            LGDebug.LogWarning("Exp required per level array is empty!", DebugTag);
            return;
        }
    }

    protected virtual void NotifyEventLevelChanged(ValueDataChanged<int> levelChangeData)
    {
        OnLevelChanged.Invoke(levelChangeData);
        if (levelVariableSO != null)
            levelVariableSO.value = levelChangeData.newValue;
    }

    protected virtual void NotifyEventExpGained(float expAmount)
    {
        OnExpGained.Invoke(expAmount);
        LGDebug.Log($"Gained {expAmount} exp. Current exp progress: {GetExpProgress()}", DebugTag);
    }

    protected virtual void LevelUp(ValueDataChanged<int> levelChangeData)
    {
        RecalculateExpToNextLevel(levelChangeData.newValue);
        NotifyEventLevelChanged(levelChangeData);
        LGDebug.Log($"Level up {levelChangeData.oldValue}->{levelChangeData.newValue}", DebugTag);
    }

    protected virtual void RecalculateExpToNextLevel(int currentLevel)
    {
        if (currentLevel >= MaxLevel)
        {
            currentExpProgress.minValue = GetExpToLevel(MaxLevel);
            currentExpProgress.maxValue = float.MaxValue;
            return;
        }

        currentExpProgress.minValue = GetExpToLevel(currentLevel);
        currentExpProgress.maxValue = GetExpToLevel(currentLevel + 1);
        currentExpProgress.RecalculateInverseLerpValue();
    }

    public virtual void Initialize()
    {
        RangeFloatValue currentExpRange = new RangeFloatValue(0, GetExpToLevel(InitialLevel + 1));
        currentExpProgress = new RangeProgress<float>(currentExpRange, currentExpRange.minValue);
        if (expProgressSO != null)
            expProgressSO.rangeProgress = currentExpProgress;
    }

    [Button]
    public virtual void AddExp(float expAmount)
    {
        if (IsMaxLevel)
        {
            LGDebug.Log("Already at max level", DebugTag);
            return;
        }

        int previousLevel = this.CurrentLevel;
        int currentLevel = CalculateLevelByExp(currentExpProgress.value + expAmount);
        currentExpProgress.value += expAmount;
        NotifyEventExpGained(expAmount);

        if (CurrentExp >= ExpToNextLevel)
            LevelUp(new ValueDataChanged<int>(previousLevel, currentLevel));
    }

    [Button]
    public virtual void SetLevel(int level)
    {
        if (level < InitialLevel || level > MaxLevel)
        {
            LGDebug.LogError($"Invalid level: {level}. Must be between 1 and {MaxLevel}", DebugTag);
            return;
        }

        int previousLevel = this.CurrentLevel;
        int currentLevel = level;
        currentExpProgress.value = GetExpToLevel(level);
        RecalculateExpToNextLevel(currentLevel);
        NotifyEventLevelChanged(new ValueDataChanged<int>(previousLevel, currentLevel));
        LGDebug.Log($"Set level {previousLevel}->{currentLevel}", DebugTag);
    }

    [Button]
    public virtual void ResetLevel()
    {
        int previousLevel = this.CurrentLevel;
        int currentLevel = InitialLevel;
        currentExpProgress.value = 0f;
        RecalculateExpToNextLevel(currentLevel);
        NotifyEventLevelChanged(new ValueDataChanged<int>(previousLevel, currentLevel));
        LGDebug.Log($"Reset level to {previousLevel}->{currentLevel}", DebugTag);
    }

    public virtual float GetExpProgress()
    {
        return currentExpProgress.inverseLerpValue;
    }

    public virtual float GetExpToLevel(int targetLevel)
    {
        if (targetLevel < InitialLevel || targetLevel > MaxLevel)
        {
            LGDebug.LogError($"Invalid target level: {targetLevel}", DebugTag);
            return 0f;
        }
        return expRequiredPerLevel[targetLevel - 1];
    }

    public virtual int CalculateLevelByExp(float currentExp)
    {
        // If exp is less than initial level requirement (usually 0), return initial level
        if (currentExp < GetExpToLevel(InitialLevel))
            return InitialLevel;

        // If exp is greater than or equal to max level requirement, return max level
        if (currentExp >= GetExpToLevel(MaxLevel))
            return MaxLevel;

        // Find the highest level where current exp meets the requirement
        int level = InitialLevel;
        while (level < MaxLevel && currentExp >= GetExpToLevel(level + 1))
        {
            level++;
        }
        return level;
    }
}