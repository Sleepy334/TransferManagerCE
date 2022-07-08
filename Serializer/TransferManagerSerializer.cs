using ICities;
using System;

namespace TransferManagerCE
{
    public class Serializer : ISerializableDataExtension
    {
        public const string DataID = "TransferManagerCE";
        public const ushort DataVersion = 4;

        public static Serializer instance;
        private ISerializableData m_serializableData;

        public void OnCreated(ISerializableData serializedData)
        {
            instance = this;
            m_serializableData = serializedData;
        }

        public void OnLoadData()
        {
            try
            {
                byte[] Data = m_serializableData.LoadData(DataID);
                if (Data != null && Data.Length > 0)
                {
                    ushort SaveFileVersion;
                    int Index = 0;

                    SaveFileVersion = StorageData.ReadUInt16(Data, ref Index);  
#if DEBUG
                    Debug.Log("Data length: " + Data.Length.ToString() + "; Data Version: " + SaveFileVersion);
#endif
                    if (SaveFileVersion <= DataVersion)
                    {
                        BuildingSettings.LoadData(SaveFileVersion, Data, ref Index);
                    }
                    else
                    {
                        string sMessage = "This saved game was saved with a newer version of Transfer Manager CE.\r\n";
                        sMessage += "\r\n";
                        sMessage += "Unable to load Transfer Manager settings.\r\n";
                        sMessage += "\r\n";
                        sMessage += "Saved game data version: " + SaveFileVersion + "\r\n";
                        sMessage += "MOD data version: " + DataVersion + "\r\n";
                        Prompt.ErrorFormat("Transfer Manager CE", sMessage);
                    }
                } 
                else
                {
                    Debug.Log("Data is null");
                }
            }
            catch (Exception ex)
            {
                Prompt.ErrorFormat("Transfer Manager CE", "Loading of Transfer Manager saved game settings failed " + ex.Message);
            }
            //Debug.Log("OnLoadData - Finish");
        }

        public void OnSaveData()
        {
            Debug.Log("OnSaveData - Start");
            try
            {
                FastList<byte> Data = new FastList<byte>();
                StorageData.WriteUInt16(DataVersion, Data);
                BuildingSettings.SaveData(Data);
                m_serializableData.SaveData(DataID, Data.ToArray());
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