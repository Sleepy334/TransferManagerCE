using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace TransferManagerCE.Util
{
    public static class DebugLog
    {
        private enum LogLevel
        {
            Trace,
            Debug,
            Info,
            Warning,
            Error,
        }

        private static readonly object LogLock = new object();
        private static Stopwatch _sw = Stopwatch.StartNew();

        public const LogReason REASON_PATHFIND = (LogReason)254;
        public const LogReason REASON_ALL = (LogReason)255;

        public enum LogReason : int {
            //ANALYSE1 = TransferManager.TransferReason.Garbage,
            //ANALYSE1 = TransferManager.TransferReason.Sick,
            //ANALYSE1 = TransferManager.TransferReason.Dead,
            //ANALYSE2 = TransferManager.TransferReason.Garbage,
            //ANALYSE3 = TransferManager.TransferReason.GarbageTransfer,
            //ANALYSE4 = TransferManager.TransferReason.Goods,
            PATHFIND = 254,
            ALL = 255 //256=DISABLED!
        };

        private const string LOG_FILE_NAME = "TransferManagerCE.log";
        private static string? LogFilePath;

        static DebugLog()
        {
            try
            {
                string dir = Application.dataPath;
                LogFilePath = Path.Combine(dir, LOG_FILE_NAME);
                if (File.Exists(LogFilePath))
                {
                    File.Delete(LogFilePath); // delete old file to avoid confusion.
                }

                var args = Environment.GetCommandLineArgs();
                int index = Array.IndexOf(args, "-logFile");
                if (index >= 0)
                {
                    dir = args[index + 1];
                    dir = Path.GetDirectoryName(dir); // drop output_log.txt
                    LogFilePath = Path.Combine(dir, LOG_FILE_NAME);
                    if (File.Exists(LogFilePath))
                    {
                        File.Delete(LogFilePath);
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }

        public static void LogInfo(string msg)
        {
            LogToFile(msg, LogLevel.Info);
        }

        public static void LogWarning(string msg)
        {
            LogToFile(msg, LogLevel.Warning);
        }

        public static void LogError(string msg, bool popup = false)
        {
            LogToFile($"[TransferManagerCE] {msg}", LogLevel.Error);
        }

        public static void LogOnly(string msg)
        {
            LogToFile(msg, LogLevel.Info);
        }

        public static void LogOnly(LogReason reason, string msg)
        {
            if (Enum.IsDefined(typeof(LogReason), reason))
            {
                LogToFile(msg, LogLevel.Info);
            }
        }

        private static void LogToFile(string log, LogLevel level)
        {
            lock (LogLock)
            {
                using (StreamWriter w = File.AppendText(LogFilePath))
                {
                    long secs = _sw.ElapsedTicks / Stopwatch.Frequency;
                    long fraction = _sw.ElapsedTicks % Stopwatch.Frequency;
                    w.WriteLine(
                        $"{level.ToString()} " +
                        $"{secs:n0}.{fraction:D7}: " +
                        $"{log}");

                    if (level == LogLevel.Warning || level == LogLevel.Error)
                    {
                        w.WriteLine((new System.Diagnostics.StackTrace(true)).ToString());
                        w.WriteLine();
                    }
                }
            }
        }
    }
}
