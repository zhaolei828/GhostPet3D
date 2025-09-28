using UnityEngine;
using System.Collections.Generic;

public class EnvironmentDecorator : MonoBehaviour
{
    [Header("装饰物设置")]
    [SerializeField] private int grassCount = 10;
    [SerializeField] private int blackRockCount = 6;
    [SerializeField] private int grayRockCount = 4;
    
    [Header("分布区域")]
    [SerializeField] private Vector2 groundSize = new Vector2(18f, 18f); // 略小于Ground(20x20)
    [SerializeField] private float centerExclusionRadius = 3f; // 中心区域不放装饰物
    
    [Header("装饰物尺寸随机化")]
    [SerializeField] private Vector2 grassScaleRange = new Vector2(0.6f, 1f);
    [SerializeField] private Vector2 grassHeightRange = new Vector2(0.2f, 0.4f);
    [SerializeField] private Vector2 rockScaleRange = new Vector2(0.4f, 0.8f);
    [SerializeField] private Vector2 rockHeightRange = new Vector2(0.15f, 0.35f);
    
    [Header("材质引用")]
    [SerializeField] private Material grassMaterial;
    [SerializeField] private Material blackRockMaterial;
    [SerializeField] private Material grayRockMaterial;
    
    private List<GameObject> decorationObjects = new List<GameObject>();
    
    private void Start()
    {
        LoadMaterials();
        GenerateDecorations();
    }
    
    private void LoadMaterials()
    {
        if (grassMaterial == null)
            grassMaterial = Resources.Load<Material>("Environment/SnowGrassMaterial");
        if (blackRockMaterial == null)
            blackRockMaterial = Resources.Load<Material>("Environment/BlackRockMaterial");
        if (grayRockMaterial == null)
            grayRockMaterial = Resources.Load<Material>("Environment/GrayRockMaterial");
            
        Debug.Log($"[EnvironmentDecorator] 材质加载: 草地={grassMaterial != null}, 黑岩={blackRockMaterial != null}, 灰岩={grayRockMaterial != null}");
    }
    
    public void GenerateDecorations()
    {
        ClearDecorations();
        
        // 生成草丛
        for (int i = 0; i < grassCount; i++)
        {
            CreateGrassDecoration();
        }
        
        // 生成黑色岩石
        for (int i = 0; i < blackRockCount; i++)
        {
            CreateRockDecoration(blackRockMaterial, "BlackRock");
        }
        
        // 生成灰色岩石
        for (int i = 0; i < grayRockCount; i++)
        {
            CreateRockDecoration(grayRockMaterial, "GrayRock");
        }
        
        Debug.Log($"[EnvironmentDecorator] 生成装饰物完成: 草丛{grassCount}, 黑岩{blackRockCount}, 灰岩{grayRockCount}");
    }
    
    private void CreateGrassDecoration()
    {
        Vector3 position = GetValidRandomPosition();
        if (position == Vector3.zero) return;
        
        GameObject grass = GameObject.CreatePrimitive(PrimitiveType.Cube);
        grass.name = $"SnowGrass_{decorationObjects.Count}";
        
        // 设置位置和随机化
        grass.transform.position = position;
        float scaleXZ = Random.Range(grassScaleRange.x, grassScaleRange.y);
        float scaleY = Random.Range(grassHeightRange.x, grassHeightRange.y);
        grass.transform.localScale = new Vector3(scaleXZ, scaleY, scaleXZ);
        grass.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        
        // 应用材质
        if (grassMaterial != null)
        {
            MeshRenderer renderer = grass.GetComponent<MeshRenderer>();
            renderer.material = grassMaterial;
        }
        
        // 移除碰撞体，避免影响游戏
        Destroy(grass.GetComponent<BoxCollider>());
        
        decorationObjects.Add(grass);
    }
    
    private void CreateRockDecoration(Material material, string namePrefix)
    {
        Vector3 position = GetValidRandomPosition();
        if (position == Vector3.zero) return;
        
        GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rock.name = $"{namePrefix}_{decorationObjects.Count}";
        
        // 设置位置和随机化
        rock.transform.position = position;
        float scaleXZ = Random.Range(rockScaleRange.x, rockScaleRange.y);
        float scaleY = Random.Range(rockHeightRange.x, rockHeightRange.y);
        rock.transform.localScale = new Vector3(scaleXZ, scaleY, scaleXZ);
        rock.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), Random.Range(-10, 10));
        
        // 应用材质
        if (material != null)
        {
            MeshRenderer renderer = rock.GetComponent<MeshRenderer>();
            renderer.material = material;
        }
        
        // 保留碰撞体，但设置为非触发器（装饰性障碍物）
        BoxCollider collider = rock.GetComponent<BoxCollider>();
        collider.isTrigger = false;
        
        decorationObjects.Add(rock);
    }
    
    private Vector3 GetValidRandomPosition()
    {
        int maxAttempts = 30; // 避免无限循环
        
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float x = Random.Range(-groundSize.x / 2, groundSize.x / 2);
            float z = Random.Range(-groundSize.y / 2, groundSize.y / 2);
            Vector3 position = new Vector3(x, 0.1f, z);
            
            // 检查是否在中心排除区域内
            if (Vector3.Distance(Vector2.zero, new Vector2(x, z)) < centerExclusionRadius)
            {
                continue;
            }
            
            // 检查是否与现有装饰物重叠
            bool tooClose = false;
            foreach (GameObject existing in decorationObjects)
            {
                if (existing != null && Vector3.Distance(position, existing.transform.position) < 1.5f)
                {
                    tooClose = true;
                    break;
                }
            }
            
            if (!tooClose)
            {
                return position;
            }
        }
        
        Debug.LogWarning("[EnvironmentDecorator] 无法找到有效位置放置装饰物");
        return Vector3.zero;
    }
    
    public void ClearDecorations()
    {
        foreach (GameObject obj in decorationObjects)
        {
            if (obj != null)
            {
                DestroyImmediate(obj);
            }
        }
        decorationObjects.Clear();
        Debug.Log("[EnvironmentDecorator] 清除所有装饰物");
    }
    
    [ContextMenu("重新生成装饰物")]
    public void RegenerateDecorations()
    {
        GenerateDecorations();
    }
    
    private void OnDrawGizmosSelected()
    {
        // 绘制地面区域
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(groundSize.x, 0.1f, groundSize.y));
        
        // 绘制中心排除区域
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, centerExclusionRadius);
    }
}
