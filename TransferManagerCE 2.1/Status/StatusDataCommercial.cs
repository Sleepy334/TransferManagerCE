using ColossalFramework.Math;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataCommercial : StatusData
    {
        public StatusDataCommercial(BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) :
            base(TransferReason.Goods, eBuildingType, BuildingId, responder, target)
        {
        }

        public override CustomTransferReason GetMaterial()
        {
            if (m_targetVehicle != 0)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[m_targetVehicle];
                return (TransferReason)vehicle.m_transferType;
            }
            else
            {
                return TransferReason.Goods;
            }
        }

        public override string GetValue()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            
            CommercialBuildingAI? buildingAI = building.Info.GetAI() as CommercialBuildingAI;
            if (buildingAI != null)
            {
                int MaxIncomingLoadSize = 4000;
                int visitPlaceCount = buildingAI.CalculateVisitplaceCount((ItemClass.Level)building.m_level, new Randomizer(m_buildingId), building.Width, building.Length);
                int iBufferSize = Mathf.Max(visitPlaceCount * 500, MaxIncomingLoadSize * 4);
                return $"{Mathf.Round(building.m_customBuffer1 * 0.001f)}/{Mathf.Round(iBufferSize * 0.001f)}";
            }

            return (building.m_customBuffer1 * 0.001).ToString("N0");
        }

        public override string GetTimer()
        {
            string sTimer = base.GetTimer();
            
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_incomingProblemTimer > 0)
            {
                if (string.IsNullOrEmpty(sTimer))
                {
                    sTimer += " ";
                }
                sTimer += "I:" + building.m_incomingProblemTimer;
            }

            return sTimer;
        }
    }
}