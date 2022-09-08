using System.Collections.Generic;
using TransferManagerCE.Util;
using static TransferManagerCE.BuildingSettings;

namespace TransferManagerCE
{
    public class BuildingSettingsSerializer
    {
        const int iBUILDING_SETTINGS_DATA_VERSION = 9;

        public static void SaveData(FastList<byte> Data)
        {
            StorageData.WriteInt32(iBUILDING_SETTINGS_DATA_VERSION, Data);
            StorageData.WriteInt32(s_BuildingsSettings.Count, Data);
            foreach (KeyValuePair<ushort, BuildingSettings> kvp in s_BuildingsSettings)
            {
                BuildingSettings settings = kvp.Value;

                StorageData.WriteInt32(kvp.Key, Data);
                StorageData.WriteBool(settings.m_bAllowImport, Data);
                StorageData.WriteBool(settings.m_bAllowExport, Data);
                StorageData.WriteInt32((int)settings.m_iPreferLocalDistrictsIncoming, Data);
                StorageData.WriteInt32((int)settings.m_iPreferLocalDistrictsOutgoing, Data);
                StorageData.WriteBool(settings.m_bDistrictAllowServices, Data);
                StorageData.WriteInt32(settings.m_iServiceDistance, Data);
                StorageData.WriteBool(settings.m_bWarehouseOverride, Data);
                StorageData.WriteBool(settings.m_bWarehouseFirst, Data);
                StorageData.WriteInt32(settings.m_iWarehouseReserveTrucksPercent, Data);
                StorageData.WriteInt32(settings.m_iOutsideMultiplier, Data);
                StorageData.WriteBool(settings.m_bIncomingAllowLocalDistrict, Data);
                StorageData.WriteBool(settings.m_bIncomingAllowLocalPark, Data);
                StorageData.WriteBool(settings.m_bOutgoingAllowLocalDistrict, Data);
                StorageData.WriteBool(settings.m_bOutgoingAllowLocalPark, Data);

                StorageData.WriteInt32(settings.m_incomingDistrictAllowed.Count, Data);
                foreach (DistrictData data in settings.m_incomingDistrictAllowed)
                {
                    data.SaveData(Data);
                }

                StorageData.WriteInt32(settings.m_outgoingDistrictAllowed.Count, Data);
                foreach (DistrictData data in settings.m_outgoingDistrictAllowed)
                {
                    data.SaveData(Data);
                }
            }
        }

        public static void LoadData(int iGlobalVersion, byte[] Data, ref int iIndex)
        {
            if (Data != null && Data.Length > iIndex)
            {
                int iBuildingSettingsVersion = StorageData.ReadInt32(Data, ref iIndex);
#if DEBUG
                DebugLog.LogInfo("Global: " + iGlobalVersion + " BuildingVersion: " + iBuildingSettingsVersion + " DataLength: " + Data.Length + " Index: " + iIndex);
#endif
                BuildingSettings defaultSettings = new BuildingSettings();

                if (iBuildingSettingsVersion <= iBUILDING_SETTINGS_DATA_VERSION)
                {
                    int iCount = StorageData.ReadInt32(Data, ref iIndex);
                    for (int i = 0; i < iCount; ++i)
                    {
                        int buildingId = StorageData.ReadInt32(Data, ref iIndex);

                        BuildingSettings? settings = null;
                        switch (iBuildingSettingsVersion)
                        {
                            case 1: settings = LoadDataVersion1(Data, ref iIndex); break;
                            case 2: settings = LoadDataVersion2(Data, ref iIndex); break;
                            case 3:
                                {
                                    // Forgot to increment building setting value here, trigger off global
                                    if (iGlobalVersion == 3)
                                    {
                                        settings = LoadDataVersion3_3(Data, ref iIndex); // 3_3 = Building_Global
                                    }
                                    else if (iGlobalVersion == 4)
                                    {
                                        settings = LoadDataVersion3_4(Data, ref iIndex); // 3_4 = Building_Global
                                    }
                                    break;
                                }
                            case 4: settings = LoadDataVersion4(Data, ref iIndex); break;
                            case 5: settings = LoadDataVersion5(Data, ref iIndex); break;
                            case 6: settings = LoadDataVersion6(Data, ref iIndex); break;
                            case 7: settings = LoadDataVersion7(Data, ref iIndex); break;
                            case 8: settings = LoadDataVersion8(Data, ref iIndex); break;
                            case 9: settings = LoadDataVersion9(Data, ref iIndex); break;
                            default:
                                {
                                    Debug.Log("New data version, unable to load!");
                                    break;
                                }
                        }

                        if (settings != null && !settings.Equals(defaultSettings))
                        {
                            s_BuildingsSettings[(ushort)buildingId] = settings;
                        }
                    }
                } 
                else
                {
                    Debug.Log("New data version, unable to load!");
                }
            }
#if DEBUG
            DebugLog.LogInfo("===== Building Settings =====");
            DebugLog.LogInfo("Building Count: " + s_BuildingsSettings.Count);
            foreach (KeyValuePair<ushort, BuildingSettings> kvp in s_BuildingsSettings)
            {
                DebugLog.LogInfo(DebugSettings(kvp.Key));
            }
            DebugLog.LogInfo("==========================");
#endif
        }

