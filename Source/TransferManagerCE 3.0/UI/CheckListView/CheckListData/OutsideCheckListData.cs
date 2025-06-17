using SleepyCommon;
using static TransferManagerCE.TransportUtils;

namespace TransferManagerCE
{
    public class OutsideCheckListData : CheckListData
    {
        private ushort m_buildingId;
        private int m_restrictionId;
        private ushort m_outsideConnectionBuildingId;

        // ----------------------------------------------------------------------------------------
        public OutsideCheckListData(ushort buildingId, int restrictionId, ushort outsideConnection)
        {
            m_buildingId = buildingId;
            m_restrictionId = restrictionId;
            m_outsideConnectionBuildingId = outsideConnection;
        }

        public OutsideCheckListData(OutsideCheckListData oSecond)
        {
            m_buildingId = oSecond.m_buildingId;
            m_restrictionId = oSecond.m_restrictionId;
            m_outsideConnectionBuildingId = oSecond.m_outsideConnectionBuildingId;
        }

        public override int CompareTo(object second)
        {
            if (second is null)
            {
                return 1;
            }

            OutsideCheckListData oSecond = (OutsideCheckListData) second;

            TransportType type1 = TransportUtils.GetTransportType(m_outsideConnectionBuildingId);
            TransportType type2 = TransportUtils.GetTransportType(oSecond.m_outsideConnectionBuildingId);
            if (type1 != type2)
            {
                return type1.CompareTo(type2);
            }

            return GetText().CompareTo(oSecond.GetText());
        }



        public override string GetText()
        {
            return $"{CitiesUtils.GetOutsideConnectionName(m_outsideConnectionBuildingId)} ({TransportUtils.GetTransportType(m_outsideConnectionBuildingId)})";
        }

        public override bool IsChecked()
        {
            BuildingSettings? settings = BuildingSettingsStorage.GetSettings(m_buildingId);
            if (settings is not null)
            {
                RestrictionSettings? restrictions = settings.GetRestrictions(m_restrictionId);
                if (restrictions is not null)
                {
                    return !restrictions.m_excludedOutsideConnections.Contains(m_outsideConnectionBuildingId);
                }
            }

            return true;
        }

        public override void OnItemCheckChanged(bool bChecked)
        {
            if (m_buildingId != 0 && m_restrictionId != -1)
            {
                BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(m_buildingId);
                RestrictionSettings restrictions = settings.GetRestrictionsOrDefault(m_restrictionId);

                if (restrictions.m_excludedOutsideConnections.Contains(m_outsideConnectionBuildingId))
                {
                    restrictions.m_excludedOutsideConnections.Remove(m_outsideConnectionBuildingId);
                }
                else
                {
                    restrictions.m_excludedOutsideConnections.Add(m_outsideConnectionBuildingId);
                }

                settings.SetRestrictions(m_restrictionId, restrictions);
                BuildingSettingsStorage.SetSettings(m_buildingId, settings);
            }
        }

        public override void OnShow()
        {
            InstanceHelper.ShowInstance(new InstanceID {  Building = m_outsideConnectionBuildingId });
        }
    }
}