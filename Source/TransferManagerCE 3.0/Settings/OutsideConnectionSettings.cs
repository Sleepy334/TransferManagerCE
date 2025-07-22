using SleepyCommon;
using System.Collections.Generic;

namespace TransferManagerCE.Settings
{
    internal class OutsideConnectionSettings
    {
        const int iSETTINGS_DATA_VERSION = 1;

        public static Dictionary<ushort, OutsideConnectionSettings> s_Settings = new Dictionary<ushort, OutsideConnectionSettings>();
        static readonly object s_dictionaryLock = new object();

        public int m_cargoCapacity;
        public int m_residentCapacity;
        public int m_touristFactor0;
        public int m_touristFactor1;
        public int m_touristFactor2;
        public int m_dummyTrafficFactor;
        public string m_name;

        public OutsideConnectionSettings()
        {
            m_cargoCapacity = 20;
            m_residentCapacity = 1000;
            m_touristFactor0 = 325;
            m_touristFactor1 = 125;
            m_touristFactor2 = 50;
            m_dummyTrafficFactor = 1000;
            m_name = "";
        }

        public OutsideConnectionSettings(OutsideConnectionSettings oSecond)
        {
            m_cargoCapacity = oSecond.m_cargoCapacity;
            m_residentCapacity = oSecond.m_residentCapacity;
            m_touristFactor0 = oSecond.m_touristFactor0;
            m_touristFactor1 = oSecond.m_touristFactor1;
            m_touristFactor2 = oSecond.m_touristFactor2;
            m_dummyTrafficFactor = oSecond.m_dummyTrafficFactor;
            m_name = oSecond.m_name;
        }

        public bool Equals(OutsideConnectionSettings oSecond)
        {
            return m_name.Equals(oSecond.m_name) &&
                    m_cargoCapacity == oSecond.m_cargoCapacity &&
                    m_residentCapacity == oSecond.m_residentCapacity &&
                    m_touristFactor0 == oSecond.m_touristFactor0 &&
                    m_touristFactor1 == oSecond.m_touristFactor1 &&
                    m_touristFactor2 == oSecond.m_touristFactor2 &&
                    m_dummyTrafficFactor == oSecond.m_dummyTrafficFactor;
        }

        public string GetName(ushort buildingId)
        {
            if (string.IsNullOrEmpty(m_name))
            {
                return CitiesUtils.GetBuildingName(buildingId, false, false); 
            }
            else
            {
                return m_name;
            }
        }

        public static bool HasSettings(ushort buildingId)
        {
            if (s_Settings is not null)
            {
                lock (s_dictionaryLock)
                {
                    return (s_Settings.ContainsKey(buildingId));
                }
            }

            return false;
        }

        public static OutsideConnectionSettings GetSettings(ushort buildingId)
        {
            if (s_Settings is not null)
            {
                lock (s_dictionaryLock)
                {
                    if (s_Settings.ContainsKey(buildingId))
                    {
                        return s_Settings[buildingId];
                    }
                }
            }

            return GetDefaultSettings(buildingId);
        }

        public static OutsideConnectionSettings GetDefaultSettings(ushort buildingId)
        {
            // Default settings depend on the actual connection
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
            OutsideConnectionAI? buildingAI = building.Info.GetAI() as OutsideConnectionAI;
            if (buildingAI is not null)
            {
                OutsideConnectionSettings settings = new OutsideConnectionSettings();
                settings.m_cargoCapacity = buildingAI.m_cargoCapacity;
                settings.m_residentCapacity = buildingAI.m_residentCapacity;
                settings.m_touristFactor0 = buildingAI.m_touristFactor0;
                settings.m_touristFactor1 = buildingAI.m_touristFactor1;
                settings.m_touristFactor2 = buildingAI.m_touristFactor2;
                settings.m_dummyTrafficFactor = buildingAI.m_dummyTrafficFactor;
                return settings;
            }
            else
            {
                return new OutsideConnectionSettings();
            }
        }

