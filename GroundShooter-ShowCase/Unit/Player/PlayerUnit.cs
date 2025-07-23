using System;
using System.Collections.Generic;
using DG.Tweening;
using HyrphusQ.Events;
using LatteGames;
using LatteGames.Template;
using UnityEngine;
using UnityEngine.Animations.Rigging;

[DefaultExecutionOrder(-100)]
public class PlayerUnit : Unit
{
    public static event Action<PlayerUnit> OnDead = delegate { };
    public static event Action<PlayerUnit, float, IAttackable> OnDamageTaken = delegate { };

    protected static PlayerUnit instance;
    [SerializeField]
    protected ExpLevelProgressionSO playerLevelSO;
    [SerializeField]
    protected RigBuilder rigBuilder;
    [SerializeField]
    protected TwoBoneIKConstraint rightHandIKConstraint, leftHandIKConstraint;
    [SerializeField]
    protected ParticleSystem levelUpVFX;
    [SerializeField]
    protected ParticleSystem healthRegenVFX;

    private int currentModelIndex = 0;

    public static PlayerUnit Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<PlayerUnit>();
            return instance;
        }
    }
    public ExpLevelProgressionSO PlayerLevelSO => playerLevelSO;
    public TwoBoneIKConstraint RightHandIKConstraint => rightHandIKConstraint;
    public TwoBoneIKConstraint LeftHandIKConstraint => leftHandIKConstraint;

    protected override void Awake()
    {
        base.Awake();
        playerLevelSO.OnLevelChanged += OnLevelChanged;
        playerLevelSO.ResetLevel();
        if (instance == null)
            instance = this;
    }
    protected override void Start()
    {
        base.Start();
        Stats.GetStat(StatType.MaxHealth).OnValueChanged += OnMaxHealthStatChanged;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        playerLevelSO.OnLevelChanged -= OnLevelChanged;
        if (instance == this)
            instance = null;
        Stats.GetStat(StatType.MaxHealth).OnValueChanged -= OnMaxHealthStatChanged;
    }

    protected virtual void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.L))
        {
            playerLevelSO.AddExp(playerLevelSO.RemainingExpToNextLevel);
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            FindObjectOfType<PlayerHitReaction>().PlayHitReaction();
        }
#endif
    }

    protected virtual void OnLevelChanged(ValueDataChanged<int> eventData)
    {
        if (eventData.newValue > eventData.oldValue)
        {
            StartCoroutine(CommonCoroutine.Delay(0.1f, false, () => levelUpVFX.Play()));
        }
    }

    protected virtual void OnMaxHealthStatChanged(ValueDataChanged<float> eventData)
    {
        Health.MaxHealth = eventData.newValue;
        Health.CurrentHealth = Mathf.Clamp(Health.CurrentHealth + eventData.newValue - eventData.oldValue, 0f, Health.MaxHealth);
    }

    protected override void NotifyEventDead()
    {
        OnDead.Invoke(this);
        base.NotifyEventDead();
    }

    protected override void NotifyEventDamageTaken(float damage, IAttackable damageSource)
    {
        OnDamageTaken(this, damage, damageSource);
        base.NotifyEventDamageTaken(damage, damageSource);
    }

    public override bool IsPlayer()
    {
        return true;
    }

    public void SetupIKConstraints(Transform rightHandTarget, Transform rightHandHint, Transform leftHandTarget, Transform leftHandHint)
    {
        rightHandIKConstraint.data.target = rightHandTarget;
        rightHandIKConstraint.data.hint = rightHandHint;
        leftHandIKConstraint.data.target = leftHandTarget;
        leftHandIKConstraint.data.hint = leftHandHint;
        rigBuilder.Build();
    }

    public void Heal(float amount)
    {
        if (Health.CurrentHealth >= Health.MaxHealth)
            return;
        Health.CurrentHealth = Mathf.Clamp(Health.CurrentHealth + amount, 0f, Health.MaxHealth);
        healthRegenVFX.Play();
    }

    public void UpgradeWeapon(List<WoddenBox.UpgradeGun> upgradeGuns)
    {
        foreach (var upgrade in upgradeGuns)
        {
            Weapon weapon = WeaponManager.Instance.GetWeapon(upgrade.weaponType);
            weapon.IncreaseStarLevel(upgrade.level);
            if (upgrade.weaponType == WeaponType.MainGun)
            {
                var assaultGun = weapon as AssaultGun;
                if (currentModelIndex < assaultGun.models.Length - 1)
                {
                    assaultGun.models[currentModelIndex].gameObject.SetActive(false);
                    assaultGun.models[currentModelIndex + 1].gameObject.SetActive(true);
                    currentModelIndex++;
                }
            }
        }
        levelUpVFX.Play();
        transform.DOPunchScale(Vector3.one * 0.2f, 0.25f, 5, 3f);
    }
}