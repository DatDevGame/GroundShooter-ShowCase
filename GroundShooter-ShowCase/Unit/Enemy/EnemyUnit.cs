using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using LatteGames.PoolManagement;
using UnityEngine;
using DG.Tweening;
using LatteGames.Template;
using Random = UnityEngine.Random;
using System.Linq;

public class EnemyUnit : Unit
{
    private readonly static int BaseColorID = Shader.PropertyToID("_BaseColor");
    private readonly static Color BeingHitColor = new Color(1f, 0.4009434f, 0.4009434f, 1f);
    public static event Action<EnemyUnit> OnDead = delegate { };
    public static event Action<EnemyUnit, float, IAttackable> OnDamageTaken = delegate { };
    public static readonly List<EnemyUnit> ActiveEnemies = new List<EnemyUnit>();

    public bool playAnimWhenDeath;
    public Material deathMat;
    [SerializeField]
    protected float explosionVolumeOnDead = 0.5f;
    [SerializeField]
    protected HapticTypes deathHapticType = HapticTypes.MediumImpact;
    [SerializeField]
    protected ParticleSystem explosionVFXPrefab;

#if LatteGames_GA
    public BotTargetingType BotTargetingType => m_BotTargetingType;
    [SerializeField, BoxGroup("Game Analytics")] protected BotTargetingType m_BotTargetingType;
#endif

    protected Renderer[] renderers;

    protected override void Start()
    {
        base.Start();
        ActiveEnemies.Add(this);
        renderers = transform.GetChild(0).GetComponentsInChildren<Renderer>().Where(renderer => renderer is not ParticleSystemRenderer).ToArray();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        ActiveEnemies.Remove(this);
    }

    protected override void NotifyEventDead()
    {
        ActiveEnemies.Remove(this);
        OnDead.Invoke(this);
        base.NotifyEventDead();

        if (!playAnimWhenDeath)
        {
            var explosionVFX = PoolManager.GetOrCreatePool(explosionVFXPrefab).Get();
            explosionVFX.transform.position = transform.position + Vector3.up;
            explosionVFX.Play();
            explosionVFX.Release(explosionVFXPrefab, explosionVFX.main.duration);
        }

        HapticManager.Instance.PlayFlashHaptic(deathHapticType);
        SoundManager.Instance.PlaySFX(SFX.Explosion, explosionVolumeOnDead);
    }

    protected override void NotifyEventDamageTaken(float damage, IAttackable damageSource)
    {
        OnDamageTaken(this, damage, damageSource);
        base.NotifyEventDamageTaken(damage, damageSource);
        if (!IsDead())
        {
            foreach (var renderer in renderers)
            {
                if (!DOTween.IsTweening(renderer.material))
                    renderer.material.DOColor(BeingHitColor, BaseColorID, 0.05f).SetLoops(2, LoopType.Yoyo);
            }
        }
    }

    public override void Die()
    {
        GetComponent<Collider>().enabled = false;
        NotifyEventDead();
        if (playAnimWhenDeath)
        {
            GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial = deathMat;
            transform.DOMoveY(transform.position.y + Random.Range(1f, 3f), 0.5f);
            transform.DORotate(transform.eulerAngles + Vector3.right * Random.Range(-75f, -110f), 0.5f);
            transform.DOMoveY(transform.position.y - 1f, 0.5f).SetDelay(0.5f);
            transform.DOScale(Vector3.zero, 0.25f).SetDelay(0.4f).OnComplete(() => { gameObject.SetActive(false); });
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}