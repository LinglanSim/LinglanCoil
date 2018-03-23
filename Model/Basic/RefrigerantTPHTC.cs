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
            //double tsat1 = CoolProp.PropsSI("T", "P", p * 1000, "Q", 0, "R410A.mix");
            r = Refrigerant.SATTTotal(fluid, composition, temperature).SATTTotalResult;
            //double densityLo1 = CoolProp.PropsSI("D", "T", temperature, "Q", 0, "R410A.mix");
            //double densityVo1 = CoolProp.PropsSI("D", "T", temperature, "Q", 1, "R410A.mix");
            //double EnthalpyL1 = CoolProp.PropsSI("H", "T", temperature, "Q", 0, "R410A.mix") / 1000 - (fluid[0] == "Water" ? 0 : 140); 
            //double EnthalpyV1 = CoolProp.PropsSI("H", "T", temperature, "Q", 1, "R410A.mix") / 1000 - (fluid[0] == "Water" ? 0 : 140);
            double ViscosityL1 = CoolProp.PropsSI("V", "T", temperature, "Q", 0, "R410A.mix");//Coolprop和RefProp的计算结果差别很大
            //double CpL1 = CoolProp.PropsSI("C", "T", temperature, "Q", 0, "R410A.mix") / 1000;
            double KL1 = CoolProp.PropsSI("L", "T", temperature, "Q", 0, "R410A.mix") / 1000;//Coolprop和RefProp的计算结果差别很大

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
       public static double Kandlikar_Evap_href(string[] fluid, double[] composition, double d, double g, double p, double x, double q, double l)
       {
           var r = new Refrigerant.SATTTotalResult();
           double temperature;
           double a_g = 9.8;
           int phase1 = 1;
           //int phase2 = 2;
           temperature = Refrigerant.SATP(fluid, composition, p, phase1).Temperature;//danwei
           //double tsat1 = CoolProp.PropsSI("T", "P", p * 1000, "Q", 0, "R410A.mix");
           r = Refrigerant.SATTTotal(fluid, composition, temperature).SATTTotalResult;
           //double densityLo1 = CoolProp.PropsSI("D", "T", temperature, "Q", 0, "R410A.mix");
           //double densityVo1 = CoolProp.PropsSI("D", "T", temperature, "Q", 1, "R410A.mix");
           //double EnthalpyL1 = CoolProp.PropsSI("H", "T", temperature, "Q", 0, "R410A.mix") / 1000 - (fluid[0] == "Water" ? 0 : 140); 
           //double EnthalpyV1 = CoolProp.PropsSI("H", "T", temperature, "Q", 1, "R410A.mix") / 1000 - (fluid[0] == "Water" ? 0 : 140);
           //double ViscosityL1 = CoolProp.PropsSI("V", "T", temperature, "Q", 0, "R410A.mix");//Coolprop和RefProp的计算结果差别很大
           //double CpL1 = CoolProp.PropsSI("C", "T", temperature, "Q", 0, "R410A.mix") / 1000;
           //double KL1 = CoolProp.PropsSI("L", "T", temperature, "Q", 0, "R410A.mix") / 1000;//Coolprop和RefProp的计算结果差别很大

           double Pr_l = r.CpL * r.ViscosityL / r.KL * 1000;
           double Re_l = g * d * (1 - x) / r.ViscosityL;
           double h_fg = (r.EnthalpyV - r.EnthalpyL);
           double qflux = q / (l * 3.14159 * d);


           double Fr_l = 0;
           if (fluid[0] == "Water") Fr_l = 1;
           else if (fluid[0] == "R11") Fr_l = 1.3;
           else if (fluid[0] == "R12") Fr_l = 1.5;
           else if (fluid[0] == "R22") Fr_l = 2.2;
           else if (fluid[0] == "R113") Fr_l = 1.3;
           else if (fluid[0] == "R114") Fr_l = 1.24;
           else if (fluid[0] == "R152a") Fr_l = 1.1;
           else if (fluid[0] == "R13B1") Fr_l = 1.31;
           else if (fluid[0] == "Nitrogen") Fr_l = 4.7;//液氮
           else if (fluid[0] == "Neon") Fr_l = 3.5;//液氖
           else
               Fr_l = 1;

           double c1 = 1.136;
           double c2 = -0.9;
           double c3 = 1058;
           double c4 = 0.7;
           double c5 = 0.3;
           if (Fr_l > 0.04) c5 = 0;

           double Fr_lo = Math.Pow(g, 2) / (Math.Pow(r.DensityL, 2) * a_g * d);
           double Bo = qflux / (g * h_fg);
           double Co = Math.Pow(r.DensityV / r.DensityL, 0.5) * Math.Pow(1 / x - 1, 0.8);

           double h_L = 0.023 * Math.Pow(Re_l, 0.8) * Math.Pow(Pr_l, 0.4) * r.KL / d;
           double href = (c1 * Math.Pow(Co, c2) * Math.Pow(25 * Fr_lo, c5) + c3 * Math.Pow(Bo, c4) * Fr_l) * h_L;

           return href;
       }
       public static double JR_Evap_href(string[] fluid, double[] composition, double d, double g, double p, double x, double q, double l)
       {
           var r = new Refrigerant.SATTTotalResult();
           double temperature;
           double a_g = 9.8;
           double sigma = 1;
           int phase1 = 1;
           //int phase2 = 2;
           temperature = Refrigerant.SATP(fluid, composition, p, phase1).Temperature;//danwei
           //double tsat1 = CoolProp.PropsSI("T", "P", p * 1000, "Q", 0, "R410A.mix");
           r = Refrigerant.SATTTotal(fluid, composition, temperature).SATTTotalResult;
           //double densityLo1 = CoolProp.PropsSI("D", "T", temperature, "Q", 0, "R410A.mix");
           //double densityVo1 = CoolProp.PropsSI("D", "T", temperature, "Q", 1, "R410A.mix");
           //double EnthalpyL1 = CoolProp.PropsSI("H", "T", temperature, "Q", 0, "R410A.mix") / 1000 - (fluid[0] == "Water" ? 0 : 140); 
           //double EnthalpyV1 = CoolProp.PropsSI("H", "T", temperature, "Q", 1, "R410A.mix") / 1000 - (fluid[0] == "Water" ? 0 : 140);
           //double ViscosityL1 = CoolProp.PropsSI("V", "T", temperature, "Q", 0, "R410A.mix");//Coolprop和RefProp的计算结果差别很大
           //double ViscosityL1 = CoolProp.PropsSI("V", "T", temperature, "Q", 1, "R410A.mix");
           //double CpL1 = CoolProp.PropsSI("C", "T", temperature, "Q", 0, "R410A.mix") / 1000;
           //double KL1 = CoolProp.PropsSI("L", "T", temperature, "Q", 0, "R410A.mix") / 1000;//Coolprop和RefProp的计算结果差别很大

           double Pr_l = r.CpL * r.ViscosityL / r.KL * 1000;
           double Re_l = g * d * (1 - x) / r.ViscosityL;
           double h_fg = (r.EnthalpyV - r.EnthalpyL);
           double qflux = q / (l * 3.14159 * d);


           //单相流体的经验关联式

           double Bo = qflux / (g * h_fg);
           double X_tt = Math.Pow(r.DensityV / r.DensityL, 0.5) * Math.Pow(1 / x - 1, 0.9) * Math.Pow(r.ViscosityL / r.ViscosityV, 0.1);

           double N = 1;
           if (X_tt < 1)
               N = 4048 * Math.Pow(X_tt, 1.22) * Math.Pow(Bo, 1.13);

           double F_p = 2.37 * Math.Pow(0.29 + 1 / X_tt, 0.85);

           //表面张力sigma的Refprop调用方法
           double b_d = 0.0146 * (35 / 180 * 3.14159) * Math.Pow(2 * sigma / a_g / (r.DensityL - r.DensityV), 0.5);
           double h_L = 0.023 * Math.Pow(Re_l, 0.8) * Math.Pow(Pr_l, 0.4) * r.KL / d;
           double h_SA = 207 * r.KL / (b_d) * Math.Pow((qflux / r.KL) * (b_d / temperature), 0.745) * Math.Pow(r.DensityV / r.DensityL, 0.581) * Math.Pow(Pr_l, 0.533);

           double h_nbc = N * h_SA;
           double h_cec = F_p * h_L;
           double href = h_nbc + h_cec;

           return href;

           //两相传热关联式
           //重点理解混合物换热关联式对纯质换热关联式的修正
           //混合工质两组分各自泡核沸腾的计算式不清楚

           //double X = 500;
           //double Y = 30;
           //double X_1 = 500;
           //double X_2 = 500;

           //double b2 = (1 - X) * log((1.01 - X) / (1.01 - Y)) + X * log(X / Y);
           //double b3 = 0;
           //if (X < 0.01)
           //b3 = Math.Pow(Y/X, 0.1) - 1;
           //double b4 = 152 * Math.Pow(p / p_cmvc, 3.9);
           //double b5 = 0.92 * Math.Pow(abs(Y - X), 0.001) * Math.Pow(p / p_cmvc, 0.66);
           //double C_un = (1 + (b2 + b3) * (1 + b4)) * (1 + b5);

           //double N = 1;
           //if (X_tt < 1)
           //N = 4048 * Math.Pow(X_tt, 1.22) * Math.Pow(Bo, 1.13);
           //double F_p = 2.37 * Math.Pow(0.29 + 1 / X_tt, 0.85);
           //double C_me = 1 - 0.35 * Math.Pow(abs(Y - X), 1.56);

           //double h_lo = 0.023 * Math.Pow(Re_l, 0.8) * Math.Pow(Pr_l, 0.4) * r.KL / d;

           //double h_i = 1 / (X_1 / h_1 + X_2 / h_2);
           //double h_un = h_i / C_un;

           //double h_tp = N / C_un * h_un + C_me * F_p * h_lo;

           //return h_tp;

       }
       public static double Shah_Cond_href(string[] fluid, double[] composition, double d, double g, double p, double x, double q, double l)
       {
           var r = new Refrigerant.SATTTotalResult();
           double temperature;
           double pc;
           double a_g = 9.8;
           int phase1 = 1;
           //int phase2 = 2;
           temperature = Refrigerant.SATP(fluid, composition, p, phase1).Temperature;
           //double tsat1 = CoolProp.PropsSI("T", "P", p * 1000, "Q", 0, "R410A.mix");
           pc = Refrigerant.CRIT(fluid, composition).p;
           double pc1 = CoolProp.Props1SI("R410A", "Pcrit") / 1000;
           double pr = p / pc;

           r = Refrigerant.SATTTotal(fluid, composition, temperature).SATTTotalResult;
           //double densityLo1 = CoolProp.PropsSI("D", "T", temperature, "Q", 0, "R410A.mix");
           //double densityVo1 = CoolProp.PropsSI("D", "T", temperature, "Q", 1, "R410A.mix");
           //double ViscosityL1 = CoolProp.PropsSI("V", "T", temperature, "Q", 0, "R410A.mix");//Coolprop和RefProp的计算结果差别很大
           //double ViscosityV1 = CoolProp.PropsSI("V", "T", temperature, "Q", 1, "R410A.mix");
           //double CpL1 = CoolProp.PropsSI("C", "T", temperature, "Q", 0, "R410A.mix") / 1000;
           //double KL1 = CoolProp.PropsSI("L", "T", temperature, "Q", 0, "R410A.mix") / 1000;//Coolprop和RefProp的计算结果差别很大

           double Pr_l = r.CpL * r.ViscosityL / r.KL * 1000.0;
           double Re_l = g * (1.0 - x) * d / r.ViscosityL;

           //double h_fg = r.EnthalpyV - r.EnthalpyL; //"KJ"
           //double qflux = q / (l * 3.14159 * d);
           //double Bo = qflux / (g * h_fg);
           //double Co = Math.Pow(1 / x - 1, 0.8) * Math.Pow(r.DensityV / r.DensityL, 0.5);
           //double Fr_l = Math.Pow(g, 2.0) / (Math.Pow(r.DensityL, 2.0) * 9.8 * d);

           //double h_LO = 0.023 * Math.Pow(g * (1 - x) * d / r.ViscosityL, 0.8) * Math.Pow(Pr_l, 0.4) * (r.KL / d);
           //double h_LT = 0.023 * Math.Pow(g * (1 - 0) * d / r.ViscosityL, 0.8) * Math.Pow(Pr_l, 0.4) * (r.KL / d);

           //int N = 0;
           //double f = 0.0;
           //if (Fr_l > 0.04) N = 0; else N = 1;
           //if (Bo > 0.0011) f = 14.7; else f = 15.43;

           //double h1 = 230 * Math.Pow(Bo, 0.5) * h_LO;
           //double h2 = 1.8 * Math.Pow(Co * Math.Pow(0.38 * Math.Pow(Fr_l, -0.3), N), -0.8) * h_LO;
           //double h3 = f * Math.Pow(Bo, 0.5) * Math.Exp(2.47 * Math.Pow(Co * Math.Pow(0.38 * Math.Pow(Fr_l, -0.3), N), -0.15)) * h_LO;
           //double h4 = f * Math.Pow(Bo, 0.5) * Math.Exp(2.74 * Math.Pow(Co * Math.Pow(0.38 * Math.Pow(Fr_l, -0.3), N), -0.1)) * h_LO;
           //double h5 = h_LT;
           //double href = Math.Max(Math.Max(Math.Max(Math.Max(h1, h2), h3), h4), h5);

           //return href;

           double Z = Math.Pow(1.0 / x - 1.0, 0.8) * Math.Pow(pr, 0.4);
           double J_g = x * g / Math.Pow(a_g * d * r.DensityV * (r.DensityL - r.DensityV), 0.5);

           double h_LS = 0.023 * Math.Pow(Re_l, 0.8) * Math.Pow(Pr_l, 0.4) * r.KL / d;
           double h_I = h_LS * (1 + 3.8 / Math.Pow(Z, 0.95)) * Math.Pow(r.ViscosityL / (14.0 * r.ViscosityV), (0.0058 + 0.557 * pr));
           double h_Nu = 1.32 * Math.Pow(Re_l, -1.0 / 3.0) * Math.Pow(r.DensityL * (r.DensityL - r.DensityV) * a_g * Math.Pow(r.KL, 3.0) / Math.Pow(r.ViscosityL, 2.0), 1.0 / 3.0);

           double a = 0.95 / (1.254 + 2.27 * Math.Pow(Z, 1.249));
           double b = 0.98 * Math.Pow(Z + 0.263, -0.62);

           double h_tp = 0;
           if (J_g <= a)
               h_tp = h_Nu;
           else
           {
               if (J_g >= b)
                   h_tp = h_I;
               else
                   h_tp = h_I + h_Nu;
           }

           return h_tp;
       }
       public static double Dobson_Cond_href(string[] fluid, double[] composition, double d, double g, double p, double x, double Ts, double l)
       {
           var r = new Refrigerant.SATTTotalResult();
           double temperature;
           //double pc;
           double a_g = 9.80665;
           int phase1 = 1;
           //int phase2 = 2;
           temperature = Refrigerant.SATP(fluid, composition, p, phase1).Temperature;
           //double tsat1 = CoolProp.PropsSI("T", "P", p * 1000, "Q", 0, "R410A.mix");

           r = Refrigerant.SATTTotal(fluid, composition, temperature).SATTTotalResult;
           //double densityLo1 = CoolProp.PropsSI("D", "T", temperature, "Q", 0, "R410A.mix");
           //double densityVo1 = CoolProp.PropsSI("D", "T", temperature, "Q", 1, "R410A.mix");
           //double EnthalpyL1 = CoolProp.PropsSI("H", "T", temperature, "Q", 0, "R410A.mix") / 1000 - (fluid[0] == "Water" ? 0 : 140); 
           //double EnthalpyV1 = CoolProp.PropsSI("H", "T", temperature, "Q", 1, "R410A.mix") / 1000 - (fluid[0] == "Water" ? 0 : 140);
           //double ViscosityL1 = CoolProp.PropsSI("V", "T", temperature, "Q", 0, "R410A.mix");//Coolprop和RefProp的计算结果差别很大
           //double ViscosityL1 = CoolProp.PropsSI("V", "T", temperature, "Q", 1, "R410A.mix");
           //double CpL1 = CoolProp.PropsSI("C", "T", temperature, "Q", 0, "R410A.mix") / 1000;
           //double KL1 = CoolProp.PropsSI("L", "T", temperature, "Q", 0, "R410A.mix") / 1000;//Coolprop和RefProp的计算结果差别很大

           double Pr_l = r.CpL * r.ViscosityL / r.KL * 1000;
           double Re_l = g * (1.0 - x) * d / r.ViscosityL;


           double X_tt = Math.Pow(r.DensityV / r.DensityL, 0.5) * Math.Pow(r.ViscosityL / r.ViscosityV, 0.1) * Math.Pow(1 / x - 1.0, 0.9);

           double Re_vo = g * d / r.ViscosityV;
           double G_a = a_g * r.DensityL * (r.DensityL - r.DensityV) * Math.Pow(d, 3.0) / Math.Pow(r.ViscosityL, 2.0);
           //double Tsat = 1;
           //double Ts = 0;
           double Ja_l = r.CpL * (temperature - Ts) / (r.EnthalpyV - r.EnthalpyL);//Ts : surface temperature of tube wall

           double Fr_l = Math.Pow(g, 2.0) / (Math.Pow(r.DensityL, 2.0) * g * d);

           double c1 = 0;
           double c2 = 0;
           if (Fr_l > 0 && Fr_l <= 0.7)
           {
               c1 = 4.172 + 5.48 * Fr_l - 1.564 * Math.Pow(Fr_l, 2.0);
               c2 = 1.773 - 0.169 * Fr_l;
           }
           else
           {
               c1 = 7.242;
               c2 = 1.655;
           }

           double alpha = 1.0 / (1.0 + (1.0 - x) / x * Math.Pow(r.DensityV / r.DensityL, 2.0 / 3.0));
           double thita_l = 3.14159 - Math.Acos(2.0 * alpha - 1);


           double a = Math.Pow(1.376 + c1 / Math.Pow(X_tt, c2), 0.5);
           double Nu_forced = 0.0195 * Math.Pow(Re_l, 0.8) * Math.Pow(Pr_l, 0.4) * a;
           double Nu1 = 0.023 * Math.Pow(Re_l, 0.8) * Math.Pow(Pr_l, 0.4) * (1 + 2.22 / Math.Pow(X_tt, 0.89));
           double Nu2 = 0.23 * Math.Pow(Re_vo, 0.12) / (1.0 + 1.11 * Math.Pow(X_tt, 0.58)) * Math.Pow(G_a * Pr_l / Ja_l, 0.25) + (1.0 - thita_l / 3.14159) * Nu_forced;

           double h_tp;
           double Fr_so = 0;
           if (Re_l <= 1250)
           {
               Fr_so = 0.025 * Math.Pow(Re_l, 1.59) * Math.Pow((1.0 + 1.09 * Math.Pow(X_tt, 0.039)) / X_tt, 1.5) * Math.Pow(G_a, -0.5);
           }
           else
           {
               Fr_so = 1.26 * Math.Pow(Re_l, 1.04) * Math.Pow((1.0 + 1.09 * Math.Pow(X_tt, 0.039)) / X_tt, 1.5) * Math.Pow(G_a, -0.5);
           }

           if (g >= 500)
               h_tp = r.KL * Nu1 / d;
           else
           {
               if (Fr_so <= 20)
                   h_tp = r.KL * Nu2 / d;
               else
                   h_tp = r.KL * Nu1 / d;
           }


           return h_tp;
       } 


    }
}