        private static HashSet<DistrictData> LoadDistrictAllowed(byte[] Data, ref int iIndex)
        {
            HashSet<DistrictData> list = new HashSet<DistrictData>();

            if (iIndex < Data.Length)
            {
                int iCount = StorageData.ReadInt32(Data, ref iIndex);
                for (int i = 0; i < iCount; ++i)
                {
                    DistrictData data = DistrictData.LoadData(Data, ref iIndex);
                    list.Add(data);
                }
            }

            return list;
        }

        private static BuildingSettings? LoadDataVersion9(byte[] Data, ref int iIndex)
        {
            BuildingSettings settings = new BuildingSettings();
            settings.m_bAllowImport = StorageData.ReadBool(Data, ref iIndex);
            settings.m_bAllowExport = StorageData.ReadBool(Data, ref iIndex);
            settings.m_iPreferLocalDistrictsIncoming = (PreferLocal)StorageData.ReadInt32(Data, ref iIndex);
            settings.m_iPreferLocalDistrictsOutgoing = (PreferLocal)StorageData.ReadInt32(Data, ref iIndex);
            settings.m_bDistrictAllowServices = StorageData.ReadBool(Data, ref iIndex);
            settings.m_iServiceDistance = StorageData.ReadInt32(Data, ref iIndex);
            settings.m_bWarehouseOverride = StorageData.ReadBool(Data, ref iIndex);
            settings.m_bWarehouseFirst = StorageData.ReadBool(Data, ref iIndex);
            settings.m_iWarehouseReserveTrucksPercent = StorageData.ReadInt32(Data, ref iIndex);
            settings.m_iOutsideMultiplier = StorageData.ReadInt32(Data, ref iIndex);
            settings.m_bIncomingAllowLocalDistrict = StorageData.ReadBool(Data, ref iIndex);
            settings.m_bIncomingAllowLocalPark = StorageData.ReadBool(Data, ref iIndex);
            settings.m_bOutgoingAllowLocalDistrict = StorageData.ReadBool(Data, ref iIndex);
            settings.m_bOutgoingAllowLocalPark = StorageData.ReadBool(Data, ref iIndex);

            settings.m_incomingDistrictAllowed = LoadDistrictAllowed(Data, ref iIndex);
            settings.m_outgoingDistrictAllowed = LoadDistrictAllowed(Data, ref iIndex);

            return settings;
        }

        private static BuildingSettings? LoadDataVersion8(byte[] Data, ref int iIndex)
        {
            // version 8 is the same as 7 except new district option ALL_DISTRICTS_EXCEPT_FOR
            return LoadDataVersion7(Data, ref iIndex);
        }

        private static BuildingSettings? LoadDataVersion7(byte[] Data, ref int iIndex)
        {
            BuildingSettings? settings = LoadDataVersion6(Data, ref iIndex);
            if (settings != null)
            {
                settings.m_bIncomingAllowLocalDistrict = StorageData.ReadBool(Data, ref iIndex);
                settings.m_bIncomingAllowLocalPark = StorageData.ReadBool(Data, ref iIndex);
                settings.m_bOutgoingAllowLocalDistrict = StorageData.ReadBool(Data, ref iIndex);
                settings.m_bOutgoingAllowLocalPark = StorageData.ReadBool(Data, ref iIndex);

                settings.m_incomingDistrictAllowed = LoadDistrictAllowed(Data, ref iIndex);
                settings.m_outgoingDistrictAllowed = LoadDistrictAllowed(Data, ref iIndex);
            }
            return settings;
        }

