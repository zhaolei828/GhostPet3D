using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 玩家血量条UI - 显示和更新玩家血量
/// </summary>
public class PlayerHealthBar : MonoBehaviour
{
    [Header("血量条组件")]
    [SerializeField] private Slider healthSlider; // 血量滑动条
    [SerializeField] private Image fillImage; // 填充图像
    [SerializeField] private TextMeshProUGUI healthText; // 血量文本
    [SerializeField] private GameObject healthBarContainer; // 血量条容器
    
    [Header("视觉效果")]
    [SerializeField] private Color fullHealthColor = Color.green; // 满血颜色
    [SerializeField] private Color lowHealthColor = Color.red; // 低血颜色
    [SerializeField] private float lowHealthThreshold = 0.3f; // 低血阈值
    [SerializeField] private bool showNumbers = true; // 是否显示数字
    [SerializeField] private bool enablePulseEffect = true; // 低血时是否闪烁
    
    [Header("动画设置")]
    [SerializeField] private float updateSpeed = 2f; // 血量条更新速度
    [SerializeField] private float pulseSpeed = 2f; // 闪烁速度
    
    private float currentHealth;
    private float maxHealth;
    private float targetHealthPercent;
    private float currentHealthPercent;
    
    // 动画相关
    private bool isLowHealth = false;
    private float pulseTimer = 0f;
    
    private void Awake()
    {
        // 自动获取组件
        if (healthSlider == null)
            healthSlider = GetComponentInChildren<Slider>();
        
        if (fillImage == null && healthSlider != null && healthSlider.fillRect != null)
            fillImage = healthSlider.fillRect.GetComponent<Image>();
        
        if (healthText == null)
            healthText = GetComponentInChildren<TextMeshProUGUI>();
        
        if (healthBarContainer == null)
            healthBarContainer = gameObject;
    }
    
    private void Update()
    {
        // 平滑更新血量条
        SmoothUpdateHealthBar();
        
        // 处理低血闪烁效果
        if (enablePulseEffect && isLowHealth)
        {
            HandlePulseEffect();
        }
    }
    
    /// <summary>
    /// 初始化血量条
    /// </summary>
    public void Initialize()
    {
        // 查找玩家的血量系统
        FindPlayerHealthSystem();
        
        // 设置初始状态
        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            healthSlider.value = 1f;
        }
        
        // 设置初始颜色
        UpdateHealthBarColor(1f);
        
        Debug.Log("玩家血量条已初始化");
    }
    
    /// <summary>
    /// 查找玩家血量系统
    /// </summary>
    private void FindPlayerHealthSystem()
    {
        // 查找玩家对象
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            player = GameObject.Find("Player");
        }
        
        if (player != null)
        {
            HealthSystem playerHealth = player.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                // 获取初始血量
                maxHealth = playerHealth.MaxHealth;
                currentHealth = playerHealth.CurrentHealth;
                targetHealthPercent = currentHealth / maxHealth;
                currentHealthPercent = targetHealthPercent;
                
                // 订阅血量变化事件（如果有的话）
                // playerHealth.OnHealthChanged += UpdateHealth;
                
                Debug.Log($"找到玩家血量系统：当前血量 {currentHealth}/{maxHealth}");
            }
        }
        else
        {
            Debug.LogWarning("未找到玩家对象，血量条将使用默认值");
            maxHealth = 100f;
            currentHealth = 100f;
            targetHealthPercent = 1f;
            currentHealthPercent = 1f;
        }
    }
    
    /// <summary>
    /// 更新血量显示
    /// </summary>
    public void UpdateHealth(float newCurrentHealth, float newMaxHealth)
    {
        currentHealth = newCurrentHealth;
        maxHealth = newMaxHealth;
        targetHealthPercent = Mathf.Clamp01(currentHealth / maxHealth);
        
        // 检查是否为低血状态
        isLowHealth = targetHealthPercent <= lowHealthThreshold;
        
        // 立即更新文本显示
        UpdateHealthText();
        
        // 如果有填充图像，更新颜色
        if (fillImage != null)
        {
            UpdateHealthBarColor(targetHealthPercent);
        }
        
        Debug.Log($"血量更新：{currentHealth:F0}/{maxHealth:F0} ({targetHealthPercent * 100:F0}%)");
    }
    
    /// <summary>
    /// 平滑更新血量条
    /// </summary>
    private void SmoothUpdateHealthBar()
    {
        if (healthSlider == null) return;
        
        // 平滑插值到目标值
        currentHealthPercent = Mathf.Lerp(currentHealthPercent, targetHealthPercent, updateSpeed * Time.deltaTime);
        healthSlider.value = currentHealthPercent;
        
        // 更新颜色
        UpdateHealthBarColor(currentHealthPercent);
    }
    
    /// <summary>
    /// 更新血量条颜色
    /// </summary>
    private void UpdateHealthBarColor(float healthPercent)
    {
        if (fillImage == null) return;
        
        // 根据血量百分比插值颜色
        Color targetColor = Color.Lerp(lowHealthColor, fullHealthColor, healthPercent);
        fillImage.color = targetColor;
    }
    
    /// <summary>
    /// 更新血量文本
    /// </summary>
    private void UpdateHealthText()
    {
        if (healthText == null || !showNumbers) return;
        
        healthText.text = $"HP: {currentHealth:F0}/{maxHealth:F0}";
    }
    
    /// <summary>
    /// 处理低血闪烁效果
    /// </summary>
    private void HandlePulseEffect()
    {
        if (fillImage == null) return;
        
        pulseTimer += Time.deltaTime * pulseSpeed;
        float alpha = 0.5f + 0.5f * Mathf.Sin(pulseTimer);
        
        Color currentColor = fillImage.color;
        currentColor.a = alpha;
        fillImage.color = currentColor;
    }
    
    /// <summary>
    /// 设置血量条可见性
    /// </summary>
    public void SetVisible(bool visible)
    {
        if (healthBarContainer != null)
        {
            healthBarContainer.SetActive(visible);
        }
    }
    
    /// <summary>
    /// 重置血量条到满血状态
    /// </summary>
    public void ResetToFullHealth()
    {
        UpdateHealth(maxHealth, maxHealth);
    }
    
    /// <summary>
    /// 获取当前血量百分比
    /// </summary>
    public float GetHealthPercent()
    {
        return targetHealthPercent;
    }
    
    /// <summary>
    /// 设置血量条位置和大小（注意：UI元素应该使用RectTransform.anchoredPosition而不是世界坐标）
    /// </summary>
    public void SetPosition(Vector3 position)
    {
        if (healthBarContainer != null)
        {
            RectTransform rectTransform = healthBarContainer.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // 对于Screen Space - Overlay Canvas，使用anchoredPosition
                rectTransform.anchoredPosition = new Vector2(position.x, position.y);
            }
            else
            {
                // 如果不是UI元素，则使用世界坐标（这种情况下可能是World Space Canvas）
                healthBarContainer.transform.position = position;
            }
        }
    }
    
    private void OnDestroy()
    {
        // 取消事件订阅
        // if (playerHealthSystem != null)
        //     playerHealthSystem.OnHealthChanged -= UpdateHealth;
    }
}
