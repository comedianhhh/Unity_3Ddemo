using UnityEngine;

namespace Core
{
    #pragma warning disable
    /// <summary>
    /// 通用单例基类 - 提供单例模式的基础实现
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = (T)FindFirstObjectByType(typeof(T));
                    if (instance == null)
                    {
                        GameObject go = new GameObject(typeof(T).Name, typeof(T));
                        instance = go.GetComponent<T>();
                    }
                }

                return instance;
            }
        }

        public static bool IsInstance()
        {
            return (instance != null);
        }

        public static void Destroy()
        {
            if (instance != null)
            {
                Destroy(instance.gameObject);
                instance = null;
            }
        }

        protected virtual void Awake()
        {
            // Ensure the singleton instance persists across scene loads
            if (instance == null)
            {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject); // Destroy duplicates if there's more than one instance
            }
        }
        
        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
} 