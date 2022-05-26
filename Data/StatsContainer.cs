using ColossalFramework.UI;
using SleepyCommon;
using System.Collections.Generic;
using static TransferManager;

namespace TransferManagerCE
{
    public class StatsContainer : ListData
    {
        public StatData m_stats;
        public TransferReason m_material;

        public StatsContainer()
        {
            m_stats = new StatData();
            m_material = 0;
        }

        public StatsContainer(TransferReason material)
        {
            m_stats = new StatData();
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
                case ListViewRowComparer.Columns.COLUMN_IN_COUNT: return m_stats.TotalIncomingCount.ToString();
                case ListViewRowComparer.Columns.COLUMN_IN_AMOUNT: return m_stats.TotalIncomingAmount.ToString();
                case ListViewRowComparer.Columns.COLUMN_OUT_COUNT: return m_stats.TotalOutgoingCount.ToString();
                case ListViewRowComparer.Columns.COLUMN_OUT_AMOUNT: return m_stats.TotalOutgoingAmount.ToString();
                case ListViewRowComparer.Columns.COLUMN_MATCH_COUNT: return m_stats.TotalMatches.ToString();
                case ListViewRowComparer.Columns.COLUMN_MATCH_AMOUNT: return m_stats.TotalMatchAmount.ToString();
                case ListViewRowComparer.Columns.COLUMN_IN_PERCENT:
                    {
                        return (SafeDiv(m_stats.TotalMatchAmount, m_stats.TotalIncomingAmount) * 100f).ToString("0.00");
                    }
                case ListViewRowComparer.Columns.COLUMN_OUT_PERCENT:
                    {
                        return (SafeDiv(m_stats.TotalMatchAmount, m_stats.TotalOutgoingAmount) * 100f).ToString("0.00");
                    }
            }
            return "TBD";
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
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_OUT_PERCENT, GetText(ListViewRowComparer.Columns.COLUMN_DESCRIPTION), "", StatsPanel.iCOLUMN_WIDTH, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_IN_PERCENT, GetText(ListViewRowComparer.Columns.COLUMN_DESCRIPTION), "", StatsPanel.iCOLUMN_WIDTH, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight);
        }
    }
}