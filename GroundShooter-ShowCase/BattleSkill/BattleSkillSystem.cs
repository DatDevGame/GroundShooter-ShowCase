using DG.Tweening;
using HyrphusQ.Events;
using I2.Loc;
using LatteGames;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static WeaponDataStorage;
using Random = UnityEngine.Random;

public class BattleSkillSystem : Singleton<BattleSkillSystem>
{
    [Serializable]
    public class ScriptedBattleSkill
    {
        public List<BattleSkillSO> scriptedBattleSkillSOs;
    }

    [SerializeField]
    private ExpLevelProgressionSO playerLevelSO;
    [SerializeField]
    private LevelManagerSO levelManagerSO;
    [SerializeField]
    private List<BattleSkillSO> battleSkillSOs;
    [SerializeField]
    private List<BattleSkillCardUI> battleSkillCardUIs;
    [SerializeField]
    private CanvasGroupVisibility battleSkillUIVisibility;
    [SerializeField, BoxGroup("Ref")] private Button m_ActiveCardButton;
    [SerializeField, BoxGroup("Ref")] private Transform m_SkillPanels;
    [SerializeField, BoxGroup("Ref")] private WeaponStatUI m_WeaponStatUI;
    [SerializeField, BoxGroup("Ref")] private BoosterStatUI m_BoosterStatUI;
    [SerializeField, BoxGroup("Ref")] private TMP_Text m_DescriptionText;
    [SerializeField, BoxGroup("Ref")] private LocalizationParamsManager m_ThisLevelTextI2;

    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility m_NoticeSkillCanvasGroup;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility m_DescriptionCanvasGroup;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility m_ComfirmButtonCanvasGroup;

    [SerializeField, BoxGroup("Data")] private LevelManagerSO m_LevelManagerSO;

    private PlayerBattleSkillController playerBattleSkillController;
    private BattleSkillCardUI SelectBattleSkillCard;
    private int scriptedBattleSkillIndex;
    private List<ScriptedBattleSkill> scriptedBattleSkills;

    protected override void Awake()
    {
        base.Awake();
        playerBattleSkillController = FindObjectOfType<PlayerBattleSkillController>();
        battleSkillCardUIs = GetComponentsInChildren<BattleSkillCardUI>().ToList();
        playerLevelSO.OnLevelChanged += OnLevelChanged;
        foreach (var battleSkillCardUI in battleSkillCardUIs)
        {
            battleSkillCardUI.OnCardClicked += OnCardClicked;
        }

        GameEventHandler.AddActionEvent(GameFlowState.EndLevel, OnEndLevel);
        m_ActiveCardButton.onClick.AddListener(OnActiveCard);
        battleSkillUIVisibility.GetOnStartShowEvent().Subscribe(OnStartShow);
        battleSkillUIVisibility.GetOnEndHideEvent().Subscribe(OnEndHide);
    }

    private void Start()
    {
        OnEndHide();
        scriptedBattleSkills = levelManagerSO.GetCurrentLevelData().scriptedBattleSkills;
    }

    private void OnDestroy()
    {
        playerLevelSO.OnLevelChanged -= OnLevelChanged;
        GameEventHandler.RemoveActionEvent(GameFlowState.EndLevel, OnEndLevel);
        m_ActiveCardButton.onClick.RemoveListener(OnActiveCard);
    }

    private void OnStartShow()
    {
        gameObject.SetActive(true);
    }

    private void OnEndHide()
    {
        gameObject.SetActive(false);
    }

    private void OnEndLevel(params object[] parrams)
    {
        if (parrams.Length <= 0)
            return;

        if (parrams[0] is LevelDataSO levelDataSO)
        {
            TimeScaleHandler.Instance.Resume();
            battleSkillUIVisibility.Hide();
        }
    }

    private void OnLevelChanged(ValueDataChanged<int> eventData)
    {
        if (eventData.newValue > eventData.oldValue)
        {
            // LoadStatUI();
            // RollRandomBattleSkills();
            m_ThisLevelTextI2.SetParameterValue("value", $"{eventData.newValue}");
        }
    }

