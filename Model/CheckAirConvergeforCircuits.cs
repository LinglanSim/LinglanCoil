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
        public static CheckAir CheckAirConverge(int[,] TotalDirection, int Nrow, int N_tube, int Nelement, double[, ,] tain, double[, ,] RHin, CalcResult[] r)//, double[, ,] taout, double[, ,] RHout)
        {
            CheckAir rr=new CheckAir();
            int index = 0;
            bool flag = false;
            double err = 0.01;
            double dev = 0;
            double dev1 = 0;
            double devsum = 0;
            double devsum1 = 0;
            double[, ,] temp = tain;
            double[, ,] temp1 = RHin;
            double[, ,] taout = new double[Nelement, N_tube, Nrow];
            double[, ,] RHout = new double[Nelement, N_tube, Nrow];

            for (int m = 0; m < r.Count(s => s != null); m++)
                for (int k = 0; k < Nrow; k++)
                    for (int j = 0; j < N_tube; j++)
                        for (int i = 0; i < Nelement; i++)
                        {
                            taout[i, j, k] = r[m].Tao_Detail[i, j, k] == 0 ? taout[i, j, k] : r[m].Tao_Detail[i, j, k];
                        }

            for (int j = N_tube - 1; j >= 0; j--) 
            {
                for (int k = 0; k < Nrow - 1; k++)
                //for (int j = 1; j <= N_tube; j++)
                {
                    //TotalDirection: 1 means in, 0 means out; ruhao 20180314
                    if (TotalDirection[j, k] == TotalDirection[j, k + 1])
                    {
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
                    else
                    {
                        for (int i = 0; i < Nelement; i++)
                        {
                            devsum = 0;
                            devsum1 = 0;

                            dev = taout[i, j, k] - tain[Nelement - i - 1, j, k + 1];
                            devsum += dev;
                            temp[Nelement - i - 1, j, k + 1] = tain[Nelement - i - 1, j, k + 1] + (taout[i, j, k] == 0 ? 0 : devsum);

                            dev1 = RHout[i, j, k] - RHin[Nelement - i - 1, j, k + 1];
                            devsum1 += dev1;
                            temp1[Nelement - i - 1, j, k + 1] = RHin[Nelement - i - 1, j, k + 1] + (RHout[i, j, k] == 0 ? 0 : devsum1);

                            if (Math.Abs(dev) > err && Math.Abs(dev1) > err)
                            {
                                index += (taout[i, j, k] == 0 ? 0 : 1);

                            }

                        }
                    }
                }
            }
            
            for (int j = 0; j < N_tube; j++)
                for (int i = 0; i < Nelement; i++)
                {
                    temp[i, j, Nrow] = (taout[i, j, Nrow - 1] == 0 ? tain[i, j, Nrow] : taout[i, j, Nrow - 1]);
                    temp1[i, j, Nrow] = (RHout[i, j, Nrow - 1] == 0 ? RHin[i, j, Nrow] : RHout[i, j, Nrow - 1]);
                }
            //to avoid back and forth of iteration especially in 2ph critical region, ruhao, 20180315
            //in the typical case of "MinNout", tube of [*, 19, 1], in first itertion is 2ph, while next iteration is superheat...  
            //to be modified..
            if (index < 0.3 * N_tube * Nelement) flag = true; //&& (r[r.Count() - 1].x_o > 1 || r[r.Count() - 1].x_o < 0)) index == 0
            else
            {
                tain = temp;
                RHin = temp1;
            }

            //converge, or new good iter
            return rr = new CheckAir { flag = flag, ta = tain, RH = RHin };

        }

    }
}

