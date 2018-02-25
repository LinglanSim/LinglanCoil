using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Basic;

namespace Model
{
    class DistributionConvert
    {
        public static AirDistribution VaConvert(double[,] Vai, int N_tube, int Nelement)
        {
            AirDistribution res = new AirDistribution();
            int Rowi = Vai.GetLength(0);
            int Columni = Vai.GetLength(1);
            double R1o, R2o, C1o, C2o, R11i, R12i, R21i, R22i, C11i, C12i, C21i, C22i;
            double A1, A2, A3, A4, A5, A6, A7, A8, A9;
            int ii1, ii2, ji1, ji2;
            double[,] Vao = new double[N_tube, Nelement];
            double avo = 0;
            for (int io = 0; io < N_tube; io++)
            {
                for (int jo = 0; jo < Nelement; jo++)
                {
                    R1o = io * (1.0 / N_tube);
                    R2o = (io + 1) * (1.0 / N_tube);
                    C1o = jo * (1.0 / Nelement);
                    C2o = (jo + 1) * (1.0 / Nelement);
                    ii1 = (int)Math.Floor(R1o / (1.0 / Rowi));
                    ii2 = (io == N_tube - 1 ? Rowi - 1 : (int)Math.Floor(R2o / (1.0 / Rowi)));
                    ji1 = (int)Math.Floor(C1o / (1.0 / Columni));
                    ji2 = (jo == Nelement - 1 ? Columni - 1 : (int)Math.Floor(C2o / (1.0 / Columni)));
                    R11i = ii1 * (1.0 / Rowi);
                    R12i = (ii1 + 1) * (1.0 / Rowi);
                    R21i = ii2 * (1.0 / Rowi);
                    R22i = (ii2 + 1) * (1.0 / Rowi);
                    C11i = ji1 * (1.0 / Columni);
                    C12i = (ji1 + 1) * (1.0 / Columni);
                    C21i = ji2 * (1.0 / Columni);
                    C22i = (ji2 + 1) * (1.0 / Columni);

                    if (ii2 - ii1 == 0)
                    {
                        if (ji2 - ji1 == 0) Vao[io, jo] = Vai[ii1, ji1];
                        if (ji2 - ji1 == 1)
                        {
                            A1 = Vai[ii1, ji1] * (1.0 / N_tube) * (C12i - C1o);
                            A2 = Vai[ii1, ji2] * (1.0 / N_tube) * (C2o - C21i);
                            Vao[io, jo] = (A1 + A2) / (1.0 / N_tube / Nelement);
                        }
                        if (ji2 - ji1 >= 2)
                        {
                            A1 = Vai[ii1, ji1] * (1.0 / N_tube) * (C12i - C1o);
                            A2 = Vai[ii1, ji2] * (1.0 / N_tube) * (C2o - C21i);
                            A3 = 0;
                            for (int i = ji1 + 1; i < ji2; i++)
                            {
                                A3 += Vai[ii1, i] * (1.0 / Columni / N_tube);
                            }
                            Vao[io, jo] = (A1 + A2 + A3) / (1.0 / N_tube / Nelement);
                        }
                    }
                    else
                    {
                        if (ii2 - ii1 == 1)
                        {
                            if (ji2 - ji1 == 0)
                            {
                                A1 = Vai[ii1, ji1] * (1.0 / Nelement) * (R12i - R1o);
                                A2 = Vai[ii2, ji1] * (1.0 / Nelement) * (R2o - R21i);
                                Vao[io, jo] = (A1 + A2) / (1.0 / N_tube / Nelement);
                            }
                            if (ji2 - ji1 == 1)
                            {
                                A1 = Vai[ii1, ji1] * (R12i - R1o) * (C12i - C1o);
                                A2 = Vai[ii1, ji2] * (R12i - R1o) * (C2o - C21i);
                                A3 = Vai[ii2, ji1] * (R2o - R21i) * (C12i - C1o);
                                A4 = Vai[ii2, ji2] * (R2o - R21i) * (C2o - C21i);
                                Vao[io, jo] = (A1 + A2 + A3 + A4) / (1.0 / N_tube / Nelement);
                            }
                            if (ji2 - ji1 >= 2)
                            {
                                A1 = Vai[ii1, ji1] * (R12i - R1o) * (C12i - C1o);
                                A2 = Vai[ii1, ji2] * (R12i - R1o) * (C2o - C21i);
                                A3 = Vai[ii2, ji1] * (R2o - R21i) * (C12i - C1o);
                                A4 = Vai[ii2, ji2] * (R2o - R21i) * (C2o - C21i);
                                A5 = 0;
                                A6 = 0;
                                for (int i = ji1 + 1; i < ji2; i++)
                                {
                                    A5 += Vai[ii1, i] * (R12i - R1o) * (1.0 / Columni);
                                    A6 += Vai[ii2, i] * (R2o - R21i) * (1.0 / Columni);
                                }
                                Vao[io, jo] = (A1 + A2 + A3 + A4 + A5 + A6) / (1.0 / N_tube / Nelement);
                            }
                        }
                        else
                        {
                            if (ji2 - ji1 == 0)
                            {
                                A1 = Vai[ii1, ji1] * (R12i - R1o) * (1.0 / Nelement);
                                A2 = Vai[ii2, ji1] * (R2o - R21i) * (1.0 / Nelement);
                                A3 = 0;
                                for (int i = ii1 + 1; i < ii2; i++)
                                {
                                    A3 += Vai[i, ji1] * (1.0 / Rowi) * (1.0 / Nelement);
                                }
                                Vao[io, jo] = (A1 + A2 + A3) / (1.0 / N_tube / Nelement);
                            }
                            if (ji2 - ji1 == 1)
                            {
                                A1 = Vai[ii1, ji1] * (R12i - R1o) * (C12i - C1o);
                                A2 = Vai[ii1, ji2] * (R12i - R1o) * (C2o - C21i);
                                A3 = Vai[ii2, ji1] * (R2o - R21i) * (C12i - C1o);
                                A4 = Vai[ii2, ji2] * (R2o - R21i) * (C2o - C21i);
                                A5 = 0;
                                A6 = 0;
                                for (int i = ii1 + 1; i < ii2; i++)
                                {
                                    A5 += Vai[i, ji1] * (1.0 / Rowi) * (C12i - C1o);
                                    A6 += Vai[i, ji2] * (1.0 / Rowi) * (C2o - C21i);
                                }
                                Vao[io, jo] = (A1 + A2 + A3 + A4 + A5 + A6) / (1.0 / N_tube / Nelement);
                            }
                            if (ji2 - ji1 >= 2)
                            {
                                A1 = Vai[ii1, ji1] * (R12i - R1o) * (C12i - C1o);
                                A2 = Vai[ii1, ji2] * (R12i - R1o) * (C2o - C21i);
                                A3 = Vai[ii2, ji1] * (R2o - R21i) * (C12i - C1o);
                                A4 = Vai[ii2, ji2] * (R2o - R21i) * (C2o - C21i);
                                A5 = 0;
                                A6 = 0;
                                A7 = 0;
                                A8 = 0;
                                A9 = 0;
                                for (int i = ii1 + 1; i < ii2; i++)
                                {
                                    for (int j = ji1 + 1; j < ji2; j++)
                                    {
                                        if (i == ii1 + 1)
                                        {
                                            A5 += Vai[ii1, j] * (R12i - R1o) * (1.0 / Columni);
                                            A6 += Vai[ii2, j] * (R2o - R21i) * (1.0 / Columni);
                                        }
                                        A9 += Vai[i, j] * (1.0 / Rowi / Columni);
                                    }
                                    A7 += Vai[i, ji1] * (1.0 / Rowi) * (C12i - C1o);
                                    A8 += Vai[i, ji2] * (1.0 / Rowi) * (C2o - C21i);
                                }
                                Vao[io, jo] = (A1 + A2 + A3 + A4 + A5 + A6 + A7 + A8 + A9) / (1.0 / N_tube / Nelement);
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < N_tube; i++)
            {
                for (int j = 0; j < Nelement; j++)
                {
                    avo += Vao[i, j];
                }
            }
            avo = avo / Vao.Length;
            res.Va = Vao;
            res.Va_ave = avo;
            return res;
        }
    }
}
