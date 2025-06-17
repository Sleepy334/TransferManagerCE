using ColossalFramework.UI;
using SleepyCommon;
using TransferManagerCE.UI;
using UnityEngine;

namespace TransferManagerCE
{
    public class CheckListRow : UIPanel
    {
        const int iROW_HEIGHT = 25;

        CheckListData? m_data = null;
        public UICheckBox? m_chkItem;
        public UIButton? m_btnShow;
        private bool m_bUpdating = false;

        public CheckListRow()
        {
        }

        public static CheckListRow? Create(UIComponent parent, CheckListData data, float height)
        {
            CheckListRow? oRow = null;

            if (parent is not null)
            {
                oRow = parent.AddUIComponent<CheckListRow>();
                oRow.Setup(data, height);
            }

            return oRow;
        }

        public void Setup(CheckListData data, float fHeight)
        {
            width = parent.width;
            height = fHeight;
            autoLayoutDirection = LayoutDirection.Horizontal;
            autoLayoutStart = LayoutStart.TopLeft;
            autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            autoLayout = true;
            clipChildren = true;

            m_data = data;

            m_chkItem = SleepyCommon.UIMyUtils.AddCheckbox(this, GetText(), UIFonts.Regular, BuildingPanel.fTEXT_SCALE, false, OnItemCheckChanged);
            m_chkItem.width = width - height - CheckListView.iSCROLL_BAR_WIDTH;
            m_chkItem.label.textColor = data.GetTextColor();

            m_btnShow = AddUIComponent<UIButton>();
            if (m_btnShow is not null)
            {
                m_btnShow.name = name;
                m_btnShow.tooltip = "Show";
                m_btnShow.width = height;
                m_btnShow.height = height;
                m_btnShow.atlas = atlas;
                m_btnShow.normalBgSprite = "LocationMarkerActiveNormal";
                m_btnShow.pressedBgSprite = "LocationMarkerActivePressed";
                m_btnShow.color = Color.white;
                m_btnShow.eventClick += OnShowClicked;
            }
        }

        public int CompareTo(object second)
        {
            if (second is null)
            {
                return 1;
            }
            CheckListRow oSecond = (CheckListRow)second;
            return m_data.CompareTo(oSecond.m_data);
        }

        public void OnItemCheckChanged(bool bChecked)
        {
            if (!m_bUpdating && m_data is not null)
            {
                m_data.OnItemCheckChanged(bChecked);
            }
        }

        public void OnShowClicked(UIComponent component, UIMouseEventParameter e)
        {
            if (m_data is not null)
            {
                m_data.OnShow();
            }
        }

        public void SetData(CheckListData data)
        {
            m_data = (CheckListData) data;
            UpdateData();
        }

        public string GetText()
        {
            return m_data.GetText();
        }

        public bool IsChecked()
        {
            if (m_data is not null)
            {
                return m_data.IsChecked();
            }

            return false;
        }

        public void UpdateData()
        {
            try
            {
                m_bUpdating = true;

                if (m_chkItem is not null && m_data is not null)
                {
                    m_chkItem.text = GetText();
                    m_chkItem.label.textColor = m_data.GetTextColor();
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