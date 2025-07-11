using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core.UI
{
    /// <summary>
    /// 玩家血量UI界面组件 - 显示在屏幕上的血条
    /// </summary>
    public class PlayerHealthUI : MonoBehaviour
    {
        [Header("UI组件引用")]
        [Tooltip("血量背景图片")]
        public Image healthBackground;
        
        [Tooltip("血量填充图片")]
        public Image healthFill;
        
        [Tooltip("护盾背景图片")]
        public Image shieldBackground;
        
        [Tooltip("护盾填充图片")]
        public Image shieldFill;
        
        [Tooltip("血量文本")]
        public TextMeshProUGUI healthText;
        
        [Tooltip("护盾文本")]
        public TextMeshProUGUI shieldText;
        
        [Header("动画设置")]
        [Tooltip("血条更新平滑时间")]
        [Range(0f, 1f)]
        public float smoothTime = 0.3f;
        
        [Tooltip("血量低时闪烁")]
        public bool flashWhenLow = true;
        
        [Tooltip("低血量阈值")]
        [Range(0f, 1f)]
        public float lowHealthThreshold = 0.3f;
        
        [Header("颜色设置")]
        [Tooltip("健康血量颜色")]
        public Color healthyColor = Color.green;
        
        [Tooltip("受伤血量颜色")]
        public Color damagedColor = Color.yellow;
        
        [Tooltip("危险血量颜色")]
        public Color dangerousColor = Color.red;
        
        [Tooltip("护盾颜色")]
        public Color shieldColor = Color.cyan;
        
        // 私有变量
        private Health playerHealth;
        private float currentHealthFill;
        private float targetHealthFill;
        private float currentShieldFill;
        private float targetShieldFill;
        private float healthVelocity;
        private float shieldVelocity;
        private bool isFlashing;
        private float flashTimer;
        private const float FLASH_SPEED = 4f;
        
        private void Start()
        {
            InitializePlayerHealth();
            InitializeUI();
        }
        
        private void Update()
        {
            UpdateHealthBars();
            HandleLowHealthFlash();
        }
        
        /// <summary>
        /// 初始化玩家血量组件
        /// </summary>
        private void InitializePlayerHealth()
        {
            // 查找玩家血量组件
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<Health>();
            }
            
            if (playerHealth == null)
            {
                playerHealth = FindFirstObjectByType<Health>();
                if (playerHealth != null && !playerHealth.CompareTag("Player"))
                {
                    playerHealth = null;
                }
            }
            
            if (playerHealth != null)
            {
                // 订阅玩家血量事件
                playerHealth.OnHealthChanged += OnPlayerHealthChanged;
                playerHealth.OnShieldChanged += OnPlayerShieldChanged;
                playerHealth.OnDamageTaken += OnPlayerDamageTaken;
                playerHealth.OnHealed += OnPlayerHealed;
                
                // 初始化血条值
                UpdateHealthDisplay(playerHealth.CurrentHealth, playerHealth.MaxHealth, true);
                if (playerHealth.HasShield)
                {
                    UpdateShieldDisplay(playerHealth.CurrentShield, playerHealth.MaxShield, true);
                }
                
                Debug.Log($"[PlayerHealthUI] 成功连接到玩家血量组件: {playerHealth.gameObject.name}");
            }
            else
            {
                Debug.LogError("[PlayerHealthUI] 未找到玩家血量组件！");
            }
        }
        
        /// <summary>
        /// 初始化UI显示
        /// </summary>
        private void InitializeUI()
        {
            // 设置初始填充值
            currentHealthFill = 1f;
            targetHealthFill = 1f;
            currentShieldFill = 1f;
            targetShieldFill = 1f;
            
            // 初始化血条填充
            if (healthFill != null)
            {
                healthFill.fillAmount = currentHealthFill;
                healthFill.color = healthyColor;
            }
            
            if (shieldFill != null)
            {
                shieldFill.fillAmount = currentShieldFill;
                shieldFill.color = shieldColor;
            }
            
            // 如果没有护盾，隐藏护盾UI
            if (playerHealth != null && !playerHealth.HasShield)
            {
                if (shieldBackground != null) shieldBackground.gameObject.SetActive(false);
                if (shieldFill != null) shieldFill.gameObject.SetActive(false);
                if (shieldText != null) shieldText.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// 更新血条显示
        /// </summary>
        private void UpdateHealthBars()
        {
            // 平滑更新血量条
            if (Mathf.Abs(currentHealthFill - targetHealthFill) > 0.01f)
            {
                currentHealthFill = Mathf.SmoothDamp(currentHealthFill, targetHealthFill, ref healthVelocity, smoothTime);
                if (healthFill != null)
                {
                    healthFill.fillAmount = currentHealthFill;
                }
            }
            
            // 平滑更新护盾条
            if (Mathf.Abs(currentShieldFill - targetShieldFill) > 0.01f)
            {
                currentShieldFill = Mathf.SmoothDamp(currentShieldFill, targetShieldFill, ref shieldVelocity, smoothTime);
                if (shieldFill != null)
                {
                    shieldFill.fillAmount = currentShieldFill;
                }
            }
        }
        
        /// <summary>
        /// 处理低血量闪烁效果
        /// </summary>
        private void HandleLowHealthFlash()
        {
            if (!flashWhenLow || playerHealth == null) return;
            
            bool shouldFlash = playerHealth.HealthPercentage <= lowHealthThreshold && !playerHealth.IsDead;
            
            if (shouldFlash != isFlashing)
            {
                isFlashing = shouldFlash;
                flashTimer = 0f;
            }
            
            if (isFlashing && healthFill != null)
            {
                flashTimer += Time.deltaTime * FLASH_SPEED;
                float alpha = Mathf.Lerp(0.3f, 1f, (Mathf.Sin(flashTimer) + 1f) * 0.5f);
                
                Color currentColor = healthFill.color;
                currentColor.a = alpha;
                healthFill.color = currentColor;
            }
        }
        
        /// <summary>
        /// 更新血量显示
        /// </summary>
        private void UpdateHealthDisplay(int currentHealth, int maxHealth, bool immediate = false)
        {
            if (maxHealth <= 0) return;
            
            float healthPercent = (float)currentHealth / maxHealth;
            targetHealthFill = healthPercent;
            
            if (immediate)
            {
                currentHealthFill = targetHealthFill;
                if (healthFill != null)
                {
                    healthFill.fillAmount = currentHealthFill;
                }
            }
            
            // 更新血量颜色
            if (healthFill != null && !isFlashing)
            {
                Color newColor;
                if (healthPercent > 0.6f)
                {
                    newColor = healthyColor;
                }
                else if (healthPercent > 0.3f)
                {
                    newColor = damagedColor;
                }
                else
                {
                    newColor = dangerousColor;
                }
                healthFill.color = newColor;
            }
            
            // 更新血量文本
            if (healthText != null)
            {
                healthText.text = $"{currentHealth} / {maxHealth}";
            }
        }
        
        /// <summary>
        /// 更新护盾显示
        /// </summary>
        private void UpdateShieldDisplay(int currentShield, int maxShield, bool immediate = false)
        {
            if (maxShield <= 0) return;
            
            float shieldPercent = (float)currentShield / maxShield;
            targetShieldFill = shieldPercent;
            
            if (immediate)
            {
                currentShieldFill = targetShieldFill;
                if (shieldFill != null)
                {
                    shieldFill.fillAmount = currentShieldFill;
                }
            }
            
            // 更新护盾文本
            if (shieldText != null)
            {
                shieldText.text = $"{currentShield} / {maxShield}";
            }
            
            // 显示/隐藏护盾UI
            bool shouldShow = currentShield > 0 || maxShield > 0;
            if (shieldBackground != null) shieldBackground.gameObject.SetActive(shouldShow);
            if (shieldFill != null) shieldFill.gameObject.SetActive(shouldShow);
            if (shieldText != null) shieldText.gameObject.SetActive(shouldShow);
        }
        
        // 事件处理方法
        private void OnPlayerHealthChanged(int currentHealth, int maxHealth)
        {
            UpdateHealthDisplay(currentHealth, maxHealth);
            Debug.Log($"[PlayerHealthUI] 血量更新: {currentHealth}/{maxHealth}");
        }
        
        private void OnPlayerShieldChanged(int currentShield, int maxShield)
        {
            UpdateShieldDisplay(currentShield, maxShield);
            Debug.Log($"[PlayerHealthUI] 护盾更新: {currentShield}/{maxShield}");
        }
        
        private void OnPlayerDamageTaken(int damage, GameObject attacker)
        {
            Debug.Log($"[PlayerHealthUI] 玩家受到 {damage} 点伤害");
            
            // 可以在这里添加受伤效果，比如屏幕震动、红色闪烁等
            StartCoroutine(DamageFlashEffect());
        }
        
        private void OnPlayerHealed(int healAmount)
        {
            Debug.Log($"[PlayerHealthUI] 玩家恢复了 {healAmount} 血量");
            
            // 可以在这里添加治疗效果
            StartCoroutine(HealFlashEffect());
        }
        
        /// <summary>
        /// 受伤闪烁效果
        /// </summary>
        private System.Collections.IEnumerator DamageFlashEffect()
        {
            if (healthBackground == null) yield break;
            
            Color originalColor = healthBackground.color;
            Color flashColor = new Color(1f, 0.2f, 0.2f, 0.3f);
            
            // 闪烁效果
            healthBackground.color = flashColor;
            yield return new WaitForSeconds(0.1f);
            healthBackground.color = originalColor;
        }
        
        /// <summary>
        /// 治疗闪烁效果
        /// </summary>
        private System.Collections.IEnumerator HealFlashEffect()
        {
            if (healthBackground == null) yield break;
            
            Color originalColor = healthBackground.color;
            Color flashColor = new Color(0.2f, 1f, 0.2f, 0.3f);
            
            // 闪烁效果
            healthBackground.color = flashColor;
            yield return new WaitForSeconds(0.1f);
            healthBackground.color = originalColor;
        }
        
        private void OnDestroy()
        {
            // 取消订阅事件
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged -= OnPlayerHealthChanged;
                playerHealth.OnShieldChanged -= OnPlayerShieldChanged;
                playerHealth.OnDamageTaken -= OnPlayerDamageTaken;
                playerHealth.OnHealed -= OnPlayerHealed;
            }
        }
        
        #if UNITY_EDITOR
        [ContextMenu("测试伤害效果")]
        private void TestDamageEffect()
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(10);
            }
        }
        
        [ContextMenu("测试治疗效果")]
        private void TestHealEffect()
        {
            if (playerHealth != null)
            {
                playerHealth.Heal(20);
            }
        }
        
        [ContextMenu("刷新玩家血量组件")]
        private void RefreshPlayerHealth()
        {
            InitializePlayerHealth();
        }
        #endif
    }
} 