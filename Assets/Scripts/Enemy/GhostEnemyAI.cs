using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// 幽灵敌人AI - 基于NavMesh的3D幽灵敌人，具有溶解死亡特效
/// 借鉴Enemy3DAI的核心AI逻辑，保持Ghost原有的视觉特效
/// </summary>
public class GhostEnemyAI : MonoBehaviour
{
    [Header("AI设置")]
    [SerializeField] private float moveSpeed = 2.5f;           // 幽灵移动速度（较慢，飘逸感）
    [SerializeField] private float followRange = 12f;         // 追踪范围（比普通敌人更大）
    [SerializeField] private float stopDistance = 2f;         // 停止距离
    
    [Header("攻击设置")]
    [SerializeField] private float attackRange = 2.5f;        // 攻击范围
    [SerializeField] private float attackDamage = 6f;         // 攻击伤害（比普通敌人稍低）
    [SerializeField] private float attackCooldown = 1.5f;     // 攻击冷却时间
    
    [Header("检测设置")]
    [SerializeField] private LayerMask playerLayerMask = 1;
    
    [Header("特效设置")]
    [SerializeField] private SkinnedMeshRenderer[] meshRenderers; // Dissolve特效用的网格渲染器
    
    // 组件引用
    private NavMeshAgent navAgent;
    private HealthSystem healthSystem;
    private Animator animator;
    private Transform player;
    
    // AI状态
    private GhostState currentState = GhostState.Idle;
    private float lastAttackTime;
    private bool isAttacking = false;
    private bool isDissolving = false;
    private float dissolveValue = 1f;
    
    // 动画状态Hash缓存（性能优化）
    private static readonly int IdleState = Animator.StringToHash("Base Layer.idle");
    private static readonly int MoveState = Animator.StringToHash("Base Layer.move");
    private static readonly int SurprisedState = Animator.StringToHash("Base Layer.surprised");
    private static readonly int AttackState = Animator.StringToHash("Base Layer.attack_shift");
    private static readonly int DissolveState = Animator.StringToHash("Base Layer.dissolve");
    private static readonly int AttackTag = Animator.StringToHash("Attack");
    
    // 幽灵状态枚举
    public enum GhostState
    {
        Idle,        // 空闲状态
        Chasing,     // 追踪玩家
        Attacking,   // 攻击状态
        Surprised,   // 受击状态
        Dissolving   // 死亡溶解状态
    }
    
    // 属性
    public bool IsAlive => healthSystem != null && healthSystem.IsAlive;
    public Transform Target => player;
    public GhostState CurrentState => currentState;
    
    private void Awake()
    {
        // 获取组件引用
        navAgent = GetComponent<NavMeshAgent>();
        healthSystem = GetComponent<HealthSystem>();
        animator = GetComponent<Animator>();
        
        // 配置NavMeshAgent
        if (navAgent != null)
        {
            navAgent.speed = moveSpeed;
            navAgent.stoppingDistance = stopDistance;
            navAgent.autoBraking = true;
            navAgent.acceleration = 4f;        // 较低加速度，营造飘逸感
            navAgent.angularSpeed = 180f;      // 转向速度
        }
        
        // 寻找玩家
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        // 自动查找并设置MeshRenderers
        if (meshRenderers == null || meshRenderers.Length == 0)
        {
            meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            Debug.Log($"[GhostEnemyAI] {gameObject.name} 自动找到 {meshRenderers.Length} 个 SkinnedMeshRenderer");
        }
        
        // 验证必要组件
        if (navAgent == null)
            Debug.LogError($"[GhostEnemyAI] {gameObject.name} 缺少 NavMeshAgent 组件！");
        if (healthSystem == null)
            Debug.LogError($"[GhostEnemyAI] {gameObject.name} 缺少 HealthSystem 组件！");
        if (animator == null)
            Debug.LogError($"[GhostEnemyAI] {gameObject.name} 缺少 Animator 组件！");
        if (meshRenderers == null || meshRenderers.Length == 0)
            Debug.LogWarning($"[GhostEnemyAI] {gameObject.name} 未设置 MeshRenderers，Dissolve特效将无法工作！");
            
        Debug.Log($"[GhostEnemyAI] {gameObject.name} 初始化完成");
    }
    
    private void Start()
    {
        // 设置初始状态
        ChangeState(GhostState.Idle);
        
        // 订阅生命系统事件
        if (healthSystem != null)
        {
            healthSystem.OnDeath += OnDeath;
            healthSystem.OnDamageTaken += OnDamageTaken;
        }
    }
    
    private void Update()
    {
        // 如果已死亡且正在溶解，只处理溶解逻辑
        if (!IsAlive && isDissolving)
        {
            ProcessDissolve();
            return;
        }
        
        // 如果没有玩家目标或已死亡，不执行AI逻辑
        if (player == null || !IsAlive)
            return;
            
        // 根据当前状态执行相应逻辑
        switch (currentState)
        {
            case GhostState.Idle:
                ProcessIdleState();
                break;
            case GhostState.Chasing:
                ProcessChasingState();
                break;
            case GhostState.Attacking:
                ProcessAttackingState();
                break;
            case GhostState.Surprised:
                ProcessSurprisedState();
                break;
        }
    }
    
    #region 状态处理方法
    
