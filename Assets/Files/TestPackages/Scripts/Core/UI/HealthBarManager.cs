using UnityEngine;
using System.Collections.Generic;
using TMPro;
namespace Core.UI
{
    /// <summary>
    /// 血条管理器 - 自动为游戏单位创建和管理头顶血条
    /// </summary>
    public class HealthBarManager : MonoBehaviour
    {
        [Header("血条预制体")]
        [Tooltip("血条预制体")]
        public GameObject healthBarPrefab;
        
        [Header("管理设置")]
        [Tooltip("是否自动检测Health组件")]
        public bool autoDetectHealth = true;
        
        [Tooltip("检测间隔时间")]
        [Range(0.5f, 5f)]
        public float detectionInterval = 1f;
        
        [Tooltip("最大血条数量")]
        [Range(10, 200)]
        public int maxHealthBars = 50;
        
        [Header("过滤设置")]
        [Tooltip("为玩家创建血条")]
        public bool createForPlayer = false;
        
        [Tooltip("为敌人创建血条")]
        public bool createForEnemies = true;
        
        [Tooltip("需要包含的标签")]
        public string[] includeTags = { "Enemy" };
        
        [Tooltip("需要排除的标签")]
        public string[] excludeTags = { };
        
        // 私有变量
        private Dictionary<Health, WorldSpaceHealthBar> activeHealthBars = new Dictionary<Health, WorldSpaceHealthBar>();
        private Transform healthBarContainer;
        private float lastDetectionTime;
        
        private void Awake()
        {
            // 创建血条容器
            CreateHealthBarContainer();
            
            // 如果没有设置预制体，创建默认的
            if (healthBarPrefab == null)
            {
                Debug.LogWarning("[HealthBarManager] 未设置血条预制体");
            }
        }
        
        private void OnEnable()
        {
            // 订阅事件
            EventManager.Subscribe<EnemyDeathEvent>(OnEnemyDeath);
            EventManager.Subscribe<PlayerDeathEvent>(OnPlayerDeath);
            
            // 初始检测
            if (autoDetectHealth)
            {
                DetectAndCreateHealthBars();
            }
        }
        
        private void OnDisable()
        {
            // 取消订阅事件
            if (EventManager.IsInstance())
            {
                EventManager.Unsubscribe<EnemyDeathEvent>(OnEnemyDeath);
                EventManager.Unsubscribe<PlayerDeathEvent>(OnPlayerDeath);
            }
        }
        
        private void Update()
        {
            if (autoDetectHealth && Time.time - lastDetectionTime >= detectionInterval)
            {
                DetectAndCreateHealthBars();
                CleanupDestroyedHealthBars();
                lastDetectionTime = Time.time;
            }
        }
        
        /// <summary>
        /// 创建血条容器
        /// </summary>
        private void CreateHealthBarContainer()
        {
            GameObject container = new GameObject("HealthBars Container");
            container.transform.SetParent(transform);
            healthBarContainer = container.transform;
        }
        /// <summary>
        /// 检测并创建血条
        /// </summary>
        public void DetectAndCreateHealthBars()
        {
            if (activeHealthBars.Count >= maxHealthBars)
            {
                return;
            }

            Health[] allHealthComponents = FindObjectsByType<Health>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            
            foreach (Health health in allHealthComponents)
            {
                if (!activeHealthBars.ContainsKey(health) && ShouldCreateHealthBar(health))
                {
                    CreateHealthBar(health);
                }
            }
        }
        
