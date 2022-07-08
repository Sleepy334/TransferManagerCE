using ColossalFramework.UI;
using SleepyCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE.Data
{
    public class VehicleData : ListData
    {
        public ushort m_vehicleId;

        public VehicleData(ushort vehicleId)
        {
            m_vehicleId = vehicleId;
        }
        public override int CompareTo(object second)
        {
            if (second == null)
            {
                return 1;
            }
            VehicleData oSecond = (VehicleData)second;
            return m_vehicleId.CompareTo(oSecond.m_vehicleId);
        }

        public virtual string GetTarget()
        {
            if (m_vehicleId != 0)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[m_vehicleId]; 
                if (vehicle.m_targetBuilding != 0)
                {
                    return CitiesUtils.GetBuildingName(vehicle.m_targetBuilding);
                }
                else if ((vehicle.m_flags & Vehicle.Flags.GoingBack) == Vehicle.Flags.GoingBack)
                {
                    return "Returning to facility";
                }
            }

            return "None";
        }
        
        public virtual string GetVehicle()
        {
            if (m_vehicleId != 0)
            {
                return CitiesUtils.GetVehicleName(m_vehicleId);
            }

            return "None";
        }

        public string GetMaterialDescription()
        {
            Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[m_vehicleId];
            return ((TransferReason)vehicle.m_transferType).ToString();
        }

        public virtual string GetValue()
        {
            Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[m_vehicleId];
            if (vehicle.Info.GetAI() is CargoTruckAI || vehicle.Info.GetAI() is PostVanAI)
            {
                return (vehicle.m_transferSize * 0.001).ToString("N0");
            }
            else
            {
                return vehicle.m_transferSize.ToString();
            }
        }
        

        public override string GetText(ListViewRowComparer.Columns eColumn)
        {
            switch (eColumn)
            {
                case ListViewRowComparer.Columns.COLUMN_MATERIAL: return GetMaterialDescription();
                case ListViewRowComparer.Columns.COLUMN_VALUE: return GetValue();
                case ListViewRowComparer.Columns.COLUMN_VEHICLE: return GetVehicle();
                case ListViewRowComparer.Columns.COLUMN_TARGET: return GetTarget();
            }
            return "";
        }

        public override void CreateColumns(ListViewRow oRow, List<ListViewRowColumn> m_columns)
        {
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, GetText(ListViewRowComparer.Columns.COLUMN_MATERIAL), "", TransferBuildingPanel.iCOLUMN_WIDTH_NORMAL, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, GetText(ListViewRowComparer.Columns.COLUMN_VALUE), "", TransferBuildingPanel.iCOLUMN_WIDTH_NORMAL, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_VEHICLE, GetText(ListViewRowComparer.Columns.COLUMN_VEHICLE), "", TransferBuildingPanel.iCOLUMN_WIDTH_250, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET, GetText(ListViewRowComparer.Columns.COLUMN_TARGET), "", TransferBuildingPanel.iCOLUMN_WIDTH_250, UIHorizontalAlignment.Left, UIAlignAnchor.TopRight);
        }
        public override void OnClick(ListViewRowColumn column)
        {
            Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[m_vehicleId];
            if (column.GetColumn() == ListViewRowComparer.Columns.COLUMN_TARGET)
            {
                if (vehicle.m_targetBuilding != 0)
                {
                    Building building = BuildingManager.instance.m_buildings.m_buffer[vehicle.m_targetBuilding];
                    Vector3 oPosition = building.m_position;
                    InstanceID buildingInstance = new InstanceID();
                    buildingInstance.Building = vehicle.m_targetBuilding;
                    ToolsModifierControl.cameraController.SetTarget(buildingInstance, oPosition, false);
                }
            }
            else if (column.GetColumn() == ListViewRowComparer.Columns.COLUMN_VEHICLE)
            {
                if (m_vehicleId != 0)
                {
                    Vector3 oPosition = CitiesUtils.GetVehiclePosition(vehicle);
                    InstanceID vehicleInstance = new InstanceID();
                    vehicleInstance.Vehicle = m_vehicleId;
                    ToolsModifierControl.cameraController.SetTarget(vehicleInstance, oPosition, false);
                }
            }
        }

        public override string OnTooltip(ListViewRowColumn column)
        {
            return "";
        }
    }
}
