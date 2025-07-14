using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// DataPersistenceManager 负责将对话历史、用户设置、记忆等数据序列化到本地磁盘，
/// 以及从本地磁盘读取并恢复到内存模型中。它采用单例模式，确保场景切换时不被销毁。
/// </summary>
public class DataPersistenceManager : MonoBehaviour, IInitializable, IDataPersistenceManager
{
    /// <summary>
    /// 单例实例，可全局访问
    /// </summary>
    public static DataPersistenceManager Instance { get; private set; }

    /// <summary>
    /// 主存档文件名
    /// </summary>
    private const string SAVE_FILENAME = "userdata.json";

    /// <summary>
    /// 当前数据结构版本号，每次对 DialogueDataContainer、SettingData、MemoryData 等类结构做改动时需自增
    /// </summary>
    private const int CURRENT_DATA_VERSION = 1;

    /// <summary>
    /// 写文件时的锁，确保多线程或重复保存不会冲突
    /// </summary>
    private readonly object _fileLock = new object();

    /// <summary>
    /// 存档文件完整路径 (Application.persistentDataPath/userdata.json)
    /// </summary>
    private string SavePath => Path.Combine(Application.persistentDataPath, SAVE_FILENAME);

    // 缓存的“当前”用户设置和记忆 — 若 SaveData 没传入，则使用它们
    private SettingData _currentSettings;
    private MemoryData _currentMemory;
    private int _randomSize;
    public void Initialize()
    {
        // 单例模式
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 尝试从磁盘加载一次
        var container = LoadContainerFromDisk();
        if (container != null)
        {
            _currentSettings = container.settings ?? new SettingData();
            _currentMemory = container.memory ?? new MemoryData();
        }
        else
        {
            _currentSettings = new SettingData();
            _currentMemory = new MemoryData();
        }
    }

    /// <summary>
    /// 保存当前内存中的数据到磁盘，包括对话模型、设置和记忆。
    /// 如果 settings 或 memory 参数为 null，则使用 Manager 缓存的 _currentSettings、_currentMemory。
    /// </summary>
    /// <param name="model">当前的 DialogueModel 实例，不能为空</param>
    /// <param name="settings">要保存的 SettingData；传 null 则使用 _currentSettings</param>
    /// <param name="memory">要保存的 MemoryData；传 null 则使用 _currentMemory</param>
    public void SaveData(DialogueModel model, SettingData settings = null, MemoryData memory = null)
    {
        if (model == null)
        {
            Debug.LogError("[DataPersistenceManager] SaveData 调用时 model 为 null，无法保存对话历史。");
            return;
        }

        // 若未传入则使用缓存，否则更新缓存
        if (settings == null)
            settings = _currentSettings;
        else
            _currentSettings = settings;

        if (memory == null)
            memory = _currentMemory;
        else
            _currentMemory = memory;

        // 构建容器
        DialogueDataContainer container = BuildContainer(model, settings, memory);

        // 使用 Newtonsoft.Json 序列化（Indented 便于阅读）
        string json = JsonConvert.SerializeObject(container, Formatting.Indented);

        // 原子写入
        WriteAtomically(SavePath, json);

#if UNITY_EDITOR
        ConsoleDebug.Log($"[DataPersistenceManager] 数据保存成功，路径：{SavePath}");
#endif
    }

    /// <summary>
    /// 公开接口：从磁盘读取并返回 DialogueDataContainer，若主存档损坏则尝试从备份恢复；若成功则更新缓存设置/记忆并返回容器，否则返回 null。
    /// </summary>
    public DialogueDataContainer LoadData()
    {
        var container = LoadContainerFromDisk();
        if (container != null)
        {
            _currentSettings = container.settings ?? new SettingData();
            _currentMemory = container.memory ?? new MemoryData();
        }
        return container;
    }

    /// <summary>
    /// 公开接口：获取当前唯一的 SettingData
    /// </summary>
    public SettingData GetCurrentSettings()
    {
        return _currentSettings;
    }

    /// <summary>
    /// 公开接口：获取当前唯一的 MemoryData
    /// </summary>
    public MemoryData GetCurrentMemory()
    {
        return _currentMemory;
    }

