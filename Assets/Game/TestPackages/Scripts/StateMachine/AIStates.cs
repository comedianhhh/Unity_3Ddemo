using UnityEngine;
using UnityEngine.AI;
using Core.ScriptableObjects;

namespace Core.StateMachine.AIStates
{
    /// <summary>
    /// AI空闲状态
    /// </summary>
    public class IdleState : State<MonsterAI>
    {
        private float stateStartTime;
        private MonsterConfig config;
        
        public IdleState(MonsterAI owner, StateMachine<MonsterAI> stateMachine, MonsterConfig config) 
            : base(owner, stateMachine)
        {
            this.config = config;
        }
        
        public override void OnEnter()
        {
            stateStartTime = Time.time;
            
            if (owner.Agent != null)
            {
                owner.Agent.isStopped = true;
                owner.Agent.ResetPath();
            }
            
            Debug.Log($"[{owner.name}] 进入空闲状态");
        }
        
        public override void OnUpdate()
        {
            // 空闲时可以播放空闲动画，环顾四周等
            if (owner.Animator != null)
            {
                owner.Animator.SetBool(config?.moveAnimationParameter ?? "isMoving", false);
            }
        }
        
        public float TimeInIdle => Time.time - stateStartTime;
        
        public bool ShouldExitIdle()
        {
            return config != null && TimeInIdle >= config.idleDuration;
        }
        
        public override void OnDrawGizmos()
        {
            if (owner != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(owner.transform.position, 0.5f);
            }
        }
    }
    
    /// <summary>
    /// AI巡逻状态
    /// </summary>
    public class PatrolState : State<MonsterAI>
    {
        private MonsterConfig config;
        private Vector3 currentDestination;
        private bool hasDestination;
        
        public PatrolState(MonsterAI owner, StateMachine<MonsterAI> stateMachine, MonsterConfig config) 
            : base(owner, stateMachine)
        {
            this.config = config;
        }
        
        public override void OnEnter()
        {
            if (owner.Agent != null)
            {
                owner.Agent.isStopped = false;
                owner.Agent.speed = config?.patrolSpeed ?? 3.5f;
            }
            
            SetNewDestination();
            Debug.Log($"[{owner.name}] 进入巡逻状态");
        }
        
        public override void OnUpdate()
        {
            if (owner.Animator != null)
            {
                owner.Animator.SetBool(config?.moveAnimationParameter ?? "isMoving", true);
            }
            
            // 检查是否到达目标点
            if (owner.Agent != null && !owner.Agent.pathPending)
            {
                float threshold = config?.waypointChangeThreshold ?? 1f;
                if (owner.Agent.remainingDistance <= threshold)
                {
                    SetNewDestination();
                }
            }
        }
        
        private void SetNewDestination()
        {
            if (owner.Agent == null) return;
            
            currentDestination = GetRandomNavMeshLocation();
            owner.Agent.SetDestination(currentDestination);
            hasDestination = true;
        }
        
        private Vector3 GetRandomNavMeshLocation()
        {
            Vector3 randomDirection = Random.insideUnitSphere * 20f;
            randomDirection += owner.transform.position;
            
            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, 20f, NavMesh.AllAreas))
            {
                return hit.position;
            }
            
