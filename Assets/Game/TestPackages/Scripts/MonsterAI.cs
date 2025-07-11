
using UnityEngine;
using UnityEngine.AI;
using Core.StateMachine;
using Core.StateMachine.AIStates;
using Core.ScriptableObjects;
using Random = UnityEngine.Random;
using Core;
public class MonsterAI : MonoBehaviour {
    [Header("配置数据")]
    [SerializeField] private MonsterConfig monsterConfig;
    
    [Header("组件引用")]
    [SerializeField] private Animator _anim;
    [SerializeField] private Health _health;
    
    [Header("攻击设置")]
    [SerializeField] private float _attackCooldown = 2f;
    private float _lastAttackTime;
    
    [Header("调试设置")]
    [SerializeField] private bool _showDebugLogs = true;
    
    // 公共属性
    public NavMeshAgent Agent => _agent;
    public Animator Animator => _anim;
    public string CurrentStateName => _enhancedStateMachine?.CurrentStateName ?? "Unknown";
    public float DistanceToPlayer => _playerTransform != null ? 
        Vector3.Distance(transform.position, _playerTransform.position) : float.MaxValue;
    public bool CanAttack => Time.time - _lastAttackTime >= _attackCooldown;
    
    // 私有字段
    private NavMeshAgent _agent;
    private StateMachine<MonsterAI> _enhancedStateMachine;
    
    // Player detection
    private Transform _playerTransform;
    private ItemInteractionHandler _playerItemInteractionHandler;
    private Health _playerHealth;
    
    // 使用配置数据或默认值的检测逻辑
    public bool IsPlayerInLongRange => _playerTransform != null && 
        Vector3.Distance(transform.position, _playerTransform.position) <= GetDetectionRange();
    public bool IsPlayerInAttackRange => _playerTransform != null && 
        Vector3.Distance(transform.position, _playerTransform.position) <= GetAttackRange();
        
    // 具体状态引用（用于状态机转换条件）
    private IdleState idleState;
    private PatrolState patrolState;
    private ChaseState chaseState;
    private AttackState attackState;
    private DeadState deadState;
    
    // For Gizmos debugging
    private Vector3 _currentPatrolGizmoDestination;
    
    private void Awake() {
        // 初始化组件
        _agent = GetComponent<NavMeshAgent>();
        _health = GetComponent<Health>();
        
        if (_agent == null)
        {
            Debug.LogError($"MonsterAI requires a NavMeshAgent component on {gameObject.name}");
        }
        
        if (_health == null)
        {
            Debug.LogError($"MonsterAI requires a Health component on {gameObject.name}");
        }
        else
        {
            // 如果有配置数据，应用配置设置
            if (monsterConfig != null)
            {
                if (monsterConfig.maxHealth > 0)
                {
                    _health.SetMaxHealth(monsterConfig.maxHealth);
                    if (_showDebugLogs)
                    {
                        Debug.Log($"[MonsterAI] 使用配置血量: {monsterConfig.maxHealth}");
                    }
                }
            }
            
            // 订阅新的Health事件
            _health.OnDeath += OnMonsterDeath;
            _health.OnHealthChanged += OnHealthChanged;
            _health.OnDamageTaken += OnDamageTaken;
        }

        // 查找玩家
        _playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (_playerTransform == null) {
            Debug.LogWarning("Player not found! Please tag your player object with 'Player'.");
        }
        else
        {
            _playerItemInteractionHandler = _playerTransform.GetComponent<ItemInteractionHandler>();
            _playerHealth = _playerTransform.GetComponent<Health>();
            
            if (_playerItemInteractionHandler == null)
            {
                Debug.LogWarning("Player ItemInteractionHandler not found on Player object. Damage reduction from gear won't be applied.");
            }
            
            if (_playerHealth == null)
            {
                Debug.LogWarning("Player Health component not found on Player object.");
            }
        }
        
        InitializeStateMachine();
    }
    
