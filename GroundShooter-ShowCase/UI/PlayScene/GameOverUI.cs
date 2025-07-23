using GameAnalyticsSDK;
using GSAnalyticsEvents;
using HyrphusQ.Events;
using I2.Loc;
using LatteGames;
using LatteGames.GameManagement;
using LatteGames.PvP;
using LatteGames.Template;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField, BoxGroup("Ref")] private GameObject m_TitleWin;
    [SerializeField, BoxGroup("Ref")] private GameObject m_TitleLose;
    [SerializeField, BoxGroup("Ref")] private TMP_Text m_CoinRewardText;
    [SerializeField, BoxGroup("Ref")] private RewardCardUI m_RewardCardUI;
    [SerializeField, BoxGroup("Ref")] private Button m_BackBtn;
    [SerializeField, BoxGroup("Ref")] private LocalizationParamsManager m_ReachedWaveI2;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility m_MainCanvasGroupVisibility;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility m_ItemRewardCanvasGroup;

    [SerializeField, BoxGroup("Data")] private WeaponLibrarySO m_WeaponLibrarySO;
    [SerializeField, BoxGroup("Data")] private LevelManagerSO m_LevelManagerSO;

    #region Progession Event
    private float m_TimePlayedLevel;
    private int m_TimePlayedSave;
    private int m_LevelStart;
    #endregion

    #region Design Event
    private string m_LastDamageSource;
    #endregion

    private void Awake()
    {
        m_BackBtn.onClick.AddListener(BackButton);
        GameEventHandler.AddActionEvent(GameFlowState.StartLevel, OnStartLevel);
        GameEventHandler.AddActionEvent(GameFlowState.EndLevel, OnEndLevel);
    }

    private void Update()
    {
        m_TimePlayedLevel += Time.deltaTime;
    }

    private void OnDestroy()
    {
        m_BackBtn.onClick.RemoveListener(BackButton);
        GameEventHandler.RemoveActionEvent(GameFlowState.StartLevel, OnStartLevel);
        GameEventHandler.RemoveActionEvent(GameFlowState.EndLevel, OnEndLevel);
    }

    private void BackButton()
    {
        SceneManager.LoadScene(SceneName.MainScene);
    }

    private void OnStartLevel(params object[] parrams)
    {
        if (parrams.Length <= 0)
            return;

        if (parrams[0] is LevelDataSO levelDataSO)
        {
#if LatteGames_GA
            m_TimePlayedLevel = 0;
            int levelID = m_LevelManagerSO.GetCurrentLevel();

            #region Progression Event
            GAProgressionStatus status = GAProgressionStatus.Start;
            m_LevelStart = levelID;
            string keyAttemptCount = KeyProgression.GetKeyAttemptMainLevel(levelID);
            int attemptSave = PlayerPrefs.GetInt(keyAttemptCount, 0);
            PlayerPrefs.SetInt(keyAttemptCount, attemptSave + 1);
            int attemptCount = PlayerPrefs.GetInt(keyAttemptCount, 0);
            GameEventHandler.Invoke(GSAnalyticProgressionEvent.MainLevel, status, levelID, attemptCount, 0);
            #endregion

            #region Design Event
            PlayerUnit.Instance.OnUnitDamageTaken += OnUnitDamageTaken;
            for (int i = 0; i < m_WeaponLibrarySO.WeaponManagerSOs.Count; i++)
            {
                var item = m_WeaponLibrarySO.WeaponManagerSOs[i].CurrentWeaponInUseVariable.value;
                if (item != null && item is WeaponSO weaponSO)
                    GameEventHandler.Invoke(GSAnalyticDesignEvent.InUsedItem, levelID, weaponSO.WeaponType.ToString(), weaponSO.GetDisplayName());
            }
            #endregion
#endif
        }
    }

    private void OnUnitDamageTaken(Unit arg1, float arg2, IAttackable arg3)
    {
#if LatteGames_GA
        if (arg3 is MonoBehaviour mono)
        {
            EnemyUnit enemyUnit = mono.GetComponent<EnemyUnit>();
            if (enemyUnit != null)
                m_LastDamageSource = enemyUnit.BotTargetingType.ToString();
        }
#endif
    }

    private void OnEndLevel(params object[] parrams)
    {
        if (parrams.Length <= 0)
            return;

        LevelDataSO levelDataSO = (LevelDataSO)parrams[0];
        bool isWin = (bool)parrams[1];
        m_TimePlayedSave = Mathf.FloorToInt(m_TimePlayedLevel);
        if (!isWin)
        {
            SoundManager.Instance.PlayLoopSFX(GSSFX.UIYouLose);
            m_ItemRewardCanvasGroup.Hide();
        }
        else
        {
            SoundManager.Instance.PlayLoopSFX(GSSFX.UIWinner);
            m_ItemRewardCanvasGroup.Show();
        }

        m_MainCanvasGroupVisibility.Show();
        m_TitleWin.SetActive(isWin);
        m_TitleLose.SetActive(!isWin);
        m_ReachedWaveI2.SetParameterValue("value", $"{m_LevelManagerSO.GetCurrentWave()}");
        CalcCoinReward(isWin);
        SetUpItemReward(levelDataSO);


        int levelID = m_LevelManagerSO.GetCurrentLevel();
        int attemptCount = PlayerPrefs.GetInt(KeyProgression.GetKeyAttemptMainLevel(levelID), 0);

        #region Progression Event
#if LatteGames_GA
        GAProgressionStatus status = isWin ? GAProgressionStatus.Complete : GAProgressionStatus.Fail;
        GameEventHandler.Invoke(GSAnalyticProgressionEvent.MainLevel, status, levelID, attemptCount, m_TimePlayedSave);
#endif
        #endregion

        #region Design Event
#if LatteGames_GA
        if (!isWin)
        {
            float currentWave = m_LevelManagerSO.GetCurrentWave();
            float totalWave = levelDataSO.Waves.Count;
            int score = (int)((currentWave / totalWave) * 100);
            GameEventHandler.Invoke(GSAnalyticDesignEvent.MainLevel_FailReason, levelID, attemptCount, m_LastDamageSource, score);
        }

        PlayerUnit.Instance.OnUnitDamageTaken -= OnUnitDamageTaken;
#endif
        #endregion
    }

    private void CalcCoinReward(bool isWin)
    {
        int coinReward = isWin ? 100 : 10;
        CurrencyManager.Instance.AcquireWithoutLogEvent(CurrencyType.Standard, coinReward);
        m_CoinRewardText.SetText(m_CoinRewardText.text.Replace("{value}", $"{coinReward}"));
    }

    private void SetUpItemReward(LevelDataSO levelDataSO)
    {
        if (m_RewardCardUI == null)
        {
            Debug.LogError("RewardCardUI is not assigned. Cannot set up item reward.");
            return;
        }

        WeaponSO reward = levelDataSO.Reward;
        if (reward == null || reward.IsUnlocked())
        {
            m_ItemRewardCanvasGroup.Hide();
            return;
        }

        ConfigureReward(reward);
        UpdateWeaponLibrary(reward);
    }

    private void ConfigureReward(WeaponSO reward)
    {
        reward.TryUnlockIgnoreRequirement();
        m_RewardCardUI.Setup(reward);
    }

    private void UpdateWeaponLibrary(WeaponSO reward)
    {
        foreach (var weaponManager in m_WeaponLibrarySO.WeaponManagerSOs)
        {
            int index = weaponManager.initialValue.IndexOf(reward);
            if (index != -1)
            {
                weaponManager.CurrentWeaponInUseVariable.value = reward;
                break;
            }
        }
    }
}
