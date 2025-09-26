using UnityEngine;

/// <summary>
/// 飞剑生命周期状态枚举（避免与UI的SwordState冲突）
/// </summary>
public enum FlyingSwordLifecycleState
{
    Orbiting,    // 环绕状态
    Launching,   // 发射状态  
    Flying,      // 飞行状态
    Destroyed    // 销毁状态
}

/// <summary>
/// 3D飞剑控制器 - 基于原FlyingSword适配
/// </summary>
public class FlyingSword3D : MonoBehaviour
{
    [Header("飞剑设置")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float damage = 25f; // 增加伤害，让敌人能够快速死亡
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private LayerMask enemyLayer = -1; // 检测所有层级（临时修复）
    
    [Header("3D移动设置")]
    [SerializeField] private bool usePhysics = false;
    // [SerializeField] private float rotationSpeed = 360f; // 预留功能，暂未使用
    [SerializeField] private AnimationCurve speedCurve = AnimationCurve.Linear(0, 1, 1, 1);
    
    [Header("视觉效果")]
    [SerializeField] private ParticleSystem trailEffect;
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private float rotateAroundAxis = 0f; // 绕自身轴旋转
    
    [Header("调试设置")]
    [SerializeField] private bool enableDebugLogs = false; // 临时启用专门调试FlyingSword3D
    
    // 私有变量
    private Vector3 direction;
    private Transform target;
    private Rigidbody rb;
    private MeshRenderer meshRenderer;
    private Collider swordCollider;
    private bool hasHit = false;
    private float currentLifeTime = 0f;
    private Vector3 startPosition;
    
    // 飞剑状态
    public bool IsActive { get; private set; } = false;
    public FlyingSwordLifecycleState CurrentState { get; private set; } = FlyingSwordLifecycleState.Orbiting;
    public Vector3 Direction => direction;
    public Transform Target => target;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        meshRenderer = GetComponent<MeshRenderer>();
        swordCollider = GetComponent<Collider>();
        
        if (rb != null && !usePhysics)
        {
            rb.isKinematic = true;
        }
        
        startPosition = transform.position;
    }
    
    private void Start()
    {
        // 什么都不做，生命周期在Launch方法中处理
        if (enableDebugLogs)
            Debug.Log($"[FlyingSword3D] Start方法执行，IsActive: {IsActive}, State: {CurrentState}");
    }
    
    private void Update()
    {
        // 环绕状态下跳过更新，但保持碰撞检测
        if (CurrentState == FlyingSwordLifecycleState.Orbiting || hasHit) return;
        
        // 只有飞行状态下才检查IsActive
        if (CurrentState == FlyingSwordLifecycleState.Flying && !IsActive) return;
        
        currentLifeTime += Time.deltaTime;
        
        // 每0.5秒打印一次位置信息用于调试（仅在启用调试时）
        if (enableDebugLogs && Time.time % 0.5f < 0.1f)
        {
            Debug.Log($"[FlyingSword3D] 飞剑移动中: 位置{transform.position}, 方向{direction}, IsActive:{IsActive}, State:{CurrentState}");
        }
        
        // 更新移动
        UpdateMovement();
        
        // 更新旋转
        UpdateRotation();
        
        // 强制距离检测：绕过物理碰撞检测的问题
        if (CurrentState == FlyingSwordLifecycleState.Flying)
        {
            CheckDistanceCollision();
        }
        
        // 检查生命周期（仅飞行状态）
        if (CurrentState == FlyingSwordLifecycleState.Flying && currentLifeTime >= lifeTime)
        {
            DestroyFlyingSword();
        }
    }
    
    /// <summary>
    /// 发射飞剑
    /// </summary>
    public void Launch(Vector3 launchDirection, Transform targetTransform = null)
    {
        direction = launchDirection.normalized;
        target = targetTransform;
        IsActive = true;
        currentLifeTime = 0f;
        hasHit = false;
        
        // 设置初始旋转
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
        
        // 启用物理和碰撞检测
        if (rb != null)
        {
            rb.isKinematic = false; // 必须为false才能触发OnTriggerEnter
            if (usePhysics)
            {
                rb.linearVelocity = direction * speed;
            }
        }
        
        // 启用粒子效果
        if (trailEffect != null)
        {
            trailEffect.Play();
        }
        
        // 更新状态为发射
        CurrentState = FlyingSwordLifecycleState.Launching;
        
        if (enableDebugLogs)
            Debug.Log($"[FlyingSword3D] 发射飞剑，方向: {direction}, IsActive: {IsActive}, State: {CurrentState}");
    }
    
