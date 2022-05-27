using System;
using BepInEx;
using System.IO;
using System.Text;
using UnityEngine;
using BepInEx.Logging;
using System.Collections.Generic;

namespace VTS_XYPlugin
{
    public static class XYLog
    {
        private static ManualLogSource _logSource;
        private static StringBuilder logStringBuilder;
        public static string LogFilePath;
        private static DirectoryInfo logDirectory;
        private static float writeFileCD;
        private static Queue<string> Messages = new Queue<string>();
        private static Queue<string> Warnings = new Queue<string>();
        private static Queue<string> Errors = new Queue<string>();

        public static void Init(ManualLogSource logSource)
        {
            logStringBuilder = new StringBuilder();
            logDirectory = new DirectoryInfo($"{Paths.GameRootPath}/XYPluginLogs");
            DateTime now = DateTime.Now;
            LogFilePath = $"{Paths.GameRootPath}/XYPluginLogs/日志_{now.Year}年{now.Month}月{now.Day}日{now.Hour}时{now.Minute}分_PID{System.Diagnostics.Process.GetCurrentProcess().Id}.log";
            _logSource = logSource;
            _logSource.LogEvent += LogEvent;
            Application.logMessageReceived += Application_logMessageReceived;
            Application.quitting += Application_quitting;
        }

        public static void Update()
        {
            while (Messages.Count > 0)
            {
                _logSource.LogMessage(Messages.Dequeue());
            }
            while (Warnings.Count > 0)
            {
                _logSource.LogWarning(Warnings.Dequeue());
            }
            while (Errors.Count > 0)
            {
                _logSource.LogError(Errors.Dequeue());
            }
            writeFileCD -= Time.deltaTime;
            if (writeFileCD < 0)
            {
                writeFileCD = 3f;
                WriteToFile();
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

        private static void LogEvent(object sender, LogEventArgs e)
        {
            logStringBuilder.AppendLine($"[{DateTime.Now.ToLongTimeString()}]{e}");
        }

        private static void Application_logMessageReceived(string condition, string stackTrace, LogType type)
        {
            logStringBuilder.AppendLine($"[{DateTime.Now}][{type}][Unity]{condition}");
        }

        private static void Application_quitting()
        {
            WriteToFile();
        }

        public static void WriteToFile()
        {
            try
            {
                if (!logDirectory.Exists)
                {
                    logDirectory.Create();
                }
                if (logStringBuilder != null && logStringBuilder.Length > 0)
                {
                    lock (logStringBuilder)
                    {
                        File.AppendAllText(LogFilePath, logStringBuilder.ToString());
                        logStringBuilder.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}
