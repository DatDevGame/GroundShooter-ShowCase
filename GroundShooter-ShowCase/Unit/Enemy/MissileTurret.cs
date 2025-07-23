using LatteGames.PoolManagement;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections;
using DG.Tweening;
using LatteGames;
using LatteGames.Template;
using UnityEngine.Events;

public class MissileTurret : BaseTurret
{
    public UnityEvent onFired = new UnityEvent();

    [SerializeField, BoxGroup("Ref")]
    protected EZAnimSequence m_FireEZAnimSequence;

    [SerializeField, BoxGroup("Resource")]
    protected Transform[] m_FirePoints;

    [SerializeField, BoxGroup("Turret")]
    protected Transform m_TurretHead;

    [SerializeField, BoxGroup("Turret")]
    protected float m_RotationDuration = 0.4f;

    [SerializeField, BoxGroup("Turret")]
    protected float m_AimToleranceAngle = 3f;

    [SerializeField, BoxGroup("Combat")]
    protected float m_FireDelayPerMissile = 0.3f;
    [SerializeField, BoxGroup("Combat")]
    protected float m_TimeDelayReset = 1f;
    [SerializeField, BoxGroup("Combat")]
    protected Vector3 m_OffsetTarget = Vector3.zero;

    protected Quaternion m_InitialRotation;
    protected Tween m_RotateTween;

    protected float m_ResetInitDuration = 0;

    protected override void Start()
    {
        base.Start();
        m_InitialRotation = m_TurretHead.rotation;
        m_ResetInitDuration = m_TimeDelayReset;
    }

    protected void Awake()
    {
        if (m_Unit == null)
            m_Unit = GetComponent<Unit>();
    }

    protected override void Update()
    {
        base.Update();
        HandleAiming();
    }

    protected void HandleAiming()
    {
        if (m_TurretHead == null) return;

        if (m_CurrentTarget != null)
        {
            Vector3 directionToTarget = m_CurrentTarget.position - m_TurretHead.position;

            // ✅ Ignore vertical Y offset
            directionToTarget.y = 0f;

            if (directionToTarget.sqrMagnitude < 0.01f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget.normalized + m_OffsetTarget, Vector3.up);
            float angleDiff = Quaternion.Angle(m_TurretHead.rotation, targetRotation);

            if (angleDiff > m_AimToleranceAngle)
            {
                if (m_RotateTween != null)
                {
                    m_RotateTween.Kill();
                    m_RotateTween = null;
                }

                if (m_RotateTween == null || !m_RotateTween.IsActive())
                {
                    m_ResetInitDuration = m_TimeDelayReset;
                    m_RotateTween = m_TurretHead
                        .DORotateQuaternion(targetRotation, m_RotationDuration)
                        .SetEase(Ease.OutSine);
                }
            }
        }
        else
        {
            m_ResetInitDuration -= Time.deltaTime;
            if (m_ResetInitDuration > 0)
                return;

            // ✅ Default rotation (still only horizontal)
            Vector3 forward = transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.01f) return;

            Quaternion defaultRotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
            m_TurretHead
                .DORotateQuaternion(defaultRotation, 1f)
                .SetEase(Ease.OutSine);
        }
    }


    protected override void Attack()
    {
        if (m_CurrentTarget == null) return;
        StartCoroutine(FireMissilesWithDelay());
    }

    protected IEnumerator FireMissilesWithDelay()
    {
        foreach (var point in m_FirePoints)
        {
            if (m_CurrentTarget == null) yield break;

            BaseMissile missile = PoolManager
                .GetOrCreatePool(m_BaseMissilePrefab, initialCapacity: 1)
                .Get();

            if (missile != null)
            {
                missile.gameObject.SetActive(true);
                missile.transform.position = point.position;
                missile.transform.rotation = point.rotation;
                missile.SetTarget(m_CurrentTarget, this);

                if (m_FireVFX != null)
                {
                    var fireVFX = PoolManager.GetOrCreatePool(m_FireVFX).Get();
                    fireVFX.transform.position = point.position;
                    fireVFX.Play();
                    fireVFX.Release(m_FireVFX, 5f);
                }

                if (m_FireEZAnimSequence != null && m_FireEZAnimSequence.gameObject.activeInHierarchy)
                    m_FireEZAnimSequence.Play();

                onFired.Invoke();

                SoundManager.Instance.PlaySFX(SFX.AssaultRifle, 0.25f);
            }

            yield return new WaitForSeconds(m_FireDelayPerMissile);
        }
    }
}
