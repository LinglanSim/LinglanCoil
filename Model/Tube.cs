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
        public static CalcResult TubeCalc(int Nelement, string[] fluid, double[] composition,
            double dh, double l, double Aa_fin, double Aa_tube, double A_r_cs, double Ar, double tai,
            double tri, double pri, double hri, double mr, double[] ma, double[] ha,
            double eta_surface, double zh, double zdp, int hexType, double thickness, double conductivity, double Pwater)
        {
            double g = mr / A_r_cs;

            CalcResult res_tube = new CalcResult();
            CalcResult[] r = new CalcResult[Nelement];
            for (int i = 0; i < Nelement; i++)
            {

                r[i] = Element.ElementCal(fluid, composition, dh, l / Nelement, Aa_fin / Nelement, Aa_tube / Nelement, A_r_cs, Ar / Nelement,
                    tai, tri, pri, hri, mr, g, ma[i], ha[i], eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater);

                tri = r[i].Tro;
                pri = r[i].Pro;
                hri = r[i].hro;
                res_tube.Tao += r[i].Tao;
                res_tube.DP += r[i].DP;
                res_tube.Q += r[i].Q;
                res_tube.M += r[i].M;
                res_tube.href += r[i].href;
                res_tube.R_1 += r[i].R_1;
                res_tube.R_1a += r[i].R_1a;
                res_tube.R_1r += r[i].R_1r;
            }
            res_tube.Tro = r[Nelement-1].Tro;
            res_tube.Pro = r[Nelement - 1].Pro;
            res_tube.hro = r[Nelement - 1].hro;
            res_tube.x_o = r[Nelement - 1].x_o;
            res_tube.Vel_r = r[Nelement - 1].Vel_r;

            res_tube.Tao = res_tube.Tao / Nelement;
            res_tube.href = res_tube.href / Nelement;
            res_tube.R_1 = res_tube.R_1 / Nelement;
            res_tube.R_1a = res_tube.R_1a / Nelement;
            res_tube.R_1r = res_tube.R_1r / Nelement;
            res_tube.Tri = r[0].Tri;
            res_tube.x_i = r[0].x_i;
            return res_tube;

        }

    }
}
