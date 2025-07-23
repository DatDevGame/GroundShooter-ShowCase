using System.Collections;
using DG.Tweening;
using UnityEngine;

public class SpiralBullet : BaseBullet
{
    public float spiralSpeed = 5f;
    public float spiralRadius = 1f;
    public float lifeTime = 5f;

    private float _angle;
    private float _time;
    private Vector3 _lastPosition;
    RaycastHit[] _hits = new RaycastHit[1];

    protected override IEnumerator ShootCoroutine()
    {
        _angle = 0f;
        _time = 0f;
        _lastPosition = transform.position;

        Vector3 forwardDir = transform.forward;

        while (_time < lifeTime)
        {
            _angle += spiralSpeed * Time.deltaTime;
            _time += Time.deltaTime;

            float x = Mathf.Cos(_angle) * spiralRadius;
            float z = Mathf.Sin(_angle) * spiralRadius;
            Vector3 offset = new Vector3(x, 0, z);

            transform.position += forwardDir * gun.bulletSpeed * Time.deltaTime + offset * Time.deltaTime;

            var delta = transform.position - _lastPosition;
            if (Physics.SphereCastNonAlloc(_lastPosition, gun.bulletRadius, transform.forward, _hits, delta.magnitude, gun.hitLayers) > 0)
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
