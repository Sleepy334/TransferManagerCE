using System.Collections.Generic;
using static ColossalFramework.Packaging.Package;

namespace TransferManagerCE.Settings
{
    public class BuildingRestrictionSettings
    {
        private HashSet<ushort> m_buildingsAllowed;

        public BuildingRestrictionSettings()
        {
            m_buildingsAllowed = new HashSet<ushort>();
        }

        public BuildingRestrictionSettings(BuildingRestrictionSettings oSecond)
        {
            m_buildingsAllowed = new HashSet<ushort>(oSecond.m_buildingsAllowed);
        }

        public int Count
        {
            get {  return m_buildingsAllowed.Count; }
        }

        public bool Equals(BuildingRestrictionSettings oSecond)
        {
            return m_buildingsAllowed.SetEquals(oSecond.m_buildingsAllowed);
        }

        public bool HasBuildingRestrictions()
        {
            return m_buildingsAllowed.Count > 0;
        }

        public HashSet<ushort> GetBuildingRestrictionsCopy()
        {
            return new HashSet<ushort>(m_buildingsAllowed);
        }

        public void SetBuildingRestrictions(HashSet<ushort> allowedBuildings)
        {
            m_buildingsAllowed = allowedBuildings;
        }

        public void ClearBuildingRestrictions()
        {
            m_buildingsAllowed.Clear();
        }

        public void Write(FastList<byte> Data)
        {
            StorageData.WriteInt32(m_buildingsAllowed.Count, Data);
            foreach (ushort buildingId in m_buildingsAllowed)
            {
                StorageData.WriteUInt16(buildingId, Data);
            }
        }

        public void Read(byte[] Data, ref int iIndex)
        {
            m_buildingsAllowed.Clear();

            if (iIndex < Data.Length)
            {
                int iCount = StorageData.ReadInt32(Data, ref iIndex);
                for (int i = 0; i < iCount; ++i)
                {
                    ushort buildingId = StorageData.ReadUInt16(Data, ref iIndex);
                    m_buildingsAllowed.Add(buildingId);
                }
            }
        }
    }
}
