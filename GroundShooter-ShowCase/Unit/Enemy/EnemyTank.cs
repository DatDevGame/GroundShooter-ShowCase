using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTank : EnemyUnit
{
    // public float delay = 20f;
    // public Vector3 playerOffset;
    // public float moveSpeed = 3f;
    // public float shootInterval = 2f;
    // public List<GunManual> guns;
    // public Transform turret;

    // PlayerMovementController playerHolder;
    // Unit playerUnit;
    // Vector3 targetPosition;
    // bool isActive;

    // void Start()
    // {
    //     playerHolder = FindObjectOfType<PlayerMovementController>();
    //     playerUnit = playerHolder.GetComponentInChildren<Unit>();
    //     StartCoroutine(ShootingRoutine());
    // }

    // void Update()
    // {
    //     if (!isActive) return;
    //     transform.position = Vector3.MoveTowards(transform.position, playerHolder.transform.position + playerOffset, 3f * Time.deltaTime);

    //     targetPosition = Vector3.MoveTowards(targetPosition, playerUnit.transform.position, 1f * Time.deltaTime);
    //     targetPosition.y = turret.position.y;
    //     targetPosition.z = playerUnit.transform.position.z;
    //     turret.LookAt(targetPosition);
    // }

    // IEnumerator ShootingRoutine()
    // {
    //     // yield return new WaitForSeconds(delay);
    //     isActive = true;
    //     while (true)
    //     {
    //         yield return new WaitForSeconds(shootInterval);
    //         ShootSpread();
    //     }
    // }

    // void ShootSpread()
    // {
    //     foreach (var gun in guns)
    //     {
    //         gun.Fire();
    //     }
    // }
}