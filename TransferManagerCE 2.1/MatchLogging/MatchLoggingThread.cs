using ColossalFramework;
using ICities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEngine;

namespace TransferManagerCE
{
    public class MatchLoggingThread
    {
        // About 10MB in file size
        const int iFILE_BUFFER_MAX_COUNT = 500000; 

        private static volatile bool s_runThread = true;
        private static Thread? s_matchLoggingThread = null;
        private static EventWaitHandle? s_waitHandle = null;

        private const string LOG_FILE_NAME = "00MatchLogging.bin";
        private string m_LogFilePath;
        private int m_iMatchesWritten = 0;
        private long m_FileOffset = 0;

        private static Queue<MatchData> s_matchBuffer = new Queue<MatchData>();
        static readonly object s_BufferLock = new object();

        private static ushort s_requestMatchesBuildingId;

        public static void StartThread()
        {
            if (s_matchLoggingThread == null)
            {
                s_runThread = true;

                // AutoResetEvent releases 1 thread only each time Set() is called.
                s_waitHandle = new AutoResetEvent(false);

                s_matchLoggingThread = new Thread(new MatchLoggingThread().ThreadMain);
                s_matchLoggingThread.IsBackground = true;
                s_matchLoggingThread.Start();
            }
        }

        public static void StopThread()
        {
            s_runThread = false;
            if (s_waitHandle != null)
            {
                s_waitHandle.Set();
            }
            s_waitHandle = null;
        }

        public static void RequestMatchesForBuilding(ushort buildingId)
        {
            s_requestMatchesBuildingId = buildingId;
            if (s_requestMatchesBuildingId != 0 && s_waitHandle != null)
            {
                s_waitHandle.Set();
            }
        }

        public static void AddMatchToBuffer(MatchData data)
        {
            lock (s_BufferLock)
            {
                s_matchBuffer.Enqueue(data);
            }
            if (s_waitHandle != null)
            {
                s_waitHandle.Set();
            }
        }

        public void ThreadMain()
        {
            SimulationManager instance = Singleton<SimulationManager>.instance;

            using (FileStream fs = new FileStream(m_LogFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Delete)) 
            {
                while (s_runThread)
                {
                    // Wait for event to trigger
                    if (s_waitHandle != null)
                    {
                        s_waitHandle.WaitOne();
                    }

                    // Check we should keep running
                    if (s_runThread)
                    {
                        // Write any waiting matches to the file
                        WriteMatchesToFile(fs);

                        // Read matches from file if requested
                        if (s_requestMatchesBuildingId != 0)
                        {
                            ushort buildingId = s_requestMatchesBuildingId;
                            s_requestMatchesBuildingId = 0;

                            List<BuildingMatchData> matches = ReadMatchesFromFile(buildingId, fs);
                            if (BuildingPanel.Instance != null && BuildingPanel.Instance.isVisible)
                            {
                                BuildingPanel.Instance.GetBuildingMatches().AddMatches(buildingId, matches);
                            }
                        }
                    }
                }
            }
        }

        public MatchLoggingThread()
        {
            try
            {
                m_LogFilePath = Path.Combine(Application.dataPath, LOG_FILE_NAME);
                if (File.Exists(m_LogFilePath))
                {
                    File.Delete(m_LogFilePath); // delete old file to avoid confusion.
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e);
                m_LogFilePath = "";
            }
        }

        private void WriteMatchesToFile(FileStream fs)
        {
            if (fs != null && fs.CanWrite)
            {
                Queue<MatchData>? bufferCopy = null;
                lock (s_BufferLock)
                {
                    if (s_matchBuffer.Count > 0)
                    {
                        bufferCopy = new Queue<MatchData>(s_matchBuffer);
                        s_matchBuffer.Clear();
                    }
                }

                if (bufferCopy != null && bufferCopy.Count > 0)
                {
                    // Write matches to log file
                    fs.Position = m_FileOffset;

                    BinaryWriter writer = new BinaryWriter(fs);
                    foreach (MatchData matchData in bufferCopy)
                    {
                        // We use the file as a ring buffer, looping back to the start after 
                        if (m_iMatchesWritten == iFILE_BUFFER_MAX_COUNT)
                        {
                            fs.Position = 0; // Write from the start again
                            m_iMatchesWritten = 0;
                        }

                        matchData.Write(writer);
                        m_iMatchesWritten++;
                    }

                    // Save location in file
                    m_FileOffset = fs.Position;
                }
            }
        }

        private List<BuildingMatchData> ReadMatchesFromFile(ushort buildingId, FileStream fs)
        {
            List<BuildingMatchData> matches = new List<BuildingMatchData>();

            if (fs.CanRead)
            {
                // Read from start of file each time
                // Note: File is a ring buffer so matches earlier in the file may not be older.
                fs.Position = 0;

                BinaryReader bw = new BinaryReader(fs);
                while (fs.Position < fs.Length)
                {
                    MatchData matchData = new MatchData();
                    matchData.Read(bw);

                    BuildingMatchData? buildingMatch = MatchLogging.GetBuildingMatch(buildingId, matchData);
                    if (buildingMatch != null)
                    {
                        matches.Add(buildingMatch);
                    }
                }
            }

            return matches;
        }
    }
}
