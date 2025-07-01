using System;
using UnityEngine;
/// <summary>
/// 单个记忆条目的详细数据
/// </summary>
[System.Serializable]
public class MemoryRecord
{
    public string id;
    public string content;
    public Role role;
    public MemoryType type;             // 短期/长期记忆分类
    public float[] vector;              // 文本向量
    public float importance;            // 重要性权重 (0-1)
    public long timestamp;              // 创建时间戳
    public long lastAccessTimestamp;    // 最后访问时间戳
    public int accessCount;             // 被访问次数

    // 默认构造函数（用于反序列化）
    public MemoryRecord() { }

    // 实例化构造函数
    public MemoryRecord(string content,Role role, float[] vector, MemoryType type, float importance=0)
    {
        this.id = Guid.NewGuid().ToString();
        this.content = content;
        this.role = role; // 默认角色为用户，可根据需要修改
        this.vector = vector;
        this.type = type;
        this.importance = Mathf.Clamp01(importance);
        this.timestamp = DateTime.UtcNow.Ticks;
        this.lastAccessTimestamp = this.timestamp;
        this.accessCount = 0;
    }
}

public enum Role { User, Assistant }
/// <summary>
/// 记忆类型枚举
/// </summary>
public enum MemoryType
{
    ShortTerm,   // 短期记忆
    LongTerm,    // 长期记忆
    Core         // 核心记忆（如角色身份等）
}