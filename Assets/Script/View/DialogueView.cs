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
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private InfiniteVariableList list;// 新增InfiniteVariableList引用
    [Header("消息预制体")]
    [SerializeField] private GameObject _userMessagePrefab;
    [SerializeField] private GameObject _aiMessagePrefab;
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
    public void AppendMessage(string text, bool isUser)
    {
        var col = isUser ? _userColor : _aiColor;
        var msg = new DialogueModel.Message(isUser ? Role.User : Role.Assistant, text);
        list.AddItem(msg);
        UpdateViewImmediate();
    }
    /// <summary>更新最后一条消息（带颜色标签）</summary>
    public void UpdateLastMessage(string coloredText)
    {
        int idx = list.Count - 1;
        if (idx < 0) return;
        var msg = new DialogueModel.Message(Role.Assistant, coloredText);
        list.UpdateItemAt(idx, msg);
        UpdateViewImmediate();
        // 自动滚动到底部
        StartCoroutine(ScrollToBottom());
    }
    #endregion

    #region 私有方法

    private void UpdateViewImmediate()
    {
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

    public void SetUIInteractable(bool enable)
    {
        _inputField.interactable = enable;
        _sendButton.interactable = enable;
    }

    #endregion
    public string GetTemplateId(object data)
    {
        if (data is DialogueModel.Message msg)
            return msg.role == Role.User ? "User" : "AI";

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