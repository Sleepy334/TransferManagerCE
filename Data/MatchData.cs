using ColossalFramework.UI;
using SleepyCommon;
using System;
using TransferManagerCE.CustomManager;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE
{
    public class MatchData : ListData
    {
        public TransferReason m_material = TransferReason.None;
        public TransferOffer m_incoming;
        public TransferOffer m_outgoing;
        public int m_inBuildingId = 0;
        public int m_outBuildingId = 0;
        public int m_iDeltaAmount = 0;
        public DateTime m_TimeStamp;

        private int m_buildingId = 0;

        public MatchData(TransferReason material, TransferOffer outgoing, TransferOffer incoming, int iDeltaAmount)
        {
            m_buildingId = 0;
            m_material = material;
            m_incoming = GetCopy(incoming);
            m_outgoing = GetCopy(outgoing);
            m_inBuildingId = TransferManagerUtils.GetOfferBuilding(ref incoming);
            m_outBuildingId = TransferManagerUtils.GetOfferBuilding(ref outgoing);
            m_iDeltaAmount = iDeltaAmount;
            m_TimeStamp = DateTime.Now;
        }

        public MatchData(ushort buildingId, MatchData second)
        {
            m_buildingId = buildingId;
            m_material = second.m_material;
            m_incoming = GetCopy(second.m_incoming);
            m_outgoing = GetCopy(second.m_outgoing);
            m_inBuildingId = second.m_inBuildingId;
            m_outBuildingId = second.m_outBuildingId;
            m_iDeltaAmount = second.m_iDeltaAmount;
            m_TimeStamp = second.m_TimeStamp;
        }

        public static TransferOffer GetCopy(TransferOffer offer)
        {
            TransferOffer newOffer = new TransferOffer();
            newOffer.m_object = offer.m_object;
            newOffer.Active = offer.Active;
            newOffer.Priority = offer.Priority;
            newOffer.Amount = offer.Amount;
            newOffer.Position = offer.Position;
            newOffer.Exclude = offer.Exclude;
            return newOffer;
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
                case ListViewRowComparer.Columns.COLUMN_INOUT: return GetInOutStatus();
                case ListViewRowComparer.Columns.COLUMN_ACTIVE: return GetActiveStatus();
                case ListViewRowComparer.Columns.COLUMN_AMOUNT: return m_incoming.Amount + "/" + m_outgoing.Amount;
                case ListViewRowComparer.Columns.COLUMN_DISTANCE: return GetDistance().ToString("0.00");
                case ListViewRowComparer.Columns.COLUMN_PRIORITY: return m_incoming.Priority + "/" + m_outgoing.Priority;
                case ListViewRowComparer.Columns.COLUMN_DESCRIPTION: return DisplayMatch();
            }
            return "";
        }

        public override void CreateColumns(ListViewRow oRow, System.Collections.Generic.List<ListViewRowColumn> m_columns)
        {
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_TIME, GetText(ListViewRowComparer.Columns.COLUMN_TIME), "", TransferBuildingPanel.iCOLUMN_WIDTH_SMALL, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, GetText(ListViewRowComparer.Columns.COLUMN_MATERIAL), "", TransferBuildingPanel.iCOLUMN_WIDTH_LARGE, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_INOUT, GetText(ListViewRowComparer.Columns.COLUMN_INOUT), "", TransferBuildingPanel.iCOLUMN_WIDTH_SMALL, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_ACTIVE, GetText(ListViewRowComparer.Columns.COLUMN_ACTIVE), "", TransferBuildingPanel.iCOLUMN_WIDTH_SMALL, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_AMOUNT, GetText(ListViewRowComparer.Columns.COLUMN_AMOUNT), "", TransferBuildingPanel.iCOLUMN_WIDTH_SMALL, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_DISTANCE, GetText(ListViewRowComparer.Columns.COLUMN_DISTANCE), "", TransferBuildingPanel.iCOLUMN_WIDTH_SMALL, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_PRIORITY, GetText(ListViewRowComparer.Columns.COLUMN_PRIORITY), "", TransferBuildingPanel.iCOLUMN_WIDTH_XS, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_DESCRIPTION, GetText(ListViewRowComparer.Columns.COLUMN_DESCRIPTION), "", TransferBuildingPanel.iCOLUMN_WIDTH_XLARGE, UIHorizontalAlignment.Left, UIAlignAnchor.TopRight);
        }

        public string GetActiveStatus()
        {
            if (m_inBuildingId == m_buildingId)
            {
                return m_incoming.Active ? "Active" : "Passive";
            }
            else if (m_outBuildingId == m_buildingId)
            {
                return m_outgoing.Active ? "Active" : "Passive";
            }
            return "";
        }

        public string GetInOutStatus()
        {
            if (m_inBuildingId == m_buildingId)
            {
                return "IN";
            }
            else if (m_outBuildingId == m_buildingId)
            {
                return "OUT";
            }
            return "";
        }

        public string DisplayOffer(TransferOffer offer)
        {
            if (offer.m_object.Type == InstanceType.Building)
            {
                return CitiesUtils.GetBuildingName(offer.Building);
            } 
            else if (offer.m_object.Type == InstanceType.Vehicle)
            {
                return CitiesUtils.GetVehicleName(offer.Vehicle);
            }
            else if (offer.m_object.Type == InstanceType.Citizen)
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

            return offer.m_object.Type.ToString() + " " + offer.m_object.ToString();
        }

        public string DisplayMatch()
        {
            if (m_inBuildingId == m_buildingId)
            {
                return DisplayOffer(m_outgoing);
            }
            else if (m_outBuildingId == m_buildingId)
            {
                return DisplayOffer(m_incoming);
            }
            else
            {
                // Can't determine which one matched, just return generic 
                return "OUT: " + m_outgoing.m_object.Type.ToString() + " " + m_outgoing.m_object.ToString() + " IN: " + m_outgoing.m_object.Type.ToString() + m_incoming.m_object.ToString();
            }
        }

        public override void OnClick(ListViewRowColumn column)
        {
            TransferOffer? offer = null;
            if (m_inBuildingId == m_buildingId)
            {
                offer = m_outgoing;
            } else if (m_outBuildingId == m_buildingId)
            {
                offer = m_incoming;
            }
            
            if (offer != null)
            {
                if (offer.Value.m_object.Type == InstanceType.Building)
                {
                    CitiesUtils.ShowBuilding(offer.Value.Building);
                }
                else if (offer.Value.m_object.Type == InstanceType.Citizen)
                {
                    CitiesUtils.ShowCitizen(offer.Value.Citizen);
                }
                else if (offer.Value.m_object.Type == InstanceType.Vehicle)
                {
                    CitiesUtils.ShowVehicle(offer.Value.Vehicle);
                }
            }
        }

        private double GetDistance()
        {
            return Math.Sqrt(Vector3.SqrMagnitude(m_incoming.Position - m_outgoing.Position)) * 0.001;
        }
    }
}