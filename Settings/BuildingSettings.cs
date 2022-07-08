using System;
using System.Collections.Generic;

namespace TransferManagerCE
{
    public class BuildingSettings : IEquatable<BuildingSettings>
    {
        const int iBUILDING_SETTINGS_DATA_VERSION = 3;

        public enum PreferLocal
        {
            ALLOW_ANY_DISTRICT,
            PREFER_LOCAL_DISTRICT,
            RESTRICT_LOCAL_DISTRICT
        }

        public enum ImportExport
        {
            ALLOW_IMPORT_AND_EXPORT,
            ALLOW_IMPORT_ONLY,
            ALLOW_EXPORT_ONLY,
            ALLOW_NEITHER,
        }

        public static Dictionary<ushort, BuildingSettings> s_BuildingsSettings = new Dictionary<ushort, BuildingSettings>();
        static readonly object s_dictionaryLock = new object();

        // Building settings
        public bool m_bAllowImport;
        public bool m_bAllowExport;
        public bool m_bReserveCargoTrucks;
        public PreferLocal m_iPreferLocalDistrictsIncoming;
        public PreferLocal m_iPreferLocalDistrictsOutgoing;

        public BuildingSettings()
        {
            m_bAllowImport = true;
            m_bAllowExport = true;
            m_bReserveCargoTrucks = false;
            m_iPreferLocalDistrictsIncoming = PreferLocal.ALLOW_ANY_DISTRICT;
            m_iPreferLocalDistrictsOutgoing = PreferLocal.ALLOW_ANY_DISTRICT;
        }

        public BuildingSettings(BuildingSettings oSecond)
        {
            m_bAllowImport = oSecond.m_bAllowImport;
            m_bAllowExport = oSecond.m_bAllowExport;
            m_bReserveCargoTrucks = oSecond.m_bReserveCargoTrucks;
            m_iPreferLocalDistrictsIncoming = oSecond.m_iPreferLocalDistrictsIncoming;
            m_iPreferLocalDistrictsOutgoing = oSecond.m_iPreferLocalDistrictsOutgoing;
        }

        public bool Equals(BuildingSettings oSecond)
        {
            return m_bAllowImport == oSecond.m_bAllowImport &&
                    m_bAllowExport == oSecond.m_bAllowExport &&
                    m_bReserveCargoTrucks == oSecond.m_bReserveCargoTrucks &&
                    m_iPreferLocalDistrictsIncoming == oSecond.m_iPreferLocalDistrictsIncoming &&
                    m_iPreferLocalDistrictsOutgoing == oSecond.m_iPreferLocalDistrictsOutgoing;
        }

        public static BuildingSettings GetSettings(ushort buildingId)
        {
            if (s_BuildingsSettings != null && s_BuildingsSettings.ContainsKey(buildingId))
            {
                return s_BuildingsSettings[buildingId];
            }
            else
            {
                return new BuildingSettings();
            }
        }

        public static void SetSettings(ushort buildingId, BuildingSettings settings)
        {
            if (s_BuildingsSettings != null)
            {
                s_BuildingsSettings[buildingId] = new BuildingSettings(settings); // Save a copy
            }
        }

        public static void SaveData(FastList<byte> Data)
        {
            StorageData.WriteInt32(iBUILDING_SETTINGS_DATA_VERSION, Data);
            StorageData.WriteInt32(s_BuildingsSettings.Count, Data);
            foreach (KeyValuePair<ushort, BuildingSettings> buildingSettings in s_BuildingsSettings)
            {
                StorageData.WriteInt32(buildingSettings.Key, Data);
                StorageData.WriteBool(buildingSettings.Value.m_bAllowImport, Data); 
                StorageData.WriteBool(buildingSettings.Value.m_bAllowExport, Data);
                StorageData.WriteBool(buildingSettings.Value.m_bReserveCargoTrucks, Data);
                StorageData.WriteInt32((int)buildingSettings.Value.m_iPreferLocalDistrictsIncoming, Data);
                StorageData.WriteInt32((int)buildingSettings.Value.m_iPreferLocalDistrictsOutgoing, Data);
            }
        }

        public static void LoadData(int iVersion, byte[] Data, ref int iIndex)
        {
            Debug.Log("Version: " + iVersion + " DataLength: " + Data.Length + " Index: " + iIndex);
            if (Data != null && Data.Length > iIndex)
            {
                switch (iVersion)
                {
                    case 1:
                        {
                            LoadDataVersion1(Data, ref iIndex);
                            break;
                        }
                    case 2:
                        {
                            LoadDataVersion2(Data, ref iIndex);
                            break;
                        }
                    case 3:
                        {
                            LoadDataVersion3(Data, ref iIndex);
                            break;
                        }
                    case 4:
                        {
                            LoadDataVersion4(Data, ref iIndex);
                            break;
                        }
                    default:
                        {
                            Debug.Log("New data version, unable to load!");
                            break;
                        }
                }
            }
#if DEBUG
            Debug.Log("Building Count: " + s_BuildingsSettings.Count);
            foreach (KeyValuePair<ushort, BuildingSettings> kvp in s_BuildingsSettings)
            {
                Debug.Log(DebugSettings(kvp.Key));
            }
#endif
        }

