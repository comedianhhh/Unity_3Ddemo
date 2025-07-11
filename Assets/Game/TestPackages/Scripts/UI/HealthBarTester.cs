using UnityEngine;

namespace Core.UI
{
    /// <summary>
    /// 血条测试器 - 用于快速测试血条系统的功能
    /// </summary>
    public class HealthBarTester : MonoBehaviour
    {
        [Header("测试设置")]
        [Tooltip("测试伤害量")]
        [Range(1, 50)]
        public int testDamage = 10;
        
        [Tooltip("测试治疗量")]
        [Range(1, 50)]
        public int testHeal = 15;
        
        [Tooltip("是否显示测试UI")]
        public bool showTestUI = true;
        
        [Header("快捷键设置")]
        [Tooltip("伤害测试快捷键")]
        public KeyCode damageKey = KeyCode.X;
        
        [Tooltip("治疗测试快捷键")]
        public KeyCode healKey = KeyCode.C;
        
        [Tooltip("刷新血条快捷键")]
        public KeyCode refreshKey = KeyCode.R;
        
        private HealthBarManager healthBarManager;
        private Health[] allHealthComponents;
        
        private void Start()
        {
            healthBarManager = FindFirstObjectByType<HealthBarManager>();
            if (healthBarManager == null)
            {
                Debug.LogWarning("HealthBarTester: 未找到HealthBarManager，血条测试功能将不可用");
            }
            
            RefreshHealthComponents();
        }
        
        private void Update()
        {
            HandleKeyboardInput();
        }
        
        private void HandleKeyboardInput()
        {
            if (Input.GetKeyDown(damageKey))
            {
                DamageRandomTarget();
            }
            
            if (Input.GetKeyDown(healKey))
            {
                HealRandomTarget();
            }
            
            if (Input.GetKeyDown(refreshKey))
            {
                RefreshHealthBars();
            }
        }
        
        /// <summary>
        /// 刷新Health组件列表
        /// </summary>
        public void RefreshHealthComponents()
        {
            allHealthComponents = FindObjectsByType<Health>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            
            Debug.Log($"HealthBarTester: 发现 {allHealthComponents.Length} 个Health组件");
        }
        
        /// <summary>
        /// 对随机目标造成伤害
        /// </summary>
        public void DamageRandomTarget()
        {
            if (allHealthComponents == null || allHealthComponents.Length == 0)
            {
                RefreshHealthComponents();
                return;
            }
            
            var randomHealth = allHealthComponents[Random.Range(0, allHealthComponents.Length)];
            if (randomHealth != null && randomHealth.CurrentHealth > 0)
            {
                randomHealth.TakeDamage(testDamage);
                Debug.Log($"对 {randomHealth.gameObject.name} 造成 {testDamage} 点伤害，剩余HP: {randomHealth.CurrentHealth}/{randomHealth.MaxHealth}");
            }
        }
        
        /// <summary>
        /// 对随机目标进行治疗
        /// </summary>
        public void HealRandomTarget()
        {
            if (allHealthComponents == null || allHealthComponents.Length == 0)
            {
                RefreshHealthComponents();
                return;
            }
            
            var randomHealth = allHealthComponents[Random.Range(0, allHealthComponents.Length)];
            if (randomHealth != null && randomHealth.CurrentHealth < randomHealth.MaxHealth)
            {
                randomHealth.Heal(testHeal);
                Debug.Log($"对 {randomHealth.gameObject.name} 治疗 {testHeal} 点生命，当前HP: {randomHealth.CurrentHealth}/{randomHealth.MaxHealth}");
            }
        }
        
        /// <summary>
        /// 刷新血条系统
        /// </summary>
        public void RefreshHealthBars()
        {
            if (healthBarManager != null)
            {
                healthBarManager.DetectAndCreateHealthBars();
                Debug.Log("已刷新血条系统");
            }
        }
        
        /// <summary>
        /// 伤害所有目标
        /// </summary>
        public void DamageAllTargets()
        {
            if (allHealthComponents == null || allHealthComponents.Length == 0)
            {
                RefreshHealthComponents();
                return;
            }
            
            int damagedCount = 0;
            foreach (var health in allHealthComponents)
            {
                if (health != null && health.CurrentHealth > 0)
                {
                    health.TakeDamage(testDamage);
                    damagedCount++;
                }
            }
            
            Debug.Log($"对 {damagedCount} 个目标造成了 {testDamage} 点伤害");
        }
        
        /// <summary>
        /// 治疗所有目标
        /// </summary>
        public void HealAllTargets()
        {
            if (allHealthComponents == null || allHealthComponents.Length == 0)
            {
                RefreshHealthComponents();
                return;
            }
            
            int healedCount = 0;
            foreach (var health in allHealthComponents)
            {
                if (health != null && health.CurrentHealth < health.MaxHealth)
                {
                    health.Heal(testHeal);
                    healedCount++;
                }
            }
            
            Debug.Log($"治疗了 {healedCount} 个目标 {testHeal} 点生命");
        }
        
        /// <summary>
        /// 重置所有目标的生命值
        /// </summary>
        public void ResetAllHealth()
        {
            if (allHealthComponents == null || allHealthComponents.Length == 0)
            {
                RefreshHealthComponents();
                return;
            }
            
            foreach (var health in allHealthComponents)
            {
                if (health != null)
                {
                    health.SetCurrentHealth(health.MaxHealth);
                }
            }
            
            Debug.Log("已重置所有目标的生命值");
        }
        
        private void OnGUI()
        {
            if (!showTestUI) return;
            
            GUILayout.BeginArea(new Rect(10, 400, 250, 300));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("血条测试器", GUI.skin.label);
            GUILayout.Space(10);
            
            // 快捷键提示
            GUILayout.Label($"快捷键:", GUI.skin.label);
            GUILayout.Label($"  {damageKey} - 随机伤害", GUI.skin.label);
            GUILayout.Label($"  {healKey} - 随机治疗", GUI.skin.label);
            GUILayout.Label($"  {refreshKey} - 刷新血条", GUI.skin.label);
            GUILayout.Space(10);
            
            GUILayout.Space(10);
            
            // 状态信息
            int healthCount = allHealthComponents?.Length ?? 0;
            int healthBarCount = healthBarManager?.GetActiveHealthBarCount() ?? 0;
            
            GUILayout.Label($"Health组件: {healthCount}", GUI.skin.label);
            GUILayout.Label($"活跃血条: {healthBarCount}", GUI.skin.label);
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        #if UNITY_EDITOR
        [ContextMenu("测试伤害")]
        private void TestDamage()
        {
            DamageRandomTarget();
        }
        
        [ContextMenu("测试治疗")]
        private void TestHeal()
        {
            HealRandomTarget();
        }
        
        [ContextMenu("显示统计")]
        private void ShowStatistics()
        {
            RefreshHealthComponents();
            Debug.Log($"=== 血条测试器统计 ===");
            Debug.Log($"Health组件数量: {allHealthComponents?.Length ?? 0}");
            Debug.Log($"血条管理器: {(healthBarManager != null ? "存在" : "缺失")}");
            
            if (healthBarManager != null)
            {
                Debug.Log($"活跃血条数量: {healthBarManager.GetActiveHealthBarCount()}");
            }
        }
        #endif
    }
} 