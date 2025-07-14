using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI 窗口管理工具（DI 版）
/// </summary>
public class UIViewUtil : IUIViewUtil
{
    private readonly IResourcesManager _resMgr;
    private readonly Dictionary<string, UIWindowViewBase> _windows = new();
    private readonly Transform _uiRoot;

    public int OpenWindowCount => _windows.Count;

    public UIViewUtil(IResourcesManager resMgr, Transform uiRoot)
    {
        _resMgr = resMgr;
        _uiRoot = uiRoot;
    }

    public void LoadWindow(string viewName, Action<GameObject> onComplete, Action onShow = null)
    {
        if (_windows.TryGetValue(viewName, out var existing))
        {
            onShow?.Invoke();
            onComplete(existing.gameObject);
            return;
        }

        // path 使用约定：UIWindows/pan_{viewName}
        var prefabPath = $"UIWindows/pan_{viewName}";
        var go = _resMgr.LoadPrefab(prefabPath, cache: true);
        var window = go.GetComponent<UIWindowViewBase>();
        if (window == null)
            throw new InvalidOperationException($"Prefab {prefabPath} 缺少 UIWindowViewBase 组件");

        window.ViewName = viewName;
        if (onShow != null) window.OnShow = onShow;

        _windows[viewName] = window;
        go.transform.SetParent(_uiRoot, worldPositionStays: false);
        go.transform.localScale = Vector3.one;
        go.SetActive(false);

        onComplete?.Invoke(go);
    }

    public void CloseWindow(string viewName)
    {
        if (_windows.TryGetValue(viewName, out var window))
        {
            _windows.Remove(viewName);
        }
    }

    public void CloseAll()
    {
        //foreach (var window in _windows.Values)
        //    window.Hide();
        _windows.Clear();
    }
}
