using HyrphusQ.Events;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Cinemachine.CinemachineTriggerAction.ActionSettings;


[Serializable]
public class GameInfo
{
    public GameInfo(GameFlowState gameFlowState)
    {
        Result = gameFlowState;
    }

    [Header("Result")]
    public GameFlowState Result = GameFlowState.None;
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public GameFlowState CurrentState { get; private set; } = GameFlowState.None;

    [SerializeField, BoxGroup("Data")] private LevelManagerSO m_LevelManagerSO;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        TimeScaleHandler.Instance.Resume();
    }

    private void Start()
    {
        GameEventHandler.Invoke(GameFlowState.StartLevel, m_LevelManagerSO.GetCurrentLevelData());
    }

    public void EndGame(bool isWin)
    {
        GameEventHandler.Invoke(GameFlowState.EndLevel, m_LevelManagerSO.GetCurrentLevelData(), isWin);
        if (isWin)
            m_LevelManagerSO.SetWinCurrentLevel();
        TimeScaleHandler.Instance.Pause();
    }
}
