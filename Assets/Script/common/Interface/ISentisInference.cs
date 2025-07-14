public interface ISentisInference
{
    /// <summary>
    /// 计算单句文本的Token数量
    /// </summary>
    public int ComputeTokenCount(string text);
    /// <summary>
    /// 计算对话文本的总Token数量（包含分句后的所有句子）
    /// </summary>
    public int ComputeConversationTokenCount(string conversation);
    /// <summary>
    /// Encode 将单一句子转换为归一化后的句向量（768 维），
    /// 包含分词、截断/填充、模型推理、归一化等步骤。
    /// </summary>
    /// <param name="text">待编码的输入句子</param>
    /// <returns>长度为 hiddenSize 的归一化向量</returns>
    public float[] Encode(string text);
    /// <summary>
    /// EncodeConversation 对一段对话文本执行混合池化（Mean + Max 拼接）编码，
    /// 返回维度为 hiddenSize * 2 的向量。
    /// 先拆句，再逐句调用 Encode，最后合成对话级向量。
    /// </summary>
    /// <param name="conversation">完整对话字符串</param>
    /// <returns>维度为 2*hiddenSize 的对话向量</returns>
    public float[] EncodeConversation(string conversation);
    /// <summary>
    /// ComputeSimilarity 计算两个向量的余弦相似度，范围 [-1, 1]，越接近 1 表示越相似。
    /// </summary>
    public float ComputeSimilarity(float[] vec1, float[] vec2);
}