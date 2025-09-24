using UnityEngine;
using System;

/// <summary>
/// 血量系统 - 处理角色的生命值逻辑
/// </summary>
public class HealthSystem : MonoBehaviour
{
    [Header("血量设置")]
    [SerializeField] private float maxHealth = 50f;    // 最大血量，敌人默认50血
    [SerializeField] private float currentHealth;       // 当前血量
    [SerializeField] private bool isInvulnerable = false; // 是否无敌状态
    
    // 事件
    public event Action<float, float> OnHealthChanged;   // 血量变化事件 (当前血量, 最大血量)
    public event Action OnDeath;                         // 死亡事件
    public event Action<float> OnDamageTaken;           // 受伤事件 (伤害值)
    public event Action<float> OnHealed;                // 治疗事件 (治疗值)
    
    // 属性
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float HealthPercentage => currentHealth / maxHealth;
    public bool IsAlive => currentHealth > 0;
    public bool IsInvulnerable { get => isInvulnerable; set => isInvulnerable = value; }
    
    private void Awake()
    {
        // 初始化血量为最大值
        currentHealth = maxHealth;
    }
    
    /// <summary>
    /// 受到伤害
    /// </summary>
    /// <param name="damage">伤害值</param>
    /// <param name="source">伤害来源</param>
    public void TakeDamage(float damage, GameObject source = null)
    {
        Debug.Log($"[HealthSystem] TakeDamage called on '{gameObject.name}' for {damage} damage. IsAlive: {IsAlive}, isInvulnerable: {isInvulnerable}.");
        if (damage <= 0 || !IsAlive || isInvulnerable) return;
        
        // 扣除血量
        currentHealth = Mathf.Max(0, currentHealth - damage);
        
        // 显示伤害数字
        ShowDamageNumber(damage);
        
        // 播放受击效果 - 临时禁用，等HitEffect类实现
        // HitEffect hitEffect = GetComponent<HitEffect>();
        // if (hitEffect != null)
        // {
        //     hitEffect.PlayHitEffect(damage);
        // }
        
        // 触发事件
        OnDamageTaken?.Invoke(damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        Debug.Log($"{gameObject.name} 受到 {damage} 点伤害，剩余血量: {currentHealth}");

        // 检查死亡
        if (currentHealth <= 0)
        {
            Debug.Log($"[HealthSystem] {gameObject.name} 血量降至0，即将调用Die()");
            Die();
        }
    }
    
    /// <summary>
    /// 恢复血量
    /// </summary>
    /// <param name="healAmount">治疗值</param>
    public void Heal(float healAmount)
    {
        if (healAmount <= 0 || !IsAlive) return;
        
        float previousHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
        
        float actualHeal = currentHealth - previousHealth;
        
        if (actualHeal > 0)
        {
            // 显示治疗数字
            ShowHealingNumber(actualHeal);
            
            OnHealed?.Invoke(actualHeal);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            Debug.Log($"{gameObject.name} 恢复了 {actualHeal} 点血量，当前血量: {currentHealth}");
        }
    }
    
    /// <summary>
    /// 设置最大血量
    /// </summary>
    /// <param name="newMaxHealth">新的最大血量</param>
    /// <param name="healToFull">是否同时恢复到满血</param>
    public void SetMaxHealth(float newMaxHealth, bool healToFull = false)
    {
        maxHealth = newMaxHealth;
        
        if (healToFull)
        {
            currentHealth = maxHealth;
        }
        else
        {
            // 确保当前血量不超过新的最大值
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    /// <summary>
    /// 立即死亡
    /// </summary>
    public void Die()
    {
        Debug.Log($"[HealthSystem] Die() called on {gameObject.name}, IsAlive: {IsAlive}");
        // 移除IsAlive检查，因为我们想要触发死亡事件即使currentHealth已经为0
        
        currentHealth = 0;
        Debug.Log($"[HealthSystem] Invoking OnDeath event for {gameObject.name}");
        OnDeath?.Invoke();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        Debug.Log($"{gameObject.name} 死亡了！");
    }
    
    /// <summary>
    /// 复活
    /// </summary>
    /// <param name="healthPercentage">复活时的血量百分比 (0-1)</param>
    public void Revive(float healthPercentage = 1f)
    {
        healthPercentage = Mathf.Clamp01(healthPercentage);
        currentHealth = maxHealth * healthPercentage;
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        Debug.Log($"{gameObject.name} 复活了！血量: {currentHealth}");
    }
    
    /// <summary>
    /// 获取血量信息文本
    /// </summary>
    public string GetHealthText()
    {
        return $"{currentHealth:F0}/{maxHealth:F0}";
    }
    
    /// <summary>
    /// 显示伤害数字
    /// </summary>
    private void ShowDamageNumber(float damage)
    {
        Debug.Log($"[HealthSystem] ShowDamageNumber called for {gameObject.name}, damage: {damage}");
        if (DamageNumberManager.Instance != null)
        {
            Debug.Log($"[HealthSystem] DamageNumberManager.Instance found, calling ShowDamageNumber");
            // 判断是玩家还是敌人
            DamageType damageType = gameObject.CompareTag("Player") ? 
                DamageType.PlayerDamage : DamageType.EnemyDamage;
            
            DamageNumberManager.Instance.ShowDamageNumber(transform.position, damage, damageType);
        }
        else
        {
            Debug.LogWarning($"[HealthSystem] DamageNumberManager.Instance is null! Cannot show damage number for {gameObject.name}");
        }
    }
    
    /// <summary>
    /// 显示治疗数字
    /// </summary>
    private void ShowHealingNumber(float healAmount)
    {
        if (DamageNumberManager.Instance != null)
        {
            DamageNumberManager.Instance.ShowHealingNumber(transform.position, healAmount);
        }
    }
    
    /// <summary>
    /// 重置到满血状态（用于对象池回收重用）
    /// </summary>
    public void ResetToFull()
    {
        currentHealth = maxHealth;
        isInvulnerable = false;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        Debug.Log($"{gameObject.name} 血量已重置到满血: {currentHealth}");
    }
}
