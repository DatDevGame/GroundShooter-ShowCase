using UnityEngine;
using System.Collections;
using LatteGames.PoolManagement;

[CreateAssetMenu(menuName = "Gun/Cross Pattern")]
public class CrossPattern : GunPattern
{
    public override IEnumerator Execute(Gun gun)
    {
        Vector3[] dirs = {
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right
        };

        foreach (var dir in dirs)
        {
            Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
            Spawn(gun, rot);
        }

        yield break;
    }

    public override void DrawGizmos(Gun gun)
    {
        Gizmos.color = Color.white;
        Vector3 pos = gun.transform.position;
        Gizmos.DrawRay(pos, Vector3.forward * gun.maxDistance * 0.2f);
        Gizmos.DrawRay(pos, Vector3.back * gun.maxDistance * 0.2f);
        Gizmos.DrawRay(pos, Vector3.left * gun.maxDistance * 0.2f);
        Gizmos.DrawRay(pos, Vector3.right * gun.maxDistance * 0.2f);
    }

    void Spawn(Gun gun, Quaternion rot)
    {
        var bullet = PoolManager.Get<BaseBullet>(gun.prefabBullet);
        bullet.gun = gun;
        bullet.transform.SetPositionAndRotation(gun.transform.position, rot);
        bullet.Shoot();
    }
}





