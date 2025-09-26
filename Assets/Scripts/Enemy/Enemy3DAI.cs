using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 3D敌人AI - 使用NavMesh进行3D寻路和攻击
/// </summary>
public class Enemy3DAI : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float followRange = 10f;
    [SerializeField] private float stopDistance = 1.5f;
    
    [Header("攻击设置")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDamage = 8f; // 敌人攻击伤害
    [SerializeField] private float attackCooldown = 1f;
    
    [Header("检测设置")]
    [SerializeField] private LayerMask playerLayerMask = 1;
    
    // 组件引用
    private NavMeshAgent navAgent;
    private HealthSystem healthSystem;
    private Transform player;
    
    // AI状态
    private float lastAttackTime;
    private bool isAttacking = false;
    // isInAttackRange字段已移除，直接使用距离计算
    
    // 属性
    public bool IsAlive => healthSystem != null && healthSystem.IsAlive;
    public Transform Target => player;
    
    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        healthSystem = GetComponent<HealthSystem>();
        
        // 配置NavMeshAgent
        if (navAgent != null)
        {
            navAgent.speed = moveSpeed;
            navAgent.stoppingDistance = stopDistance;
            navAgent.autoBraking = true;
        }
        
        // NavMesh将在Unity编辑器中手动烘焙
    }
    
    private void Start()
    {
        FindPlayer();
        
        // 订阅死亡事件
        if (healthSystem != null)
        {
            healthSystem.OnDeath += OnDeath;
        }
    }
    
    private void Update()
    {
        if (!IsAlive) return;
        
        // 安全检查：确保NavMeshAgent在NavMesh上
        if (navAgent != null && !navAgent.isOnNavMesh)
        {
            Debug.LogWarning("[Enemy3DAI] NavMeshAgent不在NavMesh上。请确保场景中已正确烘焙NavMesh。");
            return; // 如果不在NavMesh上，跳过AI更新
        }
        
        UpdateAI();
    }
    
    private void UpdateAI()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // 在跟随范围内
        if (distanceToPlayer <= followRange)
        {
            // 在攻击范围内
            if (distanceToPlayer <= attackRange)
            {
                StopMoving();
                TryAttack();
            }
            else
            {
                MoveTowardsPlayer();
            }
        }
        else
        {
            StopMoving();
        }
    }
    
    private void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }
    
    private void MoveTowardsPlayer()
    {
        if (navAgent != null && player != null && navAgent.isOnNavMesh)
        {
            navAgent.SetDestination(player.position);
        }
    }
    
    private void StopMoving()
    {
        if (navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.ResetPath();
        }
    }
    
    private void TryAttack()
    {
        if (Time.time - lastAttackTime >= attackCooldown && !isAttacking)
        {
            PerformAttack();
        }
    }
    
    private void PerformAttack()
    {
        lastAttackTime = Time.time;
        isAttacking = true;
        
        // 再次检查攻击距离（防止玩家已经远离）
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer > attackRange)
            {
                // 玩家已经超出攻击范围，取消攻击
                Debug.Log($"[Enemy3DAI] 玩家超出攻击范围 ({distanceToPlayer:F1}m > {attackRange}m)，取消攻击");
                ResetAttack();
                return;
            }
        }
        
        // 朝向玩家
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(directionToPlayer);
        
        // 攻击玩家
        if (player != null)
        {
            HealthSystem playerHealth = player.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log($"[Enemy3DAI] 攻击玩家造成 {attackDamage} 伤害，距离: {Vector3.Distance(transform.position, player.position):F1}m");
            }
        }
        
        // 攻击动画结束后重置状态
        Invoke(nameof(ResetAttack), 0.5f);
    }
    
    private void ResetAttack()
    {
        isAttacking = false;
    }
    
    private void OnDeath()
    {
        // 停止移动
        if (navAgent != null)
        {
            navAgent.enabled = false;
        }
        
        // 禁用碰撞器
        Collider enemyCollider = GetComponent<Collider>();
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }
        
        Debug.Log("[Enemy3DAI] 敌人死亡，开始沉入地下");
        
        // 开始死亡动画（沉入地下）
        StartCoroutine(SinkIntoGroundAndRecycle());
    }
    
    /// <summary>
    /// 沉入地下并回收到对象池
    /// </summary>
    private System.Collections.IEnumerator SinkIntoGroundAndRecycle()
    {
        Vector3 originalPosition = transform.position;
        Vector3 targetPosition = originalPosition + Vector3.down * 2f; // 向下沉2米
        float sinkDuration = 1.5f; // 沉入时间
        float elapsed = 0f;
        
        // 获取MeshRenderer用于逐渐变透明
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        Material originalMaterial = null;
        if (meshRenderer != null && meshRenderer.material != null)
        {
            // 创建材质副本以避免影响原材质
            originalMaterial = meshRenderer.material;
            meshRenderer.material = new Material(originalMaterial);
        }
        
        while (elapsed < sinkDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / sinkDuration;
            
            // 向下移动
            transform.position = Vector3.Lerp(originalPosition, targetPosition, progress);
            
            // 逐渐变透明
            if (meshRenderer != null && meshRenderer.material != null)
            {
                Color color = meshRenderer.material.color;
                color.a = Mathf.Lerp(1f, 0f, progress);
                meshRenderer.material.color = color;
            }
            
            yield return null;
        }
        
        Debug.Log("[Enemy3DAI] 敌人已沉入地下，开始回收");
        
        // 回收到对象池或销毁
        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.RecycleEnemy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 延迟回收到对象池（备用方法）
    /// </summary>
    private System.Collections.IEnumerator DelayedRecycle()
    {
        yield return new WaitForSeconds(2f); // 死亡动画时间
        
        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.RecycleEnemy(gameObject);
        }
    }
    
    /// <summary>
    /// 重置AI状态（用于对象池回收重用）
    /// </summary>
    public void ResetAI()
    {
        // 重置状态
        lastAttackTime = 0f;
        
        // 重新查找玩家
        FindPlayer();
        
        // 重置NavMeshAgent
        if (navAgent != null)
        {
            navAgent.isStopped = false;
            navAgent.enabled = true;
        }
        
        // 重置碰撞器
        Collider enemyCollider = GetComponent<Collider>();
        if (enemyCollider != null)
        {
            enemyCollider.enabled = true;
        }
        
        // 重置外观（恢复透明度和材质）
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null && meshRenderer.material != null)
        {
            Color color = meshRenderer.material.color;
            color.a = 1f; // 恢复不透明
            meshRenderer.material.color = color;
        }
        
        Debug.Log("[Enemy3DAI] AI状态已重置");
    }
    
    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDeath -= OnDeath;
        }
    }
    
    // Gizmos绘制调试信息
    private void OnDrawGizmosSelected()
    {
        // 跟随范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, followRange);
        
        // 攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // 停止距离
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}