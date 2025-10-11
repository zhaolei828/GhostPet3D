using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 游戏结束UI - 显示游戏结束信息和操作选项
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("UI元素引用")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;
    
    [Header("游戏结束文本")]
    [SerializeField] private string gameOverMessage = "游戏结束";
    [SerializeField] private string scoreFormat = "最终分数: {0}";
    
    // 单例
    public static GameOverUI Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // 确保面板初始状态为隐藏
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        // 设置按钮事件
        SetupButtons();
    }
    
    private void SetupButtons()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }
    }
    
    /// <summary>
    /// 显示游戏结束UI
    /// </summary>
    public void Show(int finalScore = 0)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        // 显示游戏结束文本
        if (gameOverText != null)
        {
            gameOverText.text = gameOverMessage;
        }
        
        // 显示分数
        if (scoreText != null)
        {
            scoreText.text = string.Format(scoreFormat, finalScore);
        }
        
        Debug.Log($"[GameOverUI] 显示游戏结束界面，最终分数: {finalScore}");
    }
    
    /// <summary>
    /// 隐藏游戏结束UI
    /// </summary>
    public void Hide()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        Debug.Log("[GameOverUI] 隐藏游戏结束界面");
    }
    
    /// <summary>
    /// 重新开始按钮点击事件
    /// </summary>
    private void OnRestartClicked()
    {
        Debug.Log("[GameOverUI] 重新开始按钮点击");
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }
    
    /// <summary>
    /// 退出按钮点击事件
    /// </summary>
    private void OnQuitClicked()
    {
        Debug.Log("[GameOverUI] 退出按钮点击");
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
    }
    
    private void OnDestroy()
    {
        // 清理按钮事件
        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(OnRestartClicked);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(OnQuitClicked);
        }
    }
}