        /// <summary>
        /// 判断是否应该为此Health组件创建血条
        /// </summary>
        private bool ShouldCreateHealthBar(Health health)
        {
            GameObject target = health.gameObject;
            
            // 检查玩家
            if (target.CompareTag("Player") && !createForPlayer)
            {
                return false;
            }
            
            // 检查包含标签
            bool hasIncludeTag = includeTags.Length == 0;
            foreach (string tag in includeTags)
            {
                if (target.CompareTag(tag))
                {
                    hasIncludeTag = true;
                    break;
                }
            }
            
            if (!hasIncludeTag)
            {
                return false;
            }
            
            // 检查排除标签
            foreach (string tag in excludeTags)
            {
                if (target.CompareTag(tag))
                {
                    return false;
                }
            }
            
            // 检查特殊组件
            if (target.GetComponent<MonsterAI>() != null && !createForEnemies)
            {
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 为指定Health组件创建血条
        /// </summary>
        public void CreateHealthBar(Health health)
        {
            if (health == null || healthBarPrefab == null)
            {
                return;
            }
            
            // 实例化血条
            GameObject healthBarInstance = Instantiate(healthBarPrefab, healthBarContainer);
            healthBarInstance.SetActive(true);
            
            // 获取血条组件
            WorldSpaceHealthBar healthBar = healthBarInstance.GetComponent<WorldSpaceHealthBar>();
            if (healthBar == null)
            {
                healthBar = healthBarInstance.AddComponent<WorldSpaceHealthBar>();
            }
            
            // 初始化血条
            healthBar.Initialize(health, health.transform);
            
            // 添加到字典
            activeHealthBars[health] = healthBar;
            
            Debug.Log($"为 {health.gameObject.name} 创建了血条"); ;
        }
        
        /// <summary>
        /// 移除指定Health组件的血条
        /// </summary>
        public void RemoveHealthBar(Health health)
        {
            if (activeHealthBars.TryGetValue(health, out WorldSpaceHealthBar healthBar))
            {
                if (healthBar != null)
                {
                    Destroy(healthBar.gameObject);
                }
                activeHealthBars.Remove(health);
            }
        }
        
        /// <summary>
        /// 清理已销毁的血条
        /// </summary>
        private void CleanupDestroyedHealthBars()
        {
            var keysToRemove = new List<Health>();
            
            foreach (var kvp in activeHealthBars)
            {
                if (kvp.Key == null || kvp.Value == null)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }
            
            foreach (var key in keysToRemove)
            {
                if (activeHealthBars[key] != null)
                {
                    Destroy(activeHealthBars[key].gameObject);
                }
                activeHealthBars.Remove(key);
            }
        }
        
        /// <summary>
        /// 清理所有血条
        /// </summary>
        public void ClearAllHealthBars()
        {
            foreach (var healthBar in activeHealthBars.Values)
            {
                if (healthBar != null)
                {
                    Destroy(healthBar.gameObject);
                }
            }
            
            activeHealthBars.Clear();
        }
        
        /// <summary>
        /// 获取当前活跃的血条数量
        /// </summary>
        public int GetActiveHealthBarCount()
        {
            return activeHealthBars.Count;
        }
        
        // 事件处理
        private void OnEnemyDeath(EnemyDeathEvent eventData)
        {
            // 查找对应的Health组件并移除血条
            Health[] healthComponents = FindObjectsByType<Health>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (Health health in healthComponents)
            {
                if (Vector3.Distance(health.transform.position, eventData.position) < 0.5f)
                {
                    RemoveHealthBar(health);
                    break;
                }
            }
        }
        
        private void OnPlayerDeath(PlayerDeathEvent eventData)
        {
            Health playerHealth = FindFirstObjectByType<Health>();
            if (playerHealth != null && playerHealth.CompareTag("Player"))
            {
                RemoveHealthBar(playerHealth);
            }
        }
        
        #if UNITY_EDITOR
        [ContextMenu("手动检测血条")]
        private void ManualDetectHealthBars()
        {
            DetectAndCreateHealthBars();
        }
        
        [ContextMenu("清理所有血条")]
        private void ManualClearHealthBars()
        {
            ClearAllHealthBars();
        }
        
        [ContextMenu("显示血条统计")]
        private void ShowHealthBarStats()
        {
            Debug.Log($"当前活跃血条数量: {activeHealthBars.Count}/{maxHealthBars}");
        }
        #endif
    }
} 