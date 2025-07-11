using UnityEngine;

namespace Core
{
    /// <summary>
    /// 特效管理器接口
    /// </summary>
    public interface IEffectsManager
    {
        void Initialize();
        void PlayEffect(string effectName, Vector3 position, Quaternion rotation = default);
        void PlayEffect(string effectName, Transform parent);
        void StopEffect(string effectName);
        void StopAllEffects();
    }
    
    /// <summary>
    /// 对象池管理器接口
    /// </summary>
    public interface IObjectPoolManager
    {
        void Initialize();
        GameObject Get(string poolName);
        void Return(string poolName, GameObject obj);
        void PrewarmPool(string poolName, int count);
        void ClearPool(string poolName);
        void ClearAllPools();
        int GetPoolSize(string poolName);
        int GetActiveCount(string poolName);
    }
    
    /// <summary>
    /// 存档管理器接口
    /// </summary>
    public interface ISaveManager
    {
        void Initialize();
        void SaveGame();
        bool LoadGame();
        bool HasSaveFile();
        void DeleteSaveFile();
        T GetSaveData<T>() where T : class;
        void SetSaveData<T>(T data) where T : class;
    }
} 