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

        [XmlElement("optionPathfindChirper")]
        public bool optionPathfindChirper
        {
            get;
            set;
        } = true;

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
        } = true;

        [XmlElement("optionWarehouseFirst")]
        public bool optionWarehouseFirst
        {
            get;
            set;
        } = true;

        [XmlElement("optionPreferExportShipPlaneTrain")]
        public bool optionPreferExportShipPlaneTrain
        {
            get;
            set;
        } = true;

        [XmlElement("optionWarehouseReserveTrucks")]
        public bool optionWarehouseReserveTrucks
        {
            get;
            set;
        } = true;

        [XmlElement("optionWarehouseNewBalanced")]
        public bool optionWarehouseNewBalanced
        {
            get;
            set;
        } = true;
        

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
            Debug.Log("Saving settings: " + SettingsFile); 
            try
            {
                // Pretty straightforward.
                using (StreamWriter writer = new StreamWriter(SettingsFile))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(ModSettings)); 
                    xmlSerializer.Serialize(writer, ModSettings.GetSettings());
                }

                Debug.Log("User Setting Configuration successful saved."); 
            }
            catch (Exception ex)
            {
                Debug.Log("Saving settings file failed.", ex); 
            }
        }
    }
}
