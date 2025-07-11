using UnityEngine;
using System;
using System.Collections;
using Core;
/// <summary>
/// 完整的生命值系统
/// 支持生命值、护盾、无敌时间、多种伤害类型和完整事件集成
/// </summary>
public class Health : MonoBehaviour
{
    [Header("基础设置")]
    [SerializeField] private int _maxHealth = 100;
    [SerializeField] private int _currentHealth;
    [SerializeField] private bool _initializeOnAwake = true;
    
    [Header("护盾系统")]
    [SerializeField] private bool _hasShield = false;
    [SerializeField] private int _maxShield = 50;
    [SerializeField] private int _currentShield;
    [SerializeField] private float _shieldRegenDelay = 3f;
    [SerializeField] private float _shieldRegenRate = 10f; // 每秒恢复点数
    
    [Header("无敌时间")]
    [SerializeField] private bool _hasInvincibility = false;
    [SerializeField] private float _invincibilityDuration = 1f;
    [SerializeField] private bool _isInvincible = false;
    
    [Header("死亡设置")]
    [SerializeField] private bool _destroyOnDeath = false;
    [SerializeField] private float _destroyDelay = 2f;
    [SerializeField] private bool _disableOnDeath = true;
    
    [Header("调试信息")]
    [SerializeField] private bool _showDebugLogs = true;
    [SerializeField] private bool _isDead = false;
    
    // 属性
    public int MaxHealth => _maxHealth;
    public int CurrentHealth => _currentHealth;
    public int MaxShield => _maxShield;
    public int CurrentShield => _currentShield;
    public bool HasShield => _hasShield;
    public bool IsInvincible => _isInvincible;
    public bool IsDead => _isDead;
    public bool IsFullHealth => _currentHealth >= _maxHealth;
    public bool IsFullShield => _currentShield >= _maxShield;
    public float HealthPercentage => _maxHealth > 0 ? (float)_currentHealth / _maxHealth : 0f;
    public float ShieldPercentage => _maxShield > 0 ? (float)_currentShield / _maxShield : 0f;
    public int TotalHealth => _currentHealth + _currentShield;
    public int TotalMaxHealth => _maxHealth + _maxShield;
    
    // 事件
    public event Action<int, int> OnHealthChanged; // (current, max)
    public event Action<int, int> OnShieldChanged; // (current, max)
    public event Action<int, GameObject> OnDamageTaken; // (damage, attacker)
    public event Action<int> OnHealed; // (healAmount)
    public event Action OnDeath;
    public event Action OnRevive;
    public event Action<bool> OnInvincibilityChanged; // (isInvincible)
    
    // 私有变量
    private Coroutine _shieldRegenCoroutine;
    private Coroutine _invincibilityCoroutine;
    private float _lastDamageTime;
    
    private void Awake()
    {
        if (_initializeOnAwake)
        {
            Initialize();
        }
    }
    
    /// <summary>
    /// 初始化生命值系统
    /// </summary>
    public void Initialize()
    {
        _currentHealth = _maxHealth;
        _currentShield = _hasShield ? _maxShield : 0;
        _isDead = false;
        _isInvincible = false;
        
        // 触发初始事件
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        if (_hasShield)
        {
            OnShieldChanged?.Invoke(_currentShield, _maxShield);
        }
        
        if (_showDebugLogs)
        {
            Debug.Log($"[Health] {gameObject.name} 初始化完成 - 生命值: {_currentHealth}/{_maxHealth}" + 
                     (_hasShield ? $", 护盾: {_currentShield}/{_maxShield}" : ""));
        }
    }
    
