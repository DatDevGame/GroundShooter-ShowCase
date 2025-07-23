using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] protected Unit enemyPrefab;
    [SerializeField] protected List<Unit> enemies;
    public Action<EnemySpawner> OnWaveCompleted;
    public virtual void Spawn()
    {
        this.transform.position += Vector3.forward * PlayerMovementController.Instance.GetPlayerOffsetFromStart();
    }
}
