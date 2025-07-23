using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using LatteGames.GameManagement;
using HyrphusQ.Events;
using System.Linq;
using LatteGames;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class WaveSpawnerManager : MonoBehaviour
{
    public event Action OnLevelFinished;

    [Title("Editor")]
    [ShowInInspector, ReadOnly] private LevelDataSO m_LevelData;
    [SerializeField] private Vector3 m_Offset;

    [Title("State")]
    [ReadOnly] public int CurrentKillWave = 0;
    [ReadOnly] public int CurrentKillGroup = 0;
    [ReadOnly] public int CurrentMaxKillWave = 0;
    [ReadOnly] public bool IsSpawning = false;

    public TurretSpawnerHandle TurretSpawnerHandle;
    [SerializeField, BoxGroup("Data")] private LevelManagerSO m_LevelManagerSO;
    [SerializeField, BoxGroup("Data")] private ExpDropDataSO m_ExpDropDataSO;

    private readonly List<Unit> m_AliveEnemies = new();
    private readonly List<Tween> m_ActiveGroupTweens = new();
    private readonly List<(BezierPathSO path, string label)> m_DebugPaths = new();

    private PlayerMovementController m_PlayerMovementController;
    private PlayerUnit m_PlayerUnit;
    [ShowInInspector]private float m_TimeWatingWave = 0;
    private const float TIME_WATING_WAVE = 3;

    private void Awake()
    {
        if(m_PlayerMovementController == null)
            m_PlayerMovementController = FindObjectOfType<PlayerMovementController>();
        if (m_PlayerMovementController != null)
        {
            transform.SetParent(m_PlayerMovementController.transform);
            transform.localPosition = m_Offset;
        }

        if (m_PlayerUnit == null)
            m_PlayerUnit = FindAnyObjectByType<PlayerUnit>();
        if(m_PlayerUnit != null)
            m_PlayerUnit.OnUnitDead += PlayerUnit_OnUnitDead;

        GameEventHandler.AddActionEvent(GameFlowState.StartLevel, OnStartLevel);
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (m_AliveEnemies.Count > 0)
            {
                m_AliveEnemies
                    .Where(x => x != null).ToList()
                    .ForEach(v => v.Dead());
            }
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            IsSpawning = false;
            OnLevelFinished?.Invoke();
            GameStateManager.Instance.EndGame(true);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            IsSpawning = false;
            OnLevelFinished?.Invoke();
            GameStateManager.Instance.EndGame(false);
        }

#endif
        m_TimeWatingWave -= Time.deltaTime;
    }

    private void PlayerUnit_OnUnitDead(Unit obj)
    {
        GameStateManager.Instance.EndGame(false);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(GameFlowState.StartLevel, OnStartLevel);
        m_LevelManagerSO.CurrentWave.value = 1;
    }

    private void OnStartLevel(params object[] parrams)
    {
        if (parrams.Length <= 0)
            return;

        if (parrams[0] is LevelDataSO levelDataSO)
        {
            StartSpawning(levelDataSO);
        }
    }
    private void OnEndLevel(params object[] parrams)
    {
        if (parrams.Length <= 0)
            return;
    }

    private void StartSpawning(LevelDataSO levelSO)
    {
        m_LevelData = levelSO;
        if (!IsSpawning && m_LevelData != null)
        {
            m_LevelManagerSO.CurrentWave.value = 1;
            StartCoroutine(SpawnLevelRoutine());
        }
    }

    private IEnumerator SpawnLevelRoutine()
    {
        IsSpawning = true;

        while (m_LevelManagerSO.CurrentWave.value < m_LevelData.Waves.Count + 1)
        {
            WaveData wave = m_LevelData.Waves[m_LevelManagerSO.CurrentWave.value - 1];
            yield return StartCoroutine(SpawnWave(wave));
            m_LevelManagerSO.CurrentWave.value++;
        }

        yield return new WaitUntil(() => !IsSpawning);
        yield return StartCoroutine(EndLevel());
    }

    private IEnumerator SpawnWave(WaveData wave)
    {
        yield return new WaitForSeconds(wave.WaveStartDelay);
        CurrentKillWave = 0;
        CurrentMaxKillWave = wave.EnemyGroups.Sum(group => group.Rows * group.Columns);
        GameEventHandler.Invoke(WaveStats.OnKillEnemy, CurrentKillWave, CurrentMaxKillWave);
        GameEventHandler.Invoke(WaveStats.Spawning, m_LevelManagerSO.CurrentWave.value, m_LevelData.Waves.Count);

        m_TimeWatingWave = TIME_WATING_WAVE;
        for (int groupIndex = 0; groupIndex < wave.EnemyGroups.Count; groupIndex++)
        {
            CurrentKillGroup = 0;
            var group = wave.EnemyGroups[groupIndex];
            TurretSpawnerHandle.SpawnTurrets();
            StartCoroutine(SpawnEnemyGroup(group, groupIndex, wave));

            if (group.WaitForDeath)
            {
                yield return new WaitUntil(() => m_TimeWatingWave <= 0 && CurrentKillGroup >= group.Columns * group.Rows);
                yield return new WaitUntil(() => m_AliveEnemies.Count <= 0);
                m_TimeWatingWave = TIME_WATING_WAVE;
            }
        }
    }

    private IEnumerator SpawnEnemyGroup(EnemyGroupData group, int groupIndex, WaveData wave)
    {
        IsSpawning = true;
        yield return new WaitForSeconds(group.StartDelayGroup);

        group.Resize(group.Rows, group.Columns);

        if (group.Path == null || group.Path.Points.Count < 2)
        {
            Debug.LogWarning($"❌ Wave_{m_LevelManagerSO.CurrentWave.value} | Group_{groupIndex} has invalid Path.");
            yield break;
        }

        Vector3 centerOffset = new(
            (group.Columns - 1) * group.Offset.x * 0.5f,
            0f,
            0f
        );

        List<Unit> groupEnemies = new();
        List <PathFollower> pathFollowers = new();
        for (int r = 0; r < group.Rows; r++)
        {
            string groupName = $"Wave_{m_LevelManagerSO.CurrentWave.value} | Group_{groupIndex} | Row_{r + 1}";

            GameObject groupParent = new(groupName);
            groupParent.transform.SetParent(transform, false);

            Vector3 rowOffset = new Vector3(0f, 0f, r * group.Offset.y);
            groupParent.transform.localPosition = group.Path.GetPoint(0f) + rowOffset;

            var follower = groupParent.AddComponent<PathFollower>();
            follower.SetLookForward(group.LookForward);
            follower.SetDefaultDirection(group.FacingDirection);
            follower.SetLoopPath(group.LoopPath);
            pathFollowers.Add(follower);

            follower.SetPath(group.Path, group.Speed, false);

            if (!m_DebugPaths.Exists(p => p.path == group.Path && p.label == groupName))
                m_DebugPaths.Add((group.Path, groupName));

            float rowDelayTime = group.Offset.y / Mathf.Max(group.Speed, 0.01f);
            yield return new WaitForSeconds(rowDelayTime);

            for (int c = 0; c < group.Columns; c++)
            {
                var info = group[r, c];
                if (info == null || info.EnemyPrefab == null)
                    continue;

                m_TimeWatingWave = TIME_WATING_WAVE;
                Vector3 localOffset = new Vector3((group.Columns - 1 - c) * group.Offset.x, 0f, 0f);
                GameObject enemy = Instantiate(info.EnemyPrefab, groupParent.transform);
                enemy.transform.localPosition = localOffset - centerOffset;
                Unit unit = enemy.GetComponent<Unit>();
                if (unit != null)
                {
                    unit.UnitSO = info.UnitStats;
                    m_AliveEnemies.Add(unit);
                    groupEnemies.Add(unit);
                    unit.OnUnitDamageTaken += OnUnitDamageTaken;
                }

                follower.Units.Add(unit);
            }
        }
        IsSpawning = false;
    }


    private void OnUnitDamageTaken(Unit unit, float arg2, IAttackable attackable)
    {
        if (unit == null)
            return;

        if (unit.IsDead())
        {
            m_AliveEnemies.Remove(unit);
            unit.OnUnitDamageTaken -= OnUnitDamageTaken;

            CurrentKillGroup++;
            CurrentKillWave++;
            GameEventHandler.Invoke(WaveStats.OnKillEnemy, CurrentKillWave, CurrentMaxKillWave);
        }
    }

    private IEnumerator EndLevel()
    {
        yield return new WaitUntil(() => m_AliveEnemies.Count <= 0);
        IsSpawning = false;
        OnLevelFinished?.Invoke();
        GameStateManager.Instance.EndGame(true);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (m_DebugPaths == null || m_DebugPaths.Count == 0) return;

        Color[] colors = new Color[]
        {
            Color.red, Color.green, Color.blue, Color.yellow,
            Color.cyan, Color.magenta, new Color(1f, 0.5f, 0f), new Color(0.5f, 0f, 1f)
        };

        for (int i = 0; i < m_DebugPaths.Count; i++)
        {
            var (path, label) = m_DebugPaths[i];
            if (path == null || path.Points == null || path.Points.Count < 2) continue;

            Gizmos.color = colors[i % colors.Length];
            const int res = 30;

            Vector3 prev = transform.TransformPoint(path.GetPoint(0f));

            for (int j = 1; j <= res; j++)
            {
                float t = j / (float)res;
                Vector3 point = transform.TransformPoint(path.GetPoint(t));
                Gizmos.DrawLine(prev, point);
                prev = point;
            }

            GUIStyle labelStyle = new GUIStyle
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState { textColor = Gizmos.color }
            };

            Handles.Label(transform.TransformPoint(path.Points[0].Position + Vector3.up * 0.3f), label, labelStyle);
        }
    }
#endif
}
