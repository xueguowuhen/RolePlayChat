using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfiniteVariableList : MonoBehaviour
{
    [Header("绑定 ScrollRect")]
    public ScrollRect scrollRect;
    private InfiniteVariableListCore core = new InfiniteVariableListCore();

    /// <summary>
    /// 初始化列表，请在 Start/InitializeUI 中调用一次
    /// </summary>
    public void SetHandler(IInfiniteVariableListHandler handler, IList<object> dataList)
    {
        core.Init(
            scrollRect,
            dataList,
            handler.GetTemplateId,
            handler.GetTemplatePrefab,
            handler.OnBind
        );
    }

    /// <summary>
    /// 动态在列表尾部添加一条新数据（例如新消息）
    /// </summary>
    public void AddItem(object data)
    {
        core.AddData(data);
    }
    public int Count => core.Count;
    /// <summary>
    /// 清空所有数据和 UI
    /// </summary>
    public void Clear()
    {
        core.Clear();
    }
    public bool Contains(object data)
    {
        return core.Contains(data);
    }

    /// <summary>
    /// 强制刷新整个列表（重算高度、重建所有可见项）
    /// </summary>
    public void Refresh()
    {
        core.RefreshAll();
    }
    /// <summary>
    /// 更新指定索引的数据并刷新这一项
    /// </summary>
    public void UpdateItemAt(int index, object data)
    {
        core.UpdateDataAt(index, data);
    }
}
