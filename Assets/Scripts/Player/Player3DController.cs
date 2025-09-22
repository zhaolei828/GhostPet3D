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
    
    private void Awake()
    {
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
        
        // 订阅死亡事件
        if (healthSystem != null)
        {
            healthSystem.OnDeath += Die;
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
                var attackAction = playerMap.FindAction("Attack");
                
                if (moveAction != null)
                {
                    moveAction.performed += OnMove;
                    moveAction.canceled += OnMove;
                }
                
                if (jumpAction != null)
                {
                    jumpAction.performed += OnJump;
                }
                
                if (attackAction != null)
                {
                    attackAction.performed += OnAttack;
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
                var attackAction = playerMap.FindAction("Attack");
                
                if (moveAction != null)
                {
                    moveAction.performed -= OnMove;
                    moveAction.canceled -= OnMove;
                }
                
                if (jumpAction != null)
                {
                    jumpAction.performed -= OnJump;
                }
                
                if (attackAction != null)
                {
                    attackAction.performed -= OnAttack;
                }
                
                playerMap.Disable();
            }
        }
        
        TouchInputManager.OnTouchMove -= OnTouchMove;
    }
    
    private void Update()
    {
        CheckGrounded();
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
    
    private void OnAttack(InputAction.CallbackContext context)
    {
        if (!IsAlive) return;
        Debug.Log("玩家攻击！");
        // TODO: 实现3D攻击逻辑
    }
    
    private void OnTouchMove(Vector2 direction)
    {
        touchMoveInput = direction;
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