    /// <summary>
    /// 造成伤害
    /// </summary>
    public void TakeDamage(int damage, GameObject attacker = null, bool ignoreInvincibility = false)
    {
        if (damage <= 0) return;
        if (_isDead) return;
        if (_isInvincible && !ignoreInvincibility) 
        {
            if (_showDebugLogs)
            {
                Debug.Log($"[Health] {gameObject.name} 处于无敌状态，忽略伤害: {damage}");
            }
            return;
        }
        
        int actualDamage = 0;
        
        // 先扣护盾
        if (_hasShield && _currentShield > 0)
        {
            int shieldDamage = Mathf.Min(damage, _currentShield);
            _currentShield -= shieldDamage;
            damage -= shieldDamage;
            actualDamage += shieldDamage;
            
            OnShieldChanged?.Invoke(_currentShield, _maxShield);
            
            if (_showDebugLogs)
            {
                Debug.Log($"[Health] {gameObject.name} 护盾受到伤害: {shieldDamage}, 剩余护盾: {_currentShield}");
            }
        }
        
        // 再扣生命值
        if (damage > 0)
        {
            int healthDamage = Mathf.Min(damage, _currentHealth);
            _currentHealth -= healthDamage;
            actualDamage += healthDamage;
            
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
            
            if (_showDebugLogs)
            {
                Debug.Log($"[Health] {gameObject.name} 生命值受到伤害: {healthDamage}, 剩余生命值: {_currentHealth}");
            }
        }
        
        // 记录伤害时间和触发事件
        _lastDamageTime = Time.time;
        OnDamageTaken?.Invoke(actualDamage, attacker);
        
        // 发布游戏事件
        PublishDamageEvent(actualDamage, attacker);
        
        // 停止护盾恢复
        if (_shieldRegenCoroutine != null)
        {
            StopCoroutine(_shieldRegenCoroutine);
        }
        
        // 启动无敌时间
        if (_hasInvincibility && actualDamage > 0)
        {
            StartInvincibility();
        }
        
        // 检查死亡
        if (_currentHealth <= 0)
        {
            Die(attacker);
        }
        else
        {
            // 开始护盾恢复倒计时
            if (_hasShield && _currentShield < _maxShield)
            {
                _shieldRegenCoroutine = StartCoroutine(ShieldRegeneration());
            }
        }
    }
    
    /// <summary>
    /// 治疗
    /// </summary>
    public void Heal(int healAmount, bool healShield = false)
    {
        if (healAmount <= 0) return;
        if (_isDead) return;
        
        int totalHealed = 0;
        
        // 治疗生命值
        if (_currentHealth < _maxHealth)
        {
            int healthHealed = Mathf.Min(healAmount, _maxHealth - _currentHealth);
            _currentHealth += healthHealed;
            totalHealed += healthHealed;
            healAmount -= healthHealed;
            
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
            
            if (_showDebugLogs)
            {
                Debug.Log($"[Health] {gameObject.name} 治疗生命值: {healthHealed}, 当前生命值: {_currentHealth}");
            }
        }
        
        // 治疗护盾
        if (healShield && _hasShield && healAmount > 0 && _currentShield < _maxShield)
        {
            int shieldHealed = Mathf.Min(healAmount, _maxShield - _currentShield);
            _currentShield += shieldHealed;
            totalHealed += shieldHealed;
            
            OnShieldChanged?.Invoke(_currentShield, _maxShield);
            
            if (_showDebugLogs)
            {
                Debug.Log($"[Health] {gameObject.name} 治疗护盾: {shieldHealed}, 当前护盾: {_currentShield}");
            }
        }
        
        if (totalHealed > 0)
        {
            OnHealed?.Invoke(totalHealed);
            PublishHealEvent(totalHealed);
        }
    }
    
    /// <summary>
    /// 死亡
    /// </summary>
    private void Die(GameObject killer = null)
    {
        if (_isDead) return;
        
        _isDead = true;
        _currentHealth = 0;
        
        if (_showDebugLogs)
        {
            Debug.Log($"[Health] {gameObject.name} 死亡！杀手: {(killer != null ? killer.name : "未知")}");
        }
        
        OnDeath?.Invoke();
        PublishDeathEvent(killer);
        
        // 处理死亡后的行为
        if (_disableOnDeath)
        {
            // 禁用碰撞器和其他组件
            var colliders = GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                col.enabled = false;
            }
            
            // 禁用AI组件
            var ai = GetComponent<MonsterAI>();
            if (ai != null)
            {
                ai.enabled = false;
            }
        }
        
