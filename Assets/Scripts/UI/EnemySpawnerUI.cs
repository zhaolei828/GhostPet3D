using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 敌人生成器UI显示 - 用于调试和监控
/// </summary>
public class EnemySpawnerUI : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private Text statusText;
    [SerializeField] private Button clearEnemiesButton;
    [SerializeField] private Button increaseSpawnButton;
    [SerializeField] private Button decreaseSpawnButton;
    
    private void Start()
    {
        // 设置按钮事件
        if (clearEnemiesButton != null)
        {
            clearEnemiesButton.onClick.AddListener(ClearAllEnemies);
        }
        
        if (increaseSpawnButton != null)
        {
            increaseSpawnButton.onClick.AddListener(IncreaseSpawnRate);
        }
        
        if (decreaseSpawnButton != null)
        {
            decreaseSpawnButton.onClick.AddListener(DecreaseSpawnRate);
        }
    }
    
    private void Update()
    {
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        if (statusText != null && EnemySpawner.Instance != null)
        {
            statusText.text = EnemySpawner.Instance.GetSpawnStats();
        }
        else if (statusText != null)
        {
            statusText.text = "EnemySpawner未激活";
        }
    }
    
    private void ClearAllEnemies()
    {
        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.ClearAllEnemies();
        }
    }
    
    private void IncreaseSpawnRate()
    {
        Debug.Log("[EnemySpawnerUI] 增加生成速度");
        // 可以添加调整生成速度的逻辑
    }
    
    private void DecreaseSpawnRate()
    {
        Debug.Log("[EnemySpawnerUI] 减少生成速度");
        // 可以添加调整生成速度的逻辑
    }
}