        private static BuildingSettings? LoadDataVersion6(byte[] Data, ref int iIndex)
        {
            BuildingSettings? settings = LoadDataVersion5(Data, ref iIndex);
            if (settings != null)
            {
                settings.m_iOutsideMultiplier = StorageData.ReadInt32(Data, ref iIndex);
            }
            return settings;
        }

        private static BuildingSettings? LoadDataVersion5(byte[] Data, ref int iIndex)
        {
            BuildingSettings settings = new BuildingSettings();
            settings.m_bAllowImport = StorageData.ReadBool(Data, ref iIndex);
            settings.m_bAllowExport = StorageData.ReadBool(Data, ref iIndex);
            settings.m_iPreferLocalDistrictsIncoming = (PreferLocal)StorageData.ReadInt32(Data, ref iIndex);
            settings.m_iPreferLocalDistrictsOutgoing = (PreferLocal)StorageData.ReadInt32(Data, ref iIndex);
            settings.m_bDistrictAllowServices = StorageData.ReadBool(Data, ref iIndex);
            settings.m_bWarehouseOverride = StorageData.ReadBool(Data, ref iIndex);
            settings.m_bWarehouseFirst = StorageData.ReadBool(Data, ref iIndex);
            settings.m_iWarehouseReserveTrucksPercent = StorageData.ReadInt32(Data, ref iIndex);
            return settings;
        }

        private static BuildingSettings? LoadDataVersion4(byte[] Data, ref int iIndex)
        {
            BuildingSettings? settings = LoadDataVersion3_4(Data, ref iIndex);
            if (settings != null)
            {
                settings.m_bDistrictAllowServices = StorageData.ReadBool(Data, ref iIndex);
                settings.m_bWarehouseFirst = StorageData.ReadBool(Data, ref iIndex);
            }
            return settings;
        }

        private static BuildingSettings? LoadDataVersion3_4(byte[] Data, ref int iIndex)
        {
            BuildingSettings settings = new BuildingSettings();
            settings.m_bAllowImport = StorageData.ReadBool(Data, ref iIndex);
            settings.m_bAllowExport = StorageData.ReadBool(Data, ref iIndex);
            bool bReserveWarehouseTrucks = StorageData.ReadBool(Data, ref iIndex); // Ignore we no longer use
            settings.m_iPreferLocalDistrictsIncoming = (PreferLocal)StorageData.ReadInt32(Data, ref iIndex);
            settings.m_iPreferLocalDistrictsOutgoing = (PreferLocal)StorageData.ReadInt32(Data, ref iIndex);
            return settings;
        }

        private static BuildingSettings? LoadDataVersion3_3(byte[] Data, ref int iIndex)
        {
            BuildingSettings settings = new BuildingSettings();

            ImportExport eValue = (ImportExport)StorageData.ReadInt32(Data, ref iIndex);
            settings.m_bAllowImport = (eValue == ImportExport.ALLOW_IMPORT_AND_EXPORT || eValue == ImportExport.ALLOW_IMPORT_ONLY);
            settings.m_bAllowExport = (eValue == ImportExport.ALLOW_IMPORT_AND_EXPORT || eValue == ImportExport.ALLOW_EXPORT_ONLY);
            bool bReserveWarehouseTrucks = StorageData.ReadBool(Data, ref iIndex); // Ignore we no longer use

            // Convert 1 prefer local to 2
            PreferLocal ePreferLocal = (PreferLocal)StorageData.ReadInt32(Data, ref iIndex);
            settings.m_iPreferLocalDistrictsIncoming = ePreferLocal;
            settings.m_iPreferLocalDistrictsOutgoing = ePreferLocal;

            return settings;
        }

        private static BuildingSettings? LoadDataVersion2(byte[] Data, ref int iIndex)
        {
            // Not supported, didn't really exist in the wild for long
            return null;
        }

        private static BuildingSettings? LoadDataVersion1(byte[] Data, ref int iIndex)
        {
            // Not supported, didn't really exist in the wild for long
            return null;
        }
    }
}