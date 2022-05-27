// By 宵夜97
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class XYLog
{
    private static StringBuilder logStringBuilder;
    private static Queue<string> Messages = new Queue<string>();
    private static Queue<string> Warnings = new Queue<string>();
    private static Queue<string> Errors = new Queue<string>();

    public static void Init()
    {
        logStringBuilder = new StringBuilder();
    }

    public static void Update()
    {
        while (Messages.Count > 0)
        {
            string log = Messages.Dequeue();
            Debug.Log(log);
            UIPageLiveRoom.Instance.AddLogMessage(log);
        }
        while (Warnings.Count > 0)
        {
            string log = Warnings.Dequeue();
            Debug.LogWarning(log);
            UIPageLiveRoom.Instance.AddLogWarning(log);
        }
        while (Errors.Count > 0)
        {
            string log = Errors.Dequeue();
            Debug.LogError(log);
            UIPageLiveRoom.Instance.AddLogError(log);
        }
    }

    public static void LogMessage(string log)
    {
        lock (Messages)
        {
            Messages.Enqueue(log);
        }
    }

    public static void LogWarning(string log)
    {
        lock (Warnings)
        {
            Warnings.Enqueue(log);
        }
    }

    public static void LogError(string log)
    {
        lock (Errors)
        {
            Errors.Enqueue(log);
        }
    }

    public static void LogStackTrace()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("当前调用堆栈:");
        var stacktrace = new System.Diagnostics.StackTrace();
        for (var i = 0; i < stacktrace.FrameCount; i++)
        {
            var method = stacktrace.GetFrame(i).GetMethod();
            sb.AppendLine(method.Name);
        }
        LogMessage(sb.ToString());
    }
}