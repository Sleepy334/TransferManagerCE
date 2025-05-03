using UnityEngine;
using ColossalFramework.UI;

namespace TransferManagerCE
{
    public class UITitleBar : UIPanel
    {
        const int iTITLE_HEIGHT = 36;
        const int iBUTTON_HEIGHT = 35;
        const int iBUTTON_MARGIN = 35;

        private UISprite? m_icon = null;
        private UILabel? m_title = null;
        private UIButton? m_close = null;
        private UIDragHandle? m_drag = null;
        private MouseEventHandler? m_onClick = null;

        private UIButton? m_btnFollow = null;
        private MouseEventHandler? m_onFollowClick = null;

        private UIButton? m_btnStats = null;
        private MouseEventHandler? m_onStatsClick = null;

        private UIButton? m_btnIssues = null;
        private MouseEventHandler? m_onIssueClick = null;

        private UIButton? m_btnOutside = null;
        private MouseEventHandler? m_onOutsideClick = null;

        public UIButton? m_btnHighlight = null;
        private MouseEventHandler? m_onHighlightClick = null;

        public UIButton? m_btnSettings = null;
        private MouseEventHandler? m_onSettingsClick = null;

        public bool isModal = false;

        public UITitleBar()
        {
        }

        public UIButton? closeButton
        {
            get { return m_close; }
        }

        public string title
        {
            get { return m_title?.text ?? ""; }
            set
            {
                if (m_title is null)
                {
                    SetupControls(value);
                } 
                if (m_title is not null)
                {
                    m_title.text = value;
                    m_title.position = new Vector3(this.width / 2f - m_title.width / 2f, -20f + m_title.height / 2f);
                }
            }
        }

        public void SetOnclickHandler(MouseEventHandler handler)
        {
            m_onClick = handler;
        }

        public void SetFollowHandler(MouseEventHandler handler)
        {
            m_onFollowClick = handler;
        }

        public void SetStatsHandler(MouseEventHandler handler)
        {
            m_onStatsClick = handler;
        }

        public void SetOutsideHandler(MouseEventHandler handler)
        {
            m_onOutsideClick = handler;
        }

        public void SetIssuesHandler(MouseEventHandler handler)
        {
            m_onIssueClick = handler;
        }

        public void SetHighlightHandler(MouseEventHandler handler)
        {
            m_onHighlightClick = handler;
        }

        public void SetSettingsHandler(MouseEventHandler handler)
        {
            m_onSettingsClick = handler;
        }

