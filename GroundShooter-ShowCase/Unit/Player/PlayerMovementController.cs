using HyrphusQ.Events;
using UnityEditor;
using UnityEngine;

public class PlayerMovementController : Singleton<PlayerMovementController>
{
    public float XMinLimit => xMinLimit;
    public float XMaxLimit => xMaxLimit;
    public float ZMinLimit => zMinLimit;
    public float ZMaxLimit => zMaxLimit;

    [Header("Movement Settings")]
    [SerializeField] Animator animator;
    [SerializeField] GameObject camFocusPoint;
    [SerializeField] GameObject localMovingObject;
    [SerializeField] GameObject m_MiddlePointLocal;
    [SerializeField] float forwardSpeed = 0.2f;
    [SerializeField] float dragSensitivity = 0.01f;
    [SerializeField] float movementSpeed = 20f;
    [SerializeField] AnimationCurve smoothInputCurve;

    [Header("Movement Boundaries")]
    [SerializeField] float xMinLimit = -2f;
    [SerializeField] float xMaxLimit = 2f;
    [SerializeField] float zMinLimit = -1f;
    [SerializeField] float zMaxLimit = 1f;

    private Vector2 previousInputPosition;
    private bool isDragging = false;
    private Vector3 originalLocalPosition;
    private Vector3 currentAnimDirection;
    private Vector3 previousCharacterPosition;
    private Vector3 desiredLocalPosition;
    private Plane plane = new Plane(Vector3.up, Vector3.zero);
    public float ForwardSpeed => forwardSpeed;

    private static readonly int GlobalPlayerPositionID = Shader.PropertyToID("_GlobalPlayerPosition");

    private void Start()
    {
        Input.multiTouchEnabled = false;
        if (m_MiddlePointLocal != null)
        {
            originalLocalPosition = m_MiddlePointLocal.transform.localPosition;
            desiredLocalPosition = localMovingObject.transform.localPosition;
            previousCharacterPosition = localMovingObject.transform.position;
        }
        GameEventHandler.AddActionEvent(GameFlowState.EndLevel, OnEndLevel);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(GameFlowState.EndLevel, OnEndLevel);
    }

    private void Update()
    {
        transform.position += forwardSpeed * Time.deltaTime * Vector3.forward;
        HandleInput();
        // Update global shader property with player position
        Shader.SetGlobalVector(GlobalPlayerPositionID, transform.position);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = ScreenPointOnPlane(Input.mousePosition);
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(center, 0.5f);
    }

    private void OnEndLevel()
    {
        enabled = false;
    }

    private void HandleInput()
    {
        HandleMouseInput();
    }
    private Vector3 ScreenPointOnPlane(Vector3 mousePosition)
    {
        var ray = Camera.main.ScreenPointToRay(mousePosition);
        plane.Raycast(ray, out var distance);
        return ray.GetPoint(distance);
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            previousInputPosition = Input.mousePosition;
            isDragging = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            Vector2 currentInputPosition = Input.mousePosition;
            Vector3 currentWorldPosition = ScreenPointOnPlane(currentInputPosition);
            Vector3 previousWorldPosition = ScreenPointOnPlane(previousInputPosition);
            MoveObject(currentWorldPosition - previousWorldPosition);
            Vector2 currentInputViewportPos = Camera.main.ScreenToViewportPoint(currentInputPosition);
            previousInputPosition.y = currentInputPosition.y;
            previousInputPosition.x = Mathf.Lerp(previousInputPosition.x, currentInputPosition.x, Time.deltaTime * smoothInputCurve.Evaluate(currentInputViewportPos.x));

            var direction = localMovingObject.transform.position - previousCharacterPosition;
            previousCharacterPosition = localMovingObject.transform.position;

            currentAnimDirection = Vector3.MoveTowards(currentAnimDirection, direction, direction.magnitude * 0.04f);

            animator.SetFloat("DirectionBlendKeyX", currentAnimDirection.normalized.x);
            animator.SetFloat("DirectionBlendKeyY", currentAnimDirection.normalized.z);
            animator.SetFloat("SpeedBlendKey", Mathf.Clamp01(currentAnimDirection.magnitude * 30));
        }
        else
        {
            currentAnimDirection = Vector3.zero;

            animator.SetFloat("DirectionBlendKeyX", currentAnimDirection.normalized.x);
            animator.SetFloat("DirectionBlendKeyY", currentAnimDirection.normalized.z);
            animator.SetFloat("SpeedBlendKey", Mathf.Clamp01(currentAnimDirection.magnitude * 30));
        }
    }

    private void MoveObject(Vector3 deltaPosition)
    {
        if (localMovingObject == null) return;

        // Calculate new position with both X and Z movement
        Vector3 newPosition = desiredLocalPosition;
        newPosition += dragSensitivity * Time.timeScale * Vector3.ClampMagnitude(deltaPosition, 1f);

        // Apply boundaries
        newPosition.x = Mathf.Clamp(newPosition.x, originalLocalPosition.x + xMinLimit, originalLocalPosition.x + xMaxLimit);
        newPosition.z = Mathf.Clamp(newPosition.z, originalLocalPosition.z + zMinLimit, originalLocalPosition.z + zMaxLimit);
        desiredLocalPosition = newPosition;
        localMovingObject.transform.localPosition = Vector3.Lerp(localMovingObject.transform.localPosition, desiredLocalPosition, Time.deltaTime * 8f);

        camFocusPoint.transform.localPosition = new Vector3(localMovingObject.transform.localPosition.x * 0.8f, camFocusPoint.transform.localPosition.y, camFocusPoint.transform.localPosition.z);
    }

    // Reset position (optional, can be called when needed)
    public void ResetLocalPosition()
    {
        if (localMovingObject != null)
        {
            localMovingObject.transform.localPosition = originalLocalPosition;
            desiredLocalPosition = originalLocalPosition;
        }
    }
    public float GetPlayerOffsetFromStart()
    {
        return transform.position.z;
    }
}