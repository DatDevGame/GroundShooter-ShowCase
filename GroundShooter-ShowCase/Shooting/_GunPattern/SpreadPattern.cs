// SpreadPattern.cs
using UnityEngine;
using System.Collections;
using LatteGames.PoolManagement;

[CreateAssetMenu(menuName = "Gun/Spread Pattern")]
public class SpreadPattern : GunPattern
{
    [Range(2, 128)] public int bulletCount = 5;
    [Range(0, 360)] public float spreadAngle = 45f;

    public override IEnumerator Execute(Gun gun)
    {
        float step = spreadAngle / (bulletCount - 1);
        float start = -spreadAngle * 0.5f;
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = start + step * i;
            Quaternion rot = gun.transform.rotation * Quaternion.Euler(0, angle, 0);
            Spawn(gun, rot);
        }
        yield break;
    }

    public override void DrawGizmos(Gun gun)
    {
        Gizmos.color = Color.yellow;
        Vector3 pos = gun.transform.position;
        Vector3 forward = gun.transform.forward;
        float step = spreadAngle / (bulletCount - 1);
        float start = -spreadAngle * 0.5f;
        for (int i = 0; i < bulletCount; i++)
        {
            float a = start + step * i;
            Vector3 dir = Quaternion.Euler(0, a, 0) * forward;
            Gizmos.DrawRay(pos, dir * gun.maxDistance * 0.2f);
        }
    }

    void Spawn(Gun gun, Quaternion rot)
    {
        var b = PoolManager.Get<BaseBullet>(gun.prefabBullet);
        b.gun = gun;
        b.transform.SetPositionAndRotation(gun.transform.position, rot);
        b.Shoot();
    }
}

