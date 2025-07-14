using System;
using System.Buffers;
using UnityEngine;

public class GameRoot
{
    public static GameRoot Instance = null;

    private readonly DependencyContainer _container = new DependencyContainer();
    public DependencyContainer Container => _container;
    public void Initialize()
    {
        // 1. 构建依赖关系
        BuildDependencyContainer();
        // 2. 初始化服务
        InitializeServices();
        // 3. 启动应用服务
        StartApplication();
    }
    private void BuildDependencyContainer()
    {
        // 注册服务和控制器
        _container.RegisterSingleton<IDataPersistenceManager, DataPersistenceManager>();
        _container.RegisterSingleton<IMemoryManager, MemoryManager>();
        _container.RegisterSingleton<ISentisInference, SentisInference>();
        _container.RegisterSingleton<IResourcesManager, ResourcesMgr>();
        _container.RegisterSingleton<IUIViewUtil, UIViewUtil>();
        _container.RegisterSingleton<IUIViewMgr, UIViewMgr>();
        _container.RegisterSingleton<IChatService, ChatService>();
    //    _container.RegisterSingleton<IDialogueController, DialogueController>();
       // _container.RegisterSingleton<ICustomizedController, CustomizedController>();
    }
    private void InitializeServices()
    {
        _container.InitializeServices();
    }
    private void StartApplication()
    {
        // 启动主界面
        var uiManager = _container.GetService<UIViewMgr>();
        uiManager.OpenWindow(WindowUIType.DialogMain);
    }
    public void Shutdown()
    {
        // 如果有需要在退出时释放或保存数据的服务，在这里统一调用
    }
}
