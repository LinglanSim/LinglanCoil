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
        public static double deltap_JR(string[] fluid, double[] composition, double d, double g, double p, double x, double l)
        {
            int phase1 = 1;
            //int phase2 = 2;
            var r = new Refrigerant.SATTTotalResult();
            double tsat = Refrigerant.SATP(fluid, composition, p, phase1).Temperature;
            r = Refrigerant.SATTTotal(fluid, composition, tsat).SATTTotalResult;

            double Re_l = g * d / r.ViscosityL;
            double f_sp = RefrigerantSPDP.ff_Friction(Re_l);
            double DP_L = f_sp / d * Math.Pow(g, 2.0) / r.DensityL / 2000;

            double dp = 0;
            if (x <= 1.0)//0.93
            {
                double X_tt = Math.Pow(1 / x - 1, 0.9) * Math.Pow(r.DensityV / r.DensityL, 0.5) * Math.Pow(r.ViscosityL / r.ViscosityV, 0.1);
                double f_tp = 12.82 * Math.Pow(X_tt, -1.47) * Math.Pow(1 - x, 1.8);
                double f_fo = 0.046 * Math.Pow(Re_l, -0.2);
                double DP_l = 2 * f_fo * Math.Pow(g, 2.0) / (r.DensityL * d);
                double DP_tp = DP_l * f_tp;
                dp = Math.Max(DP_L, DP_tp) * l;
                dp = dp * 0.001;
            }
            else
            {
                double Re_v = g * d / r.ViscosityV;
                double f_sp1 = RefrigerantSPDP.ff_Friction(Re_v);
                double DP_1 = f_sp1 / d * Math.Pow(g, 2.0) / r.DensityV / 2000;

                double x_point93 = 0.93;
                double X_tt = Math.Pow(1 / x_point93 - 1, 0.9) * Math.Pow(r.DensityV / r.DensityL, 0.5) * Math.Pow(r.ViscosityL / r.ViscosityV, 0.1);
                double f_tp = 12.82 * Math.Pow(X_tt, -1.47) * Math.Pow(1 - x_point93, 1.8);
                double f_fo = 0.046 * Math.Pow(Re_l, -0.2);
                double DP_l = 2 * f_fo * Math.Pow(g, 2.0) / (r.DensityL * d);
                double DP_tp_point93 = DP_l * f_tp;
                double DP_tp = (DP_tp_point93 + (x - 0.8) / (1.0 - 0.93) * (DP_1 - DP_tp_point93)) * l;
                dp = DP_tp * 0.001;
            }

            return dp;

        }
        public static double deltap_MS(string[] fluid, double[] composition, double d, double g, double p, double x, double l)
        {
            //double Co=0.01;
            //double f_oil = 0;
            //if (x == 1) f_oil = 1.0; else f_oil = 1.15;
            //double m_to_ft = 1000 / (12 * 25.4);
            //double dp = 0.0;
            int phase1 = 1;
            //int phase2 = 2;
            var r = new Refrigerant.SATTTotalResult();
            double tsat = Refrigerant.SATP(fluid, composition, p, phase1).Temperature;
            r = Refrigerant.SATTTotal(fluid, composition, tsat).SATTTotalResult;

            double Re_l = g * (1 - x) * d / r.ViscosityL;
            double Re_v = g * x * d / r.ViscosityV;


            double f_l = 0;
            double f_v = 0;
            if (Re_l <= 1187)
                f_l = 64 / Re_l;
            else
                f_l = 0.3164 / Math.Pow(Re_l, 0.25);

            if (Re_v <= 1187)
                f_v = 64 / Re_v;
            else
                f_v = 0.3164 / Math.Pow(Re_v, 0.25);

            double DP_l = 4 * f_l * l * Math.Pow(g * (1 - x), 2.0) / (2 * r.DensityL * d);

            //摩擦压降
            double a = 2 * f_l * Math.Pow(g * (1 - x), 2.0) / (r.DensityL * d);
            double b = 2 * f_v * Math.Pow(g * x, 2.0) / (r.DensityV * d);
            double c = a + 2 * (b - a) * x;
            double dp_dz = c * Math.Pow(1 - x, 1.0 / 3) + b * Math.Pow(x, 3);
            double f_gd = 1 + dp_dz * ((r.DensityL / r.DensityV) / (Math.Pow(r.ViscosityL / r.ViscosityV, 0.25)) - 1);
            double DP_frict = f_gd * DP_l;

            //加速（动力）压降
            double DP_mom = 0;

            //两相总压降
            double dp = (DP_frict + DP_mom) * 0.001;

            return dp;



            //double DP_tp = 0;
            //if (x <= 0.8)
            //{
            //double f_tp = 0.007397899 / (Math.Pow(x, 0.8) * Math.Pow(d * m_to_ft, 0.5));    //"Sacks & Geary"
            //DP_tp = f_tp * f_oil * Math.Pow(x * g, 2.0) / (2 * r.DensityV * d);
            //dp = Math.Max(DP_l, DP_tp) * l;
            //dp = dp * 0.001;
            //}
            //else
            //{
            //double x_point8 = 0.8;
            //double f_tp_x_point8 = 0.007397899 / (Math.Pow(x_point8, 0.8) * Math.Pow(d * m_to_ft, 0.5));  //0.00566
            //double DP_tp_point8 = f_tp_x_point8 * f_oil * Math.Pow(x_point8 * g, 2.0) / (2 * r.DensityV * d);
            //double x_1 = 1.0;
            //double f_tp_x_1 = f_v * (1 + 14.73 * Math.Pow(Co, 0.591958)) / Math.Pow(m_to_ft, 0.5);
            //double DP_tp_1 = f_tp_x_1 * f_oil * Math.Pow(x_1 * g, 2.0) / (2 * r.DensityV * d);
            //DP_tp = (DP_tp_point8 + (x - 0.8) / (1.0 - 0.8) * (DP_tp_1 - DP_tp_point8)) * l;
            //dp = DP_tp * 0.001;
            //}

            //return dp;






        }

    }
}
