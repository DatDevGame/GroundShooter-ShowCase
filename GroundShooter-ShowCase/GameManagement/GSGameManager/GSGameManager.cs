using System;
using UnityEngine;

public class GSGameManager : Singleton<GSGameManager>
{
    private const string k_DebugTag = nameof(GSGameManager);
    private const string k_GameManagerName = "GSGameManager";

    private static event Action s_OnSpawnCompleted = delegate { };
    public static event Action onSpawnCompleted
    {
        add
        {
            if (isSpawned)
                value?.Invoke();
            s_OnSpawnCompleted += value;
        }
        remove
        {
            s_OnSpawnCompleted -= value;
        }
    }
    public static bool isSpawned => Instance != null;

    private static void NotifyEventSpawnCompleted()
    {
        s_OnSpawnCompleted.Invoke();
        if (Instance.m_Verbose)
            LGDebug.Log("GSGameManager is spawned completely", k_DebugTag);
    }

    /// <summary>
    /// Auto spawn GameManager before scene is loaded
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnRuntimeInitializeLoad()
    {
        Instantiate(Resources.Load(k_GameManagerName)).name = k_GameManagerName;
        NotifyEventSpawnCompleted();
    }
}