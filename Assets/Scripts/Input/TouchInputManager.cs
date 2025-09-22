using UnityEngine;

/// <summary>
/// 触控输入管理器 - 处理移动端触控操作
/// </summary>
public class TouchInputManager : MonoBehaviour
{
    [Header("触控设置")]
    [SerializeField] private bool enableTouch = true;
    [SerializeField] private float dragSensitivity = 1.5f;
    [SerializeField] private float minDragDistance = 10f; // 最小拖拽距离（像素）
    
    // 输出事件
    public static System.Action<Vector2> OnTouchMove;
    public static System.Action OnTouchStart;
    public static System.Action OnTouchEnd;
    
    // 触控状态
    private Vector2 touchStartPos;
    private Vector2 lastTouchPos;
    private bool isDragging = false;
    private Camera mainCamera;
    
    // 移动方向
    private Vector2 currentMoveDirection = Vector2.zero;
    
    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
    }
    
    private void Update()
    {
        if (!enableTouch) return;
        
        // 处理触控输入
        HandleTouchInput();
        
        // 处理鼠标输入（用于PC测试）
        HandleMouseInput();
    }
    
    /// <summary>
    /// 处理触控输入
    /// </summary>
    private void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    StartTouch(touch.position);
                    break;
                    
                case TouchPhase.Moved:
                    UpdateTouch(touch.position);
                    break;
                    
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    EndTouch();
                    break;
            }
        }
        else if (isDragging)
        {
            // 没有触控时停止移动
            EndTouch();
        }
    }
    
    /// <summary>
    /// 处理鼠标输入（PC测试用）
    /// </summary>
    private void HandleMouseInput()
    {
        // 只在没有触控时处理鼠标输入
        if (Input.touchCount > 0) return;
        
        if (Input.GetMouseButtonDown(0))
        {
            StartTouch(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            UpdateTouch(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndTouch();
        }
    }
    
    /// <summary>
    /// 开始触控
    /// </summary>
    private void StartTouch(Vector2 screenPosition)
    {
        touchStartPos = screenPosition;
        lastTouchPos = screenPosition;
        isDragging = false;
        
        OnTouchStart?.Invoke();
    }
    
    /// <summary>
    /// 更新触控位置
    /// </summary>
    private void UpdateTouch(Vector2 screenPosition)
    {
        Vector2 deltaPosition = screenPosition - lastTouchPos;
        float distance = Vector2.Distance(screenPosition, touchStartPos);
        
        // 检查是否开始拖拽
        if (!isDragging && distance > minDragDistance)
        {
            isDragging = true;
        }
        
        if (isDragging)
        {
            // 计算移动方向（基于拖拽增量）
            Vector2 moveDirection = deltaPosition.normalized * dragSensitivity;
            
            // 转换屏幕坐标到世界坐标方向
            Vector3 worldDelta = mainCamera.ScreenToWorldPoint(new Vector3(deltaPosition.x, deltaPosition.y, 0));
            Vector2 worldMoveDirection = new Vector2(worldDelta.x, worldDelta.y).normalized;
            
            currentMoveDirection = worldMoveDirection;
            
            // 发送移动事件
            OnTouchMove?.Invoke(currentMoveDirection);
        }
        
        lastTouchPos = screenPosition;
    }
    
    /// <summary>
    /// 结束触控
    /// </summary>
    private void EndTouch()
    {
        isDragging = false;
        currentMoveDirection = Vector2.zero;
        
        // 发送停止移动事件
        OnTouchMove?.Invoke(Vector2.zero);
        OnTouchEnd?.Invoke();
    }
    
    /// <summary>
    /// 获取当前移动方向
    /// </summary>
    public Vector2 GetMoveDirection()
    {
        return currentMoveDirection;
    }
    
    /// <summary>
    /// 是否正在拖拽
    /// </summary>
    public bool IsDragging()
    {
        return isDragging;
    }
    
    /// <summary>
    /// 设置触控启用状态
    /// </summary>
    public void SetTouchEnabled(bool enabled)
    {
        enableTouch = enabled;
        if (!enabled)
        {
            EndTouch();
        }
    }
}
