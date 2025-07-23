using System.Collections;
using System.Collections.Generic;
using LatteGames;
using LatteGames.PoolManagement;
using UnityEngine;

public static class ParticleSystemExtensions
{
    public static void Release(this ParticleSystem particleInstance, ParticleSystem particlePrefab, float delayTime)
    {
        PoolManager.Instance.StartCoroutine(CommonCoroutine.Delay(delayTime, false, () =>
        {
            if(PoolManager.ContainsPool(particlePrefab.GetInstanceID().ToString()) || PoolManager.ContainsPool(particlePrefab.gameObject.GetInstanceID().ToString()))
                PoolManager.Release(particlePrefab, particleInstance);
        }));
    }
}