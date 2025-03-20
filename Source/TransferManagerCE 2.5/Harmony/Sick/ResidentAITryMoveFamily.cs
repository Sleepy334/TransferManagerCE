using HarmonyLib;
using ColossalFramework;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public static class ResidentAITryMoveFamily
    {
        // Force sick cims to stay in building.
        [HarmonyPatch(typeof(ResidentAI), "TryMoveFamily")]
        [HarmonyPrefix]
        public static bool TryMoveFamily(uint citizenID, ref Citizen data, int familySize)
        {
            if (data.m_homeBuilding != 0 &&
                SaveGameSettings.GetSettings().EnableNewTransferManager &&
                SaveGameSettings.GetSettings().OverrideSickHandler)
            {
                // Check the rest of the family unit for a sick member
                Building building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_homeBuilding];
                if (building.m_healthProblemTimer >= 96 &&
                    building.m_majorProblemTimer == 0)
                {
                    uint unitId = building.FindCitizenUnit(CitizenUnit.Flags.Home, citizenID);
                    if (unitId != 0)
                    {
                        Citizen[] Citizens = Singleton<CitizenManager>.instance.m_citizens.m_buffer;
                        CitizenUnit unit = Singleton<CitizenManager>.instance.m_units.m_buffer[unitId];

                        if (unit.m_citizen4 != 0 && !Citizens[unit.m_citizen4].Sick)
                        {
                            return false; // Don't move family
                        }
                        if (unit.m_citizen3 != 0 && !Citizens[unit.m_citizen3].Sick)
                        {
                            return false; // Don't move family
                        }
                        if (unit.m_citizen2 != 0 && !Citizens[unit.m_citizen2].Sick)
                        {
                            return false; // Don't move family
                        }
                        if (unit.m_citizen1 != 0 && !Citizens[unit.m_citizen1].Sick)
                        {
                            return false; // Don't move family
                        }
                        if (unit.m_citizen0 != 0 && !Citizens[unit.m_citizen0].Sick)
                        {
                            return false; // Don't move family
                        }
                    }
                }
            }

            // Handle by vanilla function
            return true;
        }
    }
}