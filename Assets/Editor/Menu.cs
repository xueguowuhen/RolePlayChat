
using System.IO;
using UnityEditor;
using UnityEngine;

public class Menu
{
    [MenuItem("Tools/设置")]
    public static void Settings()
    {
        SettingWindow win = (SettingWindow)EditorWindow.GetWindow(typeof(SettingWindow));
        win.titleContent = new GUIContent("全局设置");
        win.Show();
    }
}