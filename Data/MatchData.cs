using ColossalFramework.UI;
using SleepyCommon;
using System;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE
{
    public class MatchData : ListData
    {
        public TransferReason m_material = TransferReason.None;
        public TransferOffer m_incoming;
        public TransferOffer m_outgoing;
        public int m_iDeltaAmount = 0;
        public DateTime m_TimeStamp;
        public bool m_bIncoming = true;

        public MatchData(TransferReason material, bool bIncoming, TransferOffer outgoing, TransferOffer incoming, int iDeltaAmount)
        {
            m_material = material;
            m_incoming = incoming;
            m_outgoing = outgoing;
            m_iDeltaAmount = iDeltaAmount;
            m_bIncoming = bIncoming;
            m_TimeStamp = DateTime.Now;
        }

        public override string ToString()
        {
            string sMatchText = "MATCH " + m_material + " Amount: " + m_incoming.Amount + "/" + m_outgoing.Amount;
            return sMatchText;
        }

        public override int CompareTo(object second)
        {
            if (second == null)
            {
                return 1;
            }
            MatchData oSecond = (MatchData)second;
            return oSecond.m_TimeStamp.CompareTo(m_TimeStamp);
        }

        public override string GetText(ListViewRowComparer.Columns eColumn)
        {
            switch (eColumn)
            {
                case ListViewRowComparer.Columns.COLUMN_TIME: return m_TimeStamp.ToString("h:mm:ss");
                case ListViewRowComparer.Columns.COLUMN_MATERIAL: return m_material.ToString();
                case ListViewRowComparer.Columns.COLUMN_AMOUNT: return m_incoming.Amount + "/" + m_outgoing.Amount;
                case ListViewRowComparer.Columns.COLUMN_DELTAAMOUNT: return m_iDeltaAmount.ToString();
                case ListViewRowComparer.Columns.COLUMN_DESCRIPTION: return DisplayMatch();
            }
            return "";
        }

        public override void CreateColumns(ListViewRow oRow, System.Collections.Generic.List<ListViewRowColumn> m_columns)
        {
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_TIME, GetText(ListViewRowComparer.Columns.COLUMN_TIME), "", TransferBuildingPanel.iCOLUMN_WIDTH, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, GetText(ListViewRowComparer.Columns.COLUMN_MATERIAL), "", TransferBuildingPanel.iCOLUMN_MATERIAL_WIDTH, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_AMOUNT, GetText(ListViewRowComparer.Columns.COLUMN_AMOUNT), "", TransferBuildingPanel.iCOLUMN_WIDTH, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_DELTAAMOUNT, GetText(ListViewRowComparer.Columns.COLUMN_DELTAAMOUNT), "", TransferBuildingPanel.iCOLUMN_WIDTH, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_DESCRIPTION, GetText(ListViewRowComparer.Columns.COLUMN_DESCRIPTION), "", TransferBuildingPanel.iCOLUMN_DESCRIPTION_WIDTH, UIHorizontalAlignment.Left, UIAlignAnchor.TopRight);
        }

        public string DisplayOffer(TransferOffer offer)
        {
            if (offer.Building != 0)
            {
                return CitiesUtils.GetBuildingName(offer.Building);
            } 
            else if (offer.Vehicle > 0 && offer.Vehicle < VehicleManager.instance.m_vehicles.m_size)
            {
                return CitiesUtils.GetVehicleName(offer.Vehicle);
            }
            else if (offer.Citizen > 0)
            {
                Citizen oCitizen = CitizenManager.instance.m_citizens.m_buffer[offer.Citizen];
                ushort usBuildingId = oCitizen.GetBuildingByLocation();
                if (usBuildingId != 0)
                {
                    return CitiesUtils.GetCitizenName(offer.Citizen) + "@" + CitiesUtils.GetBuildingName(usBuildingId);
                }
                else
                {
                    return CitiesUtils.GetCitizenName(offer.Citizen);
                }
            }

            return "Unknown";
        }

        public string DisplayMatch()
        {
            if (m_bIncoming)
            {
                return DisplayOffer(m_outgoing);
            }
            else
            {
                return DisplayOffer(m_incoming);
            }
        }

        public override void OnClick(ListViewRowColumn column)
        {
            TransferOffer offer;
            if (m_bIncoming)
            {
                offer = m_outgoing;
            } else
            {
                offer = m_incoming;
            }
                
            if (offer.Building > 0 && offer.Building < BuildingManager.instance.m_buildings.m_size)
            {
                CitiesUtils.ShowBuilding(offer.Building);
            } 
            else if (offer.Citizen > 0)
            {
                CitiesUtils.ShowCitizen(offer.Citizen);
            } 
            else if (offer.Vehicle > 0)
            {
                CitiesUtils.ShowVehicle(offer.Vehicle);
            }
        }
    }
}