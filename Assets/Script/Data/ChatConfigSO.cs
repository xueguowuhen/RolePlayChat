using UnityEngine;

[CreateAssetMenu(fileName = "ChatConfig", menuName = "Configs/ChatConfig")]
public class ChatConfigSO : ScriptableObject
{
    [Header("API 设置")]
    public string apiKey = "your-api-key";
    public string apiEndpoint = "https://api.siliconflow.cn/v1/chat/completions";

    [Header("模型参数")]
    public string modelName = "deepseek-ai/DeepSeek-V3";

    [Header("对话设置")]
    [Range(1, 20)]
    public int maxHistory = 5;

    [Header("流式设置")]
    public bool enableStreaming = true;
    public int dataTimeout = 30;

    [Header("高级设置")]
    [Range(1, 10)] public int maxRetries = 2;
    public int maxTokenCount = 4000;

    [Header("服务配置")]
    [SerializeField] private int _maxRetries = 3;
}
