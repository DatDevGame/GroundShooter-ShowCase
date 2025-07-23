using UnityEngine;

public class EnemyMovementController : MonoBehaviour
{
    [SerializeField] protected float offsetFromPlayer;
    [SerializeField] protected float movementRange;
    [SerializeField] protected float pingPongRange = 5f;
    [SerializeField] protected float speed = 5f;

    protected PlayerMovementController playerMovement;

    private void Awake()
    {
        playerMovement = PlayerMovementController.Instance;
    }

    private void Update()
    {
        if (!playerMovement)
            return;
        Vector3 desiredPosition = Vector3.forward * (playerMovement.transform.position.z + offsetFromPlayer);
        float pingPongValue = Mathf.PingPong(Time.time * speed, pingPongRange);
        float mappedValue = (pingPongValue * 2) - pingPongRange;
        transform.position = desiredPosition + Vector3.right * mappedValue;
    }
}
