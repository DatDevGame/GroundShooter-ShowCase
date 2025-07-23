using LatteGames.PoolManagement;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TribustMissile : BaseMissile
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
