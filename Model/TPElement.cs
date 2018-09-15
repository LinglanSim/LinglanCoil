using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Basic;

namespace Model
{
    public class TPElement
    {
        public static CalcResult ElementCalc(string fluid, double l, double Aa_fin, double Aa_tube, double A_r_cs, double Ar, Geometry geo, double tai,
            double RHi, double tri, double pri, double hri, double mr, double g, double ma, double ha, double haw,
            double eta_surface, double zh, double zdp, int hexType, double thickness, double conductivity, AbstractState coolprop)
        {
            //AbstractState coolprop = AbstractState.factory("HEOS", fluid);

            double dh = geo.Di;
            double r_metal = thickness / conductivity / Ar;
            double gg = 9.8;
            double q_initial = 0.01;
            double q = q_initial;
            double err = 0.01;
            bool flag = false;
            int iter = 1;
            CalcResult res = new CalcResult();
            //res.Tao[0] = new double();          
            double EnthalpyL, EnthalpyV;
            //recalc tri to make sure it is 2ph, ruhao, 20180225 
            coolprop.update(input_pairs.PQ_INPUTS, pri * 1000, 0);
            tri = coolprop.T() - 273.15;
            //tri = CoolProp.PropsSI("T", "P", pri * 1000, "Q", 0, fluid) - 273.15;
            coolprop.update(input_pairs.QT_INPUTS, 0, tri + 273.15);
            EnthalpyL = coolprop.hmass() / 1000;
            //EnthalpyL = CoolProp.PropsSI("H", "T", tri + 273.15, "Q", 0, fluid) / 1000;
            coolprop.update(input_pairs.QT_INPUTS, 1, tri + 273.15);
            EnthalpyV = coolprop.hmass() / 1000;
            //EnthalpyV = CoolProp.PropsSI("H", "T", tri + 273.15, "Q", 1, fluid) / 1000;

            res.x_i = (hri - EnthalpyL) / (EnthalpyV - EnthalpyL);
            RefHTCandDPResult htc_dp = new RefHTCandDPResult();

            do
            {
                flag = false;
                htc_dp = RefrigerantHTCandDP.HTCandDP_2p(fluid, dh, g, pri, res.x_i, l, q, zh, zdp, hexType, coolprop);

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
                coolprop.update(input_pairs.HmassP_INPUTS, res.hro * 1000, res.Pro * 1000);
                res.Tro = coolprop.T();
                //res.Tro = CoolProp.PropsSI("T", "P", res.Pro * 1000, "H", res.hro * 1000, fluid);
                double rho_o = coolprop.rhomass();
                //double rho_o = CoolProp.PropsSI("D", "P", res.Pro * 1000, "H", res.hro * 1000, fluid);
                res.Tro = res.Tro - 273.15;
                res.Vel_r = g / rho_o;
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
        public static CalcResult ElementCalc1(string fluid, double l, double Aa_fin, double Aa_tube, double A_r_cs, double Ar, Geometry geo, double tai,
            double RHi, double tri, double pri, double hri, double mr, double g, double ma, double ha, double haw,
            double eta_surface, double zh, double zdp, int hexType, double thickness, double conductivity, AbstractState coolprop)
        {
            Model.HumidAirProp humidairprop = new Model.HumidAirProp();

            RHi = RHi > 1 ? 1 : RHi;
            double dh = geo.Di;
            double r_metal = thickness / conductivity / Ar;
            double gg = 9.8;
            double q_initial = 0.01;
            double q = q_initial;
            bool flag = false;
            double err = 0.01;
            int iter = 1;
            CalcResult res = new CalcResult();
            double EnthalpyL, EnthalpyV;

            coolprop.update(input_pairs.PQ_INPUTS, pri * 1000, 0);
            tri = coolprop.T() - 273.15;
            //tri = CoolProp.PropsSI("T", "P", pri * 1000, "Q", 0, fluid) - 273.15;
            coolprop.update(input_pairs.QT_INPUTS, 0, tri + 273.15);
            EnthalpyL = coolprop.hmass() / 1000;
            //EnthalpyL = CoolProp.PropsSI("H", "T", tri + 273.15, "Q", 0, fluid) / 1000;
            coolprop.update(input_pairs.QT_INPUTS, 1, tri + 273.15);
            EnthalpyV = coolprop.hmass() / 1000;
            //EnthalpyV = CoolProp.PropsSI("H", "T", tri + 273.15, "Q", 1, fluid) / 1000;

            res.x_i = (hri - EnthalpyL) / (EnthalpyV - EnthalpyL);
            RefHTCandDPResult htc_dp = new RefHTCandDPResult();

            do
            {
                flag = false;
                htc_dp = RefrigerantHTCandDP.HTCandDP_2p(fluid, dh, g, pri, res.x_i, l, q, zh, zdp, hexType, coolprop);

                res.href = htc_dp.Href;
                res.DP = htc_dp.DPref;

                double k_fin = 237;
                double Fthickness = geo.Fthickness;
                double Pt = geo.Pt;
                double Pr = geo.Pr;
                double Do = geo.Do;
                double r_eta = Do / 2;
                double XD = Math.Pow((Pr * Pr + Pt * Pt / 4), 0.5) / 2;
                double XT = Pt / 2;
                double rf_r = 1.27 * XT / r_eta * Math.Pow((XD / XT - 0.3), 0.5);
                double m = Math.Pow((2 * ha / k_fin / Fthickness), 0.5);
                double fai = (rf_r - 1) * (1 + (0.3 + Math.Pow((m * r_eta * (rf_r - r_eta) / 2.5), (1.5 - 1 / 12 * rf_r)) * (0.26 * Math.Pow(rf_r, 0.3) - 0.3)) * Math.Log(rf_r));
                double eta_0 = Math.Tanh(m * r_eta * fai) / m / r_eta / fai * Math.Cos(0.1 * m * r_eta * fai);
                double eta_a = (eta_0 * Aa_fin + Aa_tube) / (Aa_fin + Aa_tube);

                ////
                //double cp_a0 = CoolProp.HAPropsSI("C", "T", tai + 273.15, "P", 101325, "R", RHi);
                //double cp_a = 1005.1458551 + 0.1541627 * tai + 4.3454442 * RHi - 0.0090904 * Math.Pow(tai, 2) - 0.3409659 * Math.Pow(RHi, 2) - 0.0007819 * tai * RHi + 0.0001851 * Math.Pow(tai, 3) + 0.0049274 * Math.Pow(RHi, 3) + 0.0476513 * tai * Math.Pow(RHi, 2) + 0.020268209 * Math.Pow(tai, 2) * RHi;
                double cp_a = humidairprop.Cp(tai, RHi);

                double UA = 1 / (1 / ha / (Aa_fin * eta_0 + Aa_tube) + 1 / (res.href * Ar) + r_metal);
                double Ntu_dry = UA / (ma * cp_a);
                double epsilon_dry = 1 - Math.Exp(-Ntu_dry);
                double Q_dry = epsilon_dry * ma * cp_a * (tai - tri) * Math.Pow(-1, hexType);
                double tao = tai - Q_dry / (ma * cp_a) * Math.Pow(-1, hexType);
                double hro = hri + Q_dry / 1000 / mr * Math.Pow(-1, hexType);

                double UA_o = ha * (Aa_fin * eta_0 + Aa_tube);
                double UA_i = res.href * Ar;
                double NTU_o = UA_o / (cp_a * ma);
                double T_so_a = (UA_o * tai + UA_i * tri) / (UA_o + UA_i);
                double T_so_b = (UA_o * tao + UA_i * tri) / (UA_o + UA_i);
                double Q = Q_dry;
                double Q_sensible = 0;

                //double omega_in0 = CoolProp.HAPropsSI("W", "T", tai + 273.15, "P", 101325, "R", RHi);
                //double omega_in = (-0.0682340 + 0.0292341 * tai + 4.1604535 * RHi - 0.0025985 * Math.Pow(tai, 2) - 0.0769009 * Math.Pow(RHi, 2) + 0.1246489 * tai * RHi + 6.008 * Math.Pow(10, -5) * Math.Pow(tai, 3) - 0.0006775 * Math.Pow(RHi, 3) + 0.0267183 * tai * Math.Pow(RHi, 2) + 0.019904969 * Math.Pow(tai, 2) * RHi) / 1000;
                double omega_in = humidairprop.O(tai, RHi);

                //double hai0 = CoolProp.HAPropsSI("H", "T", tai + 273.15, "P", 101325, "R", RHi);
                //double hai = -244.2924077 + 1135.8711 * tai + 10101.404 * RHi - 12.968219 * Math.Pow(tai, 2) - 11.356807 * Math.Pow(RHi, 2) + 357.25464 * tai * RHi + 0.3178346 * Math.Pow(tai, 3) - 0.0024329 * Math.Pow(RHi, 3) + 44.100799 * tai * Math.Pow(RHi, 2) + 50.31444812 * Math.Pow(tai, 2) * RHi;
                double hai = humidairprop.H(tai, "R", RHi);

                //double ha2 = ((1.006 + 1.805 * omega_in1) * tai + 2501 * omega_in1) * 1000;//采用湿空气 物性的关系来写

                //double omega_out = omega_in;

                double hout_a = hai - Q / ma * Math.Pow(-1, hexType);

                /////
                //***delete***//
                //res.RHout = CoolProp.HAPropsSI("R", "T", tao + 273.15, "P", 101325, "W", omega_out);
                //res.RHout = 0.0215344 - 0.0059467 * tao + 0.2386894 * (omega_out * 1000) + 0.0004378 * Math.Pow(tao, 2) + 0.0004635 * Math.Pow((omega_out * 1000), 2) - 0.0125912 * tao * (omega_out * 1000) - 9.134 * Math.Pow(10, -6) * Math.Pow(tao, 3) + 1.696 * Math.Pow(10, -6) * Math.Pow((omega_out * 1000), 3) - 2.214 * Math.Pow(10, -5) * tao * Math.Pow((omega_out * 1000), 2) + 0.000200865 * Math.Pow(tao, 2) * (omega_out * 1000);
                //***delete***//

                //double resRHout0 = CoolProp.HAPropsSI("R", "T", tao + 273.15, "P", 101325, "H", hout_a);
                //res.RHout = 0.0259124 - 0.0996818 * tao + 0.0934877 * (hout_a / 1000) + 0.0040018 * Math.Pow(tao, 2) - 0.0003662 * Math.Pow((hout_a / 1000), 2) - 0.0034077 * tao * (hout_a / 1000) - 1.76447 * Math.Pow(10, -5) * Math.Pow(tao, 3) - 2.74524 * Math.Pow(10, -6) * Math.Pow((hout_a / 1000), 3) + 2.99291 * Math.Pow(10, -5) * tao * Math.Pow((hout_a / 1000), 2) - 9.56644 * Math.Pow(10, -6) * Math.Pow(tao, 2) * (hout_a / 1000);

                double Tdp_out = humidairprop.Ts(hout_a);
                res.RHout = humidairprop.RHI(tao, Tdp_out);

                //if (tao >= 2 && tao <= 4)
                //{
                //    res.RHout = 0.0050520 - 0.0996818 * tao + 0.0934877 * (hout_a / 1000) + 0.0040018 * Math.Pow(tao, 2) - 0.0003662 * Math.Pow((hout_a / 1000), 2) - 0.0034077 * tao * (hout_a / 1000) - 1.76447 * Math.Pow(10, -5) * Math.Pow(tao, 3) - 2.74524 * Math.Pow(10, -6) * Math.Pow((hout_a / 1000), 3) + 2.99291 * Math.Pow(10, -5) * tao * Math.Pow((hout_a / 1000), 2) - 9.56644 * Math.Pow(10, -6) * Math.Pow(tao, 2) * (hout_a / 1000);

                //}
                //else
                //{
                //    res.RHout = 0.0085208 - 0.136810208 * tao + 0.107006008 * (hout_a / 1000) + 0.008282281 * Math.Pow(tao, 2) - 8.67968 * Math.Pow(10, -7) * Math.Pow((hout_a / 1000), 2) - 0.005873912 * tao * (hout_a / 1000) - 2.71106 * Math.Pow(10, -5) * Math.Pow(tao, 3) - 5.35767 * Math.Pow(10, -6) * Math.Pow((hout_a / 1000), 3) + 4.43316 * Math.Pow(10, -5) * tao * Math.Pow((hout_a / 1000), 2) - 2.92966 * Math.Pow(10, -5) * Math.Pow(tao, 2) * (hout_a / 1000);
                //    if (res.RHout < 0.7)
                //    {
                //        res.RHout = 0.0259124 - 0.099681859 * tao + 0.093487704 * (hout_a / 1000) + 0.004001845 * Math.Pow(tao, 2) - 0.000366259 * Math.Pow((hout_a / 1000), 2) - 0.003407719 * tao * (hout_a / 1000) - 1.76447 * Math.Pow(10, -5) * Math.Pow(tao, 3) - 2.79077 * Math.Pow(10, -6) * Math.Pow((hout_a / 1000), 3) + 2.99291 * Math.Pow(10, -5) * tao * Math.Pow((hout_a / 1000), 2) - 9.56903 * Math.Pow(10, -6) * Math.Pow(tao, 2) * (hout_a / 1000);
                //        if (res.RHout < 0.4)
                //        {
                //            res.RHout = -0.0021534 - 0.102137483 * tao + 0.10016414 * (hout_a / 1000) + 0.003999776 * Math.Pow(tao, 2) - 0.000766374 * Math.Pow((hout_a / 1000), 2) - 0.003170007 * tao * (hout_a / 1000) - 1.77801 * Math.Pow(10, -5) * Math.Pow(tao, 3) - 9.57411 * Math.Pow(10, -7) * Math.Pow((hout_a / 1000), 3) + 3.79403 * Math.Pow(10, -5) * tao * Math.Pow((hout_a / 1000), 2) - 1.7912 * Math.Pow(10, -5) * Math.Pow(tao, 2) * (hout_a / 1000);

                //        }
                //    }
                //}

                double f_dry = 0;

                double time06;
                double time07;
                double time08;
                if (hexType == 0 && tri < tai)
                {
                    double hao = 0;
                    //double Tdp0 = CoolProp.HAPropsSI("D", "T", tai + 273.15, "P", 101325, "R", RHi) - 273.15;
                    //double Tdp = -273.15 + 241.0212518 + 0.5718833 * tai + 84.99553 * RHi + 0.002691 * Math.Pow(tai, 2) - 95.003186 * Math.Pow(RHi, 2) + 0.7135779 * tai * RHi - 2.691 / Math.Pow(10, 5) * Math.Pow(tai, 3) + 42.58183 * Math.Pow(RHi, 3) - 0.3227474 * tai * Math.Pow(RHi, 2) - 0.000884612 * Math.Pow(tai, 2) * RHi;
                    double Tdp = humidairprop.Tdp(tai, RHi);

                    /*
                    double Ps = omega_in * 101325 / (0.622 + omega_in);
                    double Tdp1 = -39.957 - 1.8762 * Math.Log(Ps) + 1.1689 * Math.Pow(Math.Log(Ps), 2);
                    double Tdp3 = 8.22 + 12.4 * Math.Log(Ps) + 1.9 * Math.Pow(Math.Log(Ps), 2);
                    if (Tdp1 <= 0)
                    {
                        Tdp1 = -60.45 + 7.0322 * Math.Log(Ps) + 0.37 * Math.Pow(Math.Log(Ps), 2);
                    }
                    */
                    if (T_so_b > Tdp)
                    {
                        f_dry = 1.0;
                        Q = Q_dry;
                        Q_sensible = Q;
                        //omega_out = omega_in; 
                    }
                    else
                    {
                        double T_ac = 0;
                        double h_ac = 0;
                        if (T_so_a < Tdp)
                        {
                            f_dry = 0.0;
                            Q_dry = 0.0;
                            T_ac = tai;
                            h_ac = hai;
                        }
                        else
                        {
                            T_ac = Tdp + UA_i / UA_o * (Tdp - tri);
                            epsilon_dry = (tai - T_ac) / (tai - tri);
                            f_dry = -1.0 / Ntu_dry * Math.Log(1.0 - epsilon_dry);
                            //double h_ac0 = CoolProp.HAPropsSI("H", "T", T_ac + 273.15, "P", 101325, "W", omega_in);
                            //h_ac = -244.2924077 + 1135.8711 * T_ac + 10101.404 * RHi - 12.968219 * Math.Pow(T_ac, 2) - 11.356807 * Math.Pow(RHi, 2) + 357.25464 * T_ac * RHi + 0.3178346 * Math.Pow(T_ac, 3) - 0.0024329 * Math.Pow(RHi, 3) + 44.100799 * T_ac * Math.Pow(RHi, 2) + 50.31444812 * Math.Pow(T_ac, 2) * RHi;
                            h_ac = humidairprop.H(T_ac, "Omega", omega_in);
                            Q_dry = ma * cp_a * (tai - T_ac);
                        }

                        //double h1 = 58.732687 * Math.Pow(tri + 273.15 + 0.01, 2) - 30921.970577 * (tri + 273.15 + 0.01) + 4075493.951473;
                        double h1 = humidairprop.H(tri + 0.01, "R", 1);
                        //double h2 = 58.732687 * Math.Pow(tri + 273.15 - 0.01, 2) - 30921.970577 * (tri + 273.15 - 0.01) + 4075493.951473;
                        double h2 = humidairprop.H(tri - 0.01, "R", 1);
                        double c_s = (h1 - h2) / 0.02;
                        //double c_s0 = (CoolProp.HAPropsSI("H", "T", tri + 273.15 + 0.01, "P", 101325, "R", 1.0) - CoolProp.HAPropsSI("H", "T", tri + 273.15 - 0.01, "P", 101325, "R", 1.0)) / 0.02;

                        m = Math.Pow((2 * haw / k_fin / Fthickness), 0.5);
                        fai = (rf_r - 1) * (1 + (0.3 + Math.Pow((m * r_eta * (rf_r - r_eta) / 2.5), (1.5 - 1 / 12 * rf_r)) * (0.26 * Math.Pow(rf_r, 0.3) - 0.3)) * Math.Log(rf_r));
                        double eta_wet = Math.Tanh(m * r_eta * fai) / m / r_eta / fai * Math.Cos(0.1 * m * r_eta * fai);
                        //eta_a = (eta_0 * Aa_fin + Aa_tube) / (Aa_fin + Aa_tube);
                        UA_o = haw * (eta_wet * Aa_fin + Aa_tube);
                        NTU_o = UA_o / (ma * cp_a);
                        double UA_wet = 1 / (c_s / UA_i + cp_a / UA_o);
                        double Ntu_wet = UA_wet / ma;
                        double epsilon_wet = 1 - Math.Exp(-(1 - f_dry) * Ntu_wet);
                        //double h_s_s_o0 = CoolProp.HAPropsSI("H", "T", tri + 273.15, "P", 101325, "R", 1.0);
                        //double h_s_s_o = 58.732687 * Math.Pow(tri + 273.15, 2) - 30921.970577 * (tri + 273.15 - 0.01) + 4075493.951473;
                        double h_s_s_o = humidairprop.H(tri, "R", 1);
                        double Q_wet = epsilon_wet * ma * (h_ac - h_s_s_o);
                        Q = Q_wet + Q_dry;
                        hao = h_ac - Q_wet / ma;
                        double h_s_s_e = h_ac - (h_ac - hao) / (1 - Math.Exp(-(1 - f_dry) * NTU_o));
                        //double T_s_e0 = CoolProp.HAPropsSI("T","H",h_s_s_e,"P",101325,"R",1.0)-273.15;
                        //double T_s_e = -273.15 - 1.96 * Math.Pow(10, -3) * Math.Pow(h_s_s_e / 1000, 2) + 0.5357597 * h_s_s_e / 1000 + 268.871551;
                        double T_s_e = humidairprop.Ts(h_s_s_e);
                        tao = T_s_e + (T_ac - T_s_e) * Math.Exp(-(1 - f_dry) * NTU_o);
                        Q_sensible = ma * cp_a * (tai - tao);
                        hro = hri + Q / 1000 / mr;
                    }

                    hout_a = hai - Q / ma * Math.Pow(-1, hexType);

                    //double resRHout00 = CoolProp.HAPropsSI("R", "T", tao + 273.15, "P", 101325, "H", hout_a);
                    //res.RHout = 0.0259124 - 0.0996818 * tao + 0.0934877 * (hout_a / 1000) + 0.0040018 * Math.Pow(tao, 2) - 0.0003662 * Math.Pow((hout_a / 1000), 2) - 0.0034077 * tao * (hout_a / 1000) - 1.76447 * Math.Pow(10, -5) * Math.Pow(tao, 3) - 2.74524 * Math.Pow(10, -6) * Math.Pow((hout_a / 1000), 3) + 2.99291 * Math.Pow(10, -5) * tao * Math.Pow((hout_a / 1000), 2) - 9.56644 * Math.Pow(10, -6) * Math.Pow(tao, 2) * (hout_a / 1000);

                    Tdp_out = humidairprop.Ts(hout_a);
                    res.RHout = humidairprop.RHI(tao, Tdp_out);

                    //if (tao >= 2 && tao <= 4)
                    //{
                    //    res.RHout = 0.0050520 - 0.0996818 * tao + 0.0934877 * (hout_a / 1000) + 0.0040018 * Math.Pow(tao, 2) - 0.0003662 * Math.Pow((hout_a / 1000), 2) - 0.0034077 * tao * (hout_a / 1000) - 1.76447 * Math.Pow(10, -5) * Math.Pow(tao, 3) - 2.74524 * Math.Pow(10, -6) * Math.Pow((hout_a / 1000), 3) + 2.99291 * Math.Pow(10, -5) * tao * Math.Pow((hout_a / 1000), 2) - 9.56644 * Math.Pow(10, -6) * Math.Pow(tao, 2) * (hout_a / 1000);

                    //}
                    //else
                    //{
                    //    res.RHout = 0.0085208 - 0.136810208 * tao + 0.107006008 * (hout_a / 1000) + 0.008282281 * Math.Pow(tao, 2) - 8.67968 * Math.Pow(10, -7) * Math.Pow((hout_a / 1000), 2) - 0.005873912 * tao * (hout_a / 1000) - 2.71106 * Math.Pow(10, -5) * Math.Pow(tao, 3) - 5.35767 * Math.Pow(10, -6) * Math.Pow((hout_a / 1000), 3) + 4.43316 * Math.Pow(10, -5) * tao * Math.Pow((hout_a / 1000), 2) - 2.92966 * Math.Pow(10, -5) * Math.Pow(tao, 2) * (hout_a / 1000);
                    //    if (res.RHout < 0.7)
                    //    {
                    //        res.RHout = 0.0259124 - 0.099681859 * tao + 0.093487704 * (hout_a / 1000) + 0.004001845 * Math.Pow(tao, 2) - 0.000366259 * Math.Pow((hout_a / 1000), 2) - 0.003407719 * tao * (hout_a / 1000) - 1.76447 * Math.Pow(10, -5) * Math.Pow(tao, 3) - 2.79077 * Math.Pow(10, -6) * Math.Pow((hout_a / 1000), 3) + 2.99291 * Math.Pow(10, -5) * tao * Math.Pow((hout_a / 1000), 2) - 9.56903 * Math.Pow(10, -6) * Math.Pow(tao, 2) * (hout_a / 1000);
                    //        if (res.RHout < 0.4)
                    //        {
                    //            res.RHout = -0.0021534 - 0.102137483 * tao + 0.10016414 * (hout_a / 1000) + 0.003999776 * Math.Pow(tao, 2) - 0.000766374 * Math.Pow((hout_a / 1000), 2) - 0.003170007 * tao * (hout_a / 1000) - 1.77801 * Math.Pow(10, -5) * Math.Pow(tao, 3) - 9.57411 * Math.Pow(10, -7) * Math.Pow((hout_a / 1000), 3) + 3.79403 * Math.Pow(10, -5) * tao * Math.Pow((hout_a / 1000), 2) - 1.7912 * Math.Pow(10, -5) * Math.Pow(tao, 2) * (hout_a / 1000);

                    //        }
                    //    }
                    //}
                    if (res.RHout > 1)
                    {
                        res.RHout = 1;
                        //double tao0 = CoolProp.HAPropsSI("T", "H", hao, "P", 101325, "R", 1)-273.15;
                        //tao = -273.15 - 1.96 * Math.Pow(10, -3) * Math.Pow(hao / 1000, 2) + 0.5357597 * hao / 1000 + 268.871551;
                        tao = humidairprop.Ts(hao);
                        Q_sensible = ma * cp_a * (tai - tao);
                    }
                }
                res.R_1a = 1 / ((eta_0 * Aa_fin + Aa_tube) * ha);
                res.R_1r = 1 / (res.href * Ar);
                res.R_1 = res.R_1a + res.R_1r + r_metal;
                res.Q = Q / 1000;
                res.Tao = tao;
                res.hro = hro;
                res.Pro = pri - res.DP;
                if (res.Pro < 0) { res.Pro = -10000000; return res; }
                coolprop.update(input_pairs.HmassP_INPUTS, res.hro * 1000, res.Pro * 1000);
                res.Tro = coolprop.T() - 273.15;
                //res.Tro = CoolProp.PropsSI("T", "P", res.Pro * 1000, "H", res.hro * 1000, fluid);
                double rho_o = coolprop.rhomass();
                //double rho_o = CoolProp.PropsSI("D", "P", res.Pro * 1000, "H", res.hro * 1000, fluid);

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
