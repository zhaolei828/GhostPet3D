using UnityEngine;

/// <summary>
/// 3D飞剑控制器 - 基于原FlyingSword适配
/// </summary>
public class FlyingSword3D : MonoBehaviour
{
    [Header("飞剑设置")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float damage = 1f;
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private LayerMask enemyLayer = 1 << 8; // Enemy层
    
    [Header("3D移动设置")]
    [SerializeField] private bool usePhysics = false;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private AnimationCurve speedCurve = AnimationCurve.Linear(0, 1, 1, 1);
    
    [Header("视觉效果")]
    [SerializeField] private ParticleSystem trailEffect;
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private float rotateAroundAxis = 0f; // 绕自身轴旋转
    
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
        // 启动时开始生命周期倒计时
        Destroy(gameObject, lifeTime);
    }
    
    private void Update()
    {
        if (!IsActive || hasHit) return;
        
        currentLifeTime += Time.deltaTime;
        
        // 更新移动
        UpdateMovement();
        
        // 更新旋转
        UpdateRotation();
        
        // 检查生命周期
        if (currentLifeTime >= lifeTime)
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
        
        // 启用物理（如果使用）
        if (usePhysics && rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = direction * speed;
        }
        
        // 启用粒子效果
        if (trailEffect != null)
        {
            trailEffect.Play();
        }
        
        Debug.Log($"[FlyingSword3D] 发射飞剑，方向: {direction}");
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
    
    private void UpdateMovement()
    {
        if (usePhysics && rb != null)
        {
            // 使用物理移动（已在Launch中设置velocity）
            return;
        }
        
        // 手动移动
        float currentSpeed = speed * speedCurve.Evaluate(currentLifeTime / lifeTime);
        
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
        
        // 移动
        transform.position += direction * currentSpeed * Time.deltaTime;
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
        if (hasHit || !IsActive) return;
        
        // 检查是否击中敌人
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            HitTarget(other.gameObject);
        }
        // 检查是否击中障碍物
        else if (other.CompareTag("Obstacle") || other.CompareTag("Wall"))
        {
            HitObstacle();
        }
    }
    
    private void HitTarget(GameObject target)
    {
        if (hasHit) return;
        
        hasHit = true;
        IsActive = false;
        
        Debug.Log($"[FlyingSword3D] 击中目标: {target.name}");
        
        // 对目标造成伤害
        var enemyHealth = target.GetComponent<HealthSystem>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
        }
        
        // 对Enemy3DAI造成伤害 (临时改为简化版本)
        var enemy3DAI = target.GetComponent<Enemy3DAI_Simple>();
        if (enemy3DAI != null)
        {
            enemy3DAI.TakeDamage(damage);
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
}
