using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// 特效管理器 - 负责管理游戏中的视觉特效
    /// </summary>
    public class EffectsManager : MonoBehaviour, IEffectsManager
    {
        [System.Serializable]
        public class EffectInfo
        {
            [Tooltip("特效名称标识")]
            public string name;
            
            [Tooltip("特效预制体")]
            public GameObject effectPrefab;
            
            [Tooltip("特效持续时间，0表示无限制")]
            public float duration = 2f;
            
            [Tooltip("是否使用对象池")]
            public bool useObjectPool = true;
            
            [Tooltip("是否自动销毁")]
            public bool autoDestroy = true;
        }
        
        [Header("特效配置")]
        [SerializeField] private EffectInfo[] effects;
        
        // 特效字典
        private Dictionary<string, EffectInfo> effectDict = new Dictionary<string, EffectInfo>();
        
        // 活跃特效列表
        private List<GameObject> activeEffects = new List<GameObject>();
        
        public void Initialize()
        {
            Debug.Log("初始化特效管理器...");
            
            BuildEffectDictionary();
            
            Debug.Log($"特效管理器初始化完成，加载 {effects.Length} 种特效");
        }
        
        private void BuildEffectDictionary()
        {
            effectDict.Clear();
            
            foreach (var effect in effects)
            {
                if (!string.IsNullOrEmpty(effect.name) && effect.effectPrefab != null)
                {
                    effectDict[effect.name] = effect;
                }
            }
        }
        
        public void PlayEffect(string effectName, Vector3 position, Quaternion rotation = default)
        {
            if (string.IsNullOrEmpty(effectName))
            {
                Debug.LogWarning("特效名称为空");
                return;
            }
            
            if (!effectDict.ContainsKey(effectName))
            {
                Debug.LogError($"未找到特效: {effectName}");
                return;
            }
            
            var effectInfo = effectDict[effectName];
            GameObject effectInstance;
            
            if (effectInfo.useObjectPool && GameManager.Instance?.ObjectPool != null)
            {
                effectInstance = GameManager.Instance.ObjectPool.Get(effectName);
                if (effectInstance != null)
                {
                    effectInstance.transform.position = position;
                    effectInstance.transform.rotation = rotation == default ? Quaternion.identity : rotation;
                }
            }
            else
            {
                effectInstance = Instantiate(effectInfo.effectPrefab, position, 
                    rotation == default ? Quaternion.identity : rotation);
            }
            
            if (effectInstance != null)
            {
                activeEffects.Add(effectInstance);
                
                if (effectInfo.autoDestroy && effectInfo.duration > 0)
                {
                    StartCoroutine(DestroyEffectAfterDelay(effectInstance, effectInfo));
                }
            }
        }
        
        public void PlayEffect(string effectName, Transform parent)
        {
            if (parent == null)
            {
                PlayEffect(effectName, Vector3.zero);
                return;
            }
            
            PlayEffect(effectName, parent.position, parent.rotation);
        }
        
        public void StopEffect(string effectName)
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                if (activeEffects[i] != null && activeEffects[i].name.Contains(effectName))
                {
                    DestroyEffect(activeEffects[i]);
                    activeEffects.RemoveAt(i);
                }
            }
        }
        
        public void StopAllEffects()
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                if (activeEffects[i] != null)
                {
                    DestroyEffect(activeEffects[i]);
                }
            }
            activeEffects.Clear();
        }
        
        private System.Collections.IEnumerator DestroyEffectAfterDelay(GameObject effect, EffectInfo effectInfo)
        {
            yield return new WaitForSeconds(effectInfo.duration);
            
            if (effect != null)
            {
                activeEffects.Remove(effect);
                
                if (effectInfo.useObjectPool && GameManager.Instance?.ObjectPool != null)
                {
                    GameManager.Instance.ObjectPool.Return(effectInfo.name, effect);
                }
                else
                {
                    Destroy(effect);
                }
            }
        }
        
        private void DestroyEffect(GameObject effect)
        {
            if (effect == null) return;
            
            // 尝试确定是否使用对象池
            var effectName = effect.name.Replace("(Clone)", "").Trim();
            
            if (effectDict.ContainsKey(effectName) && effectDict[effectName].useObjectPool && 
                GameManager.Instance?.ObjectPool != null)
            {
                GameManager.Instance.ObjectPool.Return(effectName, effect);
            }
            else
            {
                Destroy(effect);
            }
        }
        
        #if UNITY_EDITOR
        [ContextMenu("播放测试特效")]
        private void PlayTestEffect()
        {
            if (effects.Length > 0)
            {
                PlayEffect(effects[0].name, transform.position);
            }
        }
        
        [ContextMenu("停止所有特效")]
        private void StopAllEffectsDebug()
        {
            StopAllEffects();
        }
        #endif
    }
} 