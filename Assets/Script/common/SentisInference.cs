using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.ML.Tokenizers;
using Unity.InferenceEngine;
using UnityEngine;

/// <summary>
/// SentisInference 负责加载并运行一个 ONNX 文本模型，将文本转换为向量（Embedding），
/// 并提供单句和对话级别的向量化接口，以及相似度计算方法。
/// </summary>
public partial class SentisInference : MonoBehaviour
{
    // -------------------- 配置字段 --------------------

    [Header("将 gte-base-zh 模型拖拽到这里")]
    /// <summary>
    /// 在 Unity Inspector 中，拖拽你的 ONNX 模型资源到此处。
    /// </summary>
    public ModelAsset onnxModelAsset;

    // -------------------- 私有成员 --------------------

    private Worker worker;          // ONNX 推理执行器，用于计算模型输出
    private Tokenizer tokenizer;    // 文本分词器，将原始文本拆分为 token id 序列
    private string[] outputNames;   // 模型的输出层名称列表
    private int hiddenSize = 768;   // 模型句向量维度（一般为 768）
    private Stopwatch inferenceTimer = new Stopwatch(); // 用于测量每次推理耗时

    // -------------------- 常量 --------------------

    private const int MAX_SEQ_LENGTH = 512; // 模型支持的最大 token 序列长度
    private const int PAD_ID = 0;           // 填充时使用的 token id，一般对应 [PAD]
    /// <summary>
    /// Start 在 Awake 后运行，仅执行一次。
    /// 负责加载 ONNX 模型，创建推理执行器，并触发测试方法。
    /// </summary>
    public void Init()
    {
        // 1. 从 StreamingAssets 加载词表文件路径
        string vocabPath = Path.Combine(Application.streamingAssetsPath, "gte-base-zh/vocab.txt");

        // 2. 创建 WordPiece 分词器实例，配置未知词替换符号和单词最大字符数
        tokenizer = WordPieceTokenizer.Create(vocabPath, new WordPieceOptions
        {
            UnknownToken = "[UNK]",          // 生僻字符用 [UNK] 替代
            ContinuingSubwordPrefix = string.Empty, // 中文不需要子词前缀
            MaxInputCharsPerWord = 200        // 单个词最大字符数，防止异常过长
        });
        // 1. 加载 ONNX 模型资源
        var model = ModelLoader.Load(onnxModelAsset);

        // 2. 创建推理执行器，指定使用 GPU（如可用）
        worker = new Worker(model, BackendType.GPUCompute);

        // 3. 提取模型所有输出层的名称，通常只使用第一个
        outputNames = model.outputs.Select(o => o.name).ToArray();

        // 4. 运行内部测试示例
        // RunTestTexts();
    }

    // -------------------- 测试示例 --------------------

    /// <summary>
    /// RunTestTexts 演示如何调用单句编码和对话编码，以及相似度计算。
    /// </summary>
    //public void RunTestTexts()
    //{
    //    try
    //    {
    //        // 准备一组示例文本
    //        var texts = new Dictionary<string, string>()
    //        {
    //            {"文本1", "今天天气很好，我出门玩了"},
    //            {"文本2", "昨日NBA总决赛，湖人队获得总冠军"},
    //            {"文本3", "“对，罗峰师兄，能走到今天这一步，可完全是刻苦修炼。靠自己一拳一脚练出来的。哪像张昊白他们两个。”壮硕男生握紧拳头，深吸一口气，“我的目标就是罗峰师兄，我一定要在四年内，也就是大学毕业前，通过武馆考核，得到武馆的‘高级学员’称号！”"},
    //            {"文本4", "“耳听为虚，眼见为实。哼哼，看到了吧？罗峰师兄和另外两个可不一样。”壮硕男生撇嘴道，“那个张昊白和柳婷，家里都是富豪。从小家里花了大量金钱去培养，才能有这么强。至于罗峰师兄，和他们可不同！”"}
    //        };

    //        //// 示例：对一整段对话进行编码（混合池化）
    //        //string dialogue = "你好！最近怎么样？我想继续我们的冒险。";
    //        //float[] convoVec = EncodeConversation(dialogue);
    //        //ConsoleDebug.Log($"对话向量长度: {convoVec.Length}"); // 1536

    //        // 单句编码测试
    //        var embeddings = new Dictionary<string, float[]>();
    //        foreach (var (key, text) in texts)
    //        {
    //            embeddings[key] = EncodeConversation(text);
    //        }

