using UnityEngine;

public class EnemySuicide : MonoBehaviour, IAttackable
{
    [SerializeField]
    private EnemyUnit unit;
    [SerializeField]
    private float radius = 1f;
    [SerializeField]
    private LayerMask hitLayers;

    private Vector3 lastPosition;
    private RaycastHit[] hits = new RaycastHit[1];


    private void Start()
    {
        lastPosition = transform.position;
    }

    private void Update()
    {
        var delta = transform.position - lastPosition;
        if (delta.magnitude > 0)
        {
            // Raycast from previous position to new position
            if (Physics.SphereCastNonAlloc(lastPosition, radius, delta.normalized, hits, delta.magnitude, hitLayers, QueryTriggerInteraction.Collide) > 0)
            {
                if (hits[0].collider.TryGetComponent(out PlayerUnit playerUnit))
                {
                    playerUnit.TakeDamage(this);
                    unit.TakeDamage(new HitAttack(unit.GetMaxHealth()));
                    // HapticManager.Instance.PlayFlashHaptic(HapticTypes.LightImpact);
                }
            }
        }
        lastPosition = transform.position;
    }

    public float GetAttackDamage()
    {
        return unit.Stats.GetStatValue(StatType.AttackDamage);
    }

    public float GetCriticalDamageMultiplier()
    {
        return unit.Stats.GetStatValue(StatType.CritDamageMultiplier);
    }

    public float GetCriticalHitChance()
    {
        return unit.Stats.GetStatValue(StatType.CritChance);
    }

    public float GetInstantKillChance()
    {
        return unit.Stats.GetStatValue(StatType.InstantKillChance);
    }
}