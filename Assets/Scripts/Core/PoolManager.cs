using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Collections;

/// <summary>
/// 可监控池接口 - 为对象池提供统一的监控和管理能力
/// </summary>
public interface IPoolMonitorable
{
    string PoolName { get; }
    int TotalCount { get; }
    int ActiveCount { get; }
    int AvailableCount { get; }
    float EstimatedMemoryUsage { get; } // MB
    bool CanOptimize { get; }
    void OptimizePool(int newSize);
    void PreloadPool(int count);
    void ClearPool();
    string GetDetailedStats();
}

/// <summary>
/// 对象池管理器 - 统一监控和优化所有对象池的性能
/// </summary>
public class PoolManager : MonoBehaviour
{
    [Header("监控设置")]
    [SerializeField] private bool enableMonitoring = true;         // 是否启用监控
    [SerializeField] private float monitoringInterval = 5f;        // 监控间隔（秒）
    [SerializeField] private bool enableAutoOptimization = true;   // 是否启用自动优化
    [SerializeField] private bool enableDebugLog = false;          // 是否启用调试日志
    
    [Header("优化参数")]
    [SerializeField] private float poolUtilizationThreshold = 0.8f; // 池利用率阈值
    [SerializeField] private int minPoolSize = 5;                   // 最小池大小
    [SerializeField] private int maxPoolSize = 200;                 // 最大池大小
    [SerializeField] private float memoryWarningThreshold = 100f;   // 内存警告阈值（MB）
    
    [Header("性能统计")]
    [SerializeField] private bool showRuntimeStats = true;          // 是否显示运行时统计
    
    // 公共属性
    public bool ShowRuntimeStats => showRuntimeStats;
    
    // 单例模式
    public static PoolManager Instance { get; private set; }
    
    // 池监控数据
    private Dictionary<string, PoolStats> poolStats = new Dictionary<string, PoolStats>();
    private List<IPoolMonitorable> monitorablePools = new List<IPoolMonitorable>();
    
    // 性能统计
    private float totalMemoryUsed = 0f;
    private int totalActiveObjects = 0;
    private int totalPooledObjects = 0;
    private float lastOptimizationTime = 0f;
    
    /// <summary>
    /// 池统计数据结构
    /// </summary>
    [System.Serializable]
    public class PoolStats
    {
        public string poolName;
        public int totalCount;
        public int activeCount;
        public int availableCount;
        public float utilizationRate;
        public float memoryUsage; // MB
        public float lastAccessTime;
        public int optimizationCount;
        public bool needsOptimization;
        
        public PoolStats(string name)
        {
            poolName = name;
            lastAccessTime = Time.time;
        }
        
