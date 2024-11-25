using ColossalFramework;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE.CustomManager
{
    public class MatchOffer
    {
        public InstanceID m_object;
        public uint m_flags; // Stores lots of information with bitmasks
        public byte m_byLocalPark; // Stores which park the offer is from in P&P parks.

        public MatchOffer()
        {
            m_object = new InstanceID();
            m_flags = 0;
            m_byLocalPark = 0;
        }

        public MatchOffer(MatchOffer offer)
        {
            m_object = new InstanceID();
            m_object.RawData = offer.m_object.RawData;
            m_flags = offer.m_flags;
            m_byLocalPark = offer.m_byLocalPark;
        }

        public MatchOffer(TransferOffer offer)
        {
            m_object = new InstanceID();
            m_object.RawData = offer.m_object.RawData;
            m_flags = offer.m_flags;
            m_byLocalPark = offer.m_isLocalPark;
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

        public List<ushort> GetBuildings()
        {
            if (m_object != null)
            {
                return InstanceHelper.GetBuildings(m_object);
            }

            return new List<ushort>();
        }

        public string DisplayOffer()
        {
            string sMessage = "";
            if (m_object != null)
            {
                switch (m_object.Type)
                {
                    case InstanceType.Park:
                    case InstanceType.Building:
                        {
                            sMessage = InstanceHelper.DescribeInstance(m_object);
                            break;
                        }
                    default:
                        {
                            sMessage = InstanceHelper.DescribeInstance(m_object);
                            List<ushort> buildings = GetBuildings();
                            if (buildings.Count > 0)
                            {
                                if (sMessage.Length > 0)
                                {
                                    sMessage += "@";
                                }
                                sMessage += CitiesUtils.GetBuildingName(buildings[0]);
                            }

                            return sMessage;
                        }
                }
            }
            return sMessage;
        }

        public void Show()
        {
            InstanceHelper.ShowInstance(m_object);
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(m_object.RawData);
            writer.Write(m_flags);
            writer.Write(m_byLocalPark);
        }

        public void Read(BinaryReader reader)
        {
            m_object = new InstanceID();
            m_object.RawData = reader.ReadUInt32();
            m_flags = reader.ReadUInt32();
            m_byLocalPark = reader.ReadByte();
        }
    }
}
