using LatteGames.PoolManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using LatteGames;
using LatteGames.Template;


#if UNITY_EDITOR
using UnityEditor;
#endif

public enum StormFireMode
{
    Sweep,
    FanBurst,
    Random,
    Focused
}

public class StormtapBot : MissileTurret
{
    [SerializeField, BoxGroup("Storm Settings")] private StormFireMode m_FireMode = StormFireMode.Sweep;
    [SerializeField, BoxGroup("Storm Settings")] private int m_MissileCount = 12;
    [SerializeField, BoxGroup("Storm Settings")] private float m_AngleRange = 90f;
    [SerializeField, BoxGroup("Storm Settings")] private float m_AttackCooldown = 2f;
    [SerializeField, BoxGroup("Storm Settings")] private float m_StepDelay = 0.05f;
    [SerializeField, BoxGroup("Storm Settings")] private float m_RotateStepDuration = 0.05f;

    private bool m_IsSweeping = false;
    private bool m_SweepDirection = true; // true = A->B, false = B->A
    protected override void Start()
    {
        base.Start();
        StartCoroutine(StormLoop());
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
        }
        else
        {
            m_ResetInitDuration -= Time.deltaTime;
            if (m_ResetInitDuration > 0)
                return;

            Quaternion defaultRotation = Quaternion.LookRotation(transform.forward, Vector3.up);
            m_TurretHead
                .DORotateQuaternion(defaultRotation, 1f)
                .SetEase(Ease.OutSine);
        }
    }


    private IEnumerator StormLoop()
    {
        WaitForSeconds waitCooldown = new WaitForSeconds(m_AttackCooldown);

        while (true)
        {
            yield return null;

            if (m_CurrentTarget != null && !m_IsSweeping)
            {
                m_IsSweeping = true;

                switch (m_FireMode)
                {
                    case StormFireMode.Sweep:
                        yield return SweepFireRoutine();
                        break;
                    case StormFireMode.FanBurst:
                        FireFanBurst();
                        break;
                    case StormFireMode.Random:
                        FireRandomPattern();
                        break;
                    case StormFireMode.Focused:
                        FireFocused();
                        break;
                }

                m_IsSweeping = false;
                yield return waitCooldown;
            }
        }
    }

    #region Sweep Fire

    private IEnumerator SweepFireRoutine()
    {
        float angleStep = m_AngleRange / (m_MissileCount - 1);
        float halfAngle = m_AngleRange / 2f;

        Vector3 targetPos = m_CurrentTarget.position;
        Vector3 headPos = m_TurretHead.position;
        Vector3 dirToTarget = (targetPos - headPos).normalized;
        float distance = Vector3.Distance(targetPos, headPos);

        List<Quaternion> sweepRotations = new List<Quaternion>();

        for (int i = 0; i < m_MissileCount; i++)
        {
            float angle = -halfAngle + i * angleStep;
            if (!m_SweepDirection) angle = halfAngle - i * angleStep; // reverse

            Vector3 rotatedDir = Quaternion.AngleAxis(angle, Vector3.up) * (-dirToTarget);
            Vector3 virtualPos = targetPos + rotatedDir * distance;

            Vector3 flatLookDir = (targetPos - virtualPos);
            flatLookDir.y = 0f;
            flatLookDir.Normalize();

            sweepRotations.Add(Quaternion.LookRotation(flatLookDir));
        }

        m_TurretHead
                .DORotateQuaternion(sweepRotations[0], 1f)
                .SetEase(Ease.OutSine);
        yield return new WaitForSeconds(1);

        foreach (var rot in sweepRotations)
        {
            Tween tween = m_TurretHead
                .DORotateQuaternion(rot, m_RotateStepDuration)
                .SetEase(Ease.OutSine);

            yield return tween.WaitForCompletion();

            Vector3 euler = m_TurretHead.eulerAngles;
            m_TurretHead.rotation = Quaternion.Euler(0f, euler.y, 0f);

            foreach (var firePoint in m_FirePoints)
                SpawnMissile(firePoint.position, m_TurretHead.rotation);

            yield return new WaitForSeconds(m_StepDelay);
        }

        m_SweepDirection = !m_SweepDirection;

        Quaternion defaultRotation = Quaternion.LookRotation(transform.forward, Vector3.up);
        m_TurretHead
            .DORotateQuaternion(defaultRotation, 1f)
            .SetEase(Ease.OutSine);
    }

    #endregion

    #region Other Fire Modes

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

    private void FireRandomPattern()
    {
        Vector3 dir = (m_CurrentTarget.position - m_TurretHead.position);
        dir.y = 0f;
        dir.Normalize();

        for (int i = 0; i < m_MissileCount; i++)
        {
            float angle = Random.Range(-m_AngleRange / 2f, m_AngleRange / 2f);
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up) * Quaternion.LookRotation(dir);

            foreach (var firePoint in m_FirePoints)
                SpawnMissile(firePoint.position, rot);
        }
    }

    private void FireFocused()
    {
        Vector3 dir = (m_CurrentTarget.position - m_TurretHead.position);
        dir.y = 0f;
        dir.Normalize();

        Quaternion rot = Quaternion.LookRotation(dir);

        foreach (var firePoint in m_FirePoints)
        {
            for (int i = 0; i < m_MissileCount; i++)
                SpawnMissile(firePoint.position, rot);
        }
    }

    #endregion

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

            if (m_FireEZAnimSequence != null)
                m_FireEZAnimSequence.Play();

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
