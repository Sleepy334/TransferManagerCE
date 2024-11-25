using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace SleepyCommon
{
    public class Utils
    {
        public static string GetSortCharacter(bool bDesc)
        {
            string sUnicode;
            if (bDesc)
            {
                sUnicode = "\u25BC";
            }
            else
            {
                sUnicode = "\u25B2";
            }
            return GetUnicodeCharacter(sUnicode);
        }

        public static string GetUnicodeCharacter(string sUnicode)
        {
            return Encoding.UTF8.GetString(Encoding.Convert(Encoding.Unicode, Encoding.UTF8, Encoding.Unicode.GetBytes(sUnicode)));
        }

        public static float GetSimulationTimestamp()
        {
            // DEBUGGING
            /*
            float fTimestamp = 3500 + SimulationManager.instance.m_simulationTimer2;
            if (fTimestamp >= 3600f)
            {
                return fTimestamp - 3600;
            } else
            {
                return fTimestamp;
            }
            */
            return SimulationManager.instance.m_simulationTimer2;
        }

        public static float GetTimeSpan(float fCurrentTime, float fTimeStamp)
        {
            float fTimeSpan;
            if (fCurrentTime < fTimeStamp)
            {
                // Timer2 wraps at 3600.
                fTimeSpan = (3600f - fTimeStamp) + fCurrentTime;
            }
            else
            {
                fTimeSpan = fCurrentTime - fTimeStamp;
            }
            return fTimeSpan;
        }

        public static int GetTimestampDays(float fTimeStamp)
        {
            const int iSIMULATION_TIMER2_DAY_LENGTH = 10; // in game seconds
            float fTimeSpan = GetTimeSpan(GetSimulationTimestamp(), fTimeStamp);
            int iDays = (int)(fTimeSpan / iSIMULATION_TIMER2_DAY_LENGTH);
            return iDays;
        }

        public static string GetLongestLine(string strText, out int iLines)
        {
            string[] result = strText.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.None);
            string sLongest = "";
            iLines = result.Length;
            if (result.Length > 0 && (result[result.Length - 1] == "\r\n" || result[result.Length - 1] == "\n"))
            {
                iLines--;
            }
            foreach (string s in result)
            {
                if (s.Length > sLongest.Length)
                {
                    sLongest = s;
                }
            }
            return sLongest;
        }

        public static float GetStringWidth(UIFontRenderer oRenderer, float fUnits, string sText)
        {
            float fWidth = 0;
            try
            {
                float[] characterWidths = oRenderer.GetCharacterWidths(sText);
                
                for (int index = 0; index < characterWidths.Length; ++index)
                {
                    fWidth += characterWidths[index] / fUnits;
                }
            }
            catch (Exception e)
            {
                TransferManagerCE.Debug.Log("Unable to pad text", e);
            }
            return fWidth;
        }

        public static string PadToWidthFront(UIFontRenderer oRenderer, float fUnits, float fRequiredWidth, string text)
        {
            string sNewText = text;
            string sPadding = " ";

            float fTextWidth = GetStringWidth(oRenderer, fUnits, sNewText);
            float fPadWidth = GetStringWidth(oRenderer, fUnits, sPadding);
            if (fPadWidth > 0)
            {
                while (fTextWidth < fRequiredWidth)
                {
                    sNewText = sPadding + sNewText;
                    fTextWidth += fPadWidth;
                }
            }

            return sNewText;
        }

        public static string PadToWidthBack(UIFontRenderer oRenderer, float fUnits, float fRequiredWidth, string text)
        {
            string sNewText = text;
            string sPadding = " ";

            float fTextWidth = GetStringWidth(oRenderer, fUnits, sNewText);
            float fPadWidth = GetStringWidth(oRenderer, fUnits, sPadding);
            if (fPadWidth > 0)
            {
                while (fTextWidth < fRequiredWidth)
                {
                    sNewText += sPadding;
                    fTextWidth += fPadWidth;
                }
            }

            return sNewText;
        }

        public static List<string> PadToWidthBack(UIFontRenderer oRenderer, float fUnits, List<string> list)
        {
            float fMaxWidth = 0f;
            foreach (string s in list)
            {
                fMaxWidth = Math.Max(fMaxWidth, GetStringWidth(oRenderer, fUnits, s));
            }

            List<string> result = new List<string>();
            foreach (string s in list)
            {
                result.Add(PadToWidthBack(oRenderer, fUnits, fMaxWidth, s));
            }

            return result;
        }

        public static string PadToWidth(string sText, int iWidth, bool bFront)
        {
            while (sText.Length < iWidth)
            {
                if (bFront)
                {
                    sText = " " + sText;
                }
                else
                {
                    sText += " ";
                }

            }
            return sText;
        }

        public static List<string> PadToWidth(List<string> list, bool bFront)
        {
            int iMaxWidth = 0;
            foreach (string s in list)
            {
                iMaxWidth = Math.Max(iMaxWidth, s.Length);
            }

            List<string> result = new List<string>();
            foreach (string s in list)
            {
                result.Add(PadToWidth(s, iMaxWidth, bFront));
            }

            return result;
        }
    }
}
