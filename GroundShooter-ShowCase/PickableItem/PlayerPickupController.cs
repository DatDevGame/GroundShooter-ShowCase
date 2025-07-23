using HyrphusQ.Events;
using UnityEngine;

public class PlayerPickupController : MonoBehaviour
{
    [SerializeField]
    private LayerMask pickupItemLayers;

    private PlayerUnit playerUnit;
    private Collider[] detectedColliders = new Collider[100];

    private void Start()
    {
        playerUnit = PlayerUnit.Instance;
        InvokeRepeating(nameof(CollectPickableItems), 0f, 0.5f);
    }

    private void OnDrawGizmos()
    {
        if (playerUnit == null)
            return;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(playerUnit.transform.position, playerUnit.Stats.GetStatValue(StatType.MagnetRange));
    }

    private void CollectPickableItems()
    {
        int hitDetectionCount = Physics.OverlapSphereNonAlloc(playerUnit.transform.position, playerUnit.Stats.GetStatValue(StatType.MagnetRange), detectedColliders);
        for (int i = 0; i < hitDetectionCount; i++)
        {
            Collider hitCollider = detectedColliders[i];
            if (hitCollider.TryGetComponent(out IPickableItem pickableItem))
            {
                pickableItem.Collect(playerUnit);
            }
        }

    }
}