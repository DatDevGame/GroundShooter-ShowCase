using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpPickableItem : PickableItem
{
    [SerializeField]
    private float healthAmount = 0.25f;

    public override void Collect(PlayerUnit playerUnit)
    {
        if (playerUnit.GetCurrentHealth() >= playerUnit.GetMaxHealth())
        {
            return;
        }
        base.Collect(playerUnit);
    }

    protected override void OnCollected(PlayerUnit playerUnit)
    {
        playerUnit.Heal(healthAmount * playerUnit.GetMaxHealth());
    }
}