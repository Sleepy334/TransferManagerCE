namespace TransferManagerCE
{
    public class PathUnitMaintenance
    {
        public static void ReleaseBrokenPathUnits()
        {
            PathManager.instance.WaitForAllPaths();

            // Look for PathUnit's that have m_simulationFlags set but no references any more.
            for (int i = 0; i < PathManager.instance.m_pathUnits.m_size; ++i)
            {
                PathUnit unit = PathManager.instance.m_pathUnits.m_buffer[i];
                if (unit.m_simulationFlags != 0 && unit.m_referenceCount == 0)
                {
                    PathManager.instance.ReleasePath((uint)i);
                }
            }
        }
    }
}

