using HarmonyLib;
using System;
using TransferManagerCE;
using UnityEngine;

public class TransferIssueContainer : IComparable, IEquatable<TransferIssueContainer>
{
    public enum IssueType
    {
        Sick,
        Dead,
        Crime,
        Garbage,
        Incoming,
        Outgoing,
        Worker,
        Services,
        Mail,
        Fire,
    }

    public IssueType m_issue;
    public string m_value;
    public int m_timer;

    public Vector3 m_sourcePostion;
    public Vector3 m_targetPostion;

    private int m_priority;
    private ushort m_sourceBuildingId;
    private ushort m_targetBuildingId;
    private ushort m_vehicleId;

    private string? m_source = null;
    private string? m_target = null;
    private string? m_vehicle = null;

    public TransferIssueContainer(IssueType issue, int iPriority, string value, int timer, ushort sourceBuildingId, ushort targetBuildingId, ushort vehicleId)
    {
        m_issue = issue;
        m_priority = iPriority;
        m_value = value;
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
        if (second is null)
        {
            return 1;
        }

        TransferIssueContainer oSecond = (TransferIssueContainer)second;

        if (m_priority != oSecond.m_priority)
        {
            return oSecond.m_priority.CompareTo(m_priority);
        }

        if (m_timer != 0 && oSecond.m_timer != 0 && m_timer != oSecond.m_timer)
        {
            return oSecond.m_timer.CompareTo(m_timer);
        }

        if (m_issue != oSecond.m_issue)
        {
            return m_issue.CompareTo(oSecond.m_issue);
        }

        return oSecond.GetValue().CompareTo(GetValue());
    }

    public bool Equals(TransferIssueContainer second)
    {
        if (second is null)
        {
            return false;
        }
        TransferIssueContainer oSecond = (TransferIssueContainer)second;
        return oSecond.m_issue == m_issue &&
            oSecond.m_sourceBuildingId == m_sourceBuildingId &&
            oSecond.m_targetBuildingId == m_targetBuildingId &&
            oSecond.m_vehicleId == m_vehicleId;
    }

    public bool HasVehicle()
    {
        return m_vehicleId != 0;
    }

    public ushort GetVehicleId()
    {
        return m_vehicleId;
    }

    public ushort GetBuildingId()
    {
        return m_sourceBuildingId;
    }

    public IssueType GetIssue()
    {
        return m_issue;
    }

    public int GetPriority()
    {
        return Mathf.Clamp(m_priority, 0, 7);
    }

    public string GetTimer()
    {
        if (m_timer > 0)
        {
            return m_timer.ToString();
        }
        return "";
    }

    public string GetValue()
    {
        return m_value;
    }

    public string GetValueTooltip()
    {
        return $"{m_issue}:{m_value}";
    }

    public string GetSource()
    {
        if (m_source is null)
        {
            m_source = CitiesUtils.GetBuildingName(m_sourceBuildingId);
        }
        return m_source;
    }

    public string GetSourceTooltip()
    {
        return CitiesUtils.GetBuildingName(m_sourceBuildingId, true);
    }

    public string GetTarget()
    {
        if (m_target is null)
        {
            m_target = CitiesUtils.GetBuildingName(m_targetBuildingId);
        }
        return m_target;
    }

    public string GetTargetTooltip()
    {
        return CitiesUtils.GetBuildingName(m_targetBuildingId, true);
    }

    public string GetVehicle()
    {
        if (m_vehicle is null)
        {
            m_vehicle = CitiesUtils.GetVehicleName(m_vehicleId);
        }
        return m_vehicle;
    }

    public string GetVehicleTooltip()
    {
        return CitiesUtils.GetVehicleName(m_vehicleId, true);
    }

    public void ShowSource()
    {
        Building building = BuildingManager.instance.m_buildings.m_buffer[m_sourceBuildingId];
        if (building.m_flags != Building.Flags.None)
        {
            InstanceHelper.ShowInstanceSetBuildingPanel(new InstanceID { Building = m_sourceBuildingId });
        }
        else
        {
            CitiesUtils.ShowPosition(m_sourcePostion);
        }
    }

    public void ShowTarget()
    {
        Building building = BuildingManager.instance.m_buildings.m_buffer[m_targetBuildingId];
        if (building.m_flags != Building.Flags.None)
        {
            InstanceHelper.ShowInstanceSetBuildingPanel(new InstanceID { Building = m_targetBuildingId });
        }
        else
        {
            CitiesUtils.ShowPosition(m_targetPostion);
        }
    }

    public void ShowVehicle()
    {
        InstanceHelper.ShowInstance(new InstanceID { Vehicle = m_vehicleId });
    }

    public Color GetColor()
    {
        return GetColor(m_issue).color;
    }

    public static KnownColor GetColor(IssueType issueType)
    {
        switch (issueType)
        {
            case IssueType.Sick:
                return KnownColor.red;
            case IssueType.Dead:
                return KnownColor.gold;
            case IssueType.Crime:
                return KnownColor.blue;
            case IssueType.Garbage:
                return KnownColor.brown;
            case IssueType.Incoming:
                return KnownColor.green;
            case IssueType.Outgoing:
                return KnownColor.cyan;
            case IssueType.Worker:
                return KnownColor.purple;
            case IssueType.Services:
                return KnownColor.magenta;
            case IssueType.Mail:
                return KnownColor.yellow;
            case IssueType.Fire:
                return KnownColor.orange;
            default:
                return KnownColor.white;
        }
    }
}