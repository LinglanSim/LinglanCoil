using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Basic
{
    public class RefrigerantTPDP
    {
        //For smooth tube for now;
        public static double deltap_smooth(string[] fluid, double[] composition, double d, double g, double p, double x, double l)
        {
            double Co=0.01;
            double f_oil = 0;
            if (x == 1) f_oil = 1.0; else f_oil = 1.15;
            double m_to_ft = 1000 / (12 * 25.4);
            double dp = 0.0;
            int phase1 = 1;
            int phase2 = 2;
            var r = new Refrigerant.SATTTotalResult();
            double tsat = Refrigerant.SATP(fluid, composition, p, phase1).Temperature;
            r = Refrigerant.SATTTotal(fluid, composition, tsat).SATTTotalResult;

            double Re_l = g * d / r.ViscosityL;
            double Re_v = g * d / r.ViscosityV;
            double f_l = RefrigerantSPDP.ff_Friction(Re_l);
            double f_v = RefrigerantSPDP.ff_Friction(Re_v);
            double DP_l = f_l * Math.Pow(g, 2.0) / (2 * r.DensityL * d);

            double DP_tp = 0;
            if (x <= 0.8)
            {
                double f_tp = 0.007397899 / (Math.Pow(x, 0.8) * Math.Pow(d * m_to_ft, 0.5));    //"Sacks & Geary"
                DP_tp = f_tp * f_oil * Math.Pow(x * g, 2.0) / (2 * r.DensityV * d);
                dp = Math.Max(DP_l, DP_tp) * l;
                dp = dp * 0.001;
            }
            else
            {
                double x_point8 = 0.8;
                double f_tp_x_point8 = 0.007397899 / (Math.Pow(x_point8, 0.8) * Math.Pow(d * m_to_ft, 0.5));  //0.00566
                double DP_tp_point8 = f_tp_x_point8 * f_oil * Math.Pow(x_point8 * g, 2.0) / (2 * r.DensityV * d);
                double x_1 = 1.0;
                double f_tp_x_1 = f_v * (1 + 14.73 * Math.Pow(Co, 0.591958)) / Math.Pow(m_to_ft, 0.5);
                double DP_tp_1 = f_tp_x_1 * f_oil * Math.Pow(x_1 * g, 2.0) / (2 * r.DensityV * d);
                DP_tp = (DP_tp_point8 + (x - 0.8) / (1.0 - 0.8) * (DP_tp_1 - DP_tp_point8)) * l;
                dp = DP_tp * 0.001;
            }

            return dp;
        }

    }
}
