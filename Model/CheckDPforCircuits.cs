using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Basic;

namespace Model
{
    public class CheckDPforCircuits
    {
        public static CheckDP CheckDPConverge(CalcResult[]res_cir2, double iterforPri, int flag_ciro, double[] mr_cir, CalcResult[] r, int Ncir)
        {
            CheckDP res = new CheckDP();
            int index = 0;
            bool flag = true;//means converge
            double err = iterforPri == 0 ? 0.2 : 0.02;
            double dev = 0;
            double devsum = 0;
            //double[] f = new double[Ncir * (Ncir - 1) + 1];
            //double[] a = new double[Ncir * (Ncir - 1) + 1];
            //double[] y = new double[Ncir * (Ncir - 1) + 1];
            double[,] f = new double[Ncir + 1, Ncir + 1];
            double[,] a = new double[Ncir + 1, Ncir + 1];
            double[,] y = new double[Ncir + 1, Ncir + 1];
            double[] step = new double[Ncir + 1];
            double N = 0;

            if (flag_ciro == 1) N = 2.2; else N = 1.8; //1.8
            for (int i = 0; i < Ncir; i++)
            {
                if (res_cir2[res_cir2.Length - 1] != null)
                {
                    if (res_cir2[i].Tro - res_cir2[i].Tri > 30 && iterforPri > 0) err = 0.3;
                }
                else
                {
                    if (r[i].x_o > 1.2 && iterforPri > 0) err = 0.3;
                }
                   
            }

            for (int i = 0; i < Ncir - 1; i++)
            {
                if (Math.Abs((r[i].DP - r[i + 1].DP) / r[i + 1].DP) > err)
                    flag = false;
            }
            if (!flag)                //Jacobian iteration
            {
                for (int i = 0; i < Ncir - 1; i++)
                {
                    for (int j = i + 1; j < Ncir; j++)
                    {
                        f[i, j] = r[i].DP - r[j].DP;
                        a[i, j] = -r[i].DP * N / mr_cir[i] - r[i + 1].DP * N / mr_cir[i + 1];//1.8 to be modified
                        y[i, j] = -f[i, j] / a[i, j];
                        y[j, i] = -y[i, j];
                    }
                }

                for (int i = 0; i < Ncir; i++)
                {
                    for (int j = 0; j < Ncir; j++)
                    {
                        step[i] += y[i, j];
                    }
                    mr_cir[i] -= step[i] / (Ncir - 1); 
                }          
            }

            return res = new CheckDP { flag = flag, mr = mr_cir };

        }

    }
}

