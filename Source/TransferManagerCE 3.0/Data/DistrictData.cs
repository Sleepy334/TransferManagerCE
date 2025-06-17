using System;
using System.Collections.Generic;

namespace TransferManagerCE
{
    public class DistrictData : IEquatable<DistrictData>,  IComparable
    {
        public enum DistrictType
        {
            District = 1,
            Park = 2,
        }; 
        
        public DistrictType m_eType;
        public int m_iDistrictId;
        private string m_name = null;

        public static DistrictData Empty = new DistrictData(DistrictType.District, 0);

        // ----------------------------------------------------------------------------------------
        public DistrictData(DistrictType eType, int iDistrictId)
        {
            m_eType = eType;
            m_iDistrictId = iDistrictId;
        }

        public string GetDistrictName()
        {
            if (m_name is null)
            {
                if (m_eType == DistrictData.DistrictType.Park)
                {
                    DistrictPark district = DistrictManager.instance.m_parks.m_buffer[m_iDistrictId];
                    if (district.m_flags != 0)
                    {
                        m_name = DistrictManager.instance.GetParkName(m_iDistrictId);
                    }
                }
                else
                {
                    District district = DistrictManager.instance.m_districts.m_buffer[m_iDistrictId];
                    if (district.m_flags != 0)
                    {
                        m_name = DistrictManager.instance.GetDistrictName(m_iDistrictId);
                    }
                }
            }

            if (m_name is not null)
            {
                return m_name;
            }
            else
            {
                return "";
            }
        }

        public int CompareTo(object obj)
        {
            DistrictData oSecond = (DistrictData)obj;
            if (oSecond is null)
            {
                return 1;
            }

            return GetDistrictName().CompareTo(oSecond.GetDistrictName());
        }

        public bool Equals(DistrictData oSecond)
        {
            return m_eType == oSecond.m_eType &&
                    (m_iDistrictId == oSecond.m_iDistrictId);
        }

        public override bool Equals(object second)
        {
            if (second is null)
            {
                return false;
            }
            DistrictData oSecond = (DistrictData)second;
            return Equals(oSecond);
        }

        public override int GetHashCode()
        {
            return (m_eType == DistrictType.District) ? m_iDistrictId : -m_iDistrictId; // Parks use negative numbers so they don't conflict
        }

        public static bool Intersect(HashSet<DistrictData> oFirst, HashSet<DistrictData> oSecond)
        {
            if (oFirst.Count > 0 && oSecond.Count > 0)
            {
                foreach (DistrictData data in oFirst)
                {
                    if (oSecond.Contains(data))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsDistrict()
        {
            return m_eType == DistrictType.District;
        }
        public bool IsPark()
        {
            return m_eType == DistrictType.Park;
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