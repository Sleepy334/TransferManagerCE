using System;
using System.Collections.Generic;
using TransferManagerCE.CustomManager;
using TransferManagerCE.Util;
using static TransferManager;

namespace TransferManagerCE
{
    public class SaveGameSettings
    {
        public enum PathDistanceAlgorithm 
        {
            LineOfSight = 0,
            ConnectedLineOfSight = 1,
            PathDistance = 2
        }
        const int iSAVE_GAME_SETTINGS_DATA_VERSION = 24;
        public static SaveGameSettings s_SaveGameSettings = new SaveGameSettings();

        // Settings
        public bool EnableNewTransferManager = true;

        // General tab
        public int PathDistanceServices = (int) PathDistanceAlgorithm.LineOfSight;
        public int PathDistanceGoods = (int)PathDistanceAlgorithm.LineOfSight;
        public int PathDistanceHeuristic = 80; // 0 = Accurate, 100 = Fastest
        public int PathDistanceTravelTimeBaseValue = 3000;
        public bool EnablePathFailExclusion = true;
        public CustomTransferManager.BalancedMatchModeOption BalancedMatchMode = CustomTransferManager.BalancedMatchModeOption.MatchModeIncomingFirst; // Vanilla
        public bool DisableDummyTraffic = false;

        // Warehouse options
        public bool FactoryFirst = true; // ON by default
        public bool ImprovedWarehouseMatching = false;
        public bool NewInterWarehouseTransfer = false;
        public bool WarehouseFirst = false;
        public int WarehouseReserveTrucksPercent = 20; // [0..100]

        // Outside connections
        public int OutsideShipMultiplier = 1;
        public int OutsidePlaneMultiplier = 1;
        public int OutsideTrainMultiplier = 1;
        public int OutsideRoadMultiplier = 1;
        public int ExportVehicleLimit = 100; // OFF by default

        // Services
        public bool PreferLocalService = false;
        public bool ImprovedDeathcareMatching = true;
        public bool ImprovedGarbageMatching = true;
        public bool ImprovedCrimeMatching = true;
        public bool ImprovedMailTransfers = true;

        // TaxiMove
        public bool TaxiMove = true;
        public int TaxiStandDelay = 5;

        // Sick Collection
        public bool OverrideResidentialSickHandler = true;
        public bool CollectSickFromOtherBuildings = false;

        // Generic Industries
        public bool OverrideGenericIndustriesHandler = true;

        // VehicleAI
        public bool FireTruckAI = true;
        public bool FireCopterAI = true;
        public bool GarbageTruckAI = false;
        public bool PoliceCarAI = false;
        public bool PoliceCopterAI = false;
        
        // Arrays
        private Dictionary<CustomTransferReason.Reason, int> m_ActiveDistanceRestrictions = new Dictionary<CustomTransferReason.Reason, int>();
        private HashSet<CustomTransferReason.Reason> m_ImportRestricted = new HashSet<CustomTransferReason.Reason>();
        private HashSet<CustomTransferReason.Reason> m_WarehouseImportRestricted = new HashSet<CustomTransferReason.Reason>();
        
        public SaveGameSettings()
        {
        }

        public static SaveGameSettings GetSettings()
        {
            return s_SaveGameSettings;
        }

        public static void SetSettings(SaveGameSettings settings)
        {
            s_SaveGameSettings = settings;
        }

        public float GetActiveDistanceRestrictionKm(CustomTransferReason.Reason material)
        {
            return (float)Math.Sqrt(GetActiveDistanceRestrictionSquaredMeters(material)) * 0.001f;
        }

        public void SetActiveDistanceRestrictionKm(CustomTransferReason.Reason material, float fValueKm)
        {
            if (fValueKm == 0f)
            {
                if (m_ActiveDistanceRestrictions.ContainsKey(material))
                {
                    m_ActiveDistanceRestrictions.Remove(material);
                }
            }
            else
            {
                m_ActiveDistanceRestrictions[material] = Square(fValueKm * 1000.0f); // Saved as meters squared so we save on Sqrt calls.
            }
        }

