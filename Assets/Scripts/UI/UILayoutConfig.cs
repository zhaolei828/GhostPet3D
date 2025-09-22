using UnityEngine;

/// <summary>
/// UI布局配置 - 使用ScriptableObject管理所有UI元素的布局参数
/// </summary>
[CreateAssetMenu(fileName = "UILayoutConfig", menuName = "Game/UI Layout Config")]
public class UILayoutConfig : ScriptableObject
{
    [Header("血量条设置")]
    [Tooltip("血量条锚点最小值 (左上角为 0,1)")]
    public Vector2 healthBarAnchorMin = new Vector2(0, 1);
    
    [Tooltip("血量条锚点最大值 (左上角为 0,1)")]
    public Vector2 healthBarAnchorMax = new Vector2(0, 1);
    
    [Tooltip("血量条相对锚点位置")]
    public Vector2 healthBarPosition = new Vector2(100, -50);
    
    [Tooltip("血量条尺寸")]
    public Vector2 healthBarSize = new Vector2(250, 40);
    
    [Header("分数UI设置")]
    [Tooltip("分数UI锚点最小值 (右上角为 1,1)")]
    public Vector2 scoreUIAnchorMin = new Vector2(1, 1);
    
    [Tooltip("分数UI锚点最大值 (右上角为 1,1)")]
    public Vector2 scoreUIAnchorMax = new Vector2(1, 1);
    
    [Tooltip("分数UI相对锚点位置")]
    public Vector2 scoreUIPosition = new Vector2(-100, -50);
    
    [Tooltip("分数UI尺寸")]
    public Vector2 scoreUISize = new Vector2(300, 40);
    
    [Header("飞剑状态UI设置")]
    [Tooltip("飞剑状态UI锚点最小值 (底部中心为 0.5,0)")]
    public Vector2 swordStatusAnchorMin = new Vector2(0.5f, 0);
    
    [Tooltip("飞剑状态UI锚点最大值 (底部中心为 0.5,0)")]
    public Vector2 swordStatusAnchorMax = new Vector2(0.5f, 0);
    
    [Tooltip("飞剑状态UI相对锚点位置")]
    public Vector2 swordStatusPosition = new Vector2(0, 80);
    
    [Tooltip("飞剑状态UI尺寸")]
    public Vector2 swordStatusSize = new Vector2(250, 50);
    
    [Header("通用设置")]
    [Tooltip("UI布局验证误差范围 (像素)")]
    public float positionTolerance = 1f;
    
    [Tooltip("是否启用布局调试日志")]
    public bool enableLayoutDebug = false;
    
    /// <summary>
    /// UI元素配置数据结构
    /// </summary>
    [System.Serializable]
    public struct UIElementConfig
    {
        public Vector2 anchorMin;
        public Vector2 anchorMax;
        public Vector2 position;
        public Vector2 size;
        
        public UIElementConfig(Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
        {
            this.anchorMin = anchorMin;
            this.anchorMax = anchorMax;
            this.position = position;
            this.size = size;
        }
    }
    
    /// <summary>
    /// 获取血量条配置
    /// </summary>
    public UIElementConfig GetHealthBarConfig()
    {
        return new UIElementConfig(healthBarAnchorMin, healthBarAnchorMax, healthBarPosition, healthBarSize);
    }
    
    /// <summary>
    /// 获取分数UI配置
    /// </summary>
    public UIElementConfig GetScoreUIConfig()
    {
        return new UIElementConfig(scoreUIAnchorMin, scoreUIAnchorMax, scoreUIPosition, scoreUISize);
    }
    
    /// <summary>
    /// 获取飞剑状态UI配置
    /// </summary>
    public UIElementConfig GetSwordStatusConfig()
    {
        return new UIElementConfig(swordStatusAnchorMin, swordStatusAnchorMax, swordStatusPosition, swordStatusSize);
    }
    
