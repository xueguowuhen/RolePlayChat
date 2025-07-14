using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class ResourcesMgr : IResourcesManager
{
    public enum ResourceType { UIScene, UIWindows, UIWindowsChild, Other }

    private readonly Dictionary<string, GameObject> _cache = new();
    private readonly Dictionary<ResourceType, string> _basePaths = new()
    {
        { ResourceType.UIScene,       "UIPrefab/UIScence/" },
        { ResourceType.UIWindows,     "UIPrefab/UIWindows/" },
        { ResourceType.UIWindowsChild,"UIPrefab/UIWindowsChild/" },
        { ResourceType.Other,         "UIPrefab/UIOther/" }
    };

    public GameObject LoadPrefab(string path, bool cache = false, bool returnInstance = true)
    {
        if (string.IsNullOrEmpty(path)) //path路径不存在
            throw new ArgumentException("资源路径不能为空", nameof(path));

        if (cache && _cache.TryGetValue(path, out var cached)) //如果存在该资源
            return returnInstance ? Object.Instantiate(cached) : cached;

        var type = DetectResourceType(path, out var shortPath); //不存在则加载并取出
        var fullPath = _basePaths[type] + shortPath;

        var prefab = Resources.Load<GameObject>(fullPath);
        if (prefab == null)
            throw new InvalidOperationException($"加载资源失败: {fullPath}");

        if (cache)
            _cache[path] = prefab;

        return returnInstance ? Object.Instantiate(prefab) : prefab;
    }

    public void ClearCache()
    {
        _cache.Clear();
        Resources.UnloadUnusedAssets();
    }

    private ResourceType DetectResourceType(string path, out string shortPath)
    {
        foreach (var kv in _basePaths)
        {
            var key = kv.Key + "/";
            if (path.StartsWith(key, StringComparison.OrdinalIgnoreCase))
            {
                shortPath = path.Substring(key.Length);
                return kv.Key;
            }
        }
        shortPath = path;
        return ResourceType.UIWindows;
    }

}