        public static string DebugSettings(ushort buildingId)
        {
            if (s_BuildingsSettings != null && s_BuildingsSettings.ContainsKey(buildingId))
            {
                BuildingSettings settings = s_BuildingsSettings[buildingId];
                return "Building: " + buildingId +
                                " Import: " + settings.m_bAllowImport +
                                " Export: " + settings.m_bAllowExport +
                                " ReserveCargo: " + settings.m_bReserveCargoTrucks +
                                " PreferLocalIncoming: " + settings.m_iPreferLocalDistrictsIncoming +
                                " PreferLocalOutgoing: " + settings.m_iPreferLocalDistrictsOutgoing;
            }
            else
            {
                return "Not found";
            }
        }

        public static string GetTooltipText(ushort buildingId)
        {
            if (s_BuildingsSettings != null && s_BuildingsSettings.ContainsKey(buildingId))
            {
                BuildingSettings settings = s_BuildingsSettings[buildingId];
                return "Prefer Local Incoming: " + settings.m_iPreferLocalDistrictsIncoming + "\r\n" +
                        "Prefer Local Outgoing: " + settings.m_iPreferLocalDistrictsOutgoing + "\r\n" +
                        "Import: " + settings.m_bAllowImport + "\r\n" +
                        "Export: " + settings.m_bAllowExport + "\r\n" +
                       "Reserve Cargo: " + settings.m_bReserveCargoTrucks + "\r\n";
                       
            }
            else
            {
                return "No local settings";
            }
        }

        public static void SetImport(ushort buildingId, bool bAllow)
        {
            if (buildingId != 0)
            {
                lock (s_dictionaryLock)
                {
                    if (s_BuildingsSettings.ContainsKey(buildingId))
                    {
                        BuildingSettings settings = s_BuildingsSettings[buildingId];
                        settings.m_bAllowImport = bAllow;
                        s_BuildingsSettings[buildingId] = settings;
                    }
                    else
                    {
                        BuildingSettings settings = new BuildingSettings();
                        settings.m_bAllowImport = bAllow;
                        s_BuildingsSettings[buildingId] = settings;
                    }
                }
            }
        }

        public static bool GetImport(ushort buildingId)
        {
            lock (s_dictionaryLock)
            {
                if (s_BuildingsSettings.ContainsKey(buildingId))
                {
                    return s_BuildingsSettings[buildingId].m_bAllowImport;
                }
                return true;
            }
        }

        public static void SetExport(ushort buildingId, bool bAllow)
        {
            if (buildingId != 0)
            {
                lock (s_dictionaryLock)
                {
                    if (s_BuildingsSettings.ContainsKey(buildingId))
                    {
                        BuildingSettings settings = s_BuildingsSettings[buildingId];
                        settings.m_bAllowExport = bAllow;
                        s_BuildingsSettings[buildingId] = settings;
                    }
                    else
                    {
                        BuildingSettings settings = new BuildingSettings();
                        settings.m_bAllowExport = bAllow;
                        s_BuildingsSettings[buildingId] = settings;
                    }
                }
            }
        }

        public static bool GetExport(ushort buildingId)
        {
            lock (s_dictionaryLock)
            {
                if (s_BuildingsSettings.ContainsKey(buildingId))
                {
                    return s_BuildingsSettings[buildingId].m_bAllowExport;
                }
                return true;
            }
        }

        public static bool IsExportDisabled(ushort buildingId)
        {
            return !GetExport(buildingId);
        }

        public static bool IsImportDisabled(ushort buildingId)
        {
            return !GetImport(buildingId);
        }

        public static PreferLocal PreferLocalDistrictServicesIncoming(ushort buildingId)
        {
            lock (s_dictionaryLock)
            {
                if (s_BuildingsSettings.ContainsKey(buildingId))
                {
                    return (PreferLocal)s_BuildingsSettings[buildingId].m_iPreferLocalDistrictsIncoming;
                }
                return PreferLocal.ALLOW_ANY_DISTRICT;
            }
        }

        public static PreferLocal PreferLocalDistrictServicesOutgoing(ushort buildingId)
        {
            lock (s_dictionaryLock)
            {
                if (s_BuildingsSettings.ContainsKey(buildingId))
                {
                    return (PreferLocal)s_BuildingsSettings[buildingId].m_iPreferLocalDistrictsOutgoing;
                }
                return PreferLocal.ALLOW_ANY_DISTRICT;
            }
        }

        public static void PreferLocalDistrictServicesIncoming(ushort buildingId, PreferLocal value)
        {
            if (buildingId != 0)
            {
                lock (s_dictionaryLock)
                {
                    if (s_BuildingsSettings.ContainsKey(buildingId))
                    {
                        BuildingSettings settings = s_BuildingsSettings[buildingId];
                        settings.m_iPreferLocalDistrictsIncoming = value;
                        s_BuildingsSettings[buildingId] = settings;
                    }
                    else
                    {
                        BuildingSettings settings = new BuildingSettings();
                        settings.m_iPreferLocalDistrictsIncoming = value;
                        s_BuildingsSettings[buildingId] = settings;
                    }
                }
            }
        }

