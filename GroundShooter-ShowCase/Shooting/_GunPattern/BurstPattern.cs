using UnityEngine;
using System.Collections;
using LatteGames.PoolManagement;

[CreateAssetMenu(menuName = "Gun/Burst Pattern")]
public class BurstPattern : GunPattern
{
    [Header("Burst Config")]
    public int bursts = 3;
    public int bulletsPerBurst = 4;
    public float burstSpacing = 0.1f;
    public float spreadAngle = 15f;

    public override IEnumerator Execute(Gun gun)
    {
        for (int b = 0; b < bursts; b++)
        {
            float step = bulletsPerBurst > 1
                ? spreadAngle / (bulletsPerBurst - 1)
                : 0f;
            float start = -spreadAngle * 0.5f;

            for (int i = 0; i < bulletsPerBurst; i++)
            {
                float angle = start + step * i;
                Quaternion rot = gun.transform.rotation * Quaternion.Euler(0, angle, 0);
                Spawn(gun, rot);
            }

            if (b < bursts - 1)
                yield return new WaitForSeconds(burstSpacing);
        }
    }

    public override void DrawGizmos(Gun gun)
    {
        Gizmos.color = Color.cyan;
        Vector3 pos = gun.transform.position;
        Vector3 forward = gun.transform.forward;

        float step = bulletsPerBurst > 1
            ? spreadAngle / (bulletsPerBurst - 1)
            : 0f;
        float start = -spreadAngle * 0.5f;

        for (int i = 0; i < bulletsPerBurst; i++)
        {
            float a = start + step * i;
            Vector3 dir = Quaternion.Euler(0, a, 0) * forward;
            Gizmos.DrawRay(pos, dir * gun.maxDistance * 0.2f);
        }
    }

    void Spawn(Gun gun, Quaternion rot)
    {
        var bulletPool = PoolManager.GetOrCreatePool(gun.prefabBullet);
        var bullet = bulletPool.Get();
        bullet.gun = gun;
        bullet.transform.SetPositionAndRotation(gun.transform.position, rot);
        bullet.Shoot();
    }
}
