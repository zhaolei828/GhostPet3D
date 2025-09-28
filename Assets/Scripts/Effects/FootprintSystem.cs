using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 脚印系统 - 在雪地上创建玩家和敌人的足迹效果
/// </summary>
public class FootprintSystem : MonoBehaviour
{
    [Header("脚印设置")]
    [SerializeField] private GameObject footprintPrefab;
    [SerializeField] private float footprintLifetime = 30f;
    [SerializeField] private float minFootprintDistance = 1.5f; // 两个脚印之间的最小距离
    [SerializeField] private LayerMask groundLayer = 1; // 地面层
    
    [Header("脚印外观")]
    [SerializeField] private Vector3 footprintScale = new Vector3(0.3f, 0.01f, 0.5f);
    [SerializeField] private Color playerFootprintColor = new Color(0.7f, 0.7f, 0.8f, 0.8f);
    [SerializeField] private Color enemyFootprintColor = new Color(0.8f, 0.6f, 0.6f, 0.7f);
    
    [Header("性能优化")]
    [SerializeField] private int maxFootprints = 100;
    [SerializeField] private float footprintFadeTime = 5f; // 脚印淡出时间
    
    // 脚印对象池
    private Queue<FootprintData> footprintPool = new Queue<FootprintData>();
    private List<FootprintData> activeFootprints = new List<FootprintData>();
    
    // 跟踪对象
    private Dictionary<Transform, Vector3> trackedObjects = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, FootprintType> objectTypes = new Dictionary<Transform, FootprintType>();
    
    public enum FootprintType
    {
        Player,
        Enemy
    }
    
    [System.Serializable]
    private class FootprintData
    {
        public GameObject footprintObject;
        public float creationTime;
        public FootprintType type;
        public Vector3 position;
        public bool isActive;
        
        public FootprintData(GameObject obj, FootprintType footprintType, Vector3 pos)
        {
            footprintObject = obj;
            type = footprintType;
            position = pos;
            creationTime = Time.time;
            isActive = true;
        }
    }
    