        public void UpdateStats(int total, int active, int available, float memory = 0f)
        {
            totalCount = total;
            activeCount = active;
            availableCount = available;
            utilizationRate = total > 0 ? (float)active / total : 0f;
            memoryUsage = memory;
            lastAccessTime = Time.time;
            needsOptimization = utilizationRate > 0.9f || utilizationRate < 0.3f;
        }
    }
    
    
    private void Awake()
    {
        // 单例模式实现
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePoolManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 初始化池管理器
    /// </summary>
    private void InitializePoolManager()
    {
        if (enableMonitoring)
        {
            StartCoroutine(MonitoringCoroutine());
        }
        
        // 自动发现并注册所有对象池
        DiscoverPools();
        
        Debug.Log("[PoolManager] 对象池管理器初始化完成");
    }
    
    /// <summary>
    /// 监控协程
    /// </summary>
    private IEnumerator MonitoringCoroutine()
    {
        while (enableMonitoring)
        {
            yield return new WaitForSeconds(monitoringInterval);
            
            UpdateAllPoolStats();
            
            if (enableAutoOptimization)
            {
                PerformAutoOptimization();
            }
            
            CheckMemoryUsage();
            
            if (enableDebugLog)
            {
                LogPoolStatistics();
            }
        }
    }
    
    /// <summary>
    /// 自动发现对象池
    /// </summary>
    private void DiscoverPools()
    {
        // 注册伤害数字池
        if (DamageNumberPool.Instance != null)
        {
            RegisterPool(new DamageNumberPoolAdapter(DamageNumberPool.Instance));
        }
        
        // 注册敌人生成器池
        EnemySpawner enemySpawner = FindFirstObjectByType<EnemySpawner>();
        if (enemySpawner != null)
        {
            RegisterPool(new EnemySpawnerAdapter(enemySpawner));
            Debug.Log("[PoolManager] 注册了EnemySpawner对象池适配器");
        }
        
        // 注册残影池 - 暂时跳过，等待残影效果系统实现
        // SwordAfterimagePool功能尚未实现，暂时不注册
        
        Debug.Log($"[PoolManager] 发现并注册了 {monitorablePools.Count} 个对象池");
    }
    
    /// <summary>
    /// 注册池进行监控
    /// </summary>
    public void RegisterPool(IPoolMonitorable pool)
    {
        if (pool == null) return;
        
        if (!monitorablePools.Contains(pool))
        {
            monitorablePools.Add(pool);
            poolStats[pool.PoolName] = new PoolStats(pool.PoolName);
            
            if (enableDebugLog)
                Debug.Log($"[PoolManager] 注册对象池: {pool.PoolName}");
        }
    }
    
    /// <summary>
    /// 注销池监控
    /// </summary>
    public void UnregisterPool(IPoolMonitorable pool)
    {
        if (pool == null) return;
        
        monitorablePools.Remove(pool);
        poolStats.Remove(pool.PoolName);
        
        if (enableDebugLog)
            Debug.Log($"[PoolManager] 注销对象池: {pool.PoolName}");
    }
    
    /// <summary>
    /// 更新所有池统计
    /// </summary>
    private void UpdateAllPoolStats()
    {
        totalActiveObjects = 0;
        totalPooledObjects = 0;
        totalMemoryUsed = 0f;
        
        foreach (var pool in monitorablePools)
        {
            if (pool == null) continue;
            
            var stats = poolStats[pool.PoolName];
            stats.UpdateStats(
                pool.TotalCount,
                pool.ActiveCount,
                pool.AvailableCount,
                pool.EstimatedMemoryUsage
            );
            
            totalActiveObjects += pool.ActiveCount;
            totalPooledObjects += pool.TotalCount;
            totalMemoryUsed += pool.EstimatedMemoryUsage;
        }
    }
    
    /// <summary>
    /// 执行自动优化
    /// </summary>
    private void PerformAutoOptimization()
    {
        if (Time.time - lastOptimizationTime < 30f) // 最少30秒间隔
            return;
        
        foreach (var pool in monitorablePools)
        {
            if (pool == null || !pool.CanOptimize) continue;
            
            var stats = poolStats[pool.PoolName];
            
            // 检查是否需要优化
            if (stats.needsOptimization)
            {
                OptimizePool(pool, stats);
            }
        }
        
        lastOptimizationTime = Time.time;
    }
    
    /// <summary>
    /// 优化特定池
    /// </summary>
    private void OptimizePool(IPoolMonitorable pool, PoolStats stats)
    {
        int currentSize = pool.TotalCount;
        int newSize = currentSize;
        
        // 利用率过高，增加池大小
        if (stats.utilizationRate > poolUtilizationThreshold)
        {
            newSize = Mathf.Min(currentSize + 5, maxPoolSize);
        }
        // 利用率过低，减少池大小
        else if (stats.utilizationRate < 0.3f && currentSize > minPoolSize)
        {
            newSize = Mathf.Max(currentSize - 2, minPoolSize);
        }
        
        if (newSize != currentSize)
        {
            pool.OptimizePool(newSize);
            stats.optimizationCount++;
            
            if (enableDebugLog)
                Debug.Log($"[PoolManager] 优化池 {pool.PoolName}: {currentSize} -> {newSize} (利用率: {stats.utilizationRate:P})");
        }
    }
    
    /// <summary>
    /// 检查内存使用情况
    /// </summary>
    private void CheckMemoryUsage()
    {
        if (totalMemoryUsed > memoryWarningThreshold)
        {
            Debug.LogWarning($"[PoolManager] 内存使用警告: {totalMemoryUsed:F1}MB (阈值: {memoryWarningThreshold}MB)");
            
            // 触发内存清理
            if (enableAutoOptimization)
            {
                TriggerMemoryCleanup();
            }
        }
    }
    
    /// <summary>
    /// 触发内存清理
    /// </summary>
    private void TriggerMemoryCleanup()
    {
        Debug.Log("[PoolManager] 执行内存清理...");
        
        // 清理未使用的池对象
        foreach (var pool in monitorablePools)
        {
            if (pool == null) continue;
            
            var stats = poolStats[pool.PoolName];
            
            // 如果利用率很低，清理部分对象
            if (stats.utilizationRate < 0.2f)
            {
                int targetSize = Mathf.Max(stats.activeCount + 2, minPoolSize);
                pool.OptimizePool(targetSize);
            }
        }
        
        // 强制垃圾回收
        System.GC.Collect();
        
        Debug.Log("[PoolManager] 内存清理完成");
    }
    
    /// <summary>
    /// 记录池统计信息
    /// </summary>
    private void LogPoolStatistics()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== 对象池统计报告 ===");
        sb.AppendLine($"总活跃对象: {totalActiveObjects}");
        sb.AppendLine($"总池化对象: {totalPooledObjects}");
        sb.AppendLine($"总内存使用: {totalMemoryUsed:F1}MB");
        sb.AppendLine($"监控池数量: {monitorablePools.Count}");
        sb.AppendLine();
        
        foreach (var kvp in poolStats)
        {
            var stats = kvp.Value;
            sb.AppendLine($"{stats.poolName}:");
            sb.AppendLine($"  总数: {stats.totalCount}, 活跃: {stats.activeCount}, 可用: {stats.availableCount}");
            sb.AppendLine($"  利用率: {stats.utilizationRate:P}, 内存: {stats.memoryUsage:F1}MB");
            sb.AppendLine($"  优化次数: {stats.optimizationCount}, 需要优化: {stats.needsOptimization}");
            sb.AppendLine();
        }
        
        Debug.Log(sb.ToString());
    }
    
