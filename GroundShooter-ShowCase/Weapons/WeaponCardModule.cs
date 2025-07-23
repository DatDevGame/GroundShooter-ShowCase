using HyrphusQ.Events;
using UnityEngine;

public class WeaponCardModule : CardItemModule
{
    public override int numOfCards
    {
        get => Mathf.RoundToInt(CurrencyManager.Instance.GetCurrencySO(itemSO.Cast<WeaponSO>().WeaponType.ConvertToCurrencyType()).value);
        protected set
        {
            CurrencyManager.Instance.GetCurrencySO(itemSO.Cast<WeaponSO>().WeaponType.ConvertToCurrencyType()).value = value;
        }
    }

    private void OnCurrencyValueChanged(ValueDataChanged<float> eventData)
    {
        NotifyEventNumOfCardsChanged();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.GetCurrencySO(itemSO.Cast<WeaponSO>().WeaponType.ConvertToCurrencyType()).onValueChanged += OnCurrencyValueChanged;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.GetCurrencySO(itemSO.Cast<WeaponSO>().WeaponType.ConvertToCurrencyType()).onValueChanged -= OnCurrencyValueChanged;
    }
}