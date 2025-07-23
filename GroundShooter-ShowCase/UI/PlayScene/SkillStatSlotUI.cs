using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SkillStatSlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image m_Icon;

    public void Setup(SkillStatData data)
    {
        if (data == null)
            return;

        if (m_Icon != null)
            m_Icon.sprite = data.Icon;
    }

    public void Enable()
    {
        m_Icon.enabled = true;
    }
    public void Disable()
    {
        m_Icon.enabled = false;
    }
}
