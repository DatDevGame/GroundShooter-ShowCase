using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;

[System.Serializable]
public class WaveData
{
    [Min(0f)]
    public float WaveStartDelay = 0f;

    [Min(0f)]
    public float SpawnDelayBetweenGroups = 0.5f;

    [ShowInInspector, LabelText("Enemy Groups")]
    [ListDrawerSettings(Expanded = true, ShowIndexLabels = true)]
    public List<EnemyGroupData> EnemyGroups = new();
}
