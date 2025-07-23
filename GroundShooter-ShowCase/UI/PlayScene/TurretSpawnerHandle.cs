using DG.Tweening;
using Dreamteck.Utilities;
using LatteGames.PoolManagement;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class TurretSpawnerHandle : MonoBehaviour
{
    [BoxGroup("Config"), SerializeField] private float m_MaxDistanceRemove = 100;
    [BoxGroup("Refs"), SerializeField] private List<Transform> m_TurretSpawners = new();
    [BoxGroup("Resource"), SerializeField] private ParticleSystem m_TurretSpawnVFX;
    [BoxGroup("Data"), SerializeField] private LevelManagerSO m_LevelManagerSO;

    private Vector3 m_LastSpawnPosition;
    private PlayerUnit m_PlayerUnit;
    private LevelDataSO m_CurrentLevelDataSO;
    private List<BaseTurret> m_BaseTurrets;

    private void Start()
    {
        m_BaseTurrets = new List<BaseTurret>();
        m_PlayerUnit = PlayerUnit.Instance;
        m_CurrentLevelDataSO = m_LevelManagerSO.GetCurrentLevelData();

        if (m_PlayerUnit == null)
        {
            Debug.LogError("Player transform is not assigned!");
            enabled = false;
            return;
        }

        m_LastSpawnPosition = m_PlayerUnit.transform.position;
    }

    private void Update()
    {
        // if (Vector3.Distance(m_PlayerUnit.transform.position, m_LastSpawnPosition) >= GetCurrentSpawnDistance())
        // {
        //     SpawnTurrets();
        //     m_LastSpawnPosition = m_PlayerUnit.transform.position;
        // }
    }

[Button]
    public void SpawnTurrets()
    {
        var turretSetup = GetCurrentTurretSetup();
        if (turretSetup == null) return;

        // float chance = Random.Range(0f, 100f);
        // if (chance > turretSetup.m_SpawnChancePercent) return;

        var turretDataList = GetCurrentTurretData();
        if (turretDataList == null || turretDataList.Count == 0) return;

        m_TurretSpawners.Shuffle();
        int spawnCount = Random.Range(turretSetup.MinQuantity, turretSetup.MaxQuantity + 1);
        spawnCount = 2;//Mathf.Min(spawnCount, m_TurretSpawners.Count);

        m_BaseTurrets
            .Where(x => x != null && Vector3.Distance(x.transform.position, m_PlayerUnit.transform.position) > m_MaxDistanceRemove).ToList()
            .ForEach(v => Destroy(v.gameObject));
        m_BaseTurrets = m_BaseTurrets.Where(v => v != null).ToList();

        for (int i = 0; i < spawnCount; i++)
        {
            if (m_TurretSpawners[i] == null) continue;
            if (!m_LevelManagerSO.TurretDics.TryGetRandom(out var turretPrefab)) continue;

            Transform spawner = m_TurretSpawners[i];
            Vector3 groundPos = spawner.position;
            Vector3 skyPos = groundPos + Vector3.up * Random.Range(12f, 16f);
            Quaternion startRot = Quaternion.Euler(Random.Range(-10f, 10f), Random.Range(0f, 360f), Random.Range(-10f, 10f));
            Quaternion endRot = Quaternion.Euler(0f, 180f, 0f);

            BaseTurret turret = Instantiate(turretPrefab, skyPos, startRot);
            turret.SetEnableCollider(false);
            m_BaseTurrets.Add(turret);
            Transform tf = turret.transform;

            float fallTime = Random.Range(0.3f, 1f);

            tf.DOMove(groundPos, fallTime)
                .SetEase(Ease.InSine)
                .SetUpdate(true);

            tf.DORotateQuaternion(endRot, fallTime * 0.9f)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);

            DOVirtual.DelayedCall(fallTime, () =>
            {
                turret.SetEnableCollider(true);
                tf.DOShakePosition(0.2f, strength: 0.1f, vibrato: 10, randomness: 20, fadeOut: true)
                    .SetUpdate(true);

                if (CameraShake.Instance != null)
                    CameraShake.Instance.Shake(0.1f);

                ParticleSystem turretSpawnVFX = PoolManager.GetOrCreatePool(m_TurretSpawnVFX, initialCapacity: 1).Get();
                turretSpawnVFX.transform.position = groundPos;
                turretSpawnVFX.Play();
                turretSpawnVFX.Release(m_TurretSpawnVFX, 5f);

            }, ignoreTimeScale: true); 

            Unit unit = turret.GetComponent<Unit>();
            if (unit != null)
            {
                var turretData = turretDataList.FirstOrDefault(t => t.TurretType == turret.TurretType);
                if (turretData != null)
                    unit.UnitSO = turretData.UnitSO;
            }
        }
    }



    private float GetCurrentSpawnDistance()
    {
        return GetCurrentTurretSetup()?.DistanceSpawn ?? 10f;
    }

    private TurretSetup GetCurrentTurretSetup()
    {
        int currentWave = m_LevelManagerSO.GetCurrentWave();
        return m_CurrentLevelDataSO.TurretData.TurretSetups
            .FirstOrDefault(setup => currentWave >= setup.MinWave && currentWave <= setup.MaxWave);
    }

    private List<TurretWaveDataConfig> GetCurrentTurretData()
    {
        int currentWave = m_LevelManagerSO.GetCurrentWave();
        return m_CurrentLevelDataSO.TurretData.TurretWaveDataConfigs
            .Where(config => currentWave >= config.MinWave && currentWave <= config.MaxWave)
            .ToList();
    }

#if UNITY_EDITOR
    [TitleGroup("Spawner Grid Config")]
    [LabelText("Rows"), MinValue(1)] public int Rows = 2;
    [LabelText("Columns"), MinValue(1)] public int Columns = 3;
    [LabelText("Spacing X")] public float SpacingX = 2f;
    [LabelText("Spacing Z")] public float SpacingZ = 2f;
    [LabelText("Spawn From Ref Point")] public Transform RefPoint;

    [Button("Generate Grid Spawners")]
    private void GenerateTurretGrid()
    {
        ClearTurrets();

        Vector3 center = RefPoint ? RefPoint.position : transform.position;
        Vector3 offset = new Vector3((Columns - 1) * SpacingX / 2f, 0, (Rows - 1) * SpacingZ / 2f);

        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                Vector3 pos = center + new Vector3(col * SpacingX, 0, row * SpacingZ) - offset;
                GameObject go = new GameObject($"Turret_{row}_{col}");
                go.transform.position = pos;
                go.transform.SetParent(RefPoint);
                m_TurretSpawners.Add(go.transform);
            }
        }

        Debug.Log($"Spawned {Rows * Columns} turret spawners.");
    }

    [Button("Clear Spawners")]
    private void ClearTurrets()
    {
        for (int i = RefPoint.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(RefPoint.GetChild(i).gameObject);
        }

        m_TurretSpawners.Clear();
        Debug.Log("Cleared all turret spawners.");
    }

    private void OnDrawGizmosSelected()
    {
        if (m_TurretSpawners == null || m_TurretSpawners.Count == 0) return;

        Gizmos.color = Color.green;
        foreach (var spawner in m_TurretSpawners)
        {
            if (spawner == null) continue;
            Gizmos.DrawSphere(spawner.position, 0.2f);
            UnityEditor.Handles.Label(spawner.position + Vector3.up * 0.3f, spawner.name);
        }

        if (RefPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(RefPoint.position, 0.3f);
        }
    }
#endif
}
