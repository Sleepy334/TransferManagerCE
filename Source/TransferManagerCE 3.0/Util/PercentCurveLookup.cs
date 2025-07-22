using UnityEngine;

namespace TransferManagerCE
{
    public class PercentCurveLookup
    {
        private float[] m_y = new float[11];

        public void AddPoint(int index, float value)
        {
            m_y[index] = value;
        }

        // x_value betwen [0 .. 100]
        public float GetCurveValue(int x_value)
        {
            if (x_value == 100)
            {
                return m_y[10];
            }
            else
            {

                float x1 = Mathf.Floor(x_value / 10 * 10);

                // Check if it is exactly on a point so we dont need to scale.
                if ((int)x1 == x_value)
                {
                    int index = (int)x1 / 10;
                    return m_y[index];
                }

                float x2 = x1 + 10;
                int index1 = (int) x1 / 10;
                int index2 = index1 + 1;
                float y1 = m_y[index1];
                float y2 = m_y[index2];
                float y3 = Interpolate(x1, x2, y1, y2, x_value);
                return y3;
            }
        }

        private float Interpolate(float x1, float x2, float y1, float y2, float x3)
        {
            float fSlope = (y2 - y1) / (x2 - x1);
            return fSlope * (x3 - x1) + y1;
        }
    }
}