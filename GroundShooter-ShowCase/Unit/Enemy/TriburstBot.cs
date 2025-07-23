using DG.Tweening;
using LatteGames.PoolManagement;
using LatteGames.Template;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TriburstBot : MissileTurret
{
    private static readonly int DirectionBlendKeyXHash = Animator.StringToHash("DirectionBlendKeyX");
    private static readonly int DirectionBlendKeyYHash = Animator.StringToHash("DirectionBlendKeyY");
    private static readonly int SpeedBlendKeyHash = Animator.StringToHash("SpeedBlendKey");

    [SerializeField]
    private Animator m_Animator;
    [SerializeField, BoxGroup("Triburst Settings")] private int m_MissileCount = 12;
    [SerializeField, BoxGroup("Triburst Settings")] private float m_AngleRange = 90f;
    [SerializeField, BoxGroup("Triburst Settings")] private float m_AttackCooldown = 2f;
    [SerializeField, BoxGroup("Triburst Settings")] private float m_StepDelay = 0.05f;
    [SerializeField, BoxGroup("Triburst Settings")] private float m_RotateStepDuration = 0.05f;
    [SerializeField, BoxGroup("Triburst Settings")] private float m_GunLookTriggerAngle = 5f;
    [SerializeField, BoxGroup("Triburst Settings")] private List<Transform> m_Guns;

    private bool m_HasGunLooked = false;
    private Vector3 m_LastPosition;

    protected override void Start()
    {
        base.Start();
        StartCoroutine(Loop());
    }

    protected override void Update()
    {
        ScanForTarget();

        if (m_CurrentTarget != null)
        {
            Vector3 directionToTarget = m_CurrentTarget.position - m_TurretHead.position;
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

            if (!m_HasGunLooked && angleDiff <= m_GunLookTriggerAngle)
                GunLookAt();
            else
                foreach (Transform gun in m_Guns)
                    gun.DOLocalRotate(new Vector3(90, 0, 0), 0.1f);
        }
        else
        {
            m_ResetInitDuration -= Time.deltaTime;
            if (m_ResetInitDuration > 0)
                return;

            Quaternion defaultRotation = Quaternion.LookRotation(transform.forward, Vector3.up);
            m_TurretHead
                .DORotateQuaternion(defaultRotation, 1f)
                .SetEase(Ease.OutSine)
                .OnComplete(() =>
                {
                    foreach (Transform gun in m_Guns)
                        gun.DOLocalRotate(new Vector3(90, 0, 0), 0.1f);
                });
        }

        void GunLookAt()
        {
            if (m_CurrentTarget == null) return;

            Vector3 targetPos = m_CurrentTarget.position;
            foreach (Transform gun in m_Guns)
            {
                Vector3 dirToTarget = targetPos - gun.position;
                dirToTarget.y = 0f;

                if (dirToTarget.sqrMagnitude < 0.01f)
                    continue;

                Quaternion lookYaw = Quaternion.LookRotation(dirToTarget.normalized, Vector3.up);
                Quaternion correctYaw = lookYaw * Quaternion.Euler(0, -90f, 0);
                Vector3 currentEuler = gun.localEulerAngles;
                Vector3 targetEuler = correctYaw.eulerAngles;

                gun.DORotate(new Vector3(currentEuler.x, targetEuler.y + 90f, currentEuler.z) + m_OffsetTarget, 0.25f)
                    .SetEase(Ease.OutSine);
            }
        }

        if (m_Animator != null)
        {
            Vector3 currentPosition = transform.position;
            Vector3 delta = currentPosition - m_LastPosition;
            float speed = delta.magnitude > 0f ? 1f : 0f;
            m_Animator.SetFloat(DirectionBlendKeyXHash, -delta.normalized.x);
            m_Animator.SetFloat(DirectionBlendKeyYHash, -delta.normalized.z);
            m_Animator.SetFloat(SpeedBlendKeyHash, speed);
            m_LastPosition = currentPosition;
        }
    }


    private IEnumerator Loop()
    {
        WaitForSeconds waitCooldown = new WaitForSeconds(m_AttackCooldown);

        while (true)
        {
            yield return null;

            if (m_CurrentTarget != null)
            {
                FireFanBurst();
                yield return waitCooldown;
            }
        }
    }


    private void FireFanBurst()
    {
        float angleStep = m_AngleRange / (m_MissileCount - 1);
        float halfAngle = m_AngleRange / 2f;
        Vector3 dir = (m_CurrentTarget.position - m_TurretHead.position);
        dir.y = 0f;
        dir.Normalize();

        for (int i = 0; i < m_MissileCount; i++)
        {
            float angle = -halfAngle + i * angleStep;
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up) * Quaternion.LookRotation(dir);

            foreach (var firePoint in m_FirePoints)
                SpawnMissile(firePoint.position, rot);
        }
    }

    private void SpawnMissile(Vector3 position, Quaternion rotation)
    {
        StormMissile missile = PoolManager.GetOrCreatePool(m_BaseMissilePrefab as StormMissile, initialCapacity: 1).Get();

        if (missile != null)
        {
            missile.gameObject.SetActive(true);
            missile.transform.position = position;
            missile.transform.rotation = rotation;
            missile.SetBaseTurret(this);

            if (m_FireVFX != null)
            {
                var fireVFX = PoolManager.GetOrCreatePool(m_FireVFX).Get();
                fireVFX.transform.position = position;
                fireVFX.Play();
                fireVFX.Release(m_FireVFX, 5f);
            }

            if (m_FireEZAnimSequence != null && m_FireEZAnimSequence.gameObject.activeInHierarchy)
                m_FireEZAnimSequence.Play();

            onFired.Invoke();

            SoundManager.Instance.PlaySFX(SFX.AssaultRifle, 0.25f);
        }
    }
    protected override void OnUnitDead(Unit obj)
    {
        CameraShake.Instance.Shake(0.1f);
        // HapticManager.Instance.PlayFlashHaptic(HapticTypes.HeavyImpact);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (m_CurrentTarget == null || m_TurretHead == null)
            return;

        Vector3 origin = m_TurretHead.position;
        Vector3 toTarget = (m_CurrentTarget.position - origin);
        toTarget.y = 0f;
        toTarget.Normalize();

        float radius = 5f;
        float halfAngle = m_AngleRange / 2f;

        Quaternion left = Quaternion.AngleAxis(-halfAngle, Vector3.up);
        Quaternion right = Quaternion.AngleAxis(halfAngle, Vector3.up);

        Vector3 leftDir = left * toTarget;
        Vector3 rightDir = right * toTarget;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(origin, toTarget * radius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(origin, leftDir * radius);
        Gizmos.DrawRay(origin, rightDir * radius);

        Handles.color = new Color(1f, 1f, 0f, 0.2f);
        Handles.DrawSolidArc(origin, Vector3.up, leftDir, m_AngleRange, radius);
    }
#endif
}
