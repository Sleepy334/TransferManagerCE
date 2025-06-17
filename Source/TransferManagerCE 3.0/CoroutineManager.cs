using SleepyCommon;
using System;
using System.Collections;
using TransferManagerCE.Util;
using UnityEngine;

namespace TransferManagerCE
{
    public class ModManager : MonoBehaviour
    {
        const int iPATH_UNIT_UPDATE_RATE = 60; // 1 minute
        const int iSETTINGS_CHECK_UPDATE_RATE = 5; // 5 seconds
        const int iPATH_NODE_CACHE_UPDATE_RATE = 300; // 5 minutes
        const int iPATH_FAILURE_UPDATE_RATE = 30; // 30 seconds

        // Path unit warning check
        private Coroutine? m_pathUnitCoroutine = null;
        private bool m_bShownPathUnitWarning = false;

        // Check if districts/buildings have been deleted
        private Coroutine? m_settingsCoroutine = null;

        // Remove old entries from path failure array
        private Coroutine? m_pathFailureCoroutine = null;

        public void Start()
        {
            try
            {
                if (m_pathUnitCoroutine is null)
                {
                    m_pathUnitCoroutine = StartCoroutine(UpdatePathUnitCoroutine(iPATH_UNIT_UPDATE_RATE));
                }

                if (m_settingsCoroutine is null)
                {
                    m_settingsCoroutine = StartCoroutine(UpdateSettingsCoroutine(iSETTINGS_CHECK_UPDATE_RATE));
                }

                if (m_pathFailureCoroutine is null)
                {
                    m_pathFailureCoroutine = StartCoroutine(UpdatePathFailureCoroutine(iPATH_FAILURE_UPDATE_RATE));
                }
            }
            catch (Exception e)
            {
                CDebug.Log("Exception: " + e.Message);
            }
        }

        public void OnDestroy()
        {
            try
            {
                if (m_pathUnitCoroutine is null)
                {
                    StopCoroutine(m_pathUnitCoroutine);
                    m_pathUnitCoroutine = null;
                }

                if (m_settingsCoroutine is null)
                {
                    StopCoroutine(m_settingsCoroutine);
                    m_settingsCoroutine = null;
                }

                if (m_pathFailureCoroutine is null)
                {
                    StopCoroutine(m_pathFailureCoroutine);
                    m_pathFailureCoroutine = null;
                }
            }
            catch (Exception e)
            {
                CDebug.Log("Exception: " + e.Message);
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

        IEnumerator UpdateSettingsCoroutine(int seconds)
        {
            while (true)
            {
                yield return new WaitForSeconds(seconds);

                // Call update incase the settings have been invalidated
                BuildingSettingsStorage.Update();
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