using System;
using UnityEngine;

namespace LatteGames.PoolManagement
{
    public abstract class ObjectPool
    {
        public abstract bool isInitialized { get; }
        public abstract int count { get; }
        public abstract int countActive { get; }
        public abstract int countInactive { get; }

        public abstract void Clear();
        public abstract T Get<T>() where T : class;
        public abstract void Release<T>(T item) where T : class;
    }
    public class ObjectPool<T> : ObjectPool where T : class
    {
        #region Constructors
        public ObjectPool(Func<T> createFunc, Action<T> destroyAction, bool collectionCheck = k_DefaultCollectionCheck, int initialCapacity = k_DefaultInitialCapacity, int maxCapacity = k_DefaultMaxCapacity, Action<T> onCreatePoolItem = null, Action<T> onTakePoolItem = null, Action<T> onReturnPoolItem = null, Action<T> onDestroyPoolItem = null)
        {
            m_CreateFunc = createFunc;
            m_DestroyAction = destroyAction;
            m_CollectionCheck = collectionCheck;
            m_InitialCapacity = initialCapacity;
            m_MaxCapacity = maxCapacity;
            SubscribeEvents(onCreatePoolItem, onTakePoolItem, onReturnPoolItem, onDestroyPoolItem);
        }
        #endregion

        protected const bool k_DefaultCollectionCheck = false;
        protected const int k_DefaultInitialCapacity = 10;
        protected const int k_DefaultMaxCapacity = 10000;
        protected const string k_NotInitYetExceptionMessage = "Pool has not been initialized yet!";

        public event Action<T> onCreatePoolItem = delegate { };
        public event Action<T> onTakePoolItem = delegate { };
        public event Action<T> onReturnPoolItem = delegate { };
        public event Action<T> onDestroyPoolItem = delegate { };

        [SerializeField]
        protected bool m_CollectionCheck = k_DefaultCollectionCheck;
        [SerializeField]
        protected int m_InitialCapacity = k_DefaultInitialCapacity;
        [SerializeField]
        protected int m_MaxCapacity = k_DefaultMaxCapacity;
        [NonSerialized]
        protected UnityEngine.Pool.ObjectPool<T> m_ObjectPool;

        private Func<T> m_CreateFunc;
        private Action<T> m_DestroyAction;

        public override bool isInitialized => m_ObjectPool != null;
        public override int count => objectPool.CountAll;
        public override int countActive => objectPool.CountActive;
        public override int countInactive => objectPool.CountInactive;

        protected virtual UnityEngine.Pool.ObjectPool<T> objectPool
        {
            get
            {
                if (!isInitialized)
                    throw new Exception(k_NotInitYetExceptionMessage);
                return m_ObjectPool;
            }
        }

        protected virtual void NotifyEventOnCreatePoolItem(T instance)
        {
            (instance as IPoolEventListener)?.OnCreate();
            onCreatePoolItem.Invoke(instance);
        }

        protected virtual void NotifyEventOnTakePoolItem(T instance)
        {
            (instance as IPoolEventListener)?.OnTakeFromPool();
            onTakePoolItem.Invoke(instance);
        }

        protected virtual void NotifyEventOnReturnPoolItem(T instance)
        {
            (instance as IPoolEventListener)?.OnReturnToPool();
            onReturnPoolItem.Invoke(instance);
        }

        protected virtual void NotifyEventOnDestroyPoolItem(T instance)
        {
            (instance as IPoolEventListener)?.OnDispose();
            onDestroyPoolItem.Invoke(instance);
        }

        protected virtual T Instantiate()
        {
            T instance = m_CreateFunc.Invoke();
            return instance;
        }

        protected virtual void Destroy(T item)
        {
            m_DestroyAction.Invoke(item);
        }

        protected virtual UnityEngine.Pool.ObjectPool<T> CreatePool(Action<T> onCreatePoolItem = null, Action<T> onTakePoolItem = null, Action<T> onReturnPoolItem = null, Action<T> onDestroyPoolItem = null, bool collectionCheck = false, int initialCapacity = 0, int maxCapacity = 10000)
        {
            SubscribeEvents(onCreatePoolItem, onTakePoolItem, onReturnPoolItem, onDestroyPoolItem);
            UnityEngine.Pool.ObjectPool<T> objectPool = new(Create, OnGet, OnRelease, Dispose, collectionCheck, initialCapacity, maxCapacity);
            for (int i = 0; i < initialCapacity; i++)
            {
                objectPool.Release(Create());
            }
            return objectPool;

            T Create()
            {
                T instance = Instantiate();
                NotifyEventOnCreatePoolItem(instance);
                return instance;
            }
            void OnGet(T instance)
            {
                NotifyEventOnTakePoolItem(instance);
            }
            void OnRelease(T instance)
            {
                NotifyEventOnReturnPoolItem(instance);
            }
            void Dispose(T instance)
            {
                NotifyEventOnDestroyPoolItem(instance);
                Destroy(instance);
            }
        }

        public virtual void InitializePool(Transform anchoredTransform, Action<T> onCreatePoolItem = null, Action<T> onTakePoolItem = null, Action<T> onReturnPoolItem = null, Action<T> onDestroyPoolItem = null)
        {
            if (isInitialized)
                return;
            m_ObjectPool = CreatePool(onCreatePoolItem, onTakePoolItem, onReturnPoolItem, onDestroyPoolItem, m_CollectionCheck, m_InitialCapacity, m_MaxCapacity);
        }

        public override void Clear()
        {
            objectPool.Clear();
        }

        public override U Get<U>() where U : class
        {
            return Get() as U;
        }

        public virtual T Get()
        {
            return objectPool.Get();
        }

        public virtual void Release(T item)
        {
            objectPool.Release(item);
        }

        public override void Release<U>(U item) where U : class
        {
            Release(item as T);
        }

        public virtual void SubscribeEvents(Action<T> onCreatePoolItem = null, Action<T> onTakePoolItem = null, Action<T> onReturnPoolItem = null, Action<T> onDestroyPoolItem = null)
        {
            this.onCreatePoolItem += onCreatePoolItem;
            this.onTakePoolItem += onTakePoolItem;
            this.onReturnPoolItem += onReturnPoolItem;
            this.onDestroyPoolItem += onDestroyPoolItem;
        }

        public virtual void UnsubscribeEvents(Action<T> onCreatePoolItem = null, Action<T> onTakePoolItem = null, Action<T> onReturnPoolItem = null, Action<T> onDestroyPoolItem = null)
        {
            this.onCreatePoolItem -= onCreatePoolItem;
            this.onTakePoolItem -= onTakePoolItem;
            this.onReturnPoolItem -= onReturnPoolItem;
            this.onDestroyPoolItem -= onDestroyPoolItem;
        }

        public static explicit operator ObjectPool<T>(UnityObjectPool<UnityEngine.Object> objectPool)
        {
            return objectPool as ObjectPool<T>;
        }

        public static explicit operator ObjectPool<object>(ObjectPool<T> objectPool)
        {
            return objectPool as ObjectPool<object>;
        }
    }
}