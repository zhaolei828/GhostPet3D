using UnityEngine;
using System.Collections;

/// <summary>
/// 玩家出生点和重生管理器
/// </summary>
public class PlayerSpawnManager : MonoBehaviour
{
    [Header("出生点设置")]
    [SerializeField] private Transform spawnPoint; // 出生点位置
    [SerializeField] private float respawnDelay = 3f; // 重生延迟时间
    
    [Header("玩家引用")]
    [SerializeField] private Transform player;
    [SerializeField] private Player3DController playerController;
    [SerializeField] private HealthSystem playerHealth;
    
    // 单例
    public static PlayerSpawnManager Instance { get; private set; }
    
    // 属性
    public Vector3 SpawnPosition => spawnPoint != null ? spawnPoint.position : Vector3.zero;
    public bool IsRespawning { get; private set; } = false;
    
    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // 自动查找玩家
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                playerController = playerObj.GetComponent<Player3DController>();
                playerHealth = playerObj.GetComponent<HealthSystem>();
                Debug.Log($"[PlayerSpawnManager] 自动找到玩家: {playerObj.name}, HealthSystem: {playerHealth != null}");
            }
        }
        
        // 如果没有设置出生点，使用当前位置作为出生点
        if (spawnPoint == null)
        {
            spawnPoint = new GameObject("SpawnPoint").transform;
            spawnPoint.position = player != null ? player.position : Vector3.zero;
            spawnPoint.SetParent(transform);
        }
    }
    
    private void Start()
    {
        // 订阅玩家死亡事件
        if (playerHealth != null)
        {
            playerHealth.OnDeath += OnPlayerDeath;
        }
        
        // 设置初始出生点
        if (player != null && spawnPoint != null)
        {
            MovePlayerToSpawnPoint(false); // 不播放效果的传送
        }
    }
    
    /// <summary>
    /// 玩家死亡处理
    /// </summary>
    private void OnPlayerDeath()
    {
        if (IsRespawning) return; // 避免重复重生
        
        Debug.Log("[PlayerSpawnManager] 玩家死亡，开始重生流程");
        StartCoroutine(RespawnPlayer());
    }
    
    /// <summary>
    /// 重生玩家
    /// </summary>
    private IEnumerator RespawnPlayer()
    {
        IsRespawning = true;
        
        // 禁用玩家控制
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        Debug.Log($"[PlayerSpawnManager] 玩家将在 {respawnDelay} 秒后重生");
        
        // 等待重生延迟
        yield return new WaitForSeconds(respawnDelay);
        
        // 重生玩家
        RespawnPlayerNow();
        
        IsRespawning = false;
    }
    
    /// <summary>
    /// 立即重生玩家
    /// </summary>
    public void RespawnPlayerNow()
    {
        if (player == null || playerHealth == null) return;
        
        // 移动到出生点
        MovePlayerToSpawnPoint(true);
        
        // 恢复满血
        playerHealth.ResetToFull();
        
        // 重新启用玩家控制
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        
        Debug.Log("[PlayerSpawnManager] 玩家已重生");
    }
    
    /// <summary>
    /// 移动玩家到出生点
    /// </summary>
    /// <param name="showEffect">是否显示传送效果</param>
    public void MovePlayerToSpawnPoint(bool showEffect = true)
    {
        if (player == null || spawnPoint == null) return;
        
        // 移动玩家
        player.position = spawnPoint.position;
        
        // 重置玩家旋转
        player.rotation = spawnPoint.rotation;
        
        if (showEffect)
        {
            Debug.Log("[PlayerSpawnManager] 玩家传送到出生点");
            // 这里可以添加传送特效
        }
    }
    
    /// <summary>
    /// 设置新的出生点
    /// </summary>
    /// <param name="newSpawnPoint">新的出生点位置</param>
    public void SetSpawnPoint(Vector3 newSpawnPoint)
    {
        if (spawnPoint != null)
        {
            spawnPoint.position = newSpawnPoint;
            Debug.Log($"[PlayerSpawnManager] 出生点已设置为: {newSpawnPoint}");
        }
    }
    
    /// <summary>
    /// 设置新的出生点（使用Transform）
    /// </summary>
    /// <param name="newSpawnTransform">新的出生点Transform</param>
    public void SetSpawnPoint(Transform newSpawnTransform)
    {
        if (newSpawnTransform != null)
        {
            spawnPoint = newSpawnTransform;
            Debug.Log($"[PlayerSpawnManager] 出生点已设置为: {newSpawnTransform.position}");
        }
    }
    
    private void OnDestroy()
    {
        // 取消订阅事件
        if (playerHealth != null)
        {
            playerHealth.OnDeath -= OnPlayerDeath;
        }
    }
    
    // 调试可视化
    private void OnDrawGizmosSelected()
    {
        if (spawnPoint != null)
        {
            // 绘制出生点
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(spawnPoint.position, 0.5f);
            
            // 绘制方向指示
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(spawnPoint.position, spawnPoint.forward * 2f);
        }
    }
}
