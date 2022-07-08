using ColossalFramework.UI;
using SleepyCommon;
using System.Collections.Generic;
using TransferManagerCE.Data;
using UnityEngine;
using static TransferManager;

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
            return m_status.m_material.CompareTo(oSecond.m_status.m_material);
        }

        public string GetMaterialDescription()
        {
            return m_status.GetMaterialDescription();
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
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, GetText(ListViewRowComparer.Columns.COLUMN_MATERIAL), "", TransferBuildingPanel.iCOLUMN_WIDTH_NORMAL, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, GetText(ListViewRowComparer.Columns.COLUMN_VALUE), "", TransferBuildingPanel.iCOLUMN_WIDTH_SMALL, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_TIMER, GetText(ListViewRowComparer.Columns.COLUMN_TIMER), "", TransferBuildingPanel.iCOLUMN_WIDTH_SMALL, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_OWNER, GetText(ListViewRowComparer.Columns.COLUMN_OWNER), "", TransferBuildingPanel.iCOLUMN_WIDTH_250, UIHorizontalAlignment.Left, UIAlignAnchor.TopRight);
            oRow.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET, GetText(ListViewRowComparer.Columns.COLUMN_TARGET), "", TransferBuildingPanel.iCOLUMN_WIDTH_250, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft);
        }

        public override void OnClick(ListViewRowColumn column)
        {
            if (column.GetColumn() == ListViewRowComparer.Columns.COLUMN_OWNER)
            {
                if (m_status.m_responderBuilding != 0)
                {
                    Building building = BuildingManager.instance.m_buildings.m_buffer[m_status.m_responderBuilding];
                    Vector3 oPosition = building.m_position;
                    InstanceID buildingInstance = new InstanceID();
                    buildingInstance.Building = m_status.m_responderBuilding;
                    ToolsModifierControl.cameraController.SetTarget(buildingInstance, oPosition, false);
                }
            }
            else if (column.GetColumn() == ListViewRowComparer.Columns.COLUMN_TARGET)
            {
                if (m_status.m_targetVehicle != 0)
                {
                    Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[m_status.m_targetVehicle];
                    Vector3 oPosition = CitiesUtils.GetVehiclePosition(vehicle);
                    InstanceID vehicleInstance = new InstanceID();
                    vehicleInstance.Vehicle = m_status.m_targetVehicle;
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
