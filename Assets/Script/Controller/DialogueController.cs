using System;
using System.Buffers;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// 对话系统控制器（含网络异常处理）
/// <example>
/// controller.OnResponseReceived += UpdateUI;
/// controller.SendMessage("你好");
/// </example>
/// </summary>
public class DialogueController : SystemCtrlBase<DialogueController>, ISystemCtrl
{
    private DialogueView _view;
    public WindowUIType UIType => WindowUIType.DialogMain;
    public event Action<string> OnResponseReceived;
    public event Action<Exception> OnErrorOccurred;
    private DialogueModel _model;
    public DialogueModel GetModel() => _model;
    private readonly IChatService _chat;
    private readonly string _userColorHex;
    private readonly string _aiColorHex;
    private bool _isResponding;
    private readonly StringBuilder _aiBuffer = new StringBuilder();
    public DialogueController() { }
    public DialogueController(IChatService chat)
    {
        _chat = chat;
        _model = new DialogueModel();
        _view.OnSendMessage += HandleSendMessage;
        _view.InitializeUI();
        _isResponding = false;
    }

    private void HandleSendMessage(string message)
    {
        // 禁用UI输入
        _view.SetUIInteractable(false);

        // 追加用户消息（带色）
        var coloredUser = $"<color=#{_userColorHex}>{message}</color>";
        _view.AppendMessage(coloredUser, true);

        // 发送网络请求
        _view.StartCoroutine(_chat.SendMessageCoroutine(message, chunk => ProcessChunk(chunk), HandleNetworkError));
    }
    private void ProcessChunk(string chunk)
    {
        try
        {
            // [REASONING]
            if (chunk.StartsWith("[REASONING]"))
            {
                var reasoning = chunk.Substring(11);
                var colored = $"<color=#{_aiColorHex}>[分析]{reasoning}</color>";
                _view.AppendMessage(colored, false);
                return;
            }

            // [END]
            if (chunk == "[END]")
            {
                _isResponding = false;
                _view.SetUIInteractable(true);
                return;
            }

            // 首次响应
            if (!_isResponding)
            {
                _isResponding = true;
                _aiBuffer.Clear();
                _aiBuffer.Append(chunk);
                var colored = $"<color=#{_aiColorHex}>{chunk}</color>";
                _view.AppendMessage(colored, false);
            }
            else
            {
                // 累计响应
                _aiBuffer.Append(chunk);
                var colored = $"<color=#{_aiColorHex}>{_aiBuffer}</color>";
                _view.UpdateLastMessage(colored);
            }
        }
        catch (Exception ex)
        {
            HandleNetworkError(ex);
        }
    }
    private void HandleNetworkError(Exception ex)
    {
        Debug.LogError($"网络错误: {ex.Message}");
        OnErrorOccurred?.Invoke(ex);
    }
    /// <summary>
    /// 打开对话视图
    /// </summary>
    public void OpenDialogueOnView()
    {
        ServiceLocator.Container.GetService<IUIViewUtil>().LoadWindow(WindowUIType.DialogMain.ToString(), (GameObject obj) =>
        {
            _view = obj.GetComponent<DialogueView>();
        });
    }
    public void OpenView(WindowUIType type)
    {
        switch (type)
        {
            case WindowUIType.DialogMain:
                OpenDialogueOnView();
                break;
        }
    }
}
