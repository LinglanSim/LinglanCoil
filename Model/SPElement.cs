using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Basic;

namespace Model
{
    public class SPElement
    {
        public static CalcResult ElementCalc(string[] fluid, double[] composition, double dh, double l, double Aa_fin, double Aa_tube, double A_r_cs, double Ar, double tai, 
            double tri, double pri, double hri, double mr, double g, double ma, double ha,
            double eta_surface, double zh, double zdp, int hexType, double thickness, double conductivity, double Pwater)
        {
            double r_metal = thickness / conductivity / Ar;
            double gg = 9.8;
            CalcResult res=new CalcResult();
            Refrigerant.TPFLSHResult r = new Refrigerant.TPFLSHResult();
            r = Refrigerant.TPFLSH(fluid, composition, tri + 273.15, fluid[0] == "Water" ? Pwater : pri);
            double wm = Refrigerant.WM(fluid, composition).Wm;
            double mu_r = Refrigerant.TRNPRP(fluid, composition, tri + 273.15, r.D).Viscosity / Math.Pow(10, 6);
            double k_r = Refrigerant.TRNPRP(fluid, composition, tri + 273.15, r.D).ThermalConductivity;
            double rho_r = r.D * wm;
            double cp_r = r.cp / wm * 1000;
            double Pr_r = cp_r * mu_r / k_r;
            res.Vel_r = g / rho_r;
            double Re_r = rho_r * res.Vel_r * dh / mu_r;
            double fh = RefrigerantSPHTC.ff_CHURCHILL(Re_r);
            double Nusselt = RefrigerantSPHTC.NU_GNIELINSKI(Re_r, Pr_r, fh);

            res.href = Nusselt * k_r / dh * zh;
            double cp_a = 1.0;
            cp_a = (hexType == 0 ? 1.027 : 1.02);

            double C_r = mr * cp_r / 1000;
            double C_a = ma * cp_a;
            res.R_1a = 1 / ((eta_surface * Aa_fin + Aa_tube) * ha);
            res.R_1r = 1 / (res.href * Ar);
            res.R_1 = res.R_1a + res.R_1r + r_metal;
            double UA = 1 / res.R_1;
            double C_min = Math.Min(C_a, C_r);
            double C_max = Math.Max(C_a, C_r);
            double C_ratio = C_min / C_max;
            double NTU = UA / (C_min * 1000);
            double epsilon = 1 - Math.Exp(Math.Pow(C_ratio,-1.0)*Math.Pow(NTU,0.22)
                *(Math.Exp(-C_ratio*Math.Pow(NTU,0.78))-1));
            res.Q = epsilon * C_min * Math.Abs(tri - tai);
            if (C_r < C_a)
            { // hexType=0 :evap, 1:cond
                res.Tro = tri + Math.Pow(-1, hexType) * epsilon * Math.Abs(tai - tri);
                res.Tao = tai + Math.Pow(-1, (hexType + 1)) * C_r * (Math.Abs(res.Tro - tri) / C_a);
            }
            else
            {
                res.Tao = tai + Math.Pow(-1, (hexType + 1)) * epsilon * Math.Abs(tai - tri);
                res.Tro = tri + Math.Pow(-1, hexType) * C_a * (Math.Abs(tai - res.Tao) / C_r);
            }
            double f_sp = RefrigerantSPDP.ff_Friction(Re_r);
            res.DP = zdp * f_sp * l / dh * Math.Pow(g, 2.0) / rho_r / 2000;
            res.Pro = fluid[0] == "Water" ? pri : pri - res.DP;
            res.hro = hri + Math.Pow(-1, hexType) * res.Q / mr;

            return res; 

        }


    }
}
