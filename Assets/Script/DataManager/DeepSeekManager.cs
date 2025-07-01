// DeepSeekManager.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
public class DeepSeekManager : MonoBehaviour
{
    [Header("API 配置")]
    private string _apiKey;
    private string _apiEndpoint;
    [Header("对话设置")]
    private string _modelName;
    [Header("流式设置")]
    private bool _enableStreaming;
    private int _dataTimeout = 30;
    [Header("高级设置")]
    private int _maxRetries = 2;
    private int _maxTokenCount = 4000;
    private bool _isenableThinking = false;
    private int _thinkingBudget = 4096; // 思考预算，单位为秒
    private float _minP = 0.05f; // 最小概率阈值
    private string _stopmsg = string.Empty; // 停止标记
    private float _temperature = 0.5f; // 温度参数，控制输出的随机性
    private float _topP = 0.7f; // 停止概率阈值
    private float _topK = 50f; // Top-K采样参数
    private float _frequency_penalty = 0f; // 频率惩罚参数
    private int _n = 1;// 返回的候选答案数量
    #region 事件系统
    public event Action<string> OnStreamingResponse;
    public event Action<Exception> OnError;
    #endregion

    private bool isProcessing = false;

    public void Initialize(string _apiKey, string _apiEndpoint, string _modelName, bool enableStreaming, int dataTimeout, int maxRetries = 2)
    {
        this._apiKey = _apiKey;
        this._apiEndpoint = _apiEndpoint;
        this._modelName = _modelName;
        this._enableStreaming = enableStreaming;
        this._dataTimeout = dataTimeout;

        this._maxRetries = maxRetries;
    }
    /// <summary>
    /// 发送请求协程
    /// </summary>
    /// <param name="userInput">用户输入内容</param>
    /// <param name="memoryContext">历史上下文提示词</param>
    /// <returns></returns>
    public IEnumerator SendRequestCoroutine(string userInput, string memoryContext)
    {
        if (isProcessing)
        {
            ConsoleDebug.LogWarning("已有请求在处理中");
            yield break;
        }

        isProcessing = true;

        int retryCount = 0;//重试次数
        bool success = false;//请求是否成功
        var fullResponse = new StringBuilder(512); // 预分配512字节
        DateTime lastDataTime = DateTime.Now; // 记录最后接收数据的时间
        // 处理流式数据 该while处理重连次数
        while (!success && retryCount <= _maxRetries)
        {
            using (var request = CreateStreamingRequest(userInput, memoryContext))
            using (var handler = new EnhancedStreamHandler())
            {
                request.downloadHandler = handler;
                request.timeout = 0;

                var operation = request.SendWebRequest();
                lastDataTime = DateTime.Now;

                while (!operation.isDone)
                {
                    foreach (var chunk in handler.ProcessStream()) //处理数据流
                    {
                        var (content, reasoning, isEnd) = ProcessDataChunk(chunk);//处理数据块

                        if (!string.IsNullOrEmpty(reasoning))
                            OnStreamingResponse?.Invoke($"[REASONING]{reasoning}");

                        if (!string.IsNullOrEmpty(content))
                        {
                            fullResponse.Append(content);
                            
                            OnStreamingResponse?.Invoke(content);
                            lastDataTime = DateTime.Now;
                        }

                        if (isEnd) break;
                    }

                    if ((DateTime.Now - lastDataTime).TotalSeconds > _dataTimeout)//时间超时
                    {
                        request.Abort();
                        HandleTimeout(fullResponse.ToString());
                        yield break;
                    }

                    yield return null;
                }

                if (request.result == UnityWebRequest.Result.Success)//消息接收完毕
                {
                    foreach (var chunk in handler.GetRemainingChunks())
                    {
                        var (content, reasoning, _) = ProcessDataChunk(chunk);
                        if (!string.IsNullOrEmpty(content)) fullResponse.Append(content);
                    }
                    success = true;
                }
                else
                {
                    HandleNetworkError(request, fullResponse.ToString());
                }
            }

            if (!success && retryCount < _maxRetries)
            {
                retryCount++;
                ConsoleDebug.Log($"正在重试({retryCount}/{_maxRetries})...");
                yield return new WaitForSeconds(Mathf.Pow(2, retryCount));
            }
        }

        isProcessing = false;
        OnStreamingResponse?.Invoke("[END]"); // 确保发送结束标记
    }

