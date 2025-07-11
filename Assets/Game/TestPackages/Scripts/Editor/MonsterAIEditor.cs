#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Core.StateMachine.AIStates;

[CustomEditor(typeof(MonsterAI))]
public class MonsterAIEditor : Editor
{
    private MonsterAI monster;
    private bool showDebugInfo = true;
    private void OnEnable()
    {
        monster = (MonsterAI)target;
    }
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        
        // 运行时调试信息
        if (Application.isPlaying)
        {
            DrawRuntimeDebugInfo();
        }
        
        EditorGUILayout.Space(10);
        
        // 编辑器工具
        DrawEditorTools();
        
        // 如果有修改，标记为脏
        if (GUI.changed)
        {
            EditorUtility.SetDirty(monster);
        }
    }
    
    private void DrawRuntimeDebugInfo()
    {
        showDebugInfo = EditorGUILayout.Foldout(showDebugInfo, "运行时调试信息", true);
        
        if (showDebugInfo)
        {
            EditorGUILayout.BeginVertical("box");
            
            // 当前状态信息
            EditorGUILayout.LabelField("当前状态", monster.CurrentStateName, EditorStyles.boldLabel);
            EditorGUILayout.LabelField("玩家距离", monster.DistanceToPlayer.ToString("F2") + "m");
            EditorGUILayout.LabelField("检测范围", monster.GetDetectionRange().ToString("F2") + "m");
            EditorGUILayout.LabelField("攻击范围", monster.GetAttackRange().ToString("F2") + "m");
            
            EditorGUILayout.Space(5);
            
            // AI状态指示器
            EditorGUILayout.LabelField("AI状态指示器:", EditorStyles.boldLabel);
            
            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.color = monster.IsPlayerInLongRange ? Color.green : Color.gray;
                EditorGUILayout.LabelField("●", GUILayout.Width(20));
                GUI.color = Color.white;
                EditorGUILayout.LabelField("玩家在检测范围内");
            }
            
            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.color = monster.IsPlayerInAttackRange ? Color.green : Color.gray;
                EditorGUILayout.LabelField("●", GUILayout.Width(20));
                GUI.color = Color.white;
                EditorGUILayout.LabelField("玩家在攻击范围内");
            }
            
            EditorGUILayout.Space(5);
            
            // NavMeshAgent信息
            if (monster.Agent != null)
            {
                EditorGUILayout.LabelField("导航信息:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("当前速度", monster.Agent.velocity.magnitude.ToString("F2"));
                EditorGUILayout.LabelField("剩余距离", monster.Agent.remainingDistance.ToString("F2"));
                EditorGUILayout.LabelField("是否有路径", monster.Agent.hasPath.ToString());
                EditorGUILayout.LabelField("是否停止", monster.Agent.isStopped.ToString());
            }
            
            EditorGUILayout.EndVertical();
        }
    }
    
    private void DrawEditorTools()
    {
        EditorGUILayout.LabelField("编辑器工具", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        
        // 状态控制按钮
        if (Application.isPlaying)
        {
            EditorGUILayout.LabelField("状态控制:", EditorStyles.boldLabel);
            
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("强制空闲"))
                {
                    monster.ForceChangeState<IdleState>();
                }
                
                if (GUILayout.Button("强制巡逻"))
                {
                    monster.ForceChangeState<PatrolState>();
                }
                
                if (GUILayout.Button("强制追击"))
                {
                    monster.ForceChangeState<ChaseState>();
                }
            }
            
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("强制攻击"))
                {
                    monster.ForceAttack();
                }
                
                if (GUILayout.Button("重置AI"))
                {
                    monster.ResetAI();
                }
            }
            
            EditorGUILayout.Space(5);
        }
        
        // 实用工具
        EditorGUILayout.LabelField("实用工具:", EditorStyles.boldLabel);
        
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("验证配置"))
            {
                ValidateMonsterConfig();
            }
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void AlignToGround()
    {
        if (Physics.Raycast(monster.transform.position + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f))
        {
            Undo.RecordObject(monster.transform, "Align Monster to Ground");
            monster.transform.position = hit.point;
            EditorUtility.SetDirty(monster.transform);
        }
    }
    
    private void ValidateMonsterConfig()
    {
        bool hasErrors = false;
        string errorMessage = "MonsterAI配置验证:\n";
        
        // 检查必要组件
        
        if (monster.Animator == null)
        {
            errorMessage += "- 缺少Animator组件\n";
            hasErrors = true;
        }
        
        var health = monster.GetComponent<Health>();
        if (health == null)
        {
            errorMessage += "- 缺少Health组件\n";
            hasErrors = true;
        }
        
        // 检查检测范围设置
        if (monster.GetDetectionRange() <= monster.GetAttackRange())
        {
            errorMessage += "- 检测范围应该大于攻击范围\n";
            hasErrors = true;
        }
        
        // 检查玩家标签
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            errorMessage += "- 场景中没有找到标记为'Player'的对象\n";
            hasErrors = true;
        }
        
        if (hasErrors)
        {
            EditorUtility.DisplayDialog("配置验证", errorMessage, "确定");
        }
        else
        {
            EditorUtility.DisplayDialog("配置验证", "MonsterAI配置验证通过！", "确定");
        }
    }
    
    // Scene视图中的自定义绘制
    private void OnSceneGUI()
    {
        if (monster == null) return;
        
        Handles.color = Color.yellow;
        Handles.DrawWireDisc(monster.transform.position, Vector3.up, monster.GetDetectionRange());
        
        Handles.color = Color.red;
        Handles.DrawWireDisc(monster.transform.position, Vector3.up, monster.GetAttackRange());
        
        // 显示状态信息
        if (Application.isPlaying)
        {
            Handles.color = Color.white;
            Handles.Label(
                monster.transform.position + Vector3.up * 3f,
                $"状态: {monster.CurrentStateName}\n距离: {monster.DistanceToPlayer:F1}m",
                EditorStyles.whiteBoldLabel
            );
        }
        
        // 绘制到玩家的连线
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && monster.IsPlayerInLongRange)
        {
            Handles.color = monster.IsPlayerInAttackRange ? Color.red : Color.yellow;
            Handles.DrawLine(monster.transform.position, player.transform.position);
        }
    }
}

/// <summary>
/// 简单的路径点Gizmo组件
/// </summary>
public class WaypointGizmo : MonoBehaviour
{
    public Color color = Color.cyan;
    public float size = 0.5f;
    
    private void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(transform.position, size);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * size);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(transform.position, size * 1.2f);
    }
}

#endif 