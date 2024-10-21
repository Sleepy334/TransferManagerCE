using ColossalFramework.Math;
using Epic.OnlineServices.Presence;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataCommercial : StatusData
    {
        public StatusDataCommercial(TransferReason material, BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) :
            base(material, eBuildingType, BuildingId, responder, target)
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
                return m_material;
            }
        }

        protected override string CalculateValue()
        {
            bool bIncoming = m_material == TransferReason.Goods || m_material == TransferReason.Food;

            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            
            CommercialBuildingAI? buildingAI = building.Info.GetAI() as CommercialBuildingAI;
            if (buildingAI is not null)
            {
                if (bIncoming)
                {
                    int MaxIncomingLoadSize = 4000;
                    int visitPlaceCount = buildingAI.CalculateVisitplaceCount((ItemClass.Level)building.m_level, new Randomizer(m_buildingId), building.Width, building.Length);
                    int iBufferSize = Mathf.Max(visitPlaceCount * 500, MaxIncomingLoadSize * 4);
                    return $"{(building.m_customBuffer1 * 0.001f).ToString("N1")}/{Mathf.Round(iBufferSize * 0.001f)}"; 
                }
                else
                {
                    return $"{building.m_customBuffer2}";
                }
            }

            return (building.m_customBuffer1 * 0.001).ToString("N1");
        }

        protected override string CalculateTimer()
        {
            bool bIncoming = m_material == TransferReason.Goods || m_material == TransferReason.Food;

            string sTimer = base.CalculateTimer();
            
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_flags != 0)
            {
                if (bIncoming && building.m_incomingProblemTimer > 0)
                {
                    if (string.IsNullOrEmpty(sTimer))
                    {
                        sTimer += " ";
                    }
                    sTimer += "I:" + building.m_incomingProblemTimer;
                }

                if (!bIncoming && building.m_outgoingProblemTimer > 0)
                {
                    if (string.IsNullOrEmpty(sTimer))
                    {
                        sTimer += " ";
                    }
                    sTimer += "O:" + building.m_outgoingProblemTimer;
                }
            }

            return sTimer;
        }

        protected override string CalculateTarget()
        {
            bool bIncoming = m_material == TransferReason.Goods || m_material == TransferReason.Food;

            if (bIncoming)
            {
                return base.CalculateTarget(); 
            }
            else
            {
                return ""; // We currently dont show cims only vehicles.
            }
        }

        protected override string CalculateResponder()
        {
            bool bIncoming = m_material == TransferReason.Goods || m_material == TransferReason.Food;

            if (bIncoming)
            {
                return base.CalculateResponder();
            }
            else
            {
                return ""; // We currently dont show cims only vehicles.
            }
        }

        public static TransferReason GetOutgoingTransferReason(ushort buildingID, BuildingInfo info)
        {
            int num = 0;
            if (info.m_class.isCommercialLowGeneric)
            {
                num = 2;
            }
            else if (info.m_class.isCommercialHighGenegic || info.m_class.isCommercialWallToWall)
            {
                num = 4;
            }
            else if (info.m_class.isCommercialLeisure)
            {
                num = 20;
            }
            else if (info.m_class.isCommercialTourist)
            {
                num = 80;
            }
            else if (info.m_class.isCommercialEco)
            {
                num = 0;
            }

            Randomizer randomizer = new Randomizer(buildingID);
            if (randomizer.Int32(100u) < num)
            {
                return randomizer.Int32(4u) switch
                {
                    0 => TransferManager.TransferReason.Entertainment,
                    1 => TransferManager.TransferReason.EntertainmentB,
                    2 => TransferManager.TransferReason.EntertainmentC,
                    3 => TransferManager.TransferReason.EntertainmentD,
                    _ => TransferManager.TransferReason.Entertainment,
                };
            }

            return randomizer.Int32(8u) switch
            {
                0 => TransferManager.TransferReason.Shopping,
                1 => TransferManager.TransferReason.ShoppingB,
                2 => TransferManager.TransferReason.ShoppingC,
                3 => TransferManager.TransferReason.ShoppingD,
                4 => TransferManager.TransferReason.ShoppingE,
                5 => TransferManager.TransferReason.ShoppingF,
                6 => TransferManager.TransferReason.ShoppingG,
                7 => TransferManager.TransferReason.ShoppingH,
                _ => TransferManager.TransferReason.Shopping,
            };
        }
    }
}