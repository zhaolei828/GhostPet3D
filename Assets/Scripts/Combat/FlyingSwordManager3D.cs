using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 3D飞剑管理器 - 基于原FlyingSwordManager适配
/// </summary>
public class FlyingSwordManager3D : MonoBehaviour
{
    [Header("飞剑预制件")]
    [SerializeField] private GameObject flyingSwordPrefab;
    [SerializeField] private GameObject energySwordPrefab; // 充能状态飞剑
    
    [Header("发射设置")]
    [SerializeField] private Transform launchPoint; // 发射点
    [SerializeField] private float launchCooldown = 0.5f;
    [SerializeField] private int maxActiveSwords = 3;
    [SerializeField] private float autoTargetRange = 15f;
    
    [Header("3D环形发射")]
    [SerializeField] private bool useCircularLaunch = true;
    [SerializeField] private int circularSwordCount = 8; // 环形发射数量
    [SerializeField] private float circularRadius = 2f;
    [SerializeField] private float launchDelay = 0.1f; // 连续发射间隔
    
    [Header("能量系统")]
    [SerializeField] private bool useEnergySystem = true;
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float energyCostPerSword = 20f;
    [SerializeField] private float energyRegenRate = 10f; // 每秒恢复
    
    // 私有变量
    private float lastLaunchTime;
    private float currentEnergy;
    private List<FlyingSword3D> activeSwords = new List<FlyingSword3D>();
    private Transform player;
    private Camera playerCamera;
    
    // 单例实例
    public static FlyingSwordManager3D Instance { get; private set; }
    
    // 公共属性
    public float CurrentEnergy => currentEnergy;
    public float MaxEnergy => maxEnergy;
    public int ActiveSwordCount => activeSwords.Count;
    public bool CanLaunch => CanLaunchSword();
    
    // 事件
    public System.Action<float> OnEnergyChanged;
    public System.Action<int> OnActiveSwordCountChanged;
    
