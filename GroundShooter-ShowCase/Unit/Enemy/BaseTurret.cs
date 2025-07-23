using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public abstract class BaseTurret : MonoBehaviour, IAttackable
{
    public TurretType TurretType => m_TurretType;
    public BaseMissile BaseMissilePrefab => m_BaseMissilePrefab;

    [SerializeField, BoxGroup("Ref")] protected Unit m_Unit;

    [SerializeField, BoxGroup("Config")] protected TurretType m_TurretType;
    [SerializeField, BoxGroup("Config")] protected float m_ReloadTime = 2f;
    [SerializeField, BoxGroup("Config")] protected float m_DetectionRadius = 20f;
    [SerializeField, BoxGroup("Config")] protected float m_ScanInterval = 0.2f;
    [SerializeField, BoxGroup("Config")] protected LayerMask m_TargetMask;

    [SerializeField, BoxGroup("Resource")] protected ParticleSystem m_FireVFX;
    [SerializeField, BoxGroup("Resource")] protected BaseMissile m_BaseMissilePrefab;

    protected Transform m_CurrentTarget;
    protected float m_ReloadTimer;
    protected float m_ScanTimer;

    protected virtual void Start()
    {
        m_ReloadTimer = m_ReloadTime;
        m_ScanTimer = m_ScanInterval;

        m_Unit.OnUnitDead += OnUnitDead;
    }

    protected virtual void OnUnitDead(Unit obj)
    {
        CameraShake.Instance.Shake(0.1f);
        // HapticManager.Instance.PlayFlashHaptic();
    }

    protected virtual void Update()
    {
        ScanForTarget();

        if (m_CurrentTarget == null) return;

        m_ReloadTimer -= Time.deltaTime;
        if (m_ReloadTimer <= 0f)
        {
            Attack();
            m_ReloadTimer = m_ReloadTime;
        }
    }

    protected void ScanForTarget()
    {
        m_ScanTimer -= Time.deltaTime;
        if (m_ScanTimer > 0f) return;

        m_ScanTimer = m_ScanInterval;

        Collider[] hits = Physics.OverlapSphere(transform.position, m_DetectionRadius, m_TargetMask);
        m_CurrentTarget = hits.Length > 0 ? hits[0].transform : null;
    }

    protected abstract void Attack();

#if UNITY_EDITOR
    protected void OnDrawGizmosSelected()
    {
        Handles.color = Color.red;
        Handles.DrawWireDisc(transform.position, Vector3.up, m_DetectionRadius); // mặt phẳng XZ
    }
#endif

    public virtual float GetInstantKillChance() => m_Unit.Stats.GetStatValue(StatType.InstantKillChance, 0f);

    public virtual float GetCriticalHitChance() => m_Unit.Stats.GetStatValue(StatType.CritChance, 0f);

    public virtual float GetCriticalDamageMultiplier() => m_Unit.Stats.GetStatValue(StatType.CritDamageMultiplier, 2f);

    public virtual float GetAttackDamage() => m_Unit.Stats.GetStatValue(StatType.AttackDamage, 0f);

    public virtual void SetEnableCollider(bool isEnabled)
    {
        GetComponent<Collider>().enabled = isEnabled;
    }
}
