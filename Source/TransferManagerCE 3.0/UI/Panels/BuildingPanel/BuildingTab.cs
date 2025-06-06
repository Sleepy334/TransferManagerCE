using System.Collections.Generic;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.UI
{
    public abstract class BuildingTab
    {
        protected ushort m_buildingId = 0;
        protected BuildingType m_eBuildingType = BuildingType.None;

        protected UITabStrip? m_tabStrip = null;
        protected bool m_bAfterSetup = false;

        public BuildingTab()
        {
        }

        public abstract bool ShowTab();

        public void Setup(UITabStrip tabStrip)
        {
            m_tabStrip = tabStrip;

            SetupInternal();

            AfterSetup();
        }

        public abstract void SetupInternal();

        public virtual void AfterSetup()
        {
            m_bAfterSetup = true;
        }

        public virtual bool UpdateTab(bool bActive)
        {
            if (!m_bAfterSetup || m_tabStrip is null)
            {
                return false;
            }

            return true;
        }

        public virtual void SetTabBuilding(ushort buildingId, BuildingType buildingType, List<ushort> subBuildingIds)
        {
            if (buildingId != m_buildingId)
            {
                m_buildingId = buildingId;
                m_eBuildingType = buildingType;
                UpdateTab(false);
            }
        }

        public virtual void Destroy()
        {

        }

        public virtual void Clear()
        {

        }
    }
}