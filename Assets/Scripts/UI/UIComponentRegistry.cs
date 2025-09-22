using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// UI组件注册管理器 - 单例模式管理所有UI组件引用，解决动态查找和引用不稳定问题
/// </summary>
public class UIComponentRegistry : MonoBehaviour
{
    // 单例实例
    public static UIComponentRegistry Instance { get; private set; }
    
    [Header("调试设置")]
    [SerializeField] private bool enableDebugLog = true;
    
    // 组件引用缓存 - 使用Type作为key存储组件实例
    private Dictionary<Type, Component> componentCache = new Dictionary<Type, Component>();
    
    // RectTransform缓存 - 避免重复GetComponent调用
    private Dictionary<Component, RectTransform> rectTransformCache = new Dictionary<Component, RectTransform>();
    
    // 组件注册状态跟踪
    private HashSet<Type> registeredTypes = new HashSet<Type>();
    
    private void Awake()
    {
        // 单例模式实现
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (enableDebugLog)
                Debug.Log("[UIComponentRegistry] 初始化完成");
        }
        else
        {
            Debug.LogWarning("[UIComponentRegistry] 检测到重复实例，销毁当前对象");
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 注册UI组件到缓存中
    /// </summary>
    /// <typeparam name="T">组件类型</typeparam>
    /// <param name="component">要注册的组件实例</param>
    /// <returns>注册成功返回组件实例，失败返回null</returns>
    public T RegisterComponent<T>(T component) where T : Component
    {
        if (component == null)
        {
            Debug.LogError($"[UIComponentRegistry] 尝试注册null组件: {typeof(T).Name}");
            return null;
        }
        
        Type componentType = typeof(T);
        
        // 检查是否已经注册过相同类型的组件
        if (componentCache.ContainsKey(componentType))
        {
            if (enableDebugLog)
                Debug.LogWarning($"[UIComponentRegistry] 组件类型 {componentType.Name} 已经注册，将替换现有实例");
        }
        
        // 注册组件
        componentCache[componentType] = component;
        registeredTypes.Add(componentType);
        
        // 如果是UI组件，同时缓存其RectTransform
        if (component is MonoBehaviour)
        {
            RectTransform rectTransform = component.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransformCache[component] = rectTransform;
                
                if (enableDebugLog)
                    Debug.Log($"[UIComponentRegistry] 已注册UI组件: {componentType.Name} (含RectTransform)");
            }
            else
            {
                if (enableDebugLog)
                    Debug.Log($"[UIComponentRegistry] 已注册组件: {componentType.Name} (无RectTransform)");
            }
        }
        
        return component;
    }
    
    /// <summary>
    /// 获取已注册的组件
    /// </summary>
    /// <typeparam name="T">组件类型</typeparam>
    /// <returns>组件实例，未找到返回null</returns>
    public T GetRegisteredComponent<T>() where T : Component
    {
        Type componentType = typeof(T);
        
        if (componentCache.TryGetValue(componentType, out Component component))
        {
            // 验证组件是否仍然有效
            if (component != null)
            {
                return component as T;
            }
            else
            {
                // 组件已被销毁，从缓存中移除
                componentCache.Remove(componentType);
                registeredTypes.Remove(componentType);
                Debug.LogWarning($"[UIComponentRegistry] 组件 {componentType.Name} 已被销毁，已从缓存中移除");
                return null;
            }
        }
        
        if (enableDebugLog)
            Debug.LogWarning($"[UIComponentRegistry] 未找到已注册的组件: {componentType.Name}");
        
        return null;
    }
    
    /// <summary>
    /// 获取组件的缓存RectTransform
    /// </summary>
    /// <param name="component">UI组件</param>
    /// <returns>RectTransform实例，未找到返回null</returns>
    public RectTransform GetRectTransform(Component component)
    {
        if (component == null)
        {
            Debug.LogError("[UIComponentRegistry] 尝试获取null组件的RectTransform");
            return null;
        }
        
        if (rectTransformCache.TryGetValue(component, out RectTransform rectTransform))
        {
            // 验证RectTransform是否仍然有效
            if (rectTransform != null)
            {
                return rectTransform;
            }
            else
            {
                // RectTransform已被销毁，从缓存中移除
                rectTransformCache.Remove(component);
                Debug.LogWarning($"[UIComponentRegistry] 组件 {component.GetType().Name} 的RectTransform已被销毁");
                return null;
            }
        }
        
        // 如果缓存中没有，尝试重新获取并缓存
        RectTransform newRectTransform = component.GetComponent<RectTransform>();
        if (newRectTransform != null)
        {
            rectTransformCache[component] = newRectTransform;
            
            if (enableDebugLog)
                Debug.Log($"[UIComponentRegistry] 为组件 {component.GetType().Name} 重新缓存了RectTransform");
            
            return newRectTransform;
        }
        
        return null;
    }
    
