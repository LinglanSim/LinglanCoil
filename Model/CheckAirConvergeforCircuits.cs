using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Basic;

namespace Model
{
    public class CheckAirConvergeforCircuits
    {
        public static CheckAir CheckAirConverge(int Nrow, int N_tube, int Nelement, double[, ,] tain, double[, ,] RHin, double[, ,] taout, double[, ,] RHout)
        {
            CheckAir r=new CheckAir();
            int index = 0;
            bool flag = false;
            double err = 0.01;
            double dev = 0;
            double dev1 = 0;
            double devsum = 0;
            double devsum1 = 0;
            double[, ,] temp = tain;
            double[, ,] temp1 = RHin;
            
            for (int j = N_tube-1; j >= 0; j--)  //                    
            { 
                for (int k = 0; k < Nrow - 1; k++)
                    //for (int j = 1; j <= N_tube; j++)
                    for (int i = 0; i < Nelement; i++)
                    {
                        devsum = 0;
                        devsum1 = 0;

                        dev = taout[i, j, k] - tain[i, j, k + 1];
                        devsum += dev;
                        temp[i, j, k + 1] = tain[i, j, k + 1] + (taout[i, j, k] == 0 ? 0 : devsum);

                        dev1 = RHout[i, j, k] - RHin[i, j, k + 1];
                        devsum1 += dev1;
                        temp1[i, j, k + 1] = RHin[i, j, k + 1] + (RHout[i, j, k] == 0 ? 0 : devsum1);

                        if (Math.Abs(dev) > err && Math.Abs(dev1) > err)
                        {
                            index += (taout[i, j, k] == 0 ? 0 : 1);
                        }
                    }
            }

            for (int j = 0; j < N_tube; j++)  //                                
                for (int i = 0; i < Nelement; i++)
                {
                    temp[i, j, Nrow] = (taout[i, j, Nrow - 1] == 0 ? tain[i, j, Nrow] : taout[i, j, Nrow - 1]);
                    temp1[i, j, Nrow] = (RHout[i, j, Nrow - 1] == 0 ? RHin[i, j, Nrow] : RHout[i, j, Nrow - 1]);
                }


            if (index == 0) flag = true;
            else
            {
                tain = temp;
                RHin = temp1;
            }
                
            //converge, or new good iter

            return r = new CheckAir { flag = flag, ta = tain, RH = RHin };

        }

    }
}

