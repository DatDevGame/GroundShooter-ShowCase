using System.Collections.Generic;
using UnityEngine;

public abstract class SkillStatBase : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private bool m_IsReverse;
    [SerializeField] private List<SkillStatSlotUI> m_SkillStatSlotUIs;

    public void LoadStats(List<SkillStatData> dataList)
    {
        m_SkillStatSlotUIs.ForEach(v => v.Disable());

        int count = Mathf.Min(m_SkillStatSlotUIs.Count, dataList.Count);

        for (int i = 0; i < count; i++)
        {
            int index = m_IsReverse ? m_SkillStatSlotUIs.Count - 1 - i : i;
            m_SkillStatSlotUIs[index].Setup(dataList[i]);
            m_SkillStatSlotUIs[index].Enable();
        }
    }
}
