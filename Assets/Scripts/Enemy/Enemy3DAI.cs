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
    [SerializeField] private float attackDamage = 1f;
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
        if (navAgent != null && player != null)
        {
            navAgent.SetDestination(player.position);
        }
    }
    
    private void StopMoving()
    {
        if (navAgent != null)
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
                Debug.Log($"[Enemy3DAI] 攻击玩家造成 {attackDamage} 伤害");
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
        
        Debug.Log("[Enemy3DAI] 敌人死亡");
        
        // 可以添加死亡效果
        // 几秒后销毁对象
        Destroy(gameObject, 3f);
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