        private void SetupControls(string sTitle)
        {
            width = parent.width;
            height = iTITLE_HEIGHT;
            isVisible = true;
            canFocus = true;
            isInteractive = true;
            relativePosition = Vector3.zero;
            backgroundSprite = "ButtonMenuDisabled";
            //backgroundSprite = "InfoviewPanel";
            //color = Color.red;

            m_icon = AddUIComponent<UISprite>();
            if (m_icon is not null)
            {
                m_icon.atlas = TransferManagerLoader.LoadResources();
                m_icon.spriteName = "Transfer";
                m_icon.autoSize = false;
                m_icon.width = iBUTTON_HEIGHT - 4;
                m_icon.height = iBUTTON_HEIGHT - 4;
                m_icon.relativePosition = new Vector3(0.0f, 2.0f);
            }

            m_title = AddUIComponent<UILabel>();
            m_title.text = sTitle;
            m_title.textAlignment = UIHorizontalAlignment.Center;
            m_title.position = new Vector3(this.width / 2f - m_title.width / 2f, -20f + m_title.height / 2f);

            float fOffset = iBUTTON_MARGIN;
            m_close = AddUIComponent<UIButton>();
            if (m_close is not null)
            {
                m_close.relativePosition = new Vector3(width - iBUTTON_MARGIN, 2);
                m_close.normalBgSprite = "buttonclose";
                m_close.hoveredBgSprite = "buttonclosehover";
                m_close.pressedBgSprite = "buttonclosepressed";
                m_close.eventClick += (component, param) =>
                {
                    if (m_onClick is not null)
                    {
                        m_onClick(component, param);
                    }
                };
                fOffset += m_close.width;
            }

            if (m_onFollowClick is not null)
            {
                m_btnFollow = AddUIComponent<UIButton>();
                m_btnFollow.name = "m_btnFollow";
                m_btnFollow.tooltip = "Show";
                m_btnFollow.relativePosition = new Vector3(width - fOffset, 2);
                m_btnFollow.width = iBUTTON_HEIGHT;
                m_btnFollow.height = iBUTTON_HEIGHT;
                m_btnFollow.normalBgSprite = "LocationMarkerActiveNormal";
                m_btnFollow.hoveredBgSprite = "LocationMarkerActiveHovered";
                m_btnFollow.focusedBgSprite = "LocationMarkerActiveFocused";
                m_btnFollow.disabledBgSprite = "LocationMarkerActiveDisabled";
                m_btnFollow.pressedBgSprite = "LocationMarkerActivePressed";
                m_btnFollow.eventClick += (component, param) =>
                {
                    if (m_onFollowClick is not null)
                    {
                        m_onFollowClick(component, param);
                    }
                };
                fOffset += m_btnFollow.width;
            }

            if (m_onStatsClick is not null)
            {
                m_btnStats = AddUIComponent<UIButton>();
                m_btnStats.name = "m_btnStats";
                m_btnStats.tooltip = "Show Statistics Panel";
                m_btnStats.width = iBUTTON_HEIGHT;
                m_btnStats.height = iBUTTON_HEIGHT;
                m_btnStats.relativePosition = new Vector3(width - fOffset, 2);
                m_btnStats.normalBgSprite = "ThumbStatistics";
                m_btnStats.color = Color.white;
                m_btnStats.eventClick += (component, param) =>
                {
                    if (m_onStatsClick is not null)
                    {
                        m_onStatsClick(component, param);
                    }
                };
                fOffset += m_btnStats.width;
            }

            if (m_onSettingsClick is not null)
            {
                m_btnSettings = AddUIComponent<UIButton>();
                m_btnSettings.name = "m_btnSettings";
                m_btnSettings.tooltip = "Show list of buildings with TMCE settings";
                m_btnSettings.width = iBUTTON_HEIGHT;
                m_btnSettings.height = iBUTTON_HEIGHT;
                m_btnSettings.relativePosition = new Vector3(width - fOffset, 2);
                m_btnSettings.normalBgSprite = "Options";
                m_btnSettings.color = Color.white;
                m_btnSettings.eventClick += (component, param) =>
                {
                    if (m_onSettingsClick is not null)
                    {
                        m_onSettingsClick(component, param);
                    }
                };
                fOffset += m_btnSettings.width;
            }

            if (m_onIssueClick is not null)
            {
                m_btnIssues = AddUIComponent<UIButton>();
                m_btnIssues.name = "m_btnIssues";
                m_btnIssues.tooltip = "Show Issues Panel";
                m_btnIssues.width = iBUTTON_HEIGHT;
                m_btnIssues.height = iBUTTON_HEIGHT;
                m_btnIssues.relativePosition = new Vector3(width - fOffset, 2);
                m_btnIssues.normalBgSprite = "IconWarning";
                m_btnIssues.color = Color.white;
                m_btnIssues.eventClick += (component, param) =>
                {
                    if (m_onIssueClick is not null)
                    {
                        m_onIssueClick(component, param);
                    }
                };
                fOffset += m_btnIssues.width;
            }

            if (m_onOutsideClick is not null)
            {
                m_btnOutside = AddUIComponent<UIButton>();
                m_btnOutside.name = "m_btnOutside";
                m_btnOutside.tooltip = "Show Outside Connections Panel";
                m_btnOutside.width = iBUTTON_HEIGHT;
                m_btnOutside.height = iBUTTON_HEIGHT;
                m_btnOutside.relativePosition = new Vector3(width - fOffset, 2);
                m_btnOutside.normalBgSprite = "InfoIconOutsideConnections";
                m_btnOutside.color = Color.white;
                m_btnOutside.eventClick += (component, param) =>
                {
                    if (m_onOutsideClick is not null)
                    {
                        m_onOutsideClick(component, param);
                    }
                };
                fOffset += m_btnOutside.width;
            }

            if (m_onHighlightClick is not null)
            {
                m_btnHighlight = AddUIComponent<UIButton>();
                m_btnHighlight.name = "m_btnHighlight";
                m_btnHighlight.tooltip = "Highlight Matches";
                m_btnHighlight.width = iBUTTON_HEIGHT;
                m_btnHighlight.height = iBUTTON_HEIGHT;
                m_btnHighlight.relativePosition = new Vector3(width - fOffset, 2);
                m_btnHighlight.normalBgSprite = "InfoIconLevel";
                m_btnHighlight.color = Color.white;
                m_btnHighlight.eventClick += (component, param) =>
                {
                    if (m_onHighlightClick is not null)
                    {
                        m_onHighlightClick(component, param);
                    }

                    // Update tooltip to new state
                    m_btnHighlight.RefreshTooltip();
                };
                fOffset += m_btnHighlight.width;
            }

            m_drag = AddUIComponent<UIDragHandle>();
            if (m_drag is not null)
            {
                m_drag.width = width - fOffset;
                m_drag.height = height;
                m_drag.relativePosition = Vector3.zero;
                m_drag.target = parent;
            }
        }

        public override void OnDestroy()
        {
            if (m_icon is not null)
            {
                Destroy(m_icon.gameObject);
            }
            if (m_title is not null)
            {
                Destroy(m_title.gameObject);
            }
            if (m_close is not null)
            {
                Destroy(m_close.gameObject);
            }
            if (m_drag is not null)
            {
                Destroy(m_drag.gameObject);
            }

            base.OnDestroy();
        }
    }
}