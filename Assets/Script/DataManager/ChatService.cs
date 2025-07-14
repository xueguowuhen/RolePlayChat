using System;
using System.Collections;
using System.Linq;
using System.Text;
using static DialogueModel;

/// <summary>
/// 对话服务外观，封装记忆、持久化、推理和网络通讯
/// </summary>
public class ChatService : IChatService
{
    private readonly MemoryManager _memory;
    private readonly DataPersistenceManager _persistence;
    private readonly DeepSeekManager _network;
    private readonly SentisInference _inference;
    private readonly ChatConfigSO _config;

    public ChatService(
        MemoryManager memory,
        DataPersistenceManager persistence,
        DeepSeekManager network,
        SentisInference inference,
        ChatConfigSO config)
    {
        _memory = memory;
        _persistence = persistence;
        _network = network;
        _inference = inference;
        _config = config;
    }

    public IEnumerator SendMessageCoroutine(
        string userText,
        Action<string> onChunk,
        Action<Exception> onError)
    {
        // 1. 记录并持久化用户消息
        _memory.AddConversationMemory(userText, Role.User);

        // 2. 构建 memoryContext
        var recalls = _memory.RetrieveRelevantMemories(userText);
        var memoryContext = new StringBuilder();
        foreach (var rec in recalls)
        {
            var prefix = rec.role == Role.User ? "[用户记忆] " : "[AI记忆] ";
            var typeId = rec.type switch
            {
                MemoryType.ShortTerm => "[短期]",
                MemoryType.LongTerm => "[长期]",
                MemoryType.Core => "[核心]",
                _ => string.Empty
            };
            memoryContext.Append(prefix).Append(typeId).Append(rec.content);
        }

        // 3. 初始化网络服务
        _network.Initialize(
            _config.apiKey,
            _config.apiEndpoint,
            _config.modelName,
            _config.enableStreaming,
            _config.dataTimeout,
            _config.maxRetries
        );

        // 用于拼接 AI 响应
        var aiBuffer = new StringBuilder();

        // 4. 订阅流式和错误事件
        _network.OnStreamingResponse += chunk =>
        {
            try
            {
                if (chunk.StartsWith("[REASONING]"))
                {
                    onChunk($"[分析] {chunk.Substring(11)}");
                    return;
                }
                if (chunk == "[END]")
                {
                    // 流结束，存储完整 AI 消息
                    var full = aiBuffer.ToString();
                    _memory.AddConversationMemory(full, Role.Assistant);
                    ConsoleDebug.Log("总输出消耗token:" + _inference.ComputeConversationTokenCount(full));
                    onChunk("[END]");
                    aiBuffer.Clear();
                }
                else
                {
                    aiBuffer.Append(chunk);
                    onChunk(chunk);
                }
            }
            catch (Exception ex)
            {
                onError(ex);
            }
        };

        _network.OnError += onError;

        // 5. 启动协程发送请求
        yield return _network.SendRequestCoroutine(userText, memoryContext.ToString());

        // 6. 取消订阅，防止内存泄漏
        _network.OnStreamingResponse -= onChunk;
        _network.OnError -= onError;
    }

}
