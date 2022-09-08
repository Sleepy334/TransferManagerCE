using ColossalFramework;
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

        public MatchOffer(MatchOffer offer)
        {
            m_object = new InstanceID();
            m_object.RawData = offer.m_object.RawData;
            Priority = offer.Priority;
            Amount = offer.Amount;
            Active = offer.Active;
            Exclude = offer.Exclude;
            Position = new Vector3(offer.Position.x, offer.Position.y, offer.Position.z);
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

        public ushort GetBuilding()
        {
            if (m_object != null)
            {
                return InstanceHelper.GetBuilding(m_object);
            }

            return 0;
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
            
            ushort buildingId = GetBuilding();
            if (buildingId > 0)
            {
                if (sMessage.Length > 0)
                {
                    sMessage += "@";
                }
                sMessage += CitiesUtils.GetBuildingName(buildingId);
            }
            return sMessage;
        }

        public void Show()
        {
            InstanceHelper.ShowInstance(m_object);
        }
    }
}
