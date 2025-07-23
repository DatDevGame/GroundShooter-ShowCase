using UnityEngine;
using System.Collections;
using LatteGames.PoolManagement;

[CreateAssetMenu(menuName = "Gun/Wave Pattern")]
public class WavePattern : GunPattern
{
    public int bulletCount = 6;
    public float spreadAngle = 30f;
    public float waveFrequency = 2f;

    float time = 0f;

    public override IEnumerator Execute(Gun gun)
    {
        float step = spreadAngle / (bulletCount - 1);
        float start = -spreadAngle * 0.5f;
        time += Time.deltaTime;

        for (int i = 0; i < bulletCount; i++)
        {
            float offset = Mathf.Sin(time * waveFrequency + i * 0.5f) * spreadAngle;
            float angle = start + step * i + offset;
            Quaternion rot = gun.transform.rotation * Quaternion.Euler(0, angle, 0);
            Spawn(gun, rot);
        }
        yield break;
    }

    public override void DrawGizmos(Gun gun)
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(gun.transform.position, gun.transform.forward * gun.maxDistance * 0.25f);
    }

    void Spawn(Gun gun, Quaternion rot)
    {
        var bullet = PoolManager.Get<BaseBullet>(gun.prefabBullet);
        bullet.gun = gun;
        bullet.transform.SetPositionAndRotation(gun.transform.position, rot);
        bullet.Shoot();
    }
}




