using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using SleepyCommon;
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
        public enum BuildingHighlightMode
        {
            None,
            Matches,
            Issues,
        }

        public enum SettingsHighlightMode
        {
            None,
            Settings,
        }

        public enum IssuesHighlightMode
        {
            None,
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

        // Highlight modes
        public int HighlightMatchesState { get; set; } = (int) BuildingHighlightMode.Matches;
        public int HighlightSettingsState { get; set; } = (int) SettingsHighlightMode.None; // Off by default
        public int HighlightIssuesState { get; set; } = (int)IssuesHighlightMode.None;

        // We dont load these settings as they are for debugging only.
        [XmlIgnore]
        public int MatchLogReason { get; set; } = (int) TransferReason.None;


        public int MatchLogCandidates { get; set; } = 0; // TransferReason.None;
        public bool ShowBuildingId { get; set; } = false;

        public bool EnablePanelTransparency { get; set; } = false; 
        public bool AddUnifiedUIButton { get; set; } = true;

        public bool StatisticsEnabled { get; set; } = true;

        public string PreferredLanguage
        {
            get
            {
                return Localization.PreferredLanguage;
            }
            set
            {
                Localization.PreferredLanguage = value;
            }
        }

        public int DeadTimerValue { get; set; } = 64; // 64 is the value when the problem icon appears

        public int SickTimerValue { get; set; } = 32; // 32 is the point when the sick icon appears above the building.

        public int GoodsTimerValue { get; set; } = 32;


        // Transfer Issues panel
        public bool ShowDeadIssues { get; set; } = true;
        public bool ShowSickIssues { get; set; }  = true;
        public bool ShowIncomingIssues { get; set; } = true;
        public bool ShowOutgoingIssues { get; set; } = true;
        public bool ShowServiceIssues { get; set; } = true;
        public bool ShowWorkerIssues { get; set; } = true;
        public bool ShowGarbageIssues { get; set; } = true;
        public bool ShowCrimeIssues { get; set; } = true;
        public bool ShowMailIssues { get; set; } = true;
        public bool ShowFireIssues { get; set; } = true;
        public bool ShowWithVehiclesOnRouteIssues { get; set; } = true;


        // Pathing Issues Tab
        public bool ShowLocalPathingIssues { get; set; } = true;
        public bool ShowOutsidePathingIssues { get; set; } = true;


        // Default to showing materials
        public bool StatusHideVehicleReason { get; set; } = false;


        // Store panel positions
        public float BuildingPanelPosX { get; set; } = float.MaxValue;
        public float BuildingPanelPosY { get; set; } = float.MaxValue;
        public float SettingsPanelPosX { get; set; } = float.MaxValue;
        public float SettingsPanelPosY { get; set; } = float.MaxValue;


        // District panel
        public bool ShowAllDistricts { get; set; } = false;


        // =========================== Advanced Tab Settings ======================================
        public int ForceTrainSpawnAtCount { get; set; } = 240;

        public int ForceShipSpawnAtCount { get; set; } = 100;

        public int ForcePlaneSpawnAtCount { get; set; } = 200;

        public int ForceBusSpawnAtCount { get; set; } = 60;

        public bool ForceCargoShipSpawn { get; set; } = true;

        public bool ForcePassengerShipSpawn { get; set; } = true;

        public bool ForceCargoPlaneSpawn { get; set; } = true;

        public bool ForcePassengerPlaneSpawn { get; set; } = true;

        public bool ForcePassengerPlaneSpawnAtGate { get; set; } = true;

        public bool ForceCargoTrainDespawnOutsideConnections { get; set; } = true;

        public bool ForceCargoShipDespawnOutsideConnections { get; set; } = true;

        public bool ForceCargoPlaneDespawnOutsideConnections { get; set; } = true;

        public bool FixCargoTrucksDisappearingOutsideConnections { get; set; } = true;

        public bool ResetStopMaxWaitTimeWhenNoPasengers { get; set; } = true;

        public bool FixBankVansStuckCargoStations { get; set; } = true;

        public bool FixPostVansStuckCargoStations { get; set; } = true;

        public bool FixTransportStationNullReferenceException { get; set; } = true;

        public bool FixPostTruckCollectingMail { get; set; } = true;

        public bool FixFindHospital { get; set; } = true;

        public bool FixFishWarehouses { get; set; } = true;

        public bool FixCargoWarehouseAccessSegment { get; set; } = true;
        public bool FixCargoWarehouseUnspawn { get; set; } = true;
        
        public bool RemoveEmptyWarehouseLimit { get; set; } = true;

        public bool FixCargoWarehouseExcludeFlag { get; set; } = true;
        public bool FixCargoWarehouseOfferRatio { get; set; } = true;
        public bool LogCitizenPathFailures { get; set; } = false;

        // =========================== Vehicle AI =================================================
        public bool FireTruckAI { get; set; } = true;
        public bool FireTruckExtinguishTrees { get; set; } = true;
        public bool FireCopterAI { get; set; } = true;
        public bool PostVanAI { get; set; } = true;
        public bool GarbageTruckAI { get; set; } = true;
        public bool PoliceCarAI { get; set; } = true;
        public bool PoliceCopterAI { get; set; } = true;

        // =========================== Hot keys ===================================================
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

        [XmlIgnore]
        public UnsavedKeyMapping SettingsPanelHotkey = new UnsavedKeyMapping(
            "SettingsPanelHotkey",
            key: KeyCode.Alpha7, bCtrl: true, bShift: false, bAlt: false);

        [XmlIgnore]
        public UnsavedKeyMapping PathDistancePanelHotkey = new UnsavedKeyMapping(
            "PathDistancePanelHotkey",
            key: KeyCode.Alpha8, bCtrl: true, bShift: false, bAlt: false);

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

        [XmlElement("SettingsPanelHotkey")]
        public XmlInputKey XMLSettingsPanelHotkey
        {
            get => SettingsPanelHotkey.XmlKey;
            set
            {
                SettingsPanelHotkey.Key = value.Key;
                SettingsPanelHotkey.Control = value.Control;
                SettingsPanelHotkey.Shift = value.Shift;
                SettingsPanelHotkey.Alt = value.Alt;
            }
        }

        [XmlElement("PathDistancePanelHotkey")]
        public XmlInputKey XMLPathDistancePanelHotkey
        {
            get => PathDistancePanelHotkey.XmlKey;
            set
            {
                PathDistancePanelHotkey.Key = value.Key;
                PathDistancePanelHotkey.Control = value.Control;
                PathDistancePanelHotkey.Shift = value.Shift;
                PathDistancePanelHotkey.Alt = value.Alt;
            }
        }

        public static ModSettings Load()
        {
            CDebug.Log("Loading settings: " + SettingsFile); 
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
                CDebug.Log("Error loading settings:", e);
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
                    xmlSerializer.Serialize(writer, this);
                }
            }
            catch (Exception ex)
            {
                CDebug.Log("Saving settings file failed.", ex); 
            }
        }
    }
}
