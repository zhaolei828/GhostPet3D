using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 通用对象池 - 泛型实现，支持任何继承自Component的对象类型
/// </summary>
/// <typeparam name="T">池化的对象类型，必须继承自Component</typeparam>
public class ObjectPool<T> where T : Component
{
    // 池配置
    private readonly Transform poolParent;
    private readonly GameObject prefab;
    private readonly int initialSize;
    private readonly int maxSize;
    private readonly bool allowGrowth;
    
    // 池状态
    private readonly Queue<T> availableObjects = new Queue<T>();
    private readonly HashSet<T> activeObjects = new HashSet<T>();
    private readonly List<T> allObjects = new List<T>();
    
    // 工厂方法和回调
    private readonly Func<T> createFunction;
    private readonly Action<T> onGet;
    private readonly Action<T> onRelease;
    private readonly Action<T> onDestroy;
    
    // 统计信息
    public int TotalCount => allObjects.Count;
    public int ActiveCount => activeObjects.Count;
    public int AvailableCount => availableObjects.Count;
    public int MaxSize => maxSize;
    public bool IsAtCapacity => TotalCount >= maxSize;
    
    /// <summary>
    /// 对象池构造函数
    /// </summary>
    /// <param name="prefab">预制体</param>
    /// <param name="poolParent">池对象的父级Transform</param>
    /// <param name="initialSize">初始大小</param>
    /// <param name="maxSize">最大大小</param>
    /// <param name="allowGrowth">是否允许动态增长</param>
    /// <param name="createFunction">自定义创建函数</param>
    /// <param name="onGet">获取对象时的回调</param>
    /// <param name="onRelease">释放对象时的回调</param>
    /// <param name="onDestroy">销毁对象时的回调</param>
    public ObjectPool(
        GameObject prefab,
        Transform poolParent = null,
        int initialSize = 10,
        int maxSize = 100,
        bool allowGrowth = true,
        Func<T> createFunction = null,
        Action<T> onGet = null,
        Action<T> onRelease = null,
        Action<T> onDestroy = null)
    {
        this.prefab = prefab ?? throw new ArgumentNullException(nameof(prefab));
        this.poolParent = poolParent;
        this.initialSize = Mathf.Max(0, initialSize);
        this.maxSize = Mathf.Max(1, maxSize);
        this.allowGrowth = allowGrowth;
        
        this.createFunction = createFunction ?? DefaultCreateFunction;
        this.onGet = onGet ?? DefaultOnGet;
        this.onRelease = onRelease ?? DefaultOnRelease;
        this.onDestroy = onDestroy ?? DefaultOnDestroy;
        
        // 预先创建初始对象
        PrewarmPool();
        
        Debug.Log($"[ObjectPool<{typeof(T).Name}>] 初始化完成: 初始大小={initialSize}, 最大大小={maxSize}");
    }
    
    /// <summary>
    /// 预热池 - 创建初始对象
    /// </summary>
    private void PrewarmPool()
    {
        for (int i = 0; i < initialSize; i++)
        {
            T obj = CreateNewObject();
            if (obj != null)
            {
                ReleaseInternal(obj, false);
            }
        }
        
        Debug.Log($"[ObjectPool<{typeof(T).Name}>] 预热完成: 创建了 {availableObjects.Count} 个对象");
    }
    
    /// <summary>
    /// 从池中获取对象
    /// </summary>
    public T Get()
    {
        T obj = null;
        
        // 先尝试从可用对象队列获取
        if (availableObjects.Count > 0)
        {
            obj = availableObjects.Dequeue();
        }
        // 如果池为空且允许增长，创建新对象
        else if (allowGrowth && TotalCount < maxSize)
        {
            obj = CreateNewObject();
        }
        // 如果达到最大容量但强制需要对象，重用最老的活跃对象
        else if (activeObjects.Count > 0)
        {
            Debug.LogWarning($"[ObjectPool<{typeof(T).Name}>] 池已满，强制重用最老的对象");
            obj = GetOldestActiveObject();
            if (obj != null)
            {
                ForceRelease(obj);
            }
        }
        
        if (obj != null)
        {
            // 将对象标记为活跃
            activeObjects.Add(obj);
            
            // 激活对象
            if (obj.gameObject != null)
            {
                obj.gameObject.SetActive(true);
            }
            
            // 调用获取回调
            try
            {
                onGet?.Invoke(obj);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ObjectPool<{typeof(T).Name}>] onGet回调异常: {ex.Message}");
            }
        }
        
