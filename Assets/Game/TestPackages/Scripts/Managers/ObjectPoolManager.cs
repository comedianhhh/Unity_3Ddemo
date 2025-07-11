using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Core
{
    /// <summary>
    /// 对象池管理器 - 负责管理游戏中的对象复用
    /// </summary>
    public class ObjectPoolManager : MonoBehaviour, IObjectPoolManager
    {
        [System.Serializable]
        public class PoolSetup
        {
            [Tooltip("池的名称，用于获取对象时的标识")]
            public string poolName;
            
            [Tooltip("要池化的预制体")]
            public GameObject prefab;
            
            [Tooltip("初始化时预创建的对象数量")]
            public int initialSize = 10;
            
            [Tooltip("是否在初始化时预热池")]
            public bool prewarmOnStart = true;
            
            [Tooltip("池的最大容量，0表示无限制")]
            public int maxCapacity = 50;
        }
        
        [Header("对象池配置")]
        [SerializeField] private PoolSetup[] poolSetups;
        
        [Header("调试设置")]
        [SerializeField] private bool enableDebugLog;
        [SerializeField] private bool showPoolStats;
        
        // 对象池字典
        private readonly Dictionary<string, ObjectPool> _pools = new Dictionary<string, ObjectPool>();
        
        // 对象池类
        private class ObjectPool
        {
            public GameObject prefab;
            public Queue<GameObject> available = new Queue<GameObject>();
            public List<GameObject> active = new List<GameObject>();
            public Transform parent;
            public int maxCapacity;
            
            public int TotalCount => available.Count + active.Count;
            public int AvailableCount => available.Count;
            public int ActiveCount => active.Count;
        }
        
        public void Initialize()
        {
            Debug.Log("初始化对象池管理器...");
            
            // 创建根节点
            CreatePoolParents();
            
            // 初始化预配置的池
            foreach (var setup in poolSetups)
            {
                CreatePool(setup);
            }
            
            Debug.Log($"对象池管理器初始化完成，共创建 {_pools.Count} 个对象池");
        }
        
        /// <summary>
        /// 创建池的父节点
        /// </summary>
        private void CreatePoolParents()
        {
            // 如果不存在Pool根节点，创建一个
            var poolRoot = transform.Find("ObjectPools");
            if (poolRoot == null)
            {
                var poolRootGO = new GameObject("ObjectPools");
                poolRootGO.transform.SetParent(transform);
            }
        }
        
        /// <summary>
        /// 创建单个对象池
        /// </summary>
        private void CreatePool(PoolSetup setup)
        {
            if (string.IsNullOrEmpty(setup.poolName))
            {
                Debug.LogError("对象池名称不能为空");
                return;
            }
            
            if (setup.prefab == null)
            {
                Debug.LogError($"对象池 '{setup.poolName}' 的预制体为空");
                return;
            }
            
            if (_pools.ContainsKey(setup.poolName))
            {
                Debug.LogWarning($"对象池 '{setup.poolName}' 已存在");
                return;
            }
            
            // 创建池的父节点
            var poolParent = new GameObject($"Pool_{setup.poolName}");
            poolParent.transform.SetParent(transform.Find("ObjectPools"));
            
            // 创建对象池
            var pool = new ObjectPool
            {
                prefab = setup.prefab,
                parent = poolParent.transform,
                maxCapacity = setup.maxCapacity
            };
            
            _pools[setup.poolName] = pool;
            
            // 预热池
            if (setup.prewarmOnStart)
            {
                PrewarmPool(setup.poolName, setup.initialSize);
            }
            
            if (enableDebugLog)
            {
                Debug.Log($"创建对象池 '{setup.poolName}'，初始大小: {setup.initialSize}");
            }
        }
        
        public GameObject Get(string poolName)
        {
            if (!_pools.TryGetValue(poolName, out var pool))
            {
                Debug.LogError($"对象池 '{poolName}' 不存在");
                return null;
            }

            GameObject obj;
            
            if (pool.available.Count > 0)
            {
                // 从池中获取可用对象
                obj = pool.available.Dequeue();
            }
            else
            {
                // 池中没有可用对象，创建新的
                obj = Instantiate(pool.prefab, pool.parent);
                
                if (enableDebugLog)
                {
                    Debug.Log($"池 '{poolName}' 中没有可用对象，创建新对象");
                }
            }
            
            // 激活对象并添加到活跃列表
            obj.SetActive(true);
            pool.active.Add(obj);
            
            return obj;
        }
        
        public void Return(string poolName, GameObject obj)
        {
            if (obj == null)
            {
                Debug.LogWarning("尝试归还空对象到对象池");
                return;
            }
            
            if (!_pools.TryGetValue(poolName, out var pool))
            {
                Debug.LogError($"对象池 '{poolName}' 不存在");
                Destroy(obj);
                return;
            }

            // 从活跃列表中移除
            if (pool.active.Remove(obj))
            {
                // 重置对象状态
                obj.SetActive(false);
                obj.transform.SetParent(pool.parent);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                
                // 检查池容量限制
                if (pool.maxCapacity <= 0 || pool.available.Count < pool.maxCapacity)
                {
                    pool.available.Enqueue(obj);
                }
                else
                {
                    // 池已满，销毁对象
                    Destroy(obj);
                    
                    if (enableDebugLog)
                    {
                        Debug.Log($"池 '{poolName}' 已满，销毁归还的对象");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"对象不属于池 '{poolName}' 的活跃对象列表");
                Destroy(obj);
            }
        }
        
        public void PrewarmPool(string poolName, int count)
        {
            if (!_pools.TryGetValue(poolName, out var pool))
            {
                Debug.LogError($"对象池 '{poolName}' 不存在");
                return;
            }

            for (int i = 0; i < count; i++)
            {
                var obj = Instantiate(pool.prefab, pool.parent);
                obj.SetActive(false);
                pool.available.Enqueue(obj);
            }
            
            if (enableDebugLog)
            {
                Debug.Log($"预热池 '{poolName}'，创建了 {count} 个对象");
            }
        }
        
        public void ClearPool(string poolName)
        {
            if (!_pools.TryGetValue(poolName, out var pool))
            {
                Debug.LogError($"对象池 '{poolName}' 不存在");
                return;
            }

            // 销毁所有可用对象
            while (pool.available.Count > 0)
            {
                var obj = pool.available.Dequeue();
                if (obj != null)
                    Destroy(obj);
            }
            
            // 销毁所有活跃对象
            for (int i = pool.active.Count - 1; i >= 0; i--)
            {
                if (pool.active[i] != null)
                    Destroy(pool.active[i]);
            }
            pool.active.Clear();
            
            if (enableDebugLog)
            {
                Debug.Log($"清空池 '{poolName}'");
            }
        }
        
        public void ClearAllPools()
        {
            foreach (var poolName in _pools.Keys.ToList())
            {
                ClearPool(poolName);
            }
            
            Debug.Log("清空所有对象池");
        }
        
        public int GetPoolSize(string poolName)
        {
            if (_pools.TryGetValue(poolName, out var pool))
                return pool.TotalCount;
            return -1;
        }
        
        public int GetActiveCount(string poolName)
        {
            if (_pools.TryGetValue(poolName, out var pool))
                return pool.ActiveCount;
            return -1;
        }
        
        private void OnDestroy()
        {
            ClearAllPools();
        }
        
        #if UNITY_EDITOR
        [ContextMenu("显示池统计")]
        private void ShowPoolStats()
        {
            Debug.Log("=== 对象池统计 ===");
            foreach (var kvp in _pools)
            {
                var pool = kvp.Value;
                Debug.Log($"池 '{kvp.Key}': 总计 {pool.TotalCount}, 可用 {pool.AvailableCount}, 活跃 {pool.ActiveCount}");
            }
        }
        
        [ContextMenu("清空所有池")]
        private void ClearAllPoolsDebug()
        {
            ClearAllPools();
        }
        #endif
        
        private void OnGUI()
        {
            if (!showPoolStats || !Application.isPlaying)
                return;
            
            GUILayout.BeginArea(new Rect(10, 100, 300, 400));
            GUILayout.Label("=== 对象池统计 ===", new GUIStyle { fontSize = 14, normal = { textColor = Color.white } });
            
            foreach (var kvp in _pools)
            {
                var pool = kvp.Value;
                GUILayout.Label($"{kvp.Key}: 总计 {pool.TotalCount}, 可用 {pool.AvailableCount}, 活跃 {pool.ActiveCount}", 
                    new GUIStyle { fontSize = 12, normal = { textColor = Color.white } });
            }
            
            GUILayout.EndArea();
        }
    }
} 