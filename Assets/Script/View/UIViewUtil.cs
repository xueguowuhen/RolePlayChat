using System;
using System.Collections.Generic;
using UnityEngine;

public class UIViewUtil : Singleton<UIViewUtil>
{
    private Dictionary<string, UIWindowViewBase> m_dicWindow = new Dictionary<string, UIWindowViewBase>();
    /// <summary>
    /// 已经打开的窗口数量
    /// </summary>
    public int OpenWindowCount
    {
        get { return m_dicWindow.Count; }
    }
    #region OpenWindow 打开窗口 

    /// <summary>
    /// 打开窗口
    /// </summary>
    /// <param name="type">窗口类型</param>
    /// <returns></returns>
    public void LoadWindow(string viewName, Action<GameObject> onComplete, Action OnShow = null)
    {
        if (m_dicWindow.TryGetValue(viewName, out UIWindowViewBase window) && window != null)
        {
            LayerUIMgr.Instance.SetLayer(window.gameObject);
            onComplete(window.gameObject);
            return;
        }
        GameObject obj = ResourcesMgr.Instance.Load(ResourcesMgr.ResourceType.UIWindows, string.Format("pan_{0}", viewName), true);
        UIWindowViewBase windowBase = obj.GetComponent<UIWindowViewBase>();
        if (windowBase == null) return;
        if (OnShow != null)
        {
            windowBase.OnShow = OnShow;
        }
        m_dicWindow[viewName] = windowBase;
        windowBase.ViewName = viewName;
        Transform transParent = null;
        switch (windowBase.containerType)
        {
            case WindowUIContainerType.Center:
                //transParent = UISceneCtrl.Instance.CurrentUIScence.Container_Center;
                break;
        }
        obj.transform.SetParent(transParent);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;
        //obj.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        obj.gameObject.SetActive(false);
        LayerUIMgr.Instance.SetLayer(obj);
        StartShowWindow(windowBase, true);
        if (onComplete != null)
        {
            onComplete(obj);
        }
    }
    #endregion
    #region CloseWindow 关闭窗口
    /// <summary>
    /// 关闭窗口
    /// </summary>
    /// <param name="type"></param>
    public void CloseWindow(string viewName)
    {
        if (m_dicWindow.ContainsKey(viewName))
        {
            StartShowWindow(m_dicWindow[viewName], false);
        }
    }
    /// <summary>
    /// 关闭所有窗口
    /// </summary>
    public void CloseAllWindow()
    {
        if (m_dicWindow != null)
        {
            m_dicWindow.Clear();
        }
    }
    #endregion
    #region StartShowWindow 开始打开窗口
    /// <summary>
    /// 开始打开窗口
    /// </summary>
    /// <param name="windowBase"></param>
    /// <param name="isOpen">是否打开</param>
    private void StartShowWindow(UIWindowViewBase windowBase, bool isOpen)
    {
        switch (windowBase.showStyle)
        {
            case WindowShowStyle.Normal:
                ShowNormal(windowBase, isOpen);
                break;
            case WindowShowStyle.CenterToBig:
                ShowCenterToBig(windowBase, isOpen);
                break;
            case WindowShowStyle.FormTop:
                ShowFromDir(windowBase, 0, isOpen);
                break;
            case WindowShowStyle.FromDown:
                ShowFromDir(windowBase, 1, isOpen);
                break;
            case WindowShowStyle.FromLeft:
                ShowFromDir(windowBase, 2, isOpen);
                break;
            case WindowShowStyle.FromRight:
                ShowFromDir(windowBase, 3, isOpen);
                break;

        }
    }
    #endregion
    #region ShowCenterToBig 中间变大
    /// <summary>
    /// 中间变大
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="isOpen"></param>
    private void ShowCenterToBig(UIWindowViewBase windowBase, bool isOpen)
    {
        windowBase.gameObject.SetActive(true);
        windowBase.transform.localScale = Vector3.zero;
        ////创建一个动画，但不进行播放
        //Tweener ts = windowBase.transform.DOScale(Vector3.one, windowBase.duration)
        //    .SetAutoKill(false).SetEase(GlobaInit.Instance.UIAnimationCurve).Pause().OnRewind(() =>
        //    {
        //        DestroyWindow(windowBase);
        //    });
        //if (isOpen)
        //{
        //    windowBase.transform.DOPlayForward();//开启状态为正向播放
        //}
        //else
        //{
        //    windowBase.transform.DOPlayBackwards();//反向播放
        //}
    }

    /// <summary>
    /// 放大方向
    /// </summary>
    /// <param name="windowBase"></param>
    /// <param name="dirType"></param>
    /// <param name="isOpen"></param>
    private void ShowFromDir(UIWindowViewBase windowBase, int dirType, bool isOpen)
    {
        windowBase.gameObject.SetActive(true);
        Vector3 from = Vector3.zero;
        switch (dirType)
        {
            case 0:
                from = new Vector3(0, 1000, 0);
                break;
            case 1:
                from = new Vector3(0, -1000, 0);
                break;
            case 2:
                from = new Vector3(1800, 0, 0);
                break;
            case 3:
                from = new Vector3(-1800, 0, 0);
                break;
        }
        windowBase.transform.localPosition = from;
        //windowBase.transform.DOLocalMove(Vector3.zero, windowBase.duration)
        //    .SetAutoKill(false).Pause().OnRewind(() =>
        //    {
        //        DestroyWindow(windowBase);
        //    });
        //if (isOpen)
        //{
        //    windowBase.transform.DOPlayForward();//开启状态为正向播放
        //}
        //else
        //{
        //    windowBase.transform.DOPlayBackwards();//反向播放
        //}
    }
    #endregion


    #region ShowNormal 各种打开效果
    private void ShowNormal(UIWindowViewBase windowBase, bool isOpen)
    {
        if (isOpen)
        {
            windowBase.gameObject.SetActive(true);
        }
        else
        {
            DestroyWindow(windowBase);
        }
    }
    #endregion

    #region DestroyWindow 销毁窗口
    /// <summary>
    /// 销毁窗口
    /// </summary>
    /// <param name="obj"></param>
    private void DestroyWindow(UIWindowViewBase windowBase)
    {
        LayerUIMgr.Instance.CheckOpenWindow();
        m_dicWindow.Remove(windowBase.ViewName);
        GameObject.Destroy(windowBase.gameObject);
    }
    #endregion
}