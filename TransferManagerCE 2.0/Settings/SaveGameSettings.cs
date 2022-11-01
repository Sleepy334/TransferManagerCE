using System;
using System.Collections.Generic;
using TransferManagerCE.CustomManager;
using TransferManagerCE.Util;
using static TransferManager;

namespace TransferManagerCE
{
    public class SaveGameSettings
    {
        const int iSAVE_GAME_SETTINGS_DATA_VERSION = 12;
        public static SaveGameSettings s_SaveGameSettings = new SaveGameSettings();

        // Settings
        public bool EnableNewTransferManager = true;

        // General tab
        public bool UsePathDistanceServices = false;
        public bool UsePathDistanceGoods = false;
        public CustomTransferManager.BalancedMatchModeOption BalancedMatchMode = CustomTransferManager.BalancedMatchModeOption.MatchModeIncomingFirst;
        public bool DisableDummyTraffic = false;

        // Warehouse options
        public bool WarehouseFirst = false;
        public int WarehouseReserveTrucksPercent = 0; // [0..100]
        public bool NewInterWarehouseTransfer = false;
        
        // Outside connections
        public int OutsideShipMultiplier = 1;
        public int OutsidePlaneMultiplier = 1;
        public int OutsideTrainMultiplier = 1;
        public int OutsideRoadMultiplier = 1;

        // Services
        public bool PreferLocalService = false;
        public bool ExperimentalDeathcare = false;
        public bool ExperimentalGarbage = false;
        public bool ExperimentalCrime = false;

        // VehicleAI
        public bool FireTruckAI = true;
        public bool FireCopterAI = true;
        public bool GarbageTruckAI = false;
        public bool PoliceCarAI = false;
        public bool PoliceCopterAI = false;
        
        // Arrays
        private Dictionary<TransferReason, int> m_ActiveDistanceRestrictions = new Dictionary<TransferReason, int>();
        private List<TransferReason> m_ImportRestricted = new List<TransferReason>();
        private List<TransferReason> m_WarehouseImportRestricted = new List<TransferReason>();
        
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

        public float GetActiveDistanceRestrictionKm(TransferReason material)
        {
            return (float)Math.Sqrt(GetActiveDistanceRestrictionSquaredMeters(material)) * 0.001f;
        }

        public void SetActiveDistanceRestrictionKm(TransferReason material, float fValueKm)
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

        public float GetActiveDistanceRestrictionSquaredMeters(TransferReason material)
        {
            if (m_ActiveDistanceRestrictions.ContainsKey(material))
            {
                return m_ActiveDistanceRestrictions[material];
            }
            return 0f;
        }

        public bool IsImportRestricted(TransferReason material)
        {
            return m_ImportRestricted.Contains(material);
        }

        public void SetImportRestriction(TransferReason material, bool bRestricted)
        {
            if (bRestricted)
            {
                if (!m_ImportRestricted.Contains(material))
                {
                    m_ImportRestricted.Add(material);
                }
            }
            else
            {
                while (m_ImportRestricted.Contains(material))
                {
                    m_ImportRestricted.Remove(material);
                }
            }
        }
        public bool IsWarehouseImportRestricted(TransferReason material)
        {
            return m_WarehouseImportRestricted.Contains(material);
        }

