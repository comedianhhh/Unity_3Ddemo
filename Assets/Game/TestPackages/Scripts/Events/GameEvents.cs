using UnityEngine;

namespace Core
{
    /// <summary>
    /// 玩家相关事件
    /// </summary>
    public class PlayerDamagedEvent : BaseGameEvent
    {
        public int Damage { get; }
        public int CurrentHealth { get; }
        public int MaxHealth { get; }
        public Vector3 DamagePosition { get; }
        public GameObject Attacker { get; }
        
        public PlayerDamagedEvent(int damage, int currentHealth, int maxHealth, Vector3 damagePosition, GameObject attacker = null)
        {
            Damage = damage;
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
            DamagePosition = damagePosition;
            Attacker = attacker;
        }
    }
    
    public class PlayerHealedEvent : BaseGameEvent
    {
        public int HealAmount { get; }
        public int CurrentHealth { get; }
        public int MaxHealth { get; }
        
        public PlayerHealedEvent(int healAmount, int currentHealth, int maxHealth)
        {
            HealAmount = healAmount;
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
        }
    }
    
    public class PlayerDeathEvent : BaseGameEvent
    {
        public Vector3 DeathPosition { get; }
        public GameObject Killer { get; }
        
        public PlayerDeathEvent(Vector3 deathPosition, GameObject killer = null)
        {
            DeathPosition = deathPosition;
            Killer = killer;
        }
    }
    
    public class PlayerRespawnEvent : BaseGameEvent
    {
        public Vector3 RespawnPosition { get; }
        
        public PlayerRespawnEvent(Vector3 respawnPosition)
        {
            RespawnPosition = respawnPosition;
        }
    }
    
    /// <summary>
    /// 敌人相关事件
    /// </summary>
    public class EnemySpawnedEvent : BaseGameEvent
    {
        public GameObject Enemy { get; }
        public Vector3 SpawnPosition { get; }
        public string EnemyType { get; }
        
        public EnemySpawnedEvent(GameObject enemy, Vector3 spawnPosition, string enemyType)
        {
            Enemy = enemy;
            SpawnPosition = spawnPosition;
            EnemyType = enemyType;
        }
    }
    
    public class EnemyDefeatedEvent : BaseGameEvent
    {
        public GameObject Enemy { get; }
        public GameObject Killer { get; }
        public Vector3 DeathPosition { get; }
        public int ScoreValue { get; }
        public int ExperienceValue { get; }
        
        public EnemyDefeatedEvent(GameObject enemy, GameObject killer, Vector3 deathPosition, int scoreValue = 0, int experienceValue = 0)
        {
            Enemy = enemy;
            Killer = killer;
            DeathPosition = deathPosition;
            ScoreValue = scoreValue;
            ExperienceValue = experienceValue;
        }
    }
    
    public class EnemyDamagedEvent : BaseGameEvent
    {
        public GameObject Enemy { get; }
        public int Damage { get; }
        public int CurrentHealth { get; }
        public GameObject Attacker { get; }
        
        public Vector3 position => Enemy != null ? Enemy.transform.position : Vector3.zero;
        
        public EnemyDamagedEvent(GameObject enemy, int damage, int currentHealth, GameObject attacker)
        {
            Enemy = enemy;
            Damage = damage;
            CurrentHealth = currentHealth;
            Attacker = attacker;
        }
    }
    
    public class EnemyDeathEvent : BaseGameEvent
    {
        public GameObject Enemy { get; }
        public Vector3 position { get; }
        public GameObject Killer { get; }
        public string EnemyType { get; }
        public int ScoreValue { get; }
        
        public EnemyDeathEvent(GameObject enemy, Vector3 position, GameObject killer = null, string enemyType = "", int scoreValue = 0)
        {
            Enemy = enemy;
            this.position = position;
            Killer = killer;
            EnemyType = enemyType;
            ScoreValue = scoreValue;
        }
    }
    
    /// <summary>
    /// 物品相关事件
    /// </summary>
    public class ItemPickedUpEvent : BaseGameEvent
    {
        public GameObject Item { get; }
        public GameObject Picker { get; }
        public string ItemType { get; }
        public Vector3 PickupPosition { get; }
        
        public ItemPickedUpEvent(GameObject item, GameObject picker, string itemType, Vector3 pickupPosition)
        {
            Item = item;
            Picker = picker;
            ItemType = itemType;
            PickupPosition = pickupPosition;
        }
    }
    
    public class ItemUsedEvent : BaseGameEvent
    {
        public GameObject Item { get; }
        public GameObject User { get; }
        public string ItemType { get; }
        
        public ItemUsedEvent(GameObject item, GameObject user, string itemType)
        {
            Item = item;
            User = user;
            ItemType = itemType;
        }
    }
    
    public class GrenadeCountChangedEvent : BaseGameEvent
    {
        public int OldCount { get; }
        public int NewCount { get; }
        public int Change { get; }
        