    public int GetRandomSize()
    {
        return _randomSize;
    }
    /// <summary>
    /// 私有方法：从磁盘读取主存档并反序列化；若异常则尝试备份恢复；最终返回有效的 DialogueDataContainer 或 null。
    /// </summary>
    private DialogueDataContainer LoadContainerFromDisk()
    {
        if (!File.Exists(SavePath))
        {
#if UNITY_EDITOR
            ConsoleDebug.Log($"[DataPersistenceManager] 主存档不存在：{SavePath}");
#endif
            return null;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            if (string.IsNullOrWhiteSpace(json))
                throw new Exception("存档文件为空或仅包含空白");

            // 使用 Newtonsoft.Json 反序列化
            DialogueDataContainer container = JsonConvert.DeserializeObject<DialogueDataContainer>(json);
            if (container.randomSize == 0)
            {
                // 如果 randomSize 为 0，则生成一个随机数作为种子
                container.randomSize = new System.Random((int)DateTime.Now.Ticks).Next(1, int.MaxValue);
            }
            if (container == null)
                throw new Exception("反序列化后 container 为 null");

            // 版本迁移
            if (container.dataVersion < CURRENT_DATA_VERSION)//比对版本号
            {
                MigrateContainer(container, container.dataVersion);
                container.dataVersion = CURRENT_DATA_VERSION;
            }

            return container;
        }
        catch (Exception e)
        {
            ConsoleDebug.LogError($"[DataPersistenceManager] 主存档加载失败：{e}\n尝试从备份恢复...");
            var backupContainer = LoadFromBackupFiles();
            if (backupContainer != null)
            {
#if UNITY_EDITOR
                ConsoleDebug.Log($"[DataPersistenceManager] 成功从备份恢复存档");
#endif
                return backupContainer;
            }

            ConsoleDebug.LogWarning($"[DataPersistenceManager] 所有备份均无法加载，返回 null");
            return null;
        }
    }

    /// <summary>
    /// 构建一个包含对话历史、设置、记忆的容器，便于序列化。
    /// </summary>
    private DialogueDataContainer BuildContainer(DialogueModel model, SettingData settings, MemoryData memory)
    {
        DialogueDataContainer container = new DialogueDataContainer
        {
            dataVersion = CURRENT_DATA_VERSION,
            randomSize = _randomSize, // 使用当前随机数种子
            settings = settings,
            memory = memory,
        };
        return container;
    }

    /// <summary>
    /// 原子写入：先写到临时文件，然后用 Replace/Move 覆盖目标文件，以避免写入中断导致主文件损坏。
    /// </summary>
    private void WriteAtomically(string targetPath, string content)
    {
        lock (_fileLock)
        {
            string tmpPath = targetPath + ".tmp";

            try
            {
                File.WriteAllText(tmpPath, content);// 写入临时文件

                if (File.Exists(targetPath))//如果目标文件已存在，使用 Replace 方法原子性地替换
                {
                    // 注意：Replace 会在替换后删除临时文件
                    File.Replace(tmpPath, targetPath, null);
                }
                else
                {
                    File.Move(tmpPath, targetPath);
                }
            }
            catch (Exception e)
            {
                ConsoleDebug.LogError($"[DataPersistenceManager] 原子写入失败：{e}");
                if (File.Exists(tmpPath))
                {
                    try { File.Delete(tmpPath); } catch { }
                }
            }
        }
    }

    /// <summary>
    /// 查找并加载最新的备份文件。备份文件命名规则为 "userdata_yyyyMMdd_HHmmss.json"。
    /// 若找到合法备份则做反序列化并返回；否则返回 null。
    /// </summary>
    private DialogueDataContainer LoadFromBackupFiles()
    {
        try
        {
            string dir = Path.GetDirectoryName(SavePath);// 获取存档目录
            var backupFiles = Directory
                .GetFiles(dir, "userdata_*.json")
                .OrderByDescending(f => f)
                .ToArray();// 按时间降序排列备份文件

            foreach (var file in backupFiles) //按照时间排序从新到旧遍历备份文件列表
            {
                try
                {
                    string bakJson = File.ReadAllText(file);// 读取备份文件内容
                    if (string.IsNullOrWhiteSpace(bakJson))
                        continue;

                    DialogueDataContainer bakContainer = JsonConvert.DeserializeObject<DialogueDataContainer>(bakJson);
                    if (bakContainer != null)//如果反序列化成功则恢复
                    {
                        if (bakContainer.dataVersion < CURRENT_DATA_VERSION)
                        {
                            MigrateContainer(bakContainer, bakContainer.dataVersion);
                            bakContainer.dataVersion = CURRENT_DATA_VERSION;
                        }
                        return bakContainer;
                    }
                }
                catch
                {
                    // 当前备份无法加载，则继续尝试下一个
                }
            }
        }
        catch (Exception e)
        {
            ConsoleDebug.LogError($"[DataPersistenceManager] 读取备份列表失败：{e}");
        }

        return null;
    }

