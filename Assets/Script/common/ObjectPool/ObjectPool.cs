using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 通用对象池，实现对 Unity 组件的复用与限定缓存
/// </summary>
public class ObjectPool<T> where T : Component
{
    // 对象工厂，用于首次创建或池空时创建新实例
    private readonly Func<T> factory;
    // 缓存父节点（可选），获取时会把实例挂到该节点下
    private readonly Transform parent;
    // 回调：每次从池中取出对象时调用
    private readonly Action<T> onGet;
    // 回调：每次归还对象到池中时调用
    private readonly Action<T> onRelease;

    // 内部栈结构，用于存储可复用对象
    private readonly Stack<T> pool;
    // 最大缓存数量，超过时新归还的对象会被销毁
    private readonly int maxSize;

    /// <param name="factory">对象创建工厂</param>
    /// <param name="parent">池中对象的默认父节点</param>
    /// <param name="maxSize">最大缓存数，<=0 表示不限制</param>
    /// <param name="onGet">取出时回调</param>
    /// <param name="onRelease">归还时回调</param>
    public ObjectPool(
        Func<T> factory,
        Transform parent = null,
        int maxSize = 0,
        Action<T> onGet = null,
        Action<T> onRelease = null)
    {
        this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        this.parent = parent;
        this.maxSize = maxSize;
        this.onGet = onGet;
        this.onRelease = onRelease;
        pool = new Stack<T>(Mathf.Max(4, maxSize > 0 ? maxSize : 4));
    }

    /// <summary>
    /// 获取对象，优先从池中取出，否则使用 factory 创建
    /// </summary>
    public T Get()
    {
        T element;
        if (pool.Count > 0)
        {
            element = pool.Pop();
        }
        else
        {
            element = factory();
        }

        // 激活并设置父级
        element.gameObject.SetActive(true);
        if (parent != null)
            element.transform.SetParent(parent, false);

        // 回调
        onGet?.Invoke(element);

        return element;
    }

    /// <summary>
    /// 归还对象，执行回调并放回池中，如果池已达最大容量，则销毁对象
    /// </summary>
    public void Release(T element)
    {
        if (element == null) return;

        // 回调
        onRelease?.Invoke(element);

        // 不重复归还
        if (pool.Count > 0 && pool.Contains(element))
        {
            Debug.LogWarning("Attempting to release an object that is already in the pool.");
            return;
        }

        element.gameObject.SetActive(false);

        if (maxSize > 0 && pool.Count >= maxSize)
        {
            // 超出缓存上限，直接销毁
            UnityEngine.Object.Destroy(element.gameObject);
        }
        else
        {
            pool.Push(element);
        }
    }

    /// <summary>
    /// 清空池中所有未使用的对象
    /// </summary>
    public void Clear()
    {
        while (pool.Count > 0)
        {
            var element = pool.Pop();
            if (element != null)
                UnityEngine.Object.Destroy(element.gameObject);
        }
    }

    /// <summary>
    /// 当前池中缓存数量
    /// </summary>
    public int CountInactive => pool.Count;
}
