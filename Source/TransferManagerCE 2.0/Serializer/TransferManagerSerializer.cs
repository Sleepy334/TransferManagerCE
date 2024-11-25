using ICities;
using System;
using TransferManagerCE.Settings;

namespace TransferManagerCE
{
    public class Serializer : ISerializableDataExtension
    {
        // Some magic values to check we are on correctly on the tuple boundaries
        private const uint uiTUPLE_START = 0xFEFEFEFE;
        private const uint uiTUPLE_END = 0xFAFAFAFA;

        public const string DataID = "TransferManagerCE";
        public const ushort DataVersion = 20;

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
                if (m_serializableData != null)
                {
                    byte[] Data = m_serializableData.LoadData(DataID);
                    if (Data != null && Data.Length > 0)
                    {
                        ushort SaveGameFileVersion;
                        int Index = 0;

                        SaveGameFileVersion = StorageData.ReadUInt16(Data, ref Index);
#if DEBUG
                        Debug.Log("Data length: " + Data.Length.ToString() + "; Data Version: " + SaveGameFileVersion);
#endif
                        if (SaveGameFileVersion <= DataVersion)
                        {
                            CheckStartTuple(SaveGameFileVersion, Data, ref Index);
                            SaveGameSettings.LoadData(SaveGameFileVersion, Data, ref Index);
                            CheckEndTuple(SaveGameFileVersion, Data, ref Index);

                            CheckStartTuple(SaveGameFileVersion, Data, ref Index);
                            BuildingSettingsSerializer.LoadData(SaveGameFileVersion, Data, ref Index);
                            CheckEndTuple(SaveGameFileVersion, Data, ref Index);

                            CheckStartTuple(SaveGameFileVersion, Data, ref Index);
                            OutsideConnectionSettings.LoadData(SaveGameFileVersion, Data, ref Index);
                            CheckEndTuple(SaveGameFileVersion, Data, ref Index);
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
                Prompt.ErrorFormat("Transfer Manager CE", sErrorMessage);
            }
        }

        public void OnSaveData()
        {
            Debug.Log("OnSaveData - Start");
            try
            {
                if (m_serializableData != null)
                {
                    FastList<byte> Data = new FastList<byte>();
                    // Always write out data version first
                    StorageData.WriteUInt16(DataVersion, Data);

                    StorageData.WriteUInt32(uiTUPLE_START, Data);
                    SaveGameSettings.SaveData(Data);
                    StorageData.WriteUInt32(uiTUPLE_END, Data);

                    StorageData.WriteUInt32(uiTUPLE_START, Data);
                    BuildingSettingsSerializer.SaveData(Data);
                    StorageData.WriteUInt32(uiTUPLE_END, Data);

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
            Debug.Log("OnSaveData - Finish");
        }

        private void CheckStartTuple(int iDataVersion, byte[] Data, ref int iIndex)
        {
            if (iDataVersion >= 17)
            {
                uint iTupleStart = StorageData.ReadUInt32(Data, ref iIndex);
                if (iTupleStart != uiTUPLE_START)
                {
                    throw new Exception("Start tuple not found.");
                }
            }
        }

        private void CheckEndTuple(int iDataVersion, byte[] Data, ref int iIndex)
        {
            if (iDataVersion >= 17)
            {
                uint iTupleStart = StorageData.ReadUInt32(Data, ref iIndex);
                if (iTupleStart != uiTUPLE_END)
                {
                    throw new Exception("End tuple not found.");
                }
            }
        }

        public void OnReleased()
        {
            Serializer.instance = (Serializer)null;
        }
    }
}