using UnityEngine;

public class ExpDropSystem : MonoBehaviour
{
    private void Start()
    {
        EnemyUnit.OnDead += OnEnemyDead;
    }

    private void OnDestroy()
    {
        EnemyUnit.OnDead -= OnEnemyDead;
    }

    private void OnEnemyDead(EnemyUnit enemyUnit)
    {
        ExpLevelProgressionSO playerLevelSO = PlayerUnit.Instance.PlayerLevelSO;
        playerLevelSO.AddExp(enemyUnit.UnitSO.ExpDropOnDead);
    }
}