    /// <summary>
    /// 验证配置参数的有效性
    /// </summary>
    /// <returns>是否所有配置都有效</returns>
    public bool ValidateConfig()
    {
        bool isValid = true;
        
        // 验证锚点值范围 (0-1)
        if (!IsValidAnchor(healthBarAnchorMin) || !IsValidAnchor(healthBarAnchorMax))
        {
            Debug.LogError("[UILayoutConfig] 血量条锚点值无效，应在0-1范围内");
            isValid = false;
        }
        
        if (!IsValidAnchor(scoreUIAnchorMin) || !IsValidAnchor(scoreUIAnchorMax))
        {
            Debug.LogError("[UILayoutConfig] 分数UI锚点值无效，应在0-1范围内");
            isValid = false;
        }
        
        if (!IsValidAnchor(swordStatusAnchorMin) || !IsValidAnchor(swordStatusAnchorMax))
        {
            Debug.LogError("[UILayoutConfig] 飞剑状态UI锚点值无效，应在0-1范围内");
            isValid = false;
        }
        
        // 验证尺寸值 (应为正数)
        if (healthBarSize.x <= 0 || healthBarSize.y <= 0)
        {
            Debug.LogError("[UILayoutConfig] 血量条尺寸无效，应为正数");
            isValid = false;
        }
        
        if (scoreUISize.x <= 0 || scoreUISize.y <= 0)
        {
            Debug.LogError("[UILayoutConfig] 分数UI尺寸无效，应为正数");
            isValid = false;
        }
        
        if (swordStatusSize.x <= 0 || swordStatusSize.y <= 0)
        {
            Debug.LogError("[UILayoutConfig] 飞剑状态UI尺寸无效，应为正数");
            isValid = false;
        }
        
        // 验证容差值
        if (positionTolerance < 0)
        {
            Debug.LogError("[UILayoutConfig] 位置容差值无效，应为非负数");
            isValid = false;
        }
        
        return isValid;
    }
    
    /// <summary>
    /// 检查锚点值是否有效 (0-1范围)
    /// </summary>
    private bool IsValidAnchor(Vector2 anchor)
    {
        return anchor.x >= 0 && anchor.x <= 1 && anchor.y >= 0 && anchor.y <= 1;
    }
    
    /// <summary>
    /// 重置为默认值
    /// </summary>
    [ContextMenu("重置为默认值")]
    public void ResetToDefaults()
    {
        // 血量条默认值 (左上角)
        healthBarAnchorMin = new Vector2(0, 1);
        healthBarAnchorMax = new Vector2(0, 1);
        healthBarPosition = new Vector2(100, -50);
        healthBarSize = new Vector2(250, 40);
        
        // 分数UI默认值 (右上角)
        scoreUIAnchorMin = new Vector2(1, 1);
        scoreUIAnchorMax = new Vector2(1, 1);
        scoreUIPosition = new Vector2(-100, -50);
        scoreUISize = new Vector2(300, 40);
        
        // 飞剑状态UI默认值 (底部中心)
        swordStatusAnchorMin = new Vector2(0.5f, 0);
        swordStatusAnchorMax = new Vector2(0.5f, 0);
        swordStatusPosition = new Vector2(0, 80);
        swordStatusSize = new Vector2(250, 50);
        
        // 通用设置默认值
        positionTolerance = 1f;
        enableLayoutDebug = false;
        
        Debug.Log("[UILayoutConfig] 已重置为默认值");
    }
    
    /// <summary>
    /// 获取配置摘要信息
    /// </summary>
    public string GetConfigSummary()
    {
        return $"UI布局配置:\n" +
               $"血量条: 锚点({healthBarAnchorMin}), 位置({healthBarPosition}), 尺寸({healthBarSize})\n" +
               $"分数UI: 锚点({scoreUIAnchorMin}), 位置({scoreUIPosition}), 尺寸({scoreUISize})\n" +
               $"飞剑UI: 锚点({swordStatusAnchorMin}), 位置({swordStatusPosition}), 尺寸({swordStatusSize})";
    }
}
