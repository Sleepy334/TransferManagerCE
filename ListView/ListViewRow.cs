using ColossalFramework;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using TransferManagerCE;
using TransferManagerCE.CustomManager;
using UnityEngine;
using static SleepyCommon.ListViewRowColumn;

namespace SleepyCommon
{
    public class ListViewRow : UIPanel, IComparable
    {
        public const int iROW_HEIGHT = 20;
        public const int iROW_MARGIN = 10;
        public ListData? m_data = null;
        private List<ListViewRowColumn> m_columns;
        private string m_backgroundSprite = "GenericPanel";
        private float m_fTextScale = 1.0f;

        public ListViewRow() : base()
        {
            m_columns = new List<ListViewRowColumn>();
        }

        public static ListViewRow? Create(ListViewMainPanel oParent, string backgroundSprite, float fTextScale, ListData data)
        {
            if (oParent != null && oParent != null)
            {
                ListViewRow oRow = oParent.AddUIComponent<ListViewRow>();
                oRow.m_data = data;

                //oRow.color = oLineInfo.m_color;
                //oRow.m_backgroundSprite = backgroundSprite;
                //oRow.backgroundSprite = backgroundSprite;// "ScrollbarTrack";// "GenericPanel";
                oRow.position = new Vector3(iROW_MARGIN, -iROW_MARGIN);
                oRow.m_fTextScale = fTextScale;
                oRow.width = oParent.width;
                oRow.height = iROW_HEIGHT;
                oRow.autoLayoutDirection = LayoutDirection.Horizontal;
                oRow.autoLayoutStart = LayoutStart.TopLeft;
                oRow.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
                oRow.autoLayout = true;
                oRow.clipChildren = true;
                oRow.eventMouseEnter += new MouseEventHandler(oRow.OnItemEnter);
                oRow.eventMouseLeave += new MouseEventHandler(oRow.OnItemLeave);
                oRow.Setup(data);
       
                return oRow;
            }
            else
            {
                return null;
            }
        }

        public void Setup(ListData data)
        {
            data.CreateColumns(this, m_columns);
            UpdateData();
        }

        public void AddColumn(ListViewRowComparer.Columns eColumn, string sText, string sTooltip, int iWidth, UIHorizontalAlignment oTextAlignment, UIAlignAnchor oAncor)
        {
            m_columns.Add(ListViewRowColumn.Create(eColumn, this, sText, sTooltip, m_fTextScale, iWidth, ListViewRow.iROW_HEIGHT, oTextAlignment, oAncor, OnItemClicked, OnGetColumnTooltip));
        }

        public int CompareTo(object second)
        {
            if (second == null)
            {
                return 1;
            }
            ListViewRow oSecond = (ListViewRow)second;
            return m_data.CompareTo(oSecond.m_data);
        }

        private void OnItemClicked(ListViewRowColumn oColumn)
        {
            m_data.OnClick(oColumn);
        }

        public string OnGetColumnTooltip(ListViewRowColumn oColumn) 
        {
            return m_data.OnTooltip(oColumn);
        }

        private void OnItemEnter(UIComponent component, UIMouseEventParameter eventParam)
        {        
            
        }

        private void OnItemLeave(UIComponent component, UIMouseEventParameter eventParam)
        {
            //ClearRowHighlight();
        }

        public void ClearRowHighlight()
        {
            backgroundSprite = m_backgroundSprite;// "GenericPanel";
        }  

        public void SetData(ListData data)
        {
            m_data = data;
            UpdateData();
        }

        public void UpdateData()
        {
            if (m_data != null)
            {
                m_data.Update();

                foreach (ListViewRowColumn column in m_columns)
                {
                    column.SetText(m_data.GetText(column.GetColumn()));
                }
            }
        }

        private void HideTooltipBox(UIComponent? uiComponent)
        {
            if (uiComponent != null && uiComponent.tooltipBox != null)
            {
                uiComponent.tooltipBox.isVisible = false;
            }
        }
        
        public override void OnDestroy()
        {
            foreach (ListViewRowColumn oColumn in m_columns)
            { 
                oColumn.HideTooltipBox();
                oColumn.Destroy();
            }
            m_columns.Clear();
            base.OnDestroy();
        }
    }
     
}
