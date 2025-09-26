using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 伤害数字管理器 - 统一管理伤害数字的生成和显示（使用对象池优化）
/// </summary>
public class DamageNumberManager : MonoBehaviour
{
    [Header("生成设置")]
    [SerializeField] private Vector3 spawnOffset = new Vector3(0, 1f, 0); // 生成偏移
    [SerializeField] private float randomRange = 0.5f;       // 随机位置范围
    [SerializeField] private float defaultLifetime = 2f;     // 默认显示时长
    
    [Header("Canvas样式设置")]
    [SerializeField] private float canvasFontSize = 64f;        // Canvas字体大小
    [SerializeField] private Vector2 canvasTextSize = new Vector2(100, 50); // Canvas文本框大小
    [SerializeField] private float canvasUpSpeed = 100f;        // Canvas上升速度
    [SerializeField] private TMPro.FontStyles fontStyle = TMPro.FontStyles.Bold; // 字体样式
    
    [Header("颜色配置")]
    [SerializeField] private Color playerDamageColor = Color.red;               // 玩家伤害颜色
    [SerializeField] private Color enemyDamageColor = new Color(1f, 0.5f, 0f, 1f); // 敌人伤害颜色
    [SerializeField] private Color healingColor = Color.green;                  // 治疗颜色
    
    [Header("调试设置")]
    [SerializeField] private bool enableDebugLog = false; // 是否启用调试日志
    
    // 单例模式
    public static DamageNumberManager Instance { get; private set; }
    
    // Canvas缓存
    private Canvas uiCanvas;
    
    // 相机引用
    private Camera mainCamera;
    
    private void Awake()
    {
        // 单例设置
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[DamageNumberManager] 单例已初始化");
        }
        else
        {
            Debug.Log("[DamageNumberManager] 发现重复实例，销毁");
            Destroy(gameObject);
            return;
        }
        
        // 初始化相机引用
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
        
