using UnityEngine;

namespace Core
{
    /// <summary>
    /// 游戏核心管理器 - 使用ServiceLocator模式管理游戏中的各种服务
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        
        [Header("服务管理器引用")]
        [SerializeField] private EffectsManager effectsManager;
        [SerializeField] private ObjectPoolManager objectPoolManager;
        [SerializeField] private SaveManager saveManager;
        [SerializeField] private EventManager eventManager;
        
        
        // 服务接口访问
        public IEffectsManager Effects => effectsManager;
        public IObjectPoolManager ObjectPool => objectPoolManager;
        public ISaveManager Save => saveManager;
        public EventManager Events => eventManager;
        
        [Header("游戏设置")]
        [SerializeField] private bool initializeOnAwake = true;

        
        // 游戏状态
        public bool IsGameInitialized { get; private set; }
        public bool IsGamePaused { get; private set; }
        
        protected override void Awake()
        {
            base.Awake();
            if (initializeOnAwake)
                InitializeGame();
        }
        
        /// <summary>
        /// 初始化游戏系统
        /// </summary>
        private void InitializeGame()
        {
            if (IsGameInitialized)
            {
                Debug.LogWarning("游戏已经初始化过了");
                return;
            }
            
            Debug.Log("开始初始化游戏系统...");
            
            // 按依赖顺序初始化各个服务
            InitializeServices();
            
            IsGameInitialized = true;
            Debug.Log("游戏系统初始化完成！");
        }
        
        /// <summary>
        /// 初始化各个服务组件
        /// </summary>
        private void InitializeServices()
        {
            // 自动查找服务组件（如果没有手动分配）
            if (effectsManager == null) effectsManager = FindFirstObjectByType<EffectsManager>();
            if (objectPoolManager == null) objectPoolManager = FindFirstObjectByType<ObjectPoolManager>();
            if (saveManager == null) saveManager = FindFirstObjectByType<SaveManager>();
            if (eventManager == null) eventManager = EventManager.Instance;
            
            // 初始化对象池管理器（优先级最高）
            if (objectPoolManager != null)
            {
                objectPoolManager.Initialize();
                Debug.Log("✓ 对象池管理器初始化完成");
            }
            else
            {
                Debug.LogWarning("未找到ObjectPoolManager组件");
            }
            

            
            // 初始化特效管理器
            if (effectsManager != null)
            {
                effectsManager.Initialize();
                Debug.Log("✓ 特效管理器初始化完成");
            }
            else
            {
                Debug.LogWarning("未找到EffectsManager组件");
            }

            
            // 初始化存档管理器
            if (saveManager != null)
            {
                saveManager.Initialize();
                Debug.Log("✓ 存档管理器初始化完成");
            }
            else
            {
                Debug.LogWarning("未找到SaveManager组件");
            }
            
            // 确认事件管理器状态
            if (eventManager != null)
            {
                Debug.Log("✓ 事件管理器已连接");
            }
            else
            {
                Debug.LogWarning("未找到EventManager组件");
            }
        }
        
        /// <summary>
        /// 暂停游戏
        /// </summary>
        public void PauseGame()
        {
            if (!IsGamePaused)
            {
                Time.timeScale = 0f;
                IsGamePaused = true;
                Debug.Log("游戏已暂停");
            }
        }
        
        /// <summary>
        /// 恢复游戏
        /// </summary>
        public void ResumeGame()
        {
            if (IsGamePaused)
            {
                Time.timeScale = 1f;
                IsGamePaused = false;
                Debug.Log("游戏已恢复");
            }
        }
        
        /// <summary>
        /// 退出游戏
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("退出游戏");
            
            // 保存游戏数据
            if (saveManager != null)
            {
                saveManager.SaveGame();
            }
            
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        

        
        #if UNITY_EDITOR
        [ContextMenu("重新初始化游戏系统")]
        private void ReinitializeGame()
        {
            IsGameInitialized = false;
            InitializeGame();
        }
        
        [ContextMenu("显示服务状态")]
        private void ShowServiceStatus()
        {
            Debug.Log($"=== 游戏服务状态 ===");
            Debug.Log($"游戏已初始化: {IsGameInitialized}");
            Debug.Log($"游戏已暂停: {IsGamePaused}");
            Debug.Log($"特效管理器: {(effectsManager != null ? "✓" : "✗")}");
            Debug.Log($"对象池管理器: {(objectPoolManager != null ? "✓" : "✗")}");
            Debug.Log($"存档管理器: {(saveManager != null ? "✓" : "✗")}");
            Debug.Log($"事件管理器: {(eventManager != null ? "✓" : "✗")}");
        }
        #endif
    }
} 