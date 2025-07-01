/// <summary>
/// MessageData 表示一条对话消息，包括角色（"User" 或 "Assistant"）和文本内容。
/// </summary>
public class MessageData
{
    public string timestamp;
    public string role { get; set; }
    public string content { get; set; }

    // 无参构造，用于 Json.NET 反序列化
    public MessageData() { }

    public MessageData(string r, string c, string timestamp)
    {
        role = r;
        content = c;
        this.timestamp = timestamp;
    }
}