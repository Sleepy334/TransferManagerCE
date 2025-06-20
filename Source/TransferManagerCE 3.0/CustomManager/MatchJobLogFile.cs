﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using static TransferManagerCE.CustomManager.TransferRestrictions;
using static TransferManagerCE.Settings.ModSettings;

namespace TransferManagerCE.CustomManager
{
    public class MatchJobLogFile
    {
        // -------------------------------------------------------------------------------------------
        private enum LogLevel
        {
            Trace,
            Debug,
            Info,
            Warning,
            Error,
        }

        private CustomTransferReason.Reason m_material;
        private LogCandidates m_candidateLogging = LogCandidates.All;
        Dictionary<ExclusionReason, int> m_candidateReasons = new Dictionary<ExclusionReason, int>();
        private string LogFilePath;
        private StreamWriter? m_streamWriter = null;
        private Dictionary<InstanceID, string> m_incomingOfferDescriptions = new Dictionary<InstanceID, string>();
        private Dictionary<InstanceID, string> m_outgoingOfferDescriptions = new Dictionary<InstanceID, string>();
        private bool m_bScaleByPriority = false;
        private bool m_bDisplayNode = false;

        // Shared variables
        private static readonly object LogLock = new object();
        private static string s_path = "";
        private static int s_iLogFileNumber = 1;

        // -------------------------------------------------------------------------------------------
        public MatchJobLogFile(CustomTransferReason.Reason material)
        {
            m_material = material;
            m_candidateLogging = (LogCandidates)GetSettings().MatchLogCandidates;
            m_bScaleByPriority = TransferManagerModes.IsScaleByPriority(m_material);
            m_bDisplayNode = PathDistanceTypes.GetDistanceAlgorithm(material) != PathDistanceTypes.PathDistanceAlgorithm.LineOfSight;

            try
            {
                lock (LogLock)
                {
                    if (string.IsNullOrEmpty(s_path))
                    {
                        string dir = Path.Combine(UserSettingsDir, "TransferManagerCE");

                        // Check if the folder exists
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }

                        dir = Path.Combine(dir, $"Match Logging {DateTime.Now.ToString("yyyyMMdd-HHmm")}");

                        // Check if the folder exists
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }

                        s_path = dir;
                    }
                }

                // Set this log file name
                LogFilePath = Path.Combine(s_path, $"{s_iLogFileNumber++.ToString("000000")}-{CustomTransferDispatcher.Instance.Cycle.ToString("00000")}-{m_material}.txt");

                if (!string.IsNullOrEmpty(LogFilePath))
                {
                    m_streamWriter = new StreamWriter(LogFilePath);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
                LogFilePath = "";
            }
        }

        // -------------------------------------------------------------------------------------------
        public void Close()
        {
            if (m_streamWriter is not null)
            {
                m_streamWriter.Close();
            }
        }

        // -------------------------------------------------------------------------------------------
        public void LogInfo(string msg)
        {
            LogToFile(msg, LogLevel.Info);
        }

        // -------------------------------------------------------------------------------------------
        public void LogWarning(string msg)
        {
            LogToFile(msg, LogLevel.Warning);
        }

        // -------------------------------------------------------------------------------------------
        public void LogError(string msg)
        {
            LogToFile(msg, LogLevel.Error);
        }

        // -------------------------------------------------------------------------------------------
        public bool LogCandidate(ExclusionReason reason)
        {
            bool bLogCandidate = true;
            switch (m_candidateLogging)
            {
                case LogCandidates.All:
                    {
                        bLogCandidate = true;
                        break;
                    }
                case LogCandidates.Valid:
                    {
                        bLogCandidate = reason == ExclusionReason.None;
                        break;
                    }
                case LogCandidates.Excluded:
                    {
                        bLogCandidate = reason != ExclusionReason.None;
                        break;
                    }
                case LogCandidates.None:
                    {
                        bLogCandidate = false;
                        break;
                    }
            }
            return bLogCandidate;
        }

