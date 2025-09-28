using UnityEngine;
using System.Collections;

/// <summary>
/// 受击效果组件 - 提供受到攻击时的视觉反馈
/// </summary>
public class HitEffect : MonoBehaviour
{
    [Header("受击效果设置")]
    [SerializeField] private Color hitColor = Color.red; // 受击颜色
    [SerializeField] private float flashDuration = 0.2f; // 闪烁持续时间
    [SerializeField] private float shakeIntensity = 0.05f; // 震动强度（调小一半，更温和的受击反馈）
    [SerializeField] private float shakeDuration = 0.15f; // 震动持续时间
    
    [Header("伤害颜色设置")]
    [SerializeField] private Color lightDamageColor = Color.yellow;
    [SerializeField] private Color heavyDamageColor = Color.red;
    [SerializeField] private float heavyDamageThreshold = 50f; // 重伤阈值
    
    // 组件引用
    private MeshRenderer meshRenderer;
    private Material originalMaterial;
    private Material hitMaterial;
    private Vector3 originalPosition;
    private bool isPlayingEffect = false;
    
    private void Awake()
    {
        // 获取组件
        meshRenderer = GetComponent<MeshRenderer>();
        originalPosition = transform.localPosition;
        
        // 创建受击材质
        if (meshRenderer != null && meshRenderer.material != null)
        {
            originalMaterial = meshRenderer.material;
            hitMaterial = new Material(originalMaterial);
        }
    }
    
    /// <summary>
    /// 播放受击效果
    /// </summary>
    /// <param name="damage">伤害值，用于确定效果强度</param>
    public void PlayHitEffect(float damage)
    {
        if (isPlayingEffect) return; // 防止效果重叠
        
        Debug.Log($"[HitEffect] 播放受击效果: {gameObject.name}, 伤害: {damage}");
        
        // 根据伤害值选择效果颜色和强度
        Color effectColor = damage >= heavyDamageThreshold ? heavyDamageColor : lightDamageColor;
        float effectIntensity = Mathf.Clamp(damage / heavyDamageThreshold, 0.5f, 2f);
        
        // 启动效果协程
        StartCoroutine(HitEffectCoroutine(effectColor, effectIntensity));
    }
    
    /// <summary>
    /// 受击效果协程
    /// </summary>
    private IEnumerator HitEffectCoroutine(Color effectColor, float intensity)
    {
        isPlayingEffect = true;
        
        // 同时播放颜色闪烁和位置震动
        Coroutine flashCoroutine = StartCoroutine(ColorFlash(effectColor));
        Coroutine shakeCoroutine = StartCoroutine(PositionShake(intensity));
        
        // 等待所有效果完成
        yield return flashCoroutine;
        yield return shakeCoroutine;
        
        isPlayingEffect = false;
    }
    
    /// <summary>
    /// 颜色闪烁效果
    /// </summary>
    private IEnumerator ColorFlash(Color flashColor)
    {
        if (meshRenderer == null || hitMaterial == null) yield break;
        
        // 设置闪烁颜色
        hitMaterial.color = flashColor;
        meshRenderer.material = hitMaterial;
        
        // 等待闪烁时间
        yield return new WaitForSeconds(flashDuration);
        
        // 恢复原始材质
        if (originalMaterial != null)
        {
            meshRenderer.material = originalMaterial;
        }
    }
    
    /// <summary>
    /// 位置震动效果
    /// </summary>
    private IEnumerator PositionShake(float intensity)
    {
        float elapsed = 0f;
        float adjustedIntensity = shakeIntensity * intensity;
        
        while (elapsed < shakeDuration)
        {
            // 计算随机震动偏移
            float offsetX = Random.Range(-adjustedIntensity, adjustedIntensity);
            float offsetZ = Random.Range(-adjustedIntensity, adjustedIntensity);
            
            // 应用震动
            transform.localPosition = originalPosition + new Vector3(offsetX, 0, offsetZ);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // 恢复原始位置
        transform.localPosition = originalPosition;
    }
    
    /// <summary>
    /// 停止当前效果
    /// </summary>
    public void StopEffect()
    {
        if (isPlayingEffect)
        {
            StopAllCoroutines();
            
            // 恢复原始状态
            if (meshRenderer != null && originalMaterial != null)
            {
                meshRenderer.material = originalMaterial;
            }
            transform.localPosition = originalPosition;
            
            isPlayingEffect = false;
        }
    }
    
    private void OnDestroy()
    {
        // 清理创建的材质
        if (hitMaterial != null)
        {
            DestroyImmediate(hitMaterial);
        }
    }
}
