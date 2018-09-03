using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Basic;

namespace Model
{
    public class Tube
    {
        public static CalcResult TubeCalc(int Nelement, string fluid,
            double l, double Aa_fin, double Aa_tube, double A_r_cs, double Ar, Geometry geo, double[] tai,
            double[] RHi, double tri, double pri, double hri, double mr, double[] ma, double[] ha,double[] haw,
            double eta_surface, double zh, double zdp, int hexType, double thickness, double conductivity, double Pwater, AbstractState coolprop)
        {
            double g = mr / A_r_cs;

            CalcResult res_tube = new CalcResult();
            res_tube.Tao_Detail = new double[1, 1, Nelement];
            CalcResult[] r = new CalcResult[Nelement];
            for (int i = 0; i < Nelement; i++)
            {
                r[i] = Element.ElementCal(fluid, l / Nelement, Aa_fin, Aa_tube, A_r_cs, Ar, geo,
                    tai[i], RHi[i], tri, pri, hri, mr, g, ma[i], ha[i], haw[i],eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, coolprop);//elementtest
 
                //SP/TP Smooth
                if((r[i].x_i>1&&r[i].x_o<1)||(r[i].x_i>0&&r[i].x_o<0)||(r[i].x_i<0&&r[i].x_o>0)||(r[i].x_i<1&&r[i].x_o>1))
                {
                    int N_sub = 20;
                    r[i] = new CalcResult();
                    CalcResult[] r_sub = new CalcResult[N_sub];
                    double tri_sub=new double();
                    double pri_sub=new double();
                    double hri_sub=new double();
                    tri_sub = tri;
                    pri_sub = pri;
                    hri_sub = hri;
                    for(int j=0;j<N_sub;j++)
                    {
                        r_sub[j] = Element.ElementCal(fluid, l / Nelement / N_sub, Aa_fin / N_sub, Aa_tube / N_sub, A_r_cs, Ar / N_sub, geo,
                            tai[i], RHi[i], tri_sub, pri_sub, hri_sub, mr, g, ma[i] / N_sub, ha[i], haw[i], eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, coolprop);
                        pri_sub = r_sub[j].Pro;
                        hri_sub = r_sub[j].hro;
                        tri_sub = r_sub[j].Tro;

                        r[i].Tao += r_sub[j].Tao;
                        r[i].RHout += r_sub[j].RHout;
                        r[i].DP += r_sub[j].DP;
                        r[i].Q += r_sub[j].Q;
                        r[i].M += r_sub[j].M;
                        r[i].href += r_sub[j].href;
                        r[i].R_1 += r_sub[j].R_1;
                        r[i].R_1a += r_sub[j].R_1a;
                        r[i].R_1r += r_sub[j].R_1r;
                    }
                    r[i].Tao = r[i].Tao / N_sub;
                    r[i].RHout = r[i].RHout / N_sub;
                    r[i].href = r[i].href / N_sub;
                    r[i].R_1 = r[i].R_1 / N_sub;
                    r[i].R_1a = r[i].R_1a / N_sub;
                    r[i].R_1r = r[i].R_1r / N_sub;
                    r[i].Pro = r_sub[N_sub-1].Pro;
                    r[i].hro = r_sub[N_sub - 1].hro;
                    r[i].Tro = r_sub[N_sub - 1].Tro;
                    r[i].x_o = r_sub[N_sub - 1].x_o;
                    r[i].Vel_r = r_sub[N_sub - 1].Vel_r;
                    r[i].x_i = r_sub[0].x_i;
                    r[i].Tri = r_sub[0].Tri;
                }

                if (r[i].Pro < 0) { res_tube.Pro = -10000000; return res_tube; }
                pri = r[i].Pro;
                hri = r[i].hro;
                tri = r[i].Tro;
                res_tube.Tao_Detail[0, 0, i] = r[i].Tao;
                res_tube.Tao += r[i].Tao;
                res_tube.RHout+= r[i].RHout;
                res_tube.DP += r[i].DP;
                res_tube.Q += r[i].Q;
                res_tube.M += r[i].M;
                res_tube.href += r[i].href;
                res_tube.R_1 += r[i].R_1;
                res_tube.R_1a += r[i].R_1a;
                res_tube.R_1r += r[i].R_1r;
            }
            res_tube.Tao = res_tube.Tao / Nelement; 
            res_tube.RHout = res_tube.RHout / Nelement;
            res_tube.href = res_tube.href / Nelement;
            res_tube.R_1 = res_tube.R_1 / Nelement;
            res_tube.R_1a = res_tube.R_1a / Nelement;
            res_tube.R_1r = res_tube.R_1r / Nelement;
            res_tube.Tro = r[Nelement-1].Tro;
            res_tube.Pro = r[Nelement-1].Pro;
            res_tube.hro = r[Nelement-1].hro;
            res_tube.x_o = r[Nelement-1].x_o;
            res_tube.Vel_r = r[Nelement-1].Vel_r;
            res_tube.Tri = r[0].Tri;
            res_tube.x_i = r[0].x_i;
            return res_tube;

        }

    }
}