        public float GetActiveDistanceRestrictionSquaredMeters(CustomTransferReason.Reason material)
        {
            if (m_ActiveDistanceRestrictions.ContainsKey(material))
            {
                return m_ActiveDistanceRestrictions[material];
            }
            return 0f;
        }

        public bool IsImportRestricted(CustomTransferReason.Reason material)
        {
            return m_ImportRestricted.Contains(material);
        }

        public void SetImportRestriction(CustomTransferReason.Reason material, bool bRestricted)
        {
            if (bRestricted)
            {
                m_ImportRestricted.Add(material);
            }
            else if (m_ImportRestricted.Contains(material))
            {
                m_ImportRestricted.Remove(material);
            }
        }
        public bool IsWarehouseImportRestricted(CustomTransferReason.Reason material)
        {
            return m_WarehouseImportRestricted.Contains(material);
        }

        public void SetWarehouseImportRestriction(CustomTransferReason.Reason material, bool bRestricted)
        {
            if (bRestricted)
            {
                m_WarehouseImportRestricted.Add(material);
            }
            else if (m_WarehouseImportRestricted.Contains(material))
            {
                m_WarehouseImportRestricted.Remove(material);
            }
        }

        public static void SaveData(FastList<byte> Data)
        {
            s_SaveGameSettings.SaveDataInternal(Data);
        }

        private void SaveDataInternal(FastList<byte> Data)
        {
            StorageData.WriteInt32(iSAVE_GAME_SETTINGS_DATA_VERSION, Data); 
            
            StorageData.WriteBool(EnableNewTransferManager, Data);

            // General
            StorageData.WriteInt32((int)BalancedMatchMode, Data);
            StorageData.WriteBool(false, Data); // Old Path Distance Service setting no longer used
            StorageData.WriteBool(false, Data); // Old Path Distance Goods setting no longer used
            StorageData.WriteBool(DisableDummyTraffic, Data); // New in version 5

            // Goods delivery
            StorageData.WriteBool(FactoryFirst, Data); // Version 13
            StorageData.WriteBool(WarehouseFirst, Data);
            StorageData.WriteInt32(WarehouseReserveTrucksPercent, Data); // New in version 6
            StorageData.WriteBool(ImprovedWarehouseMatching, Data); // Version 16

            // Import/Export
            StorageData.WriteInt32(OutsideShipMultiplier, Data);
            StorageData.WriteInt32(OutsidePlaneMultiplier, Data);
            StorageData.WriteInt32(OutsideTrainMultiplier, Data);
            StorageData.WriteInt32(OutsideRoadMultiplier, Data);

            // Services
            StorageData.WriteBool(PreferLocalService, Data);
            StorageData.WriteBool(ImprovedCrimeMatching, Data);
            StorageData.WriteBool(ImprovedDeathcareMatching, Data);
            StorageData.WriteBool(ImprovedGarbageMatching, Data);
            StorageData.WriteBool(OverrideResidentialSickHandler, Data); // Version 15
            StorageData.WriteBool(CollectSickFromOtherBuildings, Data); // Version 15

            // Vehicle AI
            StorageData.WriteBool(FireTruckAI, Data);
            StorageData.WriteBool(FireCopterAI, Data);
            StorageData.WriteBool(GarbageTruckAI, Data);
            StorageData.WriteBool(PoliceCarAI, Data);
            StorageData.WriteBool(PoliceCopterAI, Data);

            // Distance restrictions
            StorageData.WriteInt32(m_ActiveDistanceRestrictions.Count, Data);
            foreach (KeyValuePair<CustomTransferReason.Reason, int> kvp in m_ActiveDistanceRestrictions)
            {
                StorageData.WriteInt32((int)kvp.Key, Data);
                StorageData.WriteInt32(kvp.Value, Data);
            }

            // Import restrictions
            StorageData.WriteInt32(m_ImportRestricted.Count, Data);
            foreach (CustomTransferReason.Reason material in m_ImportRestricted)
            {
                StorageData.WriteInt32((int)material, Data);
            }

            // Warehouse Import restrictions
            StorageData.WriteInt32(m_WarehouseImportRestricted.Count, Data);
            foreach (CustomTransferReason.Reason material in m_WarehouseImportRestricted)
            {
                StorageData.WriteInt32((int)material, Data);
            }

            StorageData.WriteInt32(ExportVehicleLimit, Data); // New in 17
            StorageData.WriteBool(NewInterWarehouseTransfer, Data); // Re-introduced in 18
            StorageData.WriteBool(OverrideGenericIndustriesHandler, Data); // Introduced in 19
            StorageData.WriteBool(EnablePathFailExclusion, Data); // Introduced in 20
            StorageData.WriteBool(ImprovedMailTransfers, Data); // Introduced in 21
            StorageData.WriteInt32(PathDistanceHeuristic, Data); // Version 22
            StorageData.WriteInt32(PathDistanceServices, Data); // Version 23
            StorageData.WriteInt32(PathDistanceGoods, Data); // Version 23
            StorageData.WriteInt32(PathDistanceTravelTimeBaseValue, Data); // 23

            StorageData.WriteBool(TaxiMove, Data); // Settings version 24
            StorageData.WriteInt32(TaxiStandDelay, Data); // Settings version 24
        }

