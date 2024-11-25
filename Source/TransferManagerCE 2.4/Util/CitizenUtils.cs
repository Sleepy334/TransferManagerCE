using ColossalFramework;
using System;
using UnityEngine;

namespace TransferManagerCE
{
    public static class CitizenUtils
    {
        
        public delegate bool CitizenDelegate(uint citizenID, Citizen citizen); // Return true to continue loop
        public delegate void CitizenUnitDelegate(uint citizenUnitId, CitizenUnit citizenUnit);

        public static void EnumerateCitizens(uint citizenUnitId, CitizenDelegate func)
        {
            Citizen[] Citizens = Singleton<CitizenManager>.instance.m_citizens.m_buffer;
            CitizenUnit[] CitizenUnits = Singleton<CitizenManager>.instance.m_units.m_buffer;
            int iMaxLength = CitizenUnits.Length;
            int iLoopCount = 0;
            while (citizenUnitId != 0)
            {
                CitizenUnit citizenUnit = CitizenUnits[citizenUnitId];
                for (int i = 0; i < 5; ++i)
                {
                    uint cim = citizenUnit.GetCitizen(i);
                    if (cim != 0)
                    {
                        Citizen citizen = Citizens[cim];

                        // Call delegate for citizen
                        if (!func(cim, citizen))
                        {
                            return;
                        }
                    }
                }

                citizenUnitId = citizenUnit.m_nextUnit;

                // Check for bad list
                if (++iLoopCount > iMaxLength)
                {

                    CODebugBase<LogChannel>.Error(LogChannel.Core, $"Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
        }

        public static void EnumerateCitizenUnits(uint citizenUnitId, CitizenUnitDelegate func)
        {
            CitizenUnit[] CitizenUnits = Singleton<CitizenManager>.instance.m_units.m_buffer;
            int iMaxLength = CitizenUnits.Length;
            int iLoopCount = 0;
            while (citizenUnitId != 0)
            {
                CitizenUnit citizenUnit = CitizenUnits[citizenUnitId];

                // Return citizen
                func(citizenUnitId, citizenUnit);

                citizenUnitId = citizenUnit.m_nextUnit;

                // Check for bad list
                if (++iLoopCount > iMaxLength)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
        }
    }
}
