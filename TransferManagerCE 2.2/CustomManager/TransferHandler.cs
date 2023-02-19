using System;
using static TransferManager;

namespace TransferManagerCE
{
    internal class TransferHandler
    {
        public static int s_iInvalidObjects = 0;

        //---------------------------------------------------------------------
        // Copied from TransferManager so we can check object is still valid before calling StartTransfer
        // as we may have taken too long to match the object and it may have been removed.
        public static void StartTransfer(Building[] Buildings, Vehicle[] Vehicles, Citizen[] Citizens, DistrictPark[] Parks, TransferReason material, TransferOffer offerOut, TransferOffer offerIn, int delta)
        {
            bool active = offerIn.Active;
            bool active2 = offerOut.Active;

            if (offerOut.Park != 0 && Parks[offerOut.Park].TryGetRandomServicePoint(material, out var buildingID))
            {
                offerOut.Building = buildingID;
            }

            if (offerIn.Park != 0 && Parks[offerIn.Park].TryGetRandomServicePoint(material, out var buildingID2))
            {
                offerIn.Building = buildingID2;
            }

            if (active && offerIn.Vehicle != 0)
            {
                ushort vehicleId = offerIn.Vehicle;
                ref Vehicle vehicle = ref Vehicles[vehicleId];
                if (vehicle.m_flags != 0)
                {
                    VehicleInfo info = vehicle.Info;
                    info.m_vehicleAI.StartTransfer(vehicleId, ref vehicle, material, offerOut);
                    offerOut.Amount = delta;
                }
                else
                {
                    s_iInvalidObjects++;
                }
            }
            else if (active2 && offerOut.Vehicle != 0)
            {
                ushort vehicleId = offerOut.Vehicle;
                ref Vehicle vehicle = ref Vehicles[vehicleId];
                if (vehicle.m_flags != 0)
                {
                    VehicleInfo info = vehicle.Info;
                    info.m_vehicleAI.StartTransfer(vehicleId, ref vehicle, material, offerIn);
                    offerIn.Amount = delta;
                }
                else
                {
                    s_iInvalidObjects++;
                }
            }
            else if (active && offerIn.Citizen != 0)
            {
                uint citizenId = offerIn.Citizen;
                ref Citizen citizen = ref Citizens[citizenId];
                if (citizen.m_flags != 0)
                {
                    CitizenInfo citizenInfo = citizen.GetCitizenInfo(citizenId);
                    if ((object)citizenInfo != null)
                    {
                        citizenInfo.m_citizenAI.StartTransfer(citizenId, ref citizen, material, offerOut);
                        offerOut.Amount = delta;
                    }
                    else
                    {
                        s_iInvalidObjects++;
                    }
                }
                else
                {
                    s_iInvalidObjects++;
                }
            }
            else if (active2 && offerOut.Citizen != 0)
            {
                uint citizenId = offerOut.Citizen;
                ref Citizen citizen = ref Citizens[citizenId];
                if (citizen.m_flags != 0)
                {
                    CitizenInfo citizenInfo = citizen.GetCitizenInfo(citizenId);
                    if ((object)citizenInfo != null)
                    {
                        citizenInfo.m_citizenAI.StartTransfer(citizenId, ref citizen, material, offerIn);
                        offerIn.Amount = delta;
                    }
                    else
                    {
                        s_iInvalidObjects++;
                    }
                }
                else
                {
                    s_iInvalidObjects++;
                }
            }
            else if (active2 && offerOut.Building != 0)
            {
                if (offerOut.m_isLocalPark != 0 && offerOut.m_isLocalPark == offerIn.m_isLocalPark)
                {
                    StartDistrictTransfer(Buildings, material, offerOut, offerIn);
                    return;
                }

                ref Building building = ref Buildings[offerOut.Building];
                if (building.m_flags != 0)
                {
                    BuildingInfo info3 = building.Info;
                    info3.m_buildingAI.StartTransfer(offerOut.Building, ref building, material, offerIn);
                }
                else
                {
                    s_iInvalidObjects++;
                }
                offerIn.Amount = delta;
            }
            else if (active && offerIn.Building != 0)
            {
                if (offerIn.m_isLocalPark != 0 && offerIn.m_isLocalPark == offerOut.m_isLocalPark)
                {
                    StartDistrictTransfer(Buildings, material, offerOut, offerIn);
                    return;
                }

                ref Building building = ref Buildings[offerIn.Building];
                if (building.m_flags != 0)
                {
                    BuildingInfo info4 = building.Info;
                    info4.m_buildingAI.StartTransfer(offerIn.Building, ref building, material, offerOut);
                }
                else
                {
                    s_iInvalidObjects++;
                }
                offerOut.Amount = delta;
            }
        }

        //---------------------------------------------------------------------
        private static void StartDistrictTransfer(Building[] Buildings, TransferReason material, TransferOffer offerOut, TransferOffer offerIn)
        {
            ushort building = offerOut.Building;
            ushort building2 = offerIn.Building;
            BuildingInfo info = Buildings[building].Info;
            BuildingInfo info2 = Buildings[building2].Info;
            info.m_buildingAI.GetMaterialAmount(building, ref Buildings[building], material, out var amount, out var _);
            info2.m_buildingAI.GetMaterialAmount(building2, ref Buildings[building2], material, out var amount2, out var max2);
            int num = Math.Min(amount, max2 - amount2);
            if (num > 0)
            {
                amount = -num;
                amount2 = num;
                info.m_buildingAI.ModifyMaterialBuffer(building, ref Buildings[building], material, ref amount);
                info2.m_buildingAI.ModifyMaterialBuffer(building2, ref Buildings[building2], material, ref amount2);
            }
        }
    }
}
