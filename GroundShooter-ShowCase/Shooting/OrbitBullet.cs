using System.Collections;
using UnityEngine;

public class OrbitBullet : BaseBullet
{
    public float orbitRadius = 2f;
    public float orbitSpeed = 180f; // degrees per second
    public float lifeTime = 5f;

    private float _currentAngle;
    private Vector3 _center;
    private float _timer;
    private Vector3 _lastPosition;
    RaycastHit[] _hits = new RaycastHit[1];

    protected override IEnumerator ShootCoroutine()
    {
        _timer = 0f;
        _currentAngle = 0f;
        _lastPosition = transform.position;
        _center = transform.position - transform.right * orbitRadius;

        while (_timer < lifeTime)
        {
            _timer += Time.deltaTime;
            _currentAngle += orbitSpeed * Time.deltaTime;

            float rad = _currentAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * orbitRadius;
            transform.position = _center + offset;

            var delta = transform.position - _lastPosition;
            if (Physics.SphereCastNonAlloc(_lastPosition, gun.bulletRadius, delta.normalized, _hits, delta.magnitude, gun.hitLayers) > 0)
            {
                OnHit(_hits[0]);
                yield break;
            }

            _lastPosition = transform.position;
            yield return null;
        }

        Despawn();
    }
}

