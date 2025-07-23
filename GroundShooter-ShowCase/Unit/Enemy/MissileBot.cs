using LatteGames;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileBot : MissileTurret
{
    private static readonly int DirectionBlendKeyXHash = Animator.StringToHash("DirectionBlendKeyX");
    private static readonly int DirectionBlendKeyYHash = Animator.StringToHash("DirectionBlendKeyY");
    private static readonly int SpeedBlendKeyHash = Animator.StringToHash("SpeedBlendKey");
    
    [SerializeField]
    private Animator m_Animator;

    private Vector3 m_LastPosition;

    protected override void Update()
    {
        base.Update();
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
}
