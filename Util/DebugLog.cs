#define TRACE

using System;
using System.Diagnostics;
using System.IO;
using ColossalFramework;
using ColossalFramework.Plugins;
using HarmonyLib;
using ICities;
using static ColossalFramework.Plugins.PluginManager;

namespace TransferManagerCE.Util
{
    public static class DebugLog
    {
        public const LogReason REASON_PATHFIND = (LogReason)254;
        public const LogReason REASON_ALL = (LogReason)255;

        public enum LogReason : int { 
            //ANALYSE1 = TransferManager.TransferReason.Sick,
            //ANALYSE1 = TransferManager.TransferReason.Dead,
            //ANALYSE2 = TransferManager.TransferReason.Garbage,
            //ANALYSE3 = TransferManager.TransferReason.GarbageTransfer,
            //ANALYSE4 = TransferManager.TransferReason.Goods,
            PATHFIND = 254,
            ALL = 255 //256=DISABLED!
        };

        private const string LOG_FILE_NAME = "TransferManagerCE.log";
        private const double LOG_FLUSH_INTERVALL = 1000 * 2; //2sec

        private static TraceListener _listener = null;
        private static bool _init = false;
        private static DateTime _lastFlush;

        private static void InitLogging()
        {
            if (!_init)
            {
                try
                {
                    // truncate new log
                    FileStream fs = File.Create(LOG_FILE_NAME);
                    fs.Close();

                    _listener = new TextWriterTraceListener(LOG_FILE_NAME);
                    Trace.Listeners.Add(_listener);
                    Trace.AutoFlush = false;
                    _init = true;

                    Trace.WriteLine(DateTime.Now);
                }
                catch (Exception ex)
                {
                    _init = false;
                }
                
            }
        }

        public static void StopLogging()
        {
            if (_init)
            {
                Trace.Flush();
                Trace.Listeners.Remove(_listener);
            }
        }

        public static void LogError(string msg, bool popup = false)
        {
            LogInfo($"[TransferManagerCE] ERROR: {msg}");
            UnityEngine.Debug.LogError($"[TransferManagerCE] ERROR: {msg}");
            if (popup)
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Error, $"[TransferManagerCE] ERROR: {msg}");
        }

        public static void LogInfo(string msg)
        {
            if (!_init)
            {
                InitLogging();
            }
#if DEBUG
            UnityEngine.Debug.Log($"[TransferManagerCE] {msg}");
#endif
            Trace.WriteLine(msg);
            if ((DateTime.Now - _lastFlush).TotalMilliseconds > LOG_FLUSH_INTERVALL)
            {
                _lastFlush = DateTime.Now;
                Trace.Flush();
            }
        }

        public static void FlushImmediate()
        {
            Trace.Flush();
        }

        public static void LogOnly(string msg)
        {
            if (!_init)
            {
                InitLogging();
            }
            Trace.WriteLine(msg);
            if ((DateTime.Now - _lastFlush).TotalMilliseconds > LOG_FLUSH_INTERVALL)
            {
                _lastFlush = DateTime.Now;
                Trace.Flush();
            }
        }

        public static void LogOnly(LogReason reason, string msg)
        {
            if (Enum.IsDefined(typeof(LogReason), reason))
            {
                if (!_init)
                {
                    InitLogging();
                }
                LogOnly(msg);
            }
        }

        public static void ReportAllHarmonyPatches()
        {
            DebugLog.LogInfo($"-- HARMONY PATCH REPORT --");
            var harmony = new Harmony(Patcher.HarmonyId);
            var methods = harmony.GetPatchedMethods();
            foreach (var method in methods)
            {
                var info = Harmony.GetPatchInfo(method);

                DebugLog.LogInfo($"- Harmony patched method = {method.FullDescription()} - #patchers: {info.Owners.Count} - Prefixes:{info.Prefixes.Count}, Postfixes:{info.Postfixes.Count}");
                foreach (var owner in info.Owners)
                {
                    DebugLog.LogInfo($"   ->Patched by: {owner.ToString()}");
                }
            }
        }

        /* Below code adapted from TMPE under MIT license */
        /* original copyright The TMPE team */

        public static void ReportAllMods()
        {
            DebugLog.LogInfo($"-- INSTALLED MOD REPORT --");
            foreach (PluginInfo mod in Singleton<PluginManager>.instance.GetPluginsInfo())
            {
                if (!mod.isCameraScript)
                {
                    string strModName = GetModName(mod);
                    ulong workshopID = mod.publishedFileID.AsUInt64;
                    bool isLocal = workshopID == ulong.MaxValue;

                    DebugLog.LogInfo($"Installed Mod: {strModName}, Id: {workshopID}, local={isLocal}, enabled={mod.isEnabled}, assemblies: {mod.assembliesString}");
                }
            }
        }


        /// <summary>
        /// Gets the name of the specified mod.
        /// It will return the <see cref="IUserMod.Name"/> if found, otherwise it will return
        /// <see cref="PluginInfo.name"/> (assembly name).
        /// </summary>
        /// <param name="plugin">The <see cref="PluginInfo"/> associated with the mod.</param>
        /// <returns>The name of the specified plugin.</returns>
        private static string GetModName(PluginInfo plugin)
        {
            try
            {
                if (plugin == null)
                {
                    return "(PluginInfo is null)";
                }

                if (plugin.userModInstance == null)
                {
                    return string.IsNullOrEmpty(plugin.name)
                        ? "(userModInstance and name are null)"
                        : $"({plugin.name})";
                }

                return ((IUserMod)plugin.userModInstance).Name;
            }
            catch
            {
                return $"(error retreiving Name)";
            }
        }

        /// <summary>
        /// Gets the <see cref="Guid"/> of a mod.
        /// </summary>
        /// <param name="plugin">The <see cref="PluginInfo"/> associated with the mod.</param>
        /// <returns>The <see cref="Guid"/> of the mod.</returns>
        private static Guid GetModGuid(PluginInfo plugin)
        {
            return plugin.userModInstance.GetType().Assembly.ManifestModule.ModuleVersionId;
        }

    }
}
