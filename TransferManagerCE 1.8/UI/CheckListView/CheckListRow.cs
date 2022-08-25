using ColossalFramework.UI;
using System;
using TransferManagerCE;
using UnityEngine;

namespace SleepyCommon
{
    public class CheckListRow : UIPanel
    {
        const int iROW_HEIGHT = 25;

        CheckListData? m_data = null;
        public UICheckBox? m_chkItem;
        private bool m_bUpdating = false;

        public CheckListRow()
        {
        }

        public static CheckListRow? Create(UIComponent parent, CheckListData data)
        {
            CheckListRow? oRow = null;

            if (parent != null)
            {
                oRow = parent.AddUIComponent<CheckListRow>();
                oRow.width = parent.width;
                oRow.height = iROW_HEIGHT;
                oRow.autoLayoutDirection = LayoutDirection.Horizontal;
                oRow.autoLayoutStart = LayoutStart.TopLeft;
                oRow.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
                oRow.autoLayout = true;
                oRow.clipChildren = true;
                oRow.m_data = data;
                oRow.m_chkItem = TransferManagerCE.UIUtils.AddCheckbox(oRow, oRow.GetText(), 1.0f, false, oRow.OnItemCheckChanged);
            }

            return oRow;
        }

        public int CompareTo(object second)
        {
            if (second == null)
            {
                return 1;
            }
            CheckListRow oSecond = (CheckListRow)second;
            return m_data.CompareTo(oSecond.m_data);
        }

        public void OnItemCheckChanged(bool bChecked)
        {
            if (!m_bUpdating)
            {
                BuildingSettings settings = BuildingSettings.GetSettings(BuildingPanel.Instance.m_buildingId);
                if (m_data.m_bIncoming)
                {
                    settings.SetIncomingDistrictAllowed(m_data.m_eType, m_data.m_districtId, bChecked);
                }
                else
                {
                    settings.SetOutgoingDistrictAllowed(m_data.m_eType, m_data.m_districtId, bChecked);
                }
            }
        }

        public void SetData(CheckListData data)
        {
            m_data = data;
            UpdateData();
        }

        public string GetText()
        {
            return m_data.GetText();
        }

        public bool IsChecked()
        {
            return m_data.IsChecked();
        }

        public void UpdateData()
        {
            try
            {
                m_bUpdating = true;

                if (m_chkItem != null && m_data != null)
                {
                    m_chkItem.text = GetText();
                    m_chkItem.isChecked = IsChecked();
                }
            }
            finally
            {
                m_bUpdating = false;
            }
        }
            
    }
}