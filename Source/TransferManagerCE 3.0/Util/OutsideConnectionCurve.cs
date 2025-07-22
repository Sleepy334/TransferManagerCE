namespace TransferManagerCE.Util
{
    public class OutsideConnectionCurve : PercentCurveLookup
    {
        public OutsideConnectionCurve()
        {
            // Scale values so that applied "strength" is linear
            AddPoint(0, 1024);
            AddPoint(1,  512);
            AddPoint(2,  256);
            AddPoint(3,  128);
            AddPoint(4,   64);
            AddPoint(5,   32);
            AddPoint(6,   16);
            AddPoint(7,    8);
            AddPoint(8,    4);
            AddPoint(9,    2);
            AddPoint(10,   1);
        }
    }
}
