using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LatteGames.PoolManagement
{
    [Serializable]
    public class UnityObjectPool<T> : ObjectPool<T> where T : Object
    {
        #region Constructors
        public UnityObjectPool() : base(null, null, k_DefaultCollectionCheck, k_DefaultInitialCapacity, k_DefaultMaxCapacity)
        {

        }
        public UnityObjectPool(T objectPrefab, bool collectionCheck = k_DefaultCollectionCheck, int initialCapacity = k_DefaultInitialCapacity, int maxCapacity = k_DefaultMaxCapacity, Action<T> onCreatePoolItem = null, Action<T> onTakePoolItem = null, Action<T> onReturnPoolItem = null, Action<T> onDestroyPoolItem = null) : base(null, null, collectionCheck, initialCapacity, maxCapacity, onCreatePoolItem, onTakePoolItem, onReturnPoolItem, onDestroyPoolItem)
        {
            m_ObjectPrefab = objectPrefab;
        }
        #endregion

        [SerializeField]
        protected T m_ObjectPrefab;

        protected Transform m_AnchoredTransform;
        protected AsyncInstantiateOperation<T> m_InitAsyncOperation;

        public virtual T objectPrefab => m_ObjectPrefab;
        public override bool isInitialized => m_AnchoredTransform != null;

        protected override T Instantiate()
        {
            T instance = Object.Instantiate(m_ObjectPrefab, m_AnchoredTransform);
            return instance;
        }

        protected override void Destroy(T item)
        {
            Object.Destroy(item);
        }

        public override void InitializePool(Transform anchoredTransform, Action<T> onCreatePoolItem = null, Action<T> onTakePoolItem = null, Action<T> onReturnPoolItem = null, Action<T> onDestroyPoolItem = null)
        {
            if (isInitialized)
                return;
            m_AnchoredTransform = anchoredTransform;
            m_ObjectPool = CreatePool(onCreatePoolItem, onTakePoolItem, onReturnPoolItem, onDestroyPoolItem, m_CollectionCheck, m_InitialCapacity, m_MaxCapacity);
        }

        public virtual AsyncInstantiateOperation InitializePoolAsync(Transform anchoredTransform, Action<T> onCreatePoolItem = null, Action<T> onTakePoolItem = null, Action<T> onReturnPoolItem = null, Action<T> onDestroyPoolItem = null)
        {
            if (isInitialized)
                return m_InitAsyncOperation;
            m_AnchoredTransform = anchoredTransform;
            m_ObjectPool = CreatePool(onCreatePoolItem, onTakePoolItem, onReturnPoolItem, onDestroyPoolItem, m_CollectionCheck, 0, m_MaxCapacity);
            m_InitAsyncOperation = Object.InstantiateAsync(m_ObjectPrefab, m_InitialCapacity, m_AnchoredTransform);
            m_InitAsyncOperation.completed += OnInstantiateCompleted;
            return m_InitAsyncOperation;

            void OnInstantiateCompleted(AsyncOperation _)
            {
                T[] result = m_InitAsyncOperation.Result;
                foreach (var instance in result)
                {
                    NotifyEventOnCreatePoolItem(instance);
                    Release(instance);
                }
            }
        }
    }
}