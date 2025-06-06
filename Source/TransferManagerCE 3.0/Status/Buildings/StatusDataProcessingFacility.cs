using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataProcessingFacility : StatusDataBuilding
    {
        public StatusDataProcessingFacility(CustomTransferReason.Reason reason, BuildingType eBuildingType, ushort BuildingId) :
            base(reason, eBuildingType, BuildingId)
        {
        }

        protected override string CalculateValue(out string tooltip)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            ProcessingFacilityAI? buildingAI = building.Info?.m_buildingAI as ProcessingFacilityAI;
            if (buildingAI is not null)
            {
                int value = 0;
                int bufferSize = 0;
                if (buildingAI.m_inputResource1 == (TransferReason) m_material)
                {
                    value = (building.m_customBuffer2);
                    bufferSize = buildingAI.GetInputBufferSize1(m_buildingId, ref building);
                } 
                else if (buildingAI.m_inputResource2 == (TransferReason) m_material)
                {
                    value = ((building.m_teens << 8) | building.m_youngs);
                    bufferSize = buildingAI.GetInputBufferSize2(m_buildingId, ref building);
                }
                else if (buildingAI.m_inputResource3 == (TransferReason) m_material)
                {
                    value = ((building.m_adults << 8) | building.m_seniors);
                    bufferSize = buildingAI.GetInputBufferSize3(m_buildingId, ref building);
                }
                else if (buildingAI.m_inputResource4 == (TransferReason) m_material)
                {
                    value = ((building.m_education1 << 8) | building.m_education2);
                    bufferSize = buildingAI.GetInputBufferSize4(m_buildingId, ref building);
                }
                else if ((TransferReason) m_material == buildingAI.m_outputResource)
                {
                    value = building.m_customBuffer1;
                    bufferSize = buildingAI.GetOutputBufferSize(m_buildingId, ref building);
                }

                bool bOutgoing = (TransferReason) m_material == buildingAI.m_outputResource;
                WarnText(!bOutgoing, bOutgoing, value, bufferSize);
                tooltip = MakeTooltip(!bOutgoing, value, bufferSize);
                return DisplayValueAsPercent(value, bufferSize);
            }

            tooltip = "";
            return "";
        }

        protected override string CalculateTimer(out string tooltip)
        {
            string sTimer = "";

            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_flags != 0)
            {
                ProcessingFacilityAI? buildingAI = building.Info?.m_buildingAI as ProcessingFacilityAI;
                if (buildingAI is not null)
                {
                    if ((TransferReason) m_material == buildingAI.m_outputResource)
                    {
                        if (building.m_outgoingProblemTimer > 0)
                        {
                            if (string.IsNullOrEmpty(sTimer))
                            {
                                sTimer += " ";
                            }
                            sTimer += "O:" + building.m_outgoingProblemTimer;
                        }
                    }
                    else
                    {
                        if (building.m_incomingProblemTimer > 0)
                        {
                            if (string.IsNullOrEmpty(sTimer))
                            {
                                sTimer += " ";
                            }
                            sTimer += "I:" + building.m_incomingProblemTimer;
                        }
                    }
                }
            }

            tooltip = "";
            return sTimer;
        }
    }
}