        public static void LoadData(int iGlobalVersion, byte[] Data, ref int iIndex)
        {
            // Clear previous settings if any
            SetSettings(new SaveGameSettings());

            // Load settings from file
            if (iGlobalVersion >= 6 && Data is not null && Data.Length > iIndex)
            {
                int iSaveGameSettingVersion = StorageData.ReadInt32(Data, ref iIndex);
#if DEBUG
                Debug.Log("Global: " + iGlobalVersion + " SaveGameVersion: " + iSaveGameSettingVersion + " DataLength: " + Data.Length + " Index: " + iIndex);
#endif
                if (s_SaveGameSettings is not null)
                {
                    switch (iSaveGameSettingVersion)
                    {
                        case 1: s_SaveGameSettings.LoadDataVersion1(Data, ref iIndex); break;
                        case 2: s_SaveGameSettings.LoadDataVersion2(Data, ref iIndex); break;
                        case 3: s_SaveGameSettings.LoadDataVersion3(Data, ref iIndex); break;
                        case 4: s_SaveGameSettings.LoadDataVersion4(Data, ref iIndex); break;
                        case 5: s_SaveGameSettings.LoadDataVersion5(Data, ref iIndex); break;
                        case 6: s_SaveGameSettings.LoadDataVersion6(Data, ref iIndex); break;
                        case 7: s_SaveGameSettings.LoadDataVersion7(Data, ref iIndex); break;
                        case 8: s_SaveGameSettings.LoadDataVersion8(Data, ref iIndex); break;
                        case 9: s_SaveGameSettings.LoadDataVersion9(Data, ref iIndex); break;
                        case 10: s_SaveGameSettings.LoadDataVersion10(Data, ref iIndex); break;
                        case 11: s_SaveGameSettings.LoadDataVersion11(Data, ref iIndex); break;
                        case 12: s_SaveGameSettings.LoadDataVersion12(Data, ref iIndex); break;
                        case 13: s_SaveGameSettings.LoadDataVersion13(Data, ref iIndex); break;
                        case 14: s_SaveGameSettings.LoadDataVersion14(Data, ref iIndex); break;
                        case 15: s_SaveGameSettings.LoadDataVersion15(Data, ref iIndex); break;
                        case 16: s_SaveGameSettings.LoadDataVersion16(Data, ref iIndex); break;
                        case 17: s_SaveGameSettings.LoadDataVersion17(Data, ref iIndex); break;
                        case 18: s_SaveGameSettings.LoadDataVersion18(Data, ref iIndex); break;
                        case 19: s_SaveGameSettings.LoadDataVersion19(Data, ref iIndex); break;
                        case 20: s_SaveGameSettings.LoadDataVersion20(Data, ref iIndex); break;
                        case 21: s_SaveGameSettings.LoadDataVersion21(Data, ref iIndex); break;
                        case 22: s_SaveGameSettings.LoadDataVersion22(Data, ref iIndex); break;
                        case 23: s_SaveGameSettings.LoadDataVersion23(Data, ref iIndex); break;
                        case 24: s_SaveGameSettings.LoadDataVersion24(Data, ref iIndex); break;
                        default:
                            {
                                Debug.Log("New data version, unable to load!");
                                break;
                            }
                    }
#if DEBUG
                    Debug.Log("Settings:\r\n" + s_SaveGameSettings.DebugSettings());
#endif
                }
            }
        }