        return obj;
    }
    
    /// <summary>
    /// 释放对象回池中
    /// </summary>
    public void Release(T obj)
    {
        if (obj == null)
        {
            Debug.LogWarning($"[ObjectPool<{typeof(T).Name}>] 尝试释放null对象");
            return;
        }
        
        ReleaseInternal(obj, true);
    }
    
    /// <summary>
    /// 内部释放逻辑
    /// </summary>
    private void ReleaseInternal(T obj, bool validateActive)
    {
        if (obj == null) return;
        
        // 验证对象是否在活跃列表中
        if (validateActive && !activeObjects.Contains(obj))
        {
            Debug.LogWarning($"[ObjectPool<{typeof(T).Name}>] 尝试释放未激活的对象: {obj.name}");
            return;
        }
        
        // 从活跃列表移除
        activeObjects.Remove(obj);
        
        // 调用释放回调
        try
        {
            onRelease?.Invoke(obj);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ObjectPool<{typeof(T).Name}>] onRelease回调异常: {ex.Message}");
        }
        
        // 禁用对象
        if (obj.gameObject != null)
        {
            obj.gameObject.SetActive(false);
        }
        
        // 将对象放回可用队列
        if (!availableObjects.Contains(obj))
        {
            availableObjects.Enqueue(obj);
        }
    }
    
    /// <summary>
    /// 强制释放对象（即使不在活跃列表中）
    /// </summary>
    public void ForceRelease(T obj)
    {
        if (obj == null) return;
        
        ReleaseInternal(obj, false);
    }
    
    /// <summary>
    /// 创建新对象
    /// </summary>
    private T CreateNewObject()
    {
        try
        {
            T newObj = createFunction.Invoke();
            if (newObj != null)
            {
                // 设置父级
                if (poolParent != null && newObj.transform != null)
                {
                    newObj.transform.SetParent(poolParent);
                }
                
                // 添加到总列表
                allObjects.Add(newObj);
                
                // 默认禁用
                if (newObj.gameObject != null)
                {
                    newObj.gameObject.SetActive(false);
                }
            }
            
            return newObj;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ObjectPool<{typeof(T).Name}>] 创建对象失败: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// 获取最老的活跃对象
    /// </summary>
    private T GetOldestActiveObject()
    {
        // 简单实现：返回第一个活跃对象
        foreach (T obj in activeObjects)
        {
            return obj;
        }
        return null;
    }
    
    /// <summary>
    /// 清空池
    /// </summary>
    public void Clear()
    {
        Debug.Log($"[ObjectPool<{typeof(T).Name}>] 开始清空池...");
        
        // 销毁所有对象
        foreach (T obj in allObjects)
        {
            if (obj != null)
            {
                try
                {
                    onDestroy?.Invoke(obj);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ObjectPool<{typeof(T).Name}>] onDestroy回调异常: {ex.Message}");
                }
                
                if (obj.gameObject != null)
                {
                    UnityEngine.Object.Destroy(obj.gameObject);
                }
            }
        }
        
        // 清空所有集合
        availableObjects.Clear();
        activeObjects.Clear();
        allObjects.Clear();
        
        Debug.Log($"[ObjectPool<{typeof(T).Name}>] 池已清空");
    }
    
    /// <summary>
    /// 预加载指定数量的对象
    /// </summary>
    public void Preload(int count)
    {
        int toCreate = Mathf.Min(count, maxSize - TotalCount);
        
        for (int i = 0; i < toCreate; i++)
        {
            T obj = CreateNewObject();
            if (obj != null)
            {
                ReleaseInternal(obj, false);
            }
        }
        
        Debug.Log($"[ObjectPool<{typeof(T).Name}>] 预加载了 {toCreate} 个对象");
    }
    
    /// <summary>
    /// 获取池统计信息
    /// </summary>
    public string GetPoolStats()
    {
        return $"ObjectPool<{typeof(T).Name}>: Total={TotalCount}, Active={ActiveCount}, Available={AvailableCount}, Max={MaxSize}";
    }
    
    /// <summary>
    /// 验证池的完整性
    /// </summary>
    public bool ValidatePool()
    {
        // 检查数量一致性
        int expectedTotal = activeObjects.Count + availableObjects.Count;
        if (allObjects.Count != expectedTotal)
        {
            Debug.LogError($"[ObjectPool<{typeof(T).Name}>] 池完整性验证失败: 总数不匹配");
            return false;
        }
        
        // 检查对象有效性
        int nullCount = 0;
        foreach (T obj in allObjects)
        {
            if (obj == null)
            {
                nullCount++;
            }
        }
        
        if (nullCount > 0)
        {
            Debug.LogWarning($"[ObjectPool<{typeof(T).Name}>] 发现 {nullCount} 个null对象");
        }
        
        return true;
    }
    
    /// <summary>
    /// 默认创建函数
    /// </summary>
    private T DefaultCreateFunction()
    {
        if (prefab == null)
        {
            throw new InvalidOperationException("预制体为null且未提供自定义创建函数");
        }
        
        GameObject instance = UnityEngine.Object.Instantiate(prefab, poolParent);
        T component = instance.GetComponent<T>();
        
        if (component == null)
        {
            Debug.LogError($"[ObjectPool<{typeof(T).Name}>] 预制体上未找到 {typeof(T).Name} 组件");
            UnityEngine.Object.Destroy(instance);
            return null;
        }
        
        return component;
    }
    
    /// <summary>
    /// 默认获取回调
    /// </summary>
    private void DefaultOnGet(T obj)
    {
        // 默认行为：重置位置和旋转
        if (obj.transform != null)
        {
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;
        }
    }
    
    /// <summary>
    /// 默认释放回调
    /// </summary>
    private void DefaultOnRelease(T obj)
    {
        // 默认行为：移回池父级
        if (obj.transform != null && poolParent != null)
        {
            obj.transform.SetParent(poolParent);
        }
    }
    
    /// <summary>
    /// 默认销毁回调
    /// </summary>
    private void DefaultOnDestroy(T obj)
    {
        // 默认行为：无特殊处理
    }
}