    private void Awake()
    {
        // 单例模式 - 修复：不销毁Player对象，只销毁重复组件
        if (Instance == null)
        {
            Instance = this;
            // 注释掉DontDestroyOnLoad，避免Player对象跨场景保持
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 只销毁重复的组件，不销毁整个Player对象
            if (Application.isEditor)
            {
                Debug.Log("[FlyingSwordManager3D] 检测到重复组件，清理中... (正常情况，无需担心)");
            }
            Destroy(this);
            return;
        }
        
        // 初始化能量
        currentEnergy = maxEnergy;
        
        // 查找玩家
        var playerController = FindFirstObjectByType<Player3DController>();
        if (playerController != null)
        {
            player = playerController.transform;
            
            // 如果没有设置发射点，使用玩家位置
            if (launchPoint == null)
            {
                launchPoint = player;
            }
        }
        
        // 查找摄像机
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindFirstObjectByType<Camera>();
        }
    }
    
    private void Update()
    {
        // 能量恢复
        if (useEnergySystem && currentEnergy < maxEnergy)
        {
            currentEnergy = Mathf.Min(maxEnergy, currentEnergy + energyRegenRate * Time.deltaTime);
            OnEnergyChanged?.Invoke(currentEnergy);
        }
        
        // 清理已销毁的飞剑
        CleanupDestroyedSwords();
    }
    
    /// <summary>
    /// 发射单个飞剑
    /// </summary>
    public bool LaunchSingle(Vector3 direction, Transform target = null)
    {
        if (!CanLaunchSword()) return false;
        
        GameObject swordPrefab = ShouldUseEnergySword() ? energySwordPrefab : flyingSwordPrefab;
        if (swordPrefab == null)
        {
            Debug.LogWarning("[FlyingSwordManager3D] 飞剑预制件未设置！");
            return false;
        }
        
        // 创建飞剑
        Vector3 spawnPosition = GetLaunchPosition();
        GameObject swordObj = Instantiate(swordPrefab, spawnPosition, Quaternion.identity);
        FlyingSword3D sword = swordObj.GetComponent<FlyingSword3D>();
        
        if (sword != null)
        {
            sword.Launch(direction, target);
            activeSwords.Add(sword);
            
            // 消耗能量
            if (useEnergySystem)
            {
                currentEnergy = Mathf.Max(0, currentEnergy - energyCostPerSword);
                OnEnergyChanged?.Invoke(currentEnergy);
            }
            
            lastLaunchTime = Time.time;
            OnActiveSwordCountChanged?.Invoke(activeSwords.Count);
            
            Debug.Log($"[FlyingSwordManager3D] 发射飞剑，方向: {direction}");
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 环形发射多个飞剑
    /// </summary>
    public void LaunchCircular()
    {
        if (!CanLaunchSword()) return;
        
        StartCoroutine(LaunchCircularCoroutine());
    }
    
    private IEnumerator LaunchCircularCoroutine()
    {
        Vector3 centerPosition = GetLaunchPosition();
        
        for (int i = 0; i < circularSwordCount; i++)
        {
            if (!CanLaunchSword()) break;
            
            // 计算环形位置和方向
            float angle = (360f / circularSwordCount) * i * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * circularRadius;
            Vector3 direction = offset.normalized;
            
            // 发射飞剑
            LaunchSingle(direction);
            
            // 等待间隔
            if (i < circularSwordCount - 1)
            {
                yield return new WaitForSeconds(launchDelay);
            }
        }
    }
    
    /// <summary>
    /// 向鼠标位置发射飞剑
    /// </summary>
    public bool LaunchTowardsMouse()
    {
        if (playerCamera == null || player == null) return false;
        
        // 获取鼠标射线
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        
        // 在玩家高度的平面上计算交点
        Plane plane = new Plane(Vector3.up, player.position);
        
        if (plane.Raycast(ray, out float distance))
        {
            Vector3 targetPoint = ray.GetPoint(distance);
            Vector3 direction = (targetPoint - GetLaunchPosition()).normalized;
            
            return LaunchSingle(direction);
        }
        
        return false;
    }
    
    /// <summary>
    /// 自动瞄准最近的敌人发射
    /// </summary>
    public bool LaunchAutoTarget()
    {
        Transform nearestEnemy = FindNearestEnemy();
        
        if (nearestEnemy != null)
        {
            Vector3 direction = (nearestEnemy.position - GetLaunchPosition()).normalized;
            return LaunchSingle(direction, nearestEnemy);
        }
        
        return false;
    }
    
    /// <summary>
    /// 停止所有飞剑
    /// </summary>
    public void StopAllSwords()
    {
        foreach (var sword in activeSwords)
        {
            if (sword != null)
            {
                sword.Stop();
            }
        }
    }
    
    /// <summary>
    /// 清理所有飞剑
    /// </summary>
    public void ClearAllSwords()
    {
        foreach (var sword in activeSwords)
        {
            if (sword != null)
            {
                Destroy(sword.gameObject);
            }
        }
        
        activeSwords.Clear();
        OnActiveSwordCountChanged?.Invoke(0);
    }
    
    private bool CanLaunchSword()
    {
        // 检查冷却时间
        if (Time.time < lastLaunchTime + launchCooldown) return false;
        
        // 检查数量限制
        if (activeSwords.Count >= maxActiveSwords) return false;
        
        // 检查能量
        if (useEnergySystem && currentEnergy < energyCostPerSword) return false;
        
        return true;
    }
    
    private bool ShouldUseEnergySword()
    {
        // 根据能量水平决定是否使用充能飞剑
        return useEnergySystem && currentEnergy >= maxEnergy * 0.8f;
    }
    
    private Vector3 GetLaunchPosition()
    {
        if (launchPoint != null)
        {
            return launchPoint.position;
        }
        
        if (player != null)
        {
            return player.position + Vector3.up * 1f; // 玩家上方1米
        }
        
        return transform.position;
    }
    
    private Transform FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform nearest = null;
        float nearestDistance = float.MaxValue;
        Vector3 launchPos = GetLaunchPosition();
        
        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;
            
            float distance = Vector3.Distance(launchPos, enemy.transform.position);
            
            if (distance <= autoTargetRange && distance < nearestDistance)
            {
                nearest = enemy.transform;
                nearestDistance = distance;
            }
        }
        
        return nearest;
    }
    
    private void CleanupDestroyedSwords()
    {
        activeSwords.RemoveAll(sword => sword == null);
        OnActiveSwordCountChanged?.Invoke(activeSwords.Count);
    }
    
    /// <summary>
    /// 设置发射点
    /// </summary>
    public void SetLaunchPoint(Transform newLaunchPoint)
    {
        launchPoint = newLaunchPoint;
    }
    
    /// <summary>
    /// 添加能量
    /// </summary>
    public void AddEnergy(float amount)
    {
        currentEnergy = Mathf.Min(maxEnergy, currentEnergy + amount);
        OnEnergyChanged?.Invoke(currentEnergy);
    }
    
    /// <summary>
    /// 消耗能量
    /// </summary>
    public bool ConsumeEnergy(float amount)
    {
        if (currentEnergy >= amount)
        {
            currentEnergy -= amount;
            OnEnergyChanged?.Invoke(currentEnergy);
            return true;
        }
        
        return false;
    }
    
    // 调试用Gizmos
    private void OnDrawGizmosSelected()
    {
        // 绘制自动瞄准范围
        Gizmos.color = Color.yellow;
        Vector3 position = GetLaunchPosition();
        Gizmos.DrawWireSphere(position, autoTargetRange);
        
        // 绘制环形发射预览
        if (useCircularLaunch)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < circularSwordCount; i++)
            {
                float angle = (360f / circularSwordCount) * i * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * circularRadius;
                Vector3 startPos = position + offset;
                Vector3 direction = offset.normalized;
                
                Gizmos.DrawWireSphere(startPos, 0.1f);
                Gizmos.DrawRay(startPos, direction);
            }
        }
        
        // 绘制发射点
        if (launchPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(launchPoint.position, 0.3f);
        }
    }
}
