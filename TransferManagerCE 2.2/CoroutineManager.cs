using System;
using System.Collections;
using TransferManagerCE.Util;
using UnityEngine;

namespace TransferManagerCE
{
    public class ModManager : MonoBehaviour
    {
        const int iPATH_UNIT_UPDATE_RATE = 60; // 1 minute
        const int iDISTRICT_CHECK_UPDATE_RATE = 5; // 5 seconds
        const int iPATH_NODE_CACHE_UPDATE_RATE = 300; // 5 minutes
        const int iPATH_FAILURE_UPDATE_RATE = 30; // 30 seconds

        // Path unit warning check
        private Coroutine? m_pathUnitCoroutine = null;
        private bool m_bShownPathUnitWarning = false;

        // Check if districts have been deleted
        private Coroutine? m_districtCoroutine = null;
        private int m_iDistrictCount = 0;
        private int m_iParkCount = 0;

        // Update outside connection cache
        private Coroutine? m_outsideCacheCoroutine = null;

        // Remove old entries from path failure array
        private Coroutine? m_pathFailureCoroutine = null;

        public void Start()
        {
            try
            {
                if (m_pathUnitCoroutine == null)
                {
                    m_pathUnitCoroutine = StartCoroutine(UpdatePathUnitCoroutine(iPATH_UNIT_UPDATE_RATE));
                }

                if (m_districtCoroutine == null)
                {
                    m_districtCoroutine = StartCoroutine(UpdateDistrictCoroutine(iDISTRICT_CHECK_UPDATE_RATE));
                }

                if (m_outsideCacheCoroutine == null)
                {
                    m_outsideCacheCoroutine = StartCoroutine(UpdateOutsideCacheCoroutine(iPATH_NODE_CACHE_UPDATE_RATE));
                }

                if (m_pathFailureCoroutine == null)
                {
                    m_pathFailureCoroutine = StartCoroutine(UpdatePathFailureCoroutine(iPATH_FAILURE_UPDATE_RATE));
                }
            }
            catch (Exception e)
            {
                Debug.Log("Exception: " + e.Message);
            }
        }

        public void OnDestroy()
        {
            try
            {
                if (m_pathUnitCoroutine == null)
                {
                    StopCoroutine(m_pathUnitCoroutine);
                    m_pathUnitCoroutine = null;
                }

                if (m_districtCoroutine == null)
                {
                    StopCoroutine(m_districtCoroutine);
                    m_districtCoroutine = null;
                }

                if (m_outsideCacheCoroutine == null)
                {
                    StopCoroutine(m_outsideCacheCoroutine);
                    m_outsideCacheCoroutine = null;
                }

                if (m_pathFailureCoroutine == null)
                {
                    StopCoroutine(m_pathFailureCoroutine);
                    m_pathFailureCoroutine = null;
                }
            }
            catch (Exception e)
            {
                Debug.Log("Exception: " + e.Message);
            }
        }

        IEnumerator UpdatePathUnitCoroutine(int seconds)
        {
            while (true)
            {
                yield return new WaitForSeconds(seconds);
                CheckPathUnits();
            }
        }

        private void CheckPathUnits()
        {
            const double dWarningPercent = 0.95;
            if (!m_bShownPathUnitWarning && PathManager.instance.m_pathUnits.m_size * dWarningPercent <= PathManager.instance.m_pathUnitCount)
            {
                string sMessage = "Path Units used has hit " + (dWarningPercent * 100) + "% of available amount.\r\n";
                sMessage += "Current: " + PathManager.instance.m_pathUnitCount + "\r\n";
                sMessage += "Available: " + PathManager.instance.m_pathUnits.m_size + "\r\n";
                sMessage += "This is a game limit, not a mod limit, once you run out of path units vehicles will no longer be able to find paths.\r\n";
                sMessage += "You could try running the \"More Path Units\" mod by algernon to increase the available path units for the game.";
                Prompt.WarningFormat("Transfer Manager CE", sMessage);
                m_bShownPathUnitWarning = true;
            }
        }

        IEnumerator UpdateDistrictCoroutine(int seconds)
        {
            while (true)
            {
                yield return new WaitForSeconds(seconds);
                CheckDistricts();
            }
        }

        private void CheckDistricts()
        {
            // Check if districts changed
            int iNewDistrictCount = DistrictManager.instance.m_districtCount;
            int iNewParkCount = DistrictManager.instance.m_parkCount;
            if (iNewDistrictCount != m_iDistrictCount || m_iParkCount != iNewParkCount)
            {
                m_iDistrictCount = iNewDistrictCount;
                m_iParkCount = iNewParkCount;
                BuildingSettingsStorage.ValidateSettings();
            }
        }

        IEnumerator UpdateOutsideCacheCoroutine(int seconds)
        {
            while (true)
            {
                yield return new WaitForSeconds(seconds);

                // Clear outside connection node cache periodically in case they change
                PathNodeCache.InvalidateOutsideConnections();
            }
        }

        IEnumerator UpdatePathFailureCoroutine(int seconds)
        {
            while (true)
            {
                yield return new WaitForSeconds(seconds);

                // clean pathfind LRU
                PathFindFailure.RemoveOldEntries();
            }
        }
    }
}