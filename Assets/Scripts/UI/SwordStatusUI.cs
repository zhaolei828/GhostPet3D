using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 飞剑状态UI - 显示飞剑的当前状态和数量
/// </summary>
public class SwordStatusUI : MonoBehaviour
{
    [Header("飞剑状态显示")]
    [SerializeField] private TextMeshProUGUI swordCountText; // 飞剑数量文本
    [SerializeField] private Transform swordIconContainer; // 飞剑图标容器
    [SerializeField] private GameObject swordIconPrefab; // 飞剑图标预制体
    [SerializeField] private GameObject statusContainer; // 状态容器
    
    [Header("图标设置")]
    [SerializeField] private Color availableColor = Color.cyan; // 可用飞剑颜色
    [SerializeField] private Color attackingColor = Color.red; // 攻击中飞剑颜色
    [SerializeField] private Color returningColor = Color.yellow; // 返回中飞剑颜色
    [SerializeField] private float iconSize = 30f; // 图标大小
    [SerializeField] private float iconSpacing = 35f; // 图标间距
    
    [Header("动画效果")]
    [SerializeField] private bool enablePulseEffect = true; // 启用脉冲效果
    [SerializeField] private float pulseSpeed = 2f; // 脉冲速度
    [SerializeField] private float pulseScale = 1.2f; // 脉冲缩放
    
    // 飞剑状态数据
    private int totalSwords = 6;
    private int availableSwords = 6;
    private int attackingSwords = 0;
    private int returningSwords = 0;
    
    // UI元素列表
    private List<SwordIcon> swordIcons = new List<SwordIcon>();
    
    private void Awake()
    {
        // 自动查找组件
        if (statusContainer == null)
            statusContainer = gameObject;
        
        if (swordIconContainer == null)
            swordIconContainer = transform;
        
        if (swordCountText == null)
            swordCountText = GetComponentInChildren<TextMeshProUGUI>();
    }
    
    /// <summary>
    /// 初始化飞剑状态UI
    /// </summary>
    public void Initialize()
    {
        // 查找飞剑管理器获取初始数据
        FindSwordManager();
        
        // 创建飞剑图标
        CreateSwordIcons();
        
        // 更新显示
        UpdateAllDisplays();
        
        Debug.Log("飞剑状态UI已初始化");
    }
    
    /// <summary>
    /// 查找飞剑管理器
    /// </summary>
    private void FindSwordManager()
    {
        FlyingSwordManager3D swordManager = FindFirstObjectByType<FlyingSwordManager3D>();
        if (swordManager != null)
        {
            // 获取飞剑总数（如果管理器有这个信息）
            // 这里假设有6把飞剑，实际可以从管理器获取
            totalSwords = 6;
            availableSwords = totalSwords;
            attackingSwords = 0;
            returningSwords = 0;
            
            Debug.Log($"找到飞剑管理器，总飞剑数：{totalSwords}");
        }
        else
        {
            Debug.LogWarning("未找到飞剑管理器，使用默认值");
        }
    }
    
    /// <summary>
    /// 创建飞剑图标
    /// </summary>
    private void CreateSwordIcons()
    {
        // 清除现有图标
        ClearExistingIcons();
        
        // 创建新图标
        for (int i = 0; i < totalSwords; i++)
        {
            GameObject iconObject = CreateSwordIcon(i);
            SwordIcon swordIcon = iconObject.GetComponent<SwordIcon>();
            
            if (swordIcon == null)
            {
                swordIcon = iconObject.AddComponent<SwordIcon>();
            }
            
            swordIcon.Initialize(i, iconSize, availableColor);
            swordIcons.Add(swordIcon);
        }
        
        // 设置图标布局
        LayoutIcons();
    }
    
    /// <summary>
    /// 清除现有图标
    /// </summary>
    private void ClearExistingIcons()
    {
        foreach (var icon in swordIcons)
        {
            if (icon != null && icon.gameObject != null)
            {
                DestroyImmediate(icon.gameObject);
            }
        }
        swordIcons.Clear();
    }
    
    /// <summary>
    /// 创建单个飞剑图标
    /// </summary>
    private GameObject CreateSwordIcon(int index)
    {
        GameObject iconObject;
        
        if (swordIconPrefab != null)
        {
            iconObject = Instantiate(swordIconPrefab, swordIconContainer);
        }
        else
        {
            // 创建简单的图标
            iconObject = new GameObject($"SwordIcon_{index}");
            iconObject.transform.SetParent(swordIconContainer);
            
            // 添加Image组件
            Image iconImage = iconObject.AddComponent<Image>();
            iconImage.sprite = CreateSimpleSwordSprite();
            iconImage.color = availableColor;
        }
        
        iconObject.name = $"SwordIcon_{index}";
        return iconObject;
    }
    