    /// <summary>
    /// 设置飞剑属性
    /// </summary>
    public void SetProperties(float newSpeed, float newDamage, float newLifeTime)
    {
        speed = newSpeed;
        damage = newDamage;
        lifeTime = newLifeTime;
    }
    
    /// <summary>
    /// 重置飞剑状态（用于环绕模式）
    /// </summary>
    public void ResetSword()
    {
        // 重置为环绕状态，但不设置IsActive = false
        // 这样可以保持碰撞检测能力
        hasHit = false;
        currentLifeTime = 0f;
        direction = Vector3.zero;
        target = null;
        
        // 设置为环绕状态（碰撞检测仍然工作）
        CurrentState = FlyingSwordLifecycleState.Orbiting;
        IsActive = true; // 保持活跃以支持碰撞检测
        
        // 重置物理状态为环绕模式
        if (rb != null)
        {
            // 先重置velocity，再设置为kinematic（避免警告）
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            rb.isKinematic = true;
        }
        
        // 停止粒子效果
        if (trailEffect != null)
        {
            trailEffect.Stop();
        }
        
        if (enableDebugLogs)
            Debug.Log($"[FlyingSword3D] 飞剑状态已重置为环绕模式，State: {CurrentState}");
    }
    
    private void UpdateMovement()
    {
        // 从发射状态转换到飞行状态
        if (CurrentState == FlyingSwordLifecycleState.Launching)
        {
            CurrentState = FlyingSwordLifecycleState.Flying;
        }
        
        if (usePhysics && rb != null)
        {
            // 使用物理移动（已在Launch中设置velocity）
            if (enableDebugLogs)
                Debug.Log($"[FlyingSword3D] 使用物理移动, velocity: {rb.linearVelocity}");
            return;
        }
        
        // 手动移动
        float currentSpeed = speed * speedCurve.Evaluate(currentLifeTime / lifeTime);
        if (enableDebugLogs)
            Debug.Log($"[FlyingSword3D] 手动移动 - direction: {direction}, speed: {speed}, currentSpeed: {currentSpeed}");
        
        if (target != null && Vector3.Distance(transform.position, target.position) > 0.5f)
        {
            // 追踪目标
            Vector3 targetDirection = (target.position - transform.position).normalized;
            direction = Vector3.Slerp(direction, targetDirection, Time.deltaTime * 2f);
            
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
        
        // 强制移动，即使方向为零也要记录
        Vector3 movement = direction * currentSpeed * Time.deltaTime;
        Vector3 oldPosition = transform.position;
        transform.position += movement;
        if (enableDebugLogs)
            Debug.Log($"[FlyingSword3D] 位置移动: {oldPosition} -> {transform.position}, movement: {movement}");
    }
    
    private void UpdateRotation()
    {
        // 绕自身轴旋转（视觉效果）
        if (Mathf.Abs(rotateAroundAxis) > 0.1f)
        {
            transform.Rotate(Vector3.forward, rotateAroundAxis * Time.deltaTime);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (enableDebugLogs)
            Debug.Log($"[FlyingSword3D] OnTriggerEnter检测到碰撞: {other.name}, layer: {other.gameObject.layer}, hasHit: {hasHit}, IsActive: {IsActive}, State: {CurrentState}");
        HandleCollision(other);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (enableDebugLogs)
            Debug.Log($"[FlyingSword3D] OnCollisionEnter检测到碰撞: {collision.gameObject.name}, layer: {collision.gameObject.layer}, hasHit: {hasHit}, IsActive: {IsActive}, State: {CurrentState}");
        HandleCollision(collision.collider);
    }
    
    private void HandleCollision(Collider other)
    {
        if (hasHit) return;
        
        // 环绕状态下不处理碰撞
        if (CurrentState == FlyingSwordLifecycleState.Orbiting) return;
        
        // 飞行状态下需要检查IsActive
        if (CurrentState == FlyingSwordLifecycleState.Flying && !IsActive) return;
        
        // 检查是否击中敌人
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            if (enableDebugLogs)
                Debug.Log($"[FlyingSword3D] 成功击中敌人: {other.name}!");
            HitTarget(other.gameObject);
        }
        // 检查是否击中障碍物（安全检查标签是否存在）
        else if (HasTag(other.gameObject, "Obstacle") || HasTag(other.gameObject, "Wall"))
        {
            HitObstacle();
        }
    }
    
    private void HitTarget(GameObject target)
    {
        if (hasHit) return;
        
        hasHit = true;
        IsActive = false;
        CurrentState = FlyingSwordLifecycleState.Destroyed;
        
        if (enableDebugLogs)
            Debug.Log($"[FlyingSword3D] 击中目标: {target.name}，位置: {target.transform.position}");
        
        // 对目标造成伤害
        var enemyHealth = target.GetComponent<HealthSystem>();
        if (enemyHealth != null)
        {
            if (enableDebugLogs)
                Debug.Log($"[FlyingSword3D] 对 {target.name} 造成 {damage} 点伤害");
            enemyHealth.TakeDamage(damage);
        }
        else
        {
            if (enableDebugLogs)
                Debug.LogWarning($"[FlyingSword3D] {target.name} 没有HealthSystem组件");
        }
        
        
        // 创建击中特效
        CreateHitEffect();
        
        // 销毁飞剑
        DestroyFlyingSword();
    }
    
    private void HitObstacle()
    {
        if (hasHit) return;
        
        hasHit = true;
        IsActive = false;
        CurrentState = FlyingSwordLifecycleState.Destroyed;
        
        if (enableDebugLogs)
            Debug.Log("[FlyingSword3D] 击中障碍物");
        
        // 创建击中特效
        CreateHitEffect();
        
        // 销毁飞剑
        DestroyFlyingSword();
    }
    
    private void CreateHitEffect()
    {
        if (hitEffect != null)
        {
            GameObject effect = Instantiate(hitEffect, transform.position, transform.rotation);
            Destroy(effect, 2f);
        }
        
        // 停止粒子效果
        if (trailEffect != null)
        {
            trailEffect.Stop();
        }
    }
    
    private void DestroyFlyingSword()
    {
        // 停止粒子效果
        if (trailEffect != null)
        {
            trailEffect.Stop();
        }
        
        // 禁用碰撞器防止重复触发
        if (swordCollider != null)
        {
            swordCollider.enabled = false;
        }
        
        // 销毁对象
        Destroy(gameObject, 0.5f);
    }
    
    /// <summary>
    /// 立即停止飞剑
    /// </summary>
    public void Stop()
    {
        IsActive = false;
        
        if (usePhysics && rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        
        if (trailEffect != null)
        {
            trailEffect.Stop();
        }
    }
    
    /// <summary>
    /// 重置飞剑到初始状态
    /// </summary>
    public void Reset()
    {
        transform.position = startPosition;
        direction = Vector3.zero;
        target = null;
        IsActive = false;
        hasHit = false;
        currentLifeTime = 0f;
        
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = !usePhysics;
        }
        
        if (swordCollider != null)
        {
            swordCollider.enabled = true;
        }
        
        if (trailEffect != null)
        {
            trailEffect.Stop();
        }
    }
    
    /// <summary>
    /// 安全检查游戏对象是否有指定标签
    /// </summary>
    private bool HasTag(GameObject obj, string tagName)
    {
        try
        {
            return obj.CompareTag(tagName);
        }
        catch (UnityException)
        {
            // 标签不存在，返回false
            return false;
        }
    }
    
    // 调试用Gizmos
    private void OnDrawGizmosSelected()
    {
        // 绘制飞行方向
        if (IsActive && direction != Vector3.zero)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, direction * 2f);
        }
        
        // 绘制到目标的线
        if (target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, target.position);
        }
        
        // 绘制起始位置
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(startPosition, 0.2f);
    }
    
    /// <summary>
    /// 强制距离检测敌人（绕过物理碰撞问题）
    /// </summary>
    private void CheckDistanceCollision()
    {
        // 查找所有敌人
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            if (enemy.activeInHierarchy)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance <= 2f) // 2米内算击中
                {
                    if (enableDebugLogs)
                        Debug.Log($"[FlyingSword3D] 距离击中敌人: {enemy.name}, 距离: {distance:F2}米");
                    
                    // 直接触发敌人受伤
                    HealthSystem enemyHealth = enemy.GetComponent<HealthSystem>();
                    if (enemyHealth != null)
                    {
                        enemyHealth.TakeDamage(damage);
                        if (enableDebugLogs)
                            Debug.Log($"[FlyingSword3D] 敌人受伤: {enemy.name}, 伤害: {damage}");
                    }
                    
                    // 击中后销毁飞剑
                    HitTarget(enemy);
                    return;
                }
            }
        }
    }
}
