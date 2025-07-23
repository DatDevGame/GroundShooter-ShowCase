using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InfiniteMapSpawner : MonoBehaviour
{
    [Header("Player Tracking")]
    [SerializeField] private Transform m_Player;

    [SerializeField, BoxGroup("Data")] private LevelManagerSO m_LevelManagerSO;

    [Header("Spawn Settings")]
    [SerializeField] private Vector3 m_SpawnOffset = new(0, 0, 50f);
    [SerializeField] private int m_MaxMapCount = 3;

    private readonly Queue<GameObject> m_ActiveMaps = new();
    private readonly Queue<GameObject> m_MapPool = new();
    private Transform m_LastMapEndPoint;

    private void Awake()
    {
        if (m_Player == null)
            m_Player = FindObjectOfType<PlayerMovementController>().transform;

        for (int i = 0; i < m_MaxMapCount; i++)
            SpawnNextMap();
    }
    private void Update()
    {
        if (m_Player == null || m_LastMapEndPoint == null)
            return;

        GameObject newMap = m_ActiveMaps.First();
        float distanceToEnd = Vector3.Distance(m_Player.position, newMap.transform.position);
        if (distanceToEnd > m_SpawnOffset.z)
        {
            SpawnNextMap();
            if (m_ActiveMaps.Count > m_MaxMapCount)
            {
                GameObject oldMap = m_ActiveMaps.Dequeue();
                ReturnMapToPool(oldMap);
            }
        }
    }

    private void SpawnNextMap()
    {
        if (m_LevelManagerSO.GetCurrentLevelData().Map == null)
        {
            LGDebug.LogWarning("No valid prefab found to spawn.");
            return;
        }

        Vector3 spawnPosition = m_LastMapEndPoint != null ? m_LastMapEndPoint.localPosition + m_SpawnOffset : Vector3.zero;

        MapAssemblerShuffle map = GetMapFromPoolOrInstantiate();
        map.ShuffleMap();
        map.transform.localPosition = spawnPosition;
        map.gameObject.SetActive(true);
        m_ActiveMaps.Enqueue(map.gameObject);
        m_LastMapEndPoint = map.transform;
    }

    private MapAssemblerShuffle GetMapFromPoolOrInstantiate()
    {
        GameObject mapObj;
        if (m_MapPool.Count > 0)
        {
            mapObj = m_MapPool.Dequeue();
        }
        else
        {
            mapObj = Instantiate(m_LevelManagerSO.GetCurrentLevelData().Map, Vector3.zero, Quaternion.identity, transform).gameObject;
        }
        return mapObj.GetComponent<MapAssemblerShuffle>();
    }

    private void ReturnMapToPool(GameObject mapObj)
    {
        mapObj.SetActive(false);
        m_MapPool.Enqueue(mapObj);
    }
}
