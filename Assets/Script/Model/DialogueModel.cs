// ================= Model层 =================
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对话数据模型（含网络层）
/// </summary>
public class DialogueModel
{

    /// <summary>
    /// 对话消息结构体
    /// <param name="content">内容长度需在1-1000字符之间</param>
    /// </summary>
    public class Message
    {
        public readonly Role role;
        public readonly string content;
        public readonly System.DateTime timestamp;
        public Message() { }
        public Message(Role role, string content)
        {
            if (string.IsNullOrEmpty(content) || content.Length > 1000)
                throw new System.ArgumentException("无效消息内容");

            this.role = role;
            this.content = content;
            this.timestamp = System.DateTime.Now;
        }
    }

    // 对话历史列表，公开以便序列化或外部访问
    private List<Message> _history = new List<Message>();


    public DialogueModel()
    {
    }

    public void AddMessage(Message msg)
    {
        _history.Add(msg);
    }
    public List<Message> GetHistory()
    {
        return _history;
    }
}