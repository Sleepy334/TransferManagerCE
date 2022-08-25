using ColossalFramework;
using ColossalFramework.UI;
using SleepyCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TransferManagerCE.Util;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE.Data
{
    public class VehicleData : ListData
    {
        public ushort m_vehicleId;
        public Vehicle m_vehicle;

        public VehicleData(ushort vehicleId)
        {
            m_vehicleId = vehicleId;
            m_vehicle = VehicleManager.instance.m_vehicles.m_buffer[m_vehicleId];
        }

        public override int CompareTo(object second)
        {
            if (second == null)
            {
                return 1;
            }
            VehicleData oSecond = (VehicleData)second;
            return m_vehicle.m_transferType.CompareTo(oSecond.m_vehicle.m_transferType);
        }

        public virtual string GetTarget()
        {
            ushort vehicleId = GetVehicleId();
            if (vehicleId != 0)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                return VehicleTypeHelper.DescribeVehicleTarget(vehicle, VehicleTypeHelper.GetVehicleTarget(vehicle));
            }

            return Localization.Get("txtVehiclesNone");
        }

        public ushort GetVehicleId()
        {
            if (m_vehicleId != 0)
            {
                if (m_vehicle.m_cargoParent != 0)
                {
                    return m_vehicle.m_cargoParent;
                }
            }
            return m_vehicleId;
        }

        public virtual string GetVehicle()
        {
            ushort vehicleId = GetVehicleId();
            if (vehicleId != 0)
            {
                string sName = CitiesUtils.GetVehicleName(vehicleId);
                if (string.IsNullOrEmpty(sName))
                {
                    sName = "Vehicle:" + m_vehicleId;
                }
                if ((m_vehicle.m_flags & Vehicle.Flags.WaitingLoading) == Vehicle.Flags.WaitingLoading ||
                    (m_vehicle.m_flags & Vehicle.Flags.WaitingCargo) == Vehicle.Flags.WaitingCargo)
                {
                    sName += " (Loading)";
                }
                return sName;
            }

            return Localization.Get("txtVehiclesNone");
        }

        public string GetMaterialDescription()
        {
            return ((TransferReason)m_vehicle.m_transferType).ToString();
        }

        public virtual string GetValue()
        {
            return CitiesUtils.GetVehicleTransferValue(GetVehicleId());
        }

        public virtual string GetTimer()
        {
            string sTimer = "";
            
            ushort vehicleId = GetVehicleId();
            if (vehicleId != 0)
            {
                if (m_vehicle.m_waitCounter > 0)
                {
                    sTimer += "W:" + m_vehicle.m_waitCounter + " ";
                }
                if (m_vehicle.m_blockCounter > 0)
                {
                    sTimer += "B:" + m_vehicle.m_blockCounter;
                }
            }

            return sTimer;
        }

        public override string GetText(ListViewRowComparer.Columns eColumn)
        {
            switch (eColumn)
            {
                case ListViewRowComparer.Columns.COLUMN_MATERIAL: return GetMaterialDescription();
                case ListViewRowComparer.Columns.COLUMN_VALUE: return GetValue();
                case ListViewRowComparer.Columns.COLUMN_TIMER: return GetTimer();
                case ListViewRowComparer.Columns.COLUMN_VEHICLE: return GetVehicle();
                case ListViewRowComparer.Columns.COLUMN_TARGET: return GetTarget();
            }
            return "";
        }

        public override void CreateColumns(ListViewRow oRow, List<ListViewRowColumn> m_columns)
        {
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, GetText(ListViewRowComparer.Columns.COLUMN_MATERIAL), "", BuildingPanel.iCOLUMN_WIDTH_LARGE, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, GetText(ListViewRowComparer.Columns.COLUMN_VALUE), "", BuildingPanel.iCOLUMN_WIDTH_NORMAL, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_TIMER, GetText(ListViewRowComparer.Columns.COLUMN_TIMER), "", BuildingPanel.iCOLUMN_WIDTH_SMALL, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_VEHICLE, GetText(ListViewRowComparer.Columns.COLUMN_VEHICLE), "", BuildingPanel.iCOLUMN_WIDTH_XLARGE, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET, GetText(ListViewRowComparer.Columns.COLUMN_TARGET), "", BuildingPanel.iCOLUMN_WIDTH_250, UIHorizontalAlignment.Left, UIAlignAnchor.TopRight);
        }
        public override void OnClick(ListViewRowColumn column)
        {
            if (column.GetColumn() == ListViewRowComparer.Columns.COLUMN_TARGET)
            {
                InstanceID target = VehicleTypeHelper.GetVehicleTarget(m_vehicle);
                InstanceHelper.ShowInstance(target);
            }
            else if (column.GetColumn() == ListViewRowComparer.Columns.COLUMN_VEHICLE)
            {
                if (m_vehicleId != 0)
                {
                    CitiesUtils.ShowVehicle(m_vehicleId);
                }
            }
        }

        public override string OnTooltip(ListViewRowColumn column)
        {
            return "";
        }

        
    }
}