    /// <summary>
    /// 对低版本的 container 执行数据结构迁移，将旧存档升级到 CURRENT_DATA_VERSION。
    /// 根据版本差异在此处填充迁移逻辑。
    /// </summary>
    private void MigrateContainer(DialogueDataContainer container, int fromVersion) //旧版本号升级为新版本号
    {
        // 示例迁移逻辑框架：
        // if (fromVersion == 1)
        // {
        //     // 旧版本只有 dialogueHistory，没有 settings 或 memory，需要填充默认值
        //     if (container.settings == null)
        //         container.settings = new SettingData();
        //     if (container.memory == null)
        //         container.memory = new MemoryData();
        //     fromVersion = 2;
        // }
        //
        // if (fromVersion == 2)
        // {
        //     // 版本 2->3 时，新增加了某个字段，需要赋予默认值。例如：
        //     // container.settings.darkModeEnabled = false;
        //     fromVersion = 3;
        // }
        //
        // ……直至 fromVersion == CURRENT_DATA_VERSION

        // TODO: 根据实际业务需求补充迁移步骤
    }

    /// <summary>
    /// 将当前内存模型（DialogueModel）、设置、记忆等做成带时间戳的备份文件。
    /// 备份文件名格式为 userdata_yyyyMMdd_HHmmss.json
    /// </summary>
    public void CreateBackup(DialogueModel model, SettingData settings = null, MemoryData memory = null)
    {
        if (model == null)
        {
            ConsoleDebug.LogError("[DataPersistenceManager] CreateBackup 调用时 model 为 null，无法创建备份。");
            return;
        }

        if (settings == null)
            settings = _currentSettings;
        if (memory == null)
            memory = _currentMemory;

        DialogueDataContainer container = BuildContainer(model, settings, memory);
        string json = JsonConvert.SerializeObject(container, Formatting.Indented);

        string dir = Path.GetDirectoryName(SavePath);
        string baseName = Path.GetFileNameWithoutExtension(SAVE_FILENAME);
        string ext = Path.GetExtension(SAVE_FILENAME);
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string backupName = $"{baseName}_{timestamp}{ext}";
        string backupPath = Path.Combine(dir, backupName);

        lock (_fileLock)
        {
            try
            {
                File.WriteAllText(backupPath, json);
#if UNITY_EDITOR
                ConsoleDebug.Log($"[DataPersistenceManager] 备份已创建：{backupPath}");
#endif
            }
            catch (Exception e)
            {
                ConsoleDebug.LogError($"[DataPersistenceManager] 备份写入失败：{e}");
            }
        }
    }
}

/// <summary>
/// DialogueDataContainer 是将要持久化的所有数据打包在一起的类：
/// 包括存档版本号、对话历史、用户设置、记忆等。以后若要新增字段，只需在此添加即可。
/// </summary>
public class DialogueDataContainer
{
    /// <summary>
    /// 数据结构版本号，每次修改字段结构时须自增
    /// </summary>
    public int dataVersion { get; set; } = 1;

    public int randomSize;//生成一个随机数的种子
    /// <summary>
    /// 用户设置数据，可根据需求扩展字段
    /// </summary>
    public SettingData settings { get; set; } = new SettingData();

    /// <summary>
    /// 用户记忆数据，可根据需求扩展字段
    /// </summary>
    public MemoryData memory { get; set; } = new MemoryData();

    public DialogueModel dialogueHistory { get; set; } = new DialogueModel(); // 对话历史数据
}





