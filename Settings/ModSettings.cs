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

        [XmlElement("optionEnableNewTransferManager")]
        public bool optionEnableNewTransferManager
        {
            get;
            set;
        } = true;

        [XmlElement("optionPreferLocalService")]
        public bool optionPreferLocalService
        {
            get;
            set;
        } = false;

        [XmlElement("optionWarehouseFirst")]
        public bool optionWarehouseFirst
        {
            get;
            set;
        } = false;

        [XmlElement("optionPreferExportShipPlaneTrain")]
        public bool optionPreferExportShipPlaneTrain
        {
            get;
            set;
        } = false;

        [XmlElement("optionWarehouseReserveTrucks")]
        public bool optionWarehouseReserveTrucks
        {
            get;
            set;
        } = false;

        public bool TransferIssueLocationSaved
        {
            get;
            set;
        } = false;

        public Vector3 TransferIssueLocation
        {
            get;
            set;
        }

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

        public bool TransferBuildingLocationSaved
        {
            get;
            set;
        } = false;

        public Vector3 TransferBuildingLocation
        {
            get;
            set;
        }

        public bool TransferPanelDeleteResolved
        {
            get;
            set;
        } = true;

        public bool TransferManagerExperimentalDeathcare
        {
            get;
            set;
        } = false;

        public bool StatisticsEnabled
        {
            get;
            set;
        } = false;

        public string PreferredLanguage
        {
            get;
            set;
        } = "System Default";

        public static SavedInputKey TransferIssueHotkey = new SavedInputKey(
            "TransferManager_TransferIssue_Hotkey", SETTINGS_FILE_NAME,
            key: KeyCode.Alpha3, control: true, shift: false, alt: false, true);

        public static SavedInputKey SelectionToolHotkey = new SavedInputKey(
            "TransferManager_SelectionTool_Hotkey", SETTINGS_FILE_NAME,
            key: KeyCode.Alpha4, control: true, shift: false, alt: false, true);

        public static SavedInputKey StatsPanelHotkey = new SavedInputKey(
            "TransferManager_Stats_Hotkey", SETTINGS_FILE_NAME,
            key: KeyCode.Alpha5, control: true, shift: false, alt: false, true);

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
                Debug.Log(e);
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