        private void LoadDataVersion24(byte[] Data, ref int iIndex)
        {
            // Load all previous versions settings
            LoadDataVersion23(Data, ref iIndex);

            // Added in version 24
            TaxiMove = StorageData.ReadBool(Data, ref iIndex);
            TaxiStandDelay = StorageData.ReadInt32(Data, ref iIndex);
        }

        private void LoadDataVersion23(byte[] Data, ref int iIndex)
        {
            // Load all previous versions settings
            LoadDataVersion22(Data, ref iIndex);

            // Added in version 23
            PathDistanceTravelTimeBaseValue = StorageData.ReadInt32(Data, ref iIndex);
        }
        private void LoadDataVersion22(byte[] Data, ref int iIndex)
        {
            // Load all previous versions settings
            LoadDataVersion21(Data, ref iIndex);

            // Added in version 22
            PathDistanceHeuristic = StorageData.ReadInt32(Data, ref iIndex);
            PathDistanceServices = StorageData.ReadInt32(Data, ref iIndex);
            PathDistanceGoods = StorageData.ReadInt32(Data, ref iIndex);
        }
        private void LoadDataVersion21(byte[] Data, ref int iIndex)
        {
            // Load all previous versions settings
            LoadDataVersion20(Data, ref iIndex);

            // Added in version 21
            ImprovedMailTransfers = StorageData.ReadBool(Data, ref iIndex);
        }
        private void LoadDataVersion20(byte[] Data, ref int iIndex)
        {
            // Load all previous versions settings
            LoadDataVersion19(Data, ref iIndex);

            // Added in version 20
            EnablePathFailExclusion = StorageData.ReadBool(Data, ref iIndex);
        }
        private void LoadDataVersion19(byte[] Data, ref int iIndex)
        {
            // Load all previous versions settings
            LoadDataVersion18(Data, ref iIndex);

            // Override industrial handler added in 19
            OverrideGenericIndustriesHandler = StorageData.ReadBool(Data, ref iIndex);
        }
        private void LoadDataVersion18(byte[] Data, ref int iIndex)
        {
            // Load all previous versions settings
            LoadDataVersion17(Data, ref iIndex);

            // Load the Inter-warehouse setting, shouldn't have removed it :-(
            NewInterWarehouseTransfer = StorageData.ReadBool(Data, ref iIndex); // Re-introduced in 18
        }
        private void LoadDataVersion17(byte[] Data, ref int iIndex)
        {
            // Load all previous versions settings
            LoadDataVersion16(Data, ref iIndex);

            // New in version 17
            ExportVehicleLimit = StorageData.ReadInt32(Data, ref iIndex); 
        }
        private void LoadDataVersion16(byte[] Data, ref int iIndex)
        {
            EnableNewTransferManager = StorageData.ReadBool(Data, ref iIndex);

            // General
            BalancedMatchMode = (CustomTransferManager.BalancedMatchModeOption)StorageData.ReadInt32(Data, ref iIndex);
            bool bUsePathDistanceServices = StorageData.ReadBool(Data, ref iIndex);
            if (bUsePathDistanceServices)
            {
                PathDistanceServices = (int)PathDistanceAlgorithm.PathDistance;
            }
            bool bUsePathDistanceGoods = StorageData.ReadBool(Data, ref iIndex);
            if (bUsePathDistanceGoods)
            {
                PathDistanceGoods = (int)PathDistanceAlgorithm.PathDistance;
            }
            DisableDummyTraffic = StorageData.ReadBool(Data, ref iIndex);

            // Goods delivery
            FactoryFirst = StorageData.ReadBool(Data, ref iIndex);
            WarehouseFirst = StorageData.ReadBool(Data, ref iIndex);
            WarehouseReserveTrucksPercent = StorageData.ReadInt32(Data, ref iIndex);
            ImprovedWarehouseMatching = StorageData.ReadBool(Data, ref iIndex);

            // Import/Export
            OutsideShipMultiplier = StorageData.ReadInt32(Data, ref iIndex);
            OutsidePlaneMultiplier = StorageData.ReadInt32(Data, ref iIndex);
            OutsideTrainMultiplier = StorageData.ReadInt32(Data, ref iIndex);
            OutsideRoadMultiplier = StorageData.ReadInt32(Data, ref iIndex);

            // Services
            PreferLocalService = StorageData.ReadBool(Data, ref iIndex);
            ImprovedCrimeMatching = StorageData.ReadBool(Data, ref iIndex);
            ImprovedDeathcareMatching = StorageData.ReadBool(Data, ref iIndex);
            ImprovedGarbageMatching = StorageData.ReadBool(Data, ref iIndex);
            OverrideResidentialSickHandler = StorageData.ReadBool(Data, ref iIndex);
            CollectSickFromOtherBuildings = StorageData.ReadBool(Data, ref iIndex);

            // VehicleAI
            FireTruckAI = StorageData.ReadBool(Data, ref iIndex);
            FireCopterAI = StorageData.ReadBool(Data, ref iIndex);
            GarbageTruckAI = StorageData.ReadBool(Data, ref iIndex);
            PoliceCarAI = StorageData.ReadBool(Data, ref iIndex);
            PoliceCopterAI = StorageData.ReadBool(Data, ref iIndex);

            // Arrays
            LoadDistanceRestrictions(Data, ref iIndex);
            LoadImportRestrictions(Data, ref iIndex);
            LoadWarehouseImportRestrictions(Data, ref iIndex);
        }
        private void LoadDataVersion15(byte[] Data, ref int iIndex)
        {
            // Specifically skip LoadDataVersion14 as the settings were changed in 15
            LoadDataVersion13(Data, ref iIndex);

            // Load the new Sick options, skipping the version 14 options.
            OverrideResidentialSickHandler = StorageData.ReadBool(Data, ref iIndex); // New in version 15
            CollectSickFromOtherBuildings = StorageData.ReadBool(Data, ref iIndex); // New in version 15
        }
        private void LoadDataVersion14(byte[] Data, ref int iIndex)
        {
            LoadDataVersion13(Data, ref iIndex);
            bool bCollectSickFromOtherBuildings = StorageData.ReadBool(Data, ref iIndex); // New in version 14
            bool bSchoolSick = StorageData.ReadBool(Data, ref iIndex); // New in version 14
            bool bParkSick = StorageData.ReadBool(Data, ref iIndex); // New in version 14
        }
        private void LoadDataVersion13(byte[] Data, ref int iIndex)
        {
            LoadDataVersion12(Data, ref iIndex);
            FactoryFirst = StorageData.ReadBool(Data, ref iIndex); // New in version 13
        }

