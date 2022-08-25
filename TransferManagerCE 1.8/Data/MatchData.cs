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
        public DateTime m_TimeStamp; 
        public TransferReason m_material = TransferReason.None;
        public MatchOffer m_incoming;
        public MatchOffer m_outgoing;
        private int m_buildingId = 0;

        public MatchData(TransferReason material, TransferOffer outgoing, TransferOffer incoming, int iDeltaAmount)
        {
            m_buildingId = 0;
            m_material = material;
            m_incoming = new MatchOffer(incoming);
            m_outgoing = new MatchOffer(outgoing);
            m_TimeStamp = DateTime.Now;
         }

        public MatchData(ushort buildingId, MatchData second)
        {
            m_buildingId = buildingId;
            m_material = second.m_material;
            m_incoming = new MatchOffer(second.m_incoming);
            m_outgoing = new MatchOffer(second.m_outgoing);
            m_TimeStamp = second.m_TimeStamp;
        }

        public static CustomTransferOffer GetCopy(CustomTransferOffer offer)
        {
            return new CustomTransferOffer(offer);
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
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_TIME, GetText(ListViewRowComparer.Columns.COLUMN_TIME), "", BuildingPanel.iCOLUMN_WIDTH_SMALL, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, GetText(ListViewRowComparer.Columns.COLUMN_MATERIAL), "", BuildingPanel.iCOLUMN_WIDTH_LARGE, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_INOUT, GetText(ListViewRowComparer.Columns.COLUMN_INOUT), "", BuildingPanel.iCOLUMN_WIDTH_SMALL, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_ACTIVE, GetText(ListViewRowComparer.Columns.COLUMN_ACTIVE), "", BuildingPanel.iCOLUMN_WIDTH_SMALL, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_AMOUNT, GetText(ListViewRowComparer.Columns.COLUMN_AMOUNT), "", BuildingPanel.iCOLUMN_WIDTH_SMALL, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_DISTANCE, GetText(ListViewRowComparer.Columns.COLUMN_DISTANCE), "", BuildingPanel.iCOLUMN_WIDTH_SMALL, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_PRIORITY, GetText(ListViewRowComparer.Columns.COLUMN_PRIORITY), "", BuildingPanel.iCOLUMN_WIDTH_SMALL, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_DESCRIPTION, GetText(ListViewRowComparer.Columns.COLUMN_DESCRIPTION), "", BuildingPanel.iCOLUMN_WIDTH_XLARGE, UIHorizontalAlignment.Left, UIAlignAnchor.TopRight);
        }

        public string GetActiveStatus()
        {
            if (m_incoming.GetBuilding() == m_buildingId)
            {
                return m_incoming.Active ? "Active" : "Passive";
            }
            else if (m_outgoing.GetBuilding() == m_buildingId)
            {
                return m_outgoing.Active ? "Active" : "Passive";
            }
            return "";
        }

        public string GetInOutStatus()
        {
            if (m_incoming.GetBuilding() == m_buildingId)
            {
                return "IN";
            }
            else if (m_outgoing.GetBuilding() == m_buildingId)
            {
                return "OUT";
            }
            return "";
        }

        public string DisplayMatch()
        {
            if (m_incoming.GetBuilding() == m_buildingId)
            {
                return m_outgoing.DisplayOffer();
            }
            else if (m_outgoing.GetBuilding() == m_buildingId)
            {
                return m_incoming.DisplayOffer();
            }
            else
            {
                // Can't determine which one matched, just return generic 
                return "OUT: " + m_outgoing.m_object.Type.ToString() + " " + m_outgoing.m_object.ToString() + " IN: " + m_outgoing.m_object.Type.ToString() + m_incoming.m_object.ToString();
            }
        }

        public override void OnClick(ListViewRowColumn column)
        {
            if (m_incoming.GetBuilding() == m_buildingId)
            {
                m_outgoing.Show();
            } 
            else if (m_outgoing.GetBuilding() == m_buildingId)
            {
                m_incoming.Show();
            }
        }

        private double GetDistance()
        {
            return Math.Sqrt(Vector3.SqrMagnitude(m_incoming.Position - m_outgoing.Position)) * 0.001;
        }
    }
}