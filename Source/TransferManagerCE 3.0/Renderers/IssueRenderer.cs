using System.Collections.Generic;
using TransferManagerCE.Settings;
using UnityEngine;
using static RenderManager;
using TransferManagerCE.UI;
using static TransferManagerCE.UI.TransferIssuePanel;
using SleepyCommon;

namespace TransferManagerCE
{
    public class IssueRenderer : SimulationManagerBase<IssueRenderer, MonoBehaviour>, IRenderableManager
    {
        private static bool s_rendererRegistered = false;

        public static void RegisterRenderer()
        {
            // Used to draw path connection graph, only add this once
            if (!s_rendererRegistered)
            {
                SimulationManager.RegisterManager(IssueRenderer.instance);
                s_rendererRegistered = true;
            }
        }

        protected override void BeginOverlayImpl(CameraInfo cameraInfo)
        {
            base.BeginOverlayImpl(cameraInfo);

            if (TransferIssuePanel.IsVisible() &&
                ModSettings.GetSettings().HighlightIssuesState == 1)
            {
                switch ((TransferIssuePanel.TabOrder) TransferIssuePanel.Instance.GetSelectTabIndex())
                {
                    case TabOrder.TAB_ISSUES:
                        {
                            HighlightBuildingIssues(cameraInfo);
                            break;
                        }
                    case TabOrder.TAB_PATHING:
                        {
                            HighlightPathingIssues(cameraInfo);
                            break;
                        }
                    case TabOrder.TAB_ROAD_ACCESS:
                        {
                            HighlightNoRoadAccess(cameraInfo);
                            break;
                        }
                }
            }
        }

        private void HighlightBuildingIssues(CameraInfo cameraInfo)
        {
            Building[] BuildingBuffer = BuildingManager.instance.m_buildings.m_buffer;
            Vehicle[] VehicleBuffer = VehicleManager.instance.m_vehicles.m_buffer;

            TransferIssueHelper helper = TransferIssuePanel.Instance.GetIssueHelper();
            if (helper is not null)
            {
                List<TransferIssueContainer> filteredIssues = helper.GetFilteredIssues();
                foreach (TransferIssueContainer issue in filteredIssues)
                {
                    RendererUtils.HighlightBuilding(BuildingBuffer, issue.GetBuildingId(), cameraInfo, issue.GetColor());

                    if (issue.HasVehicle())
                    {
                        RendererUtils.HighlightVehicle(VehicleBuffer, cameraInfo, issue.GetVehicleId(), KnownColor.green);
                    }
                }
            }
            else
            {
                CDebug.Log($"m_issueHelper is null");
            }
        }

        private void HighlightPathingIssues(CameraInfo cameraInfo)
        {
            Building[] BuildingBuffer = BuildingManager.instance.m_buildings.m_buffer;

            List<PathingContainer> listPathing = TransferIssuePanel.Instance.GetPathingIssues();

            foreach (PathingContainer pathIssue in listPathing)
            {
                if (pathIssue.m_source.Building != 0)
                {
                    RendererUtils.HighlightBuilding(BuildingBuffer, pathIssue.m_source.Building, cameraInfo, KnownColor.magenta);
                }
                if (pathIssue.m_target.Building != 0)
                {
                    RendererUtils.HighlightBuilding(BuildingBuffer, pathIssue.m_target.Building, cameraInfo, KnownColor.magenta);
                }
            }
        }

        private void HighlightNoRoadAccess(CameraInfo cameraInfo)
        {
            Building[] BuildingBuffer = BuildingManager.instance.m_buildings.m_buffer;

            TransferIssueHelper helper = TransferIssuePanel.Instance.GetIssueHelper();
            if (helper is not null)
            {
                List<RoadAccessData> roadAccessIssues = helper.GetRoadAccess();
                foreach (RoadAccessData issue in roadAccessIssues)
                {
                    RendererUtils.HighlightBuilding(BuildingBuffer, issue.m_source.Building, cameraInfo, KnownColor.purple);
                }
            }
        }
    }
}
