using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 简洁版依赖注入容器，支持构造函数注入与自动初始化。
/// </summary>
public class DependencyContainer
{
    // 类型到实现映射（目前注册自身即可）
    private readonly Dictionary<Type, Type> _serviceTypes = new();
    // 已构造好的实例缓存
    private readonly Dictionary<Type, object> _instances = new();
    // 初始化顺序记录
    private readonly List<Type> _initializationOrder = new();

    /// <summary>
    /// 注册服务类型为单例（按类型注册，稍后构造）
    /// </summary>
    public void RegisterSingleton<T>() where T : class
    {
        var type = typeof(T);
        if (_serviceTypes.ContainsKey(type))
        {
            ConsoleDebug.LogWarning($"服务 {type.Name} 已注册，将被覆盖");
        }
        _serviceTypes[type] = type;
        _initializationOrder.Add(type);
    }
    /// <summary>
    /// 注册接口到具体实现，按单例管理
    /// </summary>
    public void RegisterSingleton<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        var serviceType = typeof(TService);
        var implType = typeof(TImplementation);
        if (_serviceTypes.ContainsKey(serviceType))
            ConsoleDebug.LogWarning($"服务 {serviceType.Name} 已注册，将被覆盖");

        _serviceTypes[serviceType] = implType;
        _initializationOrder.Add(serviceType);
    }
    /// <summary>
    /// 获取服务实例，支持构造函数自动注入
    /// </summary>
    public T GetService<T>() where T : class => (T)GetService(typeof(T));

    /// <summary>
    /// 获取服务实例（通用版本）
    /// </summary>
    private object GetService(Type type)
    {
        if (_instances.TryGetValue(type, out var existing))
            return existing;

        if (!_serviceTypes.TryGetValue(type, out var implType))
            throw new InvalidOperationException($"未注册类型: {type.Name}");

        // 选择参数最多的构造函数
        var ctor = implType.GetConstructors()//获取所有的构造函数
                           .OrderByDescending(c => c.GetParameters().Length) //按照参数数量进行排序
                           .FirstOrDefault(); //默认选择第一个

        object instance;
        if (ctor == null || ctor.GetParameters().Length == 0)
        {
            instance = Activator.CreateInstance(implType);
        }
        else
        {
            var parameters = ctor.GetParameters()//获取这个构造函数的所有参数数量
                                  .Select(p => GetService(p.ParameterType))//选择所有参数的服务
                                  .ToArray();//转化为数组
            instance = Activator.CreateInstance(implType, parameters);//对该函数进行注入并构造
        }

        _instances[type] = instance;
        return instance;
    }

    /// <summary>
    /// 尝试获取服务（非抛错方式）
    /// </summary>
    public bool TryGetService(Type type, out object service)
    {
        if (_instances.TryGetValue(type, out service)) return true;
        try
        {
            service = GetService(type);
            return true;
        }
        catch
        {
            service = null;
            return false;
        }
    }

    /// <summary>
    /// 初始化所有已构建服务，调用 Init() 或 IInitializable.Initialize()
    /// </summary>
    public void InitializeServices()
    {
        foreach (var type in _initializationOrder)
        {
            var instance = _serviceTypes[type];
            if (instance is IInitializable initializable)
            {
                initializable.Initialize();
                ConsoleDebug.Log($"成功初始化: {type.Name}");
            }
            else
            {
                ConsoleDebug.LogWarning($"服务 {type.Name} 没有实现 IInitializable");
            }
        }
    }
}

/// <summary>
/// 可选接口，支持统一初始化调用
/// </summary>
public interface IInitializable
{
    void Initialize();
}