using ColossalFramework.Math;
using Epic.OnlineServices.Presence;
using ICities;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataOffice : StatusDataBuilding
    {
        public StatusDataOffice(CustomTransferReason.Reason material, BuildingType eBuildingType, ushort BuildingId) :
            base(material, eBuildingType, BuildingId)
        {
        }

        public override string GetMaterialDescription()
        {
            switch (m_material)
            {
                case CustomTransferReason.Reason.BusinessA:
                case CustomTransferReason.Reason.BusinessB:
                case CustomTransferReason.Reason.BusinessC:
                case CustomTransferReason.Reason.BusinessD:
                    {
                        return "Business";
                    }
                default:
                    {
                        return base.GetMaterialDescription();
                    }
                    break;
            }
        }

        protected override string CalculateValue(out string tooltip)
        {
            tooltip = "";

            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_flags != 0)
            {
                switch (m_material)
                {
                    case CustomTransferReason.Reason.BusinessA:
                    case CustomTransferReason.Reason.BusinessB:
                    case CustomTransferReason.Reason.BusinessC:
                    case CustomTransferReason.Reason.BusinessD:
                        {
                            OfficeBuildingAI buildingAI = building.Info.GetAI() as OfficeBuildingAI;
                            if (buildingAI is not null)
                            {
                                return $"{BuildingUtils.GetVisitorCount(buildingAI, m_buildingId, building)} / {buildingAI.CalculateVisitplaceCount((ItemClass.Level)building.m_level, new Randomizer(m_buildingId), building.Width, building.Length)}";
                            }
                            break;
                            
                        }
                    default:
                        {
                            return (building.m_customBuffer2 * 0.001).ToString("N1");
                        }
                        break;
                }
            }                

            return "";
        }

        protected override string CalculateTimer(out string tooltip)
        {
            bool bIncoming = m_material == CustomTransferReason.Reason.Goods || m_material == CustomTransferReason.Reason.Food;

            string sTimer = base.CalculateTimer(out tooltip);
            
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_flags != 0 && building.m_outgoingProblemTimer > 0)
            {
                if (!string.IsNullOrEmpty(sTimer))
                {
                    sTimer += " ";
                }
                sTimer += "O:" + building.m_outgoingProblemTimer;
            }

            tooltip = "";
            return sTimer;
        }

        public static TransferReason GetOutgoingTransferReason(Building building)
        {
            if (building.Info.GetSubService() == ItemClass.SubService.OfficeHightech)
            {
                return TransferReason.Goods;
            }

            return TransferReason.None;
        }
    }
}