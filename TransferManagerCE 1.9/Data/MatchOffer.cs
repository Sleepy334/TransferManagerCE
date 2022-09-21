using ColossalFramework;
using System.Collections.Generic;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE.CustomManager
{
    public class MatchOffer
    {
        public InstanceID m_object;
        public int Priority;
        public int Amount;
        public bool Active;
        public bool Exclude;
        public Vector3 Position;
        public byte m_byLocalPark;

        public MatchOffer(MatchOffer offer)
        {
            m_object = new InstanceID();
            m_object.RawData = offer.m_object.RawData;
            Priority = offer.Priority;
            Amount = offer.Amount;
            Active = offer.Active;
            Exclude = offer.Exclude;
            Position = new Vector3(offer.Position.x, offer.Position.y, offer.Position.z);
            m_byLocalPark = offer.m_byLocalPark;
        }

        public MatchOffer(TransferOffer offer)
        {
            m_object = new InstanceID();
            m_object.RawData = offer.m_object.RawData;
            Priority = offer.Priority;
            Amount = offer.Amount;
            Active = offer.Active;
            Exclude = offer.Exclude;
            Position = new Vector3(offer.Position.x, offer.Position.y, offer.Position.z);
            m_byLocalPark = offer.m_isLocalPark;
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
                if (m_object.Type != InstanceType.Building)
                {
                    sMessage = InstanceHelper.DescribeInstance(m_object);
                }
            }
            else
            {
                sMessage = "m_object is null";
            }

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

        public void Show()
        {
            InstanceHelper.ShowInstance(m_object);
        }
    }
}
