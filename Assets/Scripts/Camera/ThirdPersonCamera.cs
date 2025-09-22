using UnityEngine;

/// <summary>
/// 3D第三人称摄像机 - 基于原CameraFollow适配
/// </summary>
public class ThirdPersonCamera : MonoBehaviour
{
    [Header("跟随设置")]
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 8f;
    [SerializeField] private float height = 4f;
    [SerializeField] private float angle = 35f; // 俯视角度
    
    [Header("平滑设置")]
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private bool smoothRotation = true;
    
    [Header("偏移设置")]
    [SerializeField] private Vector3 offset = Vector3.zero;
    [SerializeField] private bool maintainWorldUp = true;
    
    [Header("边界限制")]
    [SerializeField] private bool useMinDistance = true;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private bool useMaxDistance = true;
    [SerializeField] private float maxDistance = 15f;
    
    // 私有变量
    private Vector3 currentVelocity;
    private Vector3 targetPosition;
    private bool isFollowing = true;
    
    // 公共属性
    public Transform Target => target;
    public bool IsFollowing => isFollowing;
    
    private void Start()
    {
        // 自动查找玩家目标
        if (target == null)
        {
            var player = FindFirstObjectByType<Player3DController>();
            if (player != null)
            {
                SetTarget(player.transform);
            }
        }
        
        // 设置初始位置
        if (target != null)
        {
            UpdateCameraPosition(true);
        }
    }
    
    private void LateUpdate()
    {
        if (target != null && isFollowing)
        {
            UpdateCameraPosition(false);
        }
    }
    
    /// <summary>
    /// 设置跟随目标
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        Debug.Log($"[ThirdPersonCamera] 设置跟随目标: {newTarget.name}");
    }
    
    /// <summary>
    /// 开始/停止跟随
    /// </summary>
    public void SetFollowing(bool follow)
    {
        isFollowing = follow;
    }
    
    /// <summary>
    /// 更新摄像机位置
    /// </summary>
    private void UpdateCameraPosition(bool immediate)
    {
        // 计算理想位置
        Vector3 idealPosition = CalculateIdealPosition();
        
        if (immediate)
        {
            // 立即设置位置
            transform.position = idealPosition;
            LookAtTarget();
        }
        else
        {
            // 平滑移动到目标位置
            transform.position = Vector3.SmoothDamp(
                transform.position, 
                idealPosition, 
                ref currentVelocity, 
                1f / followSpeed
            );
            
            // 平滑旋转看向目标
            if (smoothRotation)
            {
                LookAtTargetSmooth();
            }
            else
            {
                LookAtTarget();
            }
        }
    }
    
    /// <summary>
    /// 计算理想摄像机位置
    /// </summary>
    private Vector3 CalculateIdealPosition()
    {
        if (target == null) return transform.position;
        
        // 应用距离限制
        float clampedDistance = distance;
        if (useMinDistance) clampedDistance = Mathf.Max(clampedDistance, minDistance);
        if (useMaxDistance) clampedDistance = Mathf.Min(clampedDistance, maxDistance);
        
        // 计算摄像机位置
        Vector3 targetPos = target.position + offset;
        
        // 根据俯视角度计算位置
        float radianAngle = angle * Mathf.Deg2Rad;
        Vector3 cameraOffset = new Vector3(
            0,
            height + Mathf.Sin(radianAngle) * clampedDistance,
            -Mathf.Cos(radianAngle) * clampedDistance
        );
        
        return targetPos + cameraOffset;
    }
    
    /// <summary>
    /// 直接看向目标
    /// </summary>
    private void LookAtTarget()
    {
        if (target == null) return;
        
        Vector3 lookPosition = target.position + offset;
        Vector3 direction = (lookPosition - transform.position).normalized;
        
        if (maintainWorldUp)
        {
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
        else
        {
            transform.LookAt(lookPosition);
        }
    }
    
    /// <summary>
    /// 平滑旋转看向目标
    /// </summary>
    private void LookAtTargetSmooth()
    {
        if (target == null) return;
        
        Vector3 lookPosition = target.position + offset;
        Vector3 direction = (lookPosition - transform.position).normalized;
        
        Quaternion targetRotation;
        if (maintainWorldUp)
        {
            targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        }
        else
        {
            targetRotation = Quaternion.LookRotation(direction);
        }
        
        transform.rotation = Quaternion.Slerp(
            transform.rotation, 
            targetRotation, 
            rotationSpeed * Time.deltaTime
        );
    }
    
    /// <summary>
    /// 设置摄像机距离
    /// </summary>
    public void SetDistance(float newDistance)
    {
        distance = newDistance;
    }
    
    /// <summary>
    /// 设置摄像机高度
    /// </summary>
    public void SetHeight(float newHeight)
    {
        height = newHeight;
    }
    
    /// <summary>
    /// 设置俯视角度
    /// </summary>
    public void SetAngle(float newAngle)
    {
        angle = Mathf.Clamp(newAngle, 0f, 90f);
    }
    
    // Gizmos用于编辑器调试
    private void OnDrawGizmosSelected()
    {
        if (target == null) return;
        
        // 绘制跟随线
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, target.position);
        
        // 绘制目标点
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(target.position + offset, 0.5f);
        
        // 绘制理想位置
        Gizmos.color = Color.green;
        Vector3 idealPos = CalculateIdealPosition();
        Gizmos.DrawWireSphere(idealPos, 0.3f);
    }
}

