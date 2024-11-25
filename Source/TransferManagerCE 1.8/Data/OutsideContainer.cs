using ColossalFramework.UI;
using SleepyCommon;

namespace TransferManagerCE
{
    public class OutsideContainer : ListData
    {
        public ushort m_buildingId;
        public BuildingTypeHelper.OutsideType m_eType;

        public OutsideContainer(ushort buildingId)
        {
            m_buildingId = buildingId;
            m_eType = BuildingTypeHelper.GetOutsideConnectionType(m_buildingId);
        }

        public override int CompareTo(object second)
        {
            if (second == null)
            {
                return 1;
            }
            OutsideContainer oSecond = (OutsideContainer)second;
            return m_eType.CompareTo(oSecond.m_eType);
        }

        public override string GetText(ListViewRowComparer.Columns eColumn)
        {
            switch (eColumn)
            {
                case ListViewRowComparer.Columns.COLUMN_NAME: return CitiesUtils.GetBuildingName(m_buildingId) + ":" + m_buildingId;
                case ListViewRowComparer.Columns.COLUMN_TYPE: return m_eType.ToString();
                case ListViewRowComparer.Columns.COLUMN_GUEST: return CitiesUtils.GetGuestParentVehiclesForBuilding(m_buildingId).Count.ToString();
                case ListViewRowComparer.Columns.COLUMN_OWN: return CitiesUtils.GetOwnParentVehiclesForBuilding(m_buildingId).Count.ToString();
                case ListViewRowComparer.Columns.COLUMN_MULTIPLIER: return BuildingSettings.GetEffectiveOutsideMultiplier(m_buildingId).ToString();
                case ListViewRowComparer.Columns.COLUMN_IMPORT: return BuildingSettings.IsImportDisabled(m_buildingId) ? "No" : "Yes";
                case ListViewRowComparer.Columns.COLUMN_EXPORT: return BuildingSettings.IsExportDisabled(m_buildingId) ? "No" : "Yes";
            }
            return "";
        }

        public override void CreateColumns(ListViewRow oRow, System.Collections.Generic.List<ListViewRowColumn> m_columns)
        {
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_NAME, GetText(ListViewRowComparer.Columns.COLUMN_NAME), "", OutsideConnectionPanel.iCOLUMN_WIDTH_XLARGE, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_TYPE, GetText(ListViewRowComparer.Columns.COLUMN_TYPE), "", OutsideConnectionPanel.iCOLUMN_WIDTH_SMALL, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_MULTIPLIER, GetText(ListViewRowComparer.Columns.COLUMN_MULTIPLIER), "", OutsideConnectionPanel.iCOLUMN_WIDTH_NORMAL, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_IMPORT, GetText(ListViewRowComparer.Columns.COLUMN_IMPORT), "", OutsideConnectionPanel.iCOLUMN_WIDTH_SMALL, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_OWN, GetText(ListViewRowComparer.Columns.COLUMN_OWN), "", OutsideConnectionPanel.iCOLUMN_WIDTH_SMALL, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_EXPORT, GetText(ListViewRowComparer.Columns.COLUMN_EXPORT), "", OutsideConnectionPanel.iCOLUMN_WIDTH_SMALL, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_GUEST, GetText(ListViewRowComparer.Columns.COLUMN_GUEST), "", OutsideConnectionPanel.iCOLUMN_WIDTH_SMALL, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft);
        }

        public override void OnClick(ListViewRowColumn column)
        {
            CitiesUtils.ShowBuilding(m_buildingId, true); 
            
            BuildingPanel.Init(); 
            if (BuildingPanel.Instance != null)
            {    
                BuildingPanel.Instance.ShowPanel(m_buildingId);
            }
        }
    }
}