using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LatteGames.PoolManagement
{
    [Serializable]
    public class PredefinedObjectPool
    {
        [SerializeField, InfoBox("The default pool key is the prefab gameObject�s InstanceID. Enable this toggle if you want to manually assign a custom key.")]
        private bool m_UseOverrideKey;
        [SerializeField, ShowIf("m_UseOverrideKey")]
        private string m_OverrideKey;
        [SerializeField]
        private UnityObjectPool<Object> m_ObjectPool;

        public string key => m_UseOverrideKey ? m_OverrideKey : m_ObjectPool.objectPrefab.GetInstanceID().ToString();
        public UnityObjectPool<Object> objectPool => m_ObjectPool;
    }

    [HideMonoScript]
    [GlobalConfig("Assets/Resources")]
    [WindowMenuItem("GlobalConfig", "Pool", "Assets/Resources")]
    public class GlobalPoolConfigSO : GlobalConfig<GlobalPoolConfigSO>
    {
        [SerializeField]
        private List<PredefinedObjectPool> m_PredefinedObjectPools = new List<PredefinedObjectPool>();

        public List<PredefinedObjectPool> predefinedObjectPools => m_PredefinedObjectPools;
    }
}