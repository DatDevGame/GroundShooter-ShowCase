using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif

public enum TurretType
{
    TwinBarrel,
    Missile
}

[Serializable]
public class TurretData
{
    [BoxGroup("Config Set Up")]
    [OnCollectionChanged("OnSetUpDataChanged")]
    public List<TurretSetup> TurretSetups;

    [BoxGroup("Config Data")]
    [OnCollectionChanged("OnWaveDataChanged")]
    public List<TurretWaveDataConfig> TurretWaveDataConfigs;

#if UNITY_EDITOR
    private void OnSetUpDataChanged(CollectionChangeInfo info)
    {
        if (info.ChangeType == CollectionChangeType.Add && info.Value is TurretWaveDataConfig newConfig)
        {
            newConfig.Data = $"Data - {TurretSetups.Count}";
        }

        for (int i = 0; i < TurretSetups.Count; i++)
        {
            TurretSetups[i].Data = $"Data - {i + 1}";
        }
    }
    private void OnWaveDataChanged(CollectionChangeInfo info)
    {
        if (info.ChangeType == CollectionChangeType.Add && info.Value is TurretWaveDataConfig newConfig)
        {
            newConfig.Data = $"Data - {TurretWaveDataConfigs.Count}";
        }

        for (int i = 0; i < TurretWaveDataConfigs.Count; i++)
        {
            TurretWaveDataConfigs[i].Data = $"Data - {i + 1}";
        }
    }
#endif
}

[Serializable]
public class TurretSetup
{
    [Title("++++++++++++++++++O++++++++++++++++++", TitleAlignment = TitleAlignments.Centered), ReadOnly]
    public string Data;

    [BoxGroup("Distance Spawn")]
    public float DistanceSpawn;

    [BoxGroup("Spawn Chance Split")]
    [VerticalGroup("SpawnChanceSplit")]
    [VerticalGroup("SpawnChanceSplit/Left")]
    [LabelText("Spawn Chance Input")]
    [MinValue(0), MaxValue(100)]
    [SerializeField]
    public float m_SpawnChancePercent;

    [BoxGroup("Spawn Chance Split")]
    [LabelText("Spawn Chance (%)")]
    [ShowInInspector, ReadOnly]
    [ProgressBar(0, 100, ColorGetter = nameof(GetSpawnChanceColor))]
    [HideLabel]
    private float SpawnChanceBar => m_SpawnChancePercent;

    [BoxGroup("Wave Settings")]
    [HorizontalGroup("Wave Settings/Split", 0.5f)]
    [LabelText("Min Wave"), MinValue(0)]
    [OnValueChanged("ClampMinWave")]
    [ValidateInput("@MinWave <= MaxWave", "Min must be less than or equal to Max")]
    public int MinWave;

    [HorizontalGroup("Wave Settings/Split", 0.5f)]
    [LabelText("Max Wave"), MinValue(0)]
    [OnValueChanged("ClampMaxWave")]
    [ValidateInput("@MaxWave >= MinWave", "Max must be greater than or equal to Min")]
    public int MaxWave;

    [BoxGroup("Quantity Settings")]
    [HorizontalGroup("Quantity Settings/Split", 0.5f)]
    [LabelText("Min Quantity"), Range(0, 9)]
    [OnValueChanged("ClampMinQuantity")]
    [ValidateInput("@MinQuantity <= MaxQuantity", "Min must be less than or equal to Max")]
    public int MinQuantity;

    [HorizontalGroup("Quantity Settings/Split", 0.5f)]
    [LabelText("Max Quantity"), Range(0, 9)]
    [OnValueChanged("ClampMaxQuantity")]
    [ValidateInput("@MaxQuantity >= MinQuantity", "Max must be greater than or equal to Min")]
    public int MaxQuantity;

    private Color GetSpawnChanceColor(float value)
    {
        if (value < 30f) return Color.red;
        if (value < 70f) return Color.yellow;
        return Color.cyan;
    }
    private void ClampMinQuantity()
    {
        if (MinQuantity > MaxQuantity)
            MinQuantity = MaxQuantity;
    }

    private void ClampMaxQuantity()
    {
        if (MaxQuantity < MinQuantity)
            MaxQuantity = MinQuantity;
    }
    private void ClampMinWave()
    {
        if (MinWave > MaxWave)
            MinWave = MaxWave;
    }

    private void ClampMaxWave()
    {
        if (MaxWave < MinWave)
            MaxWave = MinWave;
    }
}

[Serializable]
public class TurretWaveDataConfig
{
    [Title("++++++++++++++++++O++++++++++++++++++", TitleAlignment = TitleAlignments.Centered), ReadOnly] 
    public string Data;

    [BoxGroup("Wave Settings")]
    [HorizontalGroup("Wave Settings/Split", 0.5f)]
    [LabelText("Min Wave"), MinValue(0)]
    [OnValueChanged("ClampMinWave")]
    [ValidateInput("@MinWave <= MaxWave", "Min must be less than or equal to Max")]
    public int MinWave;

    [HorizontalGroup("Wave Settings/Split", 0.5f)]
    [LabelText("Max Wave"), MinValue(0)]
    [OnValueChanged("ClampMaxWave")]
    [ValidateInput("@MaxWave >= MinWave", "Max must be greater than or equal to Min")]
    public int MaxWave;

    [BoxGroup("Turret Info")]
    [LabelText("Turret Type"), EnumToggleButtons]
    public TurretType TurretType;

    [BoxGroup("Turret Info")]
    [LabelText("Unit SO")]
    public UnitSO UnitSO;

    private void ClampMinWave()
    {
        if (MinWave > MaxWave)
            MinWave = MaxWave;
    }

    private void ClampMaxWave()
    {
        if (MaxWave < MinWave)
            MaxWave = MinWave;
    }
}


