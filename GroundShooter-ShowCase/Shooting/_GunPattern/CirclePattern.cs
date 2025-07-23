using UnityEngine;
using System.Collections;
using LatteGames.PoolManagement;

[CreateAssetMenu(menuName = "Gun/Circle Pattern")]
public class CirclePattern : GunPattern
{
    public int bulletCount = 24;

    public override IEnumerator Execute(Gun gun)
    {
        float step = 360f / bulletCount;
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = step * i;
            Quaternion rot = Quaternion.Euler(0, angle, 0);
            Spawn(gun, rot);
        }
        yield break;
    }

    public override void DrawGizmos(Gun gun)
    {
        Gizmos.color = Color.green;
        float step = 360f / bulletCount;
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = step * i;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            Gizmos.DrawRay(gun.transform.position, dir * gun.maxDistance * 0.15f);
        }
    }

    void Spawn(Gun gun, Quaternion rot)
    {
        var bullet = PoolManager.Get<BaseBullet>(gun.prefabBullet);
        bullet.gun = gun;
        bullet.transform.SetPositionAndRotation(gun.transform.position, rot);
        bullet.Shoot();
    }
}



