using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.StateMachine
{
    /// <summary>
    /// 状态接口 - 定义状态的基本行为
    /// </summary>
    public interface IState
    {
        void OnEnter();
        void OnUpdate();
        void OnFixedUpdate();
        void OnExit();
        void OnDrawGizmos();
    }
    
    /// <summary>
    /// 状态基类 - 提供状态的默认实现
    /// </summary>
    public abstract class State<T> : IState where T : MonoBehaviour
    {
        protected T owner;
        protected StateMachine<T> stateMachine;
        
        public State(T owner, StateMachine<T> stateMachine)
        {
            this.owner = owner;
            this.stateMachine = stateMachine;
        }
        
        public virtual void OnEnter() { }
        public virtual void OnUpdate() { }
        public virtual void OnFixedUpdate() { }
        public virtual void OnExit() { }
        public virtual void OnDrawGizmos() { }
        
        /// <summary>
        /// 获取状态名称
        /// </summary>
        public virtual string GetStateName()
        {
            return GetType().Name;
        }
    }
    
    /// <summary>
    /// 状态转换条件
    /// </summary>
    public class Transition<T> where T : MonoBehaviour
    {
        public IState fromState;
        public IState toState;
        public Func<bool> condition;
        public string conditionName; // 用于调试
        
        public Transition(IState from, IState to, Func<bool> condition, string conditionName = "")
        {
            this.fromState = from;
            this.toState = to;
            this.condition = condition;
            this.conditionName = string.IsNullOrEmpty(conditionName) ? condition.Method.Name : conditionName;
        }
        
        public bool ShouldTransition()
        {
            return condition?.Invoke() ?? false;
        }
    }
    
    /// <summary>
    /// 通用状态机 - 支持泛型，可用于任何MonoBehaviour
    /// </summary>
    public class StateMachine<T> where T : MonoBehaviour
    {
        private T owner;
        private IState currentState;
        private Dictionary<Type, IState> states = new Dictionary<Type, IState>();
        private List<Transition<T>> transitions = new List<Transition<T>>();
        private List<Transition<T>> anyStateTransitions = new List<Transition<T>>();
        
        // 调试信息
        private string lastTransitionReason = "";
        private float stateEnterTime;
        
        public IState CurrentState => currentState;
        public string CurrentStateName => currentState?.GetType().Name ?? "None";
        public float TimeInCurrentState => Time.time - stateEnterTime;
        public string LastTransitionReason => lastTransitionReason;
        
        public StateMachine(T owner)
        {
            this.owner = owner;
        }
        
        /// <summary>
        /// 添加状态
        /// </summary>
        public void AddState<TState>(TState state) where TState : IState
        {
            states[typeof(TState)] = state;
        }
        
        /// <summary>
        /// 添加状态转换
        /// </summary>
        public void AddTransition<TFrom, TTo>(Func<bool> condition, string conditionName = "") 
            where TFrom : IState where TTo : IState
        {
            if (!states.ContainsKey(typeof(TFrom)) || !states.ContainsKey(typeof(TTo)))
            {
                Debug.LogError($"状态机中缺少状态: {typeof(TFrom).Name} -> {typeof(TTo).Name}");
                return;
            }
            
            var transition = new Transition<T>(
                states[typeof(TFrom)], 
                states[typeof(TTo)], 
                condition, 
                conditionName
            );
            
            transitions.Add(transition);
        }
        
        /// <summary>
        /// 添加从任意状态到指定状态的转换
        /// </summary>
        public void AddAnyStateTransition<TTo>(Func<bool> condition, string conditionName = "") 
            where TTo : IState
        {
            if (!states.ContainsKey(typeof(TTo)))
            {
                Debug.LogError($"状态机中缺少状态: {typeof(TTo).Name}");
                return;
            }
            
            var transition = new Transition<T>(
                null, 
                states[typeof(TTo)], 
                condition, 
                conditionName
            );
            
            anyStateTransitions.Add(transition);
        }
        
        /// <summary>
        /// 强制切换到指定状态
        /// </summary>
        public void ChangeState<TState>() where TState : IState
        {
            if (!states.ContainsKey(typeof(TState)))
            {
                Debug.LogError($"状态机中不存在状态: {typeof(TState).Name}");
                return;
            }
            
            ChangeToState(states[typeof(TState)], "强制切换");
        }
        
        /// <summary>
        /// 设置初始状态
        /// </summary>
        public void SetInitialState<TState>() where TState : IState
        {
            if (!states.ContainsKey(typeof(TState)))
            {
                Debug.LogError($"状态机中不存在初始状态: {typeof(TState).Name}");
                return;
            }
            
            currentState = states[typeof(TState)];
            stateEnterTime = Time.time;
            currentState.OnEnter();
            lastTransitionReason = "初始状态";
            
            Debug.Log($"[{owner.name}] 设置初始状态: {CurrentStateName}");
        }
        
        /// <summary>
        /// 状态机更新
        /// </summary>
        public void Update()
        {
            // 检查是否有任意状态转换
            foreach (var transition in anyStateTransitions)
            {
                if (transition.ShouldTransition())
                {
                    ChangeToState(transition.toState, $"AnyState->{transition.toState.GetType().Name}: {transition.conditionName}");
                    return;
                }
            }
            
            // 检查当前状态的转换
            foreach (var transition in transitions)
            {
                if (transition.fromState == currentState && transition.ShouldTransition())
                {
                    ChangeToState(transition.toState, $"{transition.fromState.GetType().Name}->{transition.toState.GetType().Name}: {transition.conditionName}");
                    return;
                }
            }
            
            // 更新当前状态
            currentState?.OnUpdate();
        }
        
        /// <summary>
        /// 物理更新
        /// </summary>
        public void FixedUpdate()
        {
            currentState?.OnFixedUpdate();
        }
        
        /// <summary>
        /// 绘制Gizmos
        /// </summary>
        public void OnDrawGizmos()
        {
            currentState?.OnDrawGizmos();
        }
        
        /// <summary>
        /// 切换到指定状态
        /// </summary>
        private void ChangeToState(IState newState, string reason)
        {
            if (newState == currentState)
                return;
                
            currentState?.OnExit();
            
            var oldStateName = currentState?.GetType().Name ?? "None";
            currentState = newState;
            stateEnterTime = Time.time;
            lastTransitionReason = reason;
            
            currentState.OnEnter();
            
            Debug.Log($"[{owner.name}] 状态转换: {oldStateName} -> {CurrentStateName} ({reason})");
        }
        
        /// <summary>
        /// 获取调试信息
        /// </summary>
        public string GetDebugInfo()
        {
            return $"当前状态: {CurrentStateName}\n" +
                   $"状态持续时间: {TimeInCurrentState:F2}s\n" +
                   $"上次转换原因: {LastTransitionReason}\n" +
                   $"状态总数: {states.Count}\n" +
                   $"转换总数: {transitions.Count}\n" +
                   $"任意状态转换数: {anyStateTransitions.Count}";
        }
        
        /// <summary>
        /// 清理状态机
        /// </summary>
        public void Cleanup()
        {
            currentState?.OnExit();
            currentState = null;
            states.Clear();
            transitions.Clear();
            anyStateTransitions.Clear();
        }
    }
} 