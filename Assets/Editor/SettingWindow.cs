/****************************************************
    文件：SettingWindow.cs
	作者：无痕
    邮箱: 1450411269@qq.com
    日期：2024/7/13 22:29:43
	功能：Nothing
*****************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

public class SettingWindow : EditorWindow
{
    private List<MacorItem> m_list = new List<MacorItem>();
    private Dictionary<string, bool> m_Dic = new Dictionary<string, bool>();
    private string m_Macor = null;
    public SettingWindow()
    {

    }
    private void OnEnable()
    {
        m_Macor = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Android);
        m_list.Clear();
        m_list.Add(new MacorItem() { Name = "DEBUG_MODEL", DisplayName = "调试模式", IsDebug = true, IsRelese = false });
        m_list.Add(new MacorItem() { Name = "DEBUG_LOG", DisplayName = "打印日志", IsDebug = true, IsRelese = false });
        // m_list.Add(new MacorItem() { Name = "STAT_TD", DisplayName = "开启统计", IsDebug = false, IsRelese = true });
        //m_list.Add(new MacorItem() { Name = "DEBUG_ROLESTATE", DisplayName = "调试角色状态", IsDebug = false, IsRelese = false });
        m_list.Add(new MacorItem() { Name = "DEBUG_ASSETBUNDLE", DisplayName = "启用AssetBundle加载资源", IsDebug = false, IsRelese = true });
        for (int i = 0; i < m_list.Count; i++)//遍历宏
        {
            if (!string.IsNullOrEmpty(m_Macor) && m_Macor.IndexOf(m_list[i].Name) != -1)//检测宏是否被选中
            {
                m_Dic[m_list[i].Name] = true;
            }
            else
            {
                m_Dic[m_list[i].Name] = false;

            }
        }
    }
    /// <summary>
    /// 绘制界面
    /// </summary>
    private void OnGUI()
    {
        for (int i = 0; i < m_Dic.Count; i++)
        {
            EditorGUILayout.BeginHorizontal("box");
            m_Dic[m_list[i].Name] = GUILayout.Toggle(m_Dic[m_list[i].Name], m_list[i].DisplayName);
            EditorGUILayout.EndHorizontal();

        }
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("保存", GUILayout.Width(100)))
        {
            SavaMacor();
        }
        if (GUILayout.Button("调试模式", GUILayout.Width(100)))
        {
            for (int i = 0; i < m_Dic.Count; i++)
            {
                m_Dic[m_list[i].Name] = m_list[i].IsDebug;

            }
            SavaMacor();
        }
        if (GUILayout.Button("发布模式", GUILayout.Width(100)))
        {
            for (int i = 0; i < m_Dic.Count; i++)
            {
                m_Dic[m_list[i].Name] = m_list[i].IsRelese;

            }
            SavaMacor();
        }
        EditorGUILayout.EndHorizontal();
    }
    private void SavaMacor()
    {
        // 1. 组装宏定义
        var sb = new StringBuilder();
        foreach (var kv in m_Dic)
        {
            if (kv.Value)
                sb.AppendFormat("{0};", kv.Key);

            // DEBUG_ASSETBUNDLE 开关 Download 下场景
            if (string.Equals(kv.Key, "DEBUG_ASSETBUNDLE", StringComparison.OrdinalIgnoreCase))
            {
                var scenes = EditorBuildSettings.scenes;
                for (int i = 0; i < scenes.Length; i++)
                {
                    if (scenes[i].path.IndexOf("Download", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        // 禁用 AssetBundle 时启用 Download 场景
                        scenes[i] = new EditorBuildSettingsScene(scenes[i].path, !kv.Value);
                    }
                }
                EditorBuildSettings.scenes = scenes;
            }
        }

        // 去掉末尾分号
        m_Macor = sb.ToString().TrimEnd(';');

        // 2. 针对各平台（使用 NamedBuildTarget）写入宏
        var targets = new[]
        {
            NamedBuildTarget.Android,
            NamedBuildTarget.iOS,
            NamedBuildTarget.Standalone
        };

        foreach (var nt in targets)
        {
            // 如果你想在原有定义上追加，可以先取一次旧值：
            // var old = PlayerSettings.GetScriptingDefineSymbols(nt);
            // var merged = string.IsNullOrEmpty(old) ? m_Macro : $"{old};{m_Macro}";
            // PlayerSettings.SetScriptingDefineSymbols(nt, merged);

            // 直接覆盖新宏：
            PlayerSettings.SetScriptingDefineSymbols(nt, m_Macor);
        }

        ConsoleDebug.Log($"[SaveMacro] 已更新脚本宏：{m_Macor}");
    }
    /// <summary>
    /// 宏项目
    /// </summary>
    public class MacorItem
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name;
        /// <summary>
        /// 显示的名称
        /// </summary>
        public string DisplayName;
        public bool IsDebug;
        /// <summary>
        /// 是否发布项
        /// </summary>
        public bool IsRelese;
    }
}