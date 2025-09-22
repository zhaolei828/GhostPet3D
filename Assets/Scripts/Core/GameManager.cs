using UnityEngine;

/// <summary>
/// 3D游戏管理器 - 简化版本用于解决编译依赖
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("游戏设置")]
    [SerializeField] private Vector3 spawnPoint = new Vector3(0, 1, 0);
    [SerializeField] private float respawnDelay = 2f;
    
    // 单例实例
    public static GameManager Instance { get; private set; }
    
    // 玩家引用
    private Player3DController player;
    
    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // 查找玩家
        player = FindFirstObjectByType<Player3DController>();
    }
    
    private void Start()
    {
        Debug.Log("[GameManager] 3D游戏管理器已启动");
    }
    
    /// <summary>
    /// 重生玩家
    /// </summary>
    public void RespawnPlayer()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<Player3DController>();
        }
        
        if (player != null)
        {
            Invoke(nameof(PerformRespawn), respawnDelay);
        }
    }
    
    private void PerformRespawn()
    {
        if (player != null)
        {
            player.Respawn(spawnPoint);
            Debug.Log("[GameManager] 玩家已重生");
        }
    }
    
    /// <summary>
    /// 设置重生点
    /// </summary>
    public void SetSpawnPoint(Vector3 newSpawnPoint)
    {
        spawnPoint = newSpawnPoint;
        Debug.Log($"[GameManager] 重生点已设置为: {spawnPoint}");
    }
    
    /// <summary>
    /// 游戏结束
    /// </summary>
    public void GameOver()
    {
        Debug.Log("[GameManager] 游戏结束！");
        // TODO: 实现游戏结束逻辑
    }
}
