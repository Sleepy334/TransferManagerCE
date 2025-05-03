using ICities;
using System;
using System.Reflection;
using TransferManagerCE.Settings;

namespace TransferManagerCE
{
    public class Serializer : ISerializableDataExtension
    {
        // Some magic values to check we are line up correctly on the tuple boundaries
        private const uint uiTUPLE_START = 0xFEFEFEFE;
        private const uint uiTUPLE_END = 0xFAFAFAFA;

        public const string DataID = "TransferManagerCE";
        public const ushort DataVersion = 45;

        public static Serializer? instance = null;
        private ISerializableData? m_serializableData = null;

        public void OnCreated(ISerializableData serializedData)
        {
            instance = this;
            m_serializableData = serializedData;
        }

        public void OnLoadData()
        {
            try
            {
                // Clear any previous settings
                TransferManagerLoader.ClearSettings();

                if (m_serializableData is not null)
                {
                    byte[] Data = m_serializableData.LoadData(DataID);
                    if (Data is not null && Data.Length > 0)
                    {
                        ushort SaveGameFileVersion;
                        int Index = 0;

                        SaveGameFileVersion = StorageData.ReadUInt16(Data, ref Index);
#if DEBUG
                        Debug.Log("Data length: " + Data.Length.ToString() + "; Data Version: " + SaveGameFileVersion);
#endif
                        if (SaveGameFileVersion <= DataVersion)
                        {
                            // Since settings version 30 the mod version is also saved in
                            // case the settings version isn't updated correctly.
                            if (SaveGameFileVersion >= 30)
                            {
                                int iMajor = StorageData.ReadInt32(Data, ref Index);
                                int iMinor = StorageData.ReadInt32(Data, ref Index);
                                int iBuild = StorageData.ReadInt32(Data, ref Index);
                                int iRevision = StorageData.ReadInt32(Data, ref Index);
                                Debug.Log($"Settings written by mod version: {iMajor}.{iMinor}.{iBuild}.{iRevision}");
                            }

                            CheckStartTuple("SaveGameSettings", SaveGameFileVersion, Data, ref Index);
                            SaveGameSettings.LoadData(SaveGameFileVersion, Data, ref Index);
                            CheckEndTuple("SaveGameSettings", SaveGameFileVersion, Data, ref Index);

                            CheckStartTuple("BuildingSettingsSerializer", SaveGameFileVersion, Data, ref Index);
                            BuildingSettingsSerializer.LoadData(SaveGameFileVersion, Data, ref Index);
                            CheckEndTuple("BuildingSettingsSerializer", SaveGameFileVersion, Data, ref Index);

                            CheckStartTuple("OutsideConnectionSettings", SaveGameFileVersion, Data, ref Index);
                            OutsideConnectionSettings.LoadData(SaveGameFileVersion, Data, ref Index);
                            CheckEndTuple("OutsideConnectionSettings", SaveGameFileVersion, Data, ref Index);
                        }
                        else
                        {
                            string sMessage = "This saved game was saved with a newer version of Transfer Manager CE.\r\n";
                            sMessage += "\r\n";
                            sMessage += "Unable to load Transfer Manager settings.\r\n";
                            sMessage += "\r\n";
                            sMessage += "Saved game data version: " + SaveGameFileVersion + "\r\n";
                            sMessage += "MOD data version: " + DataVersion + "\r\n";
                            Prompt.Info(TransferManagerMain.Title, sMessage);
                        }
                    }
                    else
                    {
                        Debug.Log("Data is null");
                    }
                }
                else
                {
                    Debug.Log("m_serializableData is null");
                }
            }
            catch (Exception ex)
            {
                string sErrorMessage = "Loading of Transfer Manager save game settings failed with the following error:\r\n";
                sErrorMessage += "\r\n";
                sErrorMessage += ex.Message;
                Prompt.ErrorFormat(TransferManagerMain.Title, sErrorMessage);
            }
        }

        public void OnSaveData()
        {
            try
            {
                if (m_serializableData is not null)
                {
                    FastList<byte> Data = new FastList<byte>();
                    // Always write out data version first
                    StorageData.WriteUInt16(DataVersion, Data);

                    // Now also writes out mod version in case I forget to incrmement settings version
                    Version modVersion = Assembly.GetExecutingAssembly().GetName().Version;
                    StorageData.WriteInt32(modVersion.Major, Data);
                    StorageData.WriteInt32(modVersion.Minor, Data);
                    StorageData.WriteInt32(modVersion.Build, Data);
                    StorageData.WriteInt32(modVersion.Revision, Data);

                    // Global settings
                    StorageData.WriteUInt32(uiTUPLE_START, Data);
                    SaveGameSettings.SaveData(Data);
                    StorageData.WriteUInt32(uiTUPLE_END, Data);

                    // Building settings
                    StorageData.WriteUInt32(uiTUPLE_START, Data);
                    BuildingSettingsSerializer.SaveData(Data);
                    StorageData.WriteUInt32(uiTUPLE_END, Data);

                    // Outside connection settings
                    StorageData.WriteUInt32(uiTUPLE_START, Data);
                    OutsideConnectionSettings.SaveData(Data);
                    StorageData.WriteUInt32(uiTUPLE_END, Data);

                    m_serializableData.SaveData(DataID, Data.ToArray());
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Could not save data. " + ex.Message);
            }
        }

        private void CheckStartTuple(string sTupleLocation, int iDataVersion, byte[] Data, ref int iIndex)
        {
            if (iDataVersion >= 17)
            {
                uint iTupleStart = StorageData.ReadUInt32(Data, ref iIndex);
                if (iTupleStart != uiTUPLE_START)
                {
                    throw new Exception($"Start tuple not found at: {sTupleLocation}");
                }
            }
        }

        private void CheckEndTuple(string sTupleLocation, int iDataVersion, byte[] Data, ref int iIndex)
        {
            if (iDataVersion >= 17)
            {
                uint iTupleStart = StorageData.ReadUInt32(Data, ref iIndex);
                if (iTupleStart != uiTUPLE_END)
                {
                    throw new Exception($"End tuple not found at: {sTupleLocation}");
                }
            }
        }

        public void OnReleased()
        {
            Serializer.instance = (Serializer)null;
        }
    }
}