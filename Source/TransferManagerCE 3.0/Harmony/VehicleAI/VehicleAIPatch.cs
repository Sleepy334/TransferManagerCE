using ColossalFramework;

namespace TransferManagerCE
{
    public class VehicleAIPatch
    {
        protected static bool ShouldReturnToSource(ushort vehicleID, ref Vehicle data)
        {
            if (data.m_sourceBuilding != 0)
            {
                Building sourceBuilding = Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding];
                if ((sourceBuilding.m_flags & Building.Flags.Active) == 0 &&
                    sourceBuilding.m_fireIntensity == 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
