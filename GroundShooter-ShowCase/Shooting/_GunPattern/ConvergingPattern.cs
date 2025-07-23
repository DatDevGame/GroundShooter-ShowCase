using UnityEngine;
using System.Collections;
using LatteGames.PoolManagement;

[CreateAssetMenu(menuName = "Gun/Converging Pattern")]
public class ConvergingPattern : GunPattern
{
    public int bulletCount = 6;
    public float spreadAngle = 90f;
    public float convergingPointDistance = 6f;

    public override IEnumerator Execute(Gun gun)
    {
        float step = spreadAngle / (bulletCount - 1);
        float start = -spreadAngle * 0.5f;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = start + step * i;
            Quaternion rot = gun.transform.rotation * Quaternion.Euler(0, angle, 0);
            Spawn(gun, rot, true); // Make bullets curve inward
        }

        yield break;
    }

    void Spawn(Gun gun, Quaternion rot, bool curve)
    {
        var bullet = PoolManager.Get<BaseBullet>(gun.prefabBullet);
        bullet.gun = gun;
        bullet.transform.SetPositionAndRotation(gun.transform.position, rot);

        if (curve && bullet.TryGetComponent<CurveBullet>(out var cb))
            cb.ActivateCurveToward(gun.transform.position + gun.transform.forward * convergingPointDistance);

        bullet.Shoot();
    }
}
