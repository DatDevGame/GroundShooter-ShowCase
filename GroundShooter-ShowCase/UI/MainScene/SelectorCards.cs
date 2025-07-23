using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static WeaponDataStorage;

public class SelectorCards : MonoBehaviour
{
    [SerializeField, BoxGroup("Ref")] private Transform m_CardHolder;
    [SerializeField, BoxGroup("Ref")] private TMP_Text m_TitleName;
    [SerializeField, BoxGroup("Resource")] private ItemCardUI m_CardUIPrefab;
    [SerializeField, BoxGroup("Resource")] private WeaponManagerSO m_WeaponManagerSO;

    private ItemCardUI m_CurrentSelectCard;
    private List<ItemCardUI> m_CardUIs = new();
    private PreviewPlayerSpawner m_PreviewPlayerSpawner;
    private bool m_IsShow = false;

    private void Awake()
    {
        m_PreviewPlayerSpawner = FindAnyObjectByType<PreviewPlayerSpawner>();
    }

    private void Start()
    {
        CreateAllCards();
        UpdateSelectedCardUI();

        DisableSelector();
    }

    private void CreateAllCards()
    {
        foreach (var item in m_WeaponManagerSO.initialValue)
        {
            WeaponSO weaponSO = item.Cast<WeaponSO>();
            if (weaponSO == null) continue;

            var cardUI = Instantiate(m_CardUIPrefab, m_CardHolder);
            cardUI.Init(weaponSO);
            cardUI.OnCardClicked += OnCardClicked;

            m_CardUIs.Add(cardUI);

            if (m_WeaponManagerSO.CurrentWeaponInUseVariable.value == weaponSO)
            {
                m_CurrentSelectCard = cardUI;
                cardUI.Select();
            }
            else
            {
                cardUI.UnSelect();
            }
        }

        //Create Empty
        if (m_CardUIs.Count <= 1)
        {
            var cardUI = Instantiate(m_CardUIPrefab, m_CardHolder);
            WeaponSO weaponSOEmpty = new WeaponSO();
            weaponSOEmpty.name = "Empty";
            cardUI.Init(weaponSOEmpty);
            cardUI.UnSelect();
            m_CardUIs.Add(cardUI);
        }
    }

    private void OnCardClicked(ItemCardUI clickedCard)
    {
        if (m_CurrentSelectCard == clickedCard)
        {
            if (m_IsShow)
                DisableSelector();
            else
                EnableSelector();
            return;
        }


        m_CurrentSelectCard = clickedCard;
        foreach (var card in m_CardUIs)
        {
            if (card == clickedCard)
                card.Select();
            else
                card.UnSelect();
        }

        m_WeaponManagerSO.CurrentWeaponInUseVariable.value = clickedCard.WeaponSO;
        UpdateSelectedCardUI();
        m_PreviewPlayerSpawner?.LoadoutPreview();
    }

    private void UpdateSelectedCardUI()
    {
        if (m_CurrentSelectCard == null) return;

        m_CurrentSelectCard.transform.SetAsLastSibling();

        string title = m_CurrentSelectCard.WeaponSO.WeaponType.ToString();
        if (m_CurrentSelectCard.WeaponSO.WeaponType == WeaponType.ShoulderGadget)
            title = "Gadget";
        m_TitleName.SetText(title);
    }

    private void EnableSelector()
    {
        m_IsShow = true;
        m_CardUIs
            .Where(v => v != m_CurrentSelectCard).ToList()
            .ForEach(x => x.gameObject.SetActive(true));
    }
    private void DisableSelector()
    {
        m_IsShow = false;
        m_CardUIs
            .Where(v => v != m_CurrentSelectCard).ToList()
            .ForEach(x => x.gameObject.SetActive(false));
    }
}
