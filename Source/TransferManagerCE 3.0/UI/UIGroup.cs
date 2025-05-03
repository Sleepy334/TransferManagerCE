using ColossalFramework.UI;
using UnityEngine;

namespace TransferManagerCE
{
    public class UIGroup : UIPanel
    {
        public UILabel? m_label = null;
        public UIPanel? m_content = null;

        public static UIGroup? AddGroup(UIComponent parent, string text, float fTextScale, float width, float height)
        {
            UIGroup? uiGroup = null;
            if (!string.IsNullOrEmpty(text))
            {
                uiGroup = parent.AddUIComponent<UIGroup>();
                if (uiGroup is not null)
                {
                    uiGroup.autoLayout = true;
                    uiGroup.autoLayoutDirection = LayoutDirection.Vertical;
                    uiGroup.autoLayoutPadding = new RectOffset(4, 4, 2, 2);
                    uiGroup.autoSize = false;
                    uiGroup.width = width;
                    uiGroup.height = height;
                    uiGroup.backgroundSprite = "InfoviewPanel";
                    //uiGroup.color = new Color32(96, 96, 96, 255);
                    uiGroup.color = new Color32(116, 116, 116, 255);

                    uiGroup.m_label = uiGroup.AddUIComponent<UILabel>();
                    uiGroup.m_label.text = text;
                    uiGroup.m_label.textScale = fTextScale;
                    uiGroup.m_label.opacity = 1.0f;

                    uiGroup.m_content = uiGroup.AddUIComponent<UIPanel>();
                    uiGroup.m_content.autoLayout = true;
                    uiGroup.m_content.autoLayoutDirection = LayoutDirection.Vertical;
                    uiGroup.m_content.autoLayoutPadding = new RectOffset(30, 0, 0, 0);
                    uiGroup.m_content.autoSize = true;// false;
                }
            }

            return uiGroup;
        }

        public string Text
        {
            get { return m_label.text; }
            set { m_label.text = value; }
        }
    }
}