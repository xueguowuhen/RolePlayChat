public interface IDataPersistenceManager
{
    /// <summary>
    /// 保存当前内存中的数据到磁盘，包括对话模型、设置和记忆。
    /// 如果 settings 或 memory 参数为 null，则使用 Manager 缓存的 _currentSettings、_currentMemory。
    /// </summary>
    /// <param name="model">当前的 DialogueModel 实例，不能为空</param>
    /// <param name="settings">要保存的 SettingData；传 null 则使用 _currentSettings</param>
    /// <param name="memory">要保存的 MemoryData；传 null 则使用 _currentMemory</param>
    public void SaveData(DialogueModel model, SettingData settings = null, MemoryData memory = null);
    /// <summary>
    /// 公开接口：从磁盘读取并返回 DialogueDataContainer，若主存档损坏则尝试从备份恢复；若成功则更新缓存设置/记忆并返回容器，否则返回 null。
    /// </summary>
    public DialogueDataContainer LoadData();

    /// <summary>
    /// 公开接口：获取当前唯一的 SettingData
    /// </summary>
    public SettingData GetCurrentSettings();

    /// <summary>
    /// 公开接口：获取当前唯一的 MemoryData
    /// </summary>
    public MemoryData GetCurrentMemory();

    public int GetRandomSize();
    /// <summary>
    /// 将当前内存模型（DialogueModel）、设置、记忆等做成带时间戳的备份文件。
    /// 备份文件名格式为 userdata_yyyyMMdd_HHmmss.json
    /// </summary>
    public void CreateBackup(DialogueModel model, SettingData settings = null, MemoryData memory = null);
}