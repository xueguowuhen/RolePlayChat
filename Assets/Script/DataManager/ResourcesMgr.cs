using System.Collections;
using System.Text;
using UnityEngine;

public class ResourcesMgr : Singleton<ResourcesMgr>
{
    #region ResourceType 资源类型
    /// <summary>
    /// 资源类型
    /// </summary>
    public enum ResourceType
    {
        /// <summary>
        /// 场景UI
        /// </summary>
        UIScene,
        /// <summary>
        /// 窗口
        /// </summary>
        UIWindows,
        /// <summary>
        /// 其他
        /// </summary>
        Other,
        /// <summary>
        /// 窗口子项
        /// </summary>
        UIWindowsChild,
    }
    #endregion
    private Hashtable m_PrefabTable;
    public ResourcesMgr()
    {
        m_PrefabTable = new Hashtable();
    }
    #region Load 加载资源

    /// <summary>
    /// 加载资源
    /// </summary>
    /// <param name="type">资源类型</param>
    /// <param name="path">短路径</param>
    /// <param name="cache">是否放入缓存</param>
    /// <param name="returnClone">是否返回克隆体</param>
    /// <returns></returns>
    public GameObject Load(ResourceType type, string path, bool cache = false, bool returnClone = true)
    {
        StringBuilder sbr = new StringBuilder();
        GameObject obj = null;

        if (m_PrefabTable.Contains(path))
        {
            obj = m_PrefabTable[path] as GameObject;
        }
        else
        {
            switch (type)
            {
                case ResourceType.UIScene:
                    sbr.Append("UIPrefab/UIScence/");
                    break;
                case ResourceType.UIWindows:
                    sbr.Append("UIPrefab/UIWindows/");
                    break;
                case ResourceType.Other:
                    sbr.Append("UIPrefab/UIOther/");
                    break;
                case ResourceType.UIWindowsChild:
                    sbr.Append("UIPrefab/UIWindowsChild/");
                    break;
            }
            sbr.Append(path);
            obj = Resources.Load(sbr.ToString()) as GameObject;
            if (cache)
            {
                m_PrefabTable.Add(path, obj);
            }

        }
        if (returnClone)
        {
            return Object.Instantiate(obj);

        }
        else
        {
            return obj;
        }
    }
    #endregion
    #region Dispose 释放资源
    public override void Dispose()
    {
        base.Dispose();
        m_PrefabTable.Clear();
        Resources.UnloadUnusedAssets();//释放未使用的资源
    }

    #endregion
}