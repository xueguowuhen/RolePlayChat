using System;

using UnityEngine;

public class UIWindowViewBase : UIViewBase
{
    /// <summary>
    /// 挂点类型
    /// </summary>
    [SerializeField]
    public WindowUIContainerType containerType = WindowUIContainerType.Center;
    /// <summary>
    /// 打开方式
    /// </summary>
    [SerializeField]
    public WindowShowStyle showStyle = WindowShowStyle.Normal;
    /// <summary>
    /// 打开时间动画
    /// </summary>
    [SerializeField]
    public float duration;
    /// <summary>
    /// 当前窗口类型
    /// </summary>
    [HideInInspector]
    public string ViewName;
    /// <summary>
    /// 下一个窗口
    /// </summary>
    private WindowUIType m_NextOpenType;

    protected UIViewUtil uiView;
    protected override void OnAWake()
    {
        base.OnAWake();
        uiView= ServiceLocator.Container.GetService<UIViewUtil>();
    }
    protected override void OnBtnClick(GameObject gameObject)
    {
        base.OnBtnClick(gameObject);
        if (gameObject.name.Equals("btnClose", StringComparison.CurrentCultureIgnoreCase))
        {
            Close();
        }
    }
    /// <summary>
    /// 关闭窗口
    /// </summary>
    public virtual void Close()
    {
        uiView.CloseWindow(ViewName);
    }
    /// <summary>
    /// 关闭并打开下一个窗口
    /// </summary>
    public virtual void CloseAndOpenNext(WindowUIType nextType)
    {
        Close();
        m_NextOpenType = nextType;
    }
    protected override void BeforeOnDestroy()
    {
        LayerUIMgr.Instance.CheckOpenWindow();
        if (m_NextOpenType != WindowUIType.None)
        {
            ServiceLocator.Container.GetService<UIViewMgr>().OpenWindow(m_NextOpenType);
        }
    }
}