        // -------------------------------------------------------------------------------------------
        public void RecordCandidateReason(ExclusionReason reason)
        {
            // Record reason for failure for summary
            if (m_candidateReasons is not null)
            {
                if (m_candidateReasons.ContainsKey(reason))
                {
                    m_candidateReasons[reason]++;
                }
                else
                {
                    m_candidateReasons[reason] = 1;
                }
            }
        }

        // -------------------------------------------------------------------------------------------
        public void ClearCandidateReasons()
        {
            // Record reason for failure for summary
            if (m_candidateReasons is not null)
            {
                m_candidateReasons.Clear();
            }
        }

        // -------------------------------------------------------------------------------------------
        public void LogCandidateSummary()
        {
            if (m_candidateReasons.Count > 0)
            {
                StringBuilder stringBuilder = new StringBuilder("       Candidate Exclusion Summary - |");
                foreach (KeyValuePair<ExclusionReason, int> kvp in m_candidateReasons)
                {
                    stringBuilder.Append($" {kvp.Key}:{kvp.Value} |");
                }
                LogInfo(stringBuilder.ToString());

                // Clear candidate reasons ready for next match
                ClearCandidateReasons();
            }
        }

        // -------------------------------------------------------------------------------------------
        public void LogMatch(CustomTransferOffer incomingOffer, CustomTransferOffer outgoingOffer, int deltaamount)
        {
            LogInfo($"       ### Match Found ### Material:{m_material} Amount:{deltaamount} Distance:{TransferManagerUtils.GetDistanceKm(incomingOffer, outgoingOffer)}km");
            LogInfo("       - " + GetOfferDescription(incomingOffer));
            LogInfo("       - " + GetOfferDescription(outgoingOffer));
        }

        // -------------------------------------------------------------------------------------------
        public void LogHeader(TransferJob job)
        {
            LogInfo($"------ TRANSFER JOB: {new CustomTransferReason(job.material)}, IN: {job.m_incomingCountRemaining}/{job.m_incomingAmount} OUT: {job.m_outgoingCountRemaining}/{job.m_outgoingAmount} (Thread#:{Thread.CurrentThread.ManagedThreadId}) ------");
            
            for (int i = 0; i < job.m_incomingCount; i++)
            {
                CustomTransferOffer incomingOffer = job.m_incomingOffers[i];
                LogInfo($"#{i.ToString("0000")} | {GetOfferDescription(incomingOffer)} |");
            }

            // Add separator
            LogSeparator();

            for (int i = 0; i < job.m_outgoingCount; i++)
            {
                CustomTransferOffer outgoingOffer = job.m_outgoingOffers[i];
                LogInfo($"#{i.ToString("0000")} | {GetOfferDescription(outgoingOffer)} |");
            }

            LogSeparator();
        }

        // -------------------------------------------------------------------------------------------
        public void LogFooter(TransferJob job, int iMatches, long jobMatchTime)
        {
            LogSeparator();
            LogInfo($"Match Job Complete - IN Remaining: {job.m_incomingCountRemaining}/{job.m_incomingAmount} OUT Remaining: {job.m_outgoingCountRemaining}/{job.m_outgoingAmount} Matches: {iMatches} Elapsed time: {(jobMatchTime * 0.0001).ToString("F")}ms");
            LogInfo("\r\nKey:");
            LogInfo("DT = Death Timer");
            LogInfo("IT = Incoming Timer");
            LogInfo("OT = Outgoing Timer");
            LogInfo("ST = Sick Timer");
            LogInfo("WT = Worker Timer");
            LogInfo("DistanceLOS = Distance Line Of Sight");
            LogInfo("DistrictR = District Restrictions");
            LogInfo("BuildingR = Building Restrictions");
        }

        // -------------------------------------------------------------------------------------------
        public void LogCandidatePathDistance(int iIndex, CustomTransferOffer offer, CustomTransferOffer candidateOffer, ExclusionReason reason)
        {
            // Record reason for failure for summary
            RecordCandidateReason(reason);

            // Log candidate if requested
            if (LogCandidate(reason))
            {
                LogInfo($"       -> | #{iIndex.ToString("0000")} | {GetOfferDescription(candidateOffer)} | DistanceLOS:{TransferManagerUtils.GetDistanceKm(offer, candidateOffer)}km | Exclusion:{reason}");
            }
        }