        private void LoadDataVersion12(byte[] Data, ref int iIndex)
        {
            LoadDataVersion11(Data, ref iIndex);
            bool bUsePathDistanceGoods = StorageData.ReadBool(Data, ref iIndex); // New in version 12
            if (bUsePathDistanceGoods)
            {
                PathDistanceGoods = (int)PathDistanceAlgorithm.PathDistance;
            }
        }

        private void LoadDataVersion11(byte[] Data, ref int iIndex)
        {
            LoadDataVersion10(Data, ref iIndex);

            // Upgrade UsePathDistanceServices -> PathDistanceServices enum.
            bool bUsePathDistanceServices = StorageData.ReadBool(Data, ref iIndex); // New in version 11
            if (bUsePathDistanceServices)
            {
                PathDistanceServices = (int)PathDistanceAlgorithm.PathDistance;
            }
        }

        private void LoadDataVersion10(byte[] Data, ref int iIndex)
        {
            LoadDataVersion9(Data, ref iIndex);
            BalancedMatchMode = (CustomTransferManager.BalancedMatchModeOption) StorageData.ReadInt32(Data, ref iIndex); // New in version 10
        }

        private void LoadDataVersion9(byte[] Data, ref int iIndex)
        {
            LoadDataVersion8(Data, ref iIndex);
            ImprovedCrimeMatching = StorageData.ReadBool(Data, ref iIndex); // New in version 9
        }

