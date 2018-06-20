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
        public static CalcResult ElementCalc(string fluid, double dh, double l, double Aa_fin, double Aa_tube, double A_r_cs, double Ar, double tai,
            double RHi, double tri, double pri, double hri, double mr, double g, double ma, double ha,double haw,
            double eta_surface, double zh, double zdp, int hexType, double thickness, double conductivity, double Pwater)
        {
            double r_metal = thickness / conductivity / Ar;
            double gg = 9.8;
            CalcResult res=new CalcResult();
            double mu_r = CoolProp.PropsSI("V", "H", hri * 1000, "P", (fluid == "Water" ? Pwater : pri) * 1000, fluid);
            double k_r = CoolProp.PropsSI("L", "H", hri * 1000, "P", (fluid == "Water" ? Pwater : pri) * 1000, fluid);
            double rho_r = CoolProp.PropsSI("D", "H", hri * 1000, "P", (fluid == "Water" ? Pwater : pri) * 1000, fluid);
            double cp_r = CoolProp.PropsSI("C", "H", hri * 1000, "P", (fluid == "Water" ? Pwater : pri) * 1000, fluid);
            double Pr_r = cp_r * mu_r / k_r;
            //for debugging, to check if the 1ph property is in 2ph region but not in 1ph, ruhao20180209
            /*
            var rr = new Refrigerant.SATTTotalResult();
            rr = Refrigerant.SATTTotal(fluid, composition, Refrigerant.SATP(fluid, composition, pri, 1).Temperature).SATTTotalResult; //satruration temperature
            if ( rho_r > rr.DensityV && rho_r < rr.DensityL)
            {
                throw new Exception("property is not in 1ph region");
            }
            */
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
            //流体不混合的一次交叉流
            //double epsilon_jc = 1 - Math.Exp(Math.Pow(C_ratio,-1.0)*Math.Pow(NTU,0.22)
                //*(Math.Exp(-C_ratio*Math.Pow(NTU,0.78))-1));
            //顺流计算公式
            //double epsilon_downflow = (1 - Math.Exp(-NTU * (1 + C_ratio))) / (1 + C_ratio);
            //逆流计算公式
            double epsilon_counterflow = (1 - Math.Exp(-NTU * (1 - C_ratio))) / (1 - C_ratio * Math.Exp(-NTU * (1 - C_ratio)));

            double epsilon = epsilon_counterflow;
            res.Q = epsilon * C_min * (tai - tri) * Math.Pow(-1, hexType);
            if (C_r < C_a)
            { // hexType=0 :evap, 1:cond
                res.Tro = tri + epsilon * (tai - tri);//Math.Abs(tai - tri);
                res.Tao = tai - C_r * ((res.Tro - tri) / C_a);//Math.Abs(res.Tro - tri)
            }
            else
            {
                res.Tao = tai - epsilon * (tai - tri);//Math.Abs(tai - tri)
                res.Tro = tri + C_a * ((tai - res.Tao) / C_r);//(Math.Abs(tai - res.Tao) / C_r)
            }
            double f_sp = RefrigerantSPDP.ff_Friction(Re_r);
            res.DP = zdp * f_sp * l / dh * Math.Pow(g, 2.0) / rho_r / 2000;
            res.Pro = fluid == "Water" ? pri : pri - res.DP;
            if (res.Pro < 0) { res.Pro = -10000000; return res; }
            res.hro = hri + Math.Pow(-1, hexType) * res.Q / mr;
            //re-calc tro for refrigerant to avoid Tro < Tsat
            if (fluid != "Water") res.Tro = CoolProp.PropsSI("T", "P", res.Pro * 1000, "H", res.hro * 1000, fluid) - 273.15;

            res.RHout = 1.1 * RHi;
            return res; 

        }
        public static CalcResult ElementCalc2(string fluid, double dh, double l, double Aa_fin, double Aa_tube, double A_r_cs, double Ar, double tai,
         double RHi, double tri, double pri, double hri, double mr, double g, double ma, double ha, double haw,
         double eta_surface, double zh, double zdp, int hexType, double thickness, double conductivity, double Pwater)// Python Original
        {

            if (tai < tri)
                hexType = 1;
            else
                hexType = 0;//zzc
            double Q = 0;
            double Tout_a = 0;
            double Q_sensible = 0;
            double r_metal = thickness / conductivity / Ar;
            CalcResult res = new CalcResult();
            double mu_r = CoolProp.PropsSI("V", "H", hri * 1000, "P", (fluid == "Water" ? Pwater : pri) * 1000, fluid);
            double k_r = CoolProp.PropsSI("L", "H", hri * 1000, "P", (fluid == "Water" ? Pwater : pri) * 1000, fluid);
            double rho_r = CoolProp.PropsSI("D", "H", hri * 1000, "P", (fluid == "Water" ? Pwater : pri) * 1000, fluid);
            double cp_r = CoolProp.PropsSI("C", "H", hri * 1000, "P", (fluid == "Water" ? Pwater : pri) * 1000, fluid);
            double Pr_r = cp_r * mu_r / k_r;
            res.Vel_r = g / rho_r;
            double Re_r = rho_r * res.Vel_r * dh / mu_r;
            double fh = RefrigerantSPHTC.ff_CHURCHILL(Re_r);
            double Nusselt = RefrigerantSPHTC.NU_GNIELINSKI(Re_r, Pr_r, fh);

            res.href = Nusselt * k_r / dh * zh;
            double cp_da = 1.0;
            double Tin_a = tai;
            double Tin_r = tri;
            cp_da = CoolProp.HAPropsSI("C", "T", Tin_a + 273.15, "P", 101325, "R", RHi);
            double omega_in = CoolProp.HAPropsSI("W", "T", Tin_a + 273.15, "P", 101325, "R", RHi);
            double Tdp = CoolProp.HAPropsSI("D", "T", Tin_a + 273.15, "P", 101325, "R", RHi) - 273.15;
            double hin_a = CoolProp.HAPropsSI("H", "T", Tin_a + 273.15, "P", 101325, "R", RHi);
            double h_r = res.href;
            double k_fin = 237;
            double Fthickness = 0.095 * 0.001;
            double Pt = 21 * 0.001;
            double Pr = 22 * 0.001;
            double Do = 7.35 * 0.001;
            double h_fin = (Pt - Do) / 2;
            double dc = Do + 2 * Fthickness;
            double df = Math.Pow(4 / Math.PI * Pt * Pr, 0.5);
            double eta_a = 1 / (1 + ha * Math.Pow(df - dc, 2) * Math.Pow(df, 0.5) / 6 / Fthickness / k_fin / Math.Pow(dc, 0.5));
            //double m = Math.Sqrt(2 * ha / (k_fin * Fthickness));
            //double eta_a= Math.Tanh(m * h_fin) / (m * h_fin);// may cause error
            //double eta_dry = eta_surface;
            double UA_i = h_r * Ar;
            double UA_o = (eta_a * Aa_fin + Aa_tube) * ha;
            double Ntu_i = UA_i / (mr * cp_r);
            double Ntu_o = UA_o / (ma * cp_da);
            double UA = 1 / (1 / (UA_i) + 1 / (UA_o));
            double Cmin = Math.Min(cp_r * mr, cp_da * ma);
            double Cmax = Math.Max(cp_r * mr, cp_da * ma);
            double C_star = Cmin / Cmax;
            double Ntu_dry = UA / Cmin;
            double epsilon_dry = ((1 - Math.Exp(-Ntu_dry * (1 - C_star))) / (1 - C_star * Math.Exp(-Ntu_dry * (1 - C_star))));
            double Q_dry = epsilon_dry * Cmin * (Tin_a - Tin_r);
            double Tout_a_dry = Tin_a - Q_dry / (ma * cp_da);
            double Tout_r = Tin_r + Q_dry / (mr * cp_r);
            double hout_a = hin_a - Q_dry / ma;
            double Tout_s = (UA_o * Tout_a_dry + UA_i * Tin_r) / (UA_o + UA_i);
            double Tin_s = (UA_o * Tin_a + UA_i * Tout_r) / (UA_o + UA_i);
            double Tout_r_dry = Tout_r;
            double f_dry = 1.0;
            double omega_out = omega_in;
            bool isFullyWet = true;
            if (Tin_s < Tdp)
                isFullyWet = true;
            else
                isFullyWet = false;
            if (Tout_s < Tdp | isFullyWet == true)
            {
                //ha = 30.44;//verify
                double x1 = Tin_r + 0.001;
                double x2 = Tin_a - 0.001;
                double eps = 1e-8;
                int iter = 1;
                double change = 999;
                double y1 = 0, y2 = 0, x3 = 0;
                double m_star = 0, epsilon_wet = 0, h_s_w_i = 0, h_s_w_o = 0, h_s_s_e = 0, T_s_e = 0, h_a_x = 0, T_a_x = 0;
                double Q_wet = 0, Ntu_owet = 0, mdot_min = 0;
                double Ntu_wet = 0;
                while ((iter <= 3 | change > eps) && iter < 100)
                {
                    if (iter == 1)
                        Tout_r = x1;
                    if (iter > 1)
                        Tout_r = x2;
                    double Tout_r_start = Tout_r;
                    h_s_w_i = CoolProp.HAPropsSI("H", "T", Tin_r + 273.15, "P", 101325, "R", 1.0);
                    //double c_s=CoolProp.HAPropsSI("C","T",(Tin_r+Tout_r)/2.0+273.15,"P",101325,"R",1);
                    double c_s = 2500;
                    //m = Math.Sqrt(2 * ha*c_s/cp_da / (k_fin * Fthickness));
                    //eta_a= Math.Tanh(m * h_fin) / (m * h_fin);// may cause error
                    //double m_star=ma/(mr*(cp_r/c_s));
                    //eta_a = eta_a * c_s / cp_da;
                    Ntu_owet = (eta_a * Aa_fin + Aa_tube) * haw / (ma * cp_da);//zzc
                    m_star = Math.Min(cp_r * mr / c_s, ma) / Math.Max(cp_r * mr / c_s, ma);
                    mdot_min = Math.Min(cp_r * mr / c_s, ma);
                    Ntu_wet = Ntu_o / (1 + m_star * (Ntu_owet / Ntu_i));
                    if (cp_r * mr > c_s * ma)
                        Ntu_wet = Ntu_o / (1 + m_star * (Ntu_owet / Ntu_i));
                    else
                        Ntu_wet = Ntu_i / (1 + m_star * (Ntu_i / Ntu_owet));
                    epsilon_wet = ((1 - Math.Exp(-Ntu_wet * (1 - m_star))) / (1 - m_star * Math.Exp(-Ntu_wet * (1 - m_star))));
                    Q_wet = epsilon_wet * mdot_min * (hin_a - h_s_w_i);
                    hout_a = hin_a - Q_wet / ma;
                    Tout_r = Tin_r + ma / (mr * cp_r) * (hin_a - hout_a);
                    h_s_w_o = CoolProp.HAPropsSI("H", "T", Tout_r + 273.15, "P", 101325, "R", 1.0);
                    double UA_star = 1 / (cp_da / (eta_a * Aa_fin + Aa_tube) / haw + CoolProp.HAPropsSI("C", "T", (Tin_a + Tout_r) / 2.0 + 273.15, "P", 101325, "R", 1) / h_r / Ar);//zzc
                    Tin_s = Tout_r + UA_star / h_r / Ar * (hin_a - h_s_w_o);
                    h_s_s_e = hin_a + (hout_a - hin_a) / (1 - Math.Exp(-Ntu_o));
                    T_s_e = CoolProp.HAPropsSI("T", "H", h_s_s_e, "P", 101325, "R", 1.0) - 273.15;
                    Tout_a = T_s_e + (Tin_a - T_s_e) * Math.Exp(-Ntu_o);
                    Q_sensible = ma * cp_da * (Tin_a - Tout_a);
                    double errorToutr = Tout_r - Tout_r_start;
                    if (iter == 1)
                        y1 = errorToutr;
                    if (iter > 1)
                    {
                        y2 = errorToutr;
                        x3 = x2 - y2 / (y2 - y1) * (x2 - x1);
                        change = Math.Abs(y2 / (y2 - y1) * (x2 - x1));
                        y1 = y2;
                        x1 = x2;
                        x2 = x3;
                    }
                    iter++;
                }
                //if (iter > 500)
                //    Q = Q_dry;
                double Tout_r_wet = Tout_r;
                f_dry = 0.0;
                if ((Tin_s > Tdp) && isFullyWet == false)
                {
                    double iter1 = 1;
                    double error = 1;
                    double Tout_r_guess = 0;
                    x1 = 0.0001;
                    x2 = 0.9999;
                    eps = 1e-8;
                    while ((iter1 <= 3 | change > eps) && iter < 100)
                    {
                        if (iter1 == 1)
                            f_dry = x1;
                        if (iter1 > 1)
                            f_dry = x2;
                        double K = Ntu_dry * (1.0 - C_star);
                        double expk = Math.Exp(-K * f_dry);
                        if (cp_da * ma < cp_r * mr)
                            Tout_r_guess = (Tdp + C_star * (Tin_a - Tdp) - expk * (1 - K / Ntu_o) * Tin_a) / (1 - expk * (1 - K / Ntu_o));
                        else
                            Tout_r_guess = (expk * (Tin_a + (C_star - 1) * Tdp) - C_star * (1 + K / Ntu_o) * Tin_a) / (expk * C_star - C_star * (1 + K / Ntu_o));
                        epsilon_dry = ((1 - Math.Exp(-f_dry * Ntu_dry * (1 - C_star))) / (1 - C_star * Math.Exp(-f_dry * Ntu_dry * (1 - C_star))));
                        epsilon_wet = ((1 - Math.Exp(-(1 - f_dry) * Ntu_wet * (1 - m_star))) / (1 - m_star * Math.Exp(-(1 - f_dry) * Ntu_wet * (1 - m_star))));
                        double T_w_x = (Tin_r + (mdot_min) / (cp_r * mr) * epsilon_wet * (hin_a - h_s_w_i - epsilon_dry * Cmin / ma * Tin_a)) / (1 - (Cmin * mdot_min) / (cp_r * mr * ma) * epsilon_wet * epsilon_dry);
                        T_a_x = Tin_a - epsilon_dry * Cmin * (Tin_a - T_w_x) / (ma * cp_da);
                        h_a_x = hin_a - cp_da * (Tin_a - T_a_x);
                        Tout_r = (Cmin) / (cp_r * mr) * epsilon_dry * Tin_a + (1 - (Cmin) / (cp_r * mr) * epsilon_dry) * T_w_x;
                        error = Tout_r - Tout_r_guess;
                        if (iter1 > 500)
                            Q = Q_dry;
                        if (iter1 == 1)
                            y1 = error;
                        if (iter1 > 1)
                        {
                            y2 = error;
                            x3 = x2 - y2 / (y2 - y1) * (x2 - x1);
                            change = Math.Abs(y2 / (y2 - y1) * (x2 - x1));
                            y1 = y2;
                            x1 = x2;
                            x2 = x3;
                        }
                        iter1++;
                    }
                    //if (iter1 > 500)
                    //    Q = Q_dry;
                    h_s_s_e = h_a_x + (hout_a - h_a_x) / (1 - Math.Exp(-(1 - f_dry) * Ntu_o));
                    T_s_e = CoolProp.HAPropsSI("T", "H", h_s_s_e, "P", 101325, "R", 1.0) - 273.15;
                    Tout_a = T_s_e + (T_a_x - T_s_e) * Math.Exp(-(1 - f_dry) * Ntu_o);
                    Q = mr * cp_r * (Tout_r - Tin_r);
                    hout_a = hin_a - Q / ma;
                    Q_sensible = ma * cp_da * (Tin_a - Tout_a);
                }
                else
                    Q = Q_wet;
            }
            else
            {
                Tout_a = Tout_a_dry;
                Q = Q_dry;
                Q_sensible = Q_dry;
            }
            res.Tao = Tout_a;
            res.Tro = Tout_r;
            res.Q = Q / 1000;
            res.RHout = CoolProp.HAPropsSI("R", "T", Tout_a + 273.15, "P", 101325, "H", hin_a + Math.Pow(-1, hexType + 1) * Q / ma);
            double f_sp = RefrigerantSPDP.ff_Friction(Re_r);
            res.DP = zdp * f_sp * l / dh * Math.Pow(g, 2.0) / rho_r / 2000;
            res.Pro = fluid == "Water" ? pri : pri - res.DP;
            if (res.Pro < 0) { res.Pro = -10000000; return res; }
            res.hro = hri + Math.Pow(-1, hexType) * res.Q / mr;
            return res; 


        }

        public static CalcResult ElementCalc3(string fluid, double dh, double l, double Aa_fin, double Aa_tube, double A_r_cs, double Ar, double tai,
         double RHi, double tri, double pri, double hri, double mr, double g, double ma, double ha, double haw,
         double eta_surface, double zh, double zdp, int hexType, double thickness, double conductivity, double Pwater)
        {
            double Q = 0;
            double Tout_a = 0;
            double Q_sensible = 0;
            double r_metal = thickness / conductivity / Ar;
            CalcResult res = new CalcResult();
            double mu_r = CoolProp.PropsSI("V", "H", hri * 1000, "P", (fluid == "Water" ? Pwater : pri) * 1000, fluid);
            double k_r = CoolProp.PropsSI("L", "H", hri * 1000, "P", (fluid == "Water" ? Pwater : pri) * 1000, fluid);
            double rho_r = CoolProp.PropsSI("D", "H", hri * 1000, "P", (fluid == "Water" ? Pwater : pri) * 1000, fluid);
            double cp_r = CoolProp.PropsSI("C", "H", hri * 1000, "P", (fluid == "Water" ? Pwater : pri) * 1000, fluid);
            double Pr_r = cp_r * mu_r / k_r;
            res.Vel_r = g / rho_r;
            double Re_r = rho_r * res.Vel_r * dh / mu_r;
            double fh = RefrigerantSPHTC.ff_CHURCHILL(Re_r);
            double Nusselt = RefrigerantSPHTC.NU_GNIELINSKI(Re_r, Pr_r, fh);

            res.href = Nusselt * k_r / dh * zh;
            double cp_da = 1.0;
            cp_da = (hexType == 0 ? 1.027 : 1.02)*1000;
            double Tin_a = tai;
            double Tin_r = tri;
            cp_da = CoolProp.HAPropsSI("C", "T", Tin_a + 273.15, "P", 101325, "R", RHi);            
            double h_r = res.href;
            double k_fin = 237;
            double Fthickness = 0.095 * 0.001;
            double Pt = 21 * 0.001;
            double Pr = 22 * 0.001;
            double Do = 7.35 * 0.001;

            //double h_fin = (Pt - Do) / 2;
            //double dc = Do + 2 * Fthickness;
            //double df = Math.Pow(4 / Math.PI * Pt * Pr, 0.5);
            //double eta_a = 1 / (1 + ha * Math.Pow(df - dc, 2) * Math.Pow(df, 0.5) / 6 / Fthickness / k_fin / Math.Pow(dc, 0.5));
            //double m = Math.Sqrt(2 * ha / (k_fin * Fthickness));
            //double eta_a= Math.Tanh(m * h_fin) / (m * h_fin);// may cause error
            //double eta_dry = eta_surface;

            double r_eta = Do / 2;
            double XD = Math.Pow((Pr * Pr + Pt * Pt / 4), 0.5) / 2;
            double XT = Pt / 2;
            double rf_r = 1.27 * XT / r_eta * Math.Pow((XD / XT - 0.3), 0.5);
            double m = Math.Pow((2 * ha / 237 / Fthickness), 0.5);
            double fai = (rf_r - 1) * (1 + (0.3 + Math.Pow((m * r_eta * (rf_r - r_eta) / 2.5), (1.5 - 1 / 12 * rf_r)) * (0.26 * Math.Pow(rf_r, 0.3) - 0.3)) * Math.Log(rf_r));
            double eta_0 = Math.Tanh(m * r_eta * fai) / m / r_eta / fai * Math.Cos(0.1 * m * r_eta * fai);
            double eta_a = (eta_0 * Aa_fin + Aa_tube) / (Aa_fin + Aa_tube);

            double UA_i = h_r * Ar;
            double UA_o = eta_a * (Aa_fin + Aa_tube) * ha;
            double Ntu_i = UA_i / (mr * cp_r);
            double Ntu_o = UA_o / (ma * cp_da);
            double UA = 1 / (1 / (UA_i) + 1 / (UA_o)+r_metal);
            double Cmin = Math.Min(cp_r * mr, cp_da * ma);
            double Cmax = Math.Max(cp_r * mr, cp_da * ma);
            double C_star = Cmin / Cmax;
            double Ntu_dry = UA / Cmin;
            double epsilon_dry = (1 - Math.Exp(-Ntu_dry * (1 - C_star))) / (1 - C_star * Math.Exp(-Ntu_dry * (1 - C_star)));
            double Q_dry = epsilon_dry * Cmin * (Tin_a - Tin_r) * Math.Pow(-1, hexType);//need to be modified
            double Tout_a_dry = Tin_a - Q_dry / (ma * cp_da) * Math.Pow(-1, hexType);
            double Tout_r = Tin_r + Q_dry / (mr * cp_r) * Math.Pow(-1, hexType);            
            double Tout_s = (UA_o * Tout_a_dry + UA_i * Tin_r) / (UA_o + UA_i);
            double Tin_s = (UA_o * Tin_a + UA_i * Tout_r) / (UA_o + UA_i);
            double Tout_r_dry = Tout_r;
            double f_dry = 1.0;
            double omega_in = CoolProp.HAPropsSI("W", "T", Tin_a + 273.15, "P", 101325, "R", RHi);
            double omega_out = omega_in;

            if(hexType==0&&tri<tai)
            {
                bool isFullyWet = true;
                double hin_a = CoolProp.HAPropsSI("H", "T", Tin_a + 273.15, "P", 101325, "R", RHi);
                double hout_a = hin_a - Q_dry / ma * Math.Pow(-1, hexType);
                double Tdp = CoolProp.HAPropsSI("D", "T", Tin_a + 273.15, "P", 101325, "R", RHi) - 273.15;
                res.RHout = CoolProp.HAPropsSI("R", "T", Tout_a_dry + 273.15, "P", 101325, "W", omega_out);
                Tout_a = Tout_a_dry;
                Q = Q_dry;
                if (Tin_s < Tdp)
                    isFullyWet = true;
                else
                    isFullyWet = false;
                if (Tout_s < Tdp | isFullyWet == true)
                {
                    double x1 = Tin_r + 0.001;
                    double x2 = Tin_a - 0.001;
                    double eps = 1e-8;
                    int iter = 1;
                    double change = 999;
                    double y1 = 0, y2 = 0, x3 = 0;
                    double m_star = 0, epsilon_wet = 0, h_s_w_i = 0, h_s_w_o = 0, h_s_s_e = 0, T_s_e = 0, h_a_x = 0, T_a_x = 0;
                    double Q_wet = 0, Ntu_owet = 0, mdot_min = 0;
                    double Ntu_wet = 0;
                    while ((iter <= 3 | change > eps) && iter < 100)
                    {
                        if (iter == 1)
                            Tout_r = x1;
                        if (iter > 1)
                            Tout_r = x2;
                        double Tout_r_start = Tout_r;
                        h_s_w_i = CoolProp.HAPropsSI("H", "T", Tin_r + 273.15, "P", 101325, "R", 1.0);
                        double c_s = (CoolProp.HAPropsSI("H", "T", (Tin_r + Tout_r) / 2 + 273.15 + 0.01, "P", 101325, "R", 1) - CoolProp.HAPropsSI("H", "T", (Tin_r + Tout_r) / 2 + 273.15, "P", 101325, "R", 1)) / 0.01;
                        //double c_s = 2500;
                        //m = Math.Sqrt(2 * ha*c_s/cp_da / (k_fin * Fthickness));
                        //eta_a= Math.Tanh(m * h_fin) / (m * h_fin);// may cause error
                        //double m_star=ma/(mr*(cp_r/c_s));
                        //eta_a = eta_a * c_s / cp_da;
                        m = Math.Pow((2 * haw / 237 / Fthickness), 0.5);
                        fai = (rf_r - 1) * (1 + (0.3 + Math.Pow((m * r_eta * (rf_r - r_eta) / 2.5), (1.5 - 1 / 12 * rf_r)) * (0.26 * Math.Pow(rf_r, 0.3) - 0.3)) * Math.Log(rf_r));
                        eta_0 = Math.Tanh(m * r_eta * fai) / m / r_eta / fai * Math.Cos(0.1 * m * r_eta * fai);
                        double eta_wet = (eta_0 * Aa_fin + Aa_tube) / (Aa_fin + Aa_tube);
                        Ntu_owet = (eta_0 * Aa_fin + Aa_tube) * haw / (ma * cp_da);//zzc
                        m_star = Math.Min(cp_r * mr / c_s, ma) / Math.Max(cp_r * mr / c_s, ma);
                        mdot_min = Math.Min(cp_r * mr / c_s, ma);
                        Ntu_wet = Ntu_owet / (1 + m_star * (Ntu_owet / Ntu_i));//zzc
                        if (cp_r * mr > c_s * ma)
                            Ntu_wet = Ntu_owet / (1 + m_star * (Ntu_owet / Ntu_i));//zzc
                        else
                            Ntu_wet = Ntu_i / (1 + m_star * (Ntu_i / Ntu_owet));
                        epsilon_wet = ((1 - Math.Exp(-Ntu_wet * (1 - m_star))) / (1 - m_star * Math.Exp(-Ntu_wet * (1 - m_star))));
                        Q_wet = epsilon_wet * mdot_min * (hin_a - h_s_w_i);
                        hout_a = hin_a - Q_wet / ma;
                        Tout_r = Tin_r + ma / (mr * cp_r) * (hin_a - hout_a);
                        h_s_w_o = CoolProp.HAPropsSI("H", "T", Tout_r + 273.15, "P", 101325, "R", 1.0);
                        //double UA_star = 1 / (cp_da / (eta_a * Aa_fin + Aa_tube) / haw + CoolProp.HAPropsSI("C", "T", (Tin_a + Tout_r) / 2.0 + 273.15, "P", 101325, "R", 1) / h_r / Ar);//zzc
                        double UA_star = 1 / (cp_da / (eta_a * Aa_fin + Aa_tube) / haw + c_s / h_r / Ar);//zzc
                        Tin_s = Tout_r + UA_star / h_r / Ar * (hin_a - h_s_w_o);
                        h_s_s_e = hin_a + (hout_a - hin_a) / (1 - Math.Exp(-Ntu_owet));//zzc
                        T_s_e = CoolProp.HAPropsSI("T", "H", h_s_s_e, "P", 101325, "R", 1.0) - 273.15;
                        Tout_a = T_s_e + (Tin_a - T_s_e) * Math.Exp(-Ntu_owet);//zzc
                        Q_sensible = ma * cp_da * (Tin_a - Tout_a);
                        double errorToutr = Tout_r - Tout_r_start;
                        if (iter == 1)
                            y1 = errorToutr;
                        if (iter > 1)
                        {
                            y2 = errorToutr;
                            x3 = x2 - y2 / (y2 - y1) * (x2 - x1);
                            change = Math.Abs(y2 / (y2 - y1) * (x2 - x1));
                            y1 = y2;
                            x1 = x2;
                            x2 = x3;
                        }
                        iter++;
                    }
                    //if (iter > 500)
                    //    Q = Q_dry;
                    double Tout_r_wet = Tout_r;
                    f_dry = 0.0;
                    if ((Tin_s > Tdp) && isFullyWet == false)
                    {
                        double iter1 = 1;
                        double error = 1;
                        double Tout_r_guess = 0;
                        x1 = 0.0001;
                        x2 = 0.9999;
                        eps = 1e-8;
                        while ((iter1 <= 3 | change > eps) && iter < 100)
                        {
                            if (iter1 == 1)
                                f_dry = x1;
                            if (iter1 > 1)
                                f_dry = x2;
                            double K = Ntu_dry * (1.0 - C_star);
                            double expk = Math.Exp(-K * f_dry);
                            if (cp_da * ma < cp_r * mr)
                                Tout_r_guess = (Tdp + C_star * (Tin_a - Tdp) - expk * (1 - K / Ntu_o) * Tin_a) / (1 - expk * (1 - K / Ntu_o));//zzc
                            else
                                Tout_r_guess = (expk * (Tin_a + (C_star - 1) * Tdp) - C_star * (1 + K / Ntu_o) * Tin_a) / (expk * C_star - C_star * (1 + K / Ntu_o));//zzc
                            epsilon_dry = ((1 - Math.Exp(-f_dry * Ntu_dry * (1 - C_star))) / (1 - C_star * Math.Exp(-f_dry * Ntu_dry * (1 - C_star))));
                            epsilon_wet = ((1 - Math.Exp(-(1 - f_dry) * Ntu_wet * (1 - m_star))) / (1 - m_star * Math.Exp(-(1 - f_dry) * Ntu_wet * (1 - m_star))));
                            double T_w_x = (Tin_r + (mdot_min) / (cp_r * mr) * epsilon_wet * (hin_a - h_s_w_i - epsilon_dry * Cmin / ma * Tin_a)) / (1 - (Cmin * mdot_min) / (cp_r * mr * ma) * epsilon_wet * epsilon_dry);
                            T_a_x = Tin_a - epsilon_dry * Cmin * (Tin_a - T_w_x) / (ma * cp_da);
                            h_a_x = hin_a - cp_da * (Tin_a - T_a_x);
                            Tout_r = (Cmin) / (cp_r * mr) * epsilon_dry * Tin_a + (1 - (Cmin) / (cp_r * mr) * epsilon_dry) * T_w_x;
                            error = Tout_r - Tout_r_guess;
                            if (iter1 > 500)
                                Q = Q_dry;
                            if (iter1 == 1)
                                y1 = error;
                            if (iter1 > 1)
                            {
                                y2 = error;
                                x3 = x2 - y2 / (y2 - y1) * (x2 - x1);
                                change = Math.Abs(y2 / (y2 - y1) * (x2 - x1));
                                y1 = y2;
                                x1 = x2;
                                x2 = x3;
                            }
                            iter1++;
                        }
                        //if (iter1 > 500)
                        //    Q = Q_dry;
                        h_s_s_e = h_a_x + (hout_a - h_a_x) / (1 - Math.Exp(-(1 - f_dry) * Ntu_owet));//zzc
                        T_s_e = CoolProp.HAPropsSI("T", "H", h_s_s_e, "P", 101325, "R", 1.0) - 273.15;
                        Tout_a = T_s_e + (T_a_x - T_s_e) * Math.Exp(-(1 - f_dry) * Ntu_owet);//zzc
                        Q = mr * cp_r * (Tout_r - Tin_r);
                        hout_a = hin_a - Q / ma;
                        Q_sensible = ma * cp_da * (Tin_a - Tout_a);
                    }
                    else Q = Q_wet;
                    res.RHout = CoolProp.HAPropsSI("R", "T", Tout_a + 273.15, "P", 101325, "H", hin_a - Q / ma);
                }
            }            
            else
            {
                Tout_a = Tout_a_dry;
                Q = Q_dry;
                Q_sensible = Q_dry;
                res.RHout = CoolProp.HAPropsSI("R", "T", Tout_a + 273.15, "P", 101325, "W", omega_out);
            }
            res.Tao = Tout_a;
            res.Tro = Tout_r;
            res.Q = Q / 1000;
            //res.hro = (hri + Math.Pow(-1, hexType) * res.Q / mr);
            double f_sp = RefrigerantSPDP.ff_Friction(Re_r);
            res.DP = zdp * f_sp * l / dh * Math.Pow(g, 2.0) / rho_r / 2000;
            res.Pro = fluid == "Water" ? pri : pri - res.DP;
            if (res.Pro < 0) { res.Pro = -10000000; return res; }
            res.hro = hri + Math.Pow(-1, hexType) * res.Q / mr;
            res.R_1a = 1 / ((eta_0 * Aa_fin + Aa_tube) * ha);
            res.R_1r = 1 / (res.href * Ar);
            res.R_1 = res.R_1a + res.R_1r + r_metal;
            if (fluid != "Water") res.Tro = CoolProp.PropsSI("T", "P", res.Pro * 1000, "H", res.hro * 1000, fluid) - 273.15;
            return res; 

        }



    }
}
