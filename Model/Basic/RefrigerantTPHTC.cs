using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//using HTC formula of Cavallini

namespace Model.Basic
{
    public class RefrigerantTPHTC
    {
       public static double Shah_Evap_href(string[] fluid, double[] composition, double d, double g, double p, double x, double q, double l)
        {
            var r = new Refrigerant.SATTTotalResult();
            double temperature;
            int phase1 = 1;
            int phase2 = 2;
            temperature = Refrigerant.SATP(fluid, composition, p, phase1).Temperature;
            r = Refrigerant.SATTTotal(fluid, composition, temperature).SATTTotalResult;
            double Pr_l = r.CpL * r.ViscosityL / r.KL * 1000;
 
            double h_fg = r.EnthalpyV - r.EnthalpyL; //"KJ"
            double qflux = q / (l * 3.14159 * d);
            double Bo = qflux / (g * h_fg);
            double Co = Math.Pow(1 / x - 1, 0.8) * Math.Pow(r.DensityV / r.DensityL, 0.5);
            double Fr_l = Math.Pow(g, 2.0) / (Math.Pow(r.DensityL, 2.0) * 9.8 * d);

            double h_LO = 0.023 * Math.Pow(g * (1 - x) * d / r.ViscosityL, 0.8) * Math.Pow(Pr_l, 0.4) * (r.KL / d);
            double h_LT = 0.023 * Math.Pow(g * (1 - 0) * d / r.ViscosityL, 0.8) * Math.Pow(Pr_l, 0.4) * (r.KL / d);

            int N = 0;
            double f = 0.0;
            if (Fr_l > 0.04) N = 0; else N = 1;
            if (Bo > 0.0011) f = 14.7; else f = 15.43;

            double h1 = 230 * Math.Pow(Bo, 0.5) * h_LO;
            double h2 = 1.8 * Math.Pow(Co * Math.Pow(0.38 * Math.Pow(Fr_l, -0.3), N), -0.8) * h_LO;
            double h3 = f * Math.Pow(Bo, 0.5) * Math.Exp(2.47 * Math.Pow(Co * Math.Pow(0.38 * Math.Pow(Fr_l, -0.3), N), -0.15)) * h_LO;
            double h4 = f * Math.Pow(Bo, 0.5) * Math.Exp(2.74 * Math.Pow(Co * Math.Pow(0.38 * Math.Pow(Fr_l, -0.3), N), -0.1)) * h_LO;
            double h5 = h_LT;
            double href = Math.Max(Math.Max(Math.Max(Math.Max(h1, h2), h3), h4), h5);
       
            return href;
        } 

    }
}