    private void OnCardClicked(BattleSkillCardUI cardUI)
    {
        battleSkillCardUIs
            .Where(v => v != cardUI)
            .ForEach(v => v.IsSelected = false);
        SelectBattleSkillCard = cardUI;
        m_DescriptionText.SetText($"{cardUI.BattleSkillSO.Description}");

        m_NoticeSkillCanvasGroup.Hide();
        m_DescriptionCanvasGroup.Show();
        m_ComfirmButtonCanvasGroup.Show();
    }
    private bool m_CanActiveCard = true;
    private void OnActiveCard()
    {
        if (!m_CanActiveCard)
            return;
        m_CanActiveCard = false;
        m_NoticeSkillCanvasGroup.Hide();
        m_ComfirmButtonCanvasGroup.Hide();
        if (!battleSkillCardUIs.Any(v => v.IsSelected))
        {
            if (m_SkillPanels != null)
            {
                m_SkillPanels
                    .DOPunchPosition(
                        punch: new Vector3(50, 0f, 0f),
                        duration: 0.5f,
                        vibrato: 20,
                        elasticity: 1f
                    )
                    .SetUpdate(true)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => { m_CanActiveCard = true; });
            }
            return;
        }


        SelectBattleSkillCard.IsSelected = false;
        TimeScaleHandler.Instance.Resume();
        var battleSkillSO = SelectBattleSkillCard.GetBattleSkillSO();
        if (battleSkillSO != null)
        {
            #region Design Event
#if LatteGames_GA
            int levelID = m_LevelManagerSO.GetCurrentLevel();
            string skillType = battleSkillSO is StatModifierBattleSkillSO ? "EnhancementSkill" : "EquipmentSkill";
            string skillName = battleSkillSO.Title;
            int starID = SelectBattleSkillCard.GetStarId();
            GameEventHandler.Invoke(GSAnalyticDesignEvent.UpgradedBattleSkill, levelID, skillType, skillName, starID);
#endif
            #endregion

            playerBattleSkillController.AddBattleSkill(battleSkillSO);
            battleSkillUIVisibility.Hide();
            m_CanActiveCard = true;
        }
    }
    private void LoadStatUI()
    {
        var battleSkillSOs = playerBattleSkillController.ActiveBattleSkillDictionary
            .Select(x => x.Key)
            .ToList();

        var skillStatDatas = GetSkillData(battleSkillSOs);

        var weaponStats = skillStatDatas
            .Where(v => v.TypeBattleCard is TypeBattleCard.Weapon or TypeBattleCard.Ultimate)
            .ToList();

        var boosterStats = skillStatDatas
            .Where(v => v.TypeBattleCard is TypeBattleCard.Booster or TypeBattleCard.Ultimate)
            .ToList();

        m_WeaponStatUI.LoadStats(weaponStats);
        m_BoosterStatUI.LoadStats(boosterStats);

        m_NoticeSkillCanvasGroup.Show();
        m_DescriptionCanvasGroup.Hide();
    }

    private List<SkillStatData> GetSkillData(List<BattleSkillSO> battleSkillSOs)
    {
        var skillStatDataList = new List<SkillStatData>();
        foreach (var skill in battleSkillSOs)
        {
            var icon = skill switch
            {
                StatModifierBattleSkillSO s => s.Icon,
                WeaponBattleSkillSO w => w.GetLastIcon(),
                _ => null
            };
            var type = skill is StatModifierBattleSkillSO ? TypeBattleCard.Booster : TypeBattleCard.Weapon;

            var data = new SkillStatData(
                type,
                skill.GetInstanceID(),
                skill.Title,
                icon
            );

            skillStatDataList.Add(data);
        }

        return skillStatDataList;
    }


    private void RollRandomBattleSkills()
    {
        TimeScaleHandler.Instance.Pause();
        if (battleSkillSOs == null || battleSkillSOs.Count == 0)
            return;

        List<BattleSkillSO> randomBattleSkillSOs = scriptedBattleSkillIndex < scriptedBattleSkills.Count
        ? scriptedBattleSkills[scriptedBattleSkillIndex++].scriptedBattleSkillSOs
        : battleSkillSOs
            .Where(battleSkillSO =>
            {
                int stack = playerBattleSkillController.GetBattleSkillStack(battleSkillSO);
                if (stack >= battleSkillSO.MaxStack)
                    return false;
                return true;
            })
            .OrderBy(x => Random.value)
            .Take(battleSkillCardUIs.Count)
            .ToList();
        for (var i = 0; i < battleSkillCardUIs.Count; i++)
        {
            if (i >= randomBattleSkillSOs.Count)
            {
                var cardUI = battleSkillCardUIs[i];
                cardUI.gameObject.SetActive(false);
            }
            else
            {
                var battleSkillSO = randomBattleSkillSOs[i];
                var cardUI = battleSkillCardUIs[i];
                cardUI.gameObject.SetActive(true);
                cardUI.Setup(battleSkillSO, playerBattleSkillController.GetBattleSkillStack(battleSkillSO));
            }
        }

        battleSkillUIVisibility.Show();
    }
}
