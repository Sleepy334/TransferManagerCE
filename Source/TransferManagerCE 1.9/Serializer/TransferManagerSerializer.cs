using ICities;
using System;
using TransferManagerCE.Settings;

namespace TransferManagerCE
{
    public class Serializer : ISerializableDataExtension
    {
        public const string DataID = "TransferManagerCE";
        public const ushort DataVersion = 16;

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
                            SaveGameSettings.LoadData(SaveGameFileVersion, Data, ref Index);
                            BuildingSettingsSerializer.LoadData(SaveGameFileVersion, Data, ref Index);
                            OutsideConnectionSettings.LoadData(SaveGameFileVersion, Data, ref Index);
                        }
                        else
                        {
                            string sMessage = "This saved game was saved with a newer version of Transfer Manager CE.\r\n";
                            sMessage += "\r\n";
                            sMessage += "Unable to load Transfer Manager settings.\r\n";
                            sMessage += "\r\n";
                            sMessage += "Saved game data version: " + SaveGameFileVersion + "\r\n";
                            sMessage += "MOD data version: " + DataVersion + "\r\n";
                            Prompt.ErrorFormat("Transfer Manager CE", sMessage);
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
                Prompt.ErrorFormat("Transfer Manager CE", "Loading of Transfer Manager saved game settings failed " + ex.Message);
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
                    StorageData.WriteUInt16(DataVersion, Data);
                    SaveGameSettings.SaveData(Data);
                    BuildingSettingsSerializer.SaveData(Data);
                    OutsideConnectionSettings.SaveData(Data);
                    m_serializableData.SaveData(DataID, Data.ToArray());
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Could not save data. " + ex.Message);
            }
            Debug.Log("OnSaveData - Finish");
        }

        public void OnReleased()
        {
            Serializer.instance = (Serializer)null;
        }
    }
}