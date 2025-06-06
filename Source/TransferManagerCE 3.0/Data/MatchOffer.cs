using SleepyCommon;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE.CustomManager
{
    public class MatchOffer
    {
        public InstanceID m_object;
        public uint m_flags; // Stores lots of information with bitmasks
        public byte m_byLocalPark; // Stores which park the offer is from in P&P parks.
        public HashSet<ushort> m_buildings;

        public MatchOffer()
        {
            m_object = new InstanceID();
            m_flags = 0;
            m_byLocalPark = 0;
            m_buildings = new HashSet<ushort>();
        }

        public MatchOffer(MatchOffer offer)
        {
            m_object = new InstanceID();
            m_object.RawData = offer.m_object.RawData;
            m_flags = offer.m_flags;
            m_byLocalPark = offer.m_byLocalPark;
            m_buildings = new HashSet<ushort>(offer.m_buildings);
        }

        public MatchOffer(TransferOffer offer)
        {
            m_object = new InstanceID();
            m_object.RawData = offer.m_object.RawData;
            m_flags = offer.m_flags;
            m_byLocalPark = offer.m_isLocalPark;
            m_buildings = InstanceHelper.GetBuildings(offer.m_object);
        }

        public bool Active
        {
            get { return (m_flags & 1) != 0; }
        }

        public bool Exclude
        {
            get
            {
                return (m_flags & 4) != 0;
            }
        }

        public int Priority
        {
            get { return (int)((m_flags & 0xF0) >> 4); }
        }
        
        public int Amount
        {
            get { return (int)((m_flags & 0xFF00) >> 8); }
        }

        public bool Unlimited
        {
            get
            {
                return (m_flags & 8) != 0;
            }
        }

        public Vector3 Position
        {
            get
            {
                Vector3 result = default(Vector3);
                if ((m_flags & 2) != 0)
                {
                    result.x = ((float)((m_flags & 0xFF0000) >> 16) - 127.5f) * 270f;
                    result.y = 0f;
                    result.z = ((float)((uint)((int)m_flags & -16777216) >> 24) - 127.5f) * 270f;
                }
                else
                {
                    result.x = ((float)((m_flags & 0xFF0000) >> 16) - 127.5f) * 37.5f;
                    result.y = 0f;
                    result.z = ((float)((uint)((int)m_flags & -16777216) >> 24) - 127.5f) * 37.5f;
                }

                return result;
            }
        }

        public ushort Building
        {
            get { return m_object.Building; }
            set { m_object.Building = value; }
        }

        public ushort Vehicle
        {
            get { return m_object.Vehicle; }
            set { m_object.Vehicle = value; }
        }

        public uint Citizen
        {
            get { return m_object.Citizen; }
            set { m_object.Citizen = value; }
        }

        public ushort NetSegment
        {
            get { return m_object.NetSegment; }
            set { m_object.NetSegment = value; }
        }

        public ushort TransportLine
        {
            get { return m_object.TransportLine; }
            set { m_object.TransportLine = value; }
        }

        public bool IsWarehouse()
        {
            return Exclude; // Only set by warehouses.
        }

        public HashSet<ushort> GetBuildings()
        {
            return m_buildings;
        }

        public string DescribeOfferObject(bool bShowId = false)
        {
            string sMessage = "";
            switch (m_object.Type)
            {
                case InstanceType.Park:
                case InstanceType.Building:
                    {
                        sMessage = InstanceHelper.DescribeInstance(m_object, InstanceID.Empty, bShowId);
                        break;
                    }
                default:
                    {
                        sMessage = InstanceHelper.DescribeInstance(m_object, InstanceID.Empty, bShowId);

                        // Add building name if not a building.
                        if (m_buildings.Count > 0)
                        {
                            string sBuildingName = CitiesUtils.GetBuildingName(m_buildings.First(), InstanceID.Empty);
                            if (!string.IsNullOrEmpty(sBuildingName))
                            {
                                if (sMessage.Length > 0)
                                {
                                    sMessage += "@";
                                }
                                sMessage += sBuildingName;
                            }
                        }

                        return sMessage;
                    }
            }
            return sMessage;
        }

        public string DescribeActive()
        {
            if (Active)
            {
                return "Active";
            }
            else
            {
                return "Passive";
            }
        }

        public string DescribePark()
        {
            if (m_byLocalPark != 0)
            {
                return InstanceHelper.DescribeInstance(new InstanceID { Park = m_byLocalPark }, InstanceID.Empty, true);
            }
            else
            {
                return "";
            }
        }

        public string DescribeAmount()
        {
            return $"{Amount}{(Unlimited ? "*" : "")}";
        }

        public void Show()
        {
            Vector3 position = InstanceHelper.GetPosition(m_object);
            if (position != Vector3.zero)
            {
                InstanceHelper.ShowInstance(m_object);
            }
            else if (m_buildings.Count > 0)
            {
                InstanceHelper.ShowInstance(new InstanceID { Building = m_buildings.First() });
            }
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(m_object.RawData);
            writer.Write(m_flags);
            writer.Write(m_byLocalPark);

            // We always write out 5 buildingId's
            int iWritten = 0;
            ushort[] buildingIds = m_buildings.ToArray();
            while (iWritten < 5)
            {
                if (iWritten < buildingIds.Length)
                {
                    writer.Write(buildingIds[iWritten]);
                }
                else
                {
                    writer.Write((ushort)0);
                }
                iWritten++;
            }
        }

        public void Read(BinaryReader reader)
        {
            m_object = new InstanceID();

            m_object.RawData = reader.ReadUInt32();
            m_flags = reader.ReadUInt32();
            m_byLocalPark = reader.ReadByte();

            // We always write out 5 buildingId's
            m_buildings.Clear();
            int iRead = 0;
            while (iRead < 5)
            {
                ushort buildingId = reader.ReadUInt16();
                if (buildingId != 0)
                {
                    m_buildings.Add(buildingId);
                }
                iRead++;
            }
        }
    }
}
