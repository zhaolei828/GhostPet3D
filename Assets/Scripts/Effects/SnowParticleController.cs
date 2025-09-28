using UnityEngine;

/// <summary>
/// 雪花粒子系统控制器 - 创建逼真的飘雪效果
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class SnowParticleController : MonoBehaviour
{
    [Header("雪花设置")]
    [SerializeField] private int maxParticles = 500;
    [SerializeField] private float emissionRate = 50f;
    [SerializeField] private float fallSpeed = 2f;
    [SerializeField] private float sideWind = 0.5f;
    [SerializeField] private Vector2 sizeRange = new Vector2(0.02f, 0.08f);
    [SerializeField] private Vector2 lifetimeRange = new Vector2(6f, 12f);
    
    [Header("雪花范围")]
    [SerializeField] private float spawnRadius = 25f;
    [SerializeField] private float spawnHeight = 15f;
    
    [Header("视觉效果")]
    [SerializeField] private Color snowColor = new Color(1f, 1f, 1f, 0.8f);
    [SerializeField] private bool enableWind = true;
    [SerializeField] private float windStrength = 0.3f;
    
    private ParticleSystem snowParticles;
    private ParticleSystem.MainModule main;
    private ParticleSystem.EmissionModule emission;
    private ParticleSystem.ShapeModule shape;
    private ParticleSystem.VelocityOverLifetimeModule velocity;
    private ParticleSystem.SizeOverLifetimeModule sizeOverLifetime;
    private ParticleSystem.ColorOverLifetimeModule colorOverLifetime;
    
    private void Start()
    {
        InitializeSnowParticleSystem();
    }
    
    private void InitializeSnowParticleSystem()
    {
        snowParticles = GetComponent<ParticleSystem>();
        
        // 主模块设置
        main = snowParticles.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifetimeRange.x, lifetimeRange.y);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(sizeRange.x, sizeRange.y);
        main.startColor = snowColor;
        main.maxParticles = maxParticles;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0.1f; // 轻微重力，模拟雪花缓慢下落
        
        // 发射模块
        emission = snowParticles.emission;
        emission.rateOverTime = emissionRate;
        
        // 形状模块 - 在玩家周围的圆形区域生成
        shape = snowParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = spawnRadius;
        shape.position = new Vector3(0, spawnHeight, 0);
        
        // 速度模块 - 模拟风力和下落
        velocity = snowParticles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.x = new ParticleSystem.MinMaxCurve(-sideWind, sideWind);
        velocity.y = new ParticleSystem.MinMaxCurve(-fallSpeed, -fallSpeed * 0.5f);
        velocity.z = new ParticleSystem.MinMaxCurve(-sideWind, sideWind);
        
        // 大小随时间变化 - 雪花在接近地面时稍微变小
        sizeOverLifetime = snowParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(0.8f, 1f);
        sizeCurve.AddKey(1f, 0.5f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // 颜色随时间变化 - 雪花在接近地面时逐渐透明
        colorOverLifetime = snowParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient colorGradient = new Gradient();
        colorGradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.white, 0.0f), 
                new GradientColorKey(Color.white, 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0.8f, 0.0f), 
                new GradientAlphaKey(0.8f, 0.7f),
                new GradientAlphaKey(0.1f, 1.0f) 
            }
        );
        colorOverLifetime.color = colorGradient;
        
        Debug.Log("[SnowParticleController] 雪花粒子系统初始化完成");
    }
    
    /// <summary>
    /// 运行时调整风力强度
    /// </summary>
    public void SetWindStrength(float strength)
    {
        windStrength = Mathf.Clamp01(strength);
        if (velocity.enabled)
        {
            velocity.x = new ParticleSystem.MinMaxCurve(-sideWind * windStrength, sideWind * windStrength);
            velocity.z = new ParticleSystem.MinMaxCurve(-sideWind * windStrength, sideWind * windStrength);
        }
    }
    
    /// <summary>
    /// 调整雪花强度
    /// </summary>
    public void SetSnowIntensity(float intensity)
    {
        intensity = Mathf.Clamp(intensity, 0f, 2f);
        if (emission.enabled)
        {
            emission.rateOverTime = emissionRate * intensity;
        }
    }
    
    private void Update()
    {
        // 简单的风力模拟 - 可以根据游戏需要扩展
        if (enableWind && velocity.enabled)
        {
            float windVariation = Mathf.Sin(Time.time * 0.5f) * 0.1f;
            SetWindStrength(windStrength + windVariation);
        }
        
        // 让粒子系统跟随玩家（如果需要）
        FollowPlayer();
    }
    
    private void FollowPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 targetPosition = player.transform.position + Vector3.up * spawnHeight;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 0.5f);
        }
    }
}
