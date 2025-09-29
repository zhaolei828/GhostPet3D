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
        // 加载材质
        Material grassMaterial = Resources.Load<Material>("Environment/SnowGrassMaterial");
        Material blackRockMaterial = Resources.Load<Material>("Environment/BlackRockMaterial");
        Material grayRockMaterial = Resources.Load<Material>("Environment/GrayRockMaterial");
        
        Debug.Log($"[GameManager] 环境材质加载: 草={grassMaterial != null}, 黑岩={blackRockMaterial != null}, 灰岩={grayRockMaterial != null}");
        
        // 创建草丛装饰 (8个) - 随机位置分布
        for (int i = 0; i < 8; i++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(-8f, 8f),    // X轴随机范围 -8到8米
                0.08f,                    // Y轴固定高度
                Random.Range(-8f, 8f)     // Z轴随机范围 -8到8米  
            );
            CreateGrassDecoration(randomPos, grassMaterial);
        }
        
        // 创建岩石装饰 (6个) - 随机位置和材质分布
        Material[] rockMaterials = { blackRockMaterial, grayRockMaterial };
        for (int i = 0; i < 6; i++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(-8f, 8f),    // X轴随机范围 -8到8米
                0.1f,                     // Y轴固定高度
                Random.Range(-8f, 8f)     // Z轴随机范围 -8到8米
            );
            
            // 随机选择岩石材质
            Material randomRockMaterial = rockMaterials[Random.Range(0, rockMaterials.Length)];
            string rockName = (randomRockMaterial == blackRockMaterial) ? "BlackRock" : "GrayRock";
            
            CreateRockDecoration(randomPos, randomRockMaterial, rockName);
        }
        
        Debug.Log("[GameManager] 环境装饰物创建完成: 8个草丛, 6个岩石");
    }
    
    private void CreateGrassDecoration(Vector3 position, Material material)
    {
        GameObject grass = GameObject.CreatePrimitive(PrimitiveType.Cube);
        grass.name = "SnowGrass";
        
        // 添加高度随机变化，让草丛高低错落更自然
        Vector3 adjustedPosition = position;
        adjustedPosition.y += Random.Range(-0.02f, 0.02f); // ±2cm的高度变化
        grass.transform.position = adjustedPosition;
        
        // 尺寸随机变化，更有生机
        float scaleVariation = Random.Range(0.85f, 1.15f);
        grass.transform.localScale = new Vector3(0.8f * scaleVariation, 0.3f * scaleVariation, 0.8f * scaleVariation);
        grass.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        
        if (material != null)
        {
            grass.GetComponent<MeshRenderer>().material = material;
        }
        
        // 移除碰撞体，避免影响游戏
        Destroy(grass.GetComponent<BoxCollider>());
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
