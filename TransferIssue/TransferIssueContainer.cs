using ColossalFramework.UI;
using SleepyCommon;
using System.Collections.Generic;
using TransferManagerCE;
using static TransferManager;

public class TransferIssueContainer : ListData
{
    public TransferReason m_material;
    public int m_value;
    public int m_timer;
    public ushort m_sourceBuildingId;
    public ushort m_targetBuildingId;
    public ushort m_vehicleId;

    public TransferIssueContainer(TransferReason material, int iValue, int timer, ushort sourceBuilding, ushort targetBuildoing, ushort vehicleId)
    {
        m_material = material;
        m_value = iValue;
        m_timer = timer;
        m_sourceBuildingId = sourceBuilding;
        m_targetBuildingId = targetBuildoing;
        m_vehicleId = vehicleId;
    }

    public override int CompareTo(object second)
    {
        if (second == null)
        {
            return 1;
        }
        TransferIssueContainer oSecond = (TransferIssueContainer)second;
        return oSecond.m_timer.CompareTo(m_timer);
    }

    public override string GetText(ListViewRowComparer.Columns eColumn)
    {
        switch (eColumn)
        {
            case ListViewRowComparer.Columns.COLUMN_MATERIAL: return m_value.ToString();
            case ListViewRowComparer.Columns.COLUMN_TIME: return m_timer.ToString();
            case ListViewRowComparer.Columns.COLUMN_OWNER: return CitiesUtils.GetBuildingName(m_sourceBuildingId).ToString();
            case ListViewRowComparer.Columns.COLUMN_TARGET: return CitiesUtils.GetBuildingName(m_targetBuildingId).ToString();
            case ListViewRowComparer.Columns.COLUMN_VEHICLE: return CitiesUtils.GetVehicleName(m_vehicleId, false).ToString();
        }
        return "TBD";
    }

    public override void CreateColumns(ListViewRow oRow, List<ListViewRowColumn> m_columns)
    {
        oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, GetText(ListViewRowComparer.Columns.COLUMN_MATERIAL), "", TransferIssuePanel.iCOLUMN_WIDTH_VALUE, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft);
        oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_TIME, GetText(ListViewRowComparer.Columns.COLUMN_TIME), "", TransferIssuePanel.iCOLUMN_WIDTH_VALUE, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft);
        oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_OWNER, GetText(ListViewRowComparer.Columns.COLUMN_OWNER), "", TransferIssuePanel.iCOLUMN_VEHICLE_WIDTH, UIHorizontalAlignment.Left, UIAlignAnchor.TopRight);
        oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET, GetText(ListViewRowComparer.Columns.COLUMN_TARGET), "", TransferIssuePanel.iCOLUMN_VEHICLE_WIDTH, UIHorizontalAlignment.Left, UIAlignAnchor.TopRight);
        oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_VEHICLE, GetText(ListViewRowComparer.Columns.COLUMN_VEHICLE), "", TransferIssuePanel.iCOLUMN_VEHICLE_WIDTH, UIHorizontalAlignment.Left, UIAlignAnchor.TopRight);
    }

    public override void OnClick(ListViewRowColumn column)
    {
        switch (column.GetColumn())
        {
            case ListViewRowComparer.Columns.COLUMN_OWNER: CitiesUtils.ShowBuilding(m_sourceBuildingId); break;
            case ListViewRowComparer.Columns.COLUMN_TARGET: CitiesUtils.ShowBuilding(m_targetBuildingId); break;
            case ListViewRowComparer.Columns.COLUMN_VEHICLE: CitiesUtils.ShowVehicle(m_vehicleId); break;
        }
    }

    public string GetValueDescription()
    {
        Building building = BuildingManager.instance.m_buildings.m_buffer[m_sourceBuildingId];
        if (m_material == TransferReason.Sick)
        {
            return m_value + " (" + building.m_healthProblemTimer + ")";
        }
        else if (m_material == TransferReason.Dead)
        {
            return m_value + " (" + building.m_deathProblemTimer + ")";
        }
        return m_value.ToString();
    }
}