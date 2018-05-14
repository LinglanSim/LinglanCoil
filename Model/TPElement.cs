using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Basic;

namespace Model
{
    public class TPElement
    {
        public static CalcResult ElementCalc(string fluid, double dh, double l, double Aa_fin, double Aa_tube, double A_r_cs, double Ar, double tai, 
            double RHi,double tri, double pri, double hri, double mr, double g, double ma, double ha,double haw,
            double eta_surface, double zh, double zdp, int hexType, double thickness, double conductivity)
        {
            double r_metal = thickness / conductivity / Ar;
            double gg = 9.8;
            double q_initial = 0.01;
            double q = q_initial;
            double err = 0.01;
            bool flag = false;
            int iter = 1;
            CalcResult res=new CalcResult();
            //res.Tao[0] = new double();          
            double EnthalpyL, EnthalpyV; 
            //recalc tri to make sure it is 2ph, ruhao, 20180225 
            tri = CoolProp.PropsSI("T", "P", pri * 1000, "Q", 0, fluid) - 273.15;
            EnthalpyL = CoolProp.PropsSI("H", "T", tri + 273.15, "Q", 0, fluid) / 1000 ;
            EnthalpyV = CoolProp.PropsSI("H", "T", tri + 273.15, "Q", 1, fluid) / 1000 ;

            res.x_i = (hri - EnthalpyL) / (EnthalpyV - EnthalpyL);
            RefHTCandDPResult htc_dp = new RefHTCandDPResult();

            do
            {
                flag = false;
                htc_dp = RefrigerantHTCandDP.HTCandDP_2p(fluid, dh, g, pri, res.x_i, l, q, zh, zdp, hexType);

                res.href = htc_dp.Href;
                res.DP = htc_dp.DPref;
                double cp_a = 1.0; //keep it for now as a constant
                cp_a = (hexType == 0 ? 1.027 : 1.02);
                double C_a = ma * cp_a;
                res.R_1a = 1 / ((eta_surface * Aa_fin + Aa_tube) * ha);
                res.R_1r = 1 / (res.href * Ar);
                res.R_1 = res.R_1a + res.R_1r + r_metal;
                double UA = 1 / res.R_1;
                double NTU = UA / (C_a * 1000);
                double epsilon = 1 - Math.Exp(-NTU);
                res.Q = epsilon * C_a * (tai - tri) * Math.Pow(-1, hexType);//Math.Abs(tai - tri)
                res.Tao = tai + Math.Pow(-1, (hexType + 1)) * res.Q / C_a;
                res.hro = hri + Math.Pow(-1, hexType) * res.Q / mr;
                //0:evap, 1:cond
                res.x_o = (res.hro - EnthalpyL) / (EnthalpyV - EnthalpyL); //+ 139.17 for reference state, to be changed
                //res.DP = 0;
                res.Pro = pri - res.DP;
                if (res.Pro < 0) { res.Pro = -10000000; return res; }
                res.Tro = CoolProp.PropsSI("T", "P", res.Pro * 1000, "H", res.hro * 1000, fluid);
                double rho_o= CoolProp.PropsSI("D", "P", res.Pro * 1000, "H", res.hro  * 1000, fluid);
                res.Tro = res.Tro - 273.15;
                res.Vel_r = g / rho_o;
                if (Math.Abs(q - res.Q) / res.Q > err)
                {
                    q = res.Q;
                    flag = true;
                }
                iter++;
            } while (flag && iter < 100);

            if (iter>=100)
            {
                throw new Exception("iter for href > 100.");
            }
            return res; 

        }
        public static CalcResult ElementCalc1(string fluid, double dh, double l, double Aa_fin, double Aa_tube, double A_r_cs, double Ar, double tai,
            double RHi,double tri, double pri, double hri, double mr, double g, double ma, double ha,double haw,
            double eta_surface, double zh, double zdp, int hexType, double thickness, double conductivity)
        {
            double r_metal = thickness / conductivity / Ar;
            double gg = 9.8;
            double q_initial = 0.01;
            double q = q_initial;
            bool flag = false;
            double err = 0.01;
            int iter = 1;
            CalcResult res=new CalcResult();     
            double EnthalpyL, EnthalpyV; 
            tri = CoolProp.PropsSI("T", "P", pri * 1000, "Q", 0, fluid) - 273.15;
            EnthalpyL = CoolProp.PropsSI("H", "T", tri + 273.15, "Q", 0, fluid) / 1000 ;
            EnthalpyV = CoolProp.PropsSI("H", "T", tri + 273.15, "Q", 1, fluid) / 1000 ;

            res.x_i = (hri - EnthalpyL) / (EnthalpyV - EnthalpyL);
            RefHTCandDPResult htc_dp = new RefHTCandDPResult();

            do
            {
                flag = false;
                htc_dp = RefrigerantHTCandDP.HTCandDP_2p(fluid, dh, g, pri, res.x_i, l, q, zh, zdp, hexType);
                res.href = htc_dp.Href;
                res.DP = htc_dp.DPref;

                double k_fin = 237;
                double Fthickness = 0.095 * 0.001;
                double Pt = 21 * 0.001;
                double Pr = 22 * 0.001;
                double Do = 7.35 * 0.001;
                double r_eta = Do / 2;
                double XD = Math.Pow((Pr * Pr + Pt * Pt / 4), 0.5) / 2;
                double XT = Pt / 2;
                double rf_r = 1.27 * XT / r_eta * Math.Pow((XD / XT - 0.3), 0.5);
                double m = Math.Pow((2 * ha / 237 / Fthickness), 0.5);
                double fai = (rf_r - 1) * (1 + (0.3 + Math.Pow((m * r_eta * (rf_r - r_eta) / 2.5), (1.5 - 1 / 12 * rf_r)) * (0.26 * Math.Pow(rf_r, 0.3) - 0.3)) * Math.Log(rf_r));
                double eta_0 = Math.Tanh(m * r_eta * fai) / m / r_eta / fai * Math.Cos(0.1 * m * r_eta * fai);
                double eta_a = (eta_0 * Aa_fin + Aa_tube) / (Aa_fin + Aa_tube);



                double cp_a = CoolProp.HAPropsSI("C", "T", tai + 273.15, "P", 101325, "R", RHi);
                double UA = 1 / (1 / ha / (Aa_fin * eta_0 + Aa_tube) + 1 / (res.href * Ar)+r_metal);
                double Ntu_dry = UA / (ma * cp_a);
                double epsilon_dry = 1 - Math.Exp(-Ntu_dry);
                double Q_dry = epsilon_dry * ma * cp_a * (tai - tri) * Math.Pow(-1, hexType);
                double tao = tai - Q_dry / (ma * cp_a) * Math.Pow(-1, hexType);
                double hro = hri + Q_dry / 1000 / mr * Math.Pow(-1, hexType);

                double UA_o = ha * (Aa_fin * eta_surface + Aa_tube);
                double UA_i = res.href * Ar;
                double NTU_o = UA_o / (cp_a * ma);
                double T_so_a = (UA_o * tai + UA_i * tri) / (UA_o + UA_i);
                double T_so_b = (UA_o * tao + UA_i * tri) / (UA_o + UA_i);
                double Tdp = CoolProp.HAPropsSI("D", "T", tai + 273.15, "P", 101325, "R", RHi) - 273.15;
                double Q = Q_dry;
                double Q_sensible = 0;
                double omega_in = CoolProp.HAPropsSI("W", "T", tai + 273.15, "P", 101325, "R", RHi);
                double omega_out = omega_in;
                double f_dry = 0;
                double hai = CoolProp.HAPropsSI("H", "T", tai + 273.15, "P", 101325, "R", RHi);

                if(hexType==0)
                {
                    if(T_so_b>Tdp)
                    {
                        f_dry=1.0;
                        Q=Q_dry;
                        Q_sensible=Q;
                        omega_out = omega_in; 
                    }
                    else
                    {
                        double T_ac=0;
                        double h_ac=0;
                        if (T_so_a<Tdp)
                        {
                            f_dry=0.0;
                            Q_dry=0.0;
                            T_ac=tai; 
                            h_ac=hai; 
                        }
                        else
                        {
                            T_ac = Tdp + UA_i/UA_o*(Tdp - tri);
                            epsilon_dry=(tai-T_ac)/(tai-tri);
                            f_dry=-1.0/Ntu_dry*Math.Log(1.0-epsilon_dry);
                            h_ac=CoolProp.HAPropsSI("H","T",T_ac+273.15,"P",101325,"W",omega_in);
                            Q_dry=ma*cp_a*(tai-T_ac);
                        }

                        double c_s=(CoolProp.HAPropsSI("H","T",tri+273.15+0.01,"P",101325,"R",1.0)-CoolProp.HAPropsSI("H","T",tri+273.15-0.01,"P",101325,"R",1.0))/2;
                        m = Math.Pow((2 * haw / 237 / Fthickness), 0.5);
                        fai = (rf_r - 1) * (1 + (0.3 + Math.Pow((m * r_eta * (rf_r - r_eta) / 2.5), (1.5 - 1 / 12 * rf_r)) * (0.26 * Math.Pow(rf_r, 0.3) - 0.3)) * Math.Log(rf_r));
                        double eta_wet = Math.Tanh(m * r_eta * fai) / m / r_eta / fai * Math.Cos(0.1 * m * r_eta * fai);
                        //eta_a = (eta_0 * Aa_fin + Aa_tube) / (Aa_fin + Aa_tube);                                             
                        UA_o=haw*(eta_wet*Aa_fin+Aa_tube);
                        NTU_o=UA_o/(ma*cp_a);                
                        double UA_wet=1/(c_s/UA_i+cp_a/UA_o);
                        double Ntu_wet=UA_wet/ma;
                        double epsilon_wet=1-Math.Exp(-(1-f_dry)*Ntu_wet);
                        double h_s_s_o=CoolProp.HAPropsSI("H","T",tri, "P",101325,"R", 1.0);          
                        double Q_wet=epsilon_wet*ma*(h_ac-h_s_s_o);
                        Q=Q_wet+Q_dry;
                        double hao=h_ac-Q_wet/ma;
                        double h_s_s_e=h_ac-(h_ac-hao)/(1-Math.Exp(-(1-f_dry)*NTU_o));
                        double T_s_e = CoolProp.HAPropsSI("T","H",h_s_s_e,"P",101325,"R",1.0)-273.15;
                        tao = T_s_e+(T_ac-T_s_e)*Math.Exp(-(1-f_dry)*NTU_o);
                        Q_sensible=ma*cp_a*(tai-tao);
                        hro = hri + Q/1000 / mr;
                    }                     
                }
                res.R_1a = 1 / ((eta_surface * Aa_fin + Aa_tube) * ha);
                res.R_1r = 1 / (res.href * Ar);
                res.R_1 = res.R_1a + res.R_1r + r_metal;
                res.Q = Q / 1000;
                res.Tao = tao;
                res.hro = hro;
                res.Pro = pri - res.DP;
                if (res.Pro < 0) { res.Pro = -10000000; return res; }
                res.Tro = CoolProp.PropsSI("T", "P", res.Pro * 1000, "H", res.hro * 1000, fluid) - 273.15;
                double rho_o = CoolProp.PropsSI("D", "P", res.Pro * 1000, "H", res.hro * 1000, fluid);
                res.Vel_r = g / rho_o;
                res.x_o = (res.hro - EnthalpyL) / (EnthalpyV - EnthalpyL); //+ 139.17 for reference state, to be changed
                if (Math.Abs(q - res.Q) / res.Q > err)
                {
                    q = res.Q;
                    flag = true;
                }
                iter++;
            } while (flag && iter < 100);
            if (iter >= 100)
            {
                throw new Exception("iter for href > 100.");
            }
            
            return res;
          
        }
    }
}
