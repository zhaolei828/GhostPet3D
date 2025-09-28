using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 环绕飞剑管理器 - 管理九把环绕Player的飞剑
/// </summary>
public class OrbitingSwordManager : MonoBehaviour
{
    [Header("环绕飞剑设置")]
    [SerializeField] private GameObject orbitingSwordPrefab;
    [SerializeField] private int swordCount = 9;
    [SerializeField] private float orbitRadius = 3f;
    [SerializeField] private float orbitSpeed = 60f; // 度/秒
    [SerializeField] private float swordHeight = 1.5f; // 相对于Player的高度
    
    [Header("飞剑外观")]
    [SerializeField] private float swordScale = 0.3f;
    [SerializeField] private Material orbitSwordMaterial;
    
    // 环绕飞剑列表
    private List<Transform> orbitingSwords = new List<Transform>();
    private Transform player;
    private float currentAngle = 0f;
    private bool isInitialized = false;
    
    // 单例
    public static OrbitingSwordManager Instance { get; private set; }
    
    private void Awake()
    {
        Debug.Log("[OrbitingSwordManager] Awake开始执行");
        
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[OrbitingSwordManager] 设置单例实例");
        }
        else
        {
            Debug.Log("[OrbitingSwordManager] 单例已存在，销毁重复对象");
            Destroy(gameObject);
            return;
        }
        
