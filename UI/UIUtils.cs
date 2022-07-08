using UnityEngine;
using ColossalFramework.UI;

namespace TransferManagerCE
{
    public class UIUtils
    {
        public static UIDropDown? AddDropDown(UIHelper helper, string text, string[] items, int selectedIndex = 0, float width = 350)
        {
            UIPanel panelHelper = (UIPanel)helper.self;
            if (panelHelper != null)
            {
                return AddDropDown(panelHelper, text, items, selectedIndex, width);     
            }
            return null;
        }

        public static UIDropDown? AddDropDown(UIComponent parent, string text, string[] items, int selectedIndex = 0, float width = 350)
        {
            UIDropDown? dropDown = null;
            if (parent != null)
            {
                UIPanel? panel = parent.AttachUIComponent(UITemplateManager.GetAsGameObject("OptionsDropdownTemplate")) as UIPanel;
                if (panel != null)
                {
                    panel.autoLayout = true;
                    panel.autoLayoutDirection = LayoutDirection.Horizontal;
                    panel.width = parent.width;
                    panel.height = 40;

                    dropDown = panel.Find<UIDropDown>("Dropdown");

                    // Set text.
                    UILabel label = panel.Find<UILabel>("Label");
                    label.autoSize = false;
                    label.text = text;
                    label.width = 250;
                    label.height = dropDown.height;
                    label.verticalAlignment = UIVerticalAlignment.Middle;

                    // Slightly increase width.
                    dropDown.autoSize = false;
                    dropDown.width = width;
                    dropDown.items = items;
                    dropDown.selectedIndex = selectedIndex;
                }
                
            }

            return dropDown;
        }
    }
}