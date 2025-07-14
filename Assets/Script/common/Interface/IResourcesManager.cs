using UnityEngine;

/// <summary>
/// 资源管理器接口，支持加载并缓存预制体。
/// </summary>
public interface IResourcesManager
{
    GameObject LoadPrefab(string path, bool cache = false, bool returnInstance = true);
    void ClearCache();
}