        // -------------------------------------------------------------------------------------------
        public void LogCandidateDistanceLOS(int iIndex, CustomTransferOffer offer, CustomTransferOffer candidateOffer, ExclusionReason reason, bool bConnectedMode)
        {
            // Record reason for failure for summary
            RecordCandidateReason(reason);

            // Log candidate if requested
            if (LogCandidate(reason))
            {
                LogInfo($"       -> | #{iIndex.ToString("0000")} {GetOfferDescription(candidateOffer)} | DistanceLOS:{TransferManagerUtils.GetDistanceKm(offer, candidateOffer)}km |" + (m_bScaleByPriority ? $" PriorityFactor:{candidateOffer.GetPriorityFactor(m_material).ToString("0.0000000")} | " : "") + $" Exclusion: {reason}");
            }
        }

        // -------------------------------------------------------------------------------------------
        public void LogCandidateLOS(int iIndex, CustomTransferOffer offer, CustomTransferOffer candidateOffer, ExclusionReason reason, bool bConnectedMode)
        {
            // Record reason for failure for summary
            RecordCandidateReason(reason);

            // Log candidate if requested
            if (LogCandidate(reason))
            {
                LogInfo($"       -> | #{iIndex.ToString("0000")} {GetOfferDescription(candidateOffer)} | DistanceLOS:{TransferManagerUtils.GetDistanceKm(offer, candidateOffer)}km |" + (m_bScaleByPriority ? $" PriorityFactor:{candidateOffer.GetPriorityFactor(m_material).ToString("0.0000000")} | " : "") + $" | Exclusion: {reason}");
            }
        }

        // -------------------------------------------------------------------------------------------
        public void LogCandidatePriority(int iIndex, CustomTransferOffer candidateOffer, ExclusionReason reason)
        {
            // Record reason for failure for summary
            RecordCandidateReason(reason);

            // Log candidate if requested
            if (LogCandidate(reason))
            {
                LogInfo($"       -> | #{iIndex.ToString("0000")} | {GetOfferDescription(candidateOffer)} | Exclusion:{reason}");
            }
        }

        // -------------------------------------------------------------------------------------------
        public void LogElapsedTime(string strText, long elapsedTicks)
        {
            LogInfo($"{strText} Elapsed Time: {(elapsedTicks * 0.0001).ToString("F")}ms");
        }

        // -------------------------------------------------------------------------------------------
        public void LogSeparator()
        {
            LogInfo("------------------------------------------------------------------------------------------------------------------------");
        }

        // -------------------------------------------------------------------------------------------
        private void LogToFile(string log, LogLevel level)
        {
            if (m_streamWriter is not null)
            {
                m_streamWriter.WriteLine(log);

                if (level == LogLevel.Warning || level == LogLevel.Error)
                {
                    m_streamWriter.WriteLine(new StackTrace(true).ToString());
                    m_streamWriter.WriteLine();
                }
            }
        }

        // -------------------------------------------------------------------------------------------
        private string GetOfferDescription(CustomTransferOffer offer)
        {
            if (offer.IsIncoming())
            {
                if (m_incomingOfferDescriptions.ContainsKey(offer.m_object))
                {
                    return m_incomingOfferDescriptions[offer.m_object];
                }
                else
                {
                    string sDescription = TransferManagerUtils.DebugOffer(m_material, offer, true, m_bDisplayNode, true);
                    m_incomingOfferDescriptions[offer.m_object] = sDescription;
                    return sDescription;
                }
            }
            else
            {
                if (m_outgoingOfferDescriptions.ContainsKey(offer.m_object))
                {
                    return m_outgoingOfferDescriptions[offer.m_object];
                }
                else
                {
                    string sDescription = TransferManagerUtils.DebugOffer(m_material, offer, true, m_bDisplayNode, true);
                    m_outgoingOfferDescriptions[offer.m_object] = sDescription;
                    return sDescription;
                }
            }
        }
    }
}
