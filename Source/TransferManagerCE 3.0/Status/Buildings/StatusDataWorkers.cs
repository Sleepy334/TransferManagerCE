using UnityEngine;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataWorkers : StatusDataBuilding
    {
        public StatusDataWorkers(CustomTransferReason.Reason material, BuildingType eBuildingType, ushort BuildingId) :
            base(material, eBuildingType, BuildingId)
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

        protected override string CalculateValue(out string tooltip)
        {
            tooltip = "Current Workers / Total Work Places";

            if (m_buildingId != 0)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
                if (building.m_flags != 0)
                {
                    int iCurrentWorkers = BuildingUtils.GetCurrentWorkerCount(m_buildingId, building, out int worker0, out int worker1, out int worker2, out int worker3);
                    int iWorkPlaces = BuildingUtils.GetTotalWorkerPlaces(m_buildingId, building, out int workPlaces0, out int workPlaces1, out int workPlaces2, out int workPlaces3);
                    
                    switch ((TransferReason) GetMaterial())
                    {
                        case TransferReason.Worker0:
                            {
                                WarnText(false, true, worker0, workPlaces0 + 1);
                                return $"{worker0} / {workPlaces0}";
                            }
                        case TransferReason.Worker1:
                            {
                                WarnText(false, true, worker1, workPlaces1 + 1);
                                return $"{worker1} / {workPlaces1}";
                            }
                        case TransferReason.Worker2:
                            {
                                WarnText(false, true, worker2, workPlaces2 + 1);
                                return $"{worker2} / {workPlaces2}";
                            }
                        case TransferReason.Worker3:
                            {
                                WarnText(false, true, worker3, workPlaces3 + 1);
                                return $"{worker3} / {workPlaces3}";
                            }
                        case TransferReason.None:
                            {
                                WarnText(false, true, building.m_workerProblemTimer, 1);
                                return $"{iCurrentWorkers} / {iWorkPlaces}";
                            }

                    }
                }
            }

            return "-";
        }

        protected override string CalculateTimer(out string tooltip)
        {
            if ((TransferReason)GetMaterial() == TransferReason.None)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
                if (building.m_workerProblemTimer > 0)
                {
                    return $"E:{building.m_workerProblemTimer} {base.CalculateTimer(out tooltip)}";
                }
            }

            return base.CalculateTimer(out tooltip);
        }

        public static TransferReason GetOutgoingTransferReason(Building building)
        {
            return TransferReason.None;
        }
    }
}