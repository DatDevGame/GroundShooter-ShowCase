#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[ExecuteAlways]
public class InfiniteSideLinesDrawer : MonoBehaviour
{

    [Header("Side Line Settings")]
    public Color LeftColor = Color.cyan;
    public Color RightColor = Color.magenta;

    [Min(0f)] public float Thickness = 3f;
    [Min(0f)] public float SideOffset = 2f;
    [Min(1f)] public float LineLength = 999f;

    [Header("Boundaries Rectangle (like Player)")]
    public PlayerMovementController PlayerMovementController;
    public Color BoundRectColor = Color.yellow;

    private void OnDrawGizmos()
    {
        Vector3 forward = transform.forward.normalized;
        Vector3 right = transform.right.normalized;
        Vector3 center = transform.position;

        // Draw infinite side lines
        Vector3 leftStart = center - right * SideOffset;
        Vector3 rightStart = center + right * SideOffset;

        Vector3 leftEnd = leftStart + forward * LineLength;
        Vector3 rightEnd = rightStart + forward * LineLength;

        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

        Handles.color = LeftColor;
        Handles.DrawAAPolyLine(Thickness, leftStart, leftEnd);

        Handles.color = RightColor;
        Handles.DrawAAPolyLine(Thickness, rightStart, rightEnd);

        // Draw rectangle from Min/Max boundaries
        if (PlayerMovementController != null)
        {
            Handles.color = BoundRectColor;
            Vector3 p0 = transform.TransformPoint(new Vector3(PlayerMovementController.XMinLimit, 0, PlayerMovementController.ZMinLimit));
            Vector3 p1 = transform.TransformPoint(new Vector3(PlayerMovementController.XMinLimit, 0, PlayerMovementController.ZMaxLimit));
            Vector3 p2 = transform.TransformPoint(new Vector3(PlayerMovementController.XMaxLimit, 0, PlayerMovementController.ZMaxLimit));
            Vector3 p3 = transform.TransformPoint(new Vector3(PlayerMovementController.XMaxLimit, 0, PlayerMovementController.ZMinLimit));

            Vector3[] rectCorners = new Vector3[] { p0, p1, p2, p3, p0 };
            Handles.DrawAAPolyLine(2f, rectCorners);
        }
    }
}
#endif
