using UnityEngine;

/// <summary>
/// 材质管理器 - 统一管理游戏中的视觉材质效果
/// </summary>
public class MaterialManager : MonoBehaviour
{
    [Header("敌人材质设置")]
    [SerializeField] private Material furryBlackEnemyMaterial;
    [SerializeField] private Material blueGlowingSwordMaterial;
    
    [Header("材质路径")]
    [SerializeField] private string enemyMaterialPath = "Assets/Materials/FurryBlackEnemyMaterial.mat";
    [SerializeField] private string swordMaterialPath = "Assets/Materials/BlueGlowingSwordMaterial.mat";
    
    private static MaterialManager instance;
    public static MaterialManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<MaterialManager>();
            }
            return instance;
        }
    }
    
    public Material FurryBlackEnemyMaterial => furryBlackEnemyMaterial;
    public Material BlueGlowingSwordMaterial => blueGlowingSwordMaterial;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        LoadMaterials();
    }
    
    private void Start()
    {
        ApplyMaterialsToExistingObjects();
    }
    
    private void LoadMaterials()
    {
        // 尝试从Resources加载材质，如果没有则使用直接引用
        if (furryBlackEnemyMaterial == null)
        {
            furryBlackEnemyMaterial = Resources.Load<Material>("Materials/FurryBlackEnemyMaterial");
        }
        
        if (blueGlowingSwordMaterial == null)
        {
            blueGlowingSwordMaterial = Resources.Load<Material>("Materials/BlueGlowingSwordMaterial");
        }
        
        Debug.Log("[MaterialManager] 材质加载完成");
    }
    
    private void ApplyMaterialsToExistingObjects()
    {
        // 应用敌人材质到所有现有敌人
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            ApplyEnemyMaterial(enemy);
        }
        
        // 应用飞剑材质到所有现有飞剑
        ApplyMaterialsToAllSwords();
        
        Debug.Log($"[MaterialManager] 应用材质到 {enemies.Length} 个敌人");
    }
    
    /// <summary>
    /// 应用敌人材质到指定游戏对象
    /// </summary>
    public void ApplyEnemyMaterial(GameObject enemyObject)
    {
        if (enemyObject == null || furryBlackEnemyMaterial == null) return;
        
        MeshRenderer renderer = enemyObject.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material = furryBlackEnemyMaterial;
            Debug.Log($"[MaterialManager] 为敌人 {enemyObject.name} 应用了黑色毛茸茸材质");
        }
    }
    
    /// <summary>
    /// 应用飞剑材质到指定游戏对象
    /// </summary>
    public void ApplySwordMaterial(GameObject swordObject)
    {
        if (swordObject == null || blueGlowingSwordMaterial == null) return;
        
        MeshRenderer renderer = swordObject.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material = blueGlowingSwordMaterial;
            Debug.Log($"[MaterialManager] 为飞剑 {swordObject.name} 应用了蓝光发光材质");
        }
    }
    
    /// <summary>
    /// 应用材质到所有飞剑（包括预制件生成的）
    /// </summary>
    private void ApplyMaterialsToAllSwords()
    {
        // 查找所有包含"Sword"的对象
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Sword") || obj.name.Contains("sword"))
            {
                ApplySwordMaterial(obj);
            }
        }
        
        // 查找所有FlyingSword3D组件的对象
        FlyingSword3D[] flyingSwords = FindObjectsByType<FlyingSword3D>(FindObjectsSortMode.None);
        foreach (FlyingSword3D sword in flyingSwords)
        {
            ApplySwordMaterial(sword.gameObject);
        }
    }
    
    /// <summary>
    /// 当新敌人生成时调用此方法
    /// </summary>
    public void OnEnemySpawned(GameObject enemy)
    {
        ApplyEnemyMaterial(enemy);
    }
    
    /// <summary>
    /// 当新飞剑生成时调用此方法
    /// </summary>
    public void OnSwordSpawned(GameObject sword)
    {
        ApplySwordMaterial(sword);
    }
    
    /// <summary>
    /// 优化发光材质的蓝光强度
    /// </summary>
    public void SetSwordGlowIntensity(float intensity)
    {
        if (blueGlowingSwordMaterial == null) return;
        
        intensity = Mathf.Clamp(intensity, 0.1f, 3f);
        Color emissionColor = new Color(0.2f * intensity, 0.6f * intensity, 1.2f * intensity, 1f);
        blueGlowingSwordMaterial.SetColor("_EmissionColor", emissionColor);
        
        Debug.Log($"[MaterialManager] 飞剑发光强度设置为: {intensity}");
    }
    
    /// <summary>
    /// 设置敌人材质的毛茸茸效果强度
    /// </summary>
    public void SetEnemyFurEffect(float normalIntensity, float roughness)
    {
        if (furryBlackEnemyMaterial == null) return;
        
        normalIntensity = Mathf.Clamp(normalIntensity, 0.5f, 2f);
        roughness = Mathf.Clamp01(1f - roughness); // 转换为光滑度
        
        furryBlackEnemyMaterial.SetFloat("_BumpScale", normalIntensity);
        furryBlackEnemyMaterial.SetFloat("_Smoothness", roughness);
        
        Debug.Log($"[MaterialManager] 敌人毛茸茸效果: 法线强度={normalIntensity}, 粗糙度={1f - roughness}");
    }
    
    private void Update()
    {
        // 定期检查并应用材质到新生成的对象
        if (Time.frameCount % 60 == 0) // 每秒检查一次
        {
            ApplyMaterialsToAllSwords();
        }
    }
    
    private void OnValidate()
    {
        // 编辑器中实时预览效果
        if (Application.isPlaying)
        {
            ApplyMaterialsToExistingObjects();
        }
    }
}