        private void LoadDataVersion8(byte[] Data, ref int iIndex)
        {
            LoadDataVersion7(Data, ref iIndex);
            NewInterWarehouseTransfer = StorageData.ReadBool(Data, ref iIndex); // No longer used
        }

        private void LoadDataVersion7(byte[] Data, ref int iIndex)
        {
            EnableNewTransferManager = StorageData.ReadBool(Data, ref iIndex);
            PreferLocalService = StorageData.ReadBool(Data, ref iIndex);
            OutsideShipMultiplier = StorageData.ReadInt32(Data, ref iIndex);
            OutsidePlaneMultiplier = StorageData.ReadInt32(Data, ref iIndex);
            OutsideTrainMultiplier = StorageData.ReadInt32(Data, ref iIndex);
            OutsideRoadMultiplier = StorageData.ReadInt32(Data, ref iIndex);
            WarehouseFirst = StorageData.ReadBool(Data, ref iIndex);
            int iWarehouseReserve = StorageData.ReadInt32(Data, ref iIndex); // Used to be Reserve vehicle limit
            ImprovedDeathcareMatching = StorageData.ReadBool(Data, ref iIndex);
            ImprovedGarbageMatching = StorageData.ReadBool(Data, ref iIndex);
            DisableDummyTraffic = StorageData.ReadBool(Data, ref iIndex); // New in version 5

            FireTruckAI = StorageData.ReadBool(Data, ref iIndex);
            FireCopterAI = StorageData.ReadBool(Data, ref iIndex);
            GarbageTruckAI = StorageData.ReadBool(Data, ref iIndex);
            PoliceCarAI = StorageData.ReadBool(Data, ref iIndex);
            PoliceCopterAI = StorageData.ReadBool(Data, ref iIndex);

            LoadDistanceRestrictions(Data, ref iIndex);
            LoadImportRestrictions(Data, ref iIndex);
            LoadWarehouseImportRestrictions(Data, ref iIndex);
        }

        private void LoadDataVersion6(byte[] Data, ref int iIndex)
        {
            EnableNewTransferManager = StorageData.ReadBool(Data, ref iIndex);
            PreferLocalService = StorageData.ReadBool(Data, ref iIndex);
            bool PreferExportShip = StorageData.ReadBool(Data, ref iIndex);
            bool PreferExportPlane = StorageData.ReadBool(Data, ref iIndex);
            bool PreferExportTrain = StorageData.ReadBool(Data, ref iIndex);
            WarehouseFirst = StorageData.ReadBool(Data, ref iIndex);
            int iWarehouseReserve = StorageData.ReadInt32(Data, ref iIndex); // Used to be Reserve vehicle limit
            ImprovedDeathcareMatching = StorageData.ReadBool(Data, ref iIndex);
            ImprovedGarbageMatching = StorageData.ReadBool(Data, ref iIndex);
            DisableDummyTraffic = StorageData.ReadBool(Data, ref iIndex); // New in version 5

            FireTruckAI = StorageData.ReadBool(Data, ref iIndex);
            FireCopterAI = StorageData.ReadBool(Data, ref iIndex);
            GarbageTruckAI = StorageData.ReadBool(Data, ref iIndex);
            PoliceCarAI = StorageData.ReadBool(Data, ref iIndex);
            PoliceCopterAI = StorageData.ReadBool(Data, ref iIndex);

            LoadDistanceRestrictions(Data, ref iIndex);
            LoadImportRestrictions(Data, ref iIndex);
            LoadWarehouseImportRestrictions(Data, ref iIndex);
        }

        private void LoadDataVersion5(byte[] Data, ref int iIndex)
        {
            LoadDataVersion4(Data, ref iIndex);

            // New in version 5
            DisableDummyTraffic = StorageData.ReadBool(Data, ref iIndex);
        }

