using System;
using System.Collections.Generic;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataProcessingFacility : StatusData
    {
        public StatusDataProcessingFacility(TransferReason reason, BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) :
            base(reason, eBuildingType, BuildingId, responder, target)
        {
        }

        protected override string CalculateValue()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            ProcessingFacilityAI? buildingAI = building.Info?.m_buildingAI as ProcessingFacilityAI;
            if (buildingAI is not null)
            {
                double dValue = 0;
                double dBufferSize = 0;
                if (buildingAI.m_inputResource1 == m_material)
                {
                    dValue = (building.m_customBuffer2 * 0.001);
                    dBufferSize = buildingAI.GetInputBufferSize1(m_buildingId, ref building) * 0.001;
                } 
                else if (buildingAI.m_inputResource2 == m_material)
                {
                    dValue = ((building.m_teens << 8) | building.m_youngs) * 0.001;
                    dBufferSize = buildingAI.GetInputBufferSize2(m_buildingId, ref building) * 0.001;
                }
                else if (buildingAI.m_inputResource3 == m_material)
                {
                    dValue = ((building.m_adults << 8) | building.m_seniors) * 0.001;
                    dBufferSize = buildingAI.GetInputBufferSize3(m_buildingId, ref building) * 0.001;
                }
                else if (buildingAI.m_inputResource4 == m_material)
                {
                    dValue = ((building.m_education1 << 8) | building.m_education2) * 0.001;
                    dBufferSize = buildingAI.GetInputBufferSize4(m_buildingId, ref building) * 0.001;
                }
                else if (m_material == buildingAI.m_outputResource)
                {
                    dValue = building.m_customBuffer1 * 0.001;
                    dBufferSize = buildingAI.GetOutputBufferSize(m_buildingId, ref building) * 0.001;
                }
                return Math.Round(dValue, 1).ToString("N1") + "/" + Math.Round(dBufferSize).ToString("N0");
            }
            return 0.ToString();
        }

        protected override string CalculateTimer()
        {
            string sTimer = base.GetTimer();

            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_flags != 0)
            {
                ProcessingFacilityAI? buildingAI = building.Info?.m_buildingAI as ProcessingFacilityAI;
                if (buildingAI is not null)
                {
                    if (m_material == buildingAI.m_outputResource)
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

            return sTimer;
        }

        protected override string CalculateTarget()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            ProcessingFacilityAI? buildingAI = building.Info?.m_buildingAI as ProcessingFacilityAI;
            if (buildingAI is not null && m_material == buildingAI.m_outputResource)
            { 
                return ""; // A processing plant outgoing will never have a responder
            }
            else
            {
                return base.CalculateTarget();
            }
        }

        protected override string CalculateResponder()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            ProcessingFacilityAI? buildingAI = building.Info?.m_buildingAI as ProcessingFacilityAI;
            if (buildingAI is not null && m_material == buildingAI.m_outputResource)
            {
                return ""; // A processing plant outgoing will never have a responder
            }
            else
            {
                return base.CalculateResponder();
            }
        }
    }
}