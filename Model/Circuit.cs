using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Basic;

namespace Model
{
    public class Circuit
    {
        public static CalcResult CircuitCalc(int index, CirArr[] cirArr, CircuitNumber CircuitInfo, int Nrow, int[] Ntube, int Nelement, string fluid,
            double dh, double l, GeometryResult[,] geo, double[, ,] ta, double[, ,] RH,
            double tri, double pri, double hri, double mr, double[,] ma, double[,] ha,
            double eta_surface, double zh, double zdp, int hexType, double thickness, double conductivity, double Pwater,string Airdirection)
        {

            int N_tube = Ntube[0];
            int Ncir = CircuitInfo.number[0];
            int[] TubeofCir=CircuitInfo.TubeofCir;
            CalcResult res_cir = new CalcResult();
            CalcResult[] r = new CalcResult[TubeofCir[index]];

            int iRow = 0;
            int iTube = 0;

            double[] tai = new double[Nelement];
            double[] RHi = new double[Nelement];
            double[, ,] taout_calc = new double[Nelement, N_tube, Nrow];
            double[, ,] RHout_calc = new double[Nelement, N_tube, Nrow];
            double[,] Q_detail = new double[N_tube,Nrow];//detail output
            double[,] DP_detail = new double[N_tube, Nrow];
            double[,] Tro_detail = new double[N_tube, Nrow];
            double[,] href_detail = new double[N_tube, Nrow];
            double Ar = 0;
            double Aa = 0;
            double Aa_tube = 0;
            double Aa_fin = 0;
            double Ar_cs = 0;
            double pri_tube = 0;
            double tri_tube = 0;
            double hri_tube = 0;
            double[] ma_tube = new double[Nelement];
            double[] ha_tube = new double[Nelement];
            int index2 = 0;

            CheckAir airConverge = new CheckAir();
            int iter = 0;
            //ma = ma / (N_tube*Nelement); //air flow distribution to be considered

            if (index == 0) index2 = 0;
            else
                for (int i = 1; i <= index; i++)
                    index2 += TubeofCir[i - 1];
            //do
            //{
                pri_tube = pri;
                tri_tube = tri;
                hri_tube = hri;
                res_cir.DP = 0;
                // res_cir.Tao += r.Tao;
                res_cir.Q = 0;
                res_cir.M = 0;
                res_cir.Tro = 0;
                res_cir.Pro = 0;
                res_cir.hro = 0;
                res_cir.x_o = 0;
                res_cir.Vel_r = 0;
                res_cir.href = 0;
                res_cir.R_1 = 0;
                res_cir.R_1a = 0;
                res_cir.R_1r = 0;
                res_cir.mr = 0;
                for (int i = 0; i < TubeofCir[index]; i++) //to be updated //Nrow * N_tube
                {
                    //index2 = index == 1 ? 0 : (index - 1) * TubeofCir[index - 1 - 1];
                    iRow = cirArr[i + index2].iRow;
                    iTube = cirArr[i + index2].iTube;

                    Ar = geo[iTube, iRow].A_r;
                    Aa = geo[iTube, iRow].A_a;
                    Aa_tube = geo[iTube, iRow].Aa_tube;
                    Aa_fin = geo[iTube, iRow].Aa_fin;
                    Ar_cs = geo[iTube, iRow].A_r_cs;
                    //tai=ta[,iTube,iRow];
                    for (int j = 0; j < Nelement; j++)
                    {
                        tai[j] = ta[j, iTube, iRow];
                        RHi[j] = RH[j, iTube, iRow];
                        ma_tube[j] = ma[iTube, j];
                        ha_tube[j] = ha[iTube, j];
                    }

                    r[i] = Tube.TubeCalc(Nelement, fluid, dh, l, Aa_fin, Aa_tube, Ar_cs, Ar, tai, RHi, tri_tube, pri_tube, hri_tube,
                        mr, ma_tube, ha_tube, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater);
                    if (r[i].Pro < 0) { res_cir.Pro = -10000000; return res_cir; }
                    if (Airdirection=="Parallel")
                    {
                        for (int j = 0; j < Nelement; j++)
                        {
                            ta[j, iTube, iRow+1] = r[i].Tao;
                            RH[j, iTube, iRow+1] = r[i].RHout;
                        }
                    }
                    else//Counter
                    {
                        for(int j=0;j<Nelement;j++)
                        {
                            taout_calc[j, iTube, iRow] =r[i].Tao_Detail[0, 0, j];
                            RHout_calc[j, iTube, iRow] = r[i].RHout;
                        }
                    }

                    tri_tube = r[i].Tro;
                    pri_tube = r[i].Pro;
                    hri_tube = r[i].hro;
                    res_cir.DP += r[i].DP;
                    // res_cir.Tao += r.Tao;
                    res_cir.Q += r[i].Q;
                    Q_detail[iTube, iRow] = r[i].Q;//detail output
                    DP_detail[iTube, iRow] = r[i].DP;
                    Tro_detail[iTube, iRow] = r[i].Tro;
                    href_detail[iTube, iRow] = r[i].href;
                    res_cir.M += r[i].M;
                    res_cir.Tro = r[i].Tro;
                    res_cir.Pro = r[i].Pro;
                    res_cir.hro = r[i].hro;
                    res_cir.x_o = r[i].x_o;
                    res_cir.Vel_r = r[i].Vel_r;
                    res_cir.href += r[i].href;
                    res_cir.R_1 += r[i].R_1;
                    res_cir.R_1a += r[i].R_1a;
                    res_cir.R_1r += r[i].R_1r;
                    res_cir.Tri = r[i].Tri;
                    res_cir.x_i = r[i].x_i;
                }
            /*
                if (Airdirection == "Parallel")
                    airConverge.flag = true;
                else//Counter
                {
                    airConverge = CheckAirConvergeforCircuits.CheckAirConverge(Nrow, N_tube, Nelement, ta, RH, taout_calc, RHout_calc);
                    ta = airConverge.ta;
                    RH = airConverge.RH;
                    iter++;
                }
            } while (!airConverge.flag && iter < 100);
            */

            //if (iter >= 100)
            //{
            //    throw new Exception("iter for AirConverge > 100.");
            //}
            res_cir.mr = mr;
            res_cir.Tao_Detail = ta;//taout_calc;
            res_cir.RHo_Detail = RH;
            //res_cir.Tao = res_cir.Tao / Nelement;
            res_cir.href = res_cir.href / TubeofCir[index];
            res_cir.R_1 = res_cir.R_1 / TubeofCir[index];
            res_cir.R_1a = res_cir.R_1a / TubeofCir[index];
            res_cir.R_1r = res_cir.R_1r / TubeofCir[index];
            res_cir.Q_detail = Q_detail;//detail output
            res_cir.DP_detail = DP_detail;
            res_cir.Tro_detail = Tro_detail;
            res_cir.href_detail = href_detail;
            return res_cir;
        }

    }
}