    /// <summary>
    /// 创建简单的飞剑精灵（如果没有预制体）
    /// </summary>
    private Sprite CreateSimpleSwordSprite()
    {
        // 创建一个1x1的白色纹理
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        
        // 从纹理创建Sprite
        Rect rect = new Rect(0, 0, 1, 1);
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        return Sprite.Create(texture, rect, pivot);
    }
    
    /// <summary>
    /// 设置图标布局
    /// </summary>
    private void LayoutIcons()
    {
        for (int i = 0; i < swordIcons.Count; i++)
        {
            RectTransform rectTransform = swordIcons[i].GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // 水平排列
                float xPos = i * iconSpacing - (totalSwords - 1) * iconSpacing * 0.5f;
                rectTransform.anchoredPosition = new Vector2(xPos, 0);
                rectTransform.sizeDelta = new Vector2(iconSize, iconSize);
            }
        }
    }
    
    /// <summary>
    /// 更新飞剑状态
    /// </summary>
    public void UpdateStatus(int available, int attacking, int returning = 0)
    {
        availableSwords = available;
        attackingSwords = attacking;
        returningSwords = returning;
        
        UpdateAllDisplays();
    }
    
    /// <summary>
    /// 更新所有显示
    /// </summary>
    private void UpdateAllDisplays()
    {
        UpdateCountText();
        UpdateIconStates();
    }
    
    /// <summary>
    /// 更新数量文本
    /// </summary>
    private void UpdateCountText()
    {
        // 确保组件只被查找一次，避免在每次更新时重复查找
        if (swordCountText == null)
            swordCountText = GetComponentInChildren<TextMeshProUGUI>();
            
        if (swordCountText != null)
        {
            swordCountText.text = $"Swords: {availableSwords}/{totalSwords}";
            
            if (attackingSwords > 0)
            {
                swordCountText.text += $" (Attacking: {attackingSwords})";
            }
        }
    }
    
    /// <summary>
    /// 更新图标状态
    /// </summary>
    private void UpdateIconStates()
    {
        for (int i = 0; i < swordIcons.Count; i++)
        {
            if (swordIcons[i] != null)
            {
                SwordState state;
                Color color;
                
                if (i < attackingSwords)
                {
                    state = SwordState.Attacking;
                    color = attackingColor;
                }
                else if (i < attackingSwords + returningSwords)
                {
                    state = SwordState.Returning;
                    color = returningColor;
                }
                else
                {
                    state = SwordState.Available;
                    color = availableColor;
                }
                
                swordIcons[i].SetState(state, color, enablePulseEffect, pulseSpeed, pulseScale);
            }
        }
    }
    
    /// <summary>
    /// 设置状态UI可见性
    /// </summary>
    public void SetVisible(bool visible)
    {
        if (statusContainer != null)
        {
            statusContainer.SetActive(visible);
        }
    }
    
    /// <summary>
    /// 飞剑状态枚举
    /// </summary>
    public enum SwordState
    {
        Available,  // 可用
        Attacking,  // 攻击中
        Returning   // 返回中
    }
}

/// <summary>
/// 单个飞剑图标组件
/// </summary>
public class SwordIcon : MonoBehaviour
{
    private Image iconImage;
    private RectTransform rectTransform;
    private SwordStatusUI.SwordState currentState;
    private bool enablePulse;
    private float pulseTimer;
    private float currentPulseSpeed = 2f;
    private float currentPulseScale = 1.2f;
    
    public void Initialize(int index, float size, Color color)
    {
        iconImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        
        if (iconImage != null)
        {
            iconImage.color = color;
        }
        
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(size, size);
        }
    }
    
    public void SetState(SwordStatusUI.SwordState state, Color color, bool pulse, float pulseSpeed = 2f, float pulseScale = 1.2f)
    {
        currentState = state;
        enablePulse = pulse && state == SwordStatusUI.SwordState.Attacking;
        currentPulseSpeed = pulseSpeed;
        currentPulseScale = pulseScale;
        
        if (iconImage != null)
        {
            iconImage.color = color;
        }
    }
    
    private void Update()
    {
        if (enablePulse && currentState == SwordStatusUI.SwordState.Attacking)
        {
            pulseTimer += Time.deltaTime * currentPulseSpeed;
            float scale = 1f + (currentPulseScale - 1f) * Mathf.Sin(pulseTimer);
            transform.localScale = Vector3.one * scale;
        }
        else
        {
            transform.localScale = Vector3.one;
        }
    }
}
