using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using HyrphusQ.SerializedDataStructure;
using Unity.VisualScripting;
using LatteGames;

public class BattleSkillCardUI : MonoBehaviour
{
    public event Action<BattleSkillCardUI> OnCardClicked = delegate { };
    public BattleSkillSO BattleSkillSO => battleSkillSO;
    public bool IsSelected
    {
        get => m_IsSelected;
        set
        {
            m_IsSelected = value;
            m_SelectedPanel.SetActive(m_IsSelected);
            m_SelectedIconBoard.SetActive(m_IsSelected);

            if(!IsSelected)
                MakeLastStarFaded();
            else
                MakeLastStarUnFaded();
        }
    }

    [SerializeField, BoxGroup("Ref")] private Image iconImage, m_InRarityImage, m_OutRarityImage, m_BoardCard;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI stackText;
    [SerializeField] private Button button;

    [SerializeField, BoxGroup("Ref")] private GameObject m_SelectedPanel;
    [SerializeField, BoxGroup("Ref")] private GameObject m_SelectedIconBoard;
    [SerializeField, BoxGroup("Ref")] private GameObject m_UltimateBlurBoard;
    [SerializeField, BoxGroup("Ref")] private EZAnimVector3 m_SelectAnim;
    [SerializeField, BoxGroup("Ref")] private List<GameObject> m_Stars;
    [SerializeField, BoxGroup("Ref")] private SerializedDictionary<TypeBattleCard, RarityColorBattleCard> m_Raritys;

    private int m_Stack = 0;
    private bool m_IsSelected = false;
    private BattleSkillSO battleSkillSO;

    private void Awake()
    {
        button.onClick.AddListener(OnCardButtonClicked);
    }

    private void OnCardButtonClicked()
    {
        m_SelectAnim.Play();
        OnCardClicked?.Invoke(this);
        IsSelected = true;
    }

    private void SetStar(TypeBattleCard typeBattleCard, int stack)
    {
        m_Stack = stack;
        var lastStar = m_Stars.Last();
        if (typeBattleCard == TypeBattleCard.Ultimate)
        {
            m_Stack = stack + 1;
            m_Stars.ForEach(star => star.gameObject.SetActive(false));
            EnableStarIcon(lastStar, true);
            lastStar.gameObject.SetActive(true);
            MakeLastStarFaded();
            return;
        }

        MakeLastStarFaded();
        m_Stars.ForEach(star =>
        {
            star.gameObject.SetActive(true);
            EnableStarIcon(star, false);
        });
        lastStar.gameObject.SetActive(false);
        m_Stars.Take(stack).ToList()
                .ForEach(star => EnableStarIcon(star, true));
    }

    private void EnableStarIcon(GameObject star, bool isActive)
    {
        star.transform.GetChild(0).gameObject.SetActive(isActive);
    }

    private void SetLastStarAlpha(float alpha)
    {
        var stars = m_Stars.Take(m_Stack).ToList();
        for (int i = 0; i < stars.Count; i++)
        {
            var image = stars[i].transform.GetChild(0).GetComponent<Image>();
            if (image == null) continue;

            Color color = image.color;
            color.a = (i == stars.Count - 1) ? Mathf.Clamp01(alpha) : 1f;
            image.color = color;
        }
    }


    private void MakeLastStarFaded()
    {
        SetLastStarAlpha(150f / 255f);
    }

    private void MakeLastStarUnFaded()
    {
        SetLastStarAlpha(1f);
    }


    public void Setup(BattleSkillSO battleSkillSO, int stack)
    {
        this.battleSkillSO = battleSkillSO;
        iconImage.sprite = this.battleSkillSO.Icon;
        nameText.text = this.battleSkillSO.Title;
        stackText.text = $"x{stack}";


        int nextStack = stack + 1;
        TypeBattleCard typeBattleCard = battleSkillSO is StatModifierBattleSkillSO ? TypeBattleCard.Booster : TypeBattleCard.Weapon;
        if (nextStack == battleSkillSO.MaxStack)
            typeBattleCard = TypeBattleCard.Ultimate;

        SetStar(typeBattleCard, nextStack);
        m_UltimateBlurBoard.gameObject.SetActive(typeBattleCard == TypeBattleCard.Ultimate);
        m_InRarityImage.color = m_Raritys[typeBattleCard].In;
        m_OutRarityImage.color = m_Raritys[typeBattleCard].Out;
        m_BoardCard.sprite = m_Raritys[typeBattleCard].BoardCard;
    }

    public BattleSkillSO GetBattleSkillSO() => battleSkillSO;
    public int GetStarId()
    {
        return m_Stack >= GetBattleSkillSO().MaxStack ? GetBattleSkillSO().MaxStack : m_Stack;
    }
}

[Serializable]
public class RarityColorBattleCard
{
    public Color In;
    public Color Out;
    public Sprite BoardCard;
}

public enum TypeBattleCard
{
    Weapon,
    Booster,
    Ultimate
}
