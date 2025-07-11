using System;
using System.Collections;
using UnityEngine;
using Core.ScriptableObjects;

namespace UsableItem {
    public class Grenade : MonoBehaviour {
        [Header("配置数据")]
        [SerializeField] private WeaponConfig grenadeConfig;
        
        [Header("贝塞尔曲线设置")]
        [SerializeField] private float throwHeight = 5f; // 投掷弧度高度
        [SerializeField] private float throwDuration = 2f; // 投掷持续时间
        
        [Header("对象池设置")]
        [SerializeField] private string poolKey = "Grenade"; // 对象池键名
        
        private Vector3 startPosition;
        private Vector3 targetPosition;
        
        public event Action<Grenade> OnExplode;
        
        private void Start() {
            Use();
        }
        
        public void Use() {            
            StartCoroutine(Launch());
        }
        
        /// <summary>
        /// 设置投掷目标位置（由ItemInteractionHandler调用）
        /// </summary>
        public void SetThrowTarget(Vector3 target) {
            targetPosition = target;
            startPosition = transform.position;
        }
        
        /// <summary>
        /// 设置对象池键名（用于区分不同类型的手榴弹）
        /// </summary>
        public void SetPoolKey(string key) {
            poolKey = key;
        }
        
        IEnumerator Launch() {
            if (targetPosition == Vector3.zero) {
                // 如果没有设置目标，使用默认向前投掷
                targetPosition = startPosition + transform.forward * 10f;
            }
            
            // 开始贝塞尔曲线投掷
            yield return StartCoroutine(BezierThrow());
            
            // 投掷完成后等待引信时间（如果还有剩余时间）
            float remainingFuseTime = grenadeConfig.fuseTime - throwDuration;
            if (remainingFuseTime > 0) {
                yield return new WaitForSeconds(remainingFuseTime);
            }
            
            Explode();
            OnExplode?.Invoke(this);
            
            // 使用对象池归还或直接销毁
            if (Core.GameManager.Instance?.ObjectPool != null)
            {
                Core.GameManager.Instance.ObjectPool.Return(poolKey, gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// 贝塞尔曲线投掷
        /// </summary>
        IEnumerator BezierThrow() {
            // 计算贝塞尔曲线控制点
            Vector3 controlPoint = CalculateControlPoint(startPosition, targetPosition, throwHeight);
            
            float elapsedTime = 0f;
            
            while (elapsedTime < throwDuration) {
                float t = elapsedTime / throwDuration;
                
                // 使用二次贝塞尔曲线公式
                Vector3 currentPosition = CalculateBezierPoint(t, startPosition, controlPoint, targetPosition);
                transform.position = currentPosition;
                
                // 让手榴弹面向运动方向
                if (t < 0.99f) {
                    Vector3 nextPosition = CalculateBezierPoint(t + 0.01f, startPosition, controlPoint, targetPosition);
                    Vector3 direction = (nextPosition - currentPosition).normalized;
                    if (direction != Vector3.zero) {
                        transform.rotation = Quaternion.LookRotation(direction);
                    }
                }
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // 确保最终位置准确
            transform.position = targetPosition;
        }
        
        /// <summary>
        /// 计算贝塞尔曲线控制点
        /// </summary>
        private Vector3 CalculateControlPoint(Vector3 start, Vector3 end, float height) {
            Vector3 midPoint = (start + end) * 0.5f;
            midPoint.y += height;
            return midPoint;
        }
        
        /// <summary>
        /// 计算二次贝塞尔曲线上的点
        /// </summary>
        private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2) {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            
            Vector3 point = uu * p0; // (1-t)² * P0
            point += 2 * u * t * p1; // 2(1-t)t * P1
            point += tt * p2; // t² * P2
            
            return point;
        }
        private void Explode()
        {
            // 播放爆炸特效
            GameObject effectPrefab = grenadeConfig.explosionEffect;
            if (effectPrefab != null)
            {
                if (Core.GameManager.Instance?.Effects != null)
                {
                    Core.GameManager.Instance.Effects.PlayEffect("Explosion", transform.position);
                }
                else
                {
                    Instantiate(effectPrefab, transform.position, Quaternion.identity);
                }
            }

            // 获取实际参数
            float actualRadius = grenadeConfig.explosionRadius ;
            int actualDamage = grenadeConfig.GetActualDamage();
            
            // Find all colliders in the explosion radius on specified layers
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, actualRadius);
            foreach (var hitCollider in hitColliders)
            {
                Health health = hitCollider.GetComponent<Health>();
                if (health != null)
                {
                    Debug.Log($"Grenade hit {hitCollider.name} for {actualDamage} damage.");
                    health.TakeDamage(actualDamage);
                }
            }
        }

        // Gizmo for explosion radius
        private void OnDrawGizmosSelected()
        {
            float actualRadius = grenadeConfig.explosionRadius;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, actualRadius);
            
            // 绘制贝塞尔曲线轨迹（仅在设置了目标位置时）
            if (targetPosition != Vector3.zero && startPosition != Vector3.zero)
            {
                DrawBezierCurve();
            }
        }
        
        /// <summary>
        /// 绘制贝塞尔曲线轨迹
        /// </summary>
        private void DrawBezierCurve()
        {
            Vector3 controlPoint = CalculateControlPoint(startPosition, targetPosition, throwHeight);
            
            Gizmos.color = Color.yellow;
            
            // 绘制轨迹线
            Vector3 previousPoint = startPosition;
            int segments = 20;
            
            for (int i = 1; i <= segments; i++)
            {
                float t = (float)i / segments;
                Vector3 currentPoint = CalculateBezierPoint(t, startPosition, controlPoint, targetPosition);
                
                Gizmos.DrawLine(previousPoint, currentPoint);
                previousPoint = currentPoint;
            }
            
            // 绘制关键点
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(startPosition, 0.2f); // 起点
            
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(controlPoint, 0.15f); // 控制点
            
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(targetPosition, 0.2f); // 终点
            
            // 绘制控制线
            Gizmos.color = Color.gray;
            Gizmos.DrawLine(startPosition, controlPoint);
            Gizmos.DrawLine(controlPoint, targetPosition);
        }
        
    }
}