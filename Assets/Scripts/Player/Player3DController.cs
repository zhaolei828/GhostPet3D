using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 3D玩家控制器 - 基于现有PlayerController适配
/// </summary>
public class Player3DController : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private LayerMask groundMask = 1;
    [SerializeField, Range(0.1f, 2.0f)] private float playerScale = 0.2f;
    
    [Header("边界限制")]
    [SerializeField] private float mapBoundary = 9f; // Ground是20x20，玩家活动范围限制在18x18内
    
    [Header("自动攻击")]
    [SerializeField] private float autoAttackRange = 8f;
    [SerializeField] private float autoAttackCooldown = 1f;
    
    [Header("组件引用")]
    [SerializeField] private InputActionAsset inputActions;
    
    // 3D组件
    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private MeshRenderer meshRenderer;
    private TouchInputManager touchInputManager;
    private HealthSystem healthSystem;
    
    // 输入相关
    private Vector2 moveInput;
    private Vector2 touchMoveInput;
    private bool jumpInput;
    
    // 地面检测
    private bool isGrounded;
    private float groundCheckDistance = 0.1f;
    
    // 玩家状态
    public bool IsAlive { get; private set; } = true;
    
    // 自动攻击相关
    private float lastAutoAttackTime;
    
    private void Awake()
    {
        Debug.Log("[Player3DController] Awake开始执行");
        
        // 获取3D组件
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        meshRenderer = GetComponent<MeshRenderer>();
        touchInputManager = FindFirstObjectByType<TouchInputManager>();
        healthSystem = GetComponent<HealthSystem>();
        
        // 设置物理属性
        if (rb != null)
        {
            rb.freezeRotation = true; // 防止翻转
        }
        
        // 应用玩家缩放
        transform.localScale = Vector3.one * playerScale;
        
        // 订阅死亡事件并设置玩家血量
        if (healthSystem != null)
        {
            healthSystem.OnDeath += Die;
            // 给玩家设置更多血量（200血）
            healthSystem.SetMaxHealth(200f, true);
        }
    }
    
    private void OnEnable()
    {
        if (inputActions != null)
        {
            var playerMap = inputActions.FindActionMap("Player");
            if (playerMap != null)
            {
                playerMap.Enable();
                var moveAction = playerMap.FindAction("Move");
                var jumpAction = playerMap.FindAction("Jump");
                
                if (moveAction != null)
                {
                    moveAction.performed += OnMove;
                    moveAction.canceled += OnMove;
                }
                
                if (jumpAction != null)
                {
                    jumpAction.performed += OnJump;
                }
            }
        }
        
        // 订阅触控输入
        TouchInputManager.OnTouchMove += OnTouchMove;
    }
    
    private void OnDisable()
    {
        if (inputActions != null)
        {
            var playerMap = inputActions.FindActionMap("Player");
            if (playerMap != null)
            {
                var moveAction = playerMap.FindAction("Move");
                var jumpAction = playerMap.FindAction("Jump");
                
                if (moveAction != null)
                {
                    moveAction.performed -= OnMove;
                    moveAction.canceled -= OnMove;
                }
                
                if (jumpAction != null)
                {
                    jumpAction.performed -= OnJump;
                }
                
                playerMap.Disable();
            }
        }
        
        TouchInputManager.OnTouchMove -= OnTouchMove;
    }
    
    private void Update()
    {
        CheckGrounded();
        HandleAutoAttack();
        
        // 测试玩家死亡重生 - 按Jump键
        if (inputActions != null && inputActions["Jump"].WasPressedThisFrame())
        {
            Debug.Log("[Player3DController] Jump键按下，测试玩家死亡");
            if (healthSystem != null)
            {
                healthSystem.TakeDamage(999f);
            }
        }
        
        // 删除了旧的Input系统测试代码
        
        // 每2秒打印一次状态信息
        if (Time.time % 2f < 0.1f)
        {
            Debug.Log($"[Player3DController] Update运行中, IsAlive: {IsAlive}, 血量: {healthSystem?.CurrentHealth}/{healthSystem?.MaxHealth}");
        }
    }
    
    private void FixedUpdate()
    {
        HandleMovement();
        HandleJump();
    }
    
    private void CheckGrounded()
    {
        // 地面检测
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        isGrounded = Physics.Raycast(rayOrigin, Vector3.down, groundCheckDistance + 0.1f, groundMask);
        
        // 调试射线
        Debug.DrawRay(rayOrigin, Vector3.down * (groundCheckDistance + 0.1f), 
                     isGrounded ? Color.green : Color.red);
    }
    
    private void HandleMovement()
    {
        if (!IsAlive) return;
        
        // 合并输入
        Vector2 finalInput = moveInput + touchMoveInput;
        
        // 转换为3D移动
        Vector3 movement = new Vector3(finalInput.x, 0, finalInput.y);
        
        // 限制移动向量长度
        if (movement.magnitude > 1f)
        {
            movement = movement.normalized;
        }
        
        // 应用移动
        Vector3 moveVelocity = movement * moveSpeed;
        
        // 检查边界限制
        Vector3 newPosition = transform.position + moveVelocity * Time.fixedDeltaTime;
        if (Mathf.Abs(newPosition.x) > mapBoundary || Mathf.Abs(newPosition.z) > mapBoundary)
        {
            // 如果移动会超出边界，限制移动方向
            if (Mathf.Abs(newPosition.x) > mapBoundary)
            {
                moveVelocity.x = 0;
            }
            if (Mathf.Abs(newPosition.z) > mapBoundary)
            {
                moveVelocity.z = 0;
            }
        }
        
        rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);
        
        // 根据移动方向旋转玩家
        if (movement.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);
        }
    }
    
    private void HandleJump()
    {
        if (jumpInput && isGrounded && IsAlive)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpInput = false;
        }
    }
    
    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    
    private void OnJump(InputAction.CallbackContext context)
    {
        jumpInput = true;
    }
    
    /// <summary>
    /// 自动攻击处理
    /// </summary>
    private void HandleAutoAttack()
    {
        if (!IsAlive) return;
        if (Time.time < lastAutoAttackTime + autoAttackCooldown) return;
        
        // 寻找攻击范围内的敌人
        Transform nearestEnemy = FindNearestEnemyInRange();
        if (nearestEnemy == null) 
        {
            // 每5秒打印一次调试信息
            if (Time.time % 5f < 0.1f)
            {
                Debug.Log($"[Player3DController] 没有找到攻击范围内的敌人，攻击范围: {autoAttackRange}米");
            }
            return;
        }
        
        Debug.Log($"[Player3DController] 找到敌人: {nearestEnemy.name}，距离: {Vector3.Distance(transform.position, nearestEnemy.position):F1}米");
        
        // 优先使用环绕飞剑攻击
        OrbitingSwordManager orbitManager = OrbitingSwordManager.Instance;
        if (orbitManager != null)
        {
            Vector3 directionToEnemy = (nearestEnemy.position - transform.position).normalized;
            bool launched = orbitManager.LaunchNearestSword(directionToEnemy, nearestEnemy);
            
            if (launched)
            {
                lastAutoAttackTime = Time.time;
                Debug.Log($"[Player3DController] 环绕飞剑自动攻击敌人: {nearestEnemy.name}");
                return;
            }
        }
    }
    
    private void OnTouchMove(Vector2 direction)
    {
        touchMoveInput = direction;
    }
    
    /// <summary>
    /// 寻找攻击范围内最近的敌人
    /// </summary>
    private Transform FindNearestEnemyInRange()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform nearest = null;
        float nearestDistance = autoAttackRange;
        
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null && enemy.activeInHierarchy)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance <= autoAttackRange && distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = enemy.transform;
                }
            }
        }
        
        return nearest;
    }
    
    public void Die()
    {
        if (!IsAlive) return;
        
        IsAlive = false;
        rb.linearVelocity = Vector3.zero;
        
        Debug.Log("玩家死亡！");
        // GameManager.Instance?.RespawnPlayer(); // 临时注释，等MCP配置好再启用
    }
    
    public void Respawn(Vector3 spawnPosition)
    {
        transform.position = spawnPosition;
        IsAlive = true;
        rb.linearVelocity = Vector3.zero;
        
        if (healthSystem != null)
        {
            healthSystem.Revive(1f);
        }
        
        Debug.Log("玩家重生！");
    }
    
    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDeath -= Die;
        }
    }
}

