using ColossalFramework.UI;
using UnityEngine;

namespace TransferManagerCE
{
    public class UIToggleButton : UIButton
    {
        bool m_bToggleState = true;

        public bool ToggleState
        {
            get 
            { 
                return m_bToggleState; 
            }
            set 
            { 
                m_bToggleState = value;
                UpdateButton();
            }
        }

        public override void Start()
        {
            UpdateButton();
            base.Start();
        }

        protected override void OnMouseDown(UIMouseEventParameter p)
        {
            m_bToggleState = !m_bToggleState;
            UpdateButton();

            base.OnMouseDown(p);
        }

        private void UpdateButton()
        {
            if (m_bToggleState)
            {
                color = KnownColor.lightBlue;
                hoveredColor = KnownColor.lightBlue;
                pressedColor = KnownColor.lightBlue;
                focusedColor = KnownColor.lightBlue;
            }
            else
            {
                color = Color.grey;
                hoveredColor = Color.grey;
                pressedColor = Color.grey;
                focusedColor = Color.grey;
            }
        }
    }
}
