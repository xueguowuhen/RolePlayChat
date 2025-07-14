// ================= IUIViewUtil.cs =================
using System;
using UnityEngine;

/// <summary>
/// 窗口管理工具接口，负责加载与关闭UI窗口
/// </summary>
public interface IUIViewUtil
{
    void LoadWindow(string viewName, Action<GameObject> onComplete, Action onShow = null);
    void CloseWindow(string viewName);
    void CloseAll();
    int OpenWindowCount { get; }
}