using UnityEngine;

/// <summary>
/// 运行时资产更新器 - 自动替换游戏对象的素材
/// </summary>
public class RuntimeAssetUpdater : MonoBehaviour
{
    [Header("生成的素材")]
    [SerializeField] private Sprite playerSprite;
    [SerializeField] private Sprite flyingSwordSprite;
    [SerializeField] private Sprite basicGhostSprite;
    [SerializeField] private Sprite strongGhostSprite;
    [SerializeField] private Sprite swordTrailSprite;
    [SerializeField] private Sprite hitEffectSprite;
    
    [Header("自动更新设置")]
    [SerializeField] private bool updateOnStart = true;
    [SerializeField] private bool updateContinuously = true;
    [SerializeField] private float updateInterval = 1f;
    
    private float lastUpdateTime = 0f;
    
    private void Start()
    {
        if (updateOnStart)
        {
            // 自动加载生成的素材
            LoadGeneratedAssets();
            
            // 更新现有对象
            UpdateAllAssets();
        }
    }
    
    private void Update()
    {
        if (updateContinuously && Time.time - lastUpdateTime > updateInterval)
        {
            UpdateAllAssets();
            lastUpdateTime = Time.time;
        }
    }
    
    /// <summary>
    /// 自动加载生成的素材
    /// </summary>
    private void LoadGeneratedAssets()
    {
        // 加载角色素材
        if (playerSprite == null)
            playerSprite = Resources.Load<Sprite>("GeneratedAssets/characters/player_character");
        if (basicGhostSprite == null)
            basicGhostSprite = Resources.Load<Sprite>("GeneratedAssets/characters/basic_ghost");
        if (strongGhostSprite == null)
            strongGhostSprite = Resources.Load<Sprite>("GeneratedAssets/characters/strong_ghost");
            
        // 加载武器素材
        if (flyingSwordSprite == null)
            flyingSwordSprite = Resources.Load<Sprite>("GeneratedAssets/weapons/flying_sword");
        if (swordTrailSprite == null)
            swordTrailSprite = Resources.Load<Sprite>("GeneratedAssets/weapons/sword_trail");
        if (hitEffectSprite == null)
            hitEffectSprite = Resources.Load<Sprite>("GeneratedAssets/weapons/hit_effect");
    }
    
    /// <summary>
    /// 更新所有资产
    /// </summary>
    private void UpdateAllAssets()
    {
        UpdatePlayerAssets();
        UpdateFlyingSwordAssets();
        UpdateEnemyAssets();
    }
    
    /// <summary>
    /// 更新玩家素材
    /// </summary>
    private void UpdatePlayerAssets()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null && playerSprite != null)
        {
            SpriteRenderer renderer = player.GetComponent<SpriteRenderer>();
            if (renderer != null && renderer.sprite != playerSprite)
            {
                renderer.sprite = playerSprite;
                Debug.Log("玩家素材已更新");
            }
        }
    }
    
    /// <summary>
    /// 更新飞剑素材
    /// </summary>
    private void UpdateFlyingSwordAssets()
    {
        if (flyingSwordSprite == null) return;
        
        // 查找所有飞剑对象 - 修改为3D版本
        FlyingSword3D[] flyingSwords = FindObjectsByType<FlyingSword3D>(FindObjectsSortMode.None);
        foreach (FlyingSword3D sword in flyingSwords)
        {
            SpriteRenderer renderer = sword.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.sprite = flyingSwordSprite;
                // 调整飞剑尺寸为新的合适大小
                sword.transform.localScale = new Vector3(0.2f, 0.2f, 1f);
                Debug.Log($"飞剑素材已更新: {sword.name}");
            }
        }
    }
    
    /// <summary>
    /// 更新敌人素材
    /// </summary>
    private void UpdateEnemyAssets()
    {
        if (basicGhostSprite == null) return;
        
        // 查找所有敌人对象
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            SpriteRenderer renderer = enemy.GetComponent<SpriteRenderer>();
            if (renderer != null && renderer.sprite != basicGhostSprite)
            {
                renderer.sprite = basicGhostSprite;
                // 调整敌人尺寸
                enemy.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
                Debug.Log($"敌人素材已更新: {enemy.name}");
            }
        }
    }
    
    /// <summary>
    /// 手动更新所有素材
    /// </summary>
    [ContextMenu("手动更新所有素材")]
    public void ManualUpdateAllAssets()
    {
        LoadGeneratedAssets();
        UpdateAllAssets();
        Debug.Log("手动更新所有素材完成");
    }
}
