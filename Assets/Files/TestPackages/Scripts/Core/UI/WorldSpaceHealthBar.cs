using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace Core.UI
{
    /// <summary>
    /// 世界空间血条组件 - 显示在游戏单位头顶的血条
    /// </summary>
    public class WorldSpaceHealthBar : MonoBehaviour
    {
        [Header("血条设置")]
        [Tooltip("血条背景图片")]
        public Image backgroundBar;
        
        [Tooltip("血条填充图片")]
        public Image fillBar;
        
        [Tooltip("是否始终显示")]
        public bool alwaysShow = false;
        
        [Tooltip("血量满时是否隐藏")]
        public bool hideWhenFull = false;
        
        [Tooltip("血条相对于目标的偏移")]
        public Vector3 offset = new Vector3(0, 4f, 0);
        
        [Header("动画设置")]
        [Tooltip("血条更新的平滑时间")]
        [Range(0f, 1f)]
        public float smoothTime = 0.3f;
        
        [Tooltip("自动隐藏延迟时间")]
        [Range(0f, 10f)]
        public float autoHideDelay = 3f;
        
        // 私有变量
        private Health targetHealth;
        private Transform targetTransform;
        private Camera playerCamera;
        private Canvas worldCanvas;
        private float currentFillAmount;
        private float targetFillAmount;
        private float velocity;
        private float lastDamageTime;
        private bool isVisible = true;
        
        
        private void Awake()
        {
            // 获取画布组件
            worldCanvas = GetComponentInParent<Canvas>();
            if (worldCanvas == null)
            {
                worldCanvas = GetComponent<Canvas>();
            }
            
            // 确保画布设置正确
            if (worldCanvas != null)
            {
                worldCanvas.renderMode = RenderMode.WorldSpace;
                worldCanvas.worldCamera = Camera.main;
            }
        }
        
        private void OnEnable()
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                playerCamera = FindFirstObjectByType<Camera>();
            }
            
            // 订阅事件
            EventManager.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            EventManager.Subscribe<EnemyDamagedEvent>(OnEnemyDamaged);
            EventManager.Subscribe<PlayerHealedEvent>(OnPlayerHealed);
            
            // 初始化血条状态
            if (targetHealth != null)
            {
                UpdateHealthBar(true);
            }
        }
        
        private void OnDisable()
        {
            // 取消订阅全局事件
            if (EventManager.IsInstance())
            {
                EventManager.Unsubscribe<PlayerDamagedEvent>(OnPlayerDamaged);
                EventManager.Unsubscribe<EnemyDamagedEvent>(OnEnemyDamaged);
                EventManager.Unsubscribe<PlayerHealedEvent>(OnPlayerHealed);
            }
            
            // 取消订阅Health组件事件
            if (targetHealth != null)
            {
                targetHealth.OnHealthChanged -= OnHealthChanged;
                targetHealth.OnDamageTaken -= OnDamageTaken;
                targetHealth.OnHealed -= OnHealed;
            }
        }
        
        private void Update()
        {
            if (targetTransform == null || targetHealth == null)
            {
                return;
            }
            
            // 更新位置跟随目标
            UpdatePosition();
            
            // 更新朝向摄像机
            UpdateRotation();
            
            // 平滑更新血条
            UpdateFillSmooth();
            
            // 处理自动隐藏
            HandleAutoHide();
        }
        
        /// <summary>
        /// 初始化血条
        /// </summary>
        public void Initialize(Health health, Transform target)
        {
            // 先取消之前的订阅
            if (targetHealth != null)
            {
                targetHealth.OnHealthChanged -= OnHealthChanged;
                targetHealth.OnDamageTaken -= OnDamageTaken;
                targetHealth.OnHealed -= OnHealed;
            }
            
            targetHealth = health;
            targetTransform = target;
            
            if (targetHealth != null)
            {
                // 直接订阅Health组件的事件
                targetHealth.OnHealthChanged += OnHealthChanged;
                targetHealth.OnDamageTaken += OnDamageTaken;
                targetHealth.OnHealed += OnHealed;
                
                UpdateHealthBar(true);
                
                // 设置初始可见性
                SetVisible(!hideWhenFull || !targetHealth.IsFullHealth);
                
                Debug.Log($"[WorldSpaceHealthBar] 已连接到 {targetHealth.gameObject.name} 的血量系统");
            }
        }
        
        /// <summary>
        /// 更新血条显示
        /// </summary>
        private void UpdateHealthBar(bool immediate = false)
        {
            if (targetHealth == null) return;
            
            float healthPercent = (float)targetHealth.CurrentHealth / targetHealth.MaxHealth;
            targetFillAmount = healthPercent;
            
            if (immediate)
            {
                currentFillAmount = targetFillAmount;
                if (fillBar != null)
                {
                    fillBar.fillAmount = currentFillAmount;
                }
            }
            
            // 更新颜色
            UpdateHealthBarColor(healthPercent);
            
            // 处理可见性
            if (hideWhenFull && targetHealth.IsFullHealth)
            {
                SetVisible(false);
            }
            else if (!isVisible)
            {
                SetVisible(true);
            }
            
            // 记录受伤时间
            lastDamageTime = Time.time;
        }
        
        /// <summary>
        /// 平滑更新血条填充
        /// </summary>
        private void UpdateFillSmooth()
        {
            if (fillBar == null) return;
            
            if (Mathf.Abs(currentFillAmount - targetFillAmount) > 0.01f)
            {
                currentFillAmount = Mathf.SmoothDamp(currentFillAmount, targetFillAmount, ref velocity, smoothTime);
                fillBar.fillAmount = currentFillAmount;
            }
        }
        
        /// <summary>
        /// 更新血条颜色
        /// </summary>
        private void UpdateHealthBarColor(float healthPercent)
        {
            if (fillBar == null) return;
            
            Color healthColor;
            if (healthPercent > 0.6f)
            {
                healthColor = Color.green;
            }
            else if (healthPercent > 0.3f)
            {
                healthColor = Color.yellow;
            }
            else
            {
                healthColor = Color.red;
            }
            
            fillBar.color = healthColor;
        }
        
        /// <summary>
        /// 更新位置跟随目标
        /// </summary>
        private void UpdatePosition()
        {
            if (targetTransform == null) return;
            
            Vector3 worldPosition = targetTransform.position + offset;
            transform.position = worldPosition;
        }
        
        /// <summary>
        /// 更新朝向摄像机
        /// </summary>
        private void UpdateRotation()
        {
            if (playerCamera == null) return;
            
            // 让血条始终面向摄像机
            Vector3 directionToCamera = (playerCamera.transform.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(-directionToCamera);
        }
        
        /// <summary>
        /// 处理自动隐藏逻辑
        /// </summary>
        private void HandleAutoHide()
        {
            if (!alwaysShow && autoHideDelay > 0 && isVisible)
            {
                if (Time.time - lastDamageTime > autoHideDelay && targetHealth != null && targetHealth.IsFullHealth)
                {
                    SetVisible(false);
                }
            }
        }
        
        /// <summary>
        /// 设置血条可见性
        /// </summary>
        private void SetVisible(bool visible)
        {
            isVisible = visible;
            gameObject.SetActive(visible);
        }
        
        /// <summary>
        /// 强制显示血条
        /// </summary>
        public void Show()
        {
            SetVisible(true);
            lastDamageTime = Time.time;
        }
        
        /// <summary>
        /// 隐藏血条
        /// </summary>
        public void Hide()
        {
            SetVisible(false);
        }
        
        // 事件处理方法
        private void OnPlayerDamaged(PlayerDamagedEvent eventData)
        {
            if (targetHealth != null && targetTransform != null && targetTransform.CompareTag("Player"))
            {
                UpdateHealthBar();
                Show();
            }
        }
        
        private void OnEnemyDamaged(EnemyDamagedEvent eventData)
        {
            if (targetHealth != null && targetTransform != null && 
                Vector3.Distance(targetTransform.position, eventData.position) < 0.5f)
            {
                UpdateHealthBar();
                Show();
            }
        }
        
        private void OnPlayerHealed(PlayerHealedEvent eventData)
        {
            if (targetHealth != null && targetTransform != null && targetTransform.CompareTag("Player"))
            {
                UpdateHealthBar();
                Show();
            }
        }
        
        // 直接的Health组件事件处理方法（更可靠）
        private void OnHealthChanged(int currentHealth, int maxHealth)
        {
            UpdateHealthBar();
            Show();
            Debug.Log($"[WorldSpaceHealthBar] {targetTransform?.name} 血量变化: {currentHealth}/{maxHealth}");
        }
        
        private void OnDamageTaken(int damage, GameObject attacker)
        {
            UpdateHealthBar();
            Show();
            Debug.Log($"[WorldSpaceHealthBar] {targetTransform?.name} 受到 {damage} 点伤害");
        }
        
        private void OnHealed(int healAmount)
        {
            UpdateHealthBar();
            Show();
            Debug.Log($"[WorldSpaceHealthBar] {targetTransform?.name} 恢复了 {healAmount} 血量");
        }
    }
} 