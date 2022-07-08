using ColossalFramework.UI;
using SleepyCommon;
using System;
using System.Collections.Generic;
using static TransferManager;

namespace TransferManagerCE
{
    public class StatsContainer : ListData
    {
        public int TotalMatches = 0;
        public int TotalMatchAmount = 0;
        public int TotalIncomingCount = 0;
        public int TotalIncomingAmount = 0;
        public int TotalOutgoingCount = 0;
        public int TotalOutgoingAmount = 0;
        public double TotalDistance = 0;
        public TransferReason m_material;

        public StatsContainer()
        {
            TotalMatches = 0;
            TotalMatchAmount = 0;
            TotalIncomingCount = 0;
            TotalIncomingAmount = 0;
            TotalOutgoingCount = 0;
            TotalOutgoingAmount = 0;
            TotalDistance = 0;
            m_material = 0;
        }

        public StatsContainer(TransferReason material)
        {
            TotalMatches = 0;
            TotalMatchAmount = 0;
            TotalIncomingCount = 0;
            TotalIncomingAmount = 0;
            TotalOutgoingCount = 0;
            TotalOutgoingAmount = 0;
            TotalDistance = 0;
            m_material = material;
        }

        public override int CompareTo(object second)
        {
            return 1;
        }

        public float SafeDiv(float fNumerator, float fDenominator)
        {
            if (fNumerator == 0 || fDenominator == 0)
            {
                return 0f;
            }
            else
            {
                return fNumerator / fDenominator;
            }
        }

        public string GetMaterialDescription()
        {
            if (m_material == TransferReason.None)
            {
                return "Total";
            }
            else
            {
                return m_material.ToString();
            }
        }

        public override string GetText(ListViewRowComparer.Columns eColumn)
        {
            switch (eColumn)
            {
                case ListViewRowComparer.Columns.COLUMN_MATERIAL: return GetMaterialDescription();
                case ListViewRowComparer.Columns.COLUMN_IN_COUNT: return TotalIncomingCount.ToString();
                case ListViewRowComparer.Columns.COLUMN_IN_AMOUNT: return TotalIncomingAmount.ToString();
                case ListViewRowComparer.Columns.COLUMN_OUT_COUNT: return TotalOutgoingCount.ToString();
                case ListViewRowComparer.Columns.COLUMN_OUT_AMOUNT: return TotalOutgoingAmount.ToString();
                case ListViewRowComparer.Columns.COLUMN_MATCH_COUNT: return TotalMatches.ToString();
                case ListViewRowComparer.Columns.COLUMN_MATCH_AMOUNT: return TotalMatchAmount.ToString();
                case ListViewRowComparer.Columns.COLUMN_MATCH_DISTANCE:
                    {
                        if (TotalMatches == 0)
                        {
                            return "0";
                        }
                        else
                        {
                            return ((TotalDistance / (double) TotalMatches) * 0.001).ToString("0.00");
                        }
                        
                    }
                case ListViewRowComparer.Columns.COLUMN_IN_PERCENT:
                    {
                        return (SafeDiv(TotalMatchAmount, TotalIncomingAmount) * 100f).ToString("0.00");
                    }
                case ListViewRowComparer.Columns.COLUMN_OUT_PERCENT:
                    {
                        return (SafeDiv(TotalMatchAmount, TotalOutgoingAmount) * 100f).ToString("0.00");
                    }
            }
            
            return "";
        }

        public override void CreateColumns(ListViewRow oRow, List<ListViewRowColumn> m_columns)
        {
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, GetText(ListViewRowComparer.Columns.COLUMN_MATERIAL), "", StatsPanel.iCOLUMN_MATERIAL_WIDTH, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_OUT_COUNT, GetText(ListViewRowComparer.Columns.COLUMN_PRIORITY), "", StatsPanel.iCOLUMN_WIDTH, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_OUT_AMOUNT, GetText(ListViewRowComparer.Columns.COLUMN_AMOUNT), "", StatsPanel.iCOLUMN_BIGGER_WIDTH, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_IN_COUNT, GetText(ListViewRowComparer.Columns.COLUMN_INOUT), "", StatsPanel.iCOLUMN_WIDTH, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_IN_AMOUNT, GetText(ListViewRowComparer.Columns.COLUMN_MATERIAL), "", StatsPanel.iCOLUMN_WIDTH, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_MATCH_COUNT, GetText(ListViewRowComparer.Columns.COLUMN_DESCRIPTION), "", StatsPanel.iCOLUMN_WIDTH, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_MATCH_AMOUNT, GetText(ListViewRowComparer.Columns.COLUMN_DESCRIPTION), "", StatsPanel.iCOLUMN_BIGGER_WIDTH, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_MATCH_DISTANCE, GetText(ListViewRowComparer.Columns.COLUMN_MATCH_DISTANCE), "", StatsPanel.iCOLUMN_WIDTH, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_OUT_PERCENT, GetText(ListViewRowComparer.Columns.COLUMN_DESCRIPTION), "", StatsPanel.iCOLUMN_WIDTH, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_IN_PERCENT, GetText(ListViewRowComparer.Columns.COLUMN_DESCRIPTION), "", StatsPanel.iCOLUMN_WIDTH, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight);
        }
    }
}