    /// <summary>
    /// 获取全局统计信息
    /// </summary>
    public string GetGlobalStats()
    {
        UpdateAllPoolStats();
        
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== 对象池全局统计 ===");
        sb.AppendLine($"总活跃对象: {totalActiveObjects}");
        sb.AppendLine($"总池化对象: {totalPooledObjects}");
        sb.AppendLine($"总内存使用: {totalMemoryUsed:F1}MB");
        sb.AppendLine($"监控间隔: {monitoringInterval}s");
        sb.AppendLine($"自动优化: {(enableAutoOptimization ? "启用" : "禁用")}");
        
        return sb.ToString();
    }
    
    /// <summary>
    /// 获取详细统计信息
    /// </summary>
    public string GetDetailedStats()
    {
        UpdateAllPoolStats();
        
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(GetGlobalStats());
        sb.AppendLine();
        
        foreach (var pool in monitorablePools)
        {
            if (pool == null) continue;
            
            sb.AppendLine($"=== {pool.PoolName} ===");
            sb.AppendLine(pool.GetDetailedStats());
            sb.AppendLine();
        }
        
        return sb.ToString();
    }
    
    /// <summary>
    /// 手动优化所有池
    /// </summary>
    public void OptimizeAllPools()
    {
        Debug.Log("[PoolManager] 手动优化所有对象池...");
        
        foreach (var pool in monitorablePools)
        {
            if (pool == null || !pool.CanOptimize) continue;
            
            var stats = poolStats[pool.PoolName];
            OptimizePool(pool, stats);
        }
        
        Debug.Log("[PoolManager] 所有池优化完成");
    }
    