    private void InitializeStateMachine()
    {
        // 创建新的强化状态机
        _enhancedStateMachine = new StateMachine<MonsterAI>(this);
        
        // 创建状态实例
        idleState = new IdleState(this, _enhancedStateMachine, monsterConfig);
        patrolState = new PatrolState(this, _enhancedStateMachine, monsterConfig);
        chaseState = new ChaseState(this, _enhancedStateMachine, monsterConfig, _playerTransform);
        attackState = new AttackState(this, _enhancedStateMachine, monsterConfig, _playerTransform);
        deadState = new DeadState(this, _enhancedStateMachine, monsterConfig);
        
        // 添加状态到状态机
        _enhancedStateMachine.AddState(idleState);
        _enhancedStateMachine.AddState(patrolState);
        _enhancedStateMachine.AddState(chaseState);
        _enhancedStateMachine.AddState(attackState);
        _enhancedStateMachine.AddState(deadState);
        
        // 添加状态转换
        _enhancedStateMachine.AddTransition<IdleState, PatrolState>(
            () => idleState.ShouldExitIdle(), "空闲时间结束");
        _enhancedStateMachine.AddTransition<IdleState, ChaseState>(
            () => IsPlayerInLongRange && !_health.IsDead, "检测到玩家");
            
        _enhancedStateMachine.AddTransition<PatrolState, IdleState>(
            () => patrolState.HasReachedDestination(), "到达巡逻点");
        _enhancedStateMachine.AddTransition<PatrolState, ChaseState>(
            () => IsPlayerInLongRange && !_health.IsDead, "巡逻时发现玩家");
            
        _enhancedStateMachine.AddTransition<ChaseState, AttackState>(
            () => IsPlayerInAttackRange && CanAttack && !_health.IsDead, "进入攻击范围");
        _enhancedStateMachine.AddTransition<ChaseState, IdleState>(
            () => !IsPlayerInLongRange && !_health.IsDead, "失去玩家目标");
            
        // 攻击状态转换 - 添加持续攻击逻辑
        _enhancedStateMachine.AddTransition<AttackState, AttackState>(
            () => attackState.IsAttackFinished() && IsPlayerInAttackRange && CanAttack && !_health.IsDead, "攻击完成但玩家仍在范围内，继续攻击");
        _enhancedStateMachine.AddTransition<AttackState, ChaseState>(
            () => !IsPlayerInAttackRange && !_health.IsDead, "离开攻击范围");
        _enhancedStateMachine.AddTransition<AttackState, IdleState>(
            () => attackState.IsAttackFinished() && !IsPlayerInLongRange, "攻击完成且失去目标");
            
        // 死亡状态转换（从任意状态）
        _enhancedStateMachine.AddAnyStateTransition<DeadState>(
            () => _health != null && _health.IsDead, "死亡");
        
        // 设置初始状态
        _enhancedStateMachine.SetInitialState<IdleState>();
    }
    
    private void Update() {
        _enhancedStateMachine?.Update();
    }
    
    private void FixedUpdate()
    {
        _enhancedStateMachine?.FixedUpdate();
    }
    
    /// <summary>
    /// 获取检测范围（优先使用配置数据）
    /// </summary>
    public float GetDetectionRange()
    {
        return monsterConfig.detectionRange;
    }
    
    /// <summary>
    /// 获取攻击范围（优先使用配置数据）
    /// </summary>
    public float GetAttackRange()
    {
        return monsterConfig.attackRange;
    }
    
    
    /// <summary>
    /// 强制切换AI状态（用于调试）
    /// </summary>
    public void ForceChangeState<T>() where T : class, IState
    {
        _enhancedStateMachine?.ChangeState<T>();
    }
    
    /// <summary>
    /// 强制攻击（用于调试）
    /// </summary>
    public void ForceAttack()
    {
        if (!_health.IsDead && CanAttack)
        {
            _enhancedStateMachine?.ChangeState<AttackState>();
        }
    }
    
    /// <summary>
    /// 重置AI（用于调试）
    /// </summary>
    public void ResetAI()
    {
        if (!_health.IsDead)
        {
            _enhancedStateMachine?.ChangeState<IdleState>();
        }
    }
    
    
    /// <summary>
    /// 执行攻击（改进版）
    /// </summary>
    private void PerformAttack(string triggerName = "Attack", int customDamage = -1) {
        if (_health.IsDead) return;
        if (!CanAttack) return;
        
        _lastAttackTime = Time.time;
        
        // 播放攻击动画
        if (_anim != null) {
            _anim.SetTrigger(triggerName);
        }

        // 对玩家造成伤害
        if (_playerHealth != null && IsPlayerInAttackRange)
        {
            // 使用自定义伤害或配置伤害
            int finalDamage = customDamage;
            
            // 应用装备伤害减免
            if (_playerItemInteractionHandler != null)
            {
                finalDamage = _playerItemInteractionHandler.ApplyDamageReduction(finalDamage);
            }
            
            if (_showDebugLogs)
            {
                Debug.Log($"[MonsterAI] {gameObject.name} 攻击玩家造成 {finalDamage} 伤害");
            }
            
            _playerHealth.TakeDamage(finalDamage, gameObject);
            
            // 发布怪物攻击事件
            EventManager.Publish(new WeaponFiredEvent(gameObject, gameObject, transform.position, 
                (_playerTransform.position - transform.position).normalized));
        }
    }
    
    /// <summary>
    /// 受到伤害时的回调
    /// </summary>
    private void OnDamageTaken(int damage, GameObject attacker)
    {
        if (_showDebugLogs)
        {
            Debug.Log($"[MonsterAI] {gameObject.name} 受到 {damage} 伤害，来自: {(attacker != null ? attacker.name : "未知")}");
        }
        
        // 如果攻击者是玩家且怪物还活着，进入追击状态
        if (attacker != null && attacker.CompareTag("Player") && !_health.IsDead)
        {
            _enhancedStateMachine?.ChangeState<ChaseState>();
        }
    }
    
