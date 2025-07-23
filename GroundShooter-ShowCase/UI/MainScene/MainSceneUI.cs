using LatteGames.GameManagement;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainSceneUI : MonoBehaviour
{
    [SerializeField, BoxGroup("Ref")] private MultiImageButton m_PlayButton;
    [SerializeField, BoxGroup("Ref")] private TMP_Text m_PlayLevelText;
    [SerializeField, BoxGroup("Ref")] private LevelManagerSO m_LevelManagerSO;

    private void Awake()
    {
        m_PlayButton.onClick.AddListener(PlayButton);
        m_PlayLevelText.SetText(m_PlayLevelText.text.Replace("{value}", $"{m_LevelManagerSO.GetCurrentLevel()}"));
    }

    private void OnDestroy()
    {
        m_PlayButton.onClick.RemoveListener(PlayButton);
    }

    private void PlayButton()
    {
        SceneManager.LoadScene(SceneName.PlayScene);
    }
}
