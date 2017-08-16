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
            double dh, double l, double Aa_fin, double Aa_tube, double A_r_cs, double Ar, double[] tai,
            double tri, double pri, double hri, double mr, double ma, double ha,
            double eta_surface, double zh, double zdp, int hexType, double thickness, double conductivity, double Pwater)
        {
            double g = mr / A_r_cs;

            CalcResult res_tube = new CalcResult();
            CalcResult r = new CalcResult();
            for (int i = 0; i < Nelement; i++)
            {

                r = Element.ElementCal(fluid, composition, dh, l / Nelement, Aa_fin / Nelement, Aa_tube / Nelement, A_r_cs, Ar / Nelement,
                    tai[i], tri, pri, hri, mr, g, ma, ha, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater);

                pri = r.Pro;
                hri = r.hro;
                tri = r.Tro;

                res_tube.Tao = r.Tao;
                res_tube.DP += r.DP;
                res_tube.Q += r.Q;
                res_tube.M += r.M;
                res_tube.Tro = r.Tro;
                res_tube.Pro = r.Pro;
                res_tube.hro = r.hro;
                res_tube.x_o = r.x_o;
                res_tube.Vel_r = r.Vel_r;
                res_tube.href += r.href;
                res_tube.R_1 += r.R_1;
                res_tube.R_1a += r.R_1a;
                res_tube.R_1r += r.R_1r;
            }
            //res_tube.Tao = res_tube.Tao / Nelement; //to be modified....
            res_tube.href = res_tube.href / Nelement;
            res_tube.R_1 = res_tube.R_1 / Nelement;
            res_tube.R_1a = res_tube.R_1a / Nelement;
            res_tube.R_1r = res_tube.R_1r / Nelement;

            return res_tube;

        }

    }
}
