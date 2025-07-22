using HarmonyLib;
using SleepyCommon;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class IntercityBusPatch
    {
        private static bool s_bPatched = false;

        // ----------------------------------------------------------------------------------------
        // TransportStationAI.CreateConnectionLines creates 2 sets of lines for intercity bus stations
        [HarmonyPatch(typeof(TransportStationAI), "CreateConnectionLines",
            new[] { typeof(ushort), typeof(Building), typeof(ushort), typeof(Building), typeof(int) },
            new[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> CreateConnectionLinesTranspiler(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            // Have we already patched the function, if so just return unaltered.
            if (s_bPatched)
            {
                CDebug.Log($"ERROR: TransportStationAI.CreateConnectionLines - Already patched!", false);
                return instructions.AsEnumerable();
            }
            s_bPatched = true;

            // Load methods and fields
            FieldInfo fieldInfo = typeof(TransportStationAI).GetField("m_info", BindingFlags.Instance | BindingFlags.Public);
            if (fieldInfo == null)
            {
                CDebug.Log($"ERROR: TransportStationAI.m_info not found");
                return instructions.AsEnumerable();
            }

            MethodInfo methodAddBusLines = typeof(IntercityBusPatch).GetMethod("AddBusLines", BindingFlags.Static | BindingFlags.Public);
            if (methodAddBusLines == null)
            {
                CDebug.Log($"ERROR: IntercityBusPatch.AddBusLines not found");
                return instructions.AsEnumerable();
            }

            // Peform patch, we want to add in an extra check before bus line code.
            // call       static System.Boolean TransferManagerCE.IntercityBusPatch::AddBusLines(TransportStationAI station)
            // brfalse => Label15
            List<CodeInstruction> newInstructionList = new List<CodeInstruction>();

            // Look for:
            // IL_01B0: ldarg.0
            // IL_01B1: ldfld BuildingInfo BuildingAI::m_info
            // IL_01B6: brfalse => Label*
            ILSearch search = new ILSearch();
            search.AddPattern(new CodeInstruction(OpCodes.Ldarg_0));
            search.AddPattern(new CodeInstruction(OpCodes.Ldfld, fieldInfo));
            search.AddPattern(new CodeInstruction(OpCodes.Brfalse));

            bool bPatched = false;

            foreach (CodeInstruction instruction in instructions)
            {
                search.NextInstruction(instruction);

                if (!bPatched && search.IsFound())
                {
                    // Current instruction is the BrFalse with the appropriate label. Add it first
                    newInstructionList.Add(instruction);

                    // Insert our function call here
                    newInstructionList.Add(new CodeInstruction(OpCodes.Ldarg_0)); // this pointer
                    newInstructionList.Add(new CodeInstruction(OpCodes.Call, methodAddBusLines));
                    newInstructionList.Add(instruction); // Add the brFalse again with the label

                    bPatched = true;
                }
                else
                {
                    newInstructionList.Add(instruction);
                }
            }

            CDebug.Log($"TransportStationAI.CreateConnectionLines patch {((bPatched) ? "succeeded" : "failed")}.", false);
            return newInstructionList.AsEnumerable();
        }

        // ----------------------------------------------------------------------------------------
        public static bool AddBusLines(TransportStationAI station)
        {
            // Add bus as well if needed
            bool bAddedBusAlready = false;

            if (UseSecondaryTransportInfoForConnection(station))
            {
                if (station.m_secondaryTransportInfo is not null)
                {
                    bAddedBusAlready = station.m_secondaryTransportInfo.m_transportType == TransportInfo.TransportType.Bus;
                }

            }
            else
            {
                if (station.m_transportInfo is not null)
                {
                    bAddedBusAlready = station.m_transportInfo.m_transportType == TransportInfo.TransportType.Bus;
                }
            }

            return !bAddedBusAlready;
        }

        // ----------------------------------------------------------------------------------------
        private static bool UseSecondaryTransportInfoForConnection(TransportStationAI station)
        {
            return (object)station.m_secondaryTransportInfo != null &&
                station.m_secondaryTransportInfo.m_class.m_subService == station.m_transportLineInfo.m_class.m_subService &&
                station.m_secondaryTransportInfo.m_class.m_level == station.m_transportLineInfo.m_class.m_level;
        }
    }
}
