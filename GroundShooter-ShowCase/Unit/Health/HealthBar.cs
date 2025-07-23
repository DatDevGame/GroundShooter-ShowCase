using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using HyrphusQ.GUI;
using TMPro;
using UnityEngine;

public class HealthBar : ProgressBar
{
    private readonly static int HealthID = Shader.PropertyToID("_Health");

    [SerializeField]
    private MeshRenderer healthBarRenderer;
    [SerializeField]
    private TextMeshProUGUI healthText;
    private MaterialPropertyBlock materialPropertyBlock;

    private MaterialPropertyBlock MaterialPropertyBlock
    {
        get
        {
            if (materialPropertyBlock == null)
                materialPropertyBlock = new MaterialPropertyBlock();
            return materialPropertyBlock;
        }
    }

    void Update()
    {
        transform.rotation = Quaternion.LookRotation(Camera.main.transform.position - transform.position);
    }

    protected override void OnValueChanged(ValueDataChanged<int> data)
    {
        var duration = m_Config.animationDuration;
        if (m_Config.modifiedAnimationDuration)
        {
            float inverseLerpValue = m_Config.inverseAnimationDuration ? (1f - m_MinMaxIntProgress.inverseLerpValue) : m_MinMaxIntProgress.inverseLerpValue;
            duration *= inverseLerpValue;
        }
        var oldValue = m_Config.inverseValue ? (m_MinMaxIntProgress.maxValue - m_MinMaxIntProgress.CalcInterpolatedValue(materialPropertyBlock.GetFloat(HealthID))) : m_MinMaxIntProgress.CalcInterpolatedValue(materialPropertyBlock.GetFloat(HealthID));
        var newValue = m_Config.inverseValue ? (m_MinMaxIntProgress.maxValue - data.newValue) : data.newValue;
        SetValue(oldValue, newValue, duration);
    }

    protected override void OnValueChanged(ValueDataChanged<float> data)
    {
        var duration = m_Config.animationDuration;
        if (m_Config.modifiedAnimationDuration)
        {
            float inverseLerpValue = m_Config.inverseAnimationDuration ? (1f - m_MinMaxFloatProgress.inverseLerpValue) : m_MinMaxFloatProgress.inverseLerpValue;
            duration *= inverseLerpValue;
        }
        var oldValue = m_Config.inverseValue ? (m_MinMaxFloatProgress.maxValue - m_MinMaxFloatProgress.CalcInterpolatedValue(materialPropertyBlock.GetFloat(HealthID))) : m_MinMaxFloatProgress.CalcInterpolatedValue(materialPropertyBlock.GetFloat(HealthID));
        var newValue = m_Config.inverseValue ? (m_MinMaxFloatProgress.maxValue - data.newValue) : data.newValue;
        SetValue(oldValue, newValue, duration);
    }

    public override void SetValue(int oldValue, int value, float animationDuration)
    {
        if (m_LerpCoroutine != null)
        {
            StopCoroutine(m_LerpCoroutine);
        }
        m_LerpCoroutine = StartCoroutine(LerpFactor(animationDuration, t =>
        {
            SetValueImmediately((int)Mathf.Lerp(oldValue, value, t));
        }));
    }

    public override void SetValue(float oldValue, float value, float animationDuration)
    {
        if (m_LerpCoroutine != null)
        {
            StopCoroutine(m_LerpCoroutine);
        }
        m_LerpCoroutine = StartCoroutine(LerpFactor(animationDuration, t =>
        {
            SetValueImmediately(Mathf.Lerp(oldValue, value, t));
        }));
    }

    public override void SetValueImmediately(int value)
    {
        float fillAmount = m_MinMaxIntProgress.CalcInverseLerpValue(value);
        healthBarRenderer.GetPropertyBlock(MaterialPropertyBlock);
        MaterialPropertyBlock.SetFloat(HealthID, fillAmount);
        healthBarRenderer.SetPropertyBlock(MaterialPropertyBlock);
        healthText?.SetText($"{value}");
    }

    public override void SetValueImmediately(float value)
    {
        float fillAmount = m_MinMaxFloatProgress.CalcInverseLerpValue(value);
        healthBarRenderer.GetPropertyBlock(MaterialPropertyBlock);
        MaterialPropertyBlock.SetFloat(HealthID, fillAmount);
        healthBarRenderer.SetPropertyBlock(MaterialPropertyBlock);
        healthText?.SetText($"{Mathf.RoundToInt(value)}");
    }
}