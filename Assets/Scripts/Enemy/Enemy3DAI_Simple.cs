using UnityEngine;

/// <summary>
/// 简化版3D敌人AI - 临时版本，去除NavMesh依赖
/// </summary>
public class Enemy3DAI_Simple : MonoBehaviour
{
    [Header("AI设置")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float followRange = 10f;
    [SerializeField] private float attackCooldown = 2f;
    
    // 组件引用
    private Transform player;
    private HealthSystem healthSystem;
    private Rigidbody rb;
    
    // AI状态
    private float lastAttackTime;
    private bool isAlive = true;
    
    private void Awake()
    {
        // 获取组件
        healthSystem = GetComponent<HealthSystem>();
        rb = GetComponent<Rigidbody>();
        
        // 查找玩家
        var playerController = FindFirstObjectByType<Player3DController>();
        if (playerController != null)
        {
            player = playerController.transform;
        }
        
        // 订阅死亡事件
        if (healthSystem != null)
        {
            healthSystem.OnDeath += Die;
        }
    }
    
    private void Update()
    {
        if (!isAlive || player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= attackRange)
        {
            AttackPlayer();
        }
        else if (distanceToPlayer <= followRange)
        {
            ChasePlayer();
        }
    }
    
    private void ChasePlayer()
    {
        // 简单的追逐逻辑（不使用NavMesh）
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; // 保持水平
        
        if (rb != null)
        {
            rb.MovePosition(transform.position + direction * moveSpeed * Time.deltaTime);
        }
        else
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
        
        // 面向玩家
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
    
    private void AttackPlayer()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;
        
        Debug.Log("[Enemy3DAI_Simple] 执行攻击！");
        
        // 对玩家造成伤害
        if (player != null)
        {
            var playerHealth = player.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                float distance = Vector3.Distance(transform.position, player.position);
                if (distance <= attackRange)
                {
                    playerHealth.TakeDamage(1f);
                    Debug.Log("[Enemy3DAI_Simple] 对玩家造成伤害！");
                }
            }
        }
        
        lastAttackTime = Time.time;
    }
    
    public void Die()
    {
        if (!isAlive) return;
        
        isAlive = false;
        Debug.Log("[Enemy3DAI_Simple] 敌人死亡！");
        
        Destroy(gameObject, 2f);
    }
    
    public void TakeDamage(float damage)
    {
        if (!isAlive) return;
        
        if (healthSystem != null)
        {
            healthSystem.TakeDamage(damage);
        }
    }
    
    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDeath -= Die;
        }
    }
    
    // 调试用Gizmos
    private void OnDrawGizmosSelected()
    {
        // 绘制攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // 绘制跟随范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, followRange);
    }
}
