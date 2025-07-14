public interface IMemoryManager
{
    /// <summary>
    /// 添加对话内容到记忆系统
    /// </summary>
    public void AddConversationMemory(string content, Role role);
}