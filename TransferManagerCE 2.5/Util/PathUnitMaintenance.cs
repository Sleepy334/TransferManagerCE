namespace TransferManagerCE
{
    public class PathUnitMaintenance
    {
        public static int ReleaseBrokenPathUnits()
        {
            PathManager.instance.WaitForAllPaths();

            // Local reference
            PathUnit[] PathUnits = PathManager.instance.m_pathUnits.m_buffer;

            // Save current count
            int iPathUnitCount = PathManager.instance.m_pathUnitCount;

            // Look for PathUnit's that have m_simulationFlags set but no references any more.
            uint uiSize = PathManager.instance.m_pathUnits.m_size;
            for (uint i = 0; i < uiSize; ++i)
            {
                PathUnit unit = PathUnits[i];
                if (unit.m_simulationFlags != 0 && unit.m_referenceCount == 0)
                {
                    PathManager.instance.ReleasePath(i);
                }
            }

            return iPathUnitCount - PathManager.instance.m_pathUnitCount;
        }
    }
}

