using System;
using System.Collections;
using System.Collections.Generic;
using LatteGames.PoolManagement;
using UnityEngine;

public class ItemDropSystem : MonoBehaviour
{
    [SerializeField]
    private PickableItem hpPowerUpPrefab;
    [SerializeField]
    private LevelManagerSO levelManagerSO;
    private LevelDataSO levelDataSO;

    private UnityObjectPool<PickableItem> hpPowerUpPool;

    private void Awake()
    {
        hpPowerUpPool = PoolManager.GetOrCreatePool(hpPowerUpPrefab.name, hpPowerUpPrefab);
        levelDataSO = levelManagerSO.GetCurrentLevelData();
        EnemyUnit.OnDead += HandleEnemyDeath;
    }

    private void OnDestroy()
    {
        EnemyUnit.OnDead -= HandleEnemyDeath;
    }

    private void HandleEnemyDeath(EnemyUnit unit)
    {
        var hpPowerUpDropChance = levelDataSO.HpPowerUpDropChance;
        if (PlayerUnit.Instance.GetCurrentHealth() < PlayerUnit.Instance.GetMaxHealth() && hpPowerUpDropChance > 0f && UnityEngine.Random.value <= hpPowerUpDropChance)
        {
            // Assuming you have a method to spawn a health power-up item
            SpawnHealthPowerUp(unit.transform.position + Vector3.up);
        }
    }

    private void SpawnHealthPowerUp(Vector3 position)
    {
        // Implement the logic to spawn a health power-up item at the given position
        // This could involve instantiating a prefab, using a pool, etc.
        var hpPowerUp = hpPowerUpPool.Get();
        hpPowerUp.transform.SetPositionAndRotation(position, Quaternion.identity);
        hpPowerUp.PlaySpawnAnimation();
    }
}