using UnityEngine;
using System.Collections;
using LatteGames.PoolManagement;

[CreateAssetMenu(menuName = "Gun/Mirror Pattern")]
public class MirrorPattern : GunPattern
{
    public float mirrorAngle = 30f;

    public override IEnumerator Execute(Gun gun)
    {
        Quaternion left = gun.transform.rotation * Quaternion.Euler(0, -mirrorAngle, 0);
        Quaternion right = gun.transform.rotation * Quaternion.Euler(0, mirrorAngle, 0);

        Spawn(gun, left);
        Spawn(gun, right);

        yield break;
    }

    public override void DrawGizmos(Gun gun)
    {
        Gizmos.color = Color.cyan;
        Vector3 pos = gun.transform.position;
        Vector3 left = Quaternion.Euler(0, -mirrorAngle, 0) * gun.transform.forward;
        Vector3 right = Quaternion.Euler(0, mirrorAngle, 0) * gun.transform.forward;
        Gizmos.DrawRay(pos, left * gun.maxDistance * 0.2f);
        Gizmos.DrawRay(pos, right * gun.maxDistance * 0.2f);
    }

    void Spawn(Gun gun, Quaternion rot)
    {
        var bullet = PoolManager.Get<BaseBullet>(gun.prefabBullet);
        bullet.gun = gun;
        bullet.transform.SetPositionAndRotation(gun.transform.position, rot);
        bullet.Shoot();
    }
}


