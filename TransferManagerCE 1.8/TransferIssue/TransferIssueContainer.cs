using ColossalFramework.UI;
using SleepyCommon;
using System;
using System.Collections.Generic;
using TransferManagerCE;
using UnityEngine;
using static TransferManager;

public class TransferIssueContainer : ListData, IEquatable<TransferIssueContainer>
{
    public DateTime m_timeStamp;
    public TransferReason m_material;
    public int m_value;
    public int m_timer;
    public ushort m_sourceBuildingId;
    public Vector3 m_sourcePostion;
    public ushort m_targetBuildingId;
    public Vector3 m_targetPostion;
    public ushort m_vehicleId;

    public TransferIssueContainer(TransferReason material, int iValue, int timer, ushort sourceBuildingId, ushort targetBuildingId, ushort vehicleId)
    {
        m_timeStamp = DateTime.Now;
        m_material = material;
        m_value = iValue;
        m_timer = timer;
        m_sourceBuildingId = sourceBuildingId;
        m_targetBuildingId = targetBuildingId;
        m_vehicleId = vehicleId;

        // Save the position incase the building gets abandoned
        Building sourceBuilding = BuildingManager.instance.m_buildings.m_buffer[m_sourceBuildingId];
        m_sourcePostion = sourceBuilding.m_position;
        Building targetBuilding = BuildingManager.instance.m_buildings.m_buffer[m_targetBuildingId];
        m_targetPostion = targetBuilding.m_position;
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
    public bool Equals(TransferIssueContainer second)
    {
        if (second == null)
        {
            return false;
        }
        TransferIssueContainer oSecond = (TransferIssueContainer)second;
        return oSecond.m_material == m_material &&
            oSecond.m_sourceBuildingId == m_sourceBuildingId &&
            oSecond.m_targetBuildingId == m_targetBuildingId &&
            oSecond.m_vehicleId == m_vehicleId;
    }

    public override string GetText(ListViewRowComparer.Columns eColumn)
    {
        switch (eColumn)
        {
            case ListViewRowComparer.Columns.COLUMN_MATERIAL: return m_value.ToString();
            case ListViewRowComparer.Columns.COLUMN_TIME: return m_timer.ToString();
            case ListViewRowComparer.Columns.COLUMN_OWNER: return CitiesUtils.GetBuildingName(m_sourceBuildingId).ToString();
            case ListViewRowComparer.Columns.COLUMN_TARGET: return CitiesUtils.GetBuildingName(m_targetBuildingId).ToString();
            case ListViewRowComparer.Columns.COLUMN_VEHICLE: return CitiesUtils.GetVehicleName(m_vehicleId).ToString();
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
            case ListViewRowComparer.Columns.COLUMN_OWNER:
                {
                    Building building = BuildingManager.instance.m_buildings.m_buffer[m_sourceBuildingId];
                    TransferManagerCE.Debug.Log("Building flags: " + building.m_flags);
                    if (building.m_flags != Building.Flags.None)
                    {
                        CitiesUtils.ShowBuilding(m_sourceBuildingId);
                    }
                    else
                    {
                        CitiesUtils.ShowPosition(m_sourcePostion);
                    }
                        
                    break;
                }
            case ListViewRowComparer.Columns.COLUMN_TARGET:
                {
                    Building building = BuildingManager.instance.m_buildings.m_buffer[m_targetBuildingId];
                    if (building.m_flags != Building.Flags.None)
                    {
                        CitiesUtils.ShowBuilding(m_targetBuildingId);
                    }
                    else
                    {
                        CitiesUtils.ShowPosition(m_targetPostion);
                    }

                    break;
                }
            case ListViewRowComparer.Columns.COLUMN_VEHICLE: CitiesUtils.ShowVehicle(m_vehicleId); break;
        }
    }

    public string GetValueDescription()
    {
        Building building = BuildingManager.instance.m_buildings.m_buffer[m_sourceBuildingId];
        if ((building.m_flags & Building.Flags.Created) == Building.Flags.Created)
        {
            if (m_material == TransferReason.Sick)
            {
                return m_value + " (" + building.m_healthProblemTimer + ")";
            }
            else if (m_material == TransferReason.Dead)
            {
                return m_value + " (" + building.m_deathProblemTimer + ")";
            }
            else if (m_material == TransferReason.Goods)
            {
                return m_value + " (" + building.m_incomingProblemTimer + ")";
            } 
        }
        return m_value.ToString();
    }
}