    private static FootprintSystem instance;
    public static FootprintSystem Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<FootprintSystem>();
            }
            return instance;
        }
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        InitializeFootprintSystem();
    }
    
    private void Start()
    {
        // 自动注册玩家和敌人
        RegisterPlayer();
        RegisterEnemies();
    }
    
    private void InitializeFootprintSystem()
    {
        // 如果没有脚印预制件，创建一个简单的
        if (footprintPrefab == null)
        {
            CreateDefaultFootprintPrefab();
        }
        
        // 预创建脚印对象池
        for (int i = 0; i < maxFootprints; i++)
        {
            CreateFootprintObject();
        }
        
        Debug.Log($"[FootprintSystem] 脚印系统初始化完成，创建了 {maxFootprints} 个脚印对象");
    }
    
    private void CreateDefaultFootprintPrefab()
    {
        footprintPrefab = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        footprintPrefab.name = "FootprintPrefab";
        
        // 移除碰撞器，脚印不需要物理
        DestroyImmediate(footprintPrefab.GetComponent<Collider>());
        
        // 设置材质
        MeshRenderer renderer = footprintPrefab.GetComponent<MeshRenderer>();
        Material footprintMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        footprintMaterial.color = new Color(0.5f, 0.5f, 0.6f, 0.8f);
        footprintMaterial.SetFloat("_Metallic", 0);
        footprintMaterial.SetFloat("_Smoothness", 0.1f);
        renderer.material = footprintMaterial;
        
        // 缩放和变形
        footprintPrefab.transform.localScale = footprintScale;
        
        // 设置为不活跃，作为预制件使用
        footprintPrefab.SetActive(false);
        
        Debug.Log("[FootprintSystem] 创建了默认脚印预制件");
    }
    
    private void CreateFootprintObject()
    {
        if (footprintPrefab == null) return;
        
        GameObject footprint = Instantiate(footprintPrefab, transform);
        footprint.SetActive(false);
        
        FootprintData footprintData = new FootprintData(footprint, FootprintType.Player, Vector3.zero);
        footprintData.isActive = false;
        footprintPool.Enqueue(footprintData);
    }
    
    private void RegisterPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            RegisterObject(player.transform, FootprintType.Player);
            Debug.Log("[FootprintSystem] 注册了玩家脚印跟踪");
        }
    }
    
    private void RegisterEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            RegisterObject(enemy.transform, FootprintType.Enemy);
        }
        Debug.Log($"[FootprintSystem] 注册了 {enemies.Length} 个敌人的脚印跟踪");
    }
    
    /// <summary>
    /// 注册需要留下脚印的对象
    /// </summary>
    public void RegisterObject(Transform obj, FootprintType type)
    {
        if (obj != null)
        {
            trackedObjects[obj] = obj.position;
            objectTypes[obj] = type;
        }
    }
    
    /// <summary>
    /// 取消注册对象
    /// </summary>
    public void UnregisterObject(Transform obj)
    {
        if (trackedObjects.ContainsKey(obj))
        {
            trackedObjects.Remove(obj);
            objectTypes.Remove(obj);
        }
    }
    
    private void Update()
    {
        UpdateTrackedObjects();
        UpdateFootprintLifetime();
        CleanupExpiredFootprints();
    }
    
    private void UpdateTrackedObjects()
    {
        List<Transform> toRemove = new List<Transform>();
        
        foreach (var kvp in trackedObjects)
        {
            Transform obj = kvp.Key;
            Vector3 lastPos = kvp.Value;
            
            if (obj == null)
            {
                toRemove.Add(obj);
                continue;
            }
            
            Vector3 currentPos = obj.position;
            float distance = Vector3.Distance(currentPos, lastPos);
            
            // 如果移动距离足够远，创建脚印
            if (distance >= minFootprintDistance)
            {
                CreateFootprint(lastPos, objectTypes[obj]);
                trackedObjects[obj] = currentPos;
            }
        }
        
        // 清理无效对象
        foreach (Transform obj in toRemove)
        {
            UnregisterObject(obj);
        }
    }
    
    private void CreateFootprint(Vector3 position, FootprintType type)
    {
        // 检测地面
        if (Physics.Raycast(position + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 5f, groundLayer))
        {
            Vector3 footprintPos = hit.point + Vector3.up * 0.01f; // 略微高于地面
            
            FootprintData footprintData = GetPooledFootprint();
            if (footprintData != null)
            {
                SetupFootprint(footprintData, footprintPos, type);
                activeFootprints.Add(footprintData);
            }
        }
    }
    
    private FootprintData GetPooledFootprint()
    {
        // 从对象池获取脚印
        if (footprintPool.Count > 0)
        {
            return footprintPool.Dequeue();
        }
        
        // 如果池子空了，回收最老的脚印
        if (activeFootprints.Count > 0)
        {
            FootprintData oldest = activeFootprints[0];
            activeFootprints.RemoveAt(0);
            return oldest;
        }
        
        return null;
    }
    
    private void SetupFootprint(FootprintData footprintData, Vector3 position, FootprintType type)
    {
        footprintData.position = position;
        footprintData.type = type;
        footprintData.creationTime = Time.time;
        footprintData.isActive = true;
        
        GameObject footprint = footprintData.footprintObject;
        footprint.transform.position = position;
        footprint.transform.rotation = Quaternion.identity;
        footprint.SetActive(true);
        
        // 设置颜色
        MeshRenderer renderer = footprint.GetComponent<MeshRenderer>();
        if (renderer != null && renderer.material != null)
        {
            Color footprintColor = type == FootprintType.Player ? playerFootprintColor : enemyFootprintColor;
            renderer.material.color = footprintColor;
        }
    }
    
    private void UpdateFootprintLifetime()
    {
        for (int i = 0; i < activeFootprints.Count; i++)
        {
            FootprintData footprint = activeFootprints[i];
            if (!footprint.isActive) continue;
            
            float age = Time.time - footprint.creationTime;
            
            // 淡出效果
            if (age > footprintLifetime - footprintFadeTime)
            {
                float fadeAlpha = 1f - ((age - (footprintLifetime - footprintFadeTime)) / footprintFadeTime);
                fadeAlpha = Mathf.Clamp01(fadeAlpha);
                
                MeshRenderer renderer = footprint.footprintObject.GetComponent<MeshRenderer>();
                if (renderer != null && renderer.material != null)
                {
                    Color color = renderer.material.color;
                    color.a = fadeAlpha * (footprint.type == FootprintType.Player ? playerFootprintColor.a : enemyFootprintColor.a);
                    renderer.material.color = color;
                }
            }
        }
    }
    
    private void CleanupExpiredFootprints()
    {
        for (int i = activeFootprints.Count - 1; i >= 0; i--)
        {
            FootprintData footprint = activeFootprints[i];
            float age = Time.time - footprint.creationTime;
            
            if (age > footprintLifetime)
            {
                // 回收到对象池
                footprint.footprintObject.SetActive(false);
                footprint.isActive = false;
                footprintPool.Enqueue(footprint);
                activeFootprints.RemoveAt(i);
            }
        }
    }
    
    /// <summary>
    /// 清除所有脚印
    /// </summary>
    public void ClearAllFootprints()
    {
        foreach (FootprintData footprint in activeFootprints)
        {
            footprint.footprintObject.SetActive(false);
            footprint.isActive = false;
            footprintPool.Enqueue(footprint);
        }
        activeFootprints.Clear();
        
        Debug.Log("[FootprintSystem] 清除了所有脚印");
    }
    
    /// <summary>
    /// 设置脚印生命周期
    /// </summary>
    public void SetFootprintLifetime(float lifetime)
    {
        footprintLifetime = Mathf.Max(5f, lifetime);
        Debug.Log($"[FootprintSystem] 脚印生命周期设置为: {footprintLifetime}秒");
    }
    
    private void OnDrawGizmosSelected()
    {
        // 在场景视图中显示跟踪的对象
        Gizmos.color = Color.yellow;
        foreach (var kvp in trackedObjects)
        {
            if (kvp.Key != null)
            {
                Gizmos.DrawWireSphere(kvp.Key.position, 0.5f);
            }
        }
        
        // 显示活跃脚印
        Gizmos.color = Color.cyan;
        foreach (FootprintData footprint in activeFootprints)
        {
            if (footprint.isActive)
            {
                Gizmos.DrawWireCube(footprint.position, footprintScale);
            }
        }
    }
}