        public static void PreferLocalDistrictServicesOutgoing(ushort buildingId, PreferLocal value)
        {
            if (buildingId != 0)
            {
                lock (s_dictionaryLock)
                {
                    if (s_BuildingsSettings.ContainsKey(buildingId))
                    {
                        BuildingSettings settings = s_BuildingsSettings[buildingId];
                        settings.m_iPreferLocalDistrictsOutgoing = value;
                        s_BuildingsSettings[buildingId] = settings;
                    }
                    else
                    {
                        BuildingSettings settings = new BuildingSettings();
                        settings.m_iPreferLocalDistrictsOutgoing = value;
                        s_BuildingsSettings[buildingId] = settings;
                    }
                }
            }
        }

        public static bool IsReserveCargoTrucks(ushort buildingId)
        {
            lock (s_dictionaryLock)
            {
                if (s_BuildingsSettings.ContainsKey(buildingId))
                {
                    return s_BuildingsSettings[buildingId].m_bReserveCargoTrucks;
                }
                return false;
            }
        }

        public static void ReserveCargoTrucks(ushort buildingId, bool bEnable)
        {
            if (buildingId != 0)
            {
                lock (s_dictionaryLock)
                {
                    if (s_BuildingsSettings.ContainsKey(buildingId))
                    {
                        BuildingSettings settings = s_BuildingsSettings[buildingId];
                        settings.m_bReserveCargoTrucks = bEnable;
                        s_BuildingsSettings[buildingId] = settings;
                    }
                    else
                    {
                        BuildingSettings settings = new BuildingSettings();
                        settings.m_bReserveCargoTrucks = bEnable;
                        s_BuildingsSettings[buildingId] = settings;
                    }
                }
            }
        }

        // Hooks into BuildingManager do not change
        public static void ReleaseBuilding(ushort buildingId)
        {
            lock (s_dictionaryLock)
            {
                if (s_BuildingsSettings.ContainsKey(buildingId))
                {
                    s_BuildingsSettings.Remove(buildingId);
                }
            }
        }

        private static void LoadDataVersion4(byte[] Data, ref int iIndex)
        {
            int iSettingsVersion = StorageData.ReadInt32(Data, ref iIndex);
            int iCount = StorageData.ReadInt32(Data, ref iIndex);
            for (int i = 0; i < iCount; ++i)
            {
                int buildingId = StorageData.ReadInt32(Data, ref iIndex);
                BuildingSettings settings = new BuildingSettings();
                settings.m_bAllowImport = StorageData.ReadBool(Data, ref iIndex); 
                settings.m_bAllowExport = StorageData.ReadBool(Data, ref iIndex);
                settings.m_bReserveCargoTrucks = StorageData.ReadBool(Data, ref iIndex);
                settings.m_iPreferLocalDistrictsIncoming = (PreferLocal)StorageData.ReadInt32(Data, ref iIndex);
                settings.m_iPreferLocalDistrictsOutgoing = (PreferLocal)StorageData.ReadInt32(Data, ref iIndex);

                s_BuildingsSettings[(ushort)buildingId] = settings;
            }
        }

        private static void LoadDataVersion3(byte[] Data, ref int iIndex)
        {
            int iSettingsVersion = StorageData.ReadInt32(Data, ref iIndex);
            int iCount = StorageData.ReadInt32(Data, ref iIndex);
            for (int i = 0; i < iCount; ++i)
            {
                int buildingId = StorageData.ReadInt32(Data, ref iIndex);
                BuildingSettings settings = new BuildingSettings();
                ImportExport eValue = (ImportExport)StorageData.ReadInt32(Data, ref iIndex);
                settings.m_bAllowImport = (eValue == ImportExport.ALLOW_IMPORT_AND_EXPORT || eValue == ImportExport.ALLOW_IMPORT_ONLY);
                settings.m_bAllowExport = (eValue == ImportExport.ALLOW_IMPORT_AND_EXPORT || eValue == ImportExport.ALLOW_EXPORT_ONLY);
                settings.m_bReserveCargoTrucks = StorageData.ReadBool(Data, ref iIndex);

                // Convert 1 prefer local to 2
                PreferLocal ePreferLocal = (PreferLocal)StorageData.ReadInt32(Data, ref iIndex);
                settings.m_iPreferLocalDistrictsIncoming = ePreferLocal;
                settings.m_iPreferLocalDistrictsOutgoing = ePreferLocal;

                s_BuildingsSettings[(ushort)buildingId] = settings;
            }
        }

        private static void LoadDataVersion2(byte[] Data, ref int iIndex)
        {
            // Not supported, didn't really exist in the wild for long
        }

        private static void LoadDataVersion1(byte[] Data, ref int iIndex)
        {
            // Not supported, didn't really exist in the wild for long
        }
    }
}