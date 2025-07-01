using System;
using System.Collections.Generic;

/// <summary>
/// 扩展 MemoryData 以包含完整的记忆系统
/// </summary>
public class MemoryData
{
    /// <summary>
    /// 短期记忆队列（按时间顺序，有限容量）
    /// </summary>
    public Queue<MemoryRecord> shortTermMemory = new Queue<MemoryRecord>();

    /// <summary>
    /// 长期记忆列表（按重要性排序）
    /// </summary>
    public List<MemoryRecord> longTermMemory = new List<MemoryRecord>();

    /// <summary>
    /// 核心记忆（角色基本设定等）
    /// </summary>
    public List<MemoryRecord> coreMemory = new List<MemoryRecord>();

    // 存储配置参数
    public int shortTermCapacity = 7;            // 短期记忆容量
    public float importanceThreshold = 0.75f;    // 转入长期记忆的阈值
    public bool enableMemoryDecay = true;        // 是否启用记忆衰减
    public float decayTimeScale = TimeSpan.TicksPerDay;
    public float minImportance = 0.1f;

    public MemoryData()
    {

    }
}