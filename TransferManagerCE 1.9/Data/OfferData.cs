using ColossalFramework.UI;
using System;
using static TransferManager;

namespace TransferManagerCE
{
    public class OfferData : IComparable
    {
        public TransferReason m_material = TransferReason.None;
        public TransferOffer m_offer;
        public bool m_bIncoming;
        public int m_iPrioirty;
        public bool m_bActive;

        public OfferData(TransferReason material, bool bIncoming, TransferOffer offer)
        {
            m_material = material;
            m_bIncoming = bIncoming;
            m_offer = offer;
            m_iPrioirty = offer.Priority;
            m_bActive = offer.Active;
        }

        public static int CompareTo(OfferData first, OfferData second)
        {
            // Descending priority
            return second.m_iPrioirty - first.m_iPrioirty;
        }

        public int CompareTo(object second)
        {
            if (second == null) {
                return 1;
            }
            OfferData oSecond = (OfferData)second;
            return CompareTo(this, oSecond);
        }

        public string DisplayOffer()
        {
            //string sMessage = "";
            if (m_offer.Building > 0 && m_offer.Building < BuildingManager.instance.m_buildings.m_size)
            {
                return CitiesUtils.GetBuildingName(m_offer.Building);
            }
            if (m_offer.Vehicle > 0 && m_offer.Vehicle < VehicleManager.instance.m_vehicles.m_size)
            {
                return CitiesUtils.GetVehicleName(m_offer.Vehicle);
            }
            if (m_offer.Citizen > 0)
            {
                Citizen oCitizen = CitizenManager.instance.m_citizens.m_buffer[m_offer.Citizen];
                ushort usBuildingId = oCitizen.GetBuildingByLocation();
                if (usBuildingId != 0)
                {
                    return CitiesUtils.GetCitizenName(m_offer.Citizen) + "@" + CitiesUtils.GetBuildingName(usBuildingId);
                }
                else
                {
                    return CitiesUtils.GetCitizenName(m_offer.Citizen);
                }
            }
            return "Unknown";
        }

        public void Show()
        {
            if (m_offer.Building > 0 && m_offer.Building < BuildingManager.instance.m_buildings.m_size)
            {
                CitiesUtils.ShowBuilding(m_offer.Building);
            }
            else if (m_offer.Citizen > 0)
            {
                CitiesUtils.ShowCitizen(m_offer.Citizen);
            }
            else if (m_offer.Vehicle > 0)
            {
                CitiesUtils.ShowVehicle(m_offer.Vehicle);
            }
        }
    }
}