        private void LoadDataVersion4(byte[] Data, ref int iIndex)
        {
            LoadDataVersion3(Data, ref iIndex);

            // New in version 4
            float fWarehouseReserveTrucksPercent = StorageData.ReadFloat(Data, ref iIndex); // No longer used
        }

        private void LoadDataVersion3(byte[] Data, ref int iIndex)
        {
            EnableNewTransferManager = StorageData.ReadBool(Data, ref iIndex);
            PreferLocalService = StorageData.ReadBool(Data, ref iIndex);
            bool PreferExportShip = StorageData.ReadBool(Data, ref iIndex);
            bool PreferExportPlane = StorageData.ReadBool(Data, ref iIndex);
            bool PreferExportTrain = StorageData.ReadBool(Data, ref iIndex);
            WarehouseFirst = StorageData.ReadBool(Data, ref iIndex);
            bool bWarehouseReserve = StorageData.ReadBool(Data, ref iIndex); // No longer used
            ImprovedDeathcareMatching = StorageData.ReadBool(Data, ref iIndex);
            ImprovedGarbageMatching = StorageData.ReadBool(Data, ref iIndex);
            FireTruckAI = StorageData.ReadBool(Data, ref iIndex);
            FireCopterAI = StorageData.ReadBool(Data, ref iIndex);
            GarbageTruckAI = StorageData.ReadBool(Data, ref iIndex);
            PoliceCarAI = StorageData.ReadBool(Data, ref iIndex);
            PoliceCopterAI = StorageData.ReadBool(Data, ref iIndex);

            LoadDistanceRestrictions(Data, ref iIndex);
            LoadImportRestrictions(Data, ref iIndex);
            LoadWarehouseImportRestrictions(Data, ref iIndex);
        }

        private void LoadDataVersion2(byte[] Data, ref int iIndex)
        {
            EnableNewTransferManager = StorageData.ReadBool(Data, ref iIndex);
            PreferLocalService = StorageData.ReadBool(Data, ref iIndex);
            bool PreferExportShip = StorageData.ReadBool(Data, ref iIndex);
            bool PreferExportPlane = StorageData.ReadBool(Data, ref iIndex);
            bool PreferExportTrain = StorageData.ReadBool(Data, ref iIndex);
            WarehouseFirst = StorageData.ReadBool(Data, ref iIndex);
            bool bWarehouseReserve = StorageData.ReadBool(Data, ref iIndex); // No longer used
            ImprovedDeathcareMatching = StorageData.ReadBool(Data, ref iIndex);
            ImprovedGarbageMatching = StorageData.ReadBool(Data, ref iIndex);

            LoadDistanceRestrictions(Data, ref iIndex);
        }

        private void LoadDataVersion1(byte[] Data, ref int iIndex)
        {
            EnableNewTransferManager = StorageData.ReadBool(Data, ref iIndex);
            PreferLocalService = StorageData.ReadBool(Data, ref iIndex);
            bool PreferExportShip = StorageData.ReadBool(Data, ref iIndex);
            bool PreferExportPlane = StorageData.ReadBool(Data, ref iIndex);
            bool PreferExportTrain = StorageData.ReadBool(Data, ref iIndex);
            WarehouseFirst = StorageData.ReadBool(Data, ref iIndex);
            bool bWarehouseReserve = StorageData.ReadBool(Data, ref iIndex); // No longer used
            ImprovedDeathcareMatching = StorageData.ReadBool(Data, ref iIndex);

            LoadDistanceRestrictions(Data, ref iIndex);
        }

        private void LoadDistanceRestrictions(byte[] Data, ref int iIndex)
        {
            if (iIndex < Data.Length)
            {
                int iCount = StorageData.ReadInt32(Data, ref iIndex);
                for (int i = 0; i < iCount; ++i)
                {
                    int key = StorageData.ReadInt32(Data, ref iIndex);
                    int iValue = StorageData.ReadInt32(Data, ref iIndex);
                    m_ActiveDistanceRestrictions[(CustomTransferReason.Reason)key] = iValue;
                }
            }
        }

