using LatteGames.PoolManagement;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwinMissile : BaseMissile
{
    protected override void MoveMissile()
    {
        transform.position += transform.forward * m_Speed * Time.deltaTime;
    }
}
