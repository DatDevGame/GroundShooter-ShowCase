using System.Collections;
using UnityEngine;

/// <summary>
/// Bullet that can steer (curve) toward a chosen point, then continue straight.
/// Inherits hit-detection logic from <see cref="Bullet"/>.
/// </summary>
public class CurveBullet : Bullet
{
    // ────────────────────────────────
    //           Inspector
    // ────────────────────────────────

    [Header("Curve Settings")]
    [Tooltip("Maximum steering speed (degrees/second).")]
    public float steerSpeed = 180f;

    [Tooltip("Stop steering when the bullet is this close (metres) to the target point.")]
    public float reachThreshold = 0.2f;

    // ────────────────────────────────
    //           Runtime
    // ────────────────────────────────

    private bool _curveEnabled;      // Are we currently steering?
    private Vector3 _targetPoint;      // World-space point we are steering toward
    private float _traveledDistance; // How far the bullet has moved so far

    // Cached from <see cref="Bullet"/>
    // protected Vector3 _lastPosition;
    // protected RaycastHit[] _hits

    // Called whenever the object is (re)enabled by the pool.
    protected override void OnEnable()
    {
        base.OnEnable();               // Resets _lastPosition in Bullet
        _curveEnabled = false;
        _targetPoint = Vector3.forward;
        _traveledDistance = 0f;
    }

    /// <summary>
    /// Activates the curve-toward behaviour.
    /// Call this right after spawning the bullet.
    /// </summary>
    /// <param name="worldPoint">The point the bullet should curve toward.</param>
    public void ActivateCurveToward(Vector3 worldPoint)
    {
        _targetPoint = worldPoint;
        _curveEnabled = true;
    }

    /// <summary>
    /// Core movement coroutine.  
    /// Loops until the bullet hits something or exceeds <c>gun.maxDistance</c>.
    /// </summary>
    protected override IEnumerator ShootCoroutine()
    {
        // Wait one frame to give the spawner time to call ActivateCurveToward.
        yield return null;

        while (_traveledDistance < gun.maxDistance)
        {
            float delta = Time.deltaTime;
            float stepLen = gun.bulletSpeed * delta;      // Distance we *want* to move this frame

            // ─────────────────────────────────────────────
            //                Steering
            // ─────────────────────────────────────────────
            if (_curveEnabled)
            {
                Vector3 toTarget = _targetPoint - transform.position;

                // Gradually rotate forward vector toward the target
                Vector3 newDir = Vector3.RotateTowards(
                    transform.forward,
                    toTarget.normalized,
                    Mathf.Deg2Rad * steerSpeed * delta,
                    0f);

                transform.rotation = Quaternion.LookRotation(newDir, Vector3.up);

                // Stop steering when close enough to the point
                if (toTarget.sqrMagnitude <= reachThreshold * reachThreshold)
                    _curveEnabled = false;
            }

            // ─────────────────────────────────────────────
            //                Movement
            // ─────────────────────────────────────────────
            Vector3 displacement = transform.forward * stepLen;

            // If we've stopped steering and this step would overshoot the point,
            // clamp the displacement so we land exactly on the target.
            if (!_curveEnabled)
            {
                Vector3 toTarget = _targetPoint - transform.position;

                // dot > 0 → still moving toward the point
                // disp² > toTarget² → step would carry us past the point
                if (Vector3.Dot(displacement, toTarget) > 0 &&
                    displacement.sqrMagnitude > toTarget.sqrMagnitude)
                {
                    displacement = toTarget;     // Land exactly on the point
                }
            }

            Vector3 nextPos = transform.position + displacement;

            // ─────────────────────────────────────────────
            //            Collision (SphereCast)
            // ─────────────────────────────────────────────
            if (Physics.SphereCastNonAlloc(
                    transform.position,
                    gun.bulletRadius,
                    transform.forward,
                    _hits,
                    displacement.magnitude,
                    gun.hitLayers,
                    QueryTriggerInteraction.Collide) > 0)
            {
                OnHit(_hits[0]);     // Damage + despawn handled in base class
                yield break;
            }

            // Apply movement
            transform.position = nextPos;
            _traveledDistance += displacement.magnitude;
            _lastPosition = transform.position;   // For next frame’s cast

            yield return null;   // Wait for the next frame
        }

        // Exceeded max travel distance → despawn
        Despawn();
    }

    /// <summary>
    /// Extra clean-up when the bullet is recycled.
    /// </summary>
    protected override void Despawn()
    {
        base.Despawn();
        _curveEnabled = false;   // Reset flag for pooling safety
    }
}
