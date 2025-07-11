using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// 事件接口 - 所有事件类型必须实现此接口
    /// </summary>
    public interface IGameEvent
    {
        string EventName { get; }
        DateTime TimeStamp { get; }
    }
    
    /// <summary>
    /// 事件基类 - 提供基础的事件功能
    /// </summary>
    public abstract class BaseGameEvent : IGameEvent
    {
        public string EventName { get; private set; }
        public DateTime TimeStamp { get; private set; }
        
        protected BaseGameEvent()
        {
            EventName = GetType().Name;
            TimeStamp = DateTime.Now;
        }
    }
    
    /// <summary>
    /// 事件管理器 - 核心事件系统
    /// </summary>
    public class EventManager : Singleton<EventManager>
    {
        
        // 事件订阅字典
        private Dictionary<Type, List<Delegate>> eventHandlers = new Dictionary<Type, List<Delegate>>();
        
        // 事件历史记录
        private Queue<IGameEvent> eventHistory = new Queue<IGameEvent>();
        private const int MAX_HISTORY_SIZE = 100;
        
        [Header("调试设置")]
        [SerializeField] private bool enableEventLogging = true;
        [SerializeField] private bool enableEventHistory = true;
        [SerializeField] private bool showEventStats = false;
        
        // 统计信息
        private Dictionary<Type, int> eventCounts = new Dictionary<Type, int>();
        
        /// <summary>
        /// 订阅事件
        /// </summary>
        public static void Subscribe<T>(Action<T> handler) where T : IGameEvent
        {
            if (handler == null) return;
            
            var eventType = typeof(T);
            
            if (!Instance.eventHandlers.ContainsKey(eventType))
            {
                Instance.eventHandlers[eventType] = new List<Delegate>();
            }
            
            Instance.eventHandlers[eventType].Add(handler);
            
            if (Instance.enableEventLogging)
            {
                Debug.Log($"[EventManager] 订阅事件: {eventType.Name}");
            }
        }
        
        /// <summary>
        /// 取消订阅事件
        /// </summary>
        public static void Unsubscribe<T>(Action<T> handler) where T : IGameEvent
        {
            if (handler == null || Instance == null) return;
            
            var eventType = typeof(T);
            
            if (Instance.eventHandlers.ContainsKey(eventType))
            {
                Instance.eventHandlers[eventType].Remove(handler);
                
                if (Instance.eventHandlers[eventType].Count == 0)
                {
                    Instance.eventHandlers.Remove(eventType);
                }
                
                if (Instance.enableEventLogging)
                {
                    Debug.Log($"[EventManager] 取消订阅事件: {eventType.Name}");
                }
            }
        }
        
        /// <summary>
        /// 发布事件
        /// </summary>
        public static void Publish<T>(T gameEvent) where T : IGameEvent
        {
            if (gameEvent == null || Instance == null) return;
            
            var eventType = typeof(T);
            
            // 记录统计信息
            if (!Instance.eventCounts.ContainsKey(eventType))
            {
                Instance.eventCounts[eventType] = 0;
            }
            Instance.eventCounts[eventType]++;
            
            // 记录历史
            if (Instance.enableEventHistory)
            {
                Instance.eventHistory.Enqueue(gameEvent);
                
                while (Instance.eventHistory.Count > MAX_HISTORY_SIZE)
                {
                    Instance.eventHistory.Dequeue();
                }
            }
            
            // 调用事件处理器
            if (Instance.eventHandlers.ContainsKey(eventType))
            {
                var handlers = Instance.eventHandlers[eventType];
                
                for (int i = handlers.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        var handler = handlers[i] as Action<T>;
                        handler?.Invoke(gameEvent);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[EventManager] 事件处理器出错 {eventType.Name}: {e.Message}");
                    }
                }
            }
            
            if (Instance.enableEventLogging)
            {
                Debug.Log($"[EventManager] 发布事件: {eventType.Name} at {gameEvent.TimeStamp:HH:mm:ss}");
            }
        }
        
        /// <summary>
        /// 延迟发布事件
        /// </summary>
        public static void PublishDelayed<T>(T gameEvent, float delay) where T : IGameEvent
        {
            if (gameEvent == null || Instance == null) return;
            
            Instance.StartCoroutine(DelayedPublish(gameEvent, delay));
        }
        
        /// <summary>
        /// 延迟发布协程
        /// </summary>
        private static System.Collections.IEnumerator DelayedPublish<T>(T gameEvent, float delay) where T : IGameEvent
        {
            yield return new WaitForSeconds(delay);
            Publish(gameEvent);
        }
        
        /// <summary>
        /// 清除所有事件订阅
        /// </summary>
        public static void ClearAllSubscriptions()
        {
            if (Instance != null)
            {
                Instance.eventHandlers.Clear();
                Debug.Log("[EventManager] 清除所有事件订阅");
            }
        }
        
        /// <summary>
        /// 清除特定类型的事件订阅
        /// </summary>
        public static void ClearSubscriptions<T>() where T : IGameEvent
        {
            if (Instance == null) return;
            
            var eventType = typeof(T);
            
            if (Instance.eventHandlers.ContainsKey(eventType))
            {
                Instance.eventHandlers.Remove(eventType);
                Debug.Log($"[EventManager] 清除事件订阅: {eventType.Name}");
            }
        }
        
        /// <summary>
        /// 获取事件历史
        /// </summary>
        public static IGameEvent[] GetEventHistory()
        {
            if (Instance == null) return new IGameEvent[0];
            
            return Instance.eventHistory.ToArray();
        }
        
        /// <summary>
        /// 获取事件统计
        /// </summary>
        public static Dictionary<Type, int> GetEventStats()
        {
            if (Instance == null) return new Dictionary<Type, int>();
            
            return new Dictionary<Type, int>(Instance.eventCounts);
        }
        
        /// <summary>
        /// 检查是否有订阅者
        /// </summary>
        public static bool HasSubscribers<T>() where T : IGameEvent
        {
            if (Instance == null) return false;
            
            var eventType = typeof(T);
            return Instance.eventHandlers.ContainsKey(eventType) && 
                   Instance.eventHandlers[eventType].Count > 0;
        }
        
        /// <summary>
        /// 获取订阅者数量
        /// </summary>
        public static int GetSubscriberCount<T>() where T : IGameEvent
        {
            if (Instance == null) return 0;
            
            var eventType = typeof(T);
            return Instance.eventHandlers.ContainsKey(eventType) ? 
                   Instance.eventHandlers[eventType].Count : 0;
        }
        
        private void OnDisable()
        {
            eventHandlers?.Clear();
            eventHistory?.Clear();
            eventCounts?.Clear();
            if (enableEventLogging)
            {
                Debug.Log("[EventManager] EventManager 已清理所有事件数据");
            }
        }
        
        private void OnGUI()
        {
            if (!showEventStats || !Application.isPlaying) return;
            
            GUILayout.BeginArea(new Rect(Screen.width - 300, 10, 290, 400));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("=== 事件系统统计 ===", new GUIStyle { fontSize = 14, normal = { textColor = Color.white } });
            
            GUILayout.Label($"活跃订阅类型: {eventHandlers.Count}", new GUIStyle { fontSize = 12, normal = { textColor = Color.white } });
            GUILayout.Label($"历史事件数: {eventHistory.Count}", new GUIStyle { fontSize = 12, normal = { textColor = Color.white } });
            
            GUILayout.Space(10);
            GUILayout.Label("事件发布统计:", new GUIStyle { fontSize = 12, normal = { textColor = Color.yellow } });
            
            foreach (var kvp in eventCounts)
            {
                GUILayout.Label($"{kvp.Key.Name}: {kvp.Value}", 
                    new GUIStyle { fontSize = 10, normal = { textColor = Color.white } });
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        #if UNITY_EDITOR
        [ContextMenu("显示事件统计")]
        private void ShowEventStatistics()
        {
            Debug.Log("=== 事件系统统计 ===");
            Debug.Log($"活跃订阅类型: {eventHandlers.Count}");
            Debug.Log($"历史事件数: {eventHistory.Count}");
            
            Debug.Log("事件发布统计:");
            foreach (var kvp in eventCounts)
            {
                Debug.Log($"  {kvp.Key.Name}: {kvp.Value} 次");
            }
            
            Debug.Log("当前订阅:");
            foreach (var kvp in eventHandlers)
            {
                Debug.Log($"  {kvp.Key.Name}: {kvp.Value.Count} 个订阅者");
            }
        }
        
        [ContextMenu("清除所有订阅")]
        private void ClearAllSubscriptionsDebug()
        {
            ClearAllSubscriptions();
        }
        #endif
    }
} 