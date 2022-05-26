using ColossalFramework.UI;
using UnityEngine;

namespace TransferManagerCE
{
    public class InfoPanelButtons
    {
        /// <summary>
        /// Adds Ploppable RICO settings buttons to building info panels to directly access that building's RICO settings.
        /// </summary>
        internal static void AddInfoPanelButtons()
        {
            // Zoned building (PrivateBuilding) info panel.
            AddInfoPanelButton(UIView.library.Get<ZonedBuildingWorldInfoPanel>(typeof(ZonedBuildingWorldInfoPanel).Name));

            // Service building (PlayerBuilding) info panel.
            AddInfoPanelButton(UIView.library.Get<CityServiceWorldInfoPanel>(typeof(CityServiceWorldInfoPanel).Name));

            // Service building (PlayerBuilding) info panel.
            AddInfoPanelButton(UIView.library.Get<UniqueFactoryWorldInfoPanel>(typeof(UniqueFactoryWorldInfoPanel).Name));

            // Warehouse building
            AddInfoPanelButton(UIView.library.Get<WarehouseWorldInfoPanel>(typeof(WarehouseWorldInfoPanel).Name));
        }


        /// <summary>
        /// Adds a Ploppable RICO button to a building info panel to directly access that building's RICO settings.
        /// The button will be added to the right of the panel with a small margin from the panel edge, at the relative Y position specified.
        /// </summary>
        /// <param name="infoPanel">Infopanel to apply the button to</param>
        private static void AddInfoPanelButton(BuildingWorldInfoPanel infoPanel)
        {
            // Find ProblemsPanel relative position to position button.
            // We'll use 40f as a default relative Y in case something doesn't work.
            UIComponent problemsPanel;
            float relativeY = 40f;

            // Player info panels have wrappers, zoned ones don't.
            UIComponent wrapper = infoPanel.Find("Wrapper");
            if (wrapper == null)
            {
                problemsPanel = infoPanel.Find("ProblemsPanel");
            }
            else
            {
                problemsPanel = wrapper.Find("ProblemsPanel");
            }

            try
            {
                // Position button vertically in the middle of the problems panel.  If wrapper panel exists, we need to add its offset as well.
                relativeY = (wrapper == null ? 0 : wrapper.relativePosition.y) + problemsPanel.relativePosition.y + ((problemsPanel.height - 34) / 2);
            }
            catch
            {
                // Don't really care; just use default relative Y.
                Debug.Log("couldn't find ProblemsPanel relative position");
            }

            UIButton panelButton = infoPanel.component.AddUIComponent<UIButton>();

            // Basic button setup.
            panelButton.size = new Vector2(34, 34);
            panelButton.normalBgSprite = "ToolbarIconGroup6Normal";
            panelButton.normalFgSprite = "Transfer";
            panelButton.focusedBgSprite = "ToolbarIconGroup6Focused";
            panelButton.hoveredBgSprite = "ToolbarIconGroup6Hovered";
            panelButton.pressedBgSprite = "ToolbarIconGroup6Pressed";
            panelButton.disabledBgSprite = "ToolbarIconGroup6Disabled";
            panelButton.name = "TransferManagerCEButton";
            panelButton.tooltip = "Open Transfer Manager CE";
            panelButton.atlas = TransferManagerLoader.LoadResources();

            // Set position.
            float fXOffset = -5f;
            if (infoPanel is ZonedBuildingWorldInfoPanel || infoPanel is CityServiceWorldInfoPanel)
            {
                if (DependencyUtilities.IsPloppableRICORunning())
                {
                    // Next to ploppable RICO button
                    fXOffset += -panelButton.width;
                }
                if (DependencyUtilities.IsRepainterRunning())
                {
                    // Next to Repainter button
                    fXOffset += -panelButton.width;
                }
            }

            panelButton.AlignTo(infoPanel.component, UIAlignAnchor.TopRight);
            panelButton.relativePosition += new Vector3(fXOffset, relativeY, 0f);
            
            // Event handler.
            panelButton.eventClick += (control, clickEvent) =>
            {
                // Select current building in the building details panel and show.
                Debug.Log("Button pressed: " + WorldInfoPanel.GetCurrentInstanceID());
                if (WorldInfoPanel.GetCurrentInstanceID().Building != 0)
                {
                    // Open building panel
                    TransferManagerCEThreading.ShowTransferBuildingPanel(WorldInfoPanel.GetCurrentInstanceID().Building);
                }

                // Manually unfocus control, otherwise it can stay focused until next UI event (looks untidy).
                control.Unfocus();
            };
        }
    }
}
