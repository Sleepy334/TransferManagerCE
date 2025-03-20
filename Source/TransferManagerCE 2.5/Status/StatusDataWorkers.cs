using UnityEngine;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataWorkers : StatusData
    {
        public static Color orange => new Color(1f, 0.5f, 0.15f, 1f); 
        
        private Color m_color = Color.white;

        public StatusDataWorkers(TransferReason material, BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) :
            base(material, eBuildingType, BuildingId, responder, target)
        {
        }

        public override string GetMaterialDescription()
        {
            switch ((TransferReason)GetMaterial())
            {
                case TransferReason.None:
                    {
                        return "Workers";
                    }
                default:
                    {
                        return GetMaterial().ToString();
                    }

            }
        }

        public override Color GetTextColor()
        {
            return m_color;
        }

        protected override string CalculateValue()
        {
            if (m_buildingId != 0)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
                if (building.m_flags != 0)
                {
                    int iCurrentWorkers = BuildingUtils.GetCurrentWorkerCount(m_buildingId, building, out int worker0, out int worker1, out int worker2, out int worker3);
                    int iWorkPlaces = BuildingUtils.GetTotalWorkerCount(m_buildingId, building, out int workPlaces0, out int workPlaces1, out int workPlaces2, out int workPlaces3);
                    
                    switch ((TransferReason) GetMaterial())
                    {
                        case TransferReason.Worker0:
                            {
                                if (worker0 > workPlaces0)
                                {
                                    m_color = orange;
                                }
                                return $"{worker0} / {workPlaces0}";
                            }
                        case TransferReason.Worker1:
                            {
                                if (worker1 > workPlaces1)
                                {
                                    m_color = orange;
                                }
                                return $"{worker1} / {workPlaces1}";
                            }
                        case TransferReason.Worker2:
                            {
                                if (worker2 > workPlaces2)
                                {
                                    m_color = orange;
                                }
                                return $"{worker2} / {workPlaces2}";
                            }
                        case TransferReason.Worker3:
                            {
                                if (worker3 > workPlaces3)
                                {
                                    m_color = orange;
                                }
                                return $"{worker3} / {workPlaces3}";
                            }
                        case TransferReason.None:
                            {
                                return $"{iCurrentWorkers} / {iWorkPlaces}";
                            }

                    }
                }
            }

            return "-";
        }

        protected override string CalculateTimer()
        {
            return "";
        }

        protected override string CalculateTarget()
        {
            return ""; // No vehicles
        }

        protected override string CalculateResponder()
        {
            return "";
        }

        public static TransferReason GetOutgoingTransferReason(Building building)
        {
            return TransferReason.None;
        }

        public override string GetValueTooltip()
        {
            return "Current Workers / Total Work Places";
        }
    }
}