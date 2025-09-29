using UnityEngine;

/// <summary>
/// GameManager验证器 - 检查环境装饰系统是否正常工作
/// </summary>
public class GameManagerValidator : MonoBehaviour
{
    [Header("验证设置")]
    [SerializeField] private bool autoValidateOnStart = true;
    [SerializeField] private bool showDetailedLogs = true;
    
    private void Start()
    {
        if (autoValidateOnStart)
        {
            Invoke(nameof(ValidateGameManager), 1f); // 延迟1秒等待GameManager初始化
        }
    }
    
    [ContextMenu("验证GameManager")]
    public void ValidateGameManager()
    {
        Debug.Log("=== GameManager 验证开始 ===");
        
        // 检查GameManager实例
        if (GameManager.Instance == null)
        {
            Debug.LogError("❌ GameManager.Instance 为空！");
            return;
        }
        
        Debug.Log("✅ GameManager.Instance 存在");
        
        // 检查场景中的GameManager数量
        GameManager[] managers = FindObjectsOfType<GameManager>();
        Debug.Log($"📊 场景中GameManager数量: {managers.Length}");
        
        if (managers.Length > 1)
        {
            Debug.LogWarning("⚠️ 场景中有多个GameManager！可能导致装饰物创建问题");
            for (int i = 0; i < managers.Length; i++)
            {
                Debug.Log($"   GameManager[{i}]: {managers[i].gameObject.name} (ID: {managers[i].GetInstanceID()})");
            }
        }
        
        // 检查材质文件
        ValidateMaterials();
        
        // 检查装饰物
        ValidateDecorations();
        
        Debug.Log("=== GameManager 验证完成 ===");
    }
    
    private void ValidateMaterials()
    {
        Debug.Log("--- 材质验证 ---");
        
        Material grassMaterial = Resources.Load<Material>("Environment/SnowGrassMaterial");
        Material blackRockMaterial = Resources.Load<Material>("Environment/BlackRockMaterial");
        Material grayRockMaterial = Resources.Load<Material>("Environment/GrayRockMaterial");
        
        Debug.Log($"🌿 SnowGrassMaterial: {(grassMaterial != null ? "✅ 存在" : "❌ 缺失")}");
        Debug.Log($"🪨 BlackRockMaterial: {(blackRockMaterial != null ? "✅ 存在" : "❌ 缺失")}");
        Debug.Log($"🪨 GrayRockMaterial: {(grayRockMaterial != null ? "✅ 存在" : "❌ 缺失")}");
        
        if (showDetailedLogs)
        {
            if (grassMaterial != null) Debug.Log($"   草地材质纹理: {grassMaterial.mainTexture?.name ?? "无"}");
            if (blackRockMaterial != null) Debug.Log($"   黑岩材质纹理: {blackRockMaterial.mainTexture?.name ?? "无"}");
            if (grayRockMaterial != null) Debug.Log($"   灰岩材质纹理: {grayRockMaterial.mainTexture?.name ?? "无"}");
        }
    }
    
    private void ValidateDecorations()
    {
        Debug.Log("--- 装饰物验证 ---");
        
        // 检查草丛装饰
        GameObject[] grassDecorations = GameObject.FindGameObjectsWithTag("Untagged");
        int grassCount = 0;
        int blackRockCount = 0;
        int grayRockCount = 0;
        
        foreach (GameObject obj in grassDecorations)
        {
            if (obj.name.Contains("SnowGrass"))
            {
                grassCount++;
                if (showDetailedLogs)
                    Debug.Log($"   🌿 SnowGrass发现: {obj.name} 位置:{obj.transform.position}");
            }
            else if (obj.name.Contains("BlackRock"))
            {
                blackRockCount++;
                if (showDetailedLogs)
                    Debug.Log($"   🪨 BlackRock发现: {obj.name} 位置:{obj.transform.position}");
            }
            else if (obj.name.Contains("GrayRock"))
            {
                grayRockCount++;
                if (showDetailedLogs)
                    Debug.Log($"   🪨 GrayRock发现: {obj.name} 位置:{obj.transform.position}");
            }
        }
        
        Debug.Log($"📊 装饰物统计:");
        Debug.Log($"   🌿 草丛装饰: {grassCount}/8 {(grassCount == 8 ? "✅" : grassCount > 0 ? "⚠️" : "❌")}");
        Debug.Log($"   🪨 黑色岩石: {blackRockCount}/3 {(blackRockCount == 3 ? "✅" : blackRockCount > 0 ? "⚠️" : "❌")}");
        Debug.Log($"   🪨 灰色岩石: {grayRockCount}/3 {(grayRockCount == 3 ? "✅" : grayRockCount > 0 ? "⚠️" : "❌")}");
        
        int totalDecorations = grassCount + blackRockCount + grayRockCount;
        Debug.Log($"🎯 总装饰物数量: {totalDecorations}/14");
        
        if (totalDecorations == 0)
        {
            Debug.LogError("❌ 没有发现任何装饰物！可能需要重启游戏让GameManager重新生成装饰物");
        }
        else if (totalDecorations < 14)
        {
            Debug.LogWarning($"⚠️ 装饰物数量不完整 ({totalDecorations}/14)");
        }
        else
        {
            Debug.Log("✅ 所有装饰物都已正确创建！");
        }
    }
}
