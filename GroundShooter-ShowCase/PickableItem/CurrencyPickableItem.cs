using UnityEngine;

public class CurrencyPickableItem : PickableItem
{
    [SerializeField]
    private CurrencyType currencyType;
    [SerializeField]
    private float amount;

    public CurrencyType CurrencyType
    {
        get => currencyType;
        set => currencyType = value;
    }
    public float Amount
    {
        get => amount;
        set => amount = value;
    }

    protected override void OnCollected(PlayerUnit playerUnit)
    {
        CurrencyManager.Instance.AcquireWithoutLogEvent(CurrencyType, Amount);
    }
}