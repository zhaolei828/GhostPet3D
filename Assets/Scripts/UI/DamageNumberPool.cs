using UnityEngine;

/// <summary>
/// 伤害数字对象池 - 专门管理DamageNumber对象的池化
/// </summary>
public class DamageNumberPool : MonoBehaviour
{
    [Header("池配置")]
    [SerializeField] private GameObject damageNumberPrefab;
    [SerializeField] private int initialPoolSize = 20;
    [SerializeField] private int maxPoolSize = 100;
    [SerializeField] private bool allowPoolGrowth = true;
    [SerializeField] private bool enableDebugLog = false;
    
    [Header("生命周期设置")]
    [SerializeField] private float defaultLifetime = 2f;
    [SerializeField] private bool autoRecycle = true;
    
    // 单例模式
    public static DamageNumberPool Instance { get; private set; }
    
    // 对象池实例
    private ObjectPool<DamageNumber> damageNumberPool;
    private Transform poolParent;
    
    // 统计信息
    public int TotalCount => damageNumberPool?.TotalCount ?? 0;
    public int ActiveCount => damageNumberPool?.ActiveCount ?? 0;
    public int AvailableCount => damageNumberPool?.AvailableCount ?? 0;
    
    private void Awake()
    {
        // 单例模式实现
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePool();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 初始化对象池
    /// </summary>
    private void InitializePool()
    {
        // 创建池容器
        GameObject poolContainer = new GameObject("DamageNumberPool");
        poolContainer.transform.SetParent(transform);
        poolParent = poolContainer.transform;
        
        // 如果没有设置预制体，创建默认的
        if (damageNumberPrefab == null)
        {
            CreateDefaultDamageNumberPrefab();
        }
        
        // 验证预制体有DamageNumber组件
        if (damageNumberPrefab.GetComponent<DamageNumber>() == null)
        {
            Debug.LogError("[DamageNumberPool] 预制体缺少DamageNumber组件");
            return;
        }
        
        // 创建对象池
        damageNumberPool = new ObjectPool<DamageNumber>(
            prefab: damageNumberPrefab,
            poolParent: poolParent,
            initialSize: initialPoolSize,
            maxSize: maxPoolSize,
            allowGrowth: allowPoolGrowth,
            createFunction: null, // 使用默认创建函数
            onGet: OnGetDamageNumber,
            onRelease: OnReleaseDamageNumber,
            onDestroy: OnDestroyDamageNumber
        );
        
        if (enableDebugLog)
            Debug.Log($"[DamageNumberPool] 初始化完成: {GetPoolStats()}");
    }
    
    /// <summary>
    /// 获取伤害数字对象
    /// </summary>
    /// <param name="damage">伤害值</param>
    /// <param name="damageType">伤害类型</param>
    /// <param name="position">显示位置</param>
    /// <param name="parent">父级Transform</param>
    /// <param name="lifetime">显示时长（-1使用默认时长）</param>
    /// <returns>伤害数字对象</returns>
    public DamageNumber GetDamageNumber(float damage, DamageType damageType, Vector3 position, Transform parent = null, float lifetime = -1f)
    {
        if (damageNumberPool == null)
        {
            Debug.LogError("[DamageNumberPool] 对象池未初始化");
            return null;
        }
        
        DamageNumber damageNumber = damageNumberPool.Get();
        if (damageNumber == null)
        {
            Debug.LogWarning("[DamageNumberPool] 无法从池中获取伤害数字对象");
            return null;
        }
        
        // 设置位置和父级
        if (parent != null)
        {
            damageNumber.transform.SetParent(parent);
        }
        damageNumber.transform.position = position;
        
        // 初始化伤害数字
        damageNumber.Initialize(damage, damageType);
        
        // 设置生命周期
        float actualLifetime = lifetime > 0 ? lifetime : defaultLifetime;
        if (autoRecycle)
        {
            StartCoroutine(AutoRecycleDamageNumber(damageNumber, actualLifetime));
        }
        
        if (enableDebugLog)
            Debug.Log($"[DamageNumberPool] 获取伤害数字: 伤害={damage}, 类型={damageType}, 位置={position}");
        
        return damageNumber;
    }
    
    /// <summary>
    /// 获取自定义文本的伤害数字
    /// </summary>
    /// <param name="text">自定义文本</param>
    /// <param name="color">文本颜色</param>
    /// <param name="position">显示位置</param>
    /// <param name="parent">父级Transform</param>
    /// <param name="lifetime">显示时长（-1使用默认时长）</param>
    /// <returns>伤害数字对象</returns>
    public DamageNumber GetCustomDamageNumber(string text, Color color, Vector3 position, Transform parent = null, float lifetime = -1f)
    {
        if (damageNumberPool == null)
        {
            Debug.LogError("[DamageNumberPool] 对象池未初始化");
            return null;
        }
        
        DamageNumber damageNumber = damageNumberPool.Get();
        if (damageNumber == null)
        {
            Debug.LogWarning("[DamageNumberPool] 无法从池中获取自定义伤害数字对象");
            return null;
        }
        
        // 设置位置和父级
        if (parent != null)
        {
            damageNumber.transform.SetParent(parent);
        }
        damageNumber.transform.position = position;
        
        // 设置自定义文本
        damageNumber.SetCustomText(text, color);
        
        // 设置生命周期
        float actualLifetime = lifetime > 0 ? lifetime : defaultLifetime;
        if (autoRecycle)
        {
            StartCoroutine(AutoRecycleDamageNumber(damageNumber, actualLifetime));
        }
        
        if (enableDebugLog)
            Debug.Log($"[DamageNumberPool] 获取自定义伤害数字: 文本={text}, 颜色={color}, 位置={position}");
        
        return damageNumber;
    }
    
    /// <summary>
    /// 释放伤害数字对象回池中
    /// </summary>
    /// <param name="damageNumber">要释放的伤害数字对象</param>
    public void ReleaseDamageNumber(DamageNumber damageNumber)
    {
        if (damageNumber == null)
        {
            Debug.LogWarning("[DamageNumberPool] 尝试释放null的伤害数字对象");
            return;
        }
        
        if (damageNumberPool == null)
        {
            Debug.LogError("[DamageNumberPool] 对象池未初始化，无法释放对象");
            return;
        }
        
        damageNumberPool.Release(damageNumber);
        
        if (enableDebugLog)
            Debug.Log($"[DamageNumberPool] 释放伤害数字对象: {damageNumber.name}");
    }
    
    /// <summary>
    /// 自动回收伤害数字
    /// </summary>
    private System.Collections.IEnumerator AutoRecycleDamageNumber(DamageNumber damageNumber, float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        
        // 检查对象是否仍然有效
        if (damageNumber != null && damageNumber.gameObject != null)
        {
            ReleaseDamageNumber(damageNumber);
        }
    }
    
    /// <summary>
    /// 获取对象时的回调
    /// </summary>
    private void OnGetDamageNumber(DamageNumber damageNumber)
    {
        if (damageNumber == null) return;
        
        // 重置状态
        damageNumber.gameObject.SetActive(true);
        
        // 重置Transform
        damageNumber.transform.localScale = Vector3.one;
        damageNumber.transform.localRotation = Quaternion.identity;
        
        // 停止所有协程
        damageNumber.StopAllCoroutines();
        
        if (enableDebugLog)
            Debug.Log($"[DamageNumberPool] OnGet: {damageNumber.name}");
    }
    
    /// <summary>
    /// 释放对象时的回调
    /// </summary>
    private void OnReleaseDamageNumber(DamageNumber damageNumber)
    {
        if (damageNumber == null) return;
        
        // 停止所有协程
        damageNumber.StopAllCoroutines();
        
        // 重置到池父级
        damageNumber.transform.SetParent(poolParent);
        damageNumber.transform.localPosition = Vector3.zero;
        
        // 禁用对象
        damageNumber.gameObject.SetActive(false);
        
        if (enableDebugLog)
            Debug.Log($"[DamageNumberPool] OnRelease: {damageNumber.name}");
    }
    
    /// <summary>
    /// 销毁对象时的回调
    /// </summary>
    private void OnDestroyDamageNumber(DamageNumber damageNumber)
    {
        if (damageNumber == null) return;
        
        if (enableDebugLog)
            Debug.Log($"[DamageNumberPool] OnDestroy: {damageNumber.name}");
    }
    
    /// <summary>
    /// 创建默认的伤害数字预制体
    /// </summary>
    private void CreateDefaultDamageNumberPrefab()
    {
        Debug.Log("[DamageNumberPool] 创建默认伤害数字预制体...");
        
        // 创建游戏对象
        GameObject defaultPrefab = new GameObject("DamageNumber");
        
        // 添加RectTransform用于UI
        RectTransform rectTransform = defaultPrefab.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(100, 50);
        
        // 创建子对象来包含文本
        GameObject textChild = new GameObject("Text");
        textChild.transform.SetParent(defaultPrefab.transform);
        
        // 添加TextMeshProUGUI组件到子对象
        TMPro.TextMeshProUGUI textComponent = textChild.AddComponent<TMPro.TextMeshProUGUI>();
        textComponent.text = "0";
        textComponent.fontSize = 24f;
        textComponent.alignment = TMPro.TextAlignmentOptions.Center;
        textComponent.color = Color.white;
        
        // 设置文本的RectTransform
        RectTransform textRect = textChild.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        // 添加DamageNumber组件
        defaultPrefab.AddComponent<DamageNumber>();
        
        // 将其设为非激活状态，作为模板使用
        defaultPrefab.SetActive(false);
        damageNumberPrefab = defaultPrefab;
        
        Debug.Log("[DamageNumberPool] 默认伤害数字预制体创建完成");
    }
    
    /// <summary>
    /// 预加载指定数量的伤害数字对象
    /// </summary>
    /// <param name="count">预加载数量</param>
    public void PreloadDamageNumbers(int count)
    {
        if (damageNumberPool != null)
        {
            damageNumberPool.Preload(count);
            Debug.Log($"[DamageNumberPool] 预加载了 {count} 个伤害数字对象");
        }
    }
    
    /// <summary>
    /// 清空对象池
    /// </summary>
    public void ClearPool()
    {
        if (damageNumberPool != null)
        {
            damageNumberPool.Clear();
            Debug.Log("[DamageNumberPool] 对象池已清空");
        }
    }
    
    /// <summary>
    /// 获取池统计信息
    /// </summary>
    public string GetPoolStats()
    {
        if (damageNumberPool != null)
        {
            return damageNumberPool.GetPoolStats();
        }
        return "DamageNumberPool: 未初始化";
    }
    
    /// <summary>
    /// 验证池完整性
    /// </summary>
    public bool ValidatePool()
    {
        if (damageNumberPool != null)
        {
            return damageNumberPool.ValidatePool();
        }
        return false;
    }
    
    /// <summary>
    /// 设置预制体
    /// </summary>
    public void SetDamageNumberPrefab(GameObject prefab)
    {
        if (prefab != null && prefab.GetComponent<DamageNumber>() != null)
        {
            damageNumberPrefab = prefab;
            Debug.Log("[DamageNumberPool] 伤害数字预制体已更新");
        }
        else
        {
            Debug.LogError("[DamageNumberPool] 无效的预制体：缺少DamageNumber组件");
        }
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        
        // 清空池
        ClearPool();
    }
    
    // 调试方法
    [ContextMenu("显示池状态")]
    private void ShowPoolStatus()
    {
        Debug.Log($"[DamageNumberPool] {GetPoolStats()}");
    }
    
    [ContextMenu("验证池完整性")]
    private void ValidatePoolIntegrity()
    {
        bool isValid = ValidatePool();
        Debug.Log($"[DamageNumberPool] 池完整性验证: {(isValid ? "通过" : "失败")}");
    }
}
