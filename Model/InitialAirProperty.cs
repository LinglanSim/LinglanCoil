using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class InitialAirProperty
    {
        public static double[,] AirTemp(int[] Ntube, int Nrow, double tai, double te, string AirDirection)
        {
            double[,] ta = new double[Ntube[0], Nrow + 1];

            //if (AirDirection == "DowntoUp") //For refrigerator
            //{
            for (int k = 0; k < Nrow + 1; k++)
                for (int j = 0; j < Ntube[0]; j++)
                    ta[j, k] = tai - (tai - te) / Nrow * k;
            //}

            return ta;
        }
    }
}
