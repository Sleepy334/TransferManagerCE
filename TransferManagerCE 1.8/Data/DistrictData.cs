using System;
using System.Collections.Generic;

namespace TransferManagerCE
{
    public class DistrictData : IEquatable<DistrictData>
    {
        public enum DistrictType
        {
            District = 1,
            Park = 2,
        }; 
        
        public DistrictType m_eType;
        public int m_iDistrictId;

        public DistrictData(DistrictType eType, int iDistrictId)
        {
            m_eType = eType;
            m_iDistrictId = iDistrictId;
        }

        public bool Equals(DistrictData oSecond)
        {
            return m_eType == oSecond.m_eType &&
                    (m_iDistrictId == oSecond.m_iDistrictId);
        }

        public static bool Intersect(List<DistrictData> oFirst, List<DistrictData> oSecond)
        {
            foreach (DistrictData data in oFirst)
            {
                if (oSecond.Contains(data))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsDistrict()
        {
            return m_eType == DistrictType.District;
        }

        public void SaveData(FastList<byte> Data)
        {
            StorageData.WriteInt32((int)m_eType, Data);
            StorageData.WriteInt32(m_iDistrictId, Data);
        }

        public static DistrictData LoadData(byte[] Data, ref int iIndex)
        {
            DistrictType eType = (DistrictType) StorageData.ReadInt32(Data, ref iIndex);
            int district = StorageData.ReadInt32(Data, ref iIndex);
            return new DistrictData(eType, district);
        }
    }
}