    /// <summary>
    /// 血量变化时的回调
    /// </summary>
    private void OnHealthChanged(int currentHealth, int maxHealth)
    {
        if (_showDebugLogs)
        {
            Debug.Log($"[MonsterAI] {gameObject.name} 血量变化: {currentHealth}/{maxHealth} ({_health.HealthPercentage:P1})");
        }
        
        // 血量低时可以切换到更激进的行为
        if (_health.HealthPercentage <= 0.3f && !_health.IsDead)
        {
            // 低血量时加速移动和攻击
            if (_agent != null)
            {
                _agent.speed = Mathf.Max(_agent.speed, monsterConfig.moveSpeed * 1.2f);
            }
            _attackCooldown = Mathf.Max(0.5f, _attackCooldown * 0.7f);
        }
    }
    
    private void OnMonsterDeath()
    {
        if (_showDebugLogs)
        {
            Debug.Log($"[MonsterAI] {gameObject.name} 死亡！AI转换到死亡状态。");
        }
        
        // 状态机会自动处理死亡状态转换
        // 禁用导航和碰撞
        if (_agent != null)
        {
            _agent.enabled = false;
        }
        
        // 取消事件订阅
        if (_health != null)
        {
            _health.OnDeath -= OnMonsterDeath;
            _health.OnHealthChanged -= OnHealthChanged;
            _health.OnDamageTaken -= OnDamageTaken;
        }
        
        // 发布敌人死亡事件
        string enemyType = monsterConfig?.monsterName ?? "Unknown Monster";
        int scoreValue = monsterConfig?.maxHealth / 10 ?? 10;
        
        EventManager.Publish(new EnemyDeathEvent(gameObject, transform.position, null, enemyType, scoreValue));
    }
    
    public Vector3 GetRandomNavMeshLocation() {
        NavMeshHit hit;
        Vector3 randomDirection = Random.insideUnitSphere * 20f;
        randomDirection += transform.position;
        if (NavMesh.SamplePosition(randomDirection, out hit, 20f, NavMesh.AllAreas)) {
            // Store this for gizmo drawing
            _currentPatrolGizmoDestination = hit.position;
            return hit.position;
        }
        _currentPatrolGizmoDestination = transform.position;
        return transform.position;
    }

    private void OnDisable()
    {
        if (_health != null)
        {
            _health.OnDeath -= OnMonsterDeath;
            _health.OnHealthChanged -= OnHealthChanged;
            _health.OnDamageTaken -= OnDamageTaken;
        }
        
        // 清理状态机
        _enhancedStateMachine?.Cleanup();
    }

    // --- Gizmo Debugging ---
    private void OnDrawGizmos() {
        // 使用配置数据或默认值绘制检测范围
        float detectionRange = GetDetectionRange();
        float attackRange = GetAttackRange();
        
        // 玩家检测范围 (长距离)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 攻击范围 (近距离)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // 巡逻目标点
        if (_currentPatrolGizmoDestination != Vector3.zero) {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(_currentPatrolGizmoDestination, 0.5f);
            Gizmos.DrawLine(transform.position, _currentPatrolGizmoDestination);
        }

        // NavMesh路径
        if (_agent != null && _agent.hasPath) {
            Gizmos.color = Color.green;
            Vector3[] pathCorners = _agent.path.corners;
            for (int i = 0; i < pathCorners.Length - 1; i++) {
                Gizmos.DrawLine(pathCorners[i], pathCorners[i + 1]);
                Gizmos.DrawSphere(pathCorners[i], 0.2f);
            }
            if (pathCorners.Length > 0)
            {
                Gizmos.DrawSphere(pathCorners[pathCorners.Length - 1], 0.2f);
            }
        }
        
        // 让状态机绘制状态特定的Gizmos
        _enhancedStateMachine?.OnDrawGizmos();
        
        // 显示当前状态名称和血量信息
        #if UNITY_EDITOR
        if (Application.isPlaying && _health != null)
        {
            UnityEditor.Handles.color = _health.IsDead ? Color.red : Color.white;
            string statusText = $"状态: {CurrentStateName}\n距离: {DistanceToPlayer:F1}m\n血量: {_health.CurrentHealth}/{_health.MaxHealth}";
            
            if (_health.HasShield)
            {
                statusText += $"\n护盾: {_health.CurrentShield}/{_health.MaxShield}";
            }
            
            if (_health.IsInvincible)
            {
                statusText += "\n[无敌]";
            }
            
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, statusText);
        }
        #endif
    }
    
    #if UNITY_EDITOR
    [UnityEngine.ContextMenu("测试攻击")]
    private void TestAttack()
    {
        PerformAttack();
    }
    
    [UnityEngine.ContextMenu("受到伤害(10)")]
    private void TestTakeDamage()
    {
        if (_health != null)
        {
            _health.TakeDamage(10);
        }
    }
    
    [UnityEngine.ContextMenu("强制追击状态")]
    private void TestChaseState()
    {
        ForceChangeState<ChaseState>();
    }
    
    [UnityEngine.ContextMenu("强制攻击状态")]
    private void TestAttackState()
    {
        ForceAttack();
    }
    
    [UnityEngine.ContextMenu("重置AI")]
    private void TestResetAI()
    {
        ResetAI();
    }
    #endif
}