        // 查找Player对象
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log($"[OrbitingSwordManager] 找到Player对象: {playerObj.name}");
        }
        else
        {
            Debug.LogError("[OrbitingSwordManager] 未找到Player对象！");
        }
    }
    
    private void Start()
    {
        Debug.Log("[OrbitingSwordManager] Start方法执行");
    }
    
    private void Update()
    {
        // 第一帧初始化
        if (!isInitialized)
        {
            Debug.Log("[OrbitingSwordManager] Update中进行初始化");
            CreateOrbitingSwords();
            isInitialized = true;
        }
        
        UpdateOrbitingSwords();
    }
    
    /// <summary>
    /// 创建环绕飞剑
    /// </summary>
    private void CreateOrbitingSwords()
    {
        if (orbitingSwordPrefab == null)
        {
            Debug.LogWarning("[OrbitingSwordManager] 环绕飞剑预制件未设置！");
            return;
        }
        
        if (player == null)
        {
            Debug.LogError("[OrbitingSwordManager] Player对象未找到，无法创建环绕飞剑！");
            return;
        }
        
        float angleStep = 360f / swordCount;
        
        for (int i = 0; i < swordCount; i++)
        {
            // 计算位置
            float angle = i * angleStep;
            Vector3 position = CalculateSwordPosition(angle);
            
            // 创建飞剑
            GameObject sword = Instantiate(orbitingSwordPrefab, position, Quaternion.identity);
            sword.name = $"OrbitingSword_{i + 1}";
            
            // 设置材质
            if (orbitSwordMaterial != null)
            {
                MeshRenderer renderer = sword.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.material = orbitSwordMaterial;
                }
            }
            
            // 设置大小
            sword.transform.localScale = Vector3.one * swordScale;
            
            // 禁用飞剑脚本的移动逻辑
            FlyingSword3D swordScript = sword.GetComponent<FlyingSword3D>();
            if (swordScript != null)
            {
                // 不禁用脚本，而是重置状态
                swordScript.ResetSword();
            }
            
            // 设置为trigger避免物理碰撞
            Collider swordCollider = sword.GetComponent<Collider>();
            if (swordCollider != null)
            {
                swordCollider.isTrigger = true;
            }
            
            // 禁用重力
            Rigidbody rb = sword.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                rb.isKinematic = true;
            }
            
            orbitingSwords.Add(sword.transform);
        }
        
        Debug.Log($"[OrbitingSwordManager] 创建了 {swordCount} 把环绕飞剑");
    }
    
    /// <summary>
    /// 更新环绕飞剑位置
    /// </summary>
    private void UpdateOrbitingSwords()
    {
        if (orbitingSwords.Count == 0) return;
        
        // 更新旋转角度
        currentAngle += orbitSpeed * Time.deltaTime;
        if (currentAngle >= 360f)
        {
            currentAngle -= 360f;
        }
        
        float angleStep = 360f / swordCount;
        
        for (int i = 0; i < orbitingSwords.Count; i++)
        {
            if (orbitingSwords[i] == null) continue;
            
            // 计算当前飞剑的角度
            float swordAngle = currentAngle + (i * angleStep);
            Vector3 position = CalculateSwordPosition(swordAngle);
            
            // 更新位置
            orbitingSwords[i].position = position;
            
            // 更新旋转 - 让飞剑指向轨道切线方向
            Vector3 tangentDirection = new Vector3(-Mathf.Sin(swordAngle * Mathf.Deg2Rad), 0, Mathf.Cos(swordAngle * Mathf.Deg2Rad));
            orbitingSwords[i].rotation = Quaternion.LookRotation(tangentDirection) * Quaternion.Euler(0, 0, 90);
        }
    }
    
    /// <summary>
    /// 根据角度计算飞剑位置
    /// </summary>
    private Vector3 CalculateSwordPosition(float angle)
    {
        float radian = angle * Mathf.Deg2Rad;
        Vector3 playerPosition = player.position;
        
        float x = playerPosition.x + Mathf.Cos(radian) * orbitRadius;
        float z = playerPosition.z + Mathf.Sin(radian) * orbitRadius;
        float y = playerPosition.y + swordHeight;
        
        return new Vector3(x, y, z);
    }
    
    /// <summary>
    /// 获取最近的环绕飞剑发射攻击
    /// </summary>
    public bool LaunchNearestSword(Vector3 targetDirection, Transform target = null)
    {
        if (orbitingSwords.Count == 0) return false;
        
        // 找到最接近目标方向的飞剑
        int nearestIndex = 0;
        float nearestDot = -2f;
        
        for (int i = 0; i < orbitingSwords.Count; i++)
        {
            if (orbitingSwords[i] == null) continue;
            
            Vector3 swordDirection = (orbitingSwords[i].position - player.position).normalized;
            float dot = Vector3.Dot(swordDirection, targetDirection.normalized);
            
            if (dot > nearestDot)
            {
                nearestDot = dot;
                nearestIndex = i;
            }
        }
        
        // 发射最近的飞剑
        Transform swordToLaunch = orbitingSwords[nearestIndex];
        if (swordToLaunch != null)
        {
            LaunchSword(swordToLaunch, targetDirection, target, nearestIndex);
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 发射指定飞剑
    /// </summary>
    private void LaunchSword(Transform sword, Vector3 direction, Transform target, int index)
    {
        // 直接发射飞剑（脚本始终启用）
        FlyingSword3D swordScript = sword.GetComponent<FlyingSword3D>();
        if (swordScript != null)
        {
            swordScript.Launch(direction, target);
        }
        
        // 设置物理属性用于飞行
        Rigidbody rb = sword.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }
        
        // 从环绕列表移除
        orbitingSwords[index] = null;
        
        Debug.Log($"[OrbitingSwordManager] 发射环绕飞剑 {index + 1}");
        
        // 1秒后重新生成飞剑（确保快速补充，保持攻击连续性）
        StartCoroutine(RegenerateSword(index, 1f));
    }
    
    /// <summary>
    /// 重新生成飞剑
    /// </summary>
    private System.Collections.IEnumerator RegenerateSword(int index, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (orbitingSwordPrefab != null && index < orbitingSwords.Count)
        {
            // 计算重生位置
            float angleStep = 360f / swordCount;
            float angle = currentAngle + (index * angleStep);
            Vector3 position = CalculateSwordPosition(angle);
            
            // 创建新飞剑
            GameObject newSword = Instantiate(orbitingSwordPrefab, position, Quaternion.identity);
            newSword.name = $"OrbitingSword_{index + 1}";
            
            // 应用设置（与CreateOrbitingSwords中相同）
            if (orbitSwordMaterial != null)
            {
                MeshRenderer renderer = newSword.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.material = orbitSwordMaterial;
                }
            }
            
            newSword.transform.localScale = Vector3.one * swordScale;
            
            FlyingSword3D swordScript = newSword.GetComponent<FlyingSword3D>();
            if (swordScript != null)
            {
                // 不禁用脚本，而是重置状态
                swordScript.ResetSword();
            }
            
            Collider swordCollider = newSword.GetComponent<Collider>();
            if (swordCollider != null)
            {
                swordCollider.isTrigger = true;
            }
            
            Rigidbody rb = newSword.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                rb.isKinematic = true;
            }
            
            orbitingSwords[index] = newSword.transform;
            
            Debug.Log($"[OrbitingSwordManager] 重新生成环绕飞剑 {index + 1}");
        }
    }
}