using HarmonyLib;
using SleepyCommon;
using System.Collections.Generic;

namespace TransferManagerCE
{
    public class ILSearch
    {
        private List<CodeInstruction> m_patternList = new List<CodeInstruction>();
        private int m_iSearchIndex = 0;
        private int m_iOccurrance = 1; // How many times do we want to find the sequence before we stop looking
        private int m_iFoundCount = 0;
        private bool m_bSearch = true;
        private bool m_bDebug = false;
        public ILSearch(bool bDebug = false)
        {
            m_bDebug = bDebug;
        }

        public int Occurrance
        {
            get
            {
                return m_iOccurrance;
            }

            set
            {
                m_iOccurrance = value;
            }
        }

        public void AddPattern(CodeInstruction instruction)
        {
            m_patternList.Add(instruction);
        }

        public bool IsFound()
        {
            return !m_bSearch;
        }

        public void NextInstruction(CodeInstruction instruction) 
        {
            if (m_bSearch)
            {
                // Search function
                CodeInstruction search = m_patternList[m_iSearchIndex];
                if (TranspilerUtils.CompareInstructions(instruction, search))
                {
                    m_iSearchIndex++; // Look for next instruction
                    if (m_bDebug) CDebug.Log($"Occurance: {m_iFoundCount} SearchIndex: {m_iSearchIndex} - Found Instruction: {instruction.ToString()}.");
                }
                else
                {
                    m_iSearchIndex = 0;
                    if (m_bDebug) CDebug.Log($"Occurance: {m_iFoundCount} SearchIndex: {m_iSearchIndex} - Not found: {search.ToString()} Instruction: {instruction.ToString()}.");
                }

                // Check if we have found the pattern
                if (m_iSearchIndex == m_patternList.Count)
                {
                    m_iFoundCount++;
                    m_iSearchIndex = 0; // Reset search index
                    if (m_bDebug) CDebug.Log($"Found occurrance {m_iFoundCount}.");

                    if (m_iFoundCount == Occurrance)
                    {
                        // Found location disable searching
                        m_bSearch = false;

                        if (m_bDebug) CDebug.Log($"Search success.");
                    }
                }
            }   
        }
    }
}