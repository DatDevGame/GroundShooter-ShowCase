using UnityEngine;
using System.Collections;
using LatteGames.PoolManagement;

[CreateAssetMenu(menuName = "Gun/ZigZag Pattern")]
public class ZigZagPattern : GunPattern
{
    public int waveCount = 4;
    public float angle = 30f;

    private bool flip = false;

    public override IEnumerator Execute(Gun gun)
    {
        for (int i = 0; i < waveCount; i++)
        {
            float a = flip ? angle : -angle;
            flip = !flip;

            Quaternion rot = gun.transform.rotation * Quaternion.Euler(0, a, 0);
            Spawn(gun, rot);

            yield return new WaitForSeconds(0.05f);
        }
    }

    public override void DrawGizmos(Gun gun)
    {
        Gizmos.color = Color.magenta;
        Vector3 forward = gun.transform.forward;
        Vector3 left = Quaternion.Euler(0, -angle, 0) * forward;
        Vector3 right = Quaternion.Euler(0, angle, 0) * forward;

        Gizmos.DrawRay(gun.transform.position, left * gun.maxDistance * 0.15f);
        Gizmos.DrawRay(gun.transform.position, right * gun.maxDistance * 0.15f);
    }

    void Spawn(Gun gun, Quaternion rot)
    {
        var bullet = PoolManager.Get<BaseBullet>(gun.prefabBullet);
        bullet.gun = gun;
        bullet.transform.SetPositionAndRotation(gun.transform.position, rot);
        bullet.Shoot();
    }
}



