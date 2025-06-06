using ColossalFramework.UI;
using SleepyCommon;
using UnityEngine;

namespace TransferManagerCE.UI
{
    public abstract class UIListRow<T> : UIPanel, IUIFastListRow
    {
        protected UIComponent? m_MouseEnterComponent = null;
        protected int m_iIndex = -1;
        private bool m_bAfterStart = false;
        private object? m_data = null;
        private bool m_bFullRowSelect = false;
        
        // Live toltip support
        private UIComponent? m_tooltipComponent = null;
        
        // ----------------------------------------------------------------------------------------
        protected abstract void Display();
        protected abstract void Clear();
        protected abstract void ClearTooltips();
        protected abstract string GetTooltipText(UIComponent component);
        protected abstract void OnClicked(UIComponent component);
        
        // ----------------------------------------------------------------------------------------
        protected T? data
        {
            get
            {
                return (T) m_data;
            }
        }

        protected bool fullRowSelect
        {
            get
            {
                return m_bFullRowSelect;
            }

            set
            {
                m_bFullRowSelect = value;
            }
        }

        public override void Start()
        {
            base.Start();

            isVisible = true;
            canFocus = true;
            isInteractive = true;
            width = parent.width - ListView.iSCROLL_BAR_WIDTH;
            height = ListView.iROW_HEIGHT;
            autoLayoutDirection = LayoutDirection.Horizontal;
            autoLayoutStart = LayoutStart.TopLeft;
            autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            autoLayout = true;
            clipChildren = true;
        }

        protected void AfterStart()
        {
            m_bAfterStart = true;

            if (fullRowSelect)
            {
                // Hook them all up to the row panel
                eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
                eventTooltipLeave += new MouseEventHandler(OnTooltipLeave);
                eventMouseEnter += new MouseEventHandler(OnMouseEnter);
                eventMouseLeave += new MouseEventHandler(OnMouseLeave);
                eventClicked += new MouseEventHandler(OnItemClicked);
            }
            else
            {
                // Hook up independent components.
                foreach (UIComponent component in components)
                {
                    component.eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
                    component.eventTooltipLeave += new MouseEventHandler(OnTooltipLeave);
                    component.eventClicked += new MouseEventHandler(OnItemClicked);
                    // We dont hook up eventMouseEnter / eventMouseLeave as they will be component specific.
                }
            }

            if (components.Count > 0)
            {
                ResizeLastColumn();
            }

            if (m_data is not null)
            {
                Display(-1, m_data, false);
            }
        }

        public virtual void Display(int index, object data, bool isRowOdd)
        {
            m_data = data;
            m_iIndex = index;

            if (!m_bAfterStart)
            {
                return;
            }

            if (m_data is not null)
            {
                Display();

                if (m_tooltipComponent != null)
                {
                    UpdateLiveTooltip();
                }
                else
                {
                    ClearTooltips();
                }

                // Update text color
                foreach (UIComponent component in components)
                {
                    if (component is UILabel label)
                    {
                        label.textColor = GetTextColor(component, m_MouseEnterComponent != null);
                    }
                }
            }
            else
            {
                Clear();
                ClearTooltips();
                HideTooltip();
            }
        }

        protected bool IsShowingTooltip()
        {
            if (m_tooltipComponent != null)
            {
                if (m_tooltipComponent is UILabelLiveTooltip liveTooltip)
                {
                    return liveTooltip.IsTooltipVisible();
                }
                else
                {
                    return m_tooltipComponent.tooltipBox is not null &&
                            m_tooltipComponent.tooltipBox.isVisible;
                }
            }

            return false;
        }

        protected void HideTooltip()
        {
            if (m_tooltipComponent != null)
            {
                if (m_tooltipComponent is UILabelLiveTooltip liveTooltip)
                {
                    liveTooltip.HideTooltip();
                }
                else if (m_tooltipComponent.tooltipBox is not null)
                {
                    m_tooltipComponent.tooltipBox.Hide();
                }
            }

            m_tooltipComponent = null;
        }

        public virtual void Select(bool isRowOdd)
        {
        }

        public virtual void Deselect(bool isRowOdd)
        {
        }

        public virtual void Enable(object data)
        {
            m_data = (T) data;
        }

        public virtual void Disabled()
        {
            m_data = null;
            m_iIndex = -1;

            if (m_bAfterStart)
            {
                Clear();
                ClearTooltips();
                HideTooltip();
            }
        }

        private void UpdateLiveTooltip()
        {
            if (IsShowingTooltip())
            {
                string sTooltip = GetTooltipText(m_tooltipComponent);
                if (sTooltip.Length > 0)
                {
                    m_tooltipComponent.tooltip = sTooltip;
                    m_tooltipComponent.RefreshTooltip();
                }
                else
                {
                    HideTooltip();
                }
            }
        }

        protected virtual void OnTooltipEnter(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (!m_bAfterStart)
            {
                return;
            }

            if (!enabled) 
            {
                return; 
            }

            if (data is not null && component is not null)
            {
                
                string sTooltip = GetTooltipText(component);
                if (sTooltip.Length > 0)
                {
                    component.tooltip = sTooltip;
                    m_tooltipComponent = component;
                }
                else
                {
                    ClearTooltips();
                    HideTooltip();
                }
            }
            else
            {
                ClearTooltips();
                HideTooltip();
            }
        }

        protected virtual void OnTooltipLeave(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (!m_bAfterStart)
            {
                return;
            }

            ClearTooltips();
            HideTooltip();
        }

        protected virtual Color GetTextColor(UIComponent component, bool highlightRow)
        {
            if (highlightRow)
            {
                if (fullRowSelect)
                {
                    return Color.yellow;
                }
                else if (component == m_MouseEnterComponent)
                {
                    return Color.yellow;
                }
            }

            return Color.white;
        }

        protected void OnMouseEnter(UIComponent component, UIMouseEventParameter eventParam)
        {
            m_MouseEnterComponent = component;

            if (fullRowSelect)
            {
                foreach (UIComponent com in components)
                {
                    if (com is UILabel)
                    {
                        ((UILabel)com).textColor = GetTextColor(com, true);
                    }
                }
            }
            else
            {
                UILabel? txtLabel = component as UILabel;
                if (txtLabel is not null)
                {
                    txtLabel.textColor = GetTextColor(txtLabel, true);
                }
            }
        }

        protected void OnMouseLeave(UIComponent component, UIMouseEventParameter eventParam)
        {
            m_MouseEnterComponent = null;

            if (fullRowSelect)
            {
                foreach (UIComponent com in components)
                {
                    if (com is UILabel)
                    {
                        ((UILabel)com).textColor = GetTextColor(com, false);
                    }
                }
            }
            else
            {
                UILabel? txtLabel = component as UILabel;
                if (txtLabel is not null)
                {
                    txtLabel.textColor = GetTextColor(txtLabel, false);
                }
            } 
        }

        private void ResizeLastColumn()
        {
            // Subtract width of each column
            float columnWidths = 0;
            foreach (UIComponent component in components)
            {
                columnWidths += component.width + autoLayoutPadding.left + autoLayoutPadding.right;
            }

            // Adjust last label column (ignore delete buttons)
            for (int i = components.Count - 1; i >= 0; --i)
            {
                if (components[i] is UILabel)
                {
                    UILabel uILabel = (UILabel)components[i];
                    float fOldWidth = uILabel.width;    
                    uILabel.width = width - columnWidths + uILabel.width;
                    break;
                }
            }
        }

        private void OnItemClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (data is not null)
            {
                OnClicked(component);
            }
        }
    }
}