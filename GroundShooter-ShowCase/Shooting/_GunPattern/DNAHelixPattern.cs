using UnityEngine;
using System.Collections;
using LatteGames.PoolManagement;

[CreateAssetMenu(menuName = "Gun/DNA Helix Pattern")]
public class DNAHelixPattern : GunPattern
{
    public int helixCount = 2;
    public float helixAngle = 20f;
    private float helixPhase = 0f;

    public override IEnumerator Execute(Gun gun)
    {
        for (int i = 0; i < helixCount; i++)
        {
            float angle = Mathf.Sin(Time.time * 10f + i * Mathf.PI) * helixAngle;
            Quaternion rot = gun.transform.rotation * Quaternion.Euler(0, angle, 0);
            Spawn(gun, rot);
        }

        helixPhase += Time.deltaTime;
        yield break;
    }

    public override void DrawGizmos(Gun gun)
    {
        Gizmos.color = Color.white;
        Vector3 dir1 = Quaternion.Euler(0, Mathf.Sin(Time.time * 10f) * helixAngle, 0) * gun.transform.forward;
        Vector3 dir2 = Quaternion.Euler(0, -Mathf.Sin(Time.time * 10f) * helixAngle, 0) * gun.transform.forward;
        Gizmos.DrawRay(gun.transform.position, dir1 * gun.maxDistance * 0.2f);
        Gizmos.DrawRay(gun.transform.position, dir2 * gun.maxDistance * 0.2f);
    }

    void Spawn(Gun gun, Quaternion rot)
    {
        var b = PoolManager.Get<BaseBullet>(gun.prefabBullet);
        b.gun = gun;
        b.transform.SetPositionAndRotation(gun.transform.position, rot);
        b.Shoot();
    }
}