        public void SetWarehouseImportRestriction(TransferReason material, bool bRestricted)
        {
            if (bRestricted)
            {
                if (!m_WarehouseImportRestricted.Contains(material))
                {
                    m_WarehouseImportRestricted.Add(material);
                }
            }
            else
            {
                while (m_WarehouseImportRestricted.Contains(material))
                {
                    m_WarehouseImportRestricted.Remove(material);
                }
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
            StorageData.WriteBool(PreferLocalService, Data);
            StorageData.WriteInt32(OutsideShipMultiplier, Data);
            StorageData.WriteInt32(OutsidePlaneMultiplier, Data);
            StorageData.WriteInt32(OutsideTrainMultiplier, Data);
            StorageData.WriteInt32(OutsideRoadMultiplier, Data);
            StorageData.WriteBool(WarehouseFirst, Data);
            StorageData.WriteInt32(WarehouseReserveTrucksPercent, Data); // New in version 6
            StorageData.WriteBool(ExperimentalDeathcare, Data);
            StorageData.WriteBool(ExperimentalGarbage, Data);
            StorageData.WriteBool(DisableDummyTraffic, Data); // New in version 5

            StorageData.WriteBool(FireTruckAI, Data);
            StorageData.WriteBool(FireCopterAI, Data);
            StorageData.WriteBool(GarbageTruckAI, Data);
            StorageData.WriteBool(PoliceCarAI, Data);
            StorageData.WriteBool(PoliceCopterAI, Data);

            StorageData.WriteInt32(m_ActiveDistanceRestrictions.Count, Data);
            foreach (KeyValuePair<TransferReason, int> kvp in m_ActiveDistanceRestrictions)
            {
                StorageData.WriteInt32((int)kvp.Key, Data);
                StorageData.WriteInt32(kvp.Value, Data);
            }

            StorageData.WriteInt32(m_ImportRestricted.Count, Data);
            foreach (TransferReason material in m_ImportRestricted)
            {
                StorageData.WriteInt32((int)material, Data);
            }

            StorageData.WriteInt32(m_WarehouseImportRestricted.Count, Data);
            foreach (TransferReason material in m_WarehouseImportRestricted)
            {
                StorageData.WriteInt32((int)material, Data);
            }

            StorageData.WriteBool(NewInterWarehouseTransfer, Data);
            StorageData.WriteBool(ExperimentalCrime, Data);
            StorageData.WriteInt32((int)BalancedMatchMode, Data);
            StorageData.WriteBool(UsePathDistanceServices, Data);
            StorageData.WriteBool(UsePathDistanceGoods, Data);
        }

        public static void LoadData(int iGlobalVersion, byte[] Data, ref int iIndex)
        {
            if (iGlobalVersion >= 6 && Data != null && Data.Length > iIndex)
            {
                int iSaveGameSettingVersion = StorageData.ReadInt32(Data, ref iIndex);
#if DEBUG
                DebugLog.LogInfo("Global: " + iGlobalVersion + " SaveGameVersion: " + iSaveGameSettingVersion + " DataLength: " + Data.Length + " Index: " + iIndex);
#endif
                if (s_SaveGameSettings != null)
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
                        default:
                            {
                                Debug.Log("New data version, unable to load!");
                                break;
                            }
                    }
#if DEBUG
                    DebugLog.LogInfo("Settings:\r\n" + s_SaveGameSettings.DebugSettings());
#endif
                }
            }
        }

        private void LoadDataVersion12(byte[] Data, ref int iIndex)
        {
            LoadDataVersion11(Data, ref iIndex);
            UsePathDistanceGoods = StorageData.ReadBool(Data, ref iIndex); // New in version 11
        }

        private void LoadDataVersion11(byte[] Data, ref int iIndex)
        {
            LoadDataVersion10(Data, ref iIndex);
            UsePathDistanceServices = StorageData.ReadBool(Data, ref iIndex); // New in version 11
        }

        private void LoadDataVersion10(byte[] Data, ref int iIndex)
        {
            LoadDataVersion9(Data, ref iIndex);
            BalancedMatchMode = (CustomTransferManager.BalancedMatchModeOption) StorageData.ReadInt32(Data, ref iIndex); // New in version 10
        }

        private void LoadDataVersion9(byte[] Data, ref int iIndex)
        {
            LoadDataVersion8(Data, ref iIndex);
            ExperimentalCrime = StorageData.ReadBool(Data, ref iIndex); // New in version 9
        }

        private void LoadDataVersion8(byte[] Data, ref int iIndex)
        {
            LoadDataVersion7(Data, ref iIndex);
            NewInterWarehouseTransfer = StorageData.ReadBool(Data, ref iIndex); // New in version 8
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
            WarehouseReserveTrucksPercent = StorageData.ReadInt32(Data, ref iIndex); // New in version 6
            ExperimentalDeathcare = StorageData.ReadBool(Data, ref iIndex);
            ExperimentalGarbage = StorageData.ReadBool(Data, ref iIndex);
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
            WarehouseReserveTrucksPercent = StorageData.ReadInt32(Data, ref iIndex); // New in version 6
            ExperimentalDeathcare = StorageData.ReadBool(Data, ref iIndex);
            ExperimentalGarbage = StorageData.ReadBool(Data, ref iIndex);
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
            float fWarehouseReserveTrucksPercent = StorageData.ReadFloat(Data, ref iIndex);
            WarehouseReserveTrucksPercent = (int)(fWarehouseReserveTrucksPercent * 100.0f);
        }

