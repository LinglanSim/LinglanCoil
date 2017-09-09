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
        public static CheckAir CheckAirConverge(int Nrow, int N_tube, int Nelement, double[,] tain, double[,] taout)
        {
            CheckAir r=new CheckAir();
            int index = 0;
            bool flag = false;
            double err = 0.01;
            double dev = 0;
            double devsum = 0;
            double[,] temp = tain;
            
            for (int j = N_tube-1; j >= 0; j--)  //                    
            {
                devsum = 0;
                for (int k = 0; k < Nrow - 1; k++)
                //for (int j = 1; j <= N_tube; j++)
                {
                    dev = taout[j, k] - tain[j, k + 1];
                    devsum += dev;
                    temp[j, k + 1] = tain[j, k + 1] + (taout[j, k] == 0 ? 0 : devsum);
                    if (Math.Abs(dev) > err)
                    {
                        index += (taout[j, k] == 0 ? 0 : 1);
                    }
                }
            }

            for (int j = 0; j < N_tube; j++)  //                                
                temp[j, Nrow] = (taout[j, Nrow - 1] == 0 ? tain[j, Nrow] : taout[j, Nrow - 1]);

            if (index == 0) flag = true;
            else tain = temp;
            //converge, or new good iter
              
            return r = new CheckAir { flag = flag, ta = tain };

        }

    }
}

