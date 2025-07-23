using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HyrphusQ.Events;

public class PathFollower : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private bool m_LookForward = true;
    [SerializeField] private Vector3 m_DefaultDirection = Vector3.forward;
    [SerializeField] private bool m_LoopPath = false;

    private BezierPathSO m_Path;
    private Coroutine m_MoveRoutine;
    private Transform m_Transform;

    public List<Unit> Units = new();

    private void Awake()
    {
        m_Transform = transform;

        GameEventHandler.AddActionEvent(GameFlowState.EndLevel, OnEndLevel);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(GameFlowState.EndLevel, OnEndLevel);
    }

    private void OnEndLevel()
    {
        Stop();
    }

    public void SetPath(BezierPathSO path, float speed, bool destroyOnFinish)
    {
        Units = new List<Unit>();

        if (path == null || path.Points.Count < 2)
        {
            Debug.LogWarning("❌ Invalid path.");
            return;
        }

        m_Path = path;
        m_Path.BuildDistanceTable(); // ✅ Build lookup table để GetPointAtDistance chính xác

        if (m_MoveRoutine != null)
            StopCoroutine(m_MoveRoutine);

        m_MoveRoutine = StartCoroutine(MoveAlongPath(speed, destroyOnFinish));
    }

    private IEnumerator MoveAlongPath(float speed, bool destroyOnFinish)
    {
        float currentDistance = 0f;
        float pathLength = m_Path.GetTotalLength(); // Không cần build lại nữa vì có table rồi

        while (currentDistance < pathLength)
        {
            Vector3 position = m_Path.GetPointAtDistance(currentDistance);
            m_Transform.localPosition = position;

            // Look forward if needed
            if (m_LookForward)
            {
                float lookAhead = 0.1f;
                float futureDistance = Mathf.Min(currentDistance + lookAhead, pathLength);
                Vector3 direction = (m_Path.GetPointAtDistance(futureDistance) - position).normalized;

                if (direction.sqrMagnitude > 0.0001f)
                    m_Transform.forward = direction;
            }
            else
            {
                m_Transform.forward = m_DefaultDirection;
            }

            // Unit logic
            if (Units.Count > 0 && Units.All(u => u == null || u.IsDead()))
            {
                Stop();
                yield break;
            }

            currentDistance += speed * Time.deltaTime;
            yield return null;
        }

        if (m_LoopPath)
        {
            SetPath(m_Path, speed, destroyOnFinish);
        }
        else if (destroyOnFinish)
        {
            Destroy(gameObject);
        }
    }

    public void Stop()
    {
        if (m_MoveRoutine != null)
        {
            StopCoroutine(m_MoveRoutine);
            m_MoveRoutine = null;
        }
    }

    public void SetLookForward(bool lookForward) => m_LookForward = lookForward;

    public void SetLoopPath(bool loop) => m_LoopPath = loop;

    public void SetDefaultDirection(FacingDirection facingDirection)
    {
        m_DefaultDirection = GetDirection(facingDirection);
    }

    private Vector3 GetDirection(FacingDirection facingDirection)
    {
        return facingDirection switch
        {
            FacingDirection.Left => Vector3.left,
            FacingDirection.Right => Vector3.right,
            FacingDirection.Forward => Vector3.forward,
            FacingDirection.Backward => Vector3.back,
            _ => Vector3.forward,
        };
    }

    private void OnDisable()
    {
        Stop();
    }
}

public enum FacingDirection
{
    Forward,
    Backward,
    Left,
    Right
}
