using Sirenix.Utilities;
using UnityEngine;

public class BossSpawner : EnemySpawner
{
    public override void Spawn()
    {
        base.Spawn();
        
        var enemyHolder = new GameObject($"Wave_Boss");
        enemyHolder.transform.SetParent(transform);

        Unit enemy = Instantiate(enemyPrefab);
        enemyHolder.transform.localPosition = Vector3.zero;
        enemy.transform.SetParent(enemyHolder.transform);
        enemy.transform.localPosition = Vector3.zero;
        enemy.OnUnitDead += HandleOnEnemyDie;
        enemies.Add(enemy);
    }
    void HandleOnEnemyDie(Unit unit)
    {
        unit.OnUnitDead -= HandleOnEnemyDie;
        enemies.Remove(unit);
        if (enemies.IsNullOrEmpty())
        {
            OnWaveCompleted?.Invoke(this);
        }
    }
}