        // 启用布娃娃系统
        var ragdoll = GetComponent<RagdollController>();
        if (ragdoll != null)
        {
            ragdoll.SetRagdollState(true);
        }
        
        // 销毁对象
        if (_destroyOnDeath)
        {
            Destroy(gameObject, _destroyDelay);
        }
    }
    
    /// <summary>
    /// 复活
    /// </summary>
    public void Revive(int healthAmount = -1, int shieldAmount = -1)
    {
        if (!_isDead) return;
        
        _isDead = false;
        _currentHealth = healthAmount >= 0 ? Mathf.Min(healthAmount, _maxHealth) : _maxHealth;
        
        if (_hasShield)
        {
            _currentShield = shieldAmount >= 0 ? Mathf.Min(shieldAmount, _maxShield) : _maxShield;
        }
        
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        if (_hasShield)
        {
            OnShieldChanged?.Invoke(_currentShield, _maxShield);
        }
        
        OnRevive?.Invoke();
        
        if (_showDebugLogs)
        {
            Debug.Log($"[Health] {gameObject.name} 复活！生命值: {_currentHealth}/{_maxHealth}");
        }
        
        // 重新启用组件
        if (_disableOnDeath)
        {
            var colliders = GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                col.enabled = true;
            }
            
            var ai = GetComponent<MonsterAI>();
            if (ai != null)
            {
                ai.enabled = true;
            }
        }
    }
    
    /// <summary>
    /// 立即杀死
    /// </summary>
    public void Kill(GameObject killer = null)
    {
        TakeDamage(_currentHealth + _currentShield, killer, true);
    }
    
    /// <summary>
    /// 设置最大生命值
    /// </summary>
    public void SetMaxHealth(int newMaxHealth, bool adjustCurrent = true)
    {
        _maxHealth = Mathf.Max(1, newMaxHealth);
        
        if (adjustCurrent)
        {
            _currentHealth = Mathf.Min(_currentHealth, _maxHealth);
        }
        
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }
    
    /// <summary>
    /// 设置当前生命值
    /// </summary>
    public void SetCurrentHealth(int health)
    {
        _currentHealth = Mathf.Clamp(health, 0, _maxHealth);
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        
        if (_currentHealth <= 0 && !_isDead)
        {
            Die();
        }
    }
    
    /// <summary>
    /// 设置护盾
    /// </summary>
    public void SetShield(int maxShield, int currentShield = -1, bool enable = true)
    {
        _hasShield = enable;
        _maxShield = Mathf.Max(0, maxShield);
        _currentShield = currentShield >= 0 ? Mathf.Min(currentShield, _maxShield) : _maxShield;
        
        OnShieldChanged?.Invoke(_currentShield, _maxShield);
    }
    
    /// <summary>
    /// 开始无敌时间
    /// </summary>
    public void StartInvincibility(float duration = -1)
    {
        if (!_hasInvincibility && duration < 0) return;
        
        float invDuration = duration >= 0 ? duration : _invincibilityDuration;
        
        if (_invincibilityCoroutine != null)
        {
            StopCoroutine(_invincibilityCoroutine);
        }
        
        _invincibilityCoroutine = StartCoroutine(InvincibilityCoroutine(invDuration));
    }
    
    /// <summary>
    /// 护盾恢复协程
    /// </summary>
    private IEnumerator ShieldRegeneration()
    {
        yield return new WaitForSeconds(_shieldRegenDelay);
        
        while (_currentShield < _maxShield && !_isDead)
        {
            float regenAmount = _shieldRegenRate * Time.deltaTime;
            int regenInt = Mathf.FloorToInt(regenAmount);
            
            if (regenInt > 0)
            {
                _currentShield = Mathf.Min(_currentShield + regenInt, _maxShield);
                OnShieldChanged?.Invoke(_currentShield, _maxShield);
            }
            
            yield return null;
        }
    }
    
    /// <summary>
    /// 无敌时间协程
    /// </summary>
    private IEnumerator InvincibilityCoroutine(float duration)
    {
        _isInvincible = true;
        OnInvincibilityChanged?.Invoke(true);
        
        if (_showDebugLogs)
        {
            Debug.Log($"[Health] {gameObject.name} 开始无敌时间: {duration}秒");
        }
        
        yield return new WaitForSeconds(duration);
        
        _isInvincible = false;
        OnInvincibilityChanged?.Invoke(false);
        
        if (_showDebugLogs)
        {
            Debug.Log($"[Health] {gameObject.name} 无敌时间结束");
        }
    }
    
    /// <summary>
    /// 发布伤害事件
    /// </summary>
    private void PublishDamageEvent(int damage, GameObject attacker)
    {
        if (CompareTag("Player"))
        {
            EventManager.Publish(new PlayerDamagedEvent(damage, _currentHealth, _maxHealth, transform.position, attacker));
        }
        else
        {
            EventManager.Publish(new EnemyDamagedEvent(gameObject, damage, _currentHealth, attacker));
        }
    }
    
    /// <summary>
    /// 发布治疗事件
    /// </summary>
    private void PublishHealEvent(int healAmount)
    {
        if (CompareTag("Player"))
        {
            EventManager.Publish(new PlayerHealedEvent(healAmount, _currentHealth, _maxHealth));
        }
    }
    
    /// <summary>
    /// 发布死亡事件
    /// </summary>
    private void PublishDeathEvent(GameObject killer)
    {
        if (CompareTag("Player"))
        {
            EventManager.Publish(new PlayerDeathEvent(transform.position, killer));
        }
        else
        {
            // 获取怪物类型
            string enemyType = "Unknown";
            var monsterAI = GetComponent<MonsterAI>();
            if (monsterAI != null)
            {
                enemyType = "Monster";
            }
            
            // 计算分数值
            int scoreValue = _maxHealth / 10; // 简单的分数计算
            
            EventManager.Publish(new EnemyDeathEvent(gameObject, transform.position, killer, enemyType, scoreValue));
        }
    }
    
    #if UNITY_EDITOR
    
    [ContextMenu("测试受伤(10点)")]
    private void TestDamage10()
    {
        TakeDamage(10);
    }
    
    [ContextMenu("测试受伤(50点)")]
    private void TestDamage50()
    {
        TakeDamage(50);
    }
    
    [ContextMenu("测试治疗(20点)")]
    private void TestHeal20()
    {
        Heal(20);
    }
    
    [ContextMenu("完全治疗")]
    private void TestFullHeal()
    {
        Heal(_maxHealth + _maxShield, true);
    }
    
    [ContextMenu("立即杀死")]
    private void TestKill()
    {
        Kill();
    }
    
    [ContextMenu("复活")]
    private void TestRevive()
    {
        Revive();
    }
    
    [ContextMenu("开始无敌")]
    private void TestInvincibility()
    {
        StartInvincibility(5f);
    }
    
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        
        _maxHealth = Mathf.Max(1, _maxHealth);
        _maxShield = Mathf.Max(0, _maxShield);
        _invincibilityDuration = Mathf.Max(0f, _invincibilityDuration);
        _shieldRegenDelay = Mathf.Max(0f, _shieldRegenDelay);
        _shieldRegenRate = Mathf.Max(0f, _shieldRegenRate);
        _destroyDelay = Mathf.Max(0f, _destroyDelay);
         }
     #endif
}