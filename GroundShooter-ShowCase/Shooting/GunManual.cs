using System.Collections;
using UnityEngine;
using LatteGames.PoolManagement;

public class GunManual : Gun
{
    // protected override void OnEnable()
    // {
        
    // }

    public void Fire()
    {
        var bullet = PoolManager.Get<BaseBullet>(prefabBullet);
        bullet.gun = this;
        bullet.transform.position = transform.position;
        bullet.transform.rotation = transform.rotation;
        bullet.Shoot();
    }
}
