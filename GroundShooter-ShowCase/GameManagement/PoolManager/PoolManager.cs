using System;
using System.Collections.Generic;
using HyrphusQ.Events;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LatteGames.PoolManagement
{
    public class PoolManager : Singleton<PoolManager>
    {
        public static event Action<string, object> onCreatePoolItem = delegate { };
        public static event Action<string, object> onTakePoolItem = delegate { };
        public static event Action<string, object> onReturnPoolItem = delegate { };
        public static event Action<string, object> onDestroyPoolItem = delegate { };

        private const string k_DebugTag = nameof(PoolManager);
        [ShowInInspector, ReadOnly, Title("Debug")]
        private static Dictionary<string, ObjectPool> s_RuntimePoolDictionary;

        protected override void Awake()
        {
            base.Awake();
            Initialize();
        }

        private static GlobalPoolConfigSO GetConfigSO()
        {
            return GlobalPoolConfigSO.Instance;
        }

        private static void NotifyEventOnCreatePoolItem(string key, object instance)
        {
            onCreatePoolItem.Invoke(key, instance);
            GameEventHandler.Invoke(PoolManagementEventCode.OnCreatePoolItem, key, instance);
            if (Instance.m_Verbose)
                LGDebug.Log($"Create pool item [{instance}] with key [{key}]", k_DebugTag);
        }

        private static void NotifyEventOnTakePoolItem(string key, object instance)
        {
            onTakePoolItem.Invoke(key, instance);
            GameEventHandler.Invoke(PoolManagementEventCode.OnTakePoolItem, key, instance);
            if (Instance.m_Verbose)
                LGDebug.Log($"Take pool item [{instance}] with key [{key}]", k_DebugTag);
        }

        private static void NotifyEventOnReturnPoolItem(string key, object instance)
        {
            onReturnPoolItem.Invoke(key, instance);
            GameEventHandler.Invoke(PoolManagementEventCode.OnReturnPoolItem, key, instance);
            if (Instance.m_Verbose)
                LGDebug.Log($"Return pool item [{instance}] with key [{key}]", k_DebugTag);
        }

        private static void NotifyEventOnDestroyPoolItem(string key, object instance)
        {
            onDestroyPoolItem.Invoke(key, instance);
            GameEventHandler.Invoke(PoolManagementEventCode.OnDestroyPoolItem, key, instance);
            if (Instance.m_Verbose)
                LGDebug.Log($"Destroy pool item [{instance}] with key [{key}]", k_DebugTag);
        }

        public static bool IsInitialized()
        {
            return s_RuntimePoolDictionary != null;
        }

        public static void Initialize()
        {
            if (IsInitialized())
                return;
            s_RuntimePoolDictionary = new Dictionary<string, ObjectPool>();
            List<PredefinedObjectPool> predefinedObjectPools = GetConfigSO().predefinedObjectPools;
            foreach (var predefinedObjectPool in predefinedObjectPools)
            {
                AddPool(predefinedObjectPool.key, predefinedObjectPool.objectPool);
            }
            if (Instance.m_Verbose)
                LGDebug.Log($"{nameof(PoolManager)} initialize", k_DebugTag);
        }

        public static bool ContainsPool(string key)
        {
            Initialize();
            return s_RuntimePoolDictionary.ContainsKey(key);
        }

        public static void AddPool<T>(string key, ObjectPool<T> objectPool) where T : class
        {
            Initialize();
            if (!objectPool.isInitialized)
            {
                objectPool.InitializePool(Instance.transform, OnCreatePoolItem, OnTakePoolItem, OnReturnPoolItem, OnDestroyPoolItem);
            }
            else
            {
                objectPool.SubscribeEvents(OnCreatePoolItem, OnTakePoolItem, OnReturnPoolItem, OnDestroyPoolItem);
            }
            s_RuntimePoolDictionary.Add(key, objectPool);
            if (Instance.m_Verbose)
                LGDebug.Log($"Add pool with {key}", k_DebugTag);

            void OnCreatePoolItem(T instance)
            {
                NotifyEventOnCreatePoolItem(key, instance);
            }
            void OnTakePoolItem(T instance)
            {
                NotifyEventOnTakePoolItem(key, instance);
            }
            void OnReturnPoolItem(T instance)
            {
                NotifyEventOnReturnPoolItem(key, instance);
            }
            void OnDestroyPoolItem(T instance)
            {
                NotifyEventOnDestroyPoolItem(key, instance);
            }
        }

        public static void AddPool<T>(string key, UnityObjectPool<T> objectPool, bool asyncInit = false) where T : Object
        {
            Initialize();
            if (!objectPool.isInitialized)
            {
                if (!asyncInit)
                {
                    objectPool.InitializePool(Instance.transform, OnCreatePoolItem, OnTakePoolItem, OnReturnPoolItem, OnDestroyPoolItem);
                }
                else
                {
                    objectPool.InitializePoolAsync(Instance.transform, OnCreatePoolItem, OnTakePoolItem, OnReturnPoolItem, OnDestroyPoolItem);
                }
            }
            else
            {
                objectPool.SubscribeEvents(OnCreatePoolItem, OnTakePoolItem, OnReturnPoolItem, OnDestroyPoolItem);
            }
            s_RuntimePoolDictionary.Add(key, objectPool);
            if (Instance.m_Verbose)
                LGDebug.Log($"Add unity pool with {key}", k_DebugTag);

            void OnCreatePoolItem(T instance)
            {
                NotifyEventOnCreatePoolItem(key, instance);
            }
            void OnTakePoolItem(T instance)
            {
                NotifyEventOnTakePoolItem(key, instance);
            }
            void OnReturnPoolItem(T instance)
            {
                NotifyEventOnReturnPoolItem(key, instance);
            }
            void OnDestroyPoolItem(T instance)
            {
                NotifyEventOnDestroyPoolItem(key, instance);
            }
        }

        public static void AddPools<T>(IEnumerable<KeyValuePair<string, UnityObjectPool<T>>> keyPoolCollection, bool asyncInit = false) where T : Object
        {
            foreach (var kvp in keyPoolCollection)
            {
                AddPool(kvp.Key, kvp.Value, asyncInit);
            }
        }

        public static ObjectPool<T> CreatePool<T>(Func<T> createFunc, Action<T> destroyAction, bool collectionCheck = false, int initialCapacity = 10, int maxCapacity = 10000, Action<T> onCreatePoolItem = null, Action<T> onTakePoolItem = null, Action<T> onReturnPoolItem = null, Action<T> onDestroyPoolItem = null) where T : class
        {
            return new ObjectPool<T>(createFunc, destroyAction, collectionCheck, initialCapacity, maxCapacity, onCreatePoolItem, onTakePoolItem, onReturnPoolItem, onDestroyPoolItem);
        }

        public static UnityObjectPool<T> CreatePool<T>(T objectPrefab, bool collectionCheck = false, int initialCapacity = 10, int maxCapacity = 10000, Action<T> onCreatePoolItem = null, Action<T> onTakePoolItem = null, Action<T> onReturnPoolItem = null, Action<T> onDestroyPoolItem = null) where T : Object
        {
            return new UnityObjectPool<T>(objectPrefab, collectionCheck, initialCapacity, maxCapacity, onCreatePoolItem, onTakePoolItem, onReturnPoolItem, onDestroyPoolItem);
        }

        public static ObjectPool<T> CreateThenAddPool<T>(string key, Func<T> createFunc, Action<T> destroyAction, bool collectionCheck = false, int initialCapacity = 10, int maxCapacity = 10000, Action<T> onCreatePoolItem = null, Action<T> onTakePoolItem = null, Action<T> onReturnPoolItem = null, Action<T> onDestroyPoolItem = null) where T : class
        {
            ObjectPool<T> pool = CreatePool(createFunc, destroyAction, collectionCheck, initialCapacity, maxCapacity, onCreatePoolItem, onTakePoolItem, onReturnPoolItem, onDestroyPoolItem);
            AddPool(key, pool);
            return pool;
        }

        public static UnityObjectPool<T> CreateThenAddPool<T>(string key, T objectPrefab, bool collectionCheck = false, int initialCapacity = 10, int maxCapacity = 10000, bool asyncInit = false, Action<T> onCreatePoolItem = null, Action<T> onTakePoolItem = null, Action<T> onReturnPoolItem = null, Action<T> onDestroyPoolItem = null) where T : Object
        {
            UnityObjectPool<T> pool = CreatePool(objectPrefab, collectionCheck, initialCapacity, maxCapacity, onCreatePoolItem, onTakePoolItem, onReturnPoolItem, onDestroyPoolItem);
            AddPool(key, pool, asyncInit);
            return pool;
        }

        public static KeyValuePair<string, UnityObjectPool<T>>[] CreateThenAddPools<T>(Dictionary<string, T> keyObjectPrefabDictionary, bool collectionCheck = false, int initialCapacity = 10, int maxCapacity = 10000, bool asyncInit = false, Action<T> onCreatePoolItem = null, Action<T> onTakePoolItem = null, Action<T> onReturnPoolItem = null, Action<T> onDestroyPoolItem = null) where T : Object
        {
            KeyValuePair<string, UnityObjectPool<T>>[] keyPoolPairs = new KeyValuePair<string, UnityObjectPool<T>>[keyObjectPrefabDictionary.Count];
            int i = 0;
            foreach (var kvp in keyObjectPrefabDictionary)
            {
                UnityObjectPool<T> pool = CreateThenAddPool(kvp.Key, kvp.Value, collectionCheck, initialCapacity, maxCapacity, asyncInit, onCreatePoolItem, onTakePoolItem, onReturnPoolItem, onDestroyPoolItem);
                keyPoolPairs[i++] = new KeyValuePair<string, UnityObjectPool<T>>(kvp.Key, pool);
            }
            return keyPoolPairs;
        }

        public static UnityObjectPool<T> GetOrCreatePool<T>(string key, T objectPrefab, bool collectionCheck = false, int initialCapacity = 10, int maxCapacity = 10000, bool asyncInit = false, Action<T> onCreatePoolItem = null, Action<T> onTakePoolItem = null, Action<T> onReturnPoolItem = null, Action<T> onDestroyPoolItem = null) where T : Object
        {
            if (!ContainsPool(key))
            {
                return CreateThenAddPool(key, objectPrefab, collectionCheck, initialCapacity, maxCapacity, asyncInit, onCreatePoolItem, onTakePoolItem, onReturnPoolItem, onDestroyPoolItem);
            }
            else
            {
                return GetPool<UnityObjectPool<T>, T>(key);
            }
        }

        public static UnityObjectPool<T> GetOrCreatePool<T>(T objectPrefab, bool collectionCheck = false, int initialCapacity = 10, int maxCapacity = 10000, bool asyncInit = false, Action<T> onCreatePoolItem = null, Action<T> onTakePoolItem = null, Action<T> onReturnPoolItem = null, Action<T> onDestroyPoolItem = null) where T : Object
        {
            return GetOrCreatePool(objectPrefab.GetInstanceID().ToString(), objectPrefab, collectionCheck, initialCapacity, maxCapacity, asyncInit, onCreatePoolItem, onTakePoolItem, onReturnPoolItem, onDestroyPoolItem);
        }

        public static ObjectPool GetPool(string key)
        {
            Initialize();
            return s_RuntimePoolDictionary[key];
        }

        public static T1 GetPool<T1, T2>(string key) where T1 : ObjectPool<T2> where T2 : class
        {
            Initialize();
            return s_RuntimePoolDictionary[key] as T1;
        }

        public static T1 GetPool<T1, T2>(GameObject objectPrefab) where T1 : ObjectPool<T2> where T2 : class
        {
            return GetPool<T1, T2>(objectPrefab.GetInstanceID().ToString());
        }

        public static T1 GetPool<T1, T2>(Component componentPrefab) where T1 : ObjectPool<T2> where T2 : class
        {
            if (ContainsPool(componentPrefab.GetInstanceID().ToString()))
                return GetPool<T1, T2>(componentPrefab.GetInstanceID().ToString());
            return GetPool<T1, T2>(componentPrefab.gameObject);
        }

        public static ObjectPool<T> GetPool<T>(string key) where T : class
        {
            return GetPool<ObjectPool<T>, T>(key);
        }

        public static T Get<T>(string key) where T : class
        {
            Initialize();
            if (typeof(T).IsSubclassOf(typeof(Component)))
            {
                object item = s_RuntimePoolDictionary[key].Get<object>();
                if (item is GameObject gameObject)
                    return gameObject.GetComponent<T>();
                return item as T;
            }
            return s_RuntimePoolDictionary[key].Get<T>();
        }

        public static T Get<T>(GameObject objectPrefab) where T : class
        {
            return Get<T>(objectPrefab.GetInstanceID().ToString());
        }

        public static T Get<T>(Component componentPrefab) where T : class
        {
            if (ContainsPool(componentPrefab.GetInstanceID().ToString()))
                return Get<T>(componentPrefab.GetInstanceID().ToString());
            return Get<T>(componentPrefab.gameObject);
        }

        public static void Release<T>(string key, T item) where T : class
        {
            Initialize();
            s_RuntimePoolDictionary[key].Release(item);
        }

        public static void Release<T>(GameObject objectPrefab, T item) where T : class
        {
            Release(objectPrefab.GetInstanceID().ToString(), item);
        }

        public static void Release<T>(Component componentPrefab, T item) where T : class
        {
            if (ContainsPool(componentPrefab.GetInstanceID().ToString()))
                Release(componentPrefab.GetInstanceID().ToString(), item);
            else
                Release(componentPrefab.gameObject, item);
        }

        public virtual int Count(string key)
        {
            Initialize();
            int count = 0;
            if (s_RuntimePoolDictionary.TryGetValue(key, out ObjectPool pool))
            {
                count = pool.count;
            }
            return count;
        }

        public static int CountAll()
        {
            Initialize();
            int count = 0;
            foreach (var kvp in s_RuntimePoolDictionary)
            {
                count += kvp.Value.count;
            }
            return count;
        }

        public virtual int CountActive(string key)
        {
            Initialize();
            int count = 0;
            if (s_RuntimePoolDictionary.TryGetValue(key, out ObjectPool pool))
            {
                count = pool.countActive;
            }
            return count;
        }

        public static int CountActiveAll()
        {
            Initialize();
            int count = 0;
            foreach (var kvp in s_RuntimePoolDictionary)
            {
                count += kvp.Value.countActive;
            }
            return count;
        }

        public static int CountInactive(string key)
        {
            Initialize();
            int count = 0;
            if (s_RuntimePoolDictionary.TryGetValue(key, out ObjectPool pool))
            {
                count = pool.countInactive;
            }
            return count;
        }

        public static int CountInactiveAll()
        {
            Initialize();
            int count = 0;
            foreach (var kvp in s_RuntimePoolDictionary)
            {
                count += kvp.Value.countInactive;
            }
            return count;
        }

        public static void Clear()
        {
            if (!IsInitialized())
                return;
            foreach (var kvp in s_RuntimePoolDictionary)
            {
                ObjectPool pool = kvp.Value;
                pool.Clear();
            }
            s_RuntimePoolDictionary.Clear();
            if (Instance.m_Verbose)
                LGDebug.Log($"Clear all pools", k_DebugTag);
        }

        public static void Clear(string key)
        {
            if (!IsInitialized())
                return;
            if (s_RuntimePoolDictionary.TryGetValue(key, out ObjectPool pool))
            {
                pool.Clear();
                s_RuntimePoolDictionary.Remove(key);
            }
            if (Instance.m_Verbose)
                LGDebug.Log($"Clear pool with key [{key}]", k_DebugTag);
        }
    }
}