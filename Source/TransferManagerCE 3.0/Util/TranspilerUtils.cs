// ----------------------------------------------------------------------------------------
using HarmonyLib;

namespace TransferManagerCE
{
    public class TranspilerUtils
    {
        public static bool CompareInstructions(CodeInstruction instruction1, CodeInstruction instruction2)
        {
            if (instruction1.opcode == instruction2.opcode)
            {
                if (instruction1.operand == null && instruction2.operand == null)
                {
                    return true;
                }
                else if (instruction2.operand == null)
                {
                    // If the second operand is null, just compare OpCodes
                    return true;
                }
                else
                {
                    return instruction1.OperandIs(instruction2.operand);
                }
            }

            return false;
        }
    }
}