    #region 核心解析逻辑
    private (string content, string reasoning, bool isEnd) ProcessDataChunk(byte[] chunk)
    {
        try
        {
            string json = Encoding.UTF8.GetString(chunk).Trim();
            if (json == "[DONE]") return (null, null, true);

            if (json.StartsWith("data:"))
                json = json.Substring(5).Trim();

            // 处理数组格式
            if (json.StartsWith("["))
            {
                var events = JsonConvert.DeserializeObject<List<StreamEvent>>(json, new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore
                });
                return events.Count > 0 ? ExtractContent(events[0]) : (null, null, false);
            }

            // 处理单个对象
            var evt = JsonConvert.DeserializeObject<StreamEvent>(json, new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            });
            return ExtractContent(evt);
        }
        catch (Exception ex)
        {
            ConsoleDebug.LogError($"解析失败: {ex.Message}\n原始数据: {Encoding.UTF8.GetString(chunk)}");
            return (null, null, false);
        }
    }

    private (string, string, bool) ExtractContent(StreamEvent evt)
    {
        var delta = evt?.choices?.FirstOrDefault()?.delta;
        bool isEnd = evt?.choices?.Any(c => !string.IsNullOrEmpty(c.finish_reason)) ?? false;
        return (
            delta?.content ?? string.Empty,
            delta?.reasoning_content ?? string.Empty,
            isEnd
        );
    }
    #endregion

    #region 辅助方法
    private UnityWebRequest CreateStreamingRequest(string userInput, string memoryContext)
    {
        var request = new UnityWebRequest(_apiEndpoint, "POST");
        ConsoleDebug.Log("总输入消耗token:" + GameRoot.Instance.SentisInference.ComputeConversationTokenCount(userInput + memoryContext + MemoryManager.Instance.Sysguize + MemoryManager.Instance.Sysjiyi).ToString());
        ConsoleDebug.Log("历史记忆:"+memoryContext);
        var requestBody = new
        {
            model = _modelName, // 调用指定模型
                                // 将 Message[] 转换为 API 需要的对象数组，role 转为小写字符串
            messages = new List<object> {
                new { role = "system", content = MemoryManager.Instance.Sysguize }, // 用户输入
                // 系统级永久设定
                new { role = "system", content = MemoryManager.Instance.Sysjiyi },
                // 系统级永久记忆
                new { role = "system", content = $"AI角色属性：{MemoryManager.Instance.SysAi}" }, // 系统级永久记忆
                new { role = "system", content =$"当前用户身份强制绑定：{MemoryManager.Instance.SysMy}"},

                // 组合上下文+当前输入
                new { role = "user", content = userInput },
                // 历史上下文提示词
                new { role = "user", content = memoryContext }
            },
            stream = _enableStreaming, // 启用流式传输
            max_tokens = _maxTokenCount, // 设置最大令牌数 本次回复生成的token上限
            enable_thinking = _isenableThinking, // 启用思考模式  目前只适用于千问3
            thinking_budget = _thinkingBudget, // 思考预算，单位为秒 适用于推理模型
            min_p = _minP, // 最小概率阈值 只适用于千问3 在核采样过程中，模型会把所有token按照概率从大到小排序，选出前topK个token，选出累计概率 ≥ top_p 的那一小撮 token 作为候选，然后再从这部分里随机采样。
            //min_p 相当于一个“最低累计概率”阈值。如果候选 token 的累计概率已经超过 top_p，但还没达到 min_p，就 强制多收录一些概率稍低的 token，直到累计概率 ≥ min_p，再从中随机选一个。
            //在某些情况下，如果 top_p 很小，模型的输出可能会变得非常单一，甚至无法生成有意义的内容。min_p 可以帮助模型在这种情况下仍然生成多样化的输出。
            stop = _stopmsg, // 停止标记，如果为空说明没有额外停止条件。不为空则表示在遇到该字符串时停止生成
            temperature = _temperature, // 温度参数，控制输出的随机性。越小越确定，越大越随机
            top_p = _topP, // 停止概率阈值,控制“核采样”的阈值 —— 选出概率累积到 top_p（本例中 0.7）以上的高概率 token 作为候选，然后再从中随机抽一个。越大越随机，与温度类似，一般二选一即可
            top_k = _topK, // Top-K采样参数，控制“核采样”的范围 —— 只考虑概率最高的 top_k 个 token。越大越随机，通常与 top_p 二选一即可
            //通常而言，每输出一个token，模型会根据当前上下文和之前生成的内容，重新计算所有可能的下一个token的概率分布。然后从高到低对所有token进行排序，累计概率，直到达到top_p阈值
            //然后从概率最高的前top_k个token中随机选择一个作为下一个输出的token。这个过程会持续进行，直到满足停止条件（如达到最大token数或遇到停止标记）。
            frequency_penalty = _frequency_penalty, // 频率惩罚参数，控制模型对重复内容的惩罚。值越大，模型越倾向于生成新内容而不是重复之前的内容
            n = _n, // 返回的候选答案数量
            //response_format：指定返回结果的格式。先用占位对象 { type = "text" }，后续如有需求再改为 "json" 或其它格式。
            response_format = new
            {
                type = "text"
            },
            // tools：告诉模型哪些插件或函数可用。这里先用一个空数组占位，
            // 后续如果需要真正调用函数/插件，再在这里填具体的名称、描述和参数。 返回后的内容通过解析可以调用不同的函数或插件。
            tools = new object[]
            {
            // 示例占位：
            // new
            // {
            //     type = "function",
            //     function = new
            //     {
            //         name = "myFunctionName",
            //         description = "这是一个示例函数，用于……",
            //         parameters = new
            //         {
            //             type = "object",
            //             properties = new
            //             {
            //                 param1 = new { type = "string", description = "参数1说明" },
            //                 param2 = new { type = "integer", description = "参数2说明" }
            //             },
            //             required = new[] { "param1" }
            //         },
            //         strict = false
            //     }
            // }
            }
        };
        byte[] payload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestBody));
        request.uploadHandler = new UploadHandlerRaw(payload);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {_apiKey}");
        request.SetRequestHeader("X-DashScope-SSE", "enable");
        return request;
    }



    private void HandleNetworkError(UnityWebRequest request, string receivedData)
    {
        var ex = new Exception($"网络错误: {request.error}\n已接收数据: {receivedData}");
        OnError?.Invoke(ex);
    }

    private void HandleTimeout(string receivedData)
    {
        var ex = new Exception($"流式超时\n已接收数据: {receivedData}");
        OnError?.Invoke(ex);
    }
    #endregion

    #region 流式处理器
    private class EnhancedStreamHandler : DownloadHandlerScript
    {
        private readonly object bufferLock = new object();
        private List<byte> buffer = new List<byte>(4096);
        private const string Delimiter = "\n\ndata: ";

        public IEnumerable<byte[]> ProcessStream()
        {
            lock (bufferLock)
            {
                byte[] delimiter = Encoding.UTF8.GetBytes(Delimiter);
                List<byte[]> chunks = new List<byte[]>();

                while (true)
                {
                    int index = FindDelimiter(delimiter);
                    if (index == -1) break;

                    byte[] chunk = new byte[index];
                    buffer.CopyTo(0, chunk, 0, index);
                    chunks.Add(chunk);
                    buffer.RemoveRange(0, index + delimiter.Length);
                }

                return chunks;
            }
        }

        public IEnumerable<byte[]> GetRemainingChunks()
        {
            lock (bufferLock)
            {
                if (buffer.Count == 0) yield break;
                yield return buffer.ToArray();
                buffer.Clear();
            }
        }

        private int FindDelimiter(byte[] delimiter)
        {
            for (int i = 0; i <= buffer.Count - delimiter.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < delimiter.Length; j++)
                {
                    if (buffer[i + j] != delimiter[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match) return i;
            }
            return -1;
        }

        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            if (data == null || dataLength == 0) return false;
            lock (bufferLock) buffer.AddRange(data.Take(dataLength));
            return true;
        }
    }
    #endregion

    #region 数据结构
    [System.Serializable]
    private class StreamEvent
    {
        public List<StreamChoice> choices;

        [System.Serializable]
        public class StreamChoice
        {
            public DeltaContent delta;
            public string finish_reason;

            [System.Serializable]
            public class DeltaContent
            {
                public string content;
                public string reasoning_content;
            }
        }
    }
    #endregion

}