    /// <summary>
    /// 清空所有池
    /// </summary>
    public void ClearAllPools()
    {
        Debug.Log("[PoolManager] 清空所有对象池...");
        
        foreach (var pool in monitorablePools)
        {
            if (pool == null) continue;
            
            pool.ClearPool();
        }
        
        Debug.Log("[PoolManager] 所有池已清空");
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    
    // 调试方法
    [ContextMenu("显示全局统计")]
    private void ShowGlobalStats()
    {
        Debug.Log(GetGlobalStats());
    }
    
    [ContextMenu("显示详细统计")]
    private void ShowDetailedStats()
    {
        Debug.Log(GetDetailedStats());
    }
    
    [ContextMenu("优化所有池")]
    private void OptimizeAllPoolsContext()
    {
        OptimizeAllPools();
    }
    
    [ContextMenu("清空所有池")]
    private void ClearAllPoolsContext()
    {
        ClearAllPools();
    }
    
    [ContextMenu("触发内存清理")]
    private void TriggerMemoryCleanupContext()
    {
        TriggerMemoryCleanup();
    }
}

// 适配器类，用于将现有的对象池适配到监控接口
public class DamageNumberPoolAdapter : IPoolMonitorable
{
    private DamageNumberPool pool;
    
    public DamageNumberPoolAdapter(DamageNumberPool pool)
    {
        this.pool = pool;
    }
    
    public string PoolName => "DamageNumberPool";
    public int TotalCount => pool.TotalCount;
    public int ActiveCount => pool.ActiveCount;
    public int AvailableCount => pool.AvailableCount;
    public float EstimatedMemoryUsage => TotalCount * 0.05f; // 估算每个对象50KB
    public bool CanOptimize => true;
    
    public void OptimizePool(int newSize)
    {
        // 对象池优化逻辑（可能需要扩展原始类）
        Debug.Log($"[DamageNumberPool] 优化建议: 目标大小={newSize}");
    }
    
    public void PreloadPool(int count)
    {
        pool.PreloadDamageNumbers(count);
    }
    
    public void ClearPool()
    {
        pool.ClearPool();
    }
    
    public string GetDetailedStats()
    {
        return pool.GetPoolStats();
    }
}

/// <summary>
/// 敌人生成器对象池适配器 - 将EnemySpawner适配到PoolManager系统
/// </summary>
public class EnemySpawnerAdapter : IPoolMonitorable
{
    private EnemySpawner enemySpawner;
    
    public EnemySpawnerAdapter(EnemySpawner spawner)
    {
        enemySpawner = spawner;
    }
    
    public string PoolName => "EnemyPool";
    public int TotalCount => enemySpawner.ActiveEnemyCount + enemySpawner.PoolSize;
    public int ActiveCount => enemySpawner.ActiveEnemyCount;
    public int AvailableCount => enemySpawner.PoolSize;
    public float EstimatedMemoryUsage => (TotalCount * 0.5f); // 估算每个敌人0.5MB
    public bool CanOptimize => AvailableCount > ActiveCount * 2; // 可用数量超过活跃数量2倍时可优化
    
    public void OptimizePool(int newSize)
    {
        // EnemySpawner没有直接的优化方法，暂时只记录日志
        Debug.Log($"[EnemySpawnerAdapter] 建议优化对象池大小到: {newSize}");
    }
    
    public void PreloadPool(int count)
    {
        // EnemySpawner在启动时已经预加载，这里只记录
        Debug.Log($"[EnemySpawnerAdapter] 预加载请求: {count} 个敌人");
    }
    
    public void ClearPool()
    {
        if (enemySpawner != null)
        {
            enemySpawner.ClearAllEnemies();
        }
    }
    
    public string GetDetailedStats()
    {
        if (enemySpawner == null) return "EnemySpawner不可用";
        
        return $"敌人池状态:\n" +
               $"- 活跃敌人: {ActiveCount}\n" +
               $"- 池中可用: {AvailableCount}\n" +
               $"- 总生成数: {enemySpawner.TotalSpawned}\n" +
               $"- 当前难度: {enemySpawner.CurrentDifficulty}";
    }
}

// SwordAfterimagePool暂时未实现 - 等待残影效果系统实现
