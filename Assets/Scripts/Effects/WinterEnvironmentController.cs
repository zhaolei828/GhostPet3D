using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 冬季环境控制器 - 管理雪天的光照、氛围和视觉效果
/// </summary>
public class WinterEnvironmentController : MonoBehaviour
{
    [Header("光照设置")]
    [SerializeField] private Light mainLight;
    [SerializeField] private Color winterSunColor = new Color(0.85f, 0.9f, 1f, 1f);
    [SerializeField] private float winterSunIntensity = 0.8f;
    [SerializeField] private Color ambientColor = new Color(0.7f, 0.8f, 0.9f, 1f);
    
    [Header("天空盒设置")]
    [SerializeField] private Material winterSkybox;
    [SerializeField] private Color fogColor = new Color(0.8f, 0.85f, 0.9f, 1f);
    [SerializeField] private float fogDensity = 0.01f;
    
    [Header("环境音效")]
    [SerializeField] private AudioSource windAudioSource;
    [SerializeField] private AudioClip windSound;
    [SerializeField] private float windVolume = 0.3f;
    
    [Header("动态效果")]
    [SerializeField] private bool enableDynamicLighting = true;
    [SerializeField] private float lightVariationSpeed = 0.5f;
    [SerializeField] private float lightVariationAmount = 0.2f;
    
    private float baseLightIntensity;
    private Color baseLightColor;
    
    private void Start()
    {
        InitializeWinterEnvironment();
    }
    
    private void InitializeWinterEnvironment()
    {
        // 自动找到主光源
        if (mainLight == null)
        {
            mainLight = FindFirstObjectByType<Light>();
            if (mainLight == null)
            {
                Debug.LogWarning("[WinterEnvironmentController] 未找到主光源，创建一个新的");
                CreateMainLight();
            }
        }
        
        SetupWinterLighting();
        SetupWinterAtmosphere();
        SetupAudioEnvironment();
        
        Debug.Log("[WinterEnvironmentController] 冬季环境初始化完成");
    }
    
    private void CreateMainLight()
    {
        GameObject lightGO = new GameObject("WinterMainLight");
        lightGO.transform.position = new Vector3(0, 20, 0);
        lightGO.transform.rotation = Quaternion.Euler(45, 30, 0);
        
        mainLight = lightGO.AddComponent<Light>();
        mainLight.type = LightType.Directional;
    }
    
    private void SetupWinterLighting()
    {
        if (mainLight != null)
        {
            // 配置主光源
            mainLight.type = LightType.Directional;
            mainLight.color = winterSunColor;
            mainLight.intensity = winterSunIntensity;
            mainLight.shadows = LightShadows.Soft;
            
            // 保存基础值用于动态变化
            baseLightIntensity = winterSunIntensity;
            baseLightColor = winterSunColor;
            
            Debug.Log($"[WinterEnvironmentController] 主光源配置完成: 颜色={winterSunColor}, 强度={winterSunIntensity}");
        }
        
        // 配置环境光
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = ambientColor;
        RenderSettings.ambientIntensity = 0.6f;
        
        Debug.Log($"[WinterEnvironmentController] 环境光配置完成: 颜色={ambientColor}");
    }
    
    private void SetupWinterAtmosphere()
    {
        // 雾效设置
        RenderSettings.fog = true;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = fogDensity;
        
        // 天空盒（如果有的话）
        if (winterSkybox != null)
        {
            RenderSettings.skybox = winterSkybox;
        }
        
        Debug.Log("[WinterEnvironmentController] 大气效果配置完成");
    }
    
    private void SetupAudioEnvironment()
    {
        // 创建风声音效
        if (windAudioSource == null && windSound != null)
        {
            GameObject audioGO = new GameObject("WinterWindAudio");
            audioGO.transform.SetParent(transform);
            windAudioSource = audioGO.AddComponent<AudioSource>();
        }
        
        if (windAudioSource != null && windSound != null)
        {
            windAudioSource.clip = windSound;
            windAudioSource.loop = true;
            windAudioSource.volume = windVolume;
            windAudioSource.pitch = 1f;
            windAudioSource.spatialBlend = 0f; // 2D音效
            windAudioSource.Play();
            
            Debug.Log("[WinterEnvironmentController] 环境音效配置完成");
        }
    }
    
    private void Update()
    {
        if (enableDynamicLighting)
        {
            UpdateDynamicLighting();
        }
    }
    
    private void UpdateDynamicLighting()
    {
        if (mainLight == null) return;
        
        // 光照强度的细微变化，模拟云层遮挡
        float intensityVariation = Mathf.Sin(Time.time * lightVariationSpeed) * lightVariationAmount;
        mainLight.intensity = baseLightIntensity + intensityVariation;
        
        // 色温的细微变化
        float colorVariation = Mathf.Sin(Time.time * lightVariationSpeed * 0.7f) * 0.05f;
        Color variatedColor = baseLightColor;
        variatedColor.b += colorVariation; // 蓝色通道变化
        mainLight.color = variatedColor;
    }
    
    /// <summary>
    /// 切换到暴雪模式
    /// </summary>
    public void SetBlizzardMode(bool enable)
    {
        if (enable)
        {
            // 暴雪时光线更暗，雾更浓
            winterSunIntensity = 0.4f;
            fogDensity = 0.02f;
            lightVariationAmount = 0.4f;
            
            if (windAudioSource != null)
            {
                windAudioSource.volume = windVolume * 1.5f;
                windAudioSource.pitch = 1.2f;
            }
        }
        else
        {
            // 恢复正常雪天
            winterSunIntensity = 0.8f;
            fogDensity = 0.01f;
            lightVariationAmount = 0.2f;
            
            if (windAudioSource != null)
            {
                windAudioSource.volume = windVolume;
                windAudioSource.pitch = 1f;
            }
        }
        
        baseLightIntensity = winterSunIntensity;
        RenderSettings.fogDensity = fogDensity;
        
        Debug.Log($"[WinterEnvironmentController] 暴雪模式: {enable}");
    }
    
    /// <summary>
    /// 设置一天中的时间（0-24小时）
    /// </summary>
    public void SetTimeOfDay(float hour)
    {
        hour = Mathf.Clamp(hour, 0, 24);
        
        // 调整光源角度模拟太阳位置
        if (mainLight != null)
        {
            float sunAngle = (hour - 12f) * 15f; // 中午12点为最高点
            mainLight.transform.rotation = Quaternion.Euler(45 + sunAngle * 0.5f, 30, 0);
            
            // 根据时间调整光照强度和色温
            if (hour < 6 || hour > 18) // 夜晚
            {
                mainLight.intensity = 0.2f;
                mainLight.color = new Color(0.6f, 0.7f, 1f); // 更冷的蓝光
            }
            else if (hour < 8 || hour > 16) // 傍晚/清晨
            {
                mainLight.intensity = 0.5f;
                mainLight.color = new Color(0.9f, 0.8f, 0.7f); // 暖光
            }
            else // 白天
            {
                mainLight.intensity = winterSunIntensity;
                mainLight.color = winterSunColor;
            }
        }
    }
    
    private void OnValidate()
    {
        // 在编辑器中实时预览效果
        if (Application.isPlaying)
        {
            SetupWinterLighting();
            SetupWinterAtmosphere();
        }
    }
}
