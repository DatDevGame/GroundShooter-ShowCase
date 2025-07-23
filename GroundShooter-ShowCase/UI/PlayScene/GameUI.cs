using Cinemachine;
using DG.Tweening;
using HyrphusQ.Events;
using I2.Loc;
using LatteGames;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [SerializeField, BoxGroup("Ref")] private TMP_Text m_WaveText;
    [SerializeField, BoxGroup("Ref")] private TMP_Text m_KillText;
    [SerializeField, BoxGroup("Ref")] private TMP_Text m_CoinText;
    [SerializeField, BoxGroup("Ref")] private LocalizationParamsManager m_NotiveWaveText;
    [SerializeField, BoxGroup("Ref")] private Button m_SettingBtn;
    [SerializeField, BoxGroup("Ref")] private Button m_SwitchCamera;
    [SerializeField, BoxGroup("Ref")] private SettingUI m_SettingUI;
    [SerializeField, BoxGroup("Ref")] private EZAnimSequence m_NoticeWaveEZAnimSequence;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility m_MainCanvasGroup;
    [SerializeField, BoxGroup("Camera Configs")] private float m_TopDownAngle = 32.5f;
    [SerializeField, BoxGroup("Camera Configs")] private float m_RunnerAngle = 25;


    private int m_WaveCount = -1;
    private int m_KillCount = -1;
    private int m_CoinCount = -1;

    private int m_MaxWave = -1;
    private int m_MaxKill = -1;

    private CinemachineVirtualCamera m_CinemachineVirtualCamera;
    private bool m_SwitchCam = false;

    public int WaveCount
    {
        get => m_WaveCount;
        set
        {
            if (m_WaveCount != value)
            {
                m_WaveCount = value;
                m_WaveText.SetText($"{m_WaveCount.ToString("D2")}/{m_MaxWave.ToString("D2")}");
            }
        }
    }

    public int KillCount
    {
        get => m_KillCount;
        set
        {
            if (m_KillCount != value)
            {
                m_KillCount = value;
                m_KillText.SetText($"{m_KillCount.ToString("D2")}/{m_MaxKill.ToString("D2")}");
            }
        }
    }

    public int CoinCount
    {
        get => m_CoinCount;
        set
        {
            if (m_CoinCount != value)
            {
                m_CoinCount = value;
                m_CoinText.SetText(m_CoinCount.ToRoundedText());
            }
        }
    }

    private void Awake()
    {
        GameEventHandler.AddActionEvent(GameFlowState.StartLevel, OnStartLevel);
        GameEventHandler.AddActionEvent(GameFlowState.EndLevel, OnEndLevel);
        GameEventHandler.AddActionEvent(WaveStats.Spawning, OnWaveSpawning);
        GameEventHandler.AddActionEvent(WaveStats.OnKillEnemy, OnKillEnemy);
        m_SettingBtn.onClick.AddListener(SettingButton);
        m_SwitchCamera.onClick.AddListener(OnSwitchCamera);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(GameFlowState.StartLevel, OnStartLevel);
        GameEventHandler.RemoveActionEvent(GameFlowState.EndLevel, OnEndLevel);
        GameEventHandler.RemoveActionEvent(WaveStats.Spawning, OnWaveSpawning);
        GameEventHandler.RemoveActionEvent(WaveStats.OnKillEnemy, OnKillEnemy);
        m_SettingBtn.onClick.RemoveListener(SettingButton);
        m_SwitchCamera.onClick.RemoveListener(OnSwitchCamera);
    }

    private void OnStartLevel(params object[] parrams)
    {
        m_NoticeWaveEZAnimSequence.SetToEnd();
        m_WaveText.SetText(m_WaveCount.ToString("D2"));
        m_KillText.SetText(m_KillCount.ToString("D2"));
        m_CoinText.SetText(m_CoinCount.ToRoundedText());
    }    
    private void OnEndLevel(params object[] parrams)
    {
        m_MainCanvasGroup.Hide();
    }

    private void OnWaveSpawning(params object[] parrams)
    {
        if (parrams.Length <= 0)
            return;

        if (parrams[0] is int currentWave && parrams[1] is int maxWave)
        {
            m_MaxWave = maxWave;
            WaveCount = currentWave;

            if (currentWave > 1)
            {
                m_NotiveWaveText.SetParameterValue("value", $"{currentWave - 1}");
                m_NoticeWaveEZAnimSequence.Play();
            }
        }
    }

    private void OnKillEnemy(params object[] parrams)
    {
        if (parrams.Length <= 0)
            return;

        if (parrams[0] is int currentKillWave && parrams[1] is int maxKill)
        {
            m_MaxKill = maxKill;
            KillCount = currentKillWave;
        }
    }
    private void SettingButton()
    {
        m_SettingUI.Show();
    }

    private void OnSwitchCamera()
    {
        if (m_CinemachineVirtualCamera == null)
            m_CinemachineVirtualCamera = Camera.main.transform.parent.GetComponentInChildren<CinemachineVirtualCamera>();

        if (m_CinemachineVirtualCamera != null)
        {
            Vector3 targetEuler = m_SwitchCam ? new Vector3(m_TopDownAngle, 0, 0) : new Vector3(m_RunnerAngle, 0, 0);
            m_CinemachineVirtualCamera.transform.DOKill();
            m_CinemachineVirtualCamera.transform
                .DORotate(targetEuler, 0.5f)
                .SetEase(Ease.OutSine);

            m_SwitchCam = !m_SwitchCam;
        }
    }
}
