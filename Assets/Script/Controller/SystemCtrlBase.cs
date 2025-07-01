using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class SystemCtrlBase<T> : IDisposable where T : new()
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new T();
            }
            return instance;
        }
    }

    public virtual void Dispose()
    {

    }
    /// <summary>
    /// 显示窗口
    /// </summary>
    /// <param name="title">标题</param>
    /// <param name="message">内容</param>
    /// <param name="okAction">类型</param>
    /// <param name="cancelAction">确定回调</param>
    /// <param name="type">取消回调</param>
    //protected void ShowMessage(string title, string message, Action okAction = null, Action cancelAction = null, MessageViewType type = MessageViewType.Ok)
    //{
    //    MessageCtrl.Instance.Show(title, message, okAction, cancelAction, type);
    //}
    /// <summary>
    /// 添加监听
    /// </summary>
    /// <param name="key"></param>
    /// <param name="handler"></param>
    protected void AddEventListener(string key, DispatcherBase<UIDispatcher, object[], string>.OnActionClickHandler handler)
    {
        UIDispatcher.Instance.AddEventListener(key, handler);
    }
    /// <summary>
    /// 移除监听
    /// </summary>
    /// <param name="key"></param>
    /// <param name="handler"></param>
    protected void RemoveEventListener(string key, DispatcherBase<UIDispatcher, object[], string>.OnActionClickHandler handler)
    {
        UIDispatcher.Instance.RemoveEventListener(key, handler);
    }
    /// <summary>
    /// 打印日志
    /// </summary>
    /// <param name="message"></param>
    protected void Log(object message)
    {
       ConsoleDebug.Log(message);
    }
    /// <summary>
    /// 打印错误日志
    /// </summary>
    /// <param name="message"></param>
    protected void LogError(object message)
    {
        ConsoleDebug.LogError(message);
    }
}