    /// <summary>
    /// 检查组件类型是否已注册
    /// </summary>
    /// <typeparam name="T">组件类型</typeparam>
    /// <returns>是否已注册</returns>
    public bool IsComponentRegistered<T>() where T : Component
    {
        return registeredTypes.Contains(typeof(T));
    }
    
    /// <summary>
    /// 取消注册组件
    /// </summary>
    /// <typeparam name="T">组件类型</typeparam>
    public void UnregisterComponent<T>() where T : Component
    {
        Type componentType = typeof(T);
        
        if (componentCache.TryGetValue(componentType, out Component component))
        {
            // 移除RectTransform缓存
            if (component != null && rectTransformCache.ContainsKey(component))
            {
                rectTransformCache.Remove(component);
            }
            
            // 移除组件缓存
            componentCache.Remove(componentType);
            registeredTypes.Remove(componentType);
            
            if (enableDebugLog)
                Debug.Log($"[UIComponentRegistry] 已取消注册组件: {componentType.Name}");
        }
    }
    
    /// <summary>
    /// 清除所有缓存
    /// </summary>
    public void ClearAllCache()
    {
        componentCache.Clear();
        rectTransformCache.Clear();
        registeredTypes.Clear();
        
        if (enableDebugLog)
            Debug.Log("[UIComponentRegistry] 已清除所有缓存");
    }
    
    /// <summary>
    /// 获取注册统计信息
    /// </summary>
    /// <returns>注册信息字符串</returns>
    public string GetRegistryStats()
    {
        return $"已注册组件: {componentCache.Count}, RectTransform缓存: {rectTransformCache.Count}";
    }
    
    /// <summary>
    /// 验证所有已注册组件的有效性
    /// </summary>
    /// <returns>无效组件的数量</returns>
    public int ValidateAllComponents()
    {
        int invalidCount = 0;
        List<Type> toRemove = new List<Type>();
        
        foreach (var kvp in componentCache)
        {
            if (kvp.Value == null)
            {
                toRemove.Add(kvp.Key);
                invalidCount++;
            }
        }
        
        // 移除无效的组件
        foreach (Type type in toRemove)
        {
            componentCache.Remove(type);
            registeredTypes.Remove(type);
        }
        
        if (invalidCount > 0 && enableDebugLog)
        {
            Debug.LogWarning($"[UIComponentRegistry] 发现并清理了 {invalidCount} 个无效组件引用");
        }
        
        return invalidCount;
    }
    
    /// <summary>
    /// 自动注册场景中的UI组件
    /// </summary>
    public void AutoRegisterUIComponents()
    {
        if (enableDebugLog)
            Debug.Log("[UIComponentRegistry] 开始自动注册场景中的UI组件...");
        
        // 查找并注册PlayerHealthBar
        PlayerHealthBar healthBar = FindFirstObjectByType<PlayerHealthBar>();
        if (healthBar != null)
        {
            RegisterComponent(healthBar);
        }
        
        // 查找并注册ScoreUI
        ScoreUI scoreUI = FindFirstObjectByType<ScoreUI>();
        if (scoreUI != null)
        {
            RegisterComponent(scoreUI);
        }
        
        // 查找并注册SwordStatusUI
        SwordStatusUI swordStatusUI = FindFirstObjectByType<SwordStatusUI>();
        if (swordStatusUI != null)
        {
            RegisterComponent(swordStatusUI);
        }
        
        if (enableDebugLog)
        {
            Debug.Log($"[UIComponentRegistry] 自动注册完成: {GetRegistryStats()}");
        }
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    
    // 调试方法：在Inspector中显示当前注册状态
    [ContextMenu("显示注册状态")]
    private void ShowRegistryStatus()
    {
        Debug.Log($"[UIComponentRegistry] {GetRegistryStats()}");
        
        foreach (var kvp in componentCache)
        {
            string status = kvp.Value != null ? "有效" : "无效";
            Debug.Log($"  - {kvp.Key.Name}: {status}");
        }
    }
    
    [ContextMenu("验证所有组件")]
    private void ValidateComponents()
    {
        int invalidCount = ValidateAllComponents();
        Debug.Log($"[UIComponentRegistry] 组件验证完成，清理了 {invalidCount} 个无效引用");
    }
}
