using UnityEngine;
using TMPro;

/// <summary>
/// 分数UI系统 - 显示击杀数、生存时间和总分
/// </summary>
public class ScoreUI : MonoBehaviour
{
    [Header("分数显示组件")]
    [SerializeField] private TextMeshProUGUI killCountText; // 击杀数文本
    [SerializeField] private GameObject scoreContainer; // 分数容器
    
    [Header("分数计算设置")]
    [SerializeField] private int pointsPerKill = 100; // 每击杀一个敌人的分数
    [SerializeField] private int pointsPerSecond = 10; // 每秒生存的分数
    
    [Header("视觉效果")]
    [SerializeField] private Color normalColor = Color.white; // 正常颜色
    [SerializeField] private Color highlightColor = Color.yellow; // 高亮颜色（分数增加时）
    [SerializeField] private float highlightDuration = 0.5f; // 高亮持续时间
    
    // 分数数据
    private int currentKills = 0;
    private float currentSurvivalTime = 0f;
    private int totalScore = 0;
    private float gameStartTime;
    
    // 动画效果
    private float killHighlightTimer = 0f;
    private float scoreHighlightTimer = 0f;
    
    private void Awake()
    {
        // 自动查找组件
        if (scoreContainer == null)
            scoreContainer = gameObject;
        
        // 如果没有指定文本组件，尝试自动查找
        FindTextComponents();
    }
    
    private void Update()
    {
        // 更新生存时间
        UpdateSurvivalTime();
        
        // 更新高亮效果
        UpdateHighlightEffects();
    }
    
    /// <summary>
    /// 初始化分数UI
    /// </summary>
    public void Initialize()
    {
        gameStartTime = Time.time;
        currentKills = 0;
        currentSurvivalTime = 0f;
        totalScore = 0;
        
        // 更新显示
        UpdateAllDisplays();
        
        Debug.Log("分数UI已初始化");
    }
    
    /// <summary>
    /// 查找文本组件
    /// </summary>
    private void FindTextComponents()
    {
        // 使用统一的文本组件显示所有信息
        if (killCountText == null)
        {
            killCountText = GetComponentInChildren<TextMeshProUGUI>();
        }
    }
    
    /// <summary>
    /// 更新生存时间
    /// </summary>
    private void UpdateSurvivalTime()
    {
        currentSurvivalTime = Time.time - gameStartTime;
        
        // 更新生存时间显示
        UpdateSurvivalTimeDisplay();
        
        // 每秒更新一次总分
        if (Time.time % 1f < Time.deltaTime)
        {
            RecalculateTotalScore();
        }
    }
    
    /// <summary>
    /// 增加击杀数
    /// </summary>
    public void AddKill()
    {
        currentKills++;
        killHighlightTimer = highlightDuration;
        
        // 重新计算总分
        RecalculateTotalScore();
        
        // 触发高亮效果
        scoreHighlightTimer = highlightDuration;
        
        // 更新显示
        UpdateKillDisplay();
        UpdateTotalScoreDisplay();
        
        Debug.Log($"击杀数增加！当前击杀：{currentKills}，总分：{totalScore}");
    }
    
    /// <summary>
    /// 更新分数显示
    /// </summary>
    public void UpdateScore(int kills, float survivalTime)
    {
        currentKills = kills;
        currentSurvivalTime = survivalTime;
        
        RecalculateTotalScore();
        UpdateAllDisplays();
    }
    
    /// <summary>
    /// 重新计算总分
    /// </summary>
    private void RecalculateTotalScore()
    {
        int killScore = currentKills * pointsPerKill;
        int timeScore = Mathf.FloorToInt(currentSurvivalTime) * pointsPerSecond;
        totalScore = killScore + timeScore;
    }
    
    /// <summary>
    /// 更新所有显示
    /// </summary>
    private void UpdateAllDisplays()
    {
        if (killCountText != null)
        {
            int minutes = Mathf.FloorToInt(currentSurvivalTime / 60);
            int seconds = Mathf.FloorToInt(currentSurvivalTime % 60);
            killCountText.text = $"Kills: {currentKills}  Time: {minutes:00}:{seconds:00}  Score: {totalScore:N0}";
        }
    }
    
    /// <summary>
    /// 更新击杀数显示（已合并到UpdateAllDisplays中）
    /// </summary>
    private void UpdateKillDisplay()
    {
        // 该方法现在由UpdateAllDisplays统一处理
        UpdateAllDisplays();
    }
    
    /// <summary>
    /// 更新生存时间显示（已合并到UpdateAllDisplays中）
    /// </summary>
    private void UpdateSurvivalTimeDisplay()
    {
        // 该方法现在由UpdateAllDisplays统一处理，避免重复设置
    }
    
    /// <summary>
    /// 更新总分显示（已合并到UpdateAllDisplays中）
    /// </summary>
    private void UpdateTotalScoreDisplay()
    {
        // 该方法现在由UpdateAllDisplays统一处理
        UpdateAllDisplays();
    }
    
    /// <summary>
    /// 更新高亮效果
    /// </summary>
    private void UpdateHighlightEffects()
    {
        // 统一高亮效果 - 击杀或分数变化时都高亮主文本
        if (killHighlightTimer > 0 || scoreHighlightTimer > 0)
        {
            float maxTimer = Mathf.Max(killHighlightTimer, scoreHighlightTimer);
            
            killHighlightTimer -= Time.deltaTime;
            scoreHighlightTimer -= Time.deltaTime;
            
            if (killCountText != null)
            {
                killCountText.color = Color.Lerp(normalColor, highlightColor, maxTimer / highlightDuration);
            }
        }
        else if (killCountText != null)
        {
            killCountText.color = normalColor;
        }
    }
    
    /// <summary>
    /// 设置分数UI可见性
    /// </summary>
    public void SetVisible(bool visible)
    {
        if (scoreContainer != null)
        {
            scoreContainer.SetActive(visible);
        }
    }
    
    /// <summary>
    /// 重置分数
    /// </summary>
    public void ResetScore()
    {
        currentKills = 0;
        currentSurvivalTime = 0f;
        totalScore = 0;
        gameStartTime = Time.time;
        
        UpdateAllDisplays();
        
        Debug.Log("分数已重置");
    }
    
    /// <summary>
    /// 获取当前分数数据
    /// </summary>
    public ScoreData GetCurrentScore()
    {
        return new ScoreData
        {
            kills = currentKills,
            survivalTime = currentSurvivalTime,
            totalScore = totalScore
        };
    }
    
    /// <summary>
    /// 分数数据结构
    /// </summary>
    [System.Serializable]
    public struct ScoreData
    {
        public int kills;
        public float survivalTime;
        public int totalScore;
    }
}
