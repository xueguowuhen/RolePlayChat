
using System.IO;
using UnityEditor;
using UnityEngine;

public class Menu
{
    [MenuItem("Tools/����")]
    public static void Settings()
    {
        SettingWindow win = (SettingWindow)EditorWindow.GetWindow(typeof(SettingWindow));
        win.titleContent = new GUIContent("ȫ������");
        win.Show();
    }
}