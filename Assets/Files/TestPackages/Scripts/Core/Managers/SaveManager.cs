using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// 存档管理器 - 负责管理游戏存档数据
    /// </summary>
    public class SaveManager : MonoBehaviour, ISaveManager
    {
        [Header("存档设置")]
        [SerializeField] private string saveFileName = "gamesave.json";
        [SerializeField] private bool enableEncryption = false;
        
        private string saveFilePath;
        private Dictionary<Type, object> saveDataCache = new Dictionary<Type, object>();
        
        [System.Serializable]
        public class GameSaveData
        {
            public PlayerSaveData playerData;
            public LevelSaveData levelData;
            public SettingsSaveData settingsData;
            public float playTime;
            public DateTime saveDateTime;
        }
        
        [System.Serializable]
        public class PlayerSaveData
        {
            public int health;
            public int grenadeCount;
            public bool hasGearEquipped;
            public Vector3 position;
            public Vector3 rotation;
        }
        
        [System.Serializable]
        public class LevelSaveData
        {
            public string currentLevel;
            public int enemiesDefeated;
            public float levelProgress;
        }
        
        [System.Serializable]
        public class SettingsSaveData
        {
            public float musicVolume = 0.7f;
            public float sfxVolume = 1f;
            public int qualityLevel = 2;
            public bool fullscreen = false;
        }
        
        public void Initialize()
        {
            Debug.Log("初始化存档管理器...");
            
            saveFilePath = Path.Combine(Application.persistentDataPath, saveFileName);
            
            Debug.Log($"存档文件路径: {saveFilePath}");
            Debug.Log("存档管理器初始化完成");
        }
        
        public void SaveGame()
        {
            try
            {
                var saveData = new GameSaveData
                {
                    playerData = GatherPlayerData(),
                    levelData = GatherLevelData(),
                    settingsData = GatherSettingsData(),
                    playTime = Time.time,
                    saveDateTime = DateTime.Now
                };
                
                string json = JsonUtility.ToJson(saveData, true);
                
                if (enableEncryption)
                {
                    json = EncryptString(json);
                }
                
                File.WriteAllText(saveFilePath, json);
                
                Debug.Log($"游戏保存成功: {saveFilePath}");
                
                // 发布保存成功事件
                EventManager.Publish(new GameSavedEvent(true, saveFilePath));
            }
            catch (System.Exception e)
            {
                Debug.LogError($"保存游戏失败: {e.Message}");
                
                // 发布保存失败事件
                EventManager.Publish(new GameSavedEvent(false, ""));
            }
        }
        
        public bool LoadGame()
        {
            if (!HasSaveFile())
            {
                Debug.LogWarning("没有找到存档文件");
                EventManager.Publish(new GameLoadedEvent(false, ""));
                return false;
            }
            
            try
            {
                string json = File.ReadAllText(saveFilePath);
                
                if (enableEncryption)
                {
                    json = DecryptString(json);
                }
                
                var saveData = JsonUtility.FromJson<GameSaveData>(json);
                
                ApplyPlayerData(saveData.playerData);
                ApplyLevelData(saveData.levelData);
                ApplySettingsData(saveData.settingsData);
                
                Debug.Log("游戏加载成功");
                
                // 发布加载成功事件
                EventManager.Publish(new GameLoadedEvent(true, saveFilePath));
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"加载游戏失败: {e.Message}");
                
                // 发布加载失败事件
                EventManager.Publish(new GameLoadedEvent(false, ""));
                return false;
            }
        }
        
        public bool HasSaveFile()
        {
            return File.Exists(saveFilePath);
        }
        
        public void DeleteSaveFile()
        {
            if (HasSaveFile())
            {
                File.Delete(saveFilePath);
                Debug.Log("存档文件已删除");
            }
        }
        
        public T GetSaveData<T>() where T : class
        {
            if (saveDataCache.ContainsKey(typeof(T)))
            {
                return saveDataCache[typeof(T)] as T;
            }
            return null;
        }
        
        public void SetSaveData<T>(T data) where T : class
        {
            saveDataCache[typeof(T)] = data;
        }
        
        private PlayerSaveData GatherPlayerData()
        {
            var playerData = new PlayerSaveData();
            
            // 从现有组件收集玩家数据
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var health = player.GetComponent<Health>();
                if (health != null)
                {
                    playerData.health = health.CurrentHealth;
                }
                
                var itemHandler = player.GetComponent<ItemInteractionHandler>();
                if (itemHandler != null)
                {
                    playerData.grenadeCount = itemHandler.GetGrenadeCount();
                    playerData.hasGearEquipped = itemHandler.HasPermanentVariantGearEquipped();
                }
                
                playerData.position = player.transform.position;
                playerData.rotation = player.transform.eulerAngles;
            }
            
            return playerData;
        }
        
        private LevelSaveData GatherLevelData()
        {
            return new LevelSaveData
            {
                currentLevel = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                enemiesDefeated = 0, // 可以从游戏状态管理器获取
                levelProgress = 0f
            };
        }
        
        private SettingsSaveData GatherSettingsData()
        {
            var settingsData = new SettingsSaveData();
            
            return settingsData;
        }
        
        private void ApplyPlayerData(PlayerSaveData playerData)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && playerData != null)
            {
                var health = player.GetComponent<Health>();
                if (health != null)
                {
                    health.SetCurrentHealth(playerData.health);
                }
                
                var itemHandler = player.GetComponent<ItemInteractionHandler>();
                if (itemHandler != null)
                {
                    // 这里需要在ItemInteractionHandler中添加设置方法
                    // itemHandler.SetGrenadeCount(playerData.grenadeCount);
                    // itemHandler.SetGearEquipped(playerData.hasGearEquipped);
                }
                
                player.transform.position = playerData.position;
                player.transform.eulerAngles = playerData.rotation;
                
                Debug.Log($"应用玩家存档数据 - 血量: {playerData.health}, 手榴弹: {playerData.grenadeCount}");
            }
        }
        
        private void ApplyLevelData(LevelSaveData levelData)
        {
            
        }
        
        private void ApplySettingsData(SettingsSaveData settingsData)
        {

        }
        
        private string EncryptString(string text)
        {
            // 简单的Base64编码
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(text);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        
        private string DecryptString(string encryptedText)
        {
            // 简单的Base64解码
            var base64EncodedBytes = System.Convert.FromBase64String(encryptedText);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
        
        #if UNITY_EDITOR
        [ContextMenu("保存游戏")]
        private void SaveGameDebug()
        {
            SaveGame();
        }
        
        [ContextMenu("加载游戏")]
        private void LoadGameDebug()
        {
            LoadGame();
        }
        
        [ContextMenu("删除存档")]
        private void DeleteSaveFileDebug()
        {
            DeleteSaveFile();
        }
        
        [ContextMenu("显示存档路径")]
        private void ShowSaveFilePath()
        {
            Debug.Log($"存档路径: {saveFilePath}");
            Debug.Log($"存档存在: {HasSaveFile()}");
        }
        #endif
    }
} 