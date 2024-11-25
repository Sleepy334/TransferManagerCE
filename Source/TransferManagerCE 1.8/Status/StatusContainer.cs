using ColossalFramework.UI;
using SleepyCommon;
using System.Collections.Generic;
using TransferManagerCE.Data;

namespace TransferManagerCE
{
    public class StatusContainer : ListData
    {
        public StatusData m_status;

        public StatusContainer(StatusData data)
        {
            m_status = data;
        }

        public override int CompareTo(object second)
        {
            if (second == null)
            {
                return 1;
            }
            StatusContainer oSecond = (StatusContainer)second;
            return m_status.GetMaterial().CompareTo(oSecond.m_status.GetMaterial());
        }

        public string GetMaterialDescription()
        {
            return m_status.GetMaterial().ToString();
        }

        public override void Update()
        {
            m_status.Update();
        }

        public override string GetText(ListViewRowComparer.Columns eColumn)
        {
            if (m_status != null)
            {
                switch (eColumn)
                {
                    case ListViewRowComparer.Columns.COLUMN_MATERIAL: return GetMaterialDescription();
                    case ListViewRowComparer.Columns.COLUMN_VALUE: return m_status.GetValue();
                    case ListViewRowComparer.Columns.COLUMN_OWNER: return m_status.GetResponder();
                    case ListViewRowComparer.Columns.COLUMN_TARGET: return m_status.GetTarget();
                    case ListViewRowComparer.Columns.COLUMN_LOAD: return m_status.GetLoad();
                    case ListViewRowComparer.Columns.COLUMN_TIMER: return m_status.GetTimer();
                }
            } 
            else
            {
                Debug.Log("m_status is null");
            }
            
            return "";
        }

        public override void CreateColumns(ListViewRow oRow, List<ListViewRowColumn> m_columns)
        {
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, GetText(ListViewRowComparer.Columns.COLUMN_MATERIAL), "", BuildingPanel.iCOLUMN_WIDTH_LARGE, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, GetText(ListViewRowComparer.Columns.COLUMN_VALUE), "", BuildingPanel.iCOLUMN_WIDTH_NORMAL, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_TIMER, GetText(ListViewRowComparer.Columns.COLUMN_TIMER), "", BuildingPanel.iCOLUMN_WIDTH_SMALL, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET, GetText(ListViewRowComparer.Columns.COLUMN_TARGET), "", BuildingPanel.iCOLUMN_WIDTH_LARGER, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_LOAD, GetText(ListViewRowComparer.Columns.COLUMN_LOAD), "", BuildingPanel.iCOLUMN_WIDTH_SMALL, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_OWNER, GetText(ListViewRowComparer.Columns.COLUMN_OWNER), "", BuildingPanel.iCOLUMN_WIDTH_LARGER, UIHorizontalAlignment.Left, UIAlignAnchor.TopRight);
        }

        public override void OnClick(ListViewRowColumn column)
        {
            if (column.GetColumn() == ListViewRowComparer.Columns.COLUMN_OWNER)
            {
                m_status.OnClickResponder();
            }
            else if (column.GetColumn() == ListViewRowComparer.Columns.COLUMN_TARGET)
            {
                m_status.OnClickTarget();
            }
        }

        public override string OnTooltip(ListViewRowColumn column)
        {
            return "";
        }
    }
}
