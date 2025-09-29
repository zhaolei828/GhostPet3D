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
        // 单例模式（场景级别，不跨场景保持）
        if (Instance == null)
        {
            Instance = this;
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
        CreateEnvironmentDecorations();
    }
    
    /// <summary>
    /// 创建环境装饰物
    /// </summary>
    private void CreateEnvironmentDecorations()
    {
        // 加载沙地主题材质
        Material cactusMaterial = Resources.Load<Material>("Materials/CactusMaterial");
        if (cactusMaterial == null) cactusMaterial = Resources.Load<Material>("CactusMaterial");
        
        Material sandstoneRockMaterial = Resources.Load<Material>("Materials/SandstoneRockMaterial");
        if (sandstoneRockMaterial == null) sandstoneRockMaterial = Resources.Load<Material>("SandstoneRockMaterial");
        
        Material blackRockMaterial = Resources.Load<Material>("Environment/BlackRockMaterial");
        
        Debug.Log($"[GameManager] 沙地环境材质加载: 仙人掌={cactusMaterial != null}, 沙岩={sandstoneRockMaterial != null}, 黑岩={blackRockMaterial != null}");
        
        // 创建仙人掌装饰 (6个) - 随机位置分布，沙地植物较少
        for (int i = 0; i < 6; i++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(-8f, 8f),    // X轴随机范围 -8到8米
                0.12f,                    // Y轴稍高一些，仙人掌比草高
                Random.Range(-8f, 8f)     // Z轴随机范围 -8到8米  
            );
            CreateCactusDecoration(randomPos, cactusMaterial);
        }
        
        // 创建岩石装饰 (8个) - 沙地中更多岩石，随机位置和材质分布
        Material[] rockMaterials = { sandstoneRockMaterial, blackRockMaterial };
        for (int i = 0; i < 8; i++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(-8f, 8f),    // X轴随机范围 -8到8米
                0.1f,                     // Y轴固定高度
                Random.Range(-8f, 8f)     // Z轴随机范围 -8到8米
            );
            
            // 随机选择岩石材质，偏向沙岩(70%概率)
            Material randomRockMaterial = (Random.Range(0f, 1f) < 0.7f) ? sandstoneRockMaterial : blackRockMaterial;
            string rockName = (randomRockMaterial == sandstoneRockMaterial) ? "SandstoneRock" : "BlackRock";
            
            CreateRockDecoration(randomPos, randomRockMaterial, rockName);
        }
        
        Debug.Log("[GameManager] 沙地环境装饰物创建完成: 6个仙人掌, 8个岩石");
    }
    
    private void CreateCactusDecoration(Vector3 position, Material material)
    {
        GameObject cactus = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cactus.name = "Cactus";
        
        // 添加高度随机变化，让仙人掌高低错落更自然
        Vector3 adjustedPosition = position;
        adjustedPosition.y += Random.Range(-0.03f, 0.03f); // ±3cm的高度变化
        cactus.transform.position = adjustedPosition;
        
        // 仙人掌比草丛更高更窄，尺寸随机变化
        float scaleVariation = Random.Range(0.8f, 1.3f);
        float heightVariation = Random.Range(1.2f, 2.0f); // 仙人掌更高
        cactus.transform.localScale = new Vector3(0.4f * scaleVariation, 0.6f * heightVariation, 0.4f * scaleVariation);
        cactus.transform.rotation = Quaternion.Euler(Random.Range(-5, 5), Random.Range(0, 360), Random.Range(-5, 5)); // 轻微倾斜
        
        if (material != null)
        {
            cactus.GetComponent<MeshRenderer>().material = material;
        }
        
        // 移除碰撞体，避免影响游戏
        Destroy(cactus.GetComponent<BoxCollider>());
    }
    
    private void CreateRockDecoration(Vector3 position, Material material, string name)
    {
        GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rock.name = name;
        
        // 添加位置随机变化，让岩石深埋程度不同
        Vector3 adjustedPosition = position;
        adjustedPosition.y += Random.Range(-0.03f, 0.02f); // 有些岩石更深埋在雪中
        rock.transform.position = adjustedPosition;
        
        // 尺寸随机变化，大小不一的岩石更真实
        float scaleVariation = Random.Range(0.8f, 1.3f);
        rock.transform.localScale = new Vector3(0.6f * scaleVariation, 0.25f * scaleVariation, 0.6f * scaleVariation);
        rock.transform.rotation = Quaternion.Euler(Random.Range(-10, 10), Random.Range(0, 360), Random.Range(-10, 10));
        
        if (material != null)
        {
            rock.GetComponent<MeshRenderer>().material = material;
        }
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
