using UnityEngine;
using System.Collections;
using LatteGames.PoolManagement;

[CreateAssetMenu(menuName = "Gun/Random Spray Pattern")]
public class RandomSprayPattern : GunPattern
{
    public int bulletCount = 10;
    public float spreadAngle = 45f;

    public override IEnumerator Execute(Gun gun)
    {
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = Random.Range(-spreadAngle * 0.5f, spreadAngle * 0.5f);
            Quaternion rot = gun.transform.rotation * Quaternion.Euler(0, angle, 0);
            Spawn(gun, rot);
        }
        yield break;
    }

    public override void DrawGizmos(Gun gun)
    {
        Gizmos.color = Color.red;
        Vector3 forward = gun.transform.forward;
        Gizmos.DrawRay(gun.transform.position, forward * gun.maxDistance * 0.2f);
    }

    void Spawn(Gun gun, Quaternion rot)
    {
        var bullet = PoolManager.Get<BaseBullet>(gun.prefabBullet);
        bullet.gun = gun;
        bullet.transform.SetPositionAndRotation(gun.transform.position, rot);
        bullet.Shoot();
    }
}


