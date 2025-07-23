using DG.Tweening;
using LatteGames.PoolManagement;
using UnityEngine;

public abstract class PickableItem : MonoBehaviour, IPickableItem, IPoolEventListener
{
    [SerializeField]
    protected Collider triggerCollider;
    [SerializeField]
    protected float randomPositionOffset = 3f;

    protected abstract void OnCollected(PlayerUnit playerUnit);

    public virtual void Collect(PlayerUnit playerUnit)
    {
        triggerCollider.enabled = false;
        DOTween.Kill(transform);
        transform.SetParent(playerUnit.transform);
        transform.DOLocalMove(Vector3.up, 0.25f).OnComplete(() =>
        {
            OnCollected(playerUnit);
            transform.SetParent(PoolManager.Instance.transform);
            PoolManager.Release(name.Replace("(Clone)", ""), this);
        });
    }

    public virtual void OnCreate()
    {
        triggerCollider.isTrigger = true;
        triggerCollider.enabled = false;
        gameObject.SetActive(false);
    }

    public virtual void OnTakeFromPool()
    {
        gameObject.SetActive(true);
    }

    public virtual void OnReturnToPool()
    {
        gameObject.SetActive(false);
    }

    public virtual void OnDispose()
    {

    }

    public virtual void PlaySpawnAnimation()
    {
        triggerCollider.enabled = false;
        float randomX = Random.Range(-randomPositionOffset, randomPositionOffset);
        float randomZ = Random.Range(0f, randomPositionOffset);
        Vector3 randomOffset = new(randomX, 0f, randomZ);

        var finalPosition = transform.position + randomOffset.normalized * randomPositionOffset;
        transform.DOLocalMove(finalPosition, 0.35f).OnComplete(() => triggerCollider.enabled = true);
    }
}