using HyrphusQ.Helpers;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyrphusQ.SerializedDataStructure;

[CreateAssetMenu(fileName = "LevelManagerSO", menuName = "Game/LevelManagerSO")]
public class LevelManagerSO : ItemManagerSO
{
    public Variable<int> CurrentWave => m_CurrentWave;
    public Dictionary<TurretType, BaseTurret> TurretDics => m_TurretDics;

    [SerializeField] private PPrefIntVariable m_CurrentLevel;
    [SerializeField] private Variable<int> m_CurrentWave;
    [SerializeField] private SerializedDictionary<TurretType, BaseTurret> m_TurretDics;

    public void SetWinCurrentLevel()
    {
        m_CurrentLevel.value++;

        int itemIndex;
        if (m_CurrentLevel.value <= initialValue.Count)
        {
            itemIndex = m_CurrentLevel.value - 1;
        }
        else
        {
            int loopIndex = (m_CurrentLevel.value - initialValue.Count - 1) % (initialValue.Count - 1);
            itemIndex = 1 + loopIndex;
        }
        m_CurrentItemInUse.value = initialValue[itemIndex];
    }

    public int GetCurrentLevel()
    {
        return m_CurrentLevel.value;
    }
    public LevelDataSO GetCurrentLevelData()
    {
        return GetCurrentItemInUsed<LevelDataSO>();
    }
    public int GetCurrentWave()
    {
        return m_CurrentWave.value;
    }
}
