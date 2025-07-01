/****************************************************
    文件：UIViewMgr
	作者：无痕
    邮箱: 1450411269@qq.com
    日期：2024-07-17 15:36:24
	功能：Nothing
*****************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class UIViewMgr : Singleton<UIViewMgr>
{
    private Dictionary<WindowUIType, ISystemCtrl> m_SystemCtrlDic = new Dictionary<WindowUIType, ISystemCtrl>();
    public UIViewMgr()
    {
        m_SystemCtrlDic.Add(WindowUIType.DialogMain, DialogueController.Instance);
    }
    public void OpenWindow(WindowUIType uiType)
    {
        m_SystemCtrlDic[uiType].OpenView(uiType);
        // AccountCtrl.Instance.OpenLogOnView();
    }
}