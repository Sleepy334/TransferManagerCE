using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using System;
using System.IO;
using System.Xml.Serialization;
using UnifiedUI.Helpers;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE.Settings
{
    public class ModSettings
    {
        public enum HighlightMode
        {
            None,
            Matches,
            Issues,
        }
        public enum LogCandidates
        {
            All = 0,
            Valid = 1,
            Excluded = 2,
            None = 3,
        }

        private static ModSettings s_settings = null;

        public static ModSettings GetSettings()
        {
            if (s_settings is null)
            {
                s_settings = ModSettings.Load();
            }
            return s_settings;
        }

        [XmlIgnore]
        private static readonly string SettingsFileName = "TransferManagerCESettings.xml";

        [XmlIgnore]
        public static readonly string UserSettingsDir = ColossalFramework.IO.DataLocation.localApplicationData;

        [XmlIgnore]
        private static readonly string SettingsFile = Path.Combine(UserSettingsDir, SettingsFileName);

        public int HighlightMatchesState
        {
            get;
            set;
        } = (int) HighlightMode.Matches;

        public int MatchLogReason
        {
            get;
            set;
        } = (int) TransferReason.None;

        public int MatchLogCandidates
        {
            get;
            set;
        } = 0; // TransferReason.None;

        public bool ShowBuildingId
        {
            get;
            set;
        } = false;
        
        public bool EnablePanelTransparency
        {
            get;
            set;
        } = false; 

        public bool TransferIssueShowWithVehiclesOnRoute
        {
            get;
            set;
        } = false;

        public bool StatisticsEnabled
        {
            get;
            set;
        } = true;

        public string PreferredLanguage
        {
            get;
            set;
        } = "System Default";

        public int ShowConnectionGraph
        {
            get;
            set;
        } = 0;

        public int DeadTimerValue
        {
            get;
            set;
        } = 64; // 64 is the value when the problem icon appears

        public int SickTimerValue
        {
            get;
            set;
        } = 32; // 32 is the point when the sick icon appears above the building.

        public int GoodsTimerValue
        {
            get;
            set;
        } = 32;

        [XmlIgnore]
        public UnsavedKeyMapping TransferIssueHotkey = new UnsavedKeyMapping(
            "TransferIssueHotkey",
            key: KeyCode.Alpha3, bCtrl: true, bShift: false, bAlt: false);

        [XmlIgnore]
        public UnsavedKeyMapping SelectionToolHotkey = new UnsavedKeyMapping(
            "SelectionToolHotkey",
            key: KeyCode.Alpha4, bCtrl: true, bShift: false, bAlt: false);

        [XmlIgnore]
        public UnsavedKeyMapping StatsPanelHotkey = new UnsavedKeyMapping(
            "StatsPanelHotkey",
            key: KeyCode.Alpha5, bCtrl: true, bShift: false, bAlt: false);

        [XmlIgnore]
        public UnsavedKeyMapping OutsideConnectionPanelHotkey = new UnsavedKeyMapping(
            "OutsideConnectionPanelHotkey",
            key: KeyCode.Alpha6, bCtrl: true, bShift: false, bAlt: false);

        [XmlElement("SelectionToolHotkey")]
        public XmlInputKey XMLSelectionToolHotkey
        {
            get => SelectionToolHotkey.XmlKey;
            set
            {
                SelectionToolHotkey.Key = value.Key;
                SelectionToolHotkey.Control = value.Control;
                SelectionToolHotkey.Shift = value.Shift;
                SelectionToolHotkey.Alt = value.Alt;
            }
        }

        [XmlElement("TransferIssueHotkey")]
        public XmlInputKey XMLTransferIssueHotKey
        {
            get => TransferIssueHotkey.XmlKey;
            set
            {
                TransferIssueHotkey.Key = value.Key;
                TransferIssueHotkey.Control = value.Control;
                TransferIssueHotkey.Shift = value.Shift;
                TransferIssueHotkey.Alt = value.Alt;
            }
        }

        [XmlElement("StatsPanelHotkey")]
        public XmlInputKey XMLStatsPanelHotkey
        {
            get => StatsPanelHotkey.XmlKey;
            set
            {
                StatsPanelHotkey.Key = value.Key;
                StatsPanelHotkey.Control = value.Control;
                StatsPanelHotkey.Shift = value.Shift;
                StatsPanelHotkey.Alt = value.Alt;
            }
        }

        [XmlElement("OutsideConnectionPanelHotkey")]
        public XmlInputKey XMLOutsideConnectionPanelHotkey
        {
            get => OutsideConnectionPanelHotkey.XmlKey;
            set
            {
                OutsideConnectionPanelHotkey.Key = value.Key;
                OutsideConnectionPanelHotkey.Control = value.Control;
                OutsideConnectionPanelHotkey.Shift = value.Shift;
                OutsideConnectionPanelHotkey.Alt = value.Alt;
            }
        }

        public static ModSettings Load()
        {
            Debug.Log("Loading settings: " + SettingsFile); 
            try
            {
                // Read settings file.
                if (File.Exists(SettingsFile))
                {
                    using (StreamReader reader = new StreamReader(SettingsFile))
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(ModSettings));
                        ModSettings? oSettings = xmlSerializer.Deserialize(reader) as ModSettings;
                        if (oSettings is not null)
                        {
                            return oSettings;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("Error loading settings:", e);
            }

            return new ModSettings();
        }

        /// <summary>
        /// Save settings to XML file.
        /// </summary>
        public void Save()
        {
            try
            {
                // Save settings to xml file.
                using (StreamWriter writer = new StreamWriter(SettingsFile))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(ModSettings)); 
                    xmlSerializer.Serialize(writer, ModSettings.GetSettings());
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Saving settings file failed.", ex); 
            }
        }
    }
}
