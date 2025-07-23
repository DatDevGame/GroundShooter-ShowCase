using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using TMPro;
using static WeaponDataStorage;
using LatteGames.Template;

public class ItemCardUI : MonoBehaviour, IPointerClickHandler
{
    public event Action<ItemCardUI> OnCardClicked;
    public WeaponSO WeaponSO => m_WeaponSO;

    [SerializeField, BoxGroup("Ref")] private Image m_BackGroundAvatar;
    [SerializeField, BoxGroup("Ref")] private Image m_Avatar;
    [SerializeField, BoxGroup("Ref")] private TMP_Text m_NameText;
    [SerializeField, BoxGroup("Ref")] private GameObject m_UnlockPanel;
    [SerializeField, BoxGroup("Ref")] private GameObject m_EmptyIcon;

    [SerializeField, BoxGroup("Resource")] private Color m_Selected;
    [SerializeField, BoxGroup("Resource")] private Color m_UnSelect;

    private WeaponSO m_WeaponSO;

    public void Init(WeaponSO weaponSO)
    {
        m_WeaponSO = weaponSO;

        //Empty Handle
        if (IsWeaponEmpty())
        {
            m_NameText.SetText(weaponSO.name);
            m_EmptyIcon.SetActive(true);
            m_Avatar.gameObject.SetActive(false);
            return;   
        }

        m_Avatar.sprite = weaponSO.WeaponBattleSkillStages[0].Icon;
        m_NameText.SetText(weaponSO.GetDisplayName());
        m_UnlockPanel.SetActive(!m_WeaponSO.IsUnlocked());
    }

    public void Select()
    {
        m_BackGroundAvatar.color = m_Selected;
        OnCardClicked?.Invoke(this);
        SoundManager.Instance.PlaySFX(GeneralSFX.UITapButton);
    }
    public void UnSelect()
    {
        m_BackGroundAvatar.color = m_UnSelect;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsWeaponEmpty())
            return;

        if (m_WeaponSO.IsUnlocked())
            Select();
    }

    private bool IsWeaponEmpty()
    {
        if (m_WeaponSO == null)
            return false;
        return m_WeaponSO.name == "Empty";
    }
}
