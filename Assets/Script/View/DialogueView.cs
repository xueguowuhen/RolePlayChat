// ================= View层（增强版） =================
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 对话UI视图（含完整事件绑定）
/// <example>
/// view.OnSendMessage += HandleSend;
/// view.InitializeUI();
/// </example>
/// </summary>
public class DialogueView : MonoBehaviour, IInfiniteVariableListHandler
{
    // 事件定义
    public event System.Action<string> OnSendMessage;

    #region UI组件
    [Header("UI组件")]
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private Button _sendButton;
    //[SerializeField] private Text _chatText;
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private InfiniteVariableList list;// 新增InfiniteVariableList引用
    [Header("消息预制体")]
    [SerializeField] private GameObject _userMessagePrefab;
    [SerializeField] private GameObject _aiMessagePrefab;
    //[SerializeField] private Transform _messageContainer;
    #endregion

    #region 显示设置

    [Header("显示设置")]
    [Range(0.01f, 0.5f)]
    [SerializeField] private float _typingSpeed = 0.05f;
    [SerializeField] private int _maxDisplayChars = 8000;

    // 新增颜色配置字段
    [Tooltip("用户消息颜色（支持HEX或Unity颜色选择）")]
    [SerializeField] private Color _userColor = new Color(0.2f, 0.6f, 1f);
    [Tooltip("AI消息颜色（支持HEX或Unity颜色选择）")]
    [SerializeField] private Color _aiColor = new Color(0.5f, 0.5f, 0.5f); // 原#808080对应的RGB
    #endregion
    // DialogueView.cs
    private StringBuilder _aiBuffer = new StringBuilder();
    private int _currentAIIndex = -1;      // 列表里最后一次 AddItem 返回的索引
    private bool _isAIResponding = false;
    private Coroutine _typingCoroutine;
    private bool _isTyping = false;

    /// <summary>
    /// UI初始化方法（必须调用）
    /// </summary>
    public void InitializeUI()
    {
        // 绑定UI事件
        _sendButton.onClick.AddListener(ProcessInput);
        _inputField.onEndEdit.AddListener(OnInputEnd);
        list.SetHandler(this, new List<object>());

        list.Clear();
        // 初始状态
        SetUIInteractable(true);
    }
    #region 消息处理

    // 修改后的消息追加方法
    public void AppendCompleteMessage(string text, bool isUser)
    {
        if (_isTyping)
            ForceFinishTyping();

        // 动态转换颜色为HEX
        string colorHex = isUser ?
            ColorUtility.ToHtmlStringRGB(_userColor) :
            ColorUtility.ToHtmlStringRGB(_aiColor);

        string coloredText = $"<color=#{colorHex}>{text}</color>";//消息处理完毕
        var message = new DialogueModel.Message(
    isUser ? Role.User : Role.Assistant,
    text
);
        list.AddItem(message);
        UpdateViewImmediate();
    }

    public void HandleAIResponse(string chunk)
    {
        if (chunk == "[END]")
        {
            if (_isAIResponding)
            {
                // 确保颜色标签闭合
                //_aiBuffer.Append("</color>\n\n");
                UpdateItem("</color>\n\n");
                UpdateViewImmediate();
                _isAIResponding = false;
                _currentAIIndex = -1;
            }
            SetUIInteractable(true);
            return;
        }

        if (!_isAIResponding)
        {
            _isAIResponding = true;
            // 动态转换颜色为HEX
            string colorHex = ColorUtility.ToHtmlStringRGB(_aiColor);
            _aiBuffer.Clear();
            _aiBuffer.Append($"<color=#{colorHex}>");
            var aimsg = new DialogueModel.Message(
Role.Assistant,
_aiBuffer.ToString()
);
            list.AddItem(aimsg);
            _currentAIIndex = list.Count - 1;
            // _aiBuffer.Append(chunk);

            // 直接修改_contentBuilder避免中间缓冲区
        }

        UpdateItem(chunk);
        UpdateViewImmediate();

        // 自动滚动到底部
        StartCoroutine(ScrollToBottom());
    }
    public void UpdateItem(string chunk)
    {
        // 直接追加到主内容
        //chunk = ReplaceKeycapEmojisWithSprites(chunk);
        _aiBuffer.Append(chunk);
        var msg = new DialogueModel.Message(
         Role.Assistant,
        _aiBuffer.ToString()
        );
        if (_currentAIIndex == -1)
        {
            _currentAIIndex = list.Count - 1; // 确保索引有效
            return;
        }
        list.UpdateItemAt(_currentAIIndex, msg);
    }

    #endregion

    #region 私有方法

    private void UpdateViewImmediate()
    {
        //_chatText.text = _contentBuilder.ToString();
        // Canvas.ForceUpdateCanvases();
        StartCoroutine(ScrollToBottom());
    }

    private IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        _scrollRect.normalizedPosition = Vector2.zero;
    }
    #endregion

    #region 输入处理
    private void ProcessInput()
    {
        if (string.IsNullOrWhiteSpace(_inputField.text)) return;

        OnSendMessage?.Invoke(_inputField.text.Trim());
        _inputField.text = "";
    }

    private void OnInputEnd(string input)
    {
        if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter))
            ProcessInput();
    }

    public void ForceFinishTyping()
    {
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _isTyping = false;
        }
    }

    public void SetUIInteractable(bool enable)
    {
        _inputField.interactable = enable;
        _sendButton.interactable = enable;
    }

    #endregion
    public string GetTemplateId(object data)
    {
        if (data is DialogueModel.Message msg)
            return msg.role ==Role.User ? "User" : "AI";

        return "AI";
    }



    public RectTransform GetTemplatePrefab(string templateId)
    {
        if (templateId == "User")
            return _userMessagePrefab.GetComponent<RectTransform>();
        else
            return _aiMessagePrefab.GetComponent<RectTransform>();
    }

    public void OnBind(RectTransform item, object data)
    {
        if (data is DialogueModel.Message msg)
        {
            var text = item.GetComponent<ChatRootItem>();
            if (text != null)
            {
                text.SetUI(msg.content, (msg.role == Role.User) ? "我" : "小懒");
            }
        }
    }

}