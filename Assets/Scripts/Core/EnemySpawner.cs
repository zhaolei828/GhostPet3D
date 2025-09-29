using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 智能敌人生成器 - 支持对象池、波次生成、性能优化
/// 现已支持多种敌人类型随机生成：SmartEnemy(近战敌人) 和 GhostEnemy(幽灵敌人)
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("生成设置")]
    [SerializeField] private GameObject[] enemyPrefabs; // 支持多种敌人类型：普通敌人和幽灵敌人
    [SerializeField] private GameObject enemyPrefab; // 保持向后兼容
    [SerializeField] private Transform spawnParent; // 敌人父对象，用于组织层级
    [SerializeField] private Transform player;
    
    [Header("生成区域")]
    // spawnRadius已移除，使用minDistanceFromPlayer和maxDistanceFromPlayer
    [SerializeField] private float minDistanceFromPlayer = 8f; // 距离玩家最小距离
    [SerializeField] private float maxDistanceFromPlayer = 20f; // 距离玩家最大距离
    
    [Header("生成控制")]
    [SerializeField] private int maxActiveEnemies = 20; // 最大活跃敌人数
    [SerializeField] private int initialEnemyCount = 5; // 初始敌人数量
    [SerializeField] private float spawnInterval = 3f; // 生成间隔
    [SerializeField] private int enemiesPerWave = 2; // 每波生成数量
    
    [Header("性能优化")]
    [SerializeField] private float despawnDistance = 30f; // 超出距离自动回收
    [SerializeField] private bool useObjectPool = true; // 是否使用对象池
    [SerializeField] private int poolSize = 50; // 对象池大小
    
    [Header("动态难度")]
    [SerializeField] private bool enableDynamicDifficulty = true;
    [SerializeField] private float difficultyIncreaseInterval = 30f; // 难度提升间隔(秒)
    [SerializeField] private int maxDifficultyLevel = 5; // 最大难度等级
    
    // 私有变量
    private List<GameObject> activeEnemies = new List<GameObject>();
    private Queue<GameObject> enemyPool = new Queue<GameObject>();
    private float lastSpawnTime;
    private float gameStartTime;
    private int currentDifficultyLevel = 1;
    
    // 生成统计
    private int totalSpawned = 0;
    private int totalRecycled = 0;
    
    // 单例
    public static EnemySpawner Instance { get; private set; }
    
    // 公共属性
    public int ActiveEnemyCount => activeEnemies.Count;
    public int PoolSize => enemyPool.Count;
    public int TotalSpawned => totalSpawned;
    public float CurrentDifficulty => currentDifficultyLevel;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        gameStartTime = Time.time;
        
        // 自动查找Player
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
        
        // 注意：敌人预制体配置现在在 Start() 方法中的 EnsureEnemyPrefabsConfiguration() 中处理
        
        // 创建敌人父对象
        if (spawnParent == null)
        {
            GameObject parentObj = new GameObject("Spawned Enemies");
            spawnParent = parentObj.transform;
        }
    }
    
    private void Start()
    {
        // 确保敌人预制体配置正确
        EnsureEnemyPrefabsConfiguration();
        
        if (useObjectPool)
        {
            InitializeObjectPool();
        }
        
        // 生成初始敌人
        StartCoroutine(SpawnInitialEnemies());
        
        // 开始持续生成
        StartCoroutine(ContinuousSpawning());
        
        // 开始性能管理
        StartCoroutine(PerformanceManagement());
        
        if (enableDynamicDifficulty)
        {
            StartCoroutine(DynamicDifficultyAdjustment());
        }
    }
    
    /// <summary>
    /// 确保敌人预制体配置正确
    /// </summary>
    private void EnsureEnemyPrefabsConfiguration()
    {
        // 如果敌人预制体数组为空，尝试从场景中查找
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            GameObject[] enemyTemplates = GameObject.FindGameObjectsWithTag("Enemy");
            if (enemyTemplates.Length > 0)
            {
                enemyPrefabs = new GameObject[enemyTemplates.Length];
                for (int i = 0; i < enemyTemplates.Length; i++)
                {
                    enemyPrefabs[i] = enemyTemplates[i];
                }
                Debug.Log($"[EnemySpawner] 自动配置：找到 {enemyTemplates.Length} 种敌人模板");
                foreach (var enemy in enemyTemplates)
                {
                    Debug.Log($"[EnemySpawner] - {enemy.name} (标签: {enemy.tag})");
                }
            }
            else if (enemyPrefab != null)
            {
                // 如果有单一敌人预制体，转换为数组
                enemyPrefabs = new GameObject[] { enemyPrefab };
                Debug.Log($"[EnemySpawner] 单一敌人模式: {enemyPrefab.name}");
            }
            else
            {
                Debug.LogError("[EnemySpawner] 未找到任何敌人模板！请在场景中添加标记为'Enemy'的游戏对象，或在Inspector中设置敌人预制体。");
            }
        }
    }
    
    /// <summary>
    /// 初始化对象池
    /// </summary>
    private void InitializeObjectPool()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogError("[EnemySpawner] 敌人预制件未设置！");
            return;
        }
        
        // 为每种敌人类型平均分配对象池空间
        int poolPerType = poolSize / enemyPrefabs.Length;
        int remainder = poolSize % enemyPrefabs.Length;
        
        for (int typeIndex = 0; typeIndex < enemyPrefabs.Length; typeIndex++)
        {
            if (enemyPrefabs[typeIndex] == null) continue;
            
            int currentTypePool = poolPerType + (typeIndex < remainder ? 1 : 0);
            
            for (int i = 0; i < currentTypePool; i++)
            {
                GameObject enemy = Instantiate(enemyPrefabs[typeIndex], spawnParent);
                enemy.SetActive(false);
                enemyPool.Enqueue(enemy);
            }
        }
        
        Debug.Log($"[EnemySpawner] 对象池初始化完成，预创建 {poolSize} 个敌人对象 ({enemyPrefabs.Length} 种类型)");
    }
    
    /// <summary>
    /// 生成初始敌人
    /// </summary>
    private IEnumerator SpawnInitialEnemies()
    {
        for (int i = 0; i < initialEnemyCount; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(0.1f); // 避免同时生成导致卡顿
        }
        
        Debug.Log($"[EnemySpawner] 初始敌人生成完成，共 {initialEnemyCount} 个");
    }
    
    /// <summary>
    /// 持续生成敌人
    /// </summary>
    private IEnumerator ContinuousSpawning()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            
            if (activeEnemies.Count < maxActiveEnemies && player != null)
            {
                int enemiesToSpawn = Mathf.Min(enemiesPerWave, maxActiveEnemies - activeEnemies.Count);
                
                for (int i = 0; i < enemiesToSpawn; i++)
                {
                    SpawnEnemy();
                    yield return new WaitForSeconds(0.2f); // 分批生成
                }
            }
        }
    }
    
    /// <summary>
    /// 生成单个敌人
    /// </summary>
    private void SpawnEnemy()
    {
        if (player == null) return;
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;
        
        Vector3 spawnPosition = GetRandomSpawnPosition();
        if (spawnPosition == Vector3.zero) return; // 未找到合适位置
        
        // 随机选择敌人类型
        GameObject selectedPrefab = GetRandomEnemyPrefab();
        GameObject enemy = GetEnemyFromPool(selectedPrefab);
        if (enemy == null) return;
        
        // 设置位置和状态
        enemy.transform.position = spawnPosition;
        enemy.transform.rotation = Quaternion.identity;
        enemy.SetActive(true);
        
        // 重置敌人状态
        ResetEnemyState(enemy);
        
        activeEnemies.Add(enemy);
        totalSpawned++;
        lastSpawnTime = Time.time;
        
        Debug.Log($"[EnemySpawner] 生成敌人 #{totalSpawned} ({enemy.name}) 在位置 {spawnPosition}");
    }
    
    /// <summary>
    /// 从对象池获取敌人（重载方法支持特定预制体）
    /// </summary>
    private GameObject GetEnemyFromPool(GameObject preferredPrefab = null)
    {
        if (useObjectPool && enemyPool.Count > 0)
        {
            return enemyPool.Dequeue();
        }
        else if (preferredPrefab != null)
        {
            // 对象池用完了，直接创建新的
            return Instantiate(preferredPrefab, spawnParent);
        }
        else if (enemyPrefabs != null && enemyPrefabs.Length > 0)
        {
            // 随机选择一个预制体
            GameObject randomPrefab = GetRandomEnemyPrefab();
            return Instantiate(randomPrefab, spawnParent);
        }
        
        return null;
    }
    
    /// <summary>
    /// 随机选择敌人预制体
    /// </summary>
    private GameObject GetRandomEnemyPrefab()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return null;
        
        List<GameObject> validPrefabs = new List<GameObject>();
        foreach (var prefab in enemyPrefabs)
        {
            if (prefab != null) validPrefabs.Add(prefab);
        }
        
        if (validPrefabs.Count == 0) return null;
        
        int randomIndex = Random.Range(0, validPrefabs.Count);
        return validPrefabs[randomIndex];
    }
    
    /// <summary>
    /// 重置敌人状态
    /// </summary>
    private void ResetEnemyState(GameObject enemy)
    {
        // 重置血量
        HealthSystem health = enemy.GetComponent<HealthSystem>();
        if (health != null)
        {
            health.ResetToFull();
        }
        
        // 重置AI状态
        Enemy3DAI ai = enemy.GetComponent<Enemy3DAI>();
        if (ai != null)
        {
            ai.ResetAI();
        }
        
        // 重置NavMeshAgent
        UnityEngine.AI.NavMeshAgent agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
            agent.enabled = true; // 重新启用以重置状态
        }
    }
    
    /// <summary>
    /// 获取随机生成位置
    /// </summary>
    private Vector3 GetRandomSpawnPosition()
    {
        if (player == null) return Vector3.zero;
        
        for (int attempts = 0; attempts < 20; attempts++) // 最多尝试20次
        {
            // 在环形区域内随机选择角度
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);
            
            Vector3 direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            Vector3 spawnPosition = player.position + direction * distance;
            
            // 确保在地面上
            spawnPosition.y = 0.5f; // 敌人高度
            
            // 检查是否在NavMesh上
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(spawnPosition, out hit, 2f, UnityEngine.AI.NavMesh.AllAreas))
            {
                return hit.position;
            }
        }
        
        Debug.LogWarning("[EnemySpawner] 未找到合适的生成位置");
        return Vector3.zero;
    }
    
    /// <summary>
    /// 回收敌人到对象池
    /// </summary>
    public void RecycleEnemy(GameObject enemy)
    {
        if (enemy == null) return;
        
        activeEnemies.Remove(enemy);
        totalRecycled++;
        
        if (useObjectPool)
        {
            enemy.SetActive(false);
            enemyPool.Enqueue(enemy);
        }
        else
        {
            Destroy(enemy);
        }
        
        Debug.Log($"[EnemySpawner] 回收敌人，活跃数量: {activeEnemies.Count}");
        
        // 敌人死亡后，如果活跃敌人数量低于最大值，立即尝试生成新敌人
        if (activeEnemies.Count < maxActiveEnemies)
        {
            StartCoroutine(SpawnEnemyWithDelay(0.5f)); // 延迟0.5秒生成，避免同时生成太多
        }
    }
    
    /// <summary>
    /// 延迟生成敌人
    /// </summary>
    private IEnumerator SpawnEnemyWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (activeEnemies.Count < maxActiveEnemies)
        {
            SpawnEnemy();
        }
    }
    
    /// <summary>
    /// 性能管理协程
    /// </summary>
    private IEnumerator PerformanceManagement()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f); // 每2秒检查一次
            
            if (player == null) continue;
            
            // 检查距离过远的敌人并回收
            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                if (activeEnemies[i] == null)
                {
                    activeEnemies.RemoveAt(i);
                    continue;
                }
                
                float distance = Vector3.Distance(activeEnemies[i].transform.position, player.position);
                if (distance > despawnDistance)
                {
                    RecycleEnemy(activeEnemies[i]);
                }
            }
        }
    }
    
    /// <summary>
    /// 动态难度调整
    /// </summary>
    private IEnumerator DynamicDifficultyAdjustment()
    {
        while (true)
        {
            yield return new WaitForSeconds(difficultyIncreaseInterval);
            
            if (currentDifficultyLevel < maxDifficultyLevel)
            {
                currentDifficultyLevel++;
                
                // 增加难度：更多敌人，更快生成
                maxActiveEnemies = Mathf.Min(maxActiveEnemies + 3, 50);
                enemiesPerWave = Mathf.Min(enemiesPerWave + 1, 5);
                spawnInterval = Mathf.Max(spawnInterval - 0.3f, 1f);
                
                Debug.Log($"[EnemySpawner] 难度提升到等级 {currentDifficultyLevel}！最大敌人: {maxActiveEnemies}, 生成间隔: {spawnInterval}s");
            }
        }
    }
    
    /// <summary>
    /// 清除所有敌人
    /// </summary>
    public void ClearAllEnemies()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] != null)
            {
                RecycleEnemy(activeEnemies[i]);
            }
        }
        
        Debug.Log("[EnemySpawner] 清除所有敌人");
    }
    
    /// <summary>
    /// 获取生成统计信息
    /// </summary>
    public string GetSpawnStats()
    {
        return $"活跃敌人: {activeEnemies.Count}/{maxActiveEnemies} | 总生成: {totalSpawned} | 总回收: {totalRecycled} | 难度: {currentDifficultyLevel} | 对象池: {enemyPool.Count}";
    }
    
    // 调试可视化
    private void OnDrawGizmosSelected()
    {
        if (player == null) return;
        
        // 绘制生成区域 - 使用DrawWireSphere替代DrawWireCircle 
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(player.position, minDistanceFromPlayer);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.position, maxDistanceFromPlayer);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(player.position, despawnDistance);
    }
}
