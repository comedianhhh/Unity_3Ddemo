using UnityEngine;

namespace Core.ScriptableObjects
{
    /// <summary>
    /// 武器配置数据 - 用于存储武器的各种属性和设置
    /// </summary>
    [CreateAssetMenu(fileName = "New Weapon Config", menuName = "Game Config/Weapon Config", order = 1)]
    public class WeaponConfig : ScriptableObject
    {
        [Header("基础信息")]
        [Tooltip("武器名称")]
        public string weaponName = "Default Weapon";
        
        [Header("战斗属性")]
        [Tooltip("基础伤害")]
        [Range(1, 1000)]
        public int damage = 20;
        
        [Tooltip("投掷力度")]
        [Range(1f, 50f)]
        public float throwForce = 10f;
        
        [Tooltip("爆炸半径")]
        [Range(0f, 20f)]
        public float explosionRadius = 5f;
        
        [Tooltip("引爆延迟")]
        [Range(0f, 10f)]
        public float fuseTime = 3f;
        
        [Header("视觉效果")]
        [Tooltip("武器3D模型预制体")]
        public GameObject weaponPrefab;
        
        [Tooltip("武器模型缩放倍数")]
        [Range(0.1f, 5f)]
        public float weaponScale = 1f;
        
        [Tooltip("爆炸特效")]
        public GameObject explosionEffect;
        
        
        /// <summary>
        /// 获取实际伤害值（可以根据等级、技能等进行修正）
        /// </summary>
        public int GetActualDamage(float damageMultiplier = 1f)
        {
            return Mathf.RoundToInt(damage * damageMultiplier);
        }
        
        
        /// <summary>
        /// 验证配置是否有效
        /// </summary>
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(weaponName)) return false;
            if (damage <= 0) return false;
            
            return true;
        }
        
        #if UNITY_EDITOR
        [ContextMenu("验证配置")]
        private void ValidateConfig()
        {
            if (IsValid())
            {
                Debug.Log($"武器配置 '{weaponName}' 验证通过");
            }
            else
            {
                Debug.LogError($"武器配置 '{weaponName}' 验证失败");
            }
        }
        
        [ContextMenu("显示配置信息")]
        private void ShowConfigInfo()
        {
            Debug.Log($"=== {weaponName} 配置信息 ===");
            Debug.Log($"伤害: {damage}");
            Debug.Log($"3D模型: {(weaponPrefab != null ? weaponPrefab.name : "使用默认模型")}");
            Debug.Log($"模型缩放: {weaponScale}");
            

        }
        #endif
    }
} 