using System;
using UnityEngine;

public class GameRoot : MonoBehaviour
{
    public static GameRoot Instance = null;

    [Header("核心管理器")]
    public DataPersistenceManager dataPersistenceManager;
    public SentisInference SentisInference;
    public MemoryManager memoryManager;               // 新增 MemoryManager 引用
    public DialogueController dialogueController;
    private void Awake()
    {
        // 单例赋值，保证只保留一个 GameRoot
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitAllModules();
    }

    /// <summary>
    /// 按依赖顺序依次初始化各核心模块
    /// </summary>
    private void InitAllModules()
    {
        // 1. 持久化服务初始化（加载历史对话、记忆数据等）
        dataPersistenceManager.Init();

        // 2. 文本编码器初始化（提供向量化服务）
        SentisInference.Init();

        // 3. 记忆管理器初始化（依赖 DataPersistenceManager 与 SentisInference）
        memoryManager.Init();
      
        //进入对话界面
        UIViewMgr.Instance.OpenWindow(WindowUIType.DialogMain);
    }
}
