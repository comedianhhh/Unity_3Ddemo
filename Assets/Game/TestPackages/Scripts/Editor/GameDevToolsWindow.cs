#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Core.ScriptableObjects;
using Core;
public class GameDevToolsWindow : EditorWindow
{
    private Vector2 scrollPosition;
    private int selectedTab = 0;
    private string[] tabNames = { "对象创建", "场景工具", "配置管理", "调试工具", "事件系统" };
    
    
    // 场景工具相关
    private LayerMask groundLayer = 1;
    private float spawnHeight = 10f;
    private int spawnCount = 5;
    private float spawnRadius = 10f;
    
    [MenuItem("Game Dev/开发工具窗口")]
    public static void ShowWindow()
    {
        var window = GetWindow<GameDevToolsWindow>("游戏开发工具");
        window.minSize = new Vector2(400, 600);
    }
    
    private void OnGUI()
    {
        DrawHeader();
        
        selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        switch (selectedTab)
        {
            case 0: DrawObjectCreationTab(); break;
            case 1: DrawSceneToolsTab(); break;
            case 2: DrawConfigManagementTab(); break;
            case 3: DrawDebugToolsTab(); break;
            case 4: DrawEventSystemTab(); break;
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawHeader()
    {
        EditorGUILayout.BeginVertical("box");
        
        GUILayout.Label("游戏开发工具", EditorStyles.largeLabel);
        GUILayout.Label("提供各种便捷的开发和调试工具", EditorStyles.helpBox);
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }
    
    private void DrawObjectCreationTab()
    {
        GUILayout.Label("对象创建工具", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical("box");
        
        EditorGUILayout.Space();
        
        
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("创建GameManager", GUILayout.Height(30)))
            {
                CreateGameManager();
            }
            
            if (GUILayout.Button("创建EventManager", GUILayout.Height(30)))
            {
                CreateEventManager();
            }
        }
        
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("创建血条管理器", GUILayout.Height(30)))
            {
                CreateHealthBarManager();
            }
            
            if (GUILayout.Button("创建血条测试器", GUILayout.Height(30)))
            {
                CreateHealthBarTester();
            }
        }
        
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("创建玩家血条UI", GUILayout.Height(30)))
            {
                CreatePlayerHealthUI();
            }
        }
        
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("创建UI画布", GUILayout.Height(30)))
            {
                CreateUICanvas();
            }
        }
        
        EditorGUILayout.Space();
        
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawSceneToolsTab()
    {
        GUILayout.Label("场景工具", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical("box");
        
        // 对象批量放置
        GUILayout.Label("批量对象放置", EditorStyles.boldLabel);
        
        spawnCount = EditorGUILayout.IntSlider("生成数量", spawnCount, 1, 20);
        spawnRadius = EditorGUILayout.Slider("生成半径", spawnRadius, 1f, 50f);
        spawnHeight = EditorGUILayout.Slider("生成高度", spawnHeight, 1f, 20f);
        groundLayer = EditorGUILayout.LayerField("地面图层", groundLayer);
        
        if (GUILayout.Button("随机放置选中对象"))
        {
            RandomPlaceSelectedObjects();
        }
        
        EditorGUILayout.Space();
        
        // 场景清理工具
        GUILayout.Label("场景清理", EditorStyles.boldLabel);
        
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("清理空对象"))
            {
                CleanEmptyGameObjects();
            }
            
            if (GUILayout.Button("清理缺失脚本"))
            {
                CleanMissingScripts();
            }
        }
        
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("对齐选中对象到地面"))
            {
                AlignSelectedToGround();
            }
            
            if (GUILayout.Button("重置选中对象位置"))
            {
                ResetSelectedPositions();
            }
        }
        
        EditorGUILayout.Space();
        
        // 血条工具
        GUILayout.Label("血条工具", EditorStyles.boldLabel);
        
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("刷新所有血条"))
            {
                RefreshAllHealthBars();
            }
            
            if (GUILayout.Button("清理血条"))
            {
                ClearAllHealthBars();
            }
        }
        
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("为选中对象添加血条"))
            {
                AddHealthBarsToSelected();
            }
            
            if (GUILayout.Button("显示血条统计"))
            {
                ShowHealthBarStatistics();
            }
        }
        
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("强制刷新血条连接"))
            {
                ForceRefreshHealthBarConnections();
            }
            
            if (GUILayout.Button("测试血条更新"))
            {
                TestHealthBarUpdates();
            }
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawConfigManagementTab()
    {
        GUILayout.Label("配置管理", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical("box");
        
        GUILayout.Label("ScriptableObject管理", EditorStyles.boldLabel);
        
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("创建武器配置"))
            {
                CreateWeaponConfig();
            }
            
            if (GUILayout.Button("创建怪物配置"))
            {
                CreateMonsterConfig();
            }
        }
        
        if (GUILayout.Button("显示所有配置文件"))
        {
            ShowAllConfigs();
        }
        
        EditorGUILayout.Space();
        
        // 配置验证
        GUILayout.Label("配置验证", EditorStyles.boldLabel);
        
        if (GUILayout.Button("验证所有武器配置"))
        {
            ValidateAllWeaponConfigs();
        }
        
        if (GUILayout.Button("验证所有怪物配置"))
        {
            ValidateAllMonsterConfigs();
        }
        
        EditorGUILayout.Space();
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawDebugToolsTab()
    {
        GUILayout.Label("调试工具", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical("box");
        
        // 组件查找
        GUILayout.Label("组件查找", EditorStyles.boldLabel);
        
        if (GUILayout.Button("查找所有MonsterAI"))
        {
            FindAndSelectComponents<MonsterAI>();
        }
        
        if (GUILayout.Button("查找所有Health组件"))
        {
            FindAndSelectComponents<Health>();
        }
        
        if (GUILayout.Button("查找所有血条管理器"))
        {
            FindAndSelectComponents<Core.UI.HealthBarManager>();
        }
        
        if (GUILayout.Button("查找缺失组件的对象"))
        {
            FindObjectsWithMissingComponents();
        }
        
        EditorGUILayout.Space();
        
        // 性能工具
        GUILayout.Label("性能工具", EditorStyles.boldLabel);
        
        if (GUILayout.Button("显示场景统计"))
        {
            ShowSceneStatistics();
        }
        
        if (GUILayout.Button("优化建议"))
        {
            ShowOptimizationSuggestions();
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawEventSystemTab()
    {
        GUILayout.Label("事件系统", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical("box");
        
        // 事件测试
        GUILayout.Label("事件测试", EditorStyles.boldLabel);
        
        if (GUILayout.Button("发布测试玩家受伤事件"))
        {
            if (Application.isPlaying)
            {
                EventManager.Publish(new PlayerDamagedEvent(10, 90, 100, Vector3.zero));
            }
            else
            {
                EditorUtility.DisplayDialog("提示", "需要在运行时才能发布事件", "确定");
            }
        }
        
        if (GUILayout.Button("发布测试游戏暂停事件"))
        {
            if (Application.isPlaying)
            {
                EventManager.Publish(new GamePausedEvent(true));
            }
            else
            {
                EditorUtility.DisplayDialog("提示", "需要在运行时才能发布事件", "确定");
            }
        }
        
        EditorGUILayout.Space();
        
        // 事件监控
        GUILayout.Label("事件监控", EditorStyles.boldLabel);
        
        if (Application.isPlaying)
        {
            var eventStats = EventManager.GetEventStats();
            if (eventStats.Count > 0)
            {
                GUILayout.Label("事件发布统计:", EditorStyles.boldLabel);
                foreach (var kvp in eventStats)
                {
                    EditorGUILayout.LabelField(kvp.Key.Name, kvp.Value.ToString());
                }
            }
            else
            {
                GUILayout.Label("暂无事件统计数据");
            }
        }
        else
        {
            GUILayout.Label("需要运行游戏才能显示事件统计");
        }
        
        EditorGUILayout.EndVertical();
    }
    private void CreateGameManager()
    {
        if (FindFirstObjectByType<Core.GameManager>() != null)
        {
            EditorUtility.DisplayDialog("提示", "场景中已存在GameManager", "确定");
            return;
        }
        
        var gameManager = new GameObject("Game Manager");
        gameManager.AddComponent<Core.GameManager>();
        gameManager.AddComponent<Core.ObjectPoolManager>();
        gameManager.AddComponent<Core.EffectsManager>();
        gameManager.AddComponent<Core.SaveManager>();
        
        Undo.RegisterCreatedObjectUndo(gameManager, "Create Game Manager");
        Selection.activeGameObject = gameManager;
    }
    
    private void CreateEventManager()
    {
        if (FindFirstObjectByType<EventManager>() != null)
        {
            EditorUtility.DisplayDialog("提示", "场景中已存在EventManager", "确定");
            return;
        }
        
        var eventManager = new GameObject("Event Manager");
        eventManager.AddComponent<EventManager>();
        
        Undo.RegisterCreatedObjectUndo(eventManager, "Create Event Manager");
        Selection.activeGameObject = eventManager;
    }
    
    private void CreateHealthBarManager()
    {
        if (FindFirstObjectByType<Core.UI.HealthBarManager>() != null)
        {
            EditorUtility.DisplayDialog("提示", "场景中已存在HealthBarManager", "确定");
            return;
        }
        
        var healthBarManager = new GameObject("Health Bar Manager");
        healthBarManager.AddComponent<Core.UI.HealthBarManager>();
        
        Undo.RegisterCreatedObjectUndo(healthBarManager, "Create Health Bar Manager");
        Selection.activeGameObject = healthBarManager;
        
        Debug.Log("已创建血条管理器，它会自动为有Health组件的对象创建头顶血条");
    }
    
    private void CreateHealthBarTester()
    {
        if (FindFirstObjectByType<Core.UI.HealthBarTester>() != null)
        {
            EditorUtility.DisplayDialog("提示", "场景中已存在HealthBarTester", "确定");
            return;
        }
        
        var healthBarTester = new GameObject("Health Bar Tester");
        healthBarTester.AddComponent<Core.UI.HealthBarTester>();
        
        Undo.RegisterCreatedObjectUndo(healthBarTester, "Create Health Bar Tester");
        Selection.activeGameObject = healthBarTester;
        
        Debug.Log("已创建血条测试器，可以在运行时快速测试血条功能。快捷键: X-伤害, C-治疗, R-刷新血条");
    }
    
    private void CreatePlayerHealthUI()
    {
        // 查找或创建Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            CreateUICanvas();
            canvas = FindFirstObjectByType<Canvas>();
        }
        
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("错误", "无法创建或找到Canvas", "确定");
            return;
        }
        
        // 创建PlayerHealthUI容器
        GameObject playerHealthUI = new GameObject("PlayerHealthUI");
        playerHealthUI.transform.SetParent(canvas.transform, false);
        
        // 设置位置到屏幕左上角
        RectTransform healthUIRect = playerHealthUI.AddComponent<RectTransform>();
        healthUIRect.anchorMin = new Vector2(0, 1);
        healthUIRect.anchorMax = new Vector2(0, 1);
        healthUIRect.anchoredPosition = new Vector2(100, -30);
        healthUIRect.sizeDelta = new Vector2(200, 60);
        
        // 创建血条背景
        GameObject healthBarBG = new GameObject("HealthBarBackground");
        healthBarBG.transform.SetParent(playerHealthUI.transform, false);
        
        UnityEngine.UI.Image healthBGImage = healthBarBG.AddComponent<UnityEngine.UI.Image>();
        healthBGImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        RectTransform healthBGRect = healthBarBG.GetComponent<RectTransform>();
        healthBGRect.anchorMin = Vector2.zero;
        healthBGRect.anchorMax = Vector2.one;
        healthBGRect.sizeDelta = Vector2.zero;
        healthBGRect.anchoredPosition = Vector2.zero;
        
        // 创建血条填充
        GameObject healthBarFill = new GameObject("HealthBarFill");
        healthBarFill.transform.SetParent(playerHealthUI.transform, false);
        
        UnityEngine.UI.Image healthFillImage = healthBarFill.AddComponent<UnityEngine.UI.Image>();
        healthFillImage.color = Color.red;
        healthFillImage.type = UnityEngine.UI.Image.Type.Filled;
        healthFillImage.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
        
        RectTransform healthFillRect = healthBarFill.GetComponent<RectTransform>();
        healthFillRect.anchorMin = Vector2.zero;
        healthFillRect.anchorMax = Vector2.one;
        healthFillRect.sizeDelta = Vector2.zero;
        healthFillRect.anchoredPosition = Vector2.zero;
        
        // 创建血量文本
        GameObject healthText = new GameObject("HealthText");
        healthText.transform.SetParent(playerHealthUI.transform, false);
        
        TMPro.TextMeshProUGUI healthTextComponent = healthText.AddComponent<TMPro.TextMeshProUGUI>();
        healthTextComponent.text = "100 / 100";
        healthTextComponent.fontSize = 14;
        healthTextComponent.color = Color.white;
        healthTextComponent.alignment = TMPro.TextAlignmentOptions.Center;
        
        RectTransform healthTextRect = healthText.GetComponent<RectTransform>();
        healthTextRect.anchorMin = Vector2.zero;
        healthTextRect.anchorMax = Vector2.one;
        healthTextRect.sizeDelta = Vector2.zero;
        healthTextRect.anchoredPosition = Vector2.zero;
        
        // 添加PlayerHealthUI组件
        var playerHealthUIComponent = playerHealthUI.AddComponent<Core.UI.PlayerHealthUI>();
        playerHealthUIComponent.healthBackground = healthBGImage;
        playerHealthUIComponent.healthFill = healthFillImage;
        playerHealthUIComponent.healthText = healthTextComponent;
        
        Undo.RegisterCreatedObjectUndo(playerHealthUI, "Create Player Health UI");
        Selection.activeGameObject = playerHealthUI;
        
        Debug.Log("已创建玩家血条UI，它会自动连接到玩家的Health组件");
    }
    
    private void CreateUICanvas()
    {
        // 检查是否已存在Canvas
        Canvas existingCanvas = FindFirstObjectByType<Canvas>();
        if (existingCanvas != null && existingCanvas.renderMode != RenderMode.WorldSpace)
        {
            EditorUtility.DisplayDialog("提示", "场景中已存在UI Canvas", "确定");
            return;
        }
        
        // 创建主UI Canvas
        var canvasGO = new GameObject("Main UI Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;
        
        // 添加GraphicRaycaster
        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        // 创建EventSystem
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        
        Undo.RegisterCreatedObjectUndo(canvasGO, "Create UI Canvas");
        Selection.activeGameObject = canvasGO;
        
        Debug.Log("已创建UI画布，可以在此基础上添加传统UI元素");
    }
    private void RandomPlaceSelectedObjects()
    {
        var selected = Selection.gameObjects;
        if (selected.Length == 0)
        {
            EditorUtility.DisplayDialog("提示", "请先选择要放置的对象", "确定");
            return;
        }
        
        for (int i = 0; i < spawnCount; i++)
        {
            var randomObject = selected[Random.Range(0, selected.Length)];
            var copy = Instantiate(randomObject);
            
            // 随机位置
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = new Vector3(randomCircle.x, spawnHeight, randomCircle.y);
            
            // 射线检测地面
            if (Physics.Raycast(spawnPos, Vector3.down, out RaycastHit hit, spawnHeight + 10f, groundLayer))
            {
                copy.transform.position = hit.point;
            }
            else
            {
                copy.transform.position = spawnPos;
            }
            
            Undo.RegisterCreatedObjectUndo(copy, "Random Place Object");
        }
    }
    
    private void FindAndSelectComponents<T>() where T : Component
    {
        var components = FindObjectsByType<T>(FindObjectsSortMode.None);
        var gameObjects = components.Select(c => c.gameObject).ToArray();
        Selection.objects = gameObjects;
        
        Debug.Log($"找到 {components.Length} 个 {typeof(T).Name} 组件");
    }
    
    private void ShowSceneStatistics()
    {
        var allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        var renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        var colliders = FindObjectsByType<Collider>(FindObjectsSortMode.None);
        var rigidbodies = FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);
        
        string stats = $"场景统计信息:\n" +
                      $"总对象数: {allObjects.Length}\n" +
                      $"渲染器数: {renderers.Length}\n" +
                      $"碰撞器数: {colliders.Length}\n" +
                      $"刚体数: {rigidbodies.Length}";
        
        EditorUtility.DisplayDialog("场景统计", stats, "确定");
    }
    
    private void ValidateAllWeaponConfigs()
    {
        var configs = Resources.FindObjectsOfTypeAll<WeaponConfig>();
        int validCount = 0;
        int invalidCount = 0;
        
        foreach (var config in configs)
        {
            if (config.IsValid())
                validCount++;
            else
                invalidCount++;
        }
        
        EditorUtility.DisplayDialog("配置验证", 
            $"武器配置验证完成:\n有效: {validCount}\n无效: {invalidCount}", "确定");
    }
    
    private void ValidateAllMonsterConfigs()
    {
        var configs = Resources.FindObjectsOfTypeAll<MonsterConfig>();
        int validCount = 0;
        int invalidCount = 0;
        
        foreach (var config in configs)
        {
            if (config.IsValid())
                validCount++;
            else
                invalidCount++;
        }
        
        EditorUtility.DisplayDialog("配置验证", 
            $"怪物配置验证完成:\n有效: {validCount}\n无效: {invalidCount}", "确定");
    }
    
    private void CreateWeaponConfig()
    {
        var config = CreateInstance<WeaponConfig>();
        ProjectWindowUtil.CreateAsset(config, "NewWeaponConfig.asset");
    }
    
    private void CreateMonsterConfig()
    {
        var config = CreateInstance<MonsterConfig>();
        ProjectWindowUtil.CreateAsset(config, "NewMonsterConfig.asset");
    }
    
    private void ShowAllConfigs()
    {
        var weaponConfigs = Resources.FindObjectsOfTypeAll<WeaponConfig>();
        var monsterConfigs = Resources.FindObjectsOfTypeAll<MonsterConfig>();
        
        Debug.Log($"=== 所有配置文件 ===");
        Debug.Log($"武器配置 ({weaponConfigs.Length}):");
        foreach (var config in weaponConfigs)
        {
            Debug.Log($"  - {config.name}: {config.weaponName}");
        }
        
        Debug.Log($"怪物配置 ({monsterConfigs.Length}):");
        foreach (var config in monsterConfigs)
        {
            Debug.Log($"  - {config.name}: {config.monsterName}");
        }
    }
    
    private void CleanEmptyGameObjects()
    {
        var allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int cleaned = 0;
        
        foreach (var obj in allObjects)
        {
            if (obj.transform.childCount == 0 && obj.GetComponents<Component>().Length == 1)
            {
                DestroyImmediate(obj);
                cleaned++;
            }
        }
        
        EditorUtility.DisplayDialog("清理完成", $"清理了 {cleaned} 个空对象", "确定");
    }
    
    private void CleanMissingScripts()
    {
        var allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int cleaned = 0;
        
        foreach (var obj in allObjects)
        {
            var components = obj.GetComponents<Component>();
            for (int i = components.Length - 1; i >= 0; i--)
            {
                if (components[i] == null)
                {
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
                    cleaned++;
                    break;
                }
            }
        }
        
        EditorUtility.DisplayDialog("清理完成", $"清理了 {cleaned} 个缺失脚本", "确定");
    }
    
    private void AlignSelectedToGround()
    {
        foreach (var obj in Selection.gameObjects)
        {
            if (Physics.Raycast(obj.transform.position + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f))
            {
                Undo.RecordObject(obj.transform, "Align to Ground");
                obj.transform.position = hit.point;
            }
        }
    }
    
    private void ResetSelectedPositions()
    {
        foreach (var obj in Selection.gameObjects)
        {
            Undo.RecordObject(obj.transform, "Reset Position");
            obj.transform.position = Vector3.zero;
        }
    }
    
    private void FindObjectsWithMissingComponents()
    {
        var objectsWithMissing = new List<GameObject>();
        var allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        
        foreach (var obj in allObjects)
        {
            var components = obj.GetComponents<Component>();
            foreach (var component in components)
            {
                if (component == null)
                {
                    objectsWithMissing.Add(obj);
                    break;
                }
            }
        }
        
        Selection.objects = objectsWithMissing.ToArray();
        Debug.Log($"找到 {objectsWithMissing.Count} 个有缺失组件的对象");
    }
    
    private void ShowOptimizationSuggestions()
    {
        var suggestions = new List<string>();
        
        var renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        if (renderers.Length > 100)
        {
            suggestions.Add($"场景中有 {renderers.Length} 个渲染器，考虑使用批处理优化");
        }
        
        var rigidbodies = FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);
        int kinematicCount = rigidbodies.Count(rb => rb.isKinematic);
        if (kinematicCount > rigidbodies.Length * 0.8f)
        {
            suggestions.Add("大量刚体设置为Kinematic，考虑使用静态碰撞器");
        }
        
        if (suggestions.Count == 0)
        {
            suggestions.Add("暂无明显的优化建议");
        }
        
        string message = "优化建议:\n" + string.Join("\n", suggestions);
        EditorUtility.DisplayDialog("优化建议", message, "确定");
    }
    
     private void RefreshAllHealthBars()
     {
         var healthBarManager = FindFirstObjectByType<Core.UI.HealthBarManager>();
         if (healthBarManager == null)
         {
             EditorUtility.DisplayDialog("提示", "场景中没有找到HealthBarManager", "确定");
             return;
         }
         
         if (Application.isPlaying)
         {
             healthBarManager.DetectAndCreateHealthBars();
             Debug.Log("已刷新血条系统");
         }
         else
         {
             EditorUtility.DisplayDialog("提示", "需要在运行时才能刷新血条", "确定");
         }
     }
     
     private void ClearAllHealthBars()
     {
         var healthBarManager = FindFirstObjectByType<Core.UI.HealthBarManager>();
         if (healthBarManager == null)
         {
             EditorUtility.DisplayDialog("提示", "场景中没有找到HealthBarManager", "确定");
             return;
         }
         
         if (Application.isPlaying)
         {
             healthBarManager.ClearAllHealthBars();
             Debug.Log("已清理所有血条");
         }
         else
         {
             // 在编辑模式下查找并删除血条对象
             var healthBars = FindObjectsByType<Core.UI.WorldSpaceHealthBar>(FindObjectsSortMode.None);
             int removedCount = 0;
             foreach (var healthBar in healthBars)
             {
                 DestroyImmediate(healthBar.gameObject);
                 removedCount++;
             }
             
             Debug.Log($"已清理 {removedCount} 个血条对象");
             EditorUtility.DisplayDialog("完成", $"已清理 {removedCount} 个血条对象", "确定");
         }
     }
     
     private void AddHealthBarsToSelected()
     {
         var selected = Selection.gameObjects;
         if (selected.Length == 0)
         {
             EditorUtility.DisplayDialog("提示", "请先选择要添加血条的对象", "确定");
             return;
         }
         
         var healthBarManager = FindFirstObjectByType<Core.UI.HealthBarManager>();
         if (healthBarManager == null)
         {
             EditorUtility.DisplayDialog("提示", "场景中没有找到HealthBarManager", "确定");
             return;
         }
         
         int addedCount = 0;
         foreach (var obj in selected)
         {
             var health = obj.GetComponent<Health>();
             if (health != null)
             {
                 if (Application.isPlaying)
                 {
                     healthBarManager.CreateHealthBar(health);
                     addedCount++;
                 }
                 else
                 {
                     Debug.Log($"对象 {obj.name} 有Health组件，运行时将自动创建血条");
                     addedCount++;
                 }
             }
         }
         
         string message = Application.isPlaying ? 
             $"为 {addedCount} 个对象创建了血条" : 
             $"找到 {addedCount} 个有Health组件的对象，运行时将自动创建血条";
             
         Debug.Log(message);
         EditorUtility.DisplayDialog("完成", message, "确定");
     }
     
     private void ShowHealthBarStatistics()
     {
         var healthBarManager = FindFirstObjectByType<Core.UI.HealthBarManager>();
         var allHealthComponents = FindObjectsByType<Health>(FindObjectsSortMode.None);
         var allHealthBars = FindObjectsByType<Core.UI.WorldSpaceHealthBar>(FindObjectsSortMode.None);
         
         string stats = "=== 血条系统统计 ===\n\n";
         stats += $"血条管理器: {(healthBarManager != null ? "✓ 存在" : "✗ 缺失")}\n";
         stats += $"Health组件总数: {allHealthComponents.Length}\n";
         stats += $"血条总数: {allHealthBars.Length}\n";
         
         if (healthBarManager != null && Application.isPlaying)
         {
             stats += $"活跃血条数: {healthBarManager.GetActiveHealthBarCount()}\n";
         }
         
         stats += "\n=== Health组件详情 ===\n";
         foreach (var health in allHealthComponents)
         {
             string tag = health.CompareTag("Player") ? "[玩家]" : 
                         health.CompareTag("Enemy") ? "[敌人]" : 
                         health.CompareTag("NPC") ? "[NPC]" : "[其他]";
             stats += $"  {tag} {health.gameObject.name} - HP: {health.CurrentHealth}/{health.MaxHealth}\n";
         }
         
         Debug.Log(stats);
         
         string summary = $"Health组件: {allHealthComponents.Length}\n血条对象: {allHealthBars.Length}";
         if (healthBarManager != null && Application.isPlaying)
         {
             summary += $"\n活跃血条: {healthBarManager.GetActiveHealthBarCount()}";
         }
         summary += "\n\n详细信息请查看控制台";
         
         EditorUtility.DisplayDialog("血条系统统计", summary, "确定");
     }
     
     /// <summary>
     /// 强制刷新所有血条的连接
     /// </summary>
     private void ForceRefreshHealthBarConnections()
     {
         var allHealthBars = FindObjectsByType<Core.UI.WorldSpaceHealthBar>(FindObjectsSortMode.None);
         var allHealthComponents = FindObjectsByType<Health>(FindObjectsSortMode.None);
         
         int refreshed = 0;
         
         foreach (var healthBar in allHealthBars)
         {
             if (healthBar != null)
             {
                 // 找到最近的Health组件
                 Health nearestHealth = null;
                 float minDistance = float.MaxValue;
                 
                 foreach (var health in allHealthComponents)
                 {
                     if (health != null)
                     {
                         float distance = Vector3.Distance(healthBar.transform.position, health.transform.position);
                         if (distance < minDistance)
                         {
                             minDistance = distance;
                             nearestHealth = health;
                         }
                     }
                 }
                 
                 // 如果找到了附近的Health组件，重新初始化血条
                 if (nearestHealth != null && minDistance < 10f) // 10单位内
                 {
                     healthBar.Initialize(nearestHealth, nearestHealth.transform);
                     refreshed++;
                 }
             }
         }
         
         string message = $"已强制刷新 {refreshed} 个血条的连接";
         Debug.Log($"[血条系统] {message}");
         EditorUtility.DisplayDialog("刷新完成", message, "确定");
     }
     
     /// <summary>
     /// 测试血条更新功能
     /// </summary>
     private void TestHealthBarUpdates()
     {
         if (!Application.isPlaying)
         {
             EditorUtility.DisplayDialog("提示", "此功能需要在运行时使用", "确定");
             return;
         }
         
         var allHealthComponents = FindObjectsByType<Health>(FindObjectsSortMode.None);
         
         if (allHealthComponents.Length == 0)
         {
             EditorUtility.DisplayDialog("提示", "场景中没有找到Health组件", "确定");
             return;
         }
         
         int tested = 0;
         
         foreach (var health in allHealthComponents)
         {
             if (health != null && !health.IsDead)
             {
                 // 造成10点伤害进行测试
                 health.TakeDamage(10);
                 tested++;
                 
                 // 等待1秒后恢复血量
                 health.StartCoroutine(RestoreHealthAfterDelay(health, 1f));
             }
         }
         
         string message = $"已对 {tested} 个Health组件进行血条更新测试";
         Debug.Log($"[血条系统] {message}");
         EditorUtility.DisplayDialog("测试完成", message, "确定");
     }
     
           private System.Collections.IEnumerator RestoreHealthAfterDelay(Health health, float delay)
      {
          yield return new WaitForSeconds(delay);
          if (health != null && !health.IsDead)
          {
              health.Heal(10); // 恢复刚才造成的伤害
          }
      }
    
}

#endif 