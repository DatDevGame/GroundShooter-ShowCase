using UnityEngine;

public class ExpPickableItem : PickableItem
{
    [SerializeField]
    private float expAmount;

    public float ExpAmount
    {
        get => expAmount;
        set => expAmount = value;
    }

    protected override void OnCollected(PlayerUnit playerUnit)
    {
        playerUnit.PlayerLevelSO.AddExp(ExpAmount);
    }
}