        // 初始化Canvas缓存
        InitializeCanvasCache();
    }
    
    /// <summary>
    /// 初始化Canvas缓存
    /// </summary>
    private void InitializeCanvasCache()
    {
        // 查找UI Canvas，优先使用GameUICanvas
        GameObject gameUICanvasObj = GameObject.Find("GameUICanvas");
        if (gameUICanvasObj != null)
        {
            uiCanvas = gameUICanvasObj.GetComponent<Canvas>();
        }
        
        // 如果没找到，查找第一个Canvas
        if (uiCanvas == null)
        {
            uiCanvas = FindFirstObjectByType<Canvas>();
        }
        
        if (uiCanvas == null)
        {
            Debug.LogWarning("[DamageNumberManager] 未找到UI Canvas，伤害数字可能无法正确显示");
        }
        else
        {
            if (enableDebugLog)
                Debug.Log($"[DamageNumberManager] 已缓存UI Canvas: {uiCanvas.name}");
        }
    }
    
    /// <summary>
    /// 显示伤害数字
    /// </summary>
    public void ShowDamageNumber(Vector3 position, float damage, DamageType damageType = DamageType.EnemyDamage)
    {
        // Debug.Log($"[DamageNumberManager] ShowDamageNumber: position={position}, damage={damage}, type={damageType}");
        
        // 使用Canvas UI方案显示伤害数字（支持丰富样式）
        ShowDamageNumberTraditional(position, damage, damageType);
        
        if (enableDebugLog)
            Debug.Log($"[DamageNumberManager] 伤害数字显示成功: {damage} ({damageType})");
    }
    
    /// <summary>
    /// 验证Canvas是否可用
    /// </summary>
    private bool ValidateCanvas()
    {
        if (uiCanvas == null)
        {
            // 尝试重新获取Canvas
            InitializeCanvasCache();
        }
        
        return uiCanvas != null;
    }
    
    /// <summary>
    /// 计算显示位置
    /// </summary>
    private Vector3 CalculateDisplayPosition(Vector3 worldPosition)
    {
        Vector3 displayPosition = worldPosition + spawnOffset;
        
        // 添加随机偏移，避免重叠
        displayPosition += new Vector3(
            Random.Range(-randomRange, randomRange),
            Random.Range(-randomRange * 0.5f, randomRange),
            0
        );
        
        return displayPosition;
    }
    
    /// <summary>
    /// 设置屏幕位置（用于UI Canvas）
    /// </summary>
    private void SetScreenPosition(DamageNumber damageNumber, Vector3 worldPosition)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("[DamageNumberManager] 主摄像机未找到，无法转换世界坐标");
            return;
        }
        
        // 将世界坐标转换为屏幕坐标
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPosition + spawnOffset);
            
            // 添加随机偏移，避免重叠
            screenPos.x += Random.Range(-randomRange * 30, randomRange * 30);
            screenPos.y += Random.Range(-randomRange * 15, randomRange * 30);
            
        // 设置UI位置
        RectTransform rectTransform = damageNumber.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.position = screenPos;
        }
    }
    
    /// <summary>
    /// 显示玩家受到的伤害
    /// </summary>
    public void ShowPlayerDamage(Vector3 position, float damage)
    {
        ShowDamageNumber(position, damage, DamageType.PlayerDamage);
    }
    
    /// <summary>
    /// 显示敌人受到的伤害
    /// </summary>
    public void ShowEnemyDamage(Vector3 position, float damage)
    {
        ShowDamageNumber(position, damage, DamageType.EnemyDamage);
    }
    
    /// <summary>
    /// 显示治疗数字
    /// </summary>
    public void ShowHealingNumber(Vector3 position, float healAmount)
    {
        ShowDamageNumber(position, healAmount, DamageType.Healing);
    }
    
    /// <summary>
    /// 显示自定义文本
    /// </summary>
    public void ShowCustomText(Vector3 position, string text, Color color)
    {
        if (enableDebugLog)
            Debug.Log($"[DamageNumberManager] ShowCustomText: text={text}, color={color}, position={position}");
        
        // 检查对象池是否可用
        if (DamageNumberPool.Instance == null)
        {
            Debug.LogError("[DamageNumberManager] DamageNumberPool未初始化！无法显示自定义文本");
            return;
        }
        
        // 验证Canvas
        if (!ValidateCanvas())
        {
            Debug.LogWarning("[DamageNumberManager] Canvas不可用，无法显示自定义文本");
            return;
        }
        
        // 计算显示位置
        Vector3 displayPosition = CalculateDisplayPosition(position);
        
        // 直接创建自定义伤害数字（绕过对象池问题）
        Debug.Log($"[DamageNumberManager] 直接创建自定义伤害数字: {text}, 颜色: {color}");
        CreateCustomDamageNumberDirectly(text, color, position);
        
        if (enableDebugLog)
            Debug.Log($"[DamageNumberManager] 自定义文本显示成功: {text}");
    }
    
    /// <summary>
    /// 获取对象池统计信息
    /// </summary>
    public string GetPoolStats()
    {
        if (DamageNumberPool.Instance != null)
        {
            return DamageNumberPool.Instance.GetPoolStats();
        }
        return "DamageNumberPool: 未初始化";
    }
    
    /// <summary>
    /// 创建世界空间伤害文本（不依赖Canvas）
    /// </summary>
    private void CreateWorldSpaceDamageText(Vector3 position, float damage, DamageType damageType)
    {
        // 创建3D文本对象
        GameObject textObj = new GameObject("WorldDamageText");
        // 贴着头皮显示 - 根据对象类型调整高度
        float headHeight = 0.5f; // Player和Enemy头部大致高度
        Vector3 offset = Vector3.up * headHeight; // 贴着头皮
        textObj.transform.position = position + offset;
        
        // 添加TextMesh组件（3D文本，不需要Canvas）
        TextMesh textMesh = textObj.AddComponent<TextMesh>();
        textMesh.text = $"-{damage:F0}";
        textMesh.fontSize = 10; // 大幅增加字体大小，确保清晰可见
        textMesh.anchor = TextAnchor.MiddleCenter;
        
        // 设置文字缩放，确保在不同距离下都清晰可见
        textObj.transform.localScale = Vector3.one * 1.0f; // 1.0倍缩放，最大尺寸
        
        // 设置颜色
        switch (damageType)
        {
            case DamageType.PlayerDamage:
                textMesh.color = Color.red;
                break;
            case DamageType.EnemyDamage:
                textMesh.color = new Color(1f, 0.5f, 0f); // 橙色
                break;
            case DamageType.Healing:
                textMesh.color = Color.green;
                break;
        }
        
        // 让文本面向摄像机
        if (Camera.main != null)
        {
            textObj.transform.LookAt(Camera.main.transform);
            textObj.transform.Rotate(0, 180, 0); // 翻转让文字正向显示
        }
        
        // 简单动画：上升后消失
        StartCoroutine(AnimateWorldSpaceText(textObj));
        
        // 伤害文本创建完成
    }
    
    /// <summary>
    /// 世界空间文本动画
    /// </summary>
    private System.Collections.IEnumerator AnimateWorldSpaceText(GameObject textObj)
    {
        float duration = 1f;
        Vector3 startPos = textObj.transform.position;
        Vector3 endPos = startPos + Vector3.up * 1f;
        TextMesh textMesh = textObj.GetComponent<TextMesh>();
        Color startColor = textMesh.color;
        
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float progress = t / duration;
            
            // 上升运动
            textObj.transform.position = Vector3.Lerp(startPos, endPos, progress);
            
            // 淡出效果
            Color color = startColor;
            color.a = Mathf.Lerp(1f, 0f, progress);
            textMesh.color = color;
            
            yield return null;
        }
        
        // 销毁对象
        Destroy(textObj);
    }

    /// <summary>
    /// 预加载伤害数字对象
    /// </summary>
    /// <param name="count">预加载数量</param>
    public void PreloadDamageNumbers(int count)
    {
        if (DamageNumberPool.Instance != null)
        {
            DamageNumberPool.Instance.PreloadDamageNumbers(count);
            Debug.Log($"[DamageNumberManager] 已请求预加载 {count} 个伤害数字对象");
        }
        else
        {
            Debug.LogWarning("[DamageNumberManager] DamageNumberPool未初始化，无法预加载");
        }
    }
    
    /// <summary>
    /// 清空对象池
    /// </summary>
    public void ClearPool()
    {
        if (DamageNumberPool.Instance != null)
        {
            DamageNumberPool.Instance.ClearPool();
            Debug.Log("[DamageNumberManager] 已清空伤害数字对象池");
        }
    }
    
    /// <summary>
    /// 验证对象池完整性
    /// </summary>
    public bool ValidatePool()
    {
        if (DamageNumberPool.Instance != null)
        {
            return DamageNumberPool.Instance.ValidatePool();
        }
        return false;
    }
    
    /// <summary>
    /// 设置默认生命周期
    /// </summary>
    /// <param name="lifetime">新的默认生命周期</param>
    public void SetDefaultLifetime(float lifetime)
    {
        defaultLifetime = Mathf.Max(0.1f, lifetime);
        Debug.Log($"[DamageNumberManager] 默认生命周期已设置为: {defaultLifetime}秒");
    }
    
    /// <summary>
    /// 获取当前对象池状态
    /// </summary>
    public (int total, int active, int available) GetPoolStatus()
    {
        if (DamageNumberPool.Instance != null)
        {
            return (
                DamageNumberPool.Instance.TotalCount,
                DamageNumberPool.Instance.ActiveCount,
                DamageNumberPool.Instance.AvailableCount
            );
        }
        return (0, 0, 0);
    }
    
    // 调试方法
    [ContextMenu("显示池状态")]
    private void ShowPoolStatus()
    {
        Debug.Log($"[DamageNumberManager] {GetPoolStats()}");
    }
    
    [ContextMenu("验证池完整性")]
    private void ValidatePoolIntegrity()
    {
        bool isValid = ValidatePool();
        Debug.Log($"[DamageNumberManager] 池完整性验证: {(isValid ? "通过" : "失败")}");
    }

    /// <summary>
    /// 传统方式显示伤害数字（不使用对象池）
    /// </summary>
    private void ShowDamageNumberTraditional(Vector3 position, float damage, DamageType damageType)
    {
        // 验证Canvas
        if (!ValidateCanvas())
        {
            Debug.LogWarning("[DamageNumberManager] Canvas不可用，无法显示伤害数字");
            return;
        }

        // 创建简单的伤害数字GameObject
        GameObject damageObj = new GameObject("DamageNumber_Traditional");
        damageObj.transform.SetParent(uiCanvas.transform, false);

        // 添加RectTransform
        RectTransform rectTransform = damageObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = canvasTextSize;
        
        // 添加TextMeshProUGUI组件
        TextMeshProUGUI textComponent = damageObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = $"-{damage:F0}";
        // Debug.Log($"[DamageNumberManager] 设置文本内容: {textComponent.text}, 原始damage值: {damage}");
        textComponent.fontSize = canvasFontSize;
        textComponent.alignment = TMPro.TextAlignmentOptions.Center;
        textComponent.fontStyle = fontStyle;

        // 设置颜色（使用配置字段）
        switch (damageType)
        {
            case DamageType.PlayerDamage:
                textComponent.color = playerDamageColor;
                break;
            case DamageType.EnemyDamage:
                textComponent.color = enemyDamageColor;
                break;
            case DamageType.Healing:
                textComponent.color = healingColor;
                break;
        }

        // 计算并设置位置（使用与CreateDamageNumberDirectly相同的方法）
        Vector3 screenPos = mainCamera.WorldToScreenPoint(position);
        
        // 转换屏幕坐标到UI坐标
        Vector2 uiPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            uiCanvas.transform as RectTransform,
            screenPos,
            uiCanvas.worldCamera,
            out uiPosition
        );
        rectTransform.localPosition = uiPosition;

        // 简单的上移和淡出动画
        StartCoroutine(AnimateTraditionalDamageNumber(damageObj, textComponent));

        if (enableDebugLog)
            Debug.Log($"[DamageNumberManager] 传统方式显示伤害数字: {damage} ({damageType})");
    }
    
    /// <summary>
    /// 传统伤害数字动画协程
    /// </summary>
    private System.Collections.IEnumerator AnimateTraditionalDamageNumber(GameObject damageObj, TextMeshProUGUI textComponent)
    {
        float timer = 0f;
        Vector3 startPos = damageObj.transform.position;
        Color startColor = textComponent.color;

        while (timer < defaultLifetime)
        {
            timer += Time.deltaTime;
            float progress = timer / defaultLifetime;

            // 上移（使用配置的上升速度）
            damageObj.transform.position = startPos + Vector3.up * (progress * canvasUpSpeed);

            // 淡出
            Color currentColor = startColor;
            currentColor.a = 1f - progress;
            textComponent.color = currentColor;

            yield return null;
        }

        // 销毁对象
        if (damageObj != null)
        {
            Destroy(damageObj);
        }
    }

    private void CreateDamageNumberDirectly(float damage, DamageType damageType, Vector3 worldPosition)
    {
        // 将世界坐标转换为屏幕坐标
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPosition);
        
        // 创建GameObject
        GameObject damageObj = new GameObject("DamageNumber_Direct");
        damageObj.transform.SetParent(uiCanvas.transform, false);
        
        // 添加RectTransform
        RectTransform rectTransform = damageObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(120, 60);
        
        // 转换屏幕坐标到UI坐标
        Vector2 uiPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            uiCanvas.transform as RectTransform,
            screenPos,
            uiCanvas.worldCamera,
            out uiPosition
        );
        rectTransform.localPosition = uiPosition;
        
        // 添加TextMeshProUGUI
        TMPro.TextMeshProUGUI text = damageObj.AddComponent<TMPro.TextMeshProUGUI>();
        text.text = $"-{damage:F0}";
        text.fontSize = 64; // 增大字体，更容易看到
        text.alignment = TMPro.TextAlignmentOptions.Center;
        text.fontStyle = TMPro.FontStyles.Bold;
        
        // 根据伤害类型设置颜色
        switch (damageType)
        {
            case DamageType.PlayerDamage:
                text.color = Color.red;
                break;
            case DamageType.EnemyDamage:
                text.color = new Color(1f, 0.5f, 0f, 1f); // 橙色，更明显
                break;
            default:
                text.color = Color.white;
                break;
        }
        
        // 添加动画：向上移动并淡出
        StartCoroutine(AnimateDamageNumberDirect(damageObj, rectTransform));
        
        Debug.Log($"[DamageNumberManager] 成功创建伤害数字: {text.text}, 颜色: {text.color}");
    }

    private System.Collections.IEnumerator AnimateDamageNumberDirect(GameObject damageObj, RectTransform rectTransform)
    {
        float duration = 2f;
        float elapsed = 0f;
        Vector3 startPos = rectTransform.localPosition;
        Vector3 endPos = startPos + Vector3.up * 100f;
        
        TMPro.TextMeshProUGUI text = damageObj.GetComponent<TMPro.TextMeshProUGUI>();
        Color startColor = text.color;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            // 向上移动
            rectTransform.localPosition = Vector3.Lerp(startPos, endPos, progress);
            
            // 淡出
            Color currentColor = startColor;
            currentColor.a = Mathf.Lerp(1f, 0f, progress);
            text.color = currentColor;
            
            yield return null;
        }
        
        // 销毁对象
        Destroy(damageObj);
    }

    private void CreateCustomDamageNumberDirectly(string text, Color color, Vector3 worldPosition)
    {
        // 将世界坐标转换为屏幕坐标
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPosition);
        
        // 创建GameObject
        GameObject damageObj = new GameObject("CustomDamageNumber_Direct");
        damageObj.transform.SetParent(uiCanvas.transform, false);
        
        // 添加RectTransform
        RectTransform rectTransform = damageObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(120, 60);
        
        // 转换屏幕坐标到UI坐标
        Vector2 uiPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            uiCanvas.transform as RectTransform,
            screenPos,
            uiCanvas.worldCamera,
            out uiPosition
        );
        rectTransform.localPosition = uiPosition;
        
        // 添加TextMeshProUGUI
        TMPro.TextMeshProUGUI textComponent = damageObj.AddComponent<TMPro.TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = 64; // 增大字体，更容易看到
        textComponent.alignment = TMPro.TextAlignmentOptions.Center;
        textComponent.fontStyle = TMPro.FontStyles.Bold;
        textComponent.color = color;
        
        // 添加动画：向上移动并淡出
        StartCoroutine(AnimateDamageNumberDirect(damageObj, rectTransform));
        
        Debug.Log($"[DamageNumberManager] 成功创建自定义伤害数字: {text}, 颜色: {color}");
    }
}
