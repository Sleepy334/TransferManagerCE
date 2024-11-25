using ColossalFramework.UI;
using SleepyCommon;
using System;
using System.Collections.Generic;
using TransferManagerCE;
using UnityEngine;
using static TransferManager;

public class TransferIssueContainer : IComparable, IEquatable<TransferIssueContainer>
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

    public int CompareTo(object second)
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