    //        // 打印几组文本之间的余弦相似度
    //        ConsoleDebug.Log("\n🔑 文本语义相似度分析:");
    //        ConsoleDebug.Log($"1 vs 2: {ComputeSimilarity(embeddings["文本1"], embeddings["文本2"]):F4}");
    //        ConsoleDebug.Log($"1 vs 3: {ComputeSimilarity(embeddings["文本1"], embeddings["文本3"]):F4}");
    //        ConsoleDebug.Log($"1 vs 4: {ComputeSimilarity(embeddings["文本1"], embeddings["文本4"]):F4}");
    //        ConsoleDebug.Log($"3 vs 4: {ComputeSimilarity(embeddings["文本3"], embeddings["文本4"]):F4}");
    //    }
    //    catch (Exception ex)
    //    {
    //        ConsoleDebug.LogError($"测试失败: {ex.Message}");
    //    }
    //}

    // -------------------- 有用的工具方法 --------------------

    /// <summary>
    /// 将一段对话文本拆分成若干句子，用于后续句子级别编码。
    /// 支持中文标点和换行符分割。
    /// </summary>
    private List<string> SplitSentences(string conversation)
    {
        return conversation
            .Split(new[] { '。', '！', '？', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())            // 去除首尾空白
            .Where(s => s.Length > 0)         // 过滤空字符串
            .ToList();                        // 返回句子列表
    }

    // -------------------- 核心功能：文本编码 --------------------
    /// <summary>
    /// 计算单句文本的Token数量
    /// </summary>
    public int ComputeTokenCount(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;

        // 使用分词器获取token ID序列
        var encoding = tokenizer.EncodeToIds(text);
        return encoding.Count;
    }
    /// <summary>
    /// 计算对话文本的总Token数量（包含分句后的所有句子）
    /// </summary>
    public int ComputeConversationTokenCount(string conversation)
    {
        if (string.IsNullOrEmpty(conversation)) return 0;

        int totalTokens = 0;
        var clauses = SplitSentences(conversation);

        foreach (var clause in clauses)
        {
            totalTokens += ComputeTokenCount(clause);
        }
        return totalTokens;
    }
    /// <summary>
    /// Encode 将单一句子转换为归一化后的句向量（768 维），
    /// 包含分词、截断/填充、模型推理、归一化等步骤。
    /// </summary>
    /// <param name="text">待编码的输入句子</param>
    /// <returns>长度为 hiddenSize 的归一化向量</returns>
    public float[] Encode(string text)
    {
        // 1. 分词：将文本拆分为 token id 序列
        var encoding = tokenizer.EncodeToIds(text);
        var ids = encoding.Select(id => (int)id).ToList();

        // 2. 截断：如果句子太长，保留前 MAX_SEQ_LENGTH 个 token
        if (ids.Count > MAX_SEQ_LENGTH)
        {
            ids = ids.Take(MAX_SEQ_LENGTH).ToList();
            ConsoleDebug.LogWarning($"输入长度超过 {MAX_SEQ_LENGTH}，已截断到最大长度。");
        }

        // 3. 填充和注意力掩码：保证长度固定为 MAX_SEQ_LENGTH
        var padded = new int[MAX_SEQ_LENGTH]; // token id 数组
        var mask = new int[MAX_SEQ_LENGTH]; // attention mask 掩码数组
        int realLen = ids.Count;
        // 3.1 复制真实 token，并将掩码置为 1
        for (int i = 0; i < realLen; i++)
        {
            padded[i] = ids[i];
            mask[i] = 1;
        }
        // 3.2 剩余位置填 PAD，并将掩码置为 0
        for (int i = realLen; i < MAX_SEQ_LENGTH; i++)
        {
            padded[i] = PAD_ID;
            mask[i] = 0;
        }

        // 4. 调用 RunModel 进行推理，并返回归一化后的句向量
        return RunModel(padded, mask);
    }

    /// <summary>
    /// EncodeConversation 对一段对话文本执行混合池化（Mean + Max 拼接）编码，
    /// 返回维度为 hiddenSize * 2 的向量。
    /// 先拆句，再逐句调用 Encode，最后合成对话级向量。
    /// </summary>
    /// <param name="conversation">完整对话字符串</param>
    /// <returns>维度为 2*hiddenSize 的对话向量</returns>
    public float[] EncodeConversation(string conversation)
    {
        inferenceTimer.Restart();
        // 1. 拆句
        var clauses = SplitSentences(conversation);
        if (clauses.Count == 0)
            return new float[hiddenSize * 2];

        // 2. 对每句调用单句编码
        var vecs = clauses.Select(clause => Encode(clause)).ToList();

        // 3. 平均池化（Mean Pooling）：
        //    原理：将所有句向量按维度求平均，得到“整体语义”向量。
        var meanVec = new float[hiddenSize];
        foreach (var v in vecs)//遍历所有文本向量
            for (int i = 0; i < hiddenSize; i++) //遍历所有维度
                meanVec[i] += v[i]; // 累加每个句向量的对应维度值
        for (int i = 0; i < hiddenSize; i++)
            meanVec[i] /= vecs.Count; // 求平均值

        // 4. 最大池化（Max Pooling）：
        //    原理：对每个维度取多个句向量的最大值，
        //         强调最显著的特征。
        var maxVec = new float[hiddenSize];
        for (int i = 0; i < hiddenSize; i++) maxVec[i] = float.MinValue;
        foreach (var v in vecs)
            for (int i = 0; i < hiddenSize; i++)
                maxVec[i] = Mathf.Max(maxVec[i], v[i]);// 遍历所有文本向量，取每个维度的最大值


        // 5. 拼接（Concatenate）：
        //    将 meanVec 和 maxVec 首尾相连，形成长度为 hiddenSize*2 的对话向量，
        //    同时保留了“整体印象”和“局部高光”信息。
        var convoVec = new float[hiddenSize * 2];
        for (int i = 0; i < hiddenSize; i++)
        {
            convoVec[i] = meanVec[i];// 平均池化结果
            convoVec[i + hiddenSize] = maxVec[i]; // 最大池化结果
        }
        ConsoleDebug.Log($"{conversation}处理耗时: {inferenceTimer.ElapsedMilliseconds}ms");
        return convoVec;
    }

    // -------------------- 相似度计算 --------------------

    /// <summary>
    /// ComputeSimilarity 计算两个向量的余弦相似度，范围 [-1, 1]，越接近 1 表示越相似。
    /// </summary>
    public float ComputeSimilarity(float[] vec1, float[] vec2)
    {
        // 安全检查：确保两个向量非空且等长
        if (vec1 == null || vec2 == null || vec1.Length != vec2.Length)
            return 0f;

        // 计算点积和模长
        float dot = 0f, mag1 = 0f, mag2 = 0f;
        for (int i = 0; i < vec1.Length; i++)
        {
            dot += vec1[i] * vec2[i];
            mag1 += vec1[i] * vec1[i];
            mag2 += vec2[i] * vec2[i];
        }
        float denom = Mathf.Sqrt(mag1) * Mathf.Sqrt(mag2);
        // 防止除零
        return denom < 1e-6f ? 0f : dot / denom;
    }

    // -------------------- 模型推理 --------------------

    /// <summary>
    /// RunModel 执行 ONNX 模型推理：
    /// 接收已填充好的 inputIds 和 attentionMask，输出 [CLS] 位置的句向量，归一化后返回。
    /// </summary>
    private float[] RunModel(int[] inputIds, int[] attMask)
    {

        // 1. 构建输入张量，形状 [1, seqLen] 张量维度说明，批量大小为 1，序列长度为 inputIds 的长度
        var shape = new TensorShape(1, inputIds.Length);
        // 注意：ONNX 模型通常需要输入为 [batch_size, seq_length] 的形状
        // 因此这里使用 Tensor<int> 来创建输入张量
        //Tensor的int 类型用于存储 token id 和 attention mask
        using var idTensor = new Tensor<int>(shape, inputIds);
        using var maskTensor = new Tensor<int>(shape, attMask);

        // 2. 设置模型输入
        //输入了两个张量：input_ids 和 attention_mask
        //将注意力掩码传入模型，确保模型只关注实际 token
        worker.SetInput("input_ids", idTensor);
        worker.SetInput("attention_mask", maskTensor);

        // 3. 执行推理
        worker.Schedule();

        // 4. 获取输出张量，下载为数组
        var outputName = outputNames.First();//取模型第一个输出层的名称
        //获取推理结果，并将其转换为 Tensor<float> 类型
        using var output = worker.PeekOutput(outputName) as Tensor<float>;
        //将张量数据下载为一维数组
        float[] hiddenState = output.DownloadToArray();

        // 5. 取 [CLS] 第一个位置的向量（前 hiddenSize 个数值）
        //前768个数值即为[CLS]位置标记的完整向量
        var sentenceVec = new float[hiddenSize];
        Array.Copy(hiddenState, 0, sentenceVec, 0, hiddenSize);

        // 6. L2 归一化，保证长度为 1
        float norm = Mathf.Sqrt(sentenceVec.Sum(x => x * x));
        for (int i = 0; i < hiddenSize; i++)
            sentenceVec[i] /= norm;

        return sentenceVec;
    }

    /// <summary>
    /// OnDestroy 在对象销毁时释放资源，避免内存泄漏。
    /// </summary>
    void OnDestroy()
    {
        worker?.Dispose();
    }
}
