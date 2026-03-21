#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ReactiveUITK.EditorDiagnostics
{
    [InitializeOnLoad]
    public static class ReactiveLogCapture
    {
        private static bool capturing;
        private static string logFilePath;
        private static readonly object fileLock = new object();

        static ReactiveLogCapture()
        {
            EditorApplication.quitting += StopCapture;
        }

        [MenuItem("ReactiveUITK/Diagnostics/Logs/Start Capture")]
        public static void StartCaptureMenu()
        {
            StartCapture();
        }

        [MenuItem("ReactiveUITK/Diagnostics/Logs/Stop Capture")]
        public static void StopCaptureMenu()
        {
            StopCapture();
        }

        [MenuItem("ReactiveUITK/Diagnostics/Logs/Open Log Folder")]
        public static void OpenLogFolder()
        {
            string folder = EnsureFolder();
            EditorUtility.RevealInFinder(folder);
        }

        public static void StartCapture()
        {
            if (capturing)
            {
                Debug.Log("ReactiveLogCapture: already capturing → " + logFilePath);
                return;
            }
            string folder = EnsureFolder();
            string ts = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            logFilePath = Path.Combine(folder, $"reactive_log_{ts}.txt");
            TryAppend($"==== ReactiveUITK log started {DateTime.Now:O} ===={Environment.NewLine}");
            Application.logMessageReceived += OnLog;
            Application.logMessageReceivedThreaded += OnLogThreaded;
            capturing = true;
            Debug.Log("ReactiveLogCapture: started → " + logFilePath);
        }

        public static void StopCapture()
        {
            if (!capturing)
            {
                return;
            }
            Application.logMessageReceived -= OnLog;
            Application.logMessageReceivedThreaded -= OnLogThreaded;
            capturing = false;
            TryAppend($"==== ReactiveUITK log stopped {DateTime.Now:O} ===={Environment.NewLine}");
            Debug.Log("ReactiveLogCapture: stopped → " + logFilePath);
        }

        private static string EnsureFolder()
        {
            string folder = Path.Combine(
                Application.dataPath,
                "ReactiveUIToolKit",
                "Diagnostics",
                "Logs",
                "Results"
            );
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return folder;
        }

        private static void OnLog(string condition, string stackTrace, LogType type)
        {
            string line = $"{DateTime.Now:HH:mm:ss.fff} [{type}] {condition}{Environment.NewLine}";
            TryAppend(line);
            if (type == LogType.Exception || type == LogType.Error)
            {
                TryAppend(stackTrace + Environment.NewLine);
            }
        }

        private static void OnLogThreaded(string condition, string stackTrace, LogType type)
        {
            OnLog(condition, stackTrace, type);
        }

        private static void TryAppend(string text)
        {
            try
            {
                lock (fileLock)
                {
                    if (string.IsNullOrEmpty(logFilePath))
                    {
                        return;
                    }
                    File.AppendAllText(logFilePath, text);
                }
            }
            catch { }
        }
    }
}


#endif
