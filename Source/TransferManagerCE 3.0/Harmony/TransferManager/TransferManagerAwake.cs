using System.Reflection;
using ColossalFramework;
using HarmonyLib;
using SleepyCommon;
using static RenderManager;
using static TransferManager;

namespace TransferManagerCore
{
    [HarmonyPatch]
    public class TransferManagerAwakePatch
    {
        // --------------------------------------------------------------------
        public const int iTransferManagerSettingsVersion = 1; // Data file version
        public const int NEW_TRANSFER_REASON_COUNT = 256;

        // --------------------------------------------------------------------
        private const int PRIORITY_SIZE = 8;
        private const int REASON_ARRAY_SIZE = 256;

        private const int NEW_TRANSFER_OFFER_SIZE = NEW_TRANSFER_REASON_COUNT * PRIORITY_SIZE * REASON_ARRAY_SIZE;
        private const int NEW_TRANSFER_COUNT_SIZE = NEW_TRANSFER_REASON_COUNT * PRIORITY_SIZE;
        private const int NEW_TRANSFER_AMOUNT_SIZE = NEW_TRANSFER_REASON_COUNT;

        // --------------------------------------------------------------------
        public static bool IsTransferReasonArraysPatched()
        {
            // Check all the arrays have the new sizes
            TransferManager instance = Singleton<TransferManager>.instance;

            TransferManager.TransferOffer[] ___m_outgoingOffers = (TransferOffer[])typeof(TransferManager).GetField("m_outgoingOffers", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(instance);
            TransferManager.TransferOffer[] ___m_incomingOffers = (TransferOffer[])typeof(TransferManager).GetField("m_incomingOffers", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(instance);
            ushort[] ___m_outgoingCount = (ushort[])typeof(TransferManager).GetField("m_outgoingCount", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(instance);
            ushort[] ___m_incomingCount = (ushort[])typeof(TransferManager).GetField("m_incomingCount", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(instance);
            int[] ___m_outgoingAmount = (int[])typeof(TransferManager).GetField("m_outgoingAmount", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(instance);
            int[] ___m_incomingAmount = (int[])typeof(TransferManager).GetField("m_incomingAmount", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(instance);

            return ___m_outgoingOffers != null && (___m_outgoingOffers.Length == NEW_TRANSFER_OFFER_SIZE) &&
                    ___m_incomingOffers != null && (___m_incomingOffers.Length == NEW_TRANSFER_OFFER_SIZE) &&
                    ___m_outgoingCount != null && (___m_outgoingCount.Length == NEW_TRANSFER_COUNT_SIZE) &&
                    ___m_incomingCount != null && (___m_incomingCount.Length == NEW_TRANSFER_COUNT_SIZE) &&
                    ___m_outgoingAmount != null && (___m_outgoingAmount.Length == NEW_TRANSFER_AMOUNT_SIZE) &&
                    ___m_incomingAmount != null && (___m_incomingAmount.Length == NEW_TRANSFER_AMOUNT_SIZE);
        }

        // ----------------------------------------------------------------------------------------
        [HarmonyPatch(typeof(TransferManager), "Awake")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.First)]
        public static void AwakePostfix()
        {
            PatchTransferArrays();
        }

        // ----------------------------------------------------------------------------------------
        public static void PatchTransferArraysManually()
        {
            PatchTransferArrays();
        }

        // ----------------------------------------------------------------------------------------
        private static void PatchTransferArrays()
        {
            if (IsTransferReasonArraysPatched())
            {
                return;
            }
#if DEBUG
            CDebug.Log($"Patching Transfer Reason arrays");
#endif

            TransferManager instance = Singleton<TransferManager>.instance;

            // m_outgoingOffers
            {
                FieldInfo info = typeof(TransferManager).GetField("m_outgoingOffers", BindingFlags.Instance | BindingFlags.NonPublic);
                TransferOffer[] m_outgoingOffers = (TransferOffer[])info.GetValue(Singleton<TransferManager>.instance);
                CDebug.Log($"TransferManagerExtended: Resizing m_outgoingOffers from {m_outgoingOffers.Length} to {NEW_TRANSFER_OFFER_SIZE}");
                m_outgoingOffers = new TransferManager.TransferOffer[NEW_TRANSFER_OFFER_SIZE];
                info.SetValue(instance, m_outgoingOffers);
            }


            // m_incomingOffers
            {
                FieldInfo info = typeof(TransferManager).GetField("m_incomingOffers", BindingFlags.Instance | BindingFlags.NonPublic);
                TransferOffer[] m_incomingOffers = (TransferOffer[])info.GetValue(Singleton<TransferManager>.instance);
                CDebug.Log($"TransferManagerExtended: Resizing m_incomingOffers from {m_incomingOffers.Length} to {NEW_TRANSFER_OFFER_SIZE}");
                m_incomingOffers = new TransferManager.TransferOffer[NEW_TRANSFER_OFFER_SIZE];
                info.SetValue(instance, m_incomingOffers);
            }


            // m_outgoingCount
            {
                FieldInfo info = typeof(TransferManager).GetField("m_outgoingCount", BindingFlags.Instance | BindingFlags.NonPublic);
                ushort[] m_outgoingCount = (ushort[])info.GetValue(Singleton<TransferManager>.instance);
                CDebug.Log($"TransferManagerExtended: Resizing m_outgoingCount from {m_outgoingCount.Length} to {NEW_TRANSFER_COUNT_SIZE}");
                m_outgoingCount = new ushort[NEW_TRANSFER_COUNT_SIZE];
                info.SetValue(instance, m_outgoingCount);
            }

            // m_incomingCount
            {
                FieldInfo info = typeof(TransferManager).GetField("m_incomingCount", BindingFlags.Instance | BindingFlags.NonPublic);
                ushort[] m_incomingCount = (ushort[])info.GetValue(Singleton<TransferManager>.instance);
                CDebug.Log($"TransferManagerExtended: Resizing m_incomingCount from {m_incomingCount.Length} to {NEW_TRANSFER_COUNT_SIZE}");
                m_incomingCount = new ushort[NEW_TRANSFER_COUNT_SIZE];
                info.SetValue(instance, m_incomingCount);
            }


            // m_outgoingAmount
            {
                FieldInfo info = typeof(TransferManager).GetField("m_outgoingAmount", BindingFlags.Instance | BindingFlags.NonPublic);
                int[] m_outgoingAmount = (int[])info.GetValue(Singleton<TransferManager>.instance);
                CDebug.Log($"TransferManagerExtended: Resizing m_outgoingAmount from {m_outgoingAmount.Length} to {NEW_TRANSFER_AMOUNT_SIZE}");
                m_outgoingAmount = new int[NEW_TRANSFER_AMOUNT_SIZE];
                info.SetValue(instance, m_outgoingAmount);
            }

            // m_incomingAmount
            {
                FieldInfo info = typeof(TransferManager).GetField("m_incomingAmount", BindingFlags.Instance | BindingFlags.NonPublic);
                int[] m_incomingAmount = (int[])info.GetValue(Singleton<TransferManager>.instance);
                CDebug.Log($"TransferManagerExtended: Resizing m_incomingAmount from {m_incomingAmount.Length} to {NEW_TRANSFER_AMOUNT_SIZE}");
                m_incomingAmount = new int[NEW_TRANSFER_AMOUNT_SIZE];
                info.SetValue(instance, m_incomingAmount);
            }
        }

        // --------------------------------------------------------------------
        public static bool LoadData(byte[] Data)
        {
            if (!IsTransferReasonArraysPatched())
            {
                CDebug.LogError($"Transfer Manager arrays not resized, unable to continue.");
                return false;
            }

            TransferManager instance = Singleton<TransferManager>.instance;

            FieldInfo infoOutgoingOffers = typeof(TransferManager).GetField("m_outgoingOffers", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo infoIncomingOffers = typeof(TransferManager).GetField("m_incomingOffers", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo infoOutgoingCount = typeof(TransferManager).GetField("m_outgoingCount", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo infoIncomingCount = typeof(TransferManager).GetField("m_incomingCount", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo infoOutgoingAmount = typeof(TransferManager).GetField("m_outgoingAmount", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo infoIncomingAmount = typeof(TransferManager).GetField("m_incomingAmount", BindingFlags.Instance | BindingFlags.NonPublic);

            TransferManager.TransferOffer[] m_outgoingOffers = (TransferOffer[]) infoOutgoingOffers.GetValue(Singleton<TransferManager>.instance);
            TransferManager.TransferOffer[] m_incomingOffers = (TransferOffer[]) infoIncomingOffers.GetValue(Singleton<TransferManager>.instance);
            ushort[] m_outgoingCount = (ushort[]) infoOutgoingCount.GetValue(Singleton<TransferManager>.instance);
            ushort[] m_incomingCount = (ushort[]) infoIncomingCount.GetValue(Singleton<TransferManager>.instance);
            int[] m_outgoingAmount = (int[]) infoOutgoingAmount.GetValue(Singleton<TransferManager>.instance);
            int[] m_incomingAmount = (int[]) infoIncomingAmount.GetValue(Singleton<TransferManager>.instance);

            int iIndex = 0;
            int iTransferReasonDataVersion = StorageData.ReadInt32(Data, ref iIndex); // 4
            int iTransferManagerReasonSize = StorageData.ReadInt32(Data, ref iIndex); // 4
            int iTransferManagerNewReasonSize = StorageData.ReadInt32(Data, ref iIndex); // 4
            uint uiSavedTickIndex = StorageData.ReadUInt32(Data, ref iIndex); // 4

#if DEBUG
            CDebug.Log($"iTransferReasonDataVersion: {iTransferReasonDataVersion} DataLength: {Data.Length} Index: {iIndex}");
#endif

            if (iTransferManagerReasonSize == TransferManager.TRANSFER_REASON_COUNT && 
                uiSavedTickIndex == SimulationManager.instance.m_currentTickIndex &&
                iTransferReasonDataVersion <= iTransferManagerSettingsVersion)
            {
                // Read amounts
                for (int i = iTransferManagerReasonSize; i < iTransferManagerNewReasonSize; i++)
                {
                    m_incomingAmount[i] = StorageData.ReadInt32(Data, ref iIndex);
                    m_outgoingAmount[i] = StorageData.ReadInt32(Data, ref iIndex);
                }

                // Read Counts
                for (int k = iTransferManagerReasonSize; k < iTransferManagerNewReasonSize; k++)
                {
                    for (int l = 0; l < 8; l++)
                    {
                        int num2 = k * 8 + l;
                        m_incomingCount[num2] = StorageData.ReadUInt16(Data, ref iIndex);
                    }
                    for (int m = 0; m < 8; m++)
                    {
                        int num3 = k * 8 + m;
                        m_outgoingCount[num3] = StorageData.ReadUInt16(Data, ref iIndex);
                    }
                }

                // Active
                for (int num8 = iTransferManagerReasonSize; num8 < iTransferManagerNewReasonSize; num8++)
                {
                    for (int num9 = 0; num9 < 8; num9++)
                    {
                        int num10 = num8 * 8 + num9;
                        uint num11 = m_incomingCount[num10];
                        num10 *= 256;
                        for (uint num12 = 0u; num12 < num11; num12++)
                        {
                            m_incomingOffers[num10 + num12].Active = StorageData.ReadBool(Data, ref iIndex);
                        }
                    }
                    for (int num13 = 0; num13 < 8; num13++)
                    {
                        int num14 = num8 * 8 + num13;
                        uint num15 = m_outgoingCount[num14];
                        num14 *= 256;
                        for (uint num16 = 0u; num16 < num15; num16++)
                        {
                            m_outgoingOffers[num14 + num16].Active = StorageData.ReadBool(Data, ref iIndex);
                        }
                    }
                }

                // Exclude
                for (int num17 = iTransferManagerReasonSize; num17 < iTransferManagerNewReasonSize; num17++)
                {
                    for (int num18 = 0; num18 < 8; num18++)
                    {
                        int num19 = num17 * 8 + num18;
                        uint num20 = m_incomingCount[num19];
                        num19 *= 256;
                        for (uint num21 = 0u; num21 < num20; num21++)
                        {
                            m_incomingOffers[num19 + num21].Exclude = StorageData.ReadBool(Data, ref iIndex);
                        }
                    }
                    for (int num22 = 0; num22 < 8; num22++)
                    {
                        int num23 = num17 * 8 + num22;
                        uint num24 = m_outgoingCount[num23];
                        num23 *= 256;
                        for (uint num25 = 0u; num25 < num24; num25++)
                        {
                            m_outgoingOffers[num23 + num25].Exclude = StorageData.ReadBool(Data, ref iIndex);
                        }
                    }
                }

                // Priority
                for (int num35 = iTransferManagerReasonSize; num35 < iTransferManagerNewReasonSize; num35++)
                {
                    for (int num36 = 0; num36 < 8; num36++)
                    {
                        int num37 = num35 * 8 + num36;
                        uint num38 = m_incomingCount[num37];
                        num37 *= 256;
                        for (uint num39 = 0u; num39 < num38; num39++)
                        {
                            m_incomingOffers[num37 + num39].Priority = StorageData.ReadInt32(Data, ref iIndex);
                        }
                    }
                    for (int num40 = 0; num40 < 8; num40++)
                    {
                        int num41 = num35 * 8 + num40;
                        uint num42 = m_outgoingCount[num41];
                        num41 *= 256;
                        for (uint num43 = 0u; num43 < num42; num43++)
                        {
                            m_outgoingOffers[num41 + num43].Priority = StorageData.ReadInt32(Data, ref iIndex);
                        }
                    }
                }

                // Amount
                for (int num44 = iTransferManagerReasonSize; num44 < iTransferManagerNewReasonSize; num44++)
                {
                    for (int num45 = 0; num45 < 8; num45++)
                    {
                        int num46 = num44 * 8 + num45;
                        uint num47 = m_incomingCount[num46];
                        num46 *= 256;
                        for (uint num48 = 0u; num48 < num47; num48++)
                        {
                            m_incomingOffers[num46 + num48].Amount = StorageData.ReadInt32(Data, ref iIndex);
                        }
                    }
                    for (int num49 = 0; num49 < 8; num49++)
                    {
                        int num50 = num44 * 8 + num49;
                        uint num51 = m_outgoingCount[num50];
                        num50 *= 256;
                        for (uint num52 = 0u; num52 < num51; num52++)
                        {
                            m_outgoingOffers[num50 + num52].Amount = StorageData.ReadInt32(Data, ref iIndex);
                        }
                    }
                }

                // PositionX
                for (int num53 = iTransferManagerReasonSize; num53 < iTransferManagerNewReasonSize; num53++)
                {
                    for (int num54 = 0; num54 < 8; num54++)
                    {
                        int num55 = num53 * 8 + num54;
                        uint num56 = m_incomingCount[num55];
                        num55 *= 256;
                        for (uint num57 = 0u; num57 < num56; num57++)
                        {
                            m_incomingOffers[num55 + num57].PositionX = StorageData.ReadInt32(Data, ref iIndex);
                        }
                    }
                    for (int num58 = 0; num58 < 8; num58++)
                    {
                        int num59 = num53 * 8 + num58;
                        uint num60 = m_outgoingCount[num59];
                        num59 *= 256;
                        for (uint num61 = 0u; num61 < num60; num61++)
                        {
                            m_outgoingOffers[num59 + num61].PositionX = StorageData.ReadInt32(Data, ref iIndex);
                        }
                    }
                }

                // PositionZ
                for (int num62 = iTransferManagerReasonSize; num62 < iTransferManagerNewReasonSize; num62++)
                {
                    for (int num63 = 0; num63 < 8; num63++)
                    {
                        int num64 = num62 * 8 + num63;
                        uint num65 = m_incomingCount[num64];
                        num64 *= 256;
                        for (uint num66 = 0u; num66 < num65; num66++)
                        {
                            m_incomingOffers[num64 + num66].PositionZ = StorageData.ReadInt32(Data, ref iIndex);
                        }
                    }
                    for (int num67 = 0; num67 < 8; num67++)
                    {
                        int num68 = num62 * 8 + num67;
                        uint num69 = m_outgoingCount[num68];
                        num68 *= 256;
                        for (uint num70 = 0u; num70 < num69; num70++)
                        {
                            m_outgoingOffers[num68 + num70].PositionZ = StorageData.ReadInt32(Data, ref iIndex);
                        }
                    }
                }

                // m_object.Type
                for (int num71 = iTransferManagerReasonSize; num71 < iTransferManagerNewReasonSize; num71++)
                {
                    for (int num72 = 0; num72 < 8; num72++)
                    {
                        int num73 = num71 * 8 + num72;
                        uint num74 = m_incomingCount[num73];
                        num73 *= 256;
                        for (uint num75 = 0u; num75 < num74; num75++)
                        {
                            m_incomingOffers[num73 + num75].m_object.Type = (InstanceType)StorageData.ReadByte(Data, ref iIndex);
                        }
                    }
                    for (int num76 = 0; num76 < 8; num76++)
                    {
                        int num77 = num71 * 8 + num76;
                        uint num78 = m_outgoingCount[num77];
                        num77 *= 256;
                        for (uint num79 = 0u; num79 < num78; num79++)
                        {
                            m_outgoingOffers[num77 + num79].m_object.Type = (InstanceType)StorageData.ReadByte(Data, ref iIndex);
                        }
                    }
                }

                // m_object.Index
                for (int num80 = iTransferManagerReasonSize; num80 < iTransferManagerNewReasonSize; num80++)
                {
                    for (int num81 = 0; num81 < 8; num81++)
                    {
                        int num82 = num80 * 8 + num81;
                        uint num83 = m_incomingCount[num82];
                        num82 *= 256;
                        for (uint num84 = 0u; num84 < num83; num84++)
                        {
                            m_incomingOffers[num82 + num84].m_object.Index = StorageData.ReadUInt32(Data, ref iIndex);
                        }
                    }
                    for (int num85 = 0; num85 < 8; num85++)
                    {
                        int num86 = num80 * 8 + num85;
                        uint num87 = m_outgoingCount[num86];
                        num86 *= 256;
                        for (uint num88 = 0u; num88 < num87; num88++)
                        {
                            m_outgoingOffers[num86 + num88].m_object.Index = StorageData.ReadUInt32(Data, ref iIndex);
                        }
                    }
                }

                // LocalPark id
                for (int num89 = iTransferManagerReasonSize; num89 < iTransferManagerNewReasonSize; num89++)
                {
                    for (int num90 = 0; num90 < 8; num90++)
                    {
                        int num91 = num89 * 8 + num90;
                        uint num92 = m_incomingCount[num91];
                        num91 *= 256;
                        for (uint num93 = 0u; num93 < num92; num93++)
                        {
                            m_incomingOffers[num91 + num93].m_isLocalPark = StorageData.ReadByte(Data, ref iIndex);
                        }
                    }
                    for (int num94 = 0; num94 < 8; num94++)
                    {
                        int num95 = num89 * 8 + num94;
                        uint num96 = m_outgoingCount[num95];
                        num95 *= 256;
                        for (uint num97 = 0u; num97 < num96; num97++)
                        {
                            m_outgoingOffers[num95 + num97].m_isLocalPark = StorageData.ReadByte(Data, ref iIndex);
                        }
                    }
                }

                // Now set values back
                infoOutgoingOffers.SetValue(instance, m_outgoingOffers);
                infoIncomingOffers.SetValue(instance, m_incomingOffers);
                infoOutgoingCount.SetValue(instance, m_outgoingCount);
                infoIncomingCount.SetValue(instance, m_incomingCount);
                infoOutgoingAmount.SetValue(instance, m_outgoingAmount);
                infoIncomingAmount.SetValue(instance, m_incomingAmount);

                return true;
            }
            else
            {
                CDebug.LogError($"Transfer Reason data not loaded | iTransferManagerReasonSize: {iTransferManagerReasonSize} iNewTransferManagerSize: {iTransferManagerNewReasonSize} SimulationTickIndex: {SimulationManager.instance.m_currentTickIndex} SavedTickIndex: {uiSavedTickIndex}");
                return false;
            } 
        }

        // --------------------------------------------------------------------
        public static void SaveData(FastList<byte> Data)
        {
            TransferManager.TransferOffer[] m_outgoingOffers = (TransferOffer[])typeof(TransferManager).GetField("m_outgoingOffers", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Singleton<TransferManager>.instance);
            TransferManager.TransferOffer[] m_incomingOffers = (TransferOffer[])typeof(TransferManager).GetField("m_incomingOffers", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Singleton<TransferManager>.instance);
            ushort[] m_outgoingCount = (ushort[])typeof(TransferManager).GetField("m_outgoingCount", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Singleton<TransferManager>.instance);
            ushort[] m_incomingCount = (ushort[])typeof(TransferManager).GetField("m_incomingCount", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Singleton<TransferManager>.instance);
            int[] m_outgoingAmount = (int[])typeof(TransferManager).GetField("m_outgoingAmount", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Singleton<TransferManager>.instance);
            int[] m_incomingAmount = (int[])typeof(TransferManager).GetField("m_incomingAmount", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Singleton<TransferManager>.instance);

            // Tuple size gets written first (At the end of the function
            StorageData.WriteInt32(iTransferManagerSettingsVersion, Data);
            StorageData.WriteInt32(TransferManager.TRANSFER_REASON_COUNT, Data);
            StorageData.WriteInt32(NEW_TRANSFER_REASON_COUNT, Data);
            StorageData.WriteUInt32(SimulationManager.instance.m_currentTickIndex, Data);

            // Amounts
            for (int i = TransferManager.TRANSFER_REASON_COUNT; i < NEW_TRANSFER_REASON_COUNT; i++)
            {
                StorageData.WriteInt32(m_incomingAmount[i], Data);
                StorageData.WriteInt32(m_outgoingAmount[i], Data);
            }
            
            // Counts
            for (int j = TransferManager.TRANSFER_REASON_COUNT; j < NEW_TRANSFER_REASON_COUNT; j++)
            {
                for (int k = 0; k < 8; k++)
                {
                    int num2 = j * 8 + k;
                    StorageData.WriteUInt16(m_incomingCount[num2], Data);
                }
                for (int l = 0; l < 8; l++)
                {
                    int num3 = j * 8 + l;
                    StorageData.WriteUInt16(m_outgoingCount[num3], Data);
                }
            }

            // Active
            for (int m = TransferManager.TRANSFER_REASON_COUNT; m < NEW_TRANSFER_REASON_COUNT; m++)
            {
                for (int n = 0; n < 8; n++)
                {
                    int num4 = m * 8 + n;
                    uint num5 = m_incomingCount[num4];
                    num4 *= 256;
                    for (uint num6 = 0u; num6 < num5; num6++)
                    {
                        StorageData.WriteBool(m_incomingOffers[num4 + num6].Active, Data);
                    }
                }
                for (int num7 = 0; num7 < 8; num7++)
                {
                    int num8 = m * 8 + num7;
                    uint num9 = m_outgoingCount[num8];
                    num8 *= 256;
                    for (uint num10 = 0u; num10 < num9; num10++)
                    {
                        StorageData.WriteBool(m_outgoingOffers[num8 + num10].Active, Data);
                    }
                }
            }

            // Exclude
            for (int num11 = TransferManager.TRANSFER_REASON_COUNT; num11 < NEW_TRANSFER_REASON_COUNT; num11++)
            {
                for (int num12 = 0; num12 < 8; num12++)
                {
                    int num13 = num11 * 8 + num12;
                    uint num14 = m_incomingCount[num13];
                    num13 *= 256;
                    for (uint num15 = 0u; num15 < num14; num15++)
                    {
                        StorageData.WriteBool(m_incomingOffers[num13 + num15].Exclude, Data);
                    }
                }
                for (int num16 = 0; num16 < 8; num16++)
                {
                    int num17 = num11 * 8 + num16;
                    uint num18 = m_outgoingCount[num17];
                    num17 *= 256;
                    for (uint num19 = 0u; num19 < num18; num19++)
                    {
                        StorageData.WriteBool(m_outgoingOffers[num17 + num19].Exclude, Data);
                    }
                }
            }

            // Priority
            for (int num20 = TransferManager.TRANSFER_REASON_COUNT; num20 < NEW_TRANSFER_REASON_COUNT; num20++)
            {
                for (int num21 = 0; num21 < 8; num21++)
                {
                    int num22 = num20 * 8 + num21;
                    uint num23 = m_incomingCount[num22];
                    num22 *= 256;
                    for (uint num24 = 0u; num24 < num23; num24++)
                    {
                        StorageData.WriteInt32((byte)m_incomingOffers[num22 + num24].Priority, Data);
                    }
                }
                for (int num25 = 0; num25 < 8; num25++)
                {
                    int num26 = num20 * 8 + num25;
                    uint num27 = m_outgoingCount[num26];
                    num26 *= 256;
                    for (uint num28 = 0u; num28 < num27; num28++)
                    {
                        StorageData.WriteInt32((byte)m_outgoingOffers[num26 + num28].Priority, Data);
                    }
                }
            }

            // Amount
            for (int num29 = TransferManager.TRANSFER_REASON_COUNT; num29 < NEW_TRANSFER_REASON_COUNT; num29++)
            {
                for (int num30 = 0; num30 < 8; num30++)
                {
                    int num31 = num29 * 8 + num30;
                    uint num32 = m_incomingCount[num31];
                    num31 *= 256;
                    for (uint num33 = 0u; num33 < num32; num33++)
                    {
                        StorageData.WriteInt32((byte)m_incomingOffers[num31 + num33].Amount, Data);
                    }
                }
                for (int num34 = 0; num34 < 8; num34++)
                {
                    int num35 = num29 * 8 + num34;
                    uint num36 = m_outgoingCount[num35];
                    num35 *= 256;
                    for (uint num37 = 0u; num37 < num36; num37++)
                    {
                        StorageData.WriteInt32((byte)m_outgoingOffers[num35 + num37].Amount, Data);
                    }
                }
            }
 
            // PositionX
            for (int num38 = TransferManager.TRANSFER_REASON_COUNT; num38 < NEW_TRANSFER_REASON_COUNT; num38++)
            {
                for (int num39 = 0; num39 < 8; num39++)
                {
                    int num40 = num38 * 8 + num39;
                    uint num41 = m_incomingCount[num40];
                    num40 *= 256;
                    for (uint num42 = 0u; num42 < num41; num42++)
                    {
                        StorageData.WriteInt32((byte)m_incomingOffers[num40 + num42].PositionX, Data);
                    }
                }
                for (int num43 = 0; num43 < 8; num43++)
                {
                    int num44 = num38 * 8 + num43;
                    uint num45 = m_outgoingCount[num44];
                    num44 *= 256;
                    for (uint num46 = 0u; num46 < num45; num46++)
                    {
                        StorageData.WriteInt32((byte)m_outgoingOffers[num44 + num46].PositionX, Data);
                    }
                }
            }

            // PositionZ
            for (int num47 = TransferManager.TRANSFER_REASON_COUNT; num47 < NEW_TRANSFER_REASON_COUNT; num47++)
            {
                for (int num48 = 0; num48 < 8; num48++)
                {
                    int num49 = num47 * 8 + num48;
                    uint num50 = m_incomingCount[num49];
                    num49 *= 256;
                    for (uint num51 = 0u; num51 < num50; num51++)
                    {
                        StorageData.WriteInt32((byte)m_incomingOffers[num49 + num51].PositionZ, Data);
                    }
                }
                for (int num52 = 0; num52 < 8; num52++)
                {
                    int num53 = num47 * 8 + num52;
                    uint num54 = m_outgoingCount[num53];
                    num53 *= 256;
                    for (uint num55 = 0u; num55 < num54; num55++)
                    {
                        StorageData.WriteInt32((byte)m_outgoingOffers[num53 + num55].PositionZ, Data);
                    }
                }
            }

            // Object type
            for (int num56 = TransferManager.TRANSFER_REASON_COUNT; num56 < NEW_TRANSFER_REASON_COUNT; num56++)
            {
                for (int num57 = 0; num57 < 8; num57++)
                {
                    int num58 = num56 * 8 + num57;
                    uint num59 = m_incomingCount[num58];
                    num58 *= 256;
                    for (uint num60 = 0u; num60 < num59; num60++)
                    {
                        StorageData.WriteByte((byte)m_incomingOffers[num58 + num60].m_object.Type, Data);
                    }
                }
                for (int num61 = 0; num61 < 8; num61++)
                {
                    int num62 = num56 * 8 + num61;
                    uint num63 = m_outgoingCount[num62];
                    num62 *= 256;
                    for (uint num64 = 0u; num64 < num63; num64++)
                    {
                        StorageData.WriteByte((byte)m_outgoingOffers[num62 + num64].m_object.Type, Data);
                    }
                }
            }
            
            // Object index
            for (int num65 = TransferManager.TRANSFER_REASON_COUNT; num65 < NEW_TRANSFER_REASON_COUNT; num65++)
            {
                for (int num66 = 0; num66 < 8; num66++)
                {
                    int num67 = num65 * 8 + num66;
                    uint num68 = m_incomingCount[num67];
                    num67 *= 256;
                    for (uint num69 = 0u; num69 < num68; num69++)
                    {
                        StorageData.WriteUInt32(m_incomingOffers[num67 + num69].m_object.Index, Data);
                    }
                }
                for (int num70 = 0; num70 < 8; num70++)
                {
                    int num71 = num65 * 8 + num70;
                    uint num72 = m_outgoingCount[num71];
                    num71 *= 256;
                    for (uint num73 = 0u; num73 < num72; num73++)
                    {
                        StorageData.WriteUInt32(m_outgoingOffers[num71 + num73].m_object.Index, Data);
                    }
                }
            }

            // Object local park id
            for (int num74 = TransferManager.TRANSFER_REASON_COUNT; num74 < NEW_TRANSFER_REASON_COUNT; num74++)
            {
                for (int num75 = 0; num75 < 8; num75++)
                {
                    int num76 = num74 * 8 + num75;
                    uint num77 = m_incomingCount[num76];
                    num76 *= 256;
                    for (uint num78 = 0u; num78 < num77; num78++)
                    {
                        StorageData.WriteByte(m_incomingOffers[num76 + num78].m_isLocalPark, Data);
                    }
                }
                for (int num79 = 0; num79 < 8; num79++)
                {
                    int num80 = num74 * 8 + num79;
                    uint num81 = m_outgoingCount[num80];
                    num80 *= 256;
                    for (uint num82 = 0u; num82 < num81; num82++)
                    {
                        StorageData.WriteByte(m_outgoingOffers[num80 + num82].m_isLocalPark, Data);
                    }
                }
            }
        }
    }
}