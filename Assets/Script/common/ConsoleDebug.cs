using System;
using UnityEngine;

public class ConsoleDebug
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public static void Log(object message)
    {
#if DEBUG_MODEL
        Debug.Log(message);
#elif RELEASE_MODEL
        // 在发布模式下不输出日志
#endif
    }

    public static void LogError(object message)
    {
        Debug.LogError(message);
    }
    public static void LogWarning(object message)
    {
        Debug.LogWarning(message);
    }
}
