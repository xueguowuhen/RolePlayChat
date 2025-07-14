using System;
using System.Collections.Generic;
public class UIViewMgr : IUIViewMgr
{
    private readonly Dictionary<WindowUIType, ISystemCtrl> _systemCtrlDic = new();

    public UIViewMgr(IEnumerable<ISystemCtrl> controllers)
    {
        foreach (var ctrl in controllers)
        {
            _systemCtrlDic[ctrl.UIType] = ctrl;
        }
    }

    public void OpenWindow(WindowUIType uiType)
    {
        if (_systemCtrlDic.TryGetValue(uiType, out var ctrl))
            ctrl.OpenView(uiType);
        else
            ConsoleDebug.LogError($"UIViewMgr: 未注册窗口类型 {uiType}");
    }
}
