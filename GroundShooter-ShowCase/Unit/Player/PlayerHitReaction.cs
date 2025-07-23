using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core.Easing;
using LatteGames.Template;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHitReaction : MonoBehaviour
{
    private static readonly int OutsideColor = Shader.PropertyToID("_OutsideColor");

    [SerializeField]
    Image flashScreenImage;

    Color originalColor;

    private void Start()
    {
        originalColor = flashScreenImage.material.GetColor(OutsideColor);
        PlayerUnit.OnDamageTaken += OnPlayerDamageTaken;
    }

    private void OnDestroy()
    {
        flashScreenImage.material.SetColor(OutsideColor, originalColor);
        PlayerUnit.OnDamageTaken -= OnPlayerDamageTaken;
    }

    private void OnPlayerDamageTaken(PlayerUnit unit, float arg2, IAttackable attackable)
    {
        if (unit.IsDead() || DOTween.IsTweening(flashScreenImage.material))
            return;
        PlayHitReaction();
    }

    private void FlashScreen(float duration = 0.2f)
    {
        Color color = originalColor;
        color.a = 0f;
        flashScreenImage.gameObject.SetActive(true);
        flashScreenImage.material.SetColor(OutsideColor, color);
        flashScreenImage.material.DOColor(originalColor, OutsideColor, duration).SetLoops(2, LoopType.Yoyo).OnComplete(() =>
        {
            flashScreenImage.gameObject.SetActive(false);
        });
    }

    public void PlayHitReaction()
    {
        CameraShake.Instance.Shake(0.25f, 0.1f);
        HapticManager.Instance.PlayFlashHaptic(HapticTypes.LightImpact);
        SoundManager.Instance.PlaySFX(SFX.HitMale);
        FlashScreen(0.2f);
    }
}