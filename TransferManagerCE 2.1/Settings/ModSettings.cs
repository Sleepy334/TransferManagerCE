using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

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

        private static ModSettings s_settings = null;

        public static ModSettings GetSettings()
        {
            if (s_settings == null)
            {
                s_settings = ModSettings.Load();
            }
            return s_settings;
        }

        [XmlIgnore]
        const string SETTINGS_FILE_NAME = "TransferManagerCESettings";

        [XmlIgnore]
        private static readonly string SettingsFileName = "TransferManagerCESettings.xml";

        [XmlIgnore]
        private static readonly string UserSettingsDir = ColossalFramework.IO.DataLocation.localApplicationData;

        [XmlIgnore]
        private static readonly string SettingsFile = Path.Combine(UserSettingsDir, SettingsFileName);

        public int HighlightMatchesState
        {
            get;
            set;
        } = (int) HighlightMode.Matches;

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

        public int TransferIssueDeleteResolvedDelay
        {
            get;
            set;
        } = 10;

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

        public static SavedInputKey TransferIssueHotkey = new SavedInputKey(
            "TransferManager_TransferIssue_Hotkey", SETTINGS_FILE_NAME,
            key: KeyCode.Alpha3, control: true, shift: false, alt: false, true);

        public static SavedInputKey SelectionToolHotkey = new SavedInputKey(
            "TransferManager_SelectionTool_Hotkey", SETTINGS_FILE_NAME,
            key: KeyCode.Alpha4, control: true, shift: false, alt: false, true);

        public static SavedInputKey StatsPanelHotkey = new SavedInputKey(
            "TransferManager_Stats_Hotkey", SETTINGS_FILE_NAME,
            key: KeyCode.Alpha5, control: true, shift: false, alt: false, true);
        
        public static SavedInputKey OutsideConnectionPanelHotkey = new SavedInputKey(
            "TransferManager_OutsidePanel_Hotkey", SETTINGS_FILE_NAME,
            key: KeyCode.Alpha6, control: true, shift: false, alt: false, true);

        static ModSettings()
        {
            if (GameSettings.FindSettingsFileByName(SETTINGS_FILE_NAME) == null)
            {
                GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = SETTINGS_FILE_NAME } });
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
                        if (oSettings != null)
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
                // Pretty straightforward.
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
