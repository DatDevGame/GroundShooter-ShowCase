using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using Unity.VisualScripting;

public class RewardCardUI : MonoBehaviour
{
    [SerializeField, BoxGroup("Ref")] private TMP_Text m_TitleText;
    [SerializeField, BoxGroup("Ref")] private TMP_Text m_NameText;
    [SerializeField, BoxGroup("Ref")] private Image m_IconImage;

    public void Setup(WeaponSO weaponSO)
    {
        m_TitleText.text = weaponSO.GetDisplayName();
        m_NameText.text = weaponSO.WeaponType.ToString();
        m_IconImage.sprite = weaponSO.WeaponBattleSkillStages[0].Icon;
    }
}
