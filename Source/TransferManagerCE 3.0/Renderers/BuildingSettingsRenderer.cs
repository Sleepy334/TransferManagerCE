using System.Collections.Generic;
using TransferManagerCE.Settings;
using UnityEngine;
using static RenderManager;
using TransferManagerCE.UI;

namespace TransferManagerCE
{
    public class BuildingSettingsRenderer : SimulationManagerBase<BuildingSettingsRenderer, MonoBehaviour>, IRenderableManager
    {
        private static bool s_rendererRegistered = false;

        public static void RegisterRenderer()
        {
            // Used to draw path connection graph, only add this once
            if (!s_rendererRegistered)
            {
                SimulationManager.RegisterManager(BuildingSettingsRenderer.instance);
                s_rendererRegistered = true;
            }
        }

        protected override void BeginOverlayImpl(CameraInfo cameraInfo)
        {
            base.BeginOverlayImpl(cameraInfo);

            if (SettingsPanel.IsVisible() &&
                ModSettings.GetSettings().HighlightSettingsState == 1)
            {
                HighlightBuildings(cameraInfo);
            }
        }

        private void HighlightBuildings(CameraInfo cameraInfo)
        {
            Building[] BuildingBuffer = BuildingManager.instance.m_buildings.m_buffer;

            List<SettingsData> list = SettingsPanel.Instance.GetSettingsList();
            foreach (SettingsData setting in list) 
            {
                RendererUtils.HighlightBuilding(BuildingBuffer, setting.GetBuildingId(), cameraInfo, Color.red);
            }
        }
    }
}
