using UnityEngine;
using System.Collections;
using LatteGames.PoolManagement;

[CreateAssetMenu(menuName = "Gun/Spiral Pattern")]
public class SpiralPattern : GunPattern
{
    public int bulletCount = 12;
    public float angleStep = 30f;
    public float spiralSpeed = 10f;

    float currentAngle = 0f;

    public override IEnumerator Execute(Gun gun)
    {
        for (int i = 0; i < bulletCount; i++)
        {
            Quaternion rot = Quaternion.Euler(0, currentAngle + angleStep * i, 0);
            Spawn(gun, rot);
        }
        currentAngle += spiralSpeed;
        yield break;
    }

    public override void DrawGizmos(Gun gun)
    {
        Gizmos.color = Color.magenta;
        for (int i = 0; i < bulletCount; i++)
        {
            float a = currentAngle + angleStep * i;
            Vector3 dir = Quaternion.Euler(0, a, 0) * gun.transform.forward;
            Gizmos.DrawRay(gun.transform.position, dir * gun.maxDistance * 0.2f);
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