        public GrenadeCountChangedEvent(int oldCount, int newCount)
        {
            OldCount = oldCount;
            NewCount = newCount;
            Change = newCount - oldCount;
        }
    }
    
    public class GearStatusChangedEvent : BaseGameEvent
    {
        public bool HasGear { get; }
        public string GearType { get; }
        
        public GearStatusChangedEvent(bool hasGear, string gearType = "")
        {
            HasGear = hasGear;
            GearType = gearType;
        }
    }
    
    /// <summary>
    /// 武器相关事件
    /// </summary>
    public class WeaponFiredEvent : BaseGameEvent
    {
        public GameObject Weapon { get; }
        public GameObject Shooter { get; }
        public Vector3 FirePosition { get; }
        public Vector3 FireDirection { get; }
        
        public WeaponFiredEvent(GameObject weapon, GameObject shooter, Vector3 firePosition, Vector3 fireDirection)
        {
            Weapon = weapon;
            Shooter = shooter;
            FirePosition = firePosition;
            FireDirection = fireDirection;
        }
    }
    
    public class GrenadeExplosionEvent : BaseGameEvent
    {
        public Vector3 ExplosionPosition { get; }
        public float ExplosionRadius { get; }
        public int Damage { get; }
        public GameObject[] AffectedTargets { get; }
        public GameObject Thrower { get; }
        
        public GrenadeExplosionEvent(Vector3 explosionPosition, float explosionRadius, int damage, GameObject[] affectedTargets, GameObject thrower)
        {
            ExplosionPosition = explosionPosition;
            ExplosionRadius = explosionRadius;
            Damage = damage;
            AffectedTargets = affectedTargets;
            Thrower = thrower;
        }
    }
    
    /// <summary>
    /// 游戏系统事件
    /// </summary>
    public class GameStartedEvent : BaseGameEvent
    {
        public string LevelName { get; }
        
        public GameStartedEvent(string levelName)
        {
            LevelName = levelName;
        }
    }
    
    public class GamePausedEvent : BaseGameEvent
    {
        public bool IsPaused { get; }
        
        public GamePausedEvent(bool isPaused)
        {
            IsPaused = isPaused;
        }
    }
    
    public class GameOverEvent : BaseGameEvent
    {
        public bool Victory { get; }
        public int FinalScore { get; }
        public float PlayTime { get; }
        
        public GameOverEvent(bool victory, int finalScore, float playTime)
        {
            Victory = victory;
            FinalScore = finalScore;
            PlayTime = playTime;
        }
    }
    
    public class LevelCompletedEvent : BaseGameEvent
    {
        public string LevelName { get; }
        public int Score { get; }
        public float CompletionTime { get; }
        public int EnemiesKilled { get; }
        
        public LevelCompletedEvent(string levelName, int score, float completionTime, int enemiesKilled)
        {
            LevelName = levelName;
            Score = score;
            CompletionTime = completionTime;
            EnemiesKilled = enemiesKilled;
        }
    }
    
    /// <summary>
    /// UI相关事件
    /// </summary>
    public class UIShowPanelEvent : BaseGameEvent
    {
        public string PanelName { get; }
        
        public UIShowPanelEvent(string panelName)
        {
            PanelName = panelName;
        }
    }
    
    public class UIHidePanelEvent : BaseGameEvent
    {
        public string PanelName { get; }
        
        public UIHidePanelEvent(string panelName)
        {
            PanelName = panelName;
        }
    }
    
    /// <summary>
    /// 音频相关事件
    /// </summary>
    public class PlaySoundEvent : BaseGameEvent
    {
        public string SoundName { get; }
        public Vector3 Position { get; }
        public float Volume { get; }
        
        public PlaySoundEvent(string soundName, Vector3 position = default, float volume = 1f)
        {
            SoundName = soundName;
            Position = position;
            Volume = volume;
        }
    }
    
    public class PlayMusicEvent : BaseGameEvent
    {
        public string MusicName { get; }
        public bool Loop { get; }
        public float FadeTime { get; }
        
        public PlayMusicEvent(string musicName, bool loop = true, float fadeTime = 0f)
        {
            MusicName = musicName;
            Loop = loop;
            FadeTime = fadeTime;
        }
    }
    
    /// <summary>
    /// 存档相关事件
    /// </summary>
    public class GameSavedEvent : BaseGameEvent
    {
        public bool Success { get; }
        public string SavePath { get; }
        
        public GameSavedEvent(bool success, string savePath = "")
        {
            Success = success;
            SavePath = savePath;
        }
    }
    
    public class GameLoadedEvent : BaseGameEvent
    {
        public bool Success { get; }
        public string LoadPath { get; }
        
        public GameLoadedEvent(bool success, string loadPath = "")
        {
            Success = success;
            LoadPath = loadPath;
        }
    }
} 