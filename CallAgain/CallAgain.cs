using System.Collections.Generic;
using System.Diagnostics;
using TransferManagerCE.CustomManager;
using TransferManagerCE.Settings;
using static TransferManager;

namespace TransferManagerCE
{
    public struct CallAgainData
    {
        public int m_iTimer;
        public int m_iRetries;
    }

    public class CallAgain
    {
        public static int m_sickIssues = 0;
        public static int m_deadIssues = 0; 
        private Dictionary<ushort, CallAgainData> m_Healthcalls = new Dictionary<ushort, CallAgainData>();
        private Dictionary<ushort, CallAgainData> m_Deathcalls = new Dictionary<ushort, CallAgainData>();

        public CallAgain()
        {

        }

        public CallAgainData? GetHealthCallbackData(ushort buildingId)
        {
            if (m_Healthcalls.ContainsKey(buildingId))
            {
                return m_Healthcalls[buildingId];
            } else
            {
                return null;
            }
        }

        public CallAgainData? GetDeathCallbackData(ushort buildingId)
        {
            if (m_Healthcalls.ContainsKey(buildingId))
            {
                return m_Deathcalls[buildingId];
            }
            else
            {
                return null;
            }
        }

        public void Update(Stopwatch watch)
        {
#if DEBUG
            long lStartTime = watch.ElapsedMilliseconds;
#endif
            // New health calls array
            Dictionary<ushort, CallAgainData> newHealthcalls = new Dictionary<ushort, CallAgainData>();
            Dictionary<ushort, CallAgainData> newDeathcalls = new Dictionary<ushort, CallAgainData>();

            List<TransferOffer> sickList = new List<TransferOffer>();
            List<TransferOffer> deadList = new List<TransferOffer>();

            // Find all sick buildings
            m_sickIssues = 0;
            m_deadIssues = 0;

            for (int i = 0; i < BuildingManager.instance.m_buildings.m_buffer.Length; i++)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[i];

                sickList.AddRange(CheckHealthTimer((ushort)i, building, newHealthcalls));
                deadList.AddRange(CheckDeathTimer((ushort)i, building, newDeathcalls));
            }

            AddOutgoingOffersCheckExisting(TransferReason.Sick, sickList);
            AddOutgoingOffersCheckExisting(TransferReason.Dead, deadList);

            // Replace health calls with the new list
            m_Healthcalls = newHealthcalls;
            m_Deathcalls = newDeathcalls;
#if DEBUG
            long lStopTime = watch.ElapsedMilliseconds;
            Debug.Log("CallAgain - Execution Time: " + (lStopTime - lStartTime) + "ms");
#endif
        }

        public static void AddOutgoingOffersCheckExisting(TransferReason material, List<TransferOffer> offers)
        {
            // Now check the transfer offers arent already in Transfer Manager before sending offers.
            offers = TransferManagerUtils.RemoveExisitingOutgoingOffers(material, offers);
#if DEBUG
            string sMessage = "";
#endif
            foreach (TransferOffer offer in offers)
            {
#if DEBUG
                sMessage += $"\r\n{CustomTransferManager.DebugInspectOffer2(offer)}";
#endif
                TransferManager.instance.AddOutgoingOffer(material, offer);
            }
#if DEBUG
            if (sMessage.Length > 0)
            {
                Debug.Log("CALL AGAIN - Adding transfer offers for " + material + ": " + sMessage);
            }
#endif
        }

        public List<TransferOffer> CheckHealthTimer(ushort usBuilding, Building building, Dictionary<ushort, CallAgainData> newHealthcalls)
        {
            List<TransferOffer> list = new List<TransferOffer>();

            if (building.m_healthProblemTimer > ModSettings.GetSettings().HealthcareThreshold)
            {
                List<uint> cimSick = CitiesUtils.GetSickCitizens(usBuilding, building);
                if (cimSick.Count > 0)
                {
                    m_sickIssues++;
                    
                    // Only call again if there arent ambulances on the way? and we have passed call again rate
                    int iLastCallTimer = 0;
                    int iRetries = 0;
                    if (m_Healthcalls.ContainsKey(usBuilding))
                    {
                        iLastCallTimer = m_Healthcalls[usBuilding].m_iTimer;
                        iRetries = m_Healthcalls[usBuilding].m_iRetries;
                    }

                    // Call if no ambulances on the way and it has been HealthcareRate since last time we called
                    if ((building.m_healthProblemTimer - iLastCallTimer) > ModSettings.GetSettings().HealthcareRate && CitiesUtils.GetAmbulancesOnRoute(usBuilding).Count == 0)
                    {
                        // Create outgoing offers for each
                        foreach (uint cim in cimSick)
                        {
                            TransferOffer offer = new TransferOffer();
                            offer.Citizen = cim;
                            offer.Amount = 1;
                            offer.Priority = 7; // Highest
                            offer.Position = building.m_position;
                            list.Add(offer);
                        }

                        iLastCallTimer = building.m_healthProblemTimer;
                        iRetries++;
                    }

                    CallAgainData data = new CallAgainData();
                    data.m_iTimer = iLastCallTimer;
                    data.m_iRetries = iRetries;
                    newHealthcalls.Add(usBuilding, data);
                }
            }

            return list;
        }

        public List<TransferOffer> CheckDeathTimer(ushort usBuilding, Building building, Dictionary<ushort, CallAgainData> newDeathcalls)
        {
            List <TransferOffer> list = new List<TransferOffer>();

            if (building.m_deathProblemTimer > ModSettings.GetSettings().DeathcareThreshold)
            {
                List<uint> cimDead = CitiesUtils.GetDeadCitizens(usBuilding, building);
                if (cimDead.Count > 0)
                {
                    m_deadIssues++;

                    // Only call again if there arent ambulances on the way? and we have passed call again rate
                    int iLastCallTimer = 0;
                    int iRetries = 0;
                    if (m_Deathcalls.ContainsKey(usBuilding))
                    {
                        iLastCallTimer = m_Deathcalls[usBuilding].m_iTimer;
                        iRetries = m_Deathcalls[usBuilding].m_iRetries;
                    }

                    // Call if no ambulances on the way and it has been DeathcareRate since last time we called
                    if ((building.m_deathProblemTimer - iLastCallTimer) > ModSettings.GetSettings().DeathcareRate && CitiesUtils.GetHearsesOnRoute(usBuilding).Count == 0)
                    {
                        // Create outgoing offers for each
                        foreach (uint cim in cimDead)
                        {
                            TransferOffer offer = new TransferOffer();
                            offer.Citizen = cim;
                            offer.Amount = 1;
                            offer.Priority = 7; // Highest
                            offer.Position = building.m_position;
                            list.Add(offer);
                        }

                        iLastCallTimer = building.m_healthProblemTimer;
                        iRetries++;
                    }

                    CallAgainData data = new CallAgainData();
                    data.m_iTimer = iLastCallTimer;
                    data.m_iRetries = iRetries;
                    newDeathcalls.Add(usBuilding, data);
                }
            }

            return list;
        }
    }
}