using System.Collections.Generic;
using TransferManagerCE.Util;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataMail : StatusData
    {
        public StatusDataMail(TransferReason reason, BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) : 
            base(reason, eBuildingType, BuildingId, responder, target)
        {
        }

        public override string GetValueTooltip()
        {
            switch (m_eBuildingType)
            {
                case BuildingType.PostOffice:
                    {
                        return "Amount of mail being processed in Post Office.";
                    }
                case BuildingType.PostSortingFacility:
                    {
                        return "Amount of mail being processed in Post Sorting Facility.";
                    }
                case BuildingType.ServicePoint:
                    {
                        return $"<Building Count> | <Buffer>";
                    }
            }

            return "Value of buildings mail buffer.";
        }

        protected override string CalculateValue()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            switch (m_eBuildingType)
            {
                case BuildingType.PostOffice:
                case BuildingType.PostSortingFacility:
                    {
                        if (building.Info is not null && building.Info.GetAI() is PostOfficeAI postOfficeAI)
                        {
                            int amount;
                            int max;
                            postOfficeAI.GetMaterialAmount(m_buildingId, ref building, m_material, out amount, out max);
                            return (amount * 0.1).ToString();
                        }
                        break;
                    }
                case BuildingType.ServicePoint:
                    {
                        ServicePointUtils.GetServicePointOutValues(m_buildingId, m_material, out int iCount, out int iBuffer);
                        return $"{iCount} | {ServicePointUtils.DisplayBuffer(iBuffer)}";
                    }
                case BuildingType.Residential:
                    {
                        return $"{building.m_mailBuffer}/{CitiesUtils.GetHomeCount(building) * 50}";
                    }
                case BuildingType.Commercial:
                case BuildingType.GenericFactory:
                case BuildingType.GenericProcessing:
                    {
                        return $"{building.m_mailBuffer}/{CitiesUtils.GetWorkerCount(m_buildingId, building) * 50}";
                    }
            }

            return building.m_mailBuffer.ToString();
        }

        protected override string CalculateTarget()
        {
            switch (m_eBuildingType)
            {
                case BuildingType.PostOffice:
                    {
                        if (m_material == TransferReason.UnsortedMail)
                        {
                            return "";
                        }
                        break;
                    }
                case BuildingType.PostSortingFacility:
                    {
                        if (m_material == TransferReason.SortedMail)
                        {
                            return "";
                        }
                        break;
                    }
            }

            return base.CalculateTarget();
        }

        protected override string CalculateResponder()
        {
            switch (m_eBuildingType)
            {
                case BuildingType.PostOffice:
                    {
                        if (m_material == TransferReason.UnsortedMail)
                        {
                            return "";
                        }
                        break;
                    }
                case BuildingType.PostSortingFacility:
                    {
                        if (m_material == TransferReason.SortedMail)
                        {
                            return "";
                        }
                        break;
                    }
            }

            return base.CalculateResponder();
        }
    }
}