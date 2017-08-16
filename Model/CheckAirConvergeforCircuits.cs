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
        public static CheckAir CheckAirConverge(int Nrow, int N_tube, int Nelement, double[, ,] tain, double[, ,] taout)
        {
            CheckAir r=new CheckAir();
            int index = 0;
            bool flag = false;
            double err = 0.01;
            double dev = 0;
            double devsum = 0;
            double[, ,] temp = tain;
            
            for (int j = N_tube-1; j >= 0; j--)  //                    
            {
                devsum = 0;
                for (int k = 0; k < Nrow - 1; k++)
                    //for (int j = 1; j <= N_tube; j++)
                    for (int i = 0; i < Nelement; i++)
                    {
                        dev = taout[i, j, k] - tain[i, j, k + 1];
                        devsum += dev;
                        temp[i, j, k + 1] = tain[i, j, k + 1] + (taout[i, j, k] == 0 ? 0 : devsum);
                        if (Math.Abs(dev) > err)
                        {
                            index += (taout[i, j, k] == 0 ? 0 : 1);
                        }
                    }
            }

            for (int j = 0; j < N_tube; j++)  //                                
                for (int i = 0; i < Nelement; i++)
                    temp[i, j, Nrow] = (taout[i, j, Nrow - 1] == 0 ? tain[i, j, Nrow] : taout[i, j, Nrow - 1]);

            if (index == 0) flag = true;
            else tain = temp;
            //converge, or new good iter
              
            return r = new CheckAir { flag = flag, ta = tain };

        }

    }
}

