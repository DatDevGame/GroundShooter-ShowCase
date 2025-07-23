using LatteGames;
using LatteGames.GameManagement;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingUI : MonoBehaviour
{
    [SerializeField, BoxGroup("Ref")] private Button m_QuitButton;
    [SerializeField, BoxGroup("Ref")] private Button m_BackButton;
    [SerializeField, BoxGroup("Ref")] private Button m_CloseButton;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility m_CanvasGroupVisibility;

    private void Awake()
    {
        m_QuitButton.onClick.AddListener(QuitButton);
        m_BackButton.onClick.AddListener(BackButton);
        m_CloseButton.onClick.AddListener(OnCloseButton);

        m_CanvasGroupVisibility.GetOnEndShowEvent().Subscribe(OnEndShow);
    }

    private void OnDestroy()
    {
        m_QuitButton.onClick.RemoveListener(QuitButton);
        m_BackButton.onClick.RemoveListener(BackButton);
        m_CloseButton.onClick.RemoveListener(OnCloseButton);

        m_CanvasGroupVisibility.GetOnEndShowEvent().Unsubscribe(OnEndShow);
    }

    public void Show()
    {
        m_CanvasGroupVisibility.Show();
    }

    public void Hide()
    {
        TimeScaleHandler.Instance.Resume();
        m_CanvasGroupVisibility.Hide();
    }

    private void OnEndShow() => TimeScaleHandler.Instance.Pause();
    private void QuitButton()
    {
        SceneManager.LoadScene(SceneName.MainScene);
    }

    private void BackButton()
    {
        Hide();
    }
    private void OnCloseButton()
    {
        Hide();
    }
}
