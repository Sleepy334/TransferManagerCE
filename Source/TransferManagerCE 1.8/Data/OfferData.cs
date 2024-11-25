using ColossalFramework.UI;
using SleepyCommon;
using System.Collections.Generic;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE
{
    public class OfferData : ListData
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

        public override int CompareTo(object second)
        {
            if (second == null) {
                return 1;
            }
            OfferData oSecond = (OfferData)second;
            return CompareTo(this, oSecond);
        }

        public override string GetText(ListViewRowComparer.Columns eColumn)
        {
            switch (eColumn)
            {
                case ListViewRowComparer.Columns.COLUMN_INOUT: return (m_bIncoming ? "IN" : "OUT");
                case ListViewRowComparer.Columns.COLUMN_MATERIAL: return m_material.ToString();
                case ListViewRowComparer.Columns.COLUMN_AMOUNT: return m_offer.Amount.ToString();
                case ListViewRowComparer.Columns.COLUMN_PRIORITY: return m_iPrioirty.ToString();
                case ListViewRowComparer.Columns.COLUMN_ACTIVE: return m_bActive ? "Active" : "Passive";
                case ListViewRowComparer.Columns.COLUMN_DESCRIPTION: return DisplayOffer();
            }
            return "";
        }

        public override void CreateColumns(ListViewRow oRow, List<ListViewRowColumn> m_columns)
        {
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_INOUT, GetText(ListViewRowComparer.Columns.COLUMN_INOUT), "", BuildingPanel.iCOLUMN_WIDTH_SMALL, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, GetText(ListViewRowComparer.Columns.COLUMN_MATERIAL), "", BuildingPanel.iCOLUMN_WIDTH_LARGE, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_PRIORITY, GetText(ListViewRowComparer.Columns.COLUMN_PRIORITY), "", BuildingPanel.iCOLUMN_WIDTH_NORMAL, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_ACTIVE, GetText(ListViewRowComparer.Columns.COLUMN_ACTIVE), "", BuildingPanel.iCOLUMN_WIDTH_NORMAL, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_AMOUNT, GetText(ListViewRowComparer.Columns.COLUMN_AMOUNT), "", BuildingPanel.iCOLUMN_WIDTH_NORMAL, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_DESCRIPTION, GetText(ListViewRowComparer.Columns.COLUMN_DESCRIPTION), "", BuildingPanel.iCOLUMN_WIDTH_XLARGE, UIHorizontalAlignment.Left, UIAlignAnchor.TopRight);
        }

        public string DisplayOffer()
        {
            //string sMessage = "";
            if (m_offer.Building > 0 && m_offer.Building < BuildingManager.instance.m_buildings.m_size)
            {
                return CitiesUtils.GetBuildingName(m_offer.Building);
                /*
                var instB = default(InstanceID);
                instB.Building = m_offer.Building;
                sMessage += " (" + m_offer.Building + ")" + BuildingManager.instance.m_buildings.m_buffer[m_offer.Building].Info?.name + "(" + InstanceManager.instance.GetName(instB) + ")";
                */
            }
            if (m_offer.Vehicle > 0 && m_offer.Vehicle < VehicleManager.instance.m_vehicles.m_size)
            {
                return CitiesUtils.GetVehicleName(m_offer.Vehicle);
                //sMessage += " (" + m_offer.Vehicle + ")" + VehicleManager.instance.m_vehicles.m_buffer[m_offer.Vehicle].Info?.name;
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

        public override void OnClick(ListViewRowColumn column)
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