            return owner.transform.position;
        }
        
        public bool HasReachedDestination()
        {
            if (!hasDestination || owner.Agent == null) return false;
            
            if (!owner.Agent.pathPending && owner.Agent.remainingDistance <= owner.Agent.stoppingDistance)
            {
                return !owner.Agent.hasPath || owner.Agent.velocity.sqrMagnitude < 0.1f;
            }
            
            return false;
        }
        
        public override void OnDrawGizmos()
        {
            if (owner != null && hasDestination)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(currentDestination, 0.5f);
                Gizmos.DrawLine(owner.transform.position, currentDestination);
            }
        }
    }
    
    /// <summary>
    /// AI追击状态
    /// </summary>
    public class ChaseState : State<MonsterAI>
    {
        private MonsterConfig config;
        private Transform target;
        
        public ChaseState(MonsterAI owner, StateMachine<MonsterAI> stateMachine, MonsterConfig config, Transform target) 
            : base(owner, stateMachine)
        {
            this.config = config;
            this.target = target;
        }
        
        public override void OnEnter()
        {
            if (owner.Agent != null)
            {
                owner.Agent.isStopped = false;
                owner.Agent.speed = config?.chaseSpeed ?? 5f;
            }
            
            Debug.Log($"[{owner.name}] 进入追击状态");
        }
        
        public override void OnUpdate()
        {
            if (owner.Animator != null)
            {
                owner.Animator.SetBool(config?.moveAnimationParameter ?? "isMoving", true);
            }
            
            // 持续追击目标
            if (target != null && owner.Agent != null)
            {
                owner.Agent.SetDestination(target.position);
            }
        }
        
        public override void OnDrawGizmos()
        {
            if (owner != null && target != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(owner.transform.position, target.position);
                Gizmos.DrawWireSphere(owner.transform.position, 0.8f);
            }
        }
    }
    
    /// <summary>
    /// AI攻击状态
    /// </summary>
    public class AttackState : State<MonsterAI>
    {
        private MonsterConfig config;
        private Transform target;
        private MonsterConfig.AttackData currentAttack;
        private bool isAttacking;
        private float attackStartTime;
        
        public AttackState(MonsterAI owner, StateMachine<MonsterAI> stateMachine, MonsterConfig config, Transform target) 
            : base(owner, stateMachine)
        {
            this.config = config;
            this.target = target;
        }
        
        public override void OnEnter()
        {
            if (owner.Agent != null)
            {
                owner.Agent.isStopped = true;
                owner.Agent.ResetPath();
            }
            
            // 选择攻击类型
            currentAttack = config?.GetRandomAttack();
            if (currentAttack != null)
            {
                StartAttack();
            }
            
            Debug.Log($"[{owner.name}] 进入攻击状态 - 攻击类型: {currentAttack?.animationTrigger}");
        }
        
        public override void OnUpdate()
        {
            if (owner.Animator != null)
            {
                owner.Animator.SetBool(config?.moveAnimationParameter ?? "isMoving", false);
            }
            
            // 面向目标
            if (target != null)
            {
                Vector3 direction = (target.position - owner.transform.position).normalized;
                direction.y = 0; // 保持在水平面
                if (direction != Vector3.zero)
                {
                    owner.transform.rotation = Quaternion.LookRotation(direction);
                }
            }
        }
        
        private void StartAttack()
        {
            if (currentAttack == null) return;
            
            isAttacking = true;
            attackStartTime = Time.time;
            
            // 播放攻击动画
            if (owner.Animator != null && !string.IsNullOrEmpty(currentAttack.animationTrigger))
            {
                owner.Animator.SetTrigger(currentAttack.animationTrigger);
            }
            
            // 延迟执行伤害（攻击前摇）
            owner.StartCoroutine(DealDamageAfterDelay(currentAttack.windupTime));
        }
        
        private System.Collections.IEnumerator DealDamageAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (target != null && IsTargetInRange())
            {
                // 对目标造成伤害
                var targetHealth = target.GetComponent<Health>();
                if (targetHealth != null && currentAttack != null)
                {
                    int damage = currentAttack.damage;
                    
                    // 如果目标有装备，应用伤害减免
                    var itemHandler = target.GetComponent<ItemInteractionHandler>();
                    if (itemHandler != null)
                    {
                        damage = itemHandler.ApplyDamageReduction(damage);
                    }
                    
                    targetHealth.TakeDamage(damage);
                    Debug.Log($"[{owner.name}] 对 {target.name} 造成 {damage} 点伤害");
                    
                    // 播放攻击特效
                    if (!string.IsNullOrEmpty(currentAttack.attackEffect)&& GameManager.Instance?.Effects != null)
                    {
                        GameManager.Instance.Effects.PlayEffect(currentAttack.attackEffect, target.position);
                    }
                }
            }
            
            // 攻击完成后等待冷却
            yield return new WaitForSeconds(currentAttack?.cooldown ?? 1f);
            isAttacking = false;
        }
        
        private bool IsTargetInRange()
        {
            if (target == null || currentAttack == null) return false;
            
            float distance = Vector3.Distance(owner.transform.position, target.position);
            Debug.Log($"[{owner.name}] 与目标距离: {distance}, 攻击范围: {currentAttack.range}");
            return distance <= currentAttack.range;
        }
        
        public bool IsAttackFinished()
        {
            return !isAttacking;
        }
        
        public override void OnDrawGizmos()
        {
            if (owner != null && currentAttack != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(owner.transform.position, currentAttack.range);
                
                if (isAttacking)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(owner.transform.position, 1f);
                }
            }
        }
    }
    
    /// <summary>
    /// AI死亡状态
    /// </summary>
    public class DeadState : State<MonsterAI>
    {
        private MonsterConfig config;
        
        public DeadState(MonsterAI owner, StateMachine<MonsterAI> stateMachine, MonsterConfig config) 
            : base(owner, stateMachine)
        {
            this.config = config;
        }
        
        public override void OnEnter()
        {
            Debug.Log($"[{owner.name}] 进入死亡状态");
            
            // 停止所有移动
            if (owner.Agent != null)
            {
                owner.Agent.isStopped = true;
                owner.Agent.enabled = false;
            }
            
            // 播放死亡动画
            if (owner.Animator != null && config != null)
            {
                owner.Animator.SetTrigger(config.deathAnimationParameter);
                owner.Animator.SetBool(config.moveAnimationParameter, false);
            }
            
            // 禁用AI组件
            owner.enabled = false;
        }
        
        public override void OnDrawGizmos()
        {
            if (owner != null)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawWireSphere(owner.transform.position, 0.3f);
            }
        }
    }
} 