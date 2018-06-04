using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Basic
{
    public class AirHTC_CAL
    {
        public static double[,] alpha_cal(double[,] ha, double[,] Va, double Va_ave, double Vel_ave, double za, int curve, GeometryInput geoInput_air, int hexType, int N_tube, int Nelement)
        {

            for (int i = 0; i < N_tube; i++)
            {
                for (int j = 0; j < Nelement; j++)
                {
                    if (i == 0 && j == 0)
                    {
                        ha[i, j] = AirHTC.alpha1(Va[i, j] * (Vel_ave / Va_ave), za, curve, geoInput_air, hexType).ha;
                    }
                    else
                    {
                        for (int ii = 0; ii <= i; ii++)
                        {
                            for (int jj = 0; jj < Nelement; jj++)
                            {
                                //if (VaDistri.Va[i, j] == VaDistri.Va[ii, jj])
                                if (Math.Abs((Va[i, j] - Va[ii, jj]) / Va[i, j]) < 0.001)
                                {
                                    ha[i, j] = ha[ii, jj];
                                    break;
                                }
                            }
                        }
                        if (ha[i, j] == 0)
                        {
                            ha[i, j] = AirHTC.alpha1(Va[i, j] * (Vel_ave / Va_ave), za, curve, geoInput_air, hexType).ha;
                        }
                    }
                }
            }
            return ha;
        }


    }
}
