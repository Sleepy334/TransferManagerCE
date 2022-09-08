using ColossalFramework.UI;
using System;
using UnityEngine;

namespace SleepyCommon
{
    public abstract class ListViewHeaderColumnBase
    {
        public delegate void OnListViewColumnClick(ListViewHeaderColumnBase eColumn); 
        
        protected OnListViewColumnClick? m_eventClickCallback = null;
        protected ListViewRowComparer.Columns m_eColumn;
        protected string m_sText;

        public ListViewHeaderColumnBase(ListViewRowComparer.Columns eColumn, string sText, OnListViewColumnClick eventClickCallback)
        {
            m_eColumn = eColumn;
            m_sText = sText;
            m_eventClickCallback = eventClickCallback;
        }

        abstract public void Sort(ListViewRowComparer.Columns eColumn, bool bDescending);

        virtual public void SetText(string sText)
        {
        }

        virtual public void SetTooltip(string sText)
        {
        }

        virtual public bool IsHit(UIComponent component)
        {
            return false;
        }

        abstract public void Destroy();

        public ListViewRowComparer.Columns GetColumn()
        {
            return m_eColumn;
        }

        protected void OnItemClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (m_eventClickCallback != null)
            {
                m_eventClickCallback(this);
            }
        }
    }
}