    private void ProcessIdleState()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // 如果玩家进入追踪范围，开始追踪
        if (distanceToPlayer <= followRange)
        {
            ChangeState(GhostState.Chasing);
        }
    }
    
    private void ProcessChasingState()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // 如果玩家离开追踪范围，返回空闲
        if (distanceToPlayer > followRange)
        {
            ChangeState(GhostState.Idle);
            navAgent.ResetPath();
            return;
        }
        
        // 如果进入攻击范围，准备攻击
        if (distanceToPlayer <= attackRange && Time.time - lastAttackTime >= attackCooldown)
        {
            ChangeState(GhostState.Attacking);
            return;
        }
        
        // 继续追踪玩家
        navAgent.SetDestination(player.position);
    }
    
    private void ProcessAttackingState()
    {
        // 停止移动，面向玩家
        navAgent.ResetPath();
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        directionToPlayer.y = 0; // 保持在水平面上
        transform.rotation = Quaternion.LookRotation(directionToPlayer);
        
        // 如果不在攻击动画中，开始攻击
        if (!isAttacking)
        {
            StartCoroutine(PerformAttack());
        }
    }
    
    private void ProcessSurprisedState()
    {
        // 受击状态由动画控制，检查是否结束
        if (!IsInAnimationState(SurprisedState))
        {
            // 受击动画结束，根据距离决定下一个状态
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            if (distanceToPlayer <= followRange)
            {
                ChangeState(GhostState.Chasing);
            }
            else
            {
                ChangeState(GhostState.Idle);
            }
        }
    }
    
    #endregion
    
    #region 状态切换和动画控制
    
    private void ChangeState(GhostState newState)
    {
        if (currentState == newState) return;
        
        Debug.Log($"[GhostEnemyAI] {gameObject.name} 状态切换: {currentState} → {newState}");
        currentState = newState;
        
        // 根据状态播放对应动画
        switch (newState)
        {
            case GhostState.Idle:
                animator.CrossFade(IdleState, 0.1f, 0, 0);
                break;
            case GhostState.Chasing:
                animator.CrossFade(MoveState, 0.1f, 0, 0);
                break;
            case GhostState.Attacking:
                // 攻击动画在PerformAttack中控制
                break;
            case GhostState.Surprised:
                animator.CrossFade(SurprisedState, 0.1f, 0, 0);
                break;
            case GhostState.Dissolving:
                animator.CrossFade(DissolveState, 0.1f, 0, 0);
                isDissolving = true;
                break;
        }
    }
    
    private bool IsInAnimationState(int stateHash)
    {
        return animator.GetCurrentAnimatorStateInfo(0).fullPathHash == stateHash;
    }
    
    private bool IsInAttackAnimation()
    {
        return animator.GetCurrentAnimatorStateInfo(0).tagHash == AttackTag;
    }
    
    #endregion
    
    #region 攻击系统
    
    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        
        // 播放攻击动画
        animator.CrossFade(AttackState, 0.1f, 0, 0);
        
        // 等待攻击动画的伤害帧（通常在动画中间）
        yield return new WaitForSeconds(0.5f);
        
        // 检查玩家是否仍在攻击范围内
        float currentDistance = Vector3.Distance(transform.position, player.position);
        if (currentDistance <= attackRange)
        {
            // 对玩家造成伤害
            HealthSystem playerHealth = player.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log($"[GhostEnemyAI] {gameObject.name} 对玩家造成 {attackDamage} 点伤害");
            }
        }
        
        // 等待攻击动画完成
        yield return new WaitForSeconds(0.5f);
        
        isAttacking = false;
        
        // 攻击完成后，根据距离决定下一个状态
        float distanceAfterAttack = Vector3.Distance(transform.position, player.position);
        if (distanceAfterAttack <= followRange)
        {
            ChangeState(GhostState.Chasing);
        }
        else
        {
            ChangeState(GhostState.Idle);
        }
    }
    
    #endregion
    
    #region 特效系统（Dissolve）
    
    private void ProcessDissolve()
    {
        // 逐渐减少溶解值
        dissolveValue -= Time.deltaTime;
        
        // 应用溶解效果到所有材质
        if (meshRenderers != null)
        {
            foreach (var meshRenderer in meshRenderers)
            {
                if (meshRenderer != null && meshRenderer.material != null)
                {
                    meshRenderer.material.SetFloat("_Dissolve", dissolveValue);
                }
            }
        }
        
        // 溶解完成后禁用CharacterController（如果有）
        if (dissolveValue <= 0)
        {
            var characterController = GetComponent<CharacterController>();
            if (characterController != null)
            {
                characterController.enabled = false;
            }
            
            // 可以在这里添加销毁逻辑或重生逻辑
            Debug.Log($"[GhostEnemyAI] {gameObject.name} 溶解完成");
        }
    }
    
    #endregion
    
    #region 事件处理
    
    private void OnDeath()
    {
        Debug.Log($"[GhostEnemyAI] {gameObject.name} 死亡，开始溶解特效");
        
        // 停止所有移动
        if (navAgent != null)
        {
            navAgent.ResetPath();
            navAgent.enabled = false;
        }
        
        // 切换到溶解状态
        ChangeState(GhostState.Dissolving);
    }
    
    private void OnDamageTaken(float damage)
    {
        Debug.Log($"[GhostEnemyAI] {gameObject.name} 受到 {damage} 点伤害");
        
        // 如果还活着且不在攻击中，播放受击动画
        if (IsAlive && !isAttacking)
        {
            ChangeState(GhostState.Surprised);
        }
    }
    
    private void OnDestroy()
    {
        // 取消订阅事件
        if (healthSystem != null)
        {
            healthSystem.OnDeath -= OnDeath;
            healthSystem.OnDamageTaken -= OnDamageTaken;
        }
    }
    
    #endregion
    
    #region Gizmos绘制（调试用）
    
    private void OnDrawGizmosSelected()
    {
        // 绘制追踪范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, followRange);
        
        // 绘制攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // 绘制到玩家的连线
        if (player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
    
    #endregion
}
