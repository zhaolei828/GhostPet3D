using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

/// <summary>
/// UI管理器 - 基础版本，专注于核心UI功能和稳定性
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI面板引用")]
    [SerializeField] private Canvas gameUICanvas;
    [SerializeField] private GameObject gameUIPanel;
    [SerializeField] private GameObject pauseMenuPanel;
    
    [Header("玩家UI")]
    [SerializeField] private PlayerHealthBar playerHealthBar;
    [SerializeField] private ScoreUI scoreUI;
    [SerializeField] private SwordStatusUI swordStatusUI;
    
    [Header("设置")]
    [SerializeField] private bool showDebugInfo = false;
    [SerializeField] private TextMeshProUGUI debugText;
    
    // 单例模式
    public static UIManager Instance { get; private set; }
    
    // 事件
    public static System.Action OnGamePaused;
    public static System.Action OnGameResumed;
    
    private bool isPaused = false;
    
    private void Awake()
    {
        // 单例模式实现
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 自动查找UI组件
            AutoFindUIComponents();
        }
        else
        {
            // 运行时安全地处理重复实例
            if (Application.isPlaying)
            {
                Debug.LogWarning($"[UIManager] 检测到重复的UIManager实例: {gameObject.name}，将禁用此实例");
                gameObject.SetActive(false);
                return;
            }
            else
            {
                // 编辑器模式下可以安全销毁
                DestroyImmediate(gameObject);
                return;
            }
        }
        
        // 确保Canvas设置正确
        SetupCanvas();
        
        // 初始化UI状态
        InitializeUI();
    }
    
    private void Start()
    {
        // 订阅事件
        SubscribeToEvents();
        
        // 设置UI锚点
        SetupUIAnchors();
        
        // 显示游戏UI
        ShowGameUI();
    }
    
    private void Update()
    {
        if (showDebugInfo && debugText != null)
        {
            UpdateDebugInfo();
        }
    }
    
    /// <summary>
    /// 设置UI元素的锚点和位置 - 简化版本
    /// </summary>
    public void SetupUIAnchors()
    {
        Debug.Log("[UIManager_Test] 开始设置UI锚点...");
        
        // 设置血量条位置（左上角）
        if (playerHealthBar != null)
        {
            RectTransform healthBarRect = playerHealthBar.GetComponent<RectTransform>();
            if (healthBarRect != null)
            {
                healthBarRect.anchorMin = new Vector2(0, 1); // 左上
                healthBarRect.anchorMax = new Vector2(0, 1); // 左上
                healthBarRect.anchoredPosition = new Vector2(100, -50);
                healthBarRect.sizeDelta = new Vector2(250, 40);
                Debug.Log($"[UIManager_Test] 血量条位置设置为: {healthBarRect.anchoredPosition}");
            }
        }
        
        // 设置分数UI位置（右上角）
        if (scoreUI != null)
        {
            RectTransform scoreRect = scoreUI.GetComponent<RectTransform>();
            if (scoreRect != null)
            {
                scoreRect.anchorMin = new Vector2(1, 1); // 右上
                scoreRect.anchorMax = new Vector2(1, 1); // 右上
                scoreRect.anchoredPosition = new Vector2(-100, -50);
                scoreRect.sizeDelta = new Vector2(300, 40);
                Debug.Log($"[UIManager_Test] 分数UI位置设置为: {scoreRect.anchoredPosition}");
            }
        }
        
        // 设置飞剑状态UI位置（底部中心）
        if (swordStatusUI != null)
        {
            RectTransform swordRect = swordStatusUI.GetComponent<RectTransform>();
            if (swordRect != null)
            {
                swordRect.anchorMin = new Vector2(0.5f, 0); // 底部中心
                swordRect.anchorMax = new Vector2(0.5f, 0); // 底部中心
                swordRect.anchoredPosition = new Vector2(0, 80);
                swordRect.sizeDelta = new Vector2(250, 50);
                Debug.Log($"[UIManager_Test] 飞剑状态UI位置设置为: {swordRect.anchoredPosition}");
            }
        }
        
        Debug.Log("[UIManager_Test] UI锚点设置完成");
    }
    
    /// <summary>
    /// 初始化UI
    /// </summary>
    private void InitializeUI()
    {
        // 自动查找UI组件（如果引用为空）
        AutoFindUIComponents();
        
        // 默认显示游戏UI，隐藏暂停菜单
        if (gameUIPanel != null)
            gameUIPanel.SetActive(true);
        
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        
        // 初始化各个UI组件
        if (playerHealthBar != null)
            playerHealthBar.Initialize();
        
        if (scoreUI != null)
            scoreUI.Initialize();
        
        if (swordStatusUI != null)
            swordStatusUI.Initialize();
            
        Debug.Log("[UIManager_Test] UI初始化完成");
    }
    
    /// <summary>
    /// 自动查找UI组件
    /// </summary>
    private void AutoFindUIComponents()
    {
        Debug.Log("[UIManager_Test] 开始自动查找UI组件...");
        
        // 如果Canvas为空，尝试获取自身组件
        if (gameUICanvas == null)
        {
            gameUICanvas = GetComponent<Canvas>();
            Debug.Log($"[UIManager_Test] 自动找到Canvas: {gameUICanvas != null}");
        }
        
        // 查找PlayerHealthBar
        if (playerHealthBar == null)
        {
            playerHealthBar = FindFirstObjectByType<PlayerHealthBar>();
            Debug.Log($"[UIManager_Test] 自动找到PlayerHealthBar: {playerHealthBar != null}");
        }
        
        // 查找ScoreUI
        if (scoreUI == null)
        {
            scoreUI = FindFirstObjectByType<ScoreUI>();
            Debug.Log($"[UIManager_Test] 自动找到ScoreUI: {scoreUI != null}");
        }
        
        // 查找SwordStatusUI
        if (swordStatusUI == null)
        {
            swordStatusUI = FindFirstObjectByType<SwordStatusUI>();
            Debug.Log($"[UIManager_Test] 自动找到SwordStatusUI: {swordStatusUI != null}");
        }
        
        Debug.Log("[UIManager_Test] UI组件查找完成！");
    }
    
    /// <summary>
    /// 设置Canvas
    /// </summary>
    private void SetupCanvas()
    {
        if (gameUICanvas == null)
        {
            gameUICanvas = GetComponent<Canvas>();
        }
        
        if (gameUICanvas != null)
        {
            gameUICanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            gameUICanvas.sortingOrder = 0;
            
            // 确保CanvasScaler正确设置
            CanvasScaler scaler = gameUICanvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }
        }
    }
    
    /// <summary>
    /// 订阅游戏事件
    /// </summary>
    private void SubscribeToEvents()
    {
        // 这里可以订阅游戏事件
        Debug.Log("[UIManager_Test] 已订阅游戏事件");
    }
    
    /// <summary>
    /// 显示游戏UI
    /// </summary>
    public void ShowGameUI()
    {
        if (gameUIPanel != null)
        {
            gameUIPanel.SetActive(true);
        }
        
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        
        isPaused = false;
    }
    
    /// <summary>
    /// 显示暂停菜单
    /// </summary>
    public void ShowPauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }
        
        if (gameUIPanel != null)
        {
            gameUIPanel.SetActive(false);
        }
        
        isPaused = true;
        OnGamePaused?.Invoke();
    }
    
    /// <summary>
    /// 更新击杀数
    /// </summary>
    public void AddKill()
    {
        if (scoreUI != null)
        {
            scoreUI.AddKill();
        }
    }
    
    /// <summary>
    /// 更新调试信息
    /// </summary>
    private void UpdateDebugInfo()
    {
        if (debugText != null)
        {
            debugText.text = $"UI调试信息:\\n" +
                           $"血量条: {(playerHealthBar != null ? "√" : "×")}\\n" +
                           $"分数UI: {(scoreUI != null ? "√" : "×")}\\n" +
                           $"飞剑UI: {(swordStatusUI != null ? "√" : "×")}\\n" +
                           $"暂停状态: {isPaused}";
        }
    }
    
    /// <summary>
    /// 测试UI位置稳定性
    /// </summary>
    [ContextMenu("测试UI稳定性")]
    public void TestUIStability()
    {
        Debug.Log("[UIManager_Test] 开始UI稳定性测试...");
        
        // 重新设置锚点
        SetupUIAnchors();
        
        // 等待一帧后检查位置
        StartCoroutine(CheckUIPositions());
    }
    
    private System.Collections.IEnumerator CheckUIPositions()
    {
        yield return new WaitForEndOfFrame();
        
        // 检查血量条位置
        if (playerHealthBar != null)
        {
            RectTransform rect = playerHealthBar.GetComponent<RectTransform>();
            if (rect != null)
            {
                Vector2 expectedPos = new Vector2(100, -50);
                float distance = Vector2.Distance(rect.anchoredPosition, expectedPos);
                Debug.Log($"[UIManager_Test] 血量条位置偏差: {distance}px (期望: {expectedPos}, 实际: {rect.anchoredPosition})");
            }
        }
        
        Debug.Log("[UIManager_Test] UI稳定性测试完成");
    }
}