        private void LoadDataVersion3(byte[] Data, ref int iIndex)
        {
            EnableNewTransferManager = StorageData.ReadBool(Data, ref iIndex);
            PreferLocalService = StorageData.ReadBool(Data, ref iIndex);
            bool PreferExportShip = StorageData.ReadBool(Data, ref iIndex);
            bool PreferExportPlane = StorageData.ReadBool(Data, ref iIndex);
            bool PreferExportTrain = StorageData.ReadBool(Data, ref iIndex);
            WarehouseFirst = StorageData.ReadBool(Data, ref iIndex);
            bool bWarehouseReserve = StorageData.ReadBool(Data, ref iIndex);
            WarehouseReserveTrucksPercent = bWarehouseReserve ? 25 : 0;
            ExperimentalDeathcare = StorageData.ReadBool(Data, ref iIndex);
            ExperimentalGarbage = StorageData.ReadBool(Data, ref iIndex);
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
            bool bWarehouseReserve = StorageData.ReadBool(Data, ref iIndex);
            WarehouseReserveTrucksPercent = bWarehouseReserve ? 25 : 0;
            ExperimentalDeathcare = StorageData.ReadBool(Data, ref iIndex);
            ExperimentalGarbage = StorageData.ReadBool(Data, ref iIndex);

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
            bool bWarehouseReserve = StorageData.ReadBool(Data, ref iIndex);
            WarehouseReserveTrucksPercent = bWarehouseReserve ? 25 : 0;
            ExperimentalDeathcare = StorageData.ReadBool(Data, ref iIndex);

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
                    m_ActiveDistanceRestrictions[(TransferReason)key] = iValue;
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
                    TransferReason material = (TransferReason)StorageData.ReadInt32(Data, ref iIndex);
                    if (!m_ImportRestricted.Contains(material) && TransferRestrictions.IsImportRestrictionsSupported(material))
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
                    TransferReason material = (TransferReason)StorageData.ReadInt32(Data, ref iIndex);
                    if (!m_WarehouseImportRestricted.Contains(material) && TransferRestrictions.IsImportRestrictionsSupported(material))
                    {
                        m_WarehouseImportRestricted.Add(material);
                    }
                }
            }
        }

        public string DebugSettings()
        {
            string sMessage = "===== Save Game Settings =====\r\n";
            sMessage += "EnableNewTransferManager" + EnableNewTransferManager + "\r\n";
            sMessage += "PreferLocalService" + PreferLocalService + "\r\n";
            sMessage += "ShipMultiplier" + OutsideShipMultiplier + "\r\n";
            sMessage += "PlaneMultiplier" + OutsidePlaneMultiplier + "\r\n";
            sMessage += "TrainMultiplier" + OutsideTrainMultiplier + "\r\n";
            sMessage += "RoadMultiplier" + OutsideRoadMultiplier + "\r\n";
            sMessage += "WarehouseFirst" + WarehouseFirst + "\r\n";
            sMessage += "WarehouseReserveTrucks" + WarehouseReserveTrucksPercent + "\r\n";
            sMessage += "ExperimentalDeathcare" + ExperimentalDeathcare + "\r\n";
            sMessage += "ExperimentalGarbage" + ExperimentalGarbage + "\r\n";
            sMessage += "FireTruckAI" + FireTruckAI + "\r\n";
            sMessage += "FireCopterAI" + FireCopterAI + "\r\n";
            sMessage += "GarbageTruckAI" + GarbageTruckAI + "\r\n";
            sMessage += "PoliceCarAI" + PoliceCarAI + "\r\n";
            sMessage += "PoliceCopterAI" + PoliceCopterAI + "\r\n";

            if (m_ActiveDistanceRestrictions != null)
            {
                sMessage += "\r\nDistanceRestrictionCount: " + m_ActiveDistanceRestrictions.Count;
                foreach (KeyValuePair<TransferReason, int> kvp in m_ActiveDistanceRestrictions)
                {
                    sMessage += "\r\nKey: " + kvp.Key + " Value: " + kvp.Value + " (" + Math.Sqrt(kvp.Value) * 0.001 + ")";
                }
            }

            if (m_ImportRestricted != null)
            {
                sMessage += "\r\nImportRestricted: " + m_ImportRestricted.Count;
                foreach (TransferReason material in m_ImportRestricted)
                {
                    sMessage += "\r\nMaterial: " + material;
                }
            }

            if (m_WarehouseImportRestricted != null)
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