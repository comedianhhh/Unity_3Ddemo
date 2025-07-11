using UnityEngine;

namespace Core.ScriptableObjects
{
    /// <summary>
    /// 怪物配置数据 - 用于存储怪物的各种属性和AI行为设置
    /// </summary>
    [CreateAssetMenu(fileName = "New Monster Config", menuName = "Game Config/Monster Config", order = 2)]
    public class MonsterConfig : ScriptableObject
    {
        [Header("基础信息")]
        [Tooltip("怪物名称")]
        public string monsterName = "Default Monster";
        
        
        [Header("生命值设置")]
        [Tooltip("最大生命值")]
        [Range(1, 10000)]
        public int maxHealth = 100;
        
        [Tooltip("生命值再生速度（每秒）")]
        [Range(0f, 100f)]
        public float healthRegenRate = 0f;
        
        [Header("AI行为设置")]
        [Tooltip("玩家检测范围")]
        [Range(1f, 50f)]
        public float detectionRange = 10f;
        
        [Tooltip("攻击范围")]
        [Range(0.5f, 20f)]
        public float attackRange = 2f;
        
        [Tooltip("移动速度")]
        [Range(0.1f, 20f)]
        public float moveSpeed = 3.5f;
        
        [Tooltip("追击速度")]
        [Range(0.1f, 20f)]
        public float chaseSpeed = 5f;
        
        [Tooltip("巡逻速度")]
        [Range(0.1f, 15f)]
        public float patrolSpeed = 2f;
        
        [Tooltip("空闲持续时间")]
        [Range(0f, 30f)]
        public float idleDuration = 5f;
        
        [Tooltip("路径点变更阈值")]
        [Range(0.1f, 5f)]
        public float waypointChangeThreshold = 1f;
        
        [Tooltip("战斗属性")]
        [System.Serializable]
        public class AttackData
        {
            [Tooltip("攻击动画触发器名称")]
            public string animationTrigger = "attack1";
            
            [Tooltip("攻击伤害")]
            [Range(1, 1000)]
            public int damage = 10;
            
            [Tooltip("攻击冷却时间")]
            [Range(0.1f, 10f)]
            public float cooldown = 1.5f;
            
            [Tooltip("攻击范围")]
            [Range(0.5f, 10f)]
            public float range = 2f;
            
            [Tooltip("攻击前摇时间")]
            [Range(0f, 3f)]
            public float windupTime = 0.3f;
            
            [Tooltip("攻击特效")]
            public string attackEffect;
        }
        
        [Tooltip("攻击类型列表")]
        public AttackData[] attacks;
        
        [Tooltip("伤害减免百分比")]
        [Range(0f, 0.9f)]
        public float damageReduction = 0f;
        
        [Header("视觉")]
        [Tooltip("怪物3D模型预制体")]
        public GameObject modelPrefab;
        
        [Tooltip("模型缩放倍数")]
        [Range(0.1f, 5f)]
        public float modelScale = 1f;
        
        
        [Header("动画设置")]
        [Tooltip("移动动画参数名")]
        public string moveAnimationParameter = "isMoving";
        
        [Tooltip("攻击动画参数名")]
        public string attackAnimationParameter = "attack";
        
        [Tooltip("死亡动画参数名")]
        public string deathAnimationParameter = "death";
    
        [Header("AI个性化设置")]
        [Tooltip("aggression(侵略性) - 影响主动攻击倾向")]
        [Range(0f, 1f)]
        public float aggression = 0.5f;
        
        [Tooltip("警觉性 - 影响检测玩家的能力")]
        [Range(0f, 1f)]
        public float alertness = 0.5f;
        
        [Tooltip("坚韧性 - 影响逃跑倾向")]
        [Range(0f, 1f)]
        public float tenacity = 0.8f;
        
        /// <summary>
        /// 获取随机攻击数据
        /// </summary>
        public AttackData GetRandomAttack()
        {
            if (attacks == null || attacks.Length == 0)
                return null;
                
            return attacks[Random.Range(0, attacks.Length)];
        }
        
        /// <summary>
        /// 根据名称获取攻击数据
        /// </summary>
        public AttackData GetAttack(string triggerName)
        {
            if (attacks == null) return null;
            
            foreach (var attack in attacks)
            {
                if (attack.animationTrigger == triggerName)
                    return attack;
            }
            
            return null;
        }
        
        
        /// <summary>
        /// 计算实际伤害（考虑伤害减免）
        /// </summary>
        public int CalculateActualDamage(int incomingDamage)
        {
            float reducedDamage = incomingDamage * (1f - damageReduction);
            return Mathf.RoundToInt(Mathf.Max(1, reducedDamage));
        }
        
        /// <summary>
        /// 验证配置是否有效
        /// </summary>
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(monsterName)) return false;
            if (maxHealth <= 0) return false;
            if (detectionRange <= 0) return false;
            if (attackRange <= 0) return false;
            if (moveSpeed <= 0) return false;
            
            // 验证攻击数据
            if (attacks != null)
            {
                foreach (var attack in attacks)
                {
                    if (attack.damage <= 0) return false;
                    if (attack.cooldown <= 0) return false;
                }
            }
            
            return true;
        }
        
        #if UNITY_EDITOR
        [ContextMenu("验证配置")]
        private void ValidateConfig()
        {
            if (IsValid())
            {
                Debug.Log($"怪物配置 '{monsterName}' 验证通过");
            }
            else
            {
                Debug.LogError($"怪物配置 '{monsterName}' 验证失败");
            }
        }
        
        [ContextMenu("显示配置信息")]
        private void ShowConfigInfo()
        {
            Debug.Log($"=== {monsterName} 配置信息 ===");
            Debug.Log($"生命值: {maxHealth}");
            Debug.Log($"检测范围: {detectionRange}m");
            Debug.Log($"攻击范围: {attackRange}m");
            Debug.Log($"移动速度: {moveSpeed}");
            Debug.Log($"伤害减免: {damageReduction * 100}%");
            Debug.Log($"攻击类型数量: {(attacks?.Length ?? 0)}");
            Debug.Log($"侵略性: {aggression}");
            Debug.Log($"警觉性: {alertness}");
            Debug.Log($"坚韧性: {tenacity}");
            Debug.Log($"3D模型: {(modelPrefab != null ? modelPrefab.name : "使用默认模型")}");
            Debug.Log($"模型缩放: {modelScale}");
        }
        
        [ContextMenu("创建默认攻击")]
        private void CreateDefaultAttack()
        {
            attacks = new AttackData[]
            {
                new AttackData
                {
                    animationTrigger = "attack1",
                    damage = 10,
                    cooldown = 1.5f,
                    range = 2f,
                    windupTime = 0.3f
                }
            };
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }
        #endif
    }
} 