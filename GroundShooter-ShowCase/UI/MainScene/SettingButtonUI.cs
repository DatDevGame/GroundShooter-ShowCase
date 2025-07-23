using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingButtonUI : MonoBehaviour
{
    [SerializeField, BoxGroup("Ref")] private Button m_SettingBtn;

    private SettingUI m_SettingUI;
    private void Awake()
    {
        m_SettingBtn.onClick.AddListener(SettingBtnHandle);
    }

    private void Start()
    {
        m_SettingUI = FindAnyObjectByType<SettingUI>();
    }

    private void OnDestroy()
    {
        m_SettingBtn.onClick.RemoveListener(SettingBtnHandle);
    }

    private void SettingBtnHandle()
    {
        if (m_SettingUI != null)
            m_SettingUI.Show();
    }
}
