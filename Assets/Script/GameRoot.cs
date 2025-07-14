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
        // 1. ����������ϵ
        BuildDependencyContainer();
        // 2. ��ʼ������
        InitializeServices();
        // 3. ����Ӧ�÷���
        StartApplication();
    }
    private void BuildDependencyContainer()
    {
        // ע�����Ϳ�����
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
        // ����������
        var uiManager = _container.GetService<UIViewMgr>();
        uiManager.OpenWindow(WindowUIType.DialogMain);
    }
    public void Shutdown()
    {
        // �������Ҫ���˳�ʱ�ͷŻ򱣴����ݵķ���������ͳһ����
    }
}
