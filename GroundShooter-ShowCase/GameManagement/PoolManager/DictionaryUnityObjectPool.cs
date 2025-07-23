using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using HyrphusQ.SerializedDataStructure;

namespace LatteGames.PoolManagement
{
    [Serializable]
    public class DictionaryUnityObjectPool<TKey, TValue> where TValue : UnityEngine.Object
    {
        protected const string k_NotInitYetExceptionMessage = "Pool has not been initialized yet!";

        [SerializeField]
        protected SerializedDictionary<TKey, UnityObjectPool<TValue>> m_ObjectPoolDictionary = new SerializedDictionary<TKey, UnityObjectPool<TValue>>();
        protected Transform m_AnchoredTransform;

        public bool isInitialized => m_ObjectPoolDictionary.All(kvp => kvp.Value.isInitialized);
        public int dictionaryCount => objectPoolDictionary.Count;
        public int count => objectPoolDictionary.Sum(keyValuePair => keyValuePair.Value.count);
        public int countActive => objectPoolDictionary.Sum(keyValuePair => keyValuePair.Value.countActive);
        public int countInactive => objectPoolDictionary.Sum(keyValuePair => keyValuePair.Value.countInactive);
        public ICollection<TKey> keys => objectPoolDictionary.Keys;
        public ICollection<UnityObjectPool<TValue>> pools => objectPoolDictionary.Values;

        protected SerializedDictionary<TKey, UnityObjectPool<TValue>> objectPoolDictionary
        {
            get
            {
                if (!isInitialized)
                    throw new Exception(k_NotInitYetExceptionMessage);
                return m_ObjectPoolDictionary;
            }
        }

        public virtual void InitializePool(Transform anchoredTransform, Action<TKey, TValue> onCreatePoolItem = null, Action<TKey, TValue> onTakePoolItem = null, Action<TKey, TValue> onReturnPoolItem = null, Action<TKey, TValue> onDestroyPoolItem = null)
        {
            if (isInitialized)
                return;
            m_AnchoredTransform = anchoredTransform;
            foreach (var kvp in m_ObjectPoolDictionary)
            {
                TKey key = kvp.Key;
                kvp.Value.InitializePool(anchoredTransform, OnCreate, OnTake, OnReturn, OnDestroy);

                void OnCreate(TValue instance)
                {
                    onCreatePoolItem?.Invoke(key, instance);
                }
                void OnTake(TValue instance)
                {
                    onTakePoolItem?.Invoke(key, instance);
                }
                void OnReturn(TValue instance)
                {
                    onReturnPoolItem?.Invoke(key, instance);
                }
                void OnDestroy(TValue instance)
                {
                    onDestroyPoolItem?.Invoke(key, instance);
                }
            }
        }

        public virtual TValue Get(TKey key)
        {
            return objectPoolDictionary[key].Get();
        }

        public virtual UnityObjectPool<TValue> GetPool(TKey key)
        {
            return objectPoolDictionary[key];
        }

        public virtual void Release(TKey key, TValue item)
        {
            objectPoolDictionary[key].Release(item);

        }

        public virtual int Count(TKey key)
        {
            if (objectPoolDictionary.TryGetValue(key, out UnityObjectPool<TValue> pool))
            {
                return pool.count;
            }
            return 0;
        }

        public virtual int CountActive(TKey key)
        {
            if (objectPoolDictionary.TryGetValue(key, out UnityObjectPool<TValue> pool))
            {
                return pool.countActive;
            }
            return 0;
        }

        public virtual int CountInactive(TKey key)
        {
            if (objectPoolDictionary.TryGetValue(key, out UnityObjectPool<TValue> pool))
            {
                return pool.countInactive;
            }
            return 0;
        }

        public virtual void Clear()
        {
            foreach (var kvp in objectPoolDictionary)
            {
                var pool = kvp.Value;
                pool.Clear();
            }
            objectPoolDictionary.Clear();
        }

        public virtual void Clear(TKey key)
        {
            if (objectPoolDictionary.TryGetValue(key, out UnityObjectPool<TValue> pool))
            {
                pool.Clear();
                objectPoolDictionary.Remove(key);
            }
        }

        public static explicit operator Dictionary<TKey, UnityObjectPool<TValue>>(DictionaryUnityObjectPool<TKey, TValue> objectPool)
        {
            if (objectPool == null)
                return null;
            return objectPool.objectPoolDictionary;
        }
    }
}