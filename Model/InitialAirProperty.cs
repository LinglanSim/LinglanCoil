using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class InitialAirProperty
    {
        public static double[, ,] AirTemp(int element, int[] Ntube, int Nrow, double tai, double te, string AirDirection)
        {
            double[, ,] ta = new double[element, Ntube[0], Nrow + 1];

            //if (AirDirection == "DowntoUp") //For refrigerator
            //{
            if (AirDirection=="Parallel")
            {
                for (int i = 0; i < element; i++)
                    for (int j = 0; j < Ntube[0]; j++)
                        ta[i, j, 0] = tai;
            }
            else // Counter
            {
                for (int k = 0; k < Nrow + 1; k++)
                    for (int j = 0; j < Ntube[0]; j++)
                        for (int i = 0; i < element; i++)
                            ta[i, j, k] = tai - (tai - te) / Nrow * k;
            }

            //}

            return ta;
        }
        public static double[, ,] RHTemp(int element, int[] Ntube, int Nrow, double RHi, double te, string AirDirection)
        {
            double[, ,] RH = new double[element, Ntube[0], Nrow + 1];

            //if (AirDirection == "DowntoUp") //For refrigerator
            //{
            if(AirDirection=="Parallel")
            {
                for (int i = 0; i < element; i++)
                    for (int j = 0; j < Ntube[0]; j++)
                        RH[i, j, 0] = RHi;
            }
            else //Counter
            {
                for (int k = 0; k < Nrow + 1; k++)
                    for (int j = 0; j < Ntube[0]; j++)
                        for (int i = 0; i < element; i++)
                            RH[i, j, k] = RHi;
            }

            //}

            return RH;
        }
    }
}
