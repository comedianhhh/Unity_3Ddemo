using UnityEngine;
using Core;
/// <summary>
/// 玩家交互控制器 - 处理玩家输入和游戏系统交互
/// 适配新的Health系统和事件系统
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("组件引用")]
    [SerializeField] private ItemInteractionHandler _playerItemInteractionHandler;
    [SerializeField] private Health _playerHealth;
    
    [Header("输入设置")]
    [SerializeField] private KeyCode _throwGrenadeKey = KeyCode.G;
    [SerializeField] private KeyCode _toggleGearKey = KeyCode.E;
    [SerializeField] private KeyCode _healKey = KeyCode.H;
    [SerializeField] private KeyCode _testDamageKey = KeyCode.T;
    
    [Header("治疗设置")]
    [SerializeField] private int _healAmount = 25;
    [SerializeField] private float _healCooldown = 3f;
    private float _lastHealTime;
    
    [Header("调试设置")]
    [SerializeField] private bool _showDebugLogs = true;
    [SerializeField] private bool _enableTestKeys = false;
    [SerializeField] private int _testDamageAmount = 10;
    
    // 状态属性
    public bool IsAlive => _playerHealth != null && !_playerHealth.IsDead;
    public bool CanHeal => Time.time - _lastHealTime >= _healCooldown && IsAlive && !_playerHealth.IsFullHealth;
    
    private void Awake()
    {
        // 初始化组件引用
        InitializeComponents();
        
        // 订阅事件
        SubscribeToEvents();
    }
    
    private void InitializeComponents()
    {
        // 获取ItemInteractionHandler组件
        if (_playerItemInteractionHandler == null)
        {
            _playerItemInteractionHandler = GetComponent<ItemInteractionHandler>();
            if (_playerItemInteractionHandler == null)
            {
                Debug.LogError("[PlayerInteraction] PlayerItemInteractionHandler is not assigned or found on the player object.");
            }
        }
        
        // 获取Health组件
        if (_playerHealth == null)
        {
            _playerHealth = GetComponent<Health>();
            if (_playerHealth == null)
            {
                Debug.LogError("[PlayerInteraction] Player Health component not found on this GameObject.");
            }
        }
        
        if (_showDebugLogs)
        {
            Debug.Log($"[PlayerInteraction] 初始化完成 - 血量: {(_playerHealth != null ? $"{_playerHealth.CurrentHealth}/{_playerHealth.MaxHealth}" : "N/A")}");
        }
    }
    
    private void SubscribeToEvents()
    {
        if (_playerHealth != null)
        {
            _playerHealth.OnDeath += OnPlayerDeath;
            _playerHealth.OnHealthChanged += OnPlayerHealthChanged;
            _playerHealth.OnDamageTaken += OnPlayerDamageTaken;
            _playerHealth.OnHealed += OnPlayerHealed;
            _playerHealth.OnRevive += OnPlayerRevive;
            _playerHealth.OnInvincibilityChanged += OnPlayerInvincibilityChanged;
        }
        
        if (_playerItemInteractionHandler != null)
        {
            _playerItemInteractionHandler.OnGrenadeCountChanged += OnGrenadeCountChanged;
            _playerItemInteractionHandler.OnGearStatusChanged += OnGearStatusChanged;
        }
    }
    
    private void UnsubscribeFromEvents()
    {
        if (_playerHealth != null)
        {
            _playerHealth.OnDeath -= OnPlayerDeath;
            _playerHealth.OnHealthChanged -= OnPlayerHealthChanged;
            _playerHealth.OnDamageTaken -= OnPlayerDamageTaken;
            _playerHealth.OnHealed -= OnPlayerHealed;
            _playerHealth.OnRevive -= OnPlayerRevive;
            _playerHealth.OnInvincibilityChanged -= OnPlayerInvincibilityChanged;
        }
        
        if (_playerItemInteractionHandler != null)
        {
            _playerItemInteractionHandler.OnGrenadeCountChanged -= OnGrenadeCountChanged;
            _playerItemInteractionHandler.OnGearStatusChanged -= OnGearStatusChanged;
        }
    }
    
    private void Update()
    {
        if (!IsAlive) return;
        
        HandleItemInteractionInput();
        HandleHealthInput();
        
        if (_enableTestKeys)
        {
            HandleTestInput();
        }
    }
    
    /// <summary>
    /// 处理物品交互输入
    /// </summary>
    private void HandleItemInteractionInput()
    {
        // 投掷手榴弹
        if (Input.GetKeyDown(_throwGrenadeKey))
        {
            ThrowGrenade();
        }

        // 切换装备
        if (Input.GetKeyDown(_toggleGearKey))
        {
            ToggleGear();
        }
    }
    
    /// <summary>
    /// 处理血量相关输入
    /// </summary>
    private void HandleHealthInput()
    {
        // 治疗
        if (Input.GetKeyDown(_healKey))
        {
            TryHeal();
        }
    }
    
    /// <summary>
    /// 处理测试输入（仅在启用测试时）
    /// </summary>
    private void HandleTestInput()
    {
        // 测试受伤
        if (Input.GetKeyDown(_testDamageKey))
        {
            TestTakeDamage();
        }
    }
    
    /// <summary>
    /// 投掷手榴弹
    /// </summary>
    public void ThrowGrenade()
    {
        if (_playerItemInteractionHandler != null && _playerItemInteractionHandler.HasGrenadeReady())
        {
            _playerItemInteractionHandler.ThrowHeldGrenade();
            
            if (_showDebugLogs)
            {
                Debug.Log("[PlayerInteraction] 投掷手榴弹");
            }
        }
        else if (_showDebugLogs)
        {
            Debug.Log("[PlayerInteraction] 没有可投掷的手榴弹");
        }
    }
    
    /// <summary>
    /// 切换装备
    /// </summary>
    public void ToggleGear()
    {
        if (_playerItemInteractionHandler != null)
        {
            _playerItemInteractionHandler.TogglePermanentVariantGear();
            
            if (_showDebugLogs)
            {
                Debug.Log("[PlayerInteraction] 切换装备状态");
            }
        }
    }
    
    /// <summary>
    /// 尝试治疗
    /// </summary>
    public void TryHeal()
    {
        if (CanHeal && _playerHealth != null)
        {
            _lastHealTime = Time.time;
            _playerHealth.Heal(_healAmount);
            
            if (_showDebugLogs)
            {
                Debug.Log($"[PlayerInteraction] 使用治疗，恢复 {_healAmount} 血量");
            }
        }
        else if (_showDebugLogs)
        {
            string reason = !IsAlive ? "玩家已死亡" : 
                           _playerHealth.IsFullHealth ? "血量已满" : 
                           "治疗冷却中";
            Debug.Log($"[PlayerInteraction] 无法治疗: {reason}");
        }
    }
    
    /// <summary>
    /// 测试受伤（仅测试用）
    /// </summary>
    public void TestTakeDamage()
    {
        if (_playerHealth != null && IsAlive)
        {
            _playerHealth.TakeDamage(_testDamageAmount);
            
            if (_showDebugLogs)
            {
                Debug.Log($"[PlayerInteraction] 测试受伤: {_testDamageAmount} 点伤害");
            }
        }
    }
    
    /// <summary>
    /// 玩家死亡回调
    /// </summary>
    private void OnPlayerDeath()
    {
        if (_showDebugLogs)
        {
            Debug.Log("[PlayerInteraction] 玩家死亡！禁用交互。");
        }
        
        // 发布玩家死亡事件
        EventManager.Publish(new PlayerDeathEvent(transform.position));
        
        // 可以在这里添加死亡后的特殊处理
        this.enabled = false;
    }
    
    /// <summary>
    /// 玩家血量变化回调
    /// </summary>
    private void OnPlayerHealthChanged(int currentHealth, int maxHealth)
    {
        if (_showDebugLogs)
        {
            Debug.Log($"[PlayerInteraction] 玩家血量变化: {currentHealth}/{maxHealth} ({(float)currentHealth/maxHealth:P1})");
        }
        
        // 血量低时的警告
        float healthPercentage = (float)currentHealth / maxHealth;
        if (healthPercentage <= 0.2f && currentHealth > 0)
        {
            Debug.LogWarning("[PlayerInteraction] 玩家血量危险！请及时治疗。");
        }
    }
    
    /// <summary>
    /// 玩家受伤回调
    /// </summary>
    private void OnPlayerDamageTaken(int damage, GameObject attacker)
    {
        if (_showDebugLogs)
        {
            Debug.Log($"[PlayerInteraction] 玩家受到 {damage} 伤害，来自: {(attacker != null ? attacker.name : "未知")}");
        }
        
        // 可以在这里添加受伤时的特效、音效等
    }
    
    /// <summary>
    /// 玩家治疗回调
    /// </summary>
    private void OnPlayerHealed(int healAmount)
    {
        if (_showDebugLogs)
        {
            Debug.Log($"[PlayerInteraction] 玩家恢复了 {healAmount} 血量");
        }
        
        // 可以在这里添加治疗特效
    }
    
    /// <summary>
    /// 玩家复活回调
    /// </summary>
    private void OnPlayerRevive()
    {
        if (_showDebugLogs)
        {
            Debug.Log("[PlayerInteraction] 玩家复活！重新启用交互。");
        }
        
        this.enabled = true;
        
        // 发布玩家复活事件（如果有的话）
        EventManager.Publish(new PlayerRespawnEvent(transform.position));
    }
    
    /// <summary>
    /// 玩家无敌状态变化回调
    /// </summary>
    private void OnPlayerInvincibilityChanged(bool isInvincible)
    {
        if (_showDebugLogs)
        {
            Debug.Log($"[PlayerInteraction] 玩家无敌状态: {(isInvincible ? "开启" : "关闭")}");
        }
        
        // 可以在这里添加无敌时的视觉效果
    }
    
    /// <summary>
    /// 手榴弹数量变化回调
    /// </summary>
    private void OnGrenadeCountChanged(int newCount)
    {
        if (_showDebugLogs)
        {
            Debug.Log($"[PlayerInteraction] 手榴弹数量: {newCount}");
        }
    }
    
    /// <summary>
    /// 装备状态变化回调
    /// </summary>
    private void OnGearStatusChanged(bool hasGear)
    {
        if (_showDebugLogs)
        {
            Debug.Log($"[PlayerInteraction] 装备状态: {(hasGear ? "已装备" : "未装备")}");
        }
    }
    
    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }
    
    #if UNITY_EDITOR
    
    [UnityEngine.ContextMenu("测试投掷手榴弹")]
    private void EditorTestThrowGrenade()
    {
        ThrowGrenade();
    }
    
    [UnityEngine.ContextMenu("测试切换装备")]
    private void EditorTestToggleGear()
    {
        ToggleGear();
    }
    
    [UnityEngine.ContextMenu("测试治疗")]
    private void EditorTestHeal()
    {
        TryHeal();
    }
    
    [UnityEngine.ContextMenu("测试受伤")]
    private void EditorTestDamage()
    {
        TestTakeDamage();
    }
    
    [UnityEngine.ContextMenu("显示玩家状态")]
    private void EditorShowPlayerStatus()
    {
        if (_playerHealth != null)
        {
            Debug.Log($"=== 玩家状态 ===\n" +
                     $"血量: {_playerHealth.CurrentHealth}/{_playerHealth.MaxHealth}\n" +
                     $"护盾: {(_playerHealth.HasShield ? $"{_playerHealth.CurrentShield}/{_playerHealth.MaxShield}" : "无")}\n" +
                     $"存活: {IsAlive}\n" +
                     $"无敌: {_playerHealth.IsInvincible}\n" +
                     $"能否治疗: {CanHeal}");
        }
        
        if (_playerItemInteractionHandler != null)
        {
            Debug.Log($"=== 物品状态 ===\n" +
                     $"手榴弹: {(_playerItemInteractionHandler.HasGrenadeReady() ? "有" : "无")}\n" +
                     $"装备: 状态未知"); // 可以根据实际API调整
        }
    }
    #endif
}
