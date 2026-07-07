using ICities;
using SleepyCommon;
using static TransferManager;
using static TransferManagerCore.BuildingTypeHelper;

namespace TransferManagerCore.Data
{
    public abstract class StatusDataBuilding : StatusData
    {
        public enum TimerType
        {
            Death,
            Sick,
            Incoming,
            Outgoing,
            Worker,
        }

        public StatusDataBuilding(CustomTransferReason.Reason reason, BuildingType eBuildingType, ushort BuildingId) :
            base(reason, eBuildingType, BuildingId)
        {
        }

        public override bool IsBuildingData()
        {
            return true;
        }

        public override bool IsVehicleData()
        {
            return false;
        }

        public override string GetMaterialDisplay()
        {
            return GetMaterialDescription();
        }

        protected override string CalculateTimer(out string tooltip)
        {
            // Timers are material specific for buildings
            tooltip = "";
            return "";
        }

        protected override double CalculateDistance()
        {
            return double.MaxValue;
        }

        protected override string CalculateDescription1(out string tooltip)
        {
            tooltip = "";
            return "";
        }

        protected override string CalculateDescription2(out string tooltip)
        {
            tooltip = "";
            return "";
        }

        protected string DisplayValueAsPercent(int iBuffer, int iMaxValue)
        {
            return Utils.MakePercent(iBuffer, iMaxValue);
        }

        protected string MakeTooltip(int iBuffer)
        {
            // We also store the tooltip here
            return $"{GetMaterialDescription()}: {DisplayBufferLong(iBuffer)}";
        }

        protected string MakeTooltip(int iBuffer, int iMaxValue)
        {
            return $"{GetMaterialDescription()}: {DisplayBufferLong(iBuffer)}/{DisplayBufferLong(iMaxValue)}";
        }

        protected void WarnText()
        {
            m_color = KnownColor.orange;
        }

        protected void WarnText(bool bMin, bool bMax, int iBuffer, int iMaxValue)
        {
            if (bMin && iBuffer == 0)
            {
                m_color = KnownColor.orange;
            }

            if (bMax && iBuffer >= iMaxValue)
            {
                m_color = KnownColor.orange;
            }
        }

        protected void AddTimerText(TimerType type, ref string sText, ref string tooltip)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_flags != 0)
            {
                if (type == TimerType.Incoming && building.m_incomingProblemTimer > 0)
                {
                    if (string.IsNullOrEmpty(sText))
                    {
                        sText += " ";
                    }
                    sText += "I:" + building.m_incomingProblemTimer;

                    tooltip = $"Incoming Timer: {building.m_incomingProblemTimer}\r\n{tooltip}";
                }

                if (type == TimerType.Outgoing && building.m_outgoingProblemTimer > 0)
                {
                    if (string.IsNullOrEmpty(sText))
                    {
                        sText += " ";
                    }
                    sText += "O:" + building.m_outgoingProblemTimer;

                    tooltip += $"Outgoing Timer: {building.m_outgoingProblemTimer}\r\n{tooltip}";
                }

                if (type == TimerType.Death && building.m_deathProblemTimer > 0)
                {
                    if (string.IsNullOrEmpty(sText))
                    {
                        sText += " ";
                    }
                    sText += "D:" + building.m_deathProblemTimer;

                    tooltip = $"Death Timer: {building.m_deathProblemTimer}\r\n{tooltip}";
                }

                if (type == TimerType.Sick && building.m_healthProblemTimer > 0)
                {
                    if (string.IsNullOrEmpty(sText))
                    {
                        sText += " ";
                    }
                    sText += "S:" + building.m_healthProblemTimer;

                    tooltip = $"Sick Timer: {building.m_healthProblemTimer}\r\n{tooltip}";
                }

                if (type == TimerType.Worker && building.m_workerProblemTimer > 0)
                {
                    if (string.IsNullOrEmpty(sText))
                    {
                        sText += " ";
                    }
                    sText += "S:" + building.m_workerProblemTimer;

                    tooltip = $"Worker Timer: {building.m_workerProblemTimer}\r\n{tooltip}";
                }
            }
        }
    }
}