        public static void SetSettings(ushort buildingId, OutsideConnectionSettings settings)
        {
            if (s_Settings is not null)
            {
                lock (s_dictionaryLock)
                {
                    if (settings.Equals(new OutsideConnectionSettings()))
                    {
                        if (s_Settings.ContainsKey(buildingId))
                        {
                            // Default values, just remove settings
                            s_Settings.Remove(buildingId);
                        }
                    }
                    else
                    {
                        // Save a copy not the pointer
                        s_Settings[buildingId] = new OutsideConnectionSettings(settings);
                    }
                }
            }
        }

        public static void ClearSettings()
        {
            if (s_Settings is not null)
            {
                lock (s_dictionaryLock)
                {
                    s_Settings.Clear();
                }
            }
        }

        public static void Reset(ushort buildingId)
        {
            if (s_Settings is not null)
            {
                lock (s_dictionaryLock)
                {
                    if (s_Settings.ContainsKey(buildingId))
                    {
                        // Default values, just remove settings
                        s_Settings.Remove(buildingId);
                    }
                }
            }
        }

        public static void SaveData(FastList<byte> Data)
        {
            StorageData.WriteInt32(iSETTINGS_DATA_VERSION, Data);
            StorageData.WriteInt32(s_Settings.Count, Data);
            foreach (KeyValuePair<ushort, OutsideConnectionSettings> kvp in s_Settings)
            {
                OutsideConnectionSettings settings = kvp.Value;

                StorageData.WriteInt32(kvp.Key, Data);
                StorageData.WriteInt32(settings.m_cargoCapacity, Data);
                StorageData.WriteInt32(settings.m_residentCapacity, Data);
                StorageData.WriteInt32(settings.m_touristFactor0, Data);
                StorageData.WriteInt32(settings.m_touristFactor1, Data);
                StorageData.WriteInt32(settings.m_touristFactor2, Data);
                StorageData.WriteInt32(settings.m_dummyTrafficFactor, Data);
                StorageData.WriteString(settings.m_name, Data);
            }
        }

        public static void LoadData(int iGlobalVersion, byte[] Data, ref int iIndex)
        {
            if (iGlobalVersion >= 14 && Data is not null && Data.Length > iIndex)
            {
                int iOutsideConnectionSettingsVersion = StorageData.ReadInt32(Data, ref iIndex);
#if DEBUG
                CDebug.Log("Global: " + iGlobalVersion + " OutsideConnectionSettingsVersion: " + iOutsideConnectionSettingsVersion + " DataLength: " + Data.Length + " Index: " + iIndex);
#endif
                OutsideConnectionSettings defaultSettings = new OutsideConnectionSettings();

                if (iOutsideConnectionSettingsVersion <= iSETTINGS_DATA_VERSION)
                {
                    int iCount = StorageData.ReadInt32(Data, ref iIndex);
                    for (int i = 0; i < iCount; ++i)
                    {
                        int buildingId = StorageData.ReadInt32(Data, ref iIndex);

                        OutsideConnectionSettings? settings = null;
                        switch (iOutsideConnectionSettingsVersion)
                        {
                            case 1: settings = LoadDataVersion1(Data, ref iIndex); break;
                            default:
                                {
                                    CDebug.Log("New data version, unable to load!");
                                    break;
                                }
                        }

                        if (settings is not null && !settings.Equals(GetDefaultSettings((ushort) buildingId)))
                        {
                            s_Settings[(ushort)buildingId] = settings;
                        }
                    }
                }
            }
        }

        private static OutsideConnectionSettings? LoadDataVersion1(byte[] Data, ref int iIndex)
        {
            OutsideConnectionSettings settings = new OutsideConnectionSettings();
            settings.m_cargoCapacity = StorageData.ReadInt32(Data, ref iIndex);
            settings.m_residentCapacity = StorageData.ReadInt32(Data, ref iIndex);
            settings.m_touristFactor0 = StorageData.ReadInt32(Data, ref iIndex);
            settings.m_touristFactor1 = StorageData.ReadInt32(Data, ref iIndex);
            settings.m_touristFactor2 = StorageData.ReadInt32(Data, ref iIndex);
            settings.m_dummyTrafficFactor = StorageData.ReadInt32(Data, ref iIndex);
            settings.m_name = StorageData.ReadString(Data, ref iIndex);
            return settings;
        }
    }
}
