using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Basic
{
    public class MalAirDistribution
    {
        public static MalAirDistr AirDistribution(int Nelement, double[,] ma_distribution)
        {
            int V_ma = ma_distribution.GetLength(0);
            int H_ma = ma_distribution.GetLength(1);
            double ma_nominate = 0;
            double[,] ma_distribution1 = new double[V_ma, 1];
            double[,] ma_distribution2 = new double[V_ma, 2];
            double midValue = 0;
            int k = 0;
            if (Nelement == 1)
            {
                for (int i = 0; i < V_ma; i++)
                    for (int j = 0; j < H_ma; j++)
                    {
                        ma_distribution1[i, 0] += ma_distribution[i, j];
                    }
                ma_distribution = ma_distribution1;
                H_ma = 1;
            }
            else // then Nelement=2
            {
                decimal Nmid = (decimal)H_ma / 2;
                if ((H_ma % 2) == 0)
                {
                    for (int i = 0; i < V_ma; i++)
                        for (int j = 0; j < H_ma; j++)
                        {
                            k = (int)decimal.Floor(j / Nmid);
                            ma_distribution2[i, k] += ma_distribution[i, j];
                        }
                }
                else
                {
                    for (int i = 0; i < V_ma; i++)
                    {
                        for (int j = 0; j < H_ma; j++)
                        {
                            k = (int)decimal.Floor(j / Nmid);
                            if (j + 1 < Nmid)
                            {
                                ma_distribution2[i, 0] += ma_distribution[i, j];
                            }
                            else if (j + 1 > Nmid && j + 1 < Nmid + 1)
                            {
                                midValue = ma_distribution[i, j] / 2;
                                ma_distribution2[i, 0] += midValue;
                                ma_distribution2[i, 1] += midValue;
                            }
                            else
                            {
                                ma_distribution2[i, 1] += ma_distribution[i, j];
                            }
                        }
                    }

                }

                ma_distribution = ma_distribution2;
                H_ma = 2;
            }
            for (int i = 0; i < V_ma; i++)
                for (int j = 0; j < H_ma; j++)
                {
                    ma_nominate += ma_distribution[i, j];
                }
            return new MalAirDistr { distribution = ma_distribution, H=H_ma, V = V_ma, nominate = ma_nominate };
        }
    }
}
