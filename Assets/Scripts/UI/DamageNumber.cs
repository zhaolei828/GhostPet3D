using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// 浮动伤害数字 - 显示受到的伤害值
/// </summary>
public class DamageNumber : MonoBehaviour
{
    [Header("显示设置")]
    [SerializeField] private TextMeshProUGUI damageText;      // 伤害文本组件（UI版本）
    [SerializeField] private float lifetime = 1.5f;          // 显示持续时间
    [SerializeField] private float moveSpeed = 2f;           // 上升速度
    [SerializeField] private float fadeSpeed = 1f;           // 淡出速度
    
    [Header("颜色设置")]
    [SerializeField] private Color playerDamageColor = Color.red;     // 玩家受伤颜色
    [SerializeField] private Color enemyDamageColor = Color.yellow;   // 敌人受伤颜色
    [SerializeField] private Color healColor = Color.green;           // 治疗颜色
    
    [Header("动画设置")]
    [SerializeField] private float scaleStartSize = 1.2f;    // 起始缩放
    [SerializeField] private float scaleEndSize = 1f;        // 结束缩放
    [SerializeField] private float scaleTime = 0.2f;         // 缩放时间
    
    private Vector3 moveDirection;
    private float timer = 0f;
    private Color originalColor;
    private Vector3 originalScale;
    
    private void Awake()
    {
        // 如果没有指定文本组件，尝试在子对象中查找，避免动态创建
        if (damageText == null)
        {
            damageText = GetComponentInChildren<TextMeshProUGUI>();
            if (damageText == null)
            {
                // 如果确实没有找到，记录警告但不创建，避免内存泄漏
                Debug.LogWarning($"DamageNumber: 未找到TextMeshProUGUI组件在 {gameObject.name}，伤害数字将不可见");
                enabled = false; // 禁用此组件避免后续错误
                return;
            }
        }
        
        // 确保originalScale有合理的默认值
        originalScale = transform.localScale;
        if (originalScale == Vector3.zero)
        {
            originalScale = Vector3.one; // 默认为(1,1,1)
            transform.localScale = originalScale;
        }
        
        // 随机移动方向（主要向上，稍微偏左或偏右）
        moveDirection = new Vector3(
            Random.Range(-0.5f, 0.5f), // X轴随机偏移
            1f,                         // Y轴向上
            0f
        ).normalized;
    }
    
    private void Update()
    {
        // 移动
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
        
        // 淡出和缩放
        timer += Time.deltaTime;
        
        // 缩放动画（开始时稍大，然后缩小）
        if (timer < scaleTime)
        {
            float scaleProgress = timer / scaleTime;
            float currentScale = Mathf.Lerp(scaleStartSize, scaleEndSize, scaleProgress);
            transform.localScale = originalScale * currentScale;
        }
        
        // 淡出效果
        if (timer > lifetime * 0.6f) // 在60%的生命周期后开始淡出
        {
            float fadeProgress = (timer - lifetime * 0.6f) / (lifetime * 0.4f);
            // 应用fadeSpeed来调整淡出速度
            fadeProgress *= fadeSpeed;
            fadeProgress = Mathf.Clamp01(fadeProgress); // 确保不超过1
            
            Color newColor = damageText.color;
            newColor.a = Mathf.Lerp(originalColor.a, 0f, fadeProgress);
            damageText.color = newColor;
        }
        
        // 生命周期结束，销毁对象
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 初始化伤害数字
    /// </summary>
    public void Initialize(float damage, DamageType damageType = DamageType.EnemyDamage, Vector3? customDirection = null)
    {
        // 设置伤害文本
        damageText.text = Mathf.RoundToInt(damage).ToString();
        
        // 设置颜色
        switch (damageType)
        {
            case DamageType.PlayerDamage:
                damageText.color = playerDamageColor;
                break;
            case DamageType.EnemyDamage:
                damageText.color = enemyDamageColor;
                break;
            case DamageType.Healing:
                damageText.color = healColor;
                damageText.text = "+" + damageText.text; // 治疗显示加号
                break;
        }
        
        originalColor = damageText.color;
        
        // 自定义移动方向
        if (customDirection.HasValue)
        {
            moveDirection = customDirection.Value.normalized;
        }
        
        // 根据伤害大小调整字体大小
        float baseFontSize = 3f;
        if (damage > 50f)
        {
            damageText.fontSize = baseFontSize * 1.3f;
        }
        else if (damage > 25f)
        {
            damageText.fontSize = baseFontSize * 1.1f;
        }
        else
        {
            damageText.fontSize = baseFontSize;
        }
        
        // 设置初始缩放
        transform.localScale = originalScale * scaleStartSize;
    }
    
    /// <summary>
    /// 设置自定义文本
    /// </summary>
    public void SetCustomText(string text, Color color)
    {
        damageText.text = text;
        damageText.color = color;
        originalColor = color;
    }
}

/// <summary>
/// 伤害类型枚举
/// </summary>
public enum DamageType
{
    PlayerDamage,  // 玩家受到伤害
    EnemyDamage,   // 敌人受到伤害  
    Healing        // 治疗
}
