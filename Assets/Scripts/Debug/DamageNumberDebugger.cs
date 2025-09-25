using UnityEngine;

/// <summary>
/// 伤害数字调试器 - 帮助诊断伤害数字系统问题
/// </summary>
public class DamageNumberDebugger : MonoBehaviour
{
    [Header("调试设置")]
    [SerializeField] private bool enableAutoTest = true;
    [SerializeField] private float testInterval = 3f;
    [SerializeField] private float testDamage = 50f;
    
    [Header("按键测试")]
    [SerializeField] private KeyCode testKey = KeyCode.T;
    
    private float lastTestTime;
    
    private void Start()
    {
        Debug.Log("[DamageNumberDebugger] 伤害数字调试器已启动");
        
        // 检查伤害数字系统
        CheckDamageNumberSystem();
    }
    
    private void Update()
    {
        // 按键测试
        if (Input.GetKeyDown(testKey))
        {
            TestDamageNumberAtPlayer();
        }
        
        // 自动测试
        if (enableAutoTest && Time.time - lastTestTime >= testInterval)
        {
            lastTestTime = Time.time;
            TestDamageNumberAtPlayer();
        }
    }
    
    /// <summary>
    /// 检查伤害数字系统配置
    /// </summary>
    private void CheckDamageNumberSystem()
    {
        Debug.Log("=== 伤害数字系统检查 ===");
        
        // 检查DamageNumberManager
        var damageManager = DamageNumberManager.Instance;
        if (damageManager != null)
        {
            Debug.Log("✅ DamageNumberManager.Instance 存在");
            Debug.Log($"   - GameObject: {damageManager.gameObject.name}");
            Debug.Log($"   - 组件启用: {damageManager.enabled}");
            Debug.Log($"   - GameObject激活: {damageManager.gameObject.activeInHierarchy}");
        }
        else
        {
            Debug.LogError("❌ DamageNumberManager.Instance 为 null");
        }
        
        // 检查DamageNumberPool
        var damagePool = DamageNumberPool.Instance;
        if (damagePool != null)
        {
            Debug.Log("✅ DamageNumberPool.Instance 存在");
            Debug.Log($"   - GameObject: {damagePool.gameObject.name}");
            Debug.Log($"   - 组件启用: {damagePool.enabled}");
            Debug.Log($"   - GameObject激活: {damagePool.gameObject.activeInHierarchy}");
        }
        else
        {
            Debug.LogError("❌ DamageNumberPool.Instance 为 null");
        }
        
        // 检查UI Canvas
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        Debug.Log($"📱 场景中发现 {canvases.Length} 个Canvas:");
        for (int i = 0; i < canvases.Length; i++)
        {
            Canvas canvas = canvases[i];
            Debug.Log($"   Canvas {i + 1}: {canvas.gameObject.name}");
            Debug.Log($"     - RenderMode: {canvas.renderMode}");
            Debug.Log($"     - SortingOrder: {canvas.sortingOrder}");
            Debug.Log($"     - WorldCamera: {(canvas.worldCamera != null ? canvas.worldCamera.name : "null")}");
            Debug.Log($"     - 激活状态: {canvas.gameObject.activeInHierarchy}");
        }
        
        Debug.Log("=== 检查完成 ===");
    }
    
    /// <summary>
    /// 在玩家位置测试伤害数字
    /// </summary>
    private void TestDamageNumberAtPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 testPosition = player.transform.position + Vector3.up * 2f;
            TestDamageNumberAtPosition(testPosition, testDamage);
        }
        else
        {
            Debug.LogWarning("[DamageNumberDebugger] 找不到Player对象");
        }
    }
    
    /// <summary>
    /// 在指定位置测试伤害数字
    /// </summary>
    private void TestDamageNumberAtPosition(Vector3 position, float damage)
    {
        Debug.Log($"[DamageNumberDebugger] 测试伤害数字: 位置{position}, 伤害{damage}");
        
        if (DamageNumberManager.Instance != null)
        {
            try
            {
                DamageNumberManager.Instance.ShowDamageNumber(position, damage, DamageType.EnemyDamage);
                Debug.Log("✅ 伤害数字显示调用成功");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ 伤害数字显示调用失败: {e.Message}");
            }
        }
        else
        {
            Debug.LogError("❌ DamageNumberManager.Instance 为 null，无法显示伤害数字");
        }
    }
    
    /// <summary>
    /// 手动强制创建伤害数字系统（如果缺失）
    /// </summary>
    [ContextMenu("强制创建伤害数字系统")]
    public void ForceCreateDamageSystem()
    {
        Debug.Log("[DamageNumberDebugger] 尝试强制创建伤害数字系统...");
        
        // 创建DamageNumberManager
        if (DamageNumberManager.Instance == null)
        {
            GameObject managerObj = new GameObject("DamageNumberManager");
            managerObj.AddComponent<DamageNumberManager>();
            Debug.Log("✅ 创建了 DamageNumberManager");
        }
        
        // 创建DamageNumberPool
        if (DamageNumberPool.Instance == null)
        {
            GameObject poolObj = new GameObject("DamageNumberPool");
            poolObj.AddComponent<DamageNumberPool>();
            Debug.Log("✅ 创建了 DamageNumberPool");
        }
        
        // 创建UI Canvas（如果没有合适的Canvas）
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        bool hasUICanvas = false;
        foreach (Canvas canvas in canvases)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                hasUICanvas = true;
                break;
            }
        }
        
        if (!hasUICanvas)
        {
            GameObject canvasObj = new GameObject("GameUICanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            Debug.Log("✅ 创建了 GameUICanvas");
        }
        
        Debug.Log("伤害数字系统强制创建完成！");
        
        // 重新检查
        Invoke(nameof(CheckDamageNumberSystem), 0.5f);
    }
}
