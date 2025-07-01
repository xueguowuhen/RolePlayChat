using System;
using System.Linq;
using System.Text;
using UnityEngine;
using static DialogueModel;

/// <summary>
/// 对话系统控制器（含网络异常处理）
/// <example>
/// controller.OnResponseReceived += UpdateUI;
/// controller.SendMessage("你好");
/// </example>
/// </summary>
[RequireComponent(typeof(DeepSeekManager))]
public class DialogueController : SystemCtrlBase<DialogueController>, ISystemCtrl
{
    #region 配置区
    [Header("API 配置")]
    [SerializeField] private string apiKey = "your-api-key";
    [SerializeField] private string apiEndpoint = "https://api.siliconflow.cn/v1/chat/completions";

    [Header("对话设置")]
    [Range(1, 20)]
    [SerializeField] private int maxHistory = 5;
    [SerializeField] private string modelName = "deepseek-ai/DeepSeek-V3";

    [Header("流式设置")]
    [SerializeField] private bool enableStreaming = true;
    [SerializeField] private int dataTimeout = 30;

    [Header("高级设置")]
    [SerializeField] private int maxRetries = 2;
    [SerializeField] private int maxTokenCount = 4000;
    [Header("服务配置")]
    [SerializeField] private int _maxRetries = 3;
    [SerializeField] private DialogueView _view;
    #endregion
    public event Action<string> OnResponseReceived;
    public event Action<Exception> OnErrorOccurred;

    private DialogueModel _model;
    private MemoryData memoryData; // 用于存储记忆数据
    private DeepSeekManager _networkService;
    private StringBuilder _aiResponseBuilder = new StringBuilder();
    public DialogueModel GetModel() => _model;

    public  DialogueController()
    {
        _model = new DialogueModel();//维护20条历史记录
       // _networkService = gameObject.GetComponent<DeepSeekManager>();
        memoryData = DataPersistenceManager.Instance.GetCurrentMemory();
        InitializeNetworkService();
        // 初始化事件绑定
        _view.OnSendMessage += HandleSendMessage;
        OnResponseReceived += _view.HandleAIResponse; // 新增事件绑定
        _view.InitializeUI();
    }

    private void HandleSendMessage(string message)
    {
        // 禁用UI输入
        _view.SetUIInteractable(false);

        // 显示用户消息
        _view.AppendCompleteMessage($"{message}", true);

        // 发送网络请求
        HandleNetworkRequest(message);
    }

    private void InitializeNetworkService()
    {
        _networkService.Initialize(apiKey, apiEndpoint, modelName, enableStreaming, dataTimeout);
        _networkService.OnStreamingResponse += HandleStreamResponse;
        _networkService.OnError += HandleNetworkError;
    }

    /// <summary>
    /// 发送用户消息
    /// <param name="message">需经过内容校验</param>
    /// </summary>
    public void HandleNetworkRequest(string message)
    {
        if (!ValidateMessage(message)) return;
        // 添加用户 消息到模型
        MemoryManager.Instance.AddConversationMemory(message, Role.User);// 添加到记忆
        var recalls = MemoryManager.Instance.RetrieveRelevantMemories(message);// 检索相关记忆提示词

        StringBuilder memoryContext = new StringBuilder();

        foreach (var recatext in recalls)
        {
            // 添加角色标识符以保留来源信息
            var rolePrefix = recatext.role == Role.User ? "[用户记忆] " : "[AI记忆] ";
            // 添加记忆类型标识
            var typeIdentifier = recatext.type switch
            {
                MemoryType.ShortTerm => "[短期]",
                MemoryType.LongTerm => "[长期]",
                MemoryType.Core => "[核心]",
                _ => ""
            };
            memoryContext.Append(rolePrefix + typeIdentifier + recatext.content);
        }

        AddMessageToHistory(Role.User, message); // 添加用户消息到历史记录
      //  StartCoroutine(_networkService.SendRequestCoroutine(message, memoryContext.ToString()));//历史消息

    }

    private bool ValidateMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            OnErrorOccurred?.Invoke(new ArgumentException("消息内容不能为空"));
            return false;
        }
        return true;
    }

    private void HandleStreamResponse(string chunk)
    {
        try
        {
            if (chunk.StartsWith("[REASONING]"))
            {
                HandleReasoningContent(chunk.Substring(11));
                return;
            }
            if (chunk == "[END]")
            {
                // 处理结束标记
                MemoryManager.Instance.AddConversationMemory(_aiResponseBuilder.ToString(), Role.Assistant);// 添加到记忆
                ConsoleDebug.Log("总输出消耗token:" + GameRoot.Instance.SentisInference.ComputeConversationTokenCount(_aiResponseBuilder.ToString()));
                AddMessageToHistory(Role.Assistant, _aiResponseBuilder.ToString());
                _aiResponseBuilder.Clear();
            }
            else
                _aiResponseBuilder.Append(chunk);
            OnResponseReceived?.Invoke(chunk);
        }
        catch (Exception ex)
        {
            OnErrorOccurred?.Invoke(ex);
        }
    }
    private void AddMessageToHistory(Role role, string content)
    {
        var msg = new Message(role, content);
        _model.AddMessage(msg);
    }

    // DialogueController.cs
    private Message[] GetOptimizedHistory()
    {
        // 转换为API需要的格式
        return _model.GetHistory().Select(m => new Message//构建新Message对象防止引用
                                                          // 直接使用原Message对象可能导致数据不一致
        (
             m.role,
            m.content
            )).ToArray();
    }

    private void HandleNetworkError(Exception ex)
    {
        Debug.LogError($"网络错误: {ex.Message}");
        OnErrorOccurred?.Invoke(ex);
    }

    private void HandleReasoningContent(string reasoning)
    {
        // 可扩展为独立事件
        OnResponseReceived?.Invoke($"[分析] {reasoning}");
    }

    public void OpenView(WindowUIType type)
    {
        
    }
}