        private void LoadImportRestrictions(byte[] Data, ref int iIndex)
        {
            if (iIndex < Data.Length)
            {
                int iImportRestrictionCount = StorageData.ReadInt32(Data, ref iIndex);
                for (int i = 0; i < iImportRestrictionCount; ++i)
                {
                    CustomTransferReason.Reason material = (CustomTransferReason.Reason)StorageData.ReadInt32(Data, ref iIndex);
                    if (!m_ImportRestricted.Contains(material) && TransferManagerModes.IsImportRestrictionsSupported(material))
                    {
                        m_ImportRestricted.Add(material);
                    }
                }
            }
        }

        private void LoadWarehouseImportRestrictions(byte[] Data, ref int iIndex)
        {
            if (iIndex < Data.Length)
            {
                int iWarehouseImportRestricted = StorageData.ReadInt32(Data, ref iIndex);
                for (int i = 0; i < iWarehouseImportRestricted; ++i)
                {
                    CustomTransferReason.Reason material = (CustomTransferReason.Reason)StorageData.ReadInt32(Data, ref iIndex);
                    if (!m_WarehouseImportRestricted.Contains(material) && TransferManagerModes.IsImportRestrictionsSupported(material))
                    {
                        m_WarehouseImportRestricted.Add(material);
                    }
                }
            }
        }

        public string DebugSettings()
        {
            string sMessage = "===== Save Game Settings =====\r\n";
            sMessage += "EnableNewTransferManager: " + EnableNewTransferManager + "\r\n";

            // Warehouse
            sMessage += "WarehouseFirst: " + WarehouseFirst + "\r\n";
            sMessage += "WarehouseReserveTrucks: " + WarehouseReserveTrucksPercent + "\r\n";
            
            // Import / Export
            sMessage += "ShipMultiplier: " + OutsideShipMultiplier + "\r\n";
            sMessage += "PlaneMultiplier: " + OutsidePlaneMultiplier + "\r\n";
            sMessage += "TrainMultiplier: " + OutsideTrainMultiplier + "\r\n";
            sMessage += "RoadMultiplier: " + OutsideRoadMultiplier + "\r\n";
            sMessage += "ExportVehicleLimit: " + ExportVehicleLimit + "\r\n";

            // Services
            sMessage += "PreferLocalService: " + PreferLocalService + "\r\n";
            sMessage += "ImprovedDeathcareMatching: " + ImprovedDeathcareMatching + "\r\n";
            sMessage += "ImprovedGarbageMatching: " + ImprovedGarbageMatching + "\r\n";
            sMessage += "ImprovedCrimeMatching: " + ImprovedCrimeMatching + "\r\n";

            // VehicleAI
            sMessage += "FireTruckAI: " + FireTruckAI + "\r\n";
            sMessage += "FireCopterAI: " + FireCopterAI + "\r\n";
            sMessage += "GarbageTruckAI: " + GarbageTruckAI + "\r\n";
            sMessage += "PoliceCarAI: " + PoliceCarAI + "\r\n";
            sMessage += "PoliceCopterAI: " + PoliceCopterAI + "\r\n";

            if (m_ActiveDistanceRestrictions is not null)
            {
                sMessage += "\r\nDistanceRestrictionCount: " + m_ActiveDistanceRestrictions.Count;
                foreach (KeyValuePair<CustomTransferReason.Reason, int> kvp in m_ActiveDistanceRestrictions)
                {
                    sMessage += "\r\nKey: " + kvp.Key + " Value: " + kvp.Value + " (" + Math.Sqrt(kvp.Value) * 0.001 + ")";
                }
            }

            if (m_ImportRestricted is not null)
            {
                sMessage += "\r\nImportRestricted: " + m_ImportRestricted.Count;
                foreach (TransferReason material in m_ImportRestricted)
                {
                    sMessage += "\r\nMaterial: " + material;
                }
            }

            if (m_WarehouseImportRestricted is not null)
            {
                sMessage += "\r\nWarehouseImportRestricted: " + m_WarehouseImportRestricted.Count;
                foreach (TransferReason material in m_WarehouseImportRestricted)
                {
                    sMessage += "\r\nMaterial: " + material;
                }
            }

            return sMessage;
        }

        private int Square(float value)
        {
            return (int)(value * value);
        }
    }
}