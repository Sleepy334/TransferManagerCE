using System.Collections.Generic;

namespace TransferManagerCE
{
    // The m_citizenCount field is very buggy and it is very slow to calculate the citizens for all buildings
    // so we cache the value passed into the HandleCrime functions for each building type.
    public class CrimeCitizenCountStorage
    {
        private static Dictionary<ushort, int> s_buildingCitizenCount = new Dictionary<ushort, int>();
        static readonly object s_lock = new object();

        public static int GetCitizenCount(ushort buildingId, Building building)
        {
            lock (s_lock)
            {
                if (s_buildingCitizenCount.ContainsKey(buildingId))
                {
                    return s_buildingCitizenCount[buildingId];
                }
                else
                {
                    // If we are asked and dont have a value yet, just calculate basic citizen count instead
                    int iCitizenCount = BuildingUtils.GetCitizenCount(buildingId, building);
                    s_buildingCitizenCount[buildingId] = iCitizenCount;
                    return iCitizenCount;
                }
            }
        }

        public static void SetCitizenCount(ushort buildingId, Building building, int iCitizenCount)
        {
            lock (s_lock)
            {   
                s_buildingCitizenCount[buildingId] = iCitizenCount;
            }
        }

        public static void ReleaseBuilding(ushort buildingId)
        {
            lock (s_lock)
            {
                if (s_buildingCitizenCount.ContainsKey(buildingId))
                {
                    s_buildingCitizenCount.Remove(buildingId);
                }
            }
        }
    }
}
