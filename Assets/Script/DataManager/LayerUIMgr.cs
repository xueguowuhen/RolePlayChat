using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class LayerUIMgr : Singleton<LayerUIMgr>
{
    /// <summary>
    /// 层级深度
    /// </summary>
    private int m_UIViewLayer = 50;
    /// <summary>
    /// 重置
    /// </summary>
    public void Reset()
    {
        m_UIViewLayer = 0;
    }
    /// <summary>
    /// 检查窗口数量
    /// </summary>
    public void CheckOpenWindow()
    {
        if (UIViewUtil.Instance.OpenWindowCount == 0)
        {
            Reset();
        }
    }
    /// <summary>
    /// 设置层级
    /// </summary>
    /// <param name="obj"></param>
    public void SetLayer(GameObject obj)
    {
        m_UIViewLayer++;
        Canvas m_Canvas = obj.GetComponent<Canvas>();
        m_Canvas.sortingOrder = m_UIViewLayer;
    }
}