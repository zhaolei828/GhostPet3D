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
    
    [Header("调试设置")]
    [SerializeField] private bool enableDebugLog = false;    // 是否启用调试日志
    
    // 单例模式
    public static DamageNumberManager Instance { get; private set; }
    
    // Canvas缓存
    private Canvas uiCanvas;
    
    private void Awake()
    {
        // 单例设置
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
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
        if (enableDebugLog)
            Debug.Log($"[DamageNumberManager] ShowDamageNumber: position={position}, damage={damage}, type={damageType}");
        
        // 检查对象池是否可用，如果不可用则使用传统方法
        if (DamageNumberPool.Instance == null)
        {
            // 静默切换到传统方法，不显示警告
            ShowDamageNumberTraditional(position, damage, damageType);
            return;
        }
        
        // 验证Canvas
        if (!ValidateCanvas())
        {
            Debug.LogWarning("[DamageNumberManager] Canvas不可用，无法显示伤害数字");
            return;
        }
        
        // 计算显示位置
        Vector3 displayPosition = CalculateDisplayPosition(position);
        
        // 从对象池获取伤害数字
        DamageNumber damageNumber = DamageNumberPool.Instance.GetDamageNumber(
            damage, 
            damageType, 
            displayPosition, 
            uiCanvas.transform,
            defaultLifetime
        );
        
        if (damageNumber == null)
        {
            Debug.LogWarning("[DamageNumberManager] 无法从对象池获取伤害数字对象");
            return;
        }
        
        // 设置为屏幕坐标（如果是UI Canvas）
        if (uiCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            SetScreenPosition(damageNumber, position);
        }
        
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
        
        // 从对象池获取自定义伤害数字
        DamageNumber damageNumber = DamageNumberPool.Instance.GetCustomDamageNumber(
            text,
            color,
            displayPosition,
            uiCanvas.transform,
            defaultLifetime
        );
        
        if (damageNumber == null)
        {
            Debug.LogWarning("[DamageNumberManager] 无法从对象池获取自定义文本对象");
            return;
        }
        
        // 设置为屏幕坐标（如果是UI Canvas）
        if (uiCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            SetScreenPosition(damageNumber, position);
        }
        
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
        rectTransform.sizeDelta = new Vector2(100, 50);
        
        // 添加TextMeshProUGUI组件
        TextMeshProUGUI textComponent = damageObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = damage.ToString("F0");
        textComponent.fontSize = 24;
        textComponent.alignment = TMPro.TextAlignmentOptions.Center;

        // 设置颜色
        switch (damageType)
        {
            case DamageType.PlayerDamage:
                textComponent.color = Color.red;
                break;
            case DamageType.EnemyDamage:
                textComponent.color = Color.yellow;
                break;
            case DamageType.Healing:
                textComponent.color = Color.green;
                break;
        }

        // 计算并设置位置
        Vector3 displayPosition = CalculateDisplayPosition(position);
        if (uiCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                Vector3 screenPos = mainCamera.WorldToScreenPoint(displayPosition);
                rectTransform.position = screenPos;
            }
        }
        else
        {
            rectTransform.position = displayPosition;
        }

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

            // 上移
            damageObj.transform.position = startPos + Vector3.up * (progress * 100f);

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
}
