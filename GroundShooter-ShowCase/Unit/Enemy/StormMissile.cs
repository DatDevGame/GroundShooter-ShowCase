using LatteGames.PoolManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StormMissile : BaseMissile
{
    public override void SetBaseTurret(BaseTurret baseTurret)
    {
        m_Timer = 0;
        base.SetBaseTurret(baseTurret);
    }
    protected override void MoveMissile()
    {
        transform.position += transform.forward * m_Speed * Time.deltaTime;
    }
}
