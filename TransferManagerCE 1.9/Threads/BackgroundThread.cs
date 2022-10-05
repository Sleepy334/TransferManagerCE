using ICities;
using System.Diagnostics;
using TransferManagerCE.Settings;
using TransferManagerCE.Util;
using static TransferManager;

namespace TransferManagerCE
{
    public class BackgroundThread : ThreadingExtensionBase
    {
        const int iSTUCK_VEHICLE_UPDATE_RATE = 10000;// 10 seconds
        const int iPATH_UNIT_UPDATE_RATE = 60000; // 1 minute
        const int iDISTRICT_CHECK_UPDATE_RATE = 5000; // 5 seconds
        const int iPATH_FAILURE_UPDATE_RATE = 1000; // 1 seconds

        private long m_LastStuckVehicleElapsedTime = 0;
        private long m_LastCheckPathUnitsElapsedTime = 0;
        private long m_LastCheckDistrictElapsedTime = 0;
        private long m_LastPathFailureElapsedTime = 0;

        private Stopwatch? m_watch = null;

        private static bool s_bShownPathUnitWarning = false;
        private static int s_iDistrictCount = 0;
        private static int s_iParkCount = 0;

        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            if (!TransferManagerLoader.IsLoaded())
            {
                return;
            }

            if (m_watch == null)
            {
                m_watch = new Stopwatch();
            }

            // Update panel
            if (SimulationManager.instance.SimulationPaused)
            {
                if (m_watch.IsRunning)
                {
                    m_watch.Stop();
                    m_LastStuckVehicleElapsedTime = 0;
                    m_LastCheckPathUnitsElapsedTime = 0;
                    m_LastCheckDistrictElapsedTime = 0;
                    m_LastPathFailureElapsedTime = 0;
                }
            }
            else
            {
                if (!m_watch.IsRunning)
                {
                    m_watch.Start();
                    m_LastStuckVehicleElapsedTime = m_watch.ElapsedMilliseconds;
                    m_LastCheckPathUnitsElapsedTime = m_watch.ElapsedMilliseconds;
                    m_LastCheckDistrictElapsedTime = m_watch.ElapsedMilliseconds;
                    m_LastPathFailureElapsedTime = m_watch.ElapsedMilliseconds;
                }
#if DEBUG
                long lStartTime = m_watch.ElapsedMilliseconds;
#endif

#if DEBUG
                // Check for stuck vehicles
                if ((m_watch.ElapsedMilliseconds - m_LastStuckVehicleElapsedTime) > iSTUCK_VEHICLE_UPDATE_RATE)
                {
                    // Check for stuck vehciles
                    StuckVehicles.CheckStuckVehicles();
                    m_LastStuckVehicleElapsedTime = m_watch.ElapsedMilliseconds;
                }
#endif

                // Check path units
                if ((m_watch.ElapsedMilliseconds - m_LastCheckPathUnitsElapsedTime) > iPATH_UNIT_UPDATE_RATE)
                {
                    CheckPathUnits();
                    //Debug.Log("Path units: " + PathManager.instance.m_pathUnitCount + " Size: " + PathManager.instance.m_pathUnits.m_size);
                    m_LastCheckPathUnitsElapsedTime = m_watch.ElapsedMilliseconds;
                }

                // Check for deleted districts
                if ((m_watch.ElapsedMilliseconds - m_LastCheckDistrictElapsedTime) > iDISTRICT_CHECK_UPDATE_RATE)
                {
                    CheckDistricts();
                    m_LastCheckDistrictElapsedTime = m_watch.ElapsedMilliseconds;
                }

                if ((m_watch.ElapsedMilliseconds - m_LastPathFailureElapsedTime) > iPATH_FAILURE_UPDATE_RATE)
                {
                    // clean pathfind LRU
                    PathFindFailure.RemoveOldEntries();
                    m_LastPathFailureElapsedTime = m_watch.ElapsedMilliseconds;
                }

#if DEBUG
                long lExecutionTime = m_watch.ElapsedMilliseconds - lStartTime;
                if (lExecutionTime > 0)
                {
                    Debug.Log("Background thread - Execution Time: " + lExecutionTime + "ms");
                }
#endif
            }
        }

        private void CheckPathUnits()
        {
            const double dWarningPercent = 0.95;
            if (!s_bShownPathUnitWarning && PathManager.instance.m_pathUnits.m_size * dWarningPercent <= PathManager.instance.m_pathUnitCount)
            {
                string sMessage = "Path Units used has hit " + (dWarningPercent * 100) + "% of available amount.\r\n";
                sMessage += "Current: " + PathManager.instance.m_pathUnitCount + "\r\n";
                sMessage += "Available: " + PathManager.instance.m_pathUnits.m_size + "\r\n";
                sMessage += "This is a game limit, not a mod limit, once you run out of path units vehicles will no longer be able to find paths.\r\n";
                sMessage += "You could try running the \"More Path Units\" mod by algernon to increase the available path units for the game.";
                Prompt.WarningFormat("Transfer Manager CE", sMessage);
                s_bShownPathUnitWarning = true;
            }
        }

        private void CheckDistricts()
        {
            // Check if districts changed
            int iNewDistrictCount = DistrictManager.instance.m_districtCount;
            int iNewParkCount = DistrictManager.instance.m_parkCount;
            if (iNewDistrictCount != s_iDistrictCount || s_iParkCount != iNewParkCount)
            {
                s_iDistrictCount = iNewDistrictCount;
                s_iParkCount = iNewParkCount;
                BuildingSettings.ValidateSettings();
            }
        }

        public override void OnReleased()
        {
            base.OnReleased();
        }
    }
}
