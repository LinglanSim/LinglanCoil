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
            double tri, double pri, double hri, double mr, double g, double ma, double ha,
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
            EnthalpyL = CoolProp.PropsSI("H", "T", tri + 273.15, "Q", 0, fluid) / 1000 - (fluid == "Water" ? 0 : 140);
            EnthalpyV = CoolProp.PropsSI("H", "T", tri + 273.15, "Q", 1, fluid) / 1000 - (fluid == "Water" ? 0 : 140);

            res.x_i = (hri - EnthalpyL) / (EnthalpyV - EnthalpyL);   //+ 140 for reference state, to be changed
            RefHTCandDPResult htc_dp = new RefHTCandDPResult();

            do
            {
                flag = false;
                htc_dp = RefrigerantHTCandDP.HTCandDP(fluid, dh, g, pri, res.x_i, l, q, zh, zdp, hexType);

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
                res.Q = epsilon * C_a * Math.Abs(tai - tri);
                res.Tao = tai + Math.Pow(-1, (hexType + 1)) * res.Q / C_a;
                res.hro = hri + Math.Pow(-1, hexType) * res.Q / mr;
                //0:evap, 1:cond
                res.x_o = (res.hro - EnthalpyL) / (EnthalpyV - EnthalpyL); //+ 139.17 for reference state, to be changed
                //res.DP = 0;
                res.Pro = pri - res.DP;
                res.Tro = CoolProp.PropsSI("T", "P", res.Pro * 1000, "H", (res.hro + (fluid == "Water" ? 0 : 140)) * 1000, fluid);
                double rho_o= CoolProp.PropsSI("D", "P", res.Pro * 1000, "H", (res.hro + (fluid == "Water" ? 0 : 140)) * 1000, fluid);
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
            double tri, double pri, double hri, double mr, double g, double ma, double ha,
            double eta_surface, double zh, double zdp, int hexType, double thickness, double conductivity)
        {
            double r_metal = thickness / conductivity / Ar;
            //double gg = 9.8;
            //double temperature;
            int phase1 = 1;
            //int phase2 = 2;
            double q_initial = 0.01;
            double q = q_initial;
            //double Ts_initial = tri - 5;
            //double Ts = Ts_initial;
            double err = 0.01;
            bool flag = false;
            int iter = 1;
            double k_fin = 237;
            double Fthickness = 0.095 * 0.001;

            double Pt = 1 * 25.4 * 0.001;
            //double Pr = 0.75 * 25.4 * 0.001;
            //double Di = 8.4074 * 0.001;//8 6.8944
            double Do = 10.0584 * 0.001;//8.4 7.35
            double h_fin = Pt - Do;

            CalcResult res = new CalcResult();
            //res.Tao[0] = new double();
            double EnthalpyL, EnthalpyV;
            //recalc tri to make sure it is 2ph, ruhao, 20180225 
            double temperature = CoolProp.PropsSI("T", "P", pri * 1000, "Q", 0, fluid) - 273.15;
            EnthalpyL = CoolProp.PropsSI("H", "T", tri + 273.15, "Q", 0, fluid) / 1000 - (fluid == "Water" ? 0 : 140);
            EnthalpyV = CoolProp.PropsSI("H", "T", tri + 273.15, "Q", 1, fluid) / 1000 - (fluid == "Water" ? 0 : 140);

            res.x_i = (hri - EnthalpyL) / (EnthalpyV - EnthalpyL);   //+ 140 for reference state, to be changed

            RefHTCandDPResult htc_dp = new RefHTCandDPResult();

            //重新定义
            //r = Refrigerant.TPFLSH(fluid, composition, tri + 273.15, fluid == "Water" ? Pwater : pri);
            //double wm = Refrigerant.WM(fluid, composition).Wm;
            //double cp_r = r.cp / wm * 1000;
            //double cp_r = 0.5;//暂时可以不管,两相没有用到
            //double d = 1.0;//绝对含湿量
            //double d = ;//绝对含湿量
            //double b_1 = 0.0142498; double b_2 = 0; double b_3 = 5.1055414; double b_4 = 2.912 * Math.Pow(10, -7); double b_5 = 0;
            //double b_6 = 0.0007217; double b_7 = 0; double b_8 = 0; double b_9 = 0.0208652; double b_10 = 0.023634259;
            //double omega_in = b_1 + b_2 * Tin_a + b_3 * RHin_a + b_4 * Math.Pow(Tin_a, 2) + b_5 * Math.Pow(RHin_a, 2) + b_6 * Tin_a * RHin_a + b_7 * Math.Pow(Tin_a, 3) + b_8 * Math.Pow(RHin_a, 3) + b_9 * Tin_a * Math.Pow(RHin_a, 2) + b_10 * Math.Pow(Tin_a, 2) * RHin_a;  
            double RHin_a = 0.4696;//需要输入，后期可以改成湿球温度0.4696(27,19)

            double Tin_a = tai;
            //重新定义
            //Calculate the dewpoint (amongst others)
            //double omega_in = 0.1;//=HAProps('W','T',Tin_a,'P',pin_a,'R',RHin_a)//kg/kg
            double b_1 = 0.0142543; double b_2 = -0.0008854; double b_3 = 5.1791661; double b_4 = -0.0002376; double b_5 = -0.0017752;
            double b_6 = 0.0007217; double b_7 = 8.172 * Math.Pow(10, -6); double b_8 = -0.0029446; double b_9 = 0.0208344; double b_10 = 0.023259933;
            double omega_in = b_1 + b_2 * Tin_a + b_3 * RHin_a + b_4 * Math.Pow(Tin_a, 2) + b_5 * Math.Pow(RHin_a, 2) + b_6 * Tin_a * RHin_a + b_7 * Math.Pow(Tin_a, 3) + b_8 * Math.Pow(RHin_a, 3) + b_9 * Tin_a * Math.Pow(RHin_a, 2) + b_10 * Math.Pow(Tin_a, 2) * RHin_a;
            double Tdp = 10;//=HAProps('D','T', Tin_a,'P',pin_a,'W',omega_in)

            omega_in = omega_in / 1000;
            double cp_a = 1.005 + omega_in * 1.86;//1.005 + d * 1.86; //keep it for now as a constant，1.027

            if (Tin_a < 20)
            {
                //Tdp = 10;
                double d_1 = -26.9340838; double d_2 = 0.5144618; double d_3 = 53.671294; double d_4 = -0.0010925; double d_5 = -52.737287;
                double d_6 = 1.6405722; double d_7 = 0.0001091; double d_8 = 22.0578; double d_9 = -0.4717929; double d_10 = -0.028161674;
                Tdp = d_1 + d_2 * Tin_a + d_3 * RHin_a + d_4 * Math.Pow(Tin_a, 2) + d_5 * Math.Pow(RHin_a, 2) + d_6 * Tin_a * RHin_a + d_7 * Math.Pow(Tin_a, 3) + d_8 * Math.Pow(RHin_a, 3) + d_9 * Tin_a * Math.Pow(RHin_a, 2) + d_10 * Math.Pow(Tin_a, 2) * RHin_a;

            }
            else
            {
                //Tdp = 10;
                double e_1 = -25.6011797; double e_2 = 0.3826516; double e_3 = 56.876903; double e_4 = -0.0010384; double e_5 = -65.622156;
                double e_6 = 1.6443265; double e_7 = 0.0001627; double e_8 = 29.084734; double e_9 = -0.4697309; double e_10 = -0.016729144;
                Tdp = e_1 + e_2 * Tin_a + e_3 * RHin_a + e_4 * Math.Pow(Tin_a, 2) + e_5 * Math.Pow(RHin_a, 2) + e_6 * Tin_a * RHin_a + e_7 * Math.Pow(Tin_a, 3) + e_8 * Math.Pow(RHin_a, 3) + e_9 * Tin_a * Math.Pow(RHin_a, 2) + e_10 * Math.Pow(Tin_a, 2) * RHin_a;

            }


            //double hin_a = 100;//=HAProps('H','T',Tin_a,'P',pin_a,'W',omega_in)*1000/J/Kg
            double a_1 = -0.1232369; double a_2 = 0.611334891; double a_3 = 31.34295802; double a_4 = 0.025365477; double a_5 = -29.94267762;
            double a_6 = -0.456313635; double a_7 = -0.000454003; double a_8 = 14.97634549; double a_9 = 0.290827526; double a_10 = 0.065911723;
            double hin_a = a_1 + a_2 * Tin_a + a_3 * RHin_a + a_4 * Math.Pow(Tin_a, 2) + a_5 * Math.Pow(RHin_a, 2) + a_6 * Tin_a * RHin_a + a_7 * Math.Pow(Tin_a, 3) + a_8 * Math.Pow(RHin_a, 3) + a_9 * Tin_a * Math.Pow(RHin_a, 2) + a_10 * Math.Pow(Tin_a, 2) * RHin_a;

            do
            {
                flag = false;
                htc_dp = RefrigerantHTCandDP.HTCandDP(fluid, dh, g, pri, res.x_i, l, q, zh, zdp, hexType);

                res.href = htc_dp.Href;
                res.DP = htc_dp.DPref;

                //Intenal UA between fluid flow and outside surface (neglecting tube conduction)
                double UA_i = res.href * Ar;
                //External UA between wall and free stream
                double UA_o = (eta_surface * Aa_fin + Aa_tube) * ha;
                //Internal Ntu两相换热没有用到
                //double Ntu_i = UA_i / (mr * cp_r);
                //External Ntu (multiplied by eta_a since surface is finned and has lower effectiveness)
                double Ntu_o = UA_o / (ma * cp_a * 1000);

                //Two-Phase analysis
                res.R_1a = 1 / ((eta_surface * Aa_fin + Aa_tube) * ha);
                res.R_1r = 1 / (res.href * Ar);
                r_metal = 0;//set 0 for coil resistance for now
                res.R_1 = res.R_1a + res.R_1r + r_metal;
                double UA = 1 / res.R_1;//overall heat transfer coefficient
                double Ntu_dry = UA / (ma * cp_a * 1000);//Number of transfer units*1000
                double epsilon_dry = 1 - Math.Exp(-Ntu_dry);//since Cr = 0,
                double Q_dry = epsilon_dry * ma * cp_a * Math.Abs(tai - tri);
                res.Tao = tai + Math.Pow(-1, (hexType + 1)) * res.Q / (ma * cp_a);//outlet temperature, dry fin

                double T_so_a = (UA_o * tai + UA_i * tri) / (UA_o + UA_i);//inlet surface temperature
                double T_so_b = (UA_o * res.Tao + UA_i * tri) / (UA_o + UA_i);//outlet surface temperature

                double f_dry = 0.0;
                double Q_sensible = 0.0;
                double hout_a = 0.0;
                double omega_out = 0.0;
                double T_ac = 0.0;
                double h_ac = 0.0;
                double c_s = 0.0;
                double cs_cp = 0.0;
                double eta_a_wet = 0.0;
                double UA_wet = 0.0;
                double Ntu_wet = 0.0;
                double epsilon_wet = 0.0;
                double h_s_s_o = 0.0;
                double Q_wet = 0.0;
                //double Q = 0.0;
                double h_s_s_e = 0.0;
                double T_s_e = 0.0;
                double Tout_a = 0.0;
                if (T_so_b > Tdp)
                {
                    //All dry, since surface at outlet dry
                    f_dry = 1.0;
                    res.Q = Q_dry;//W
                    Q_sensible = res.Q;//W
                    hout_a = hin_a - res.Q / ma;//J/kg
                    //Air outlet humidity ratio
                    omega_out = omega_in;//kg/kg
                }

                else
                {
                    if (T_so_a < Tdp)
                    {
                        //All wet, since surface at inlet wet
                        f_dry = 0.0;
                        Q_dry = 0.0;
                        T_ac = tai;//temp at onset of the wetted wall
                        h_ac = hin_a;//enthalpy at onset of wetted surface
                    }
                    else
                    {
                        //Partially wet and dry

                        //Air temperature at the interface between wet and dry surface
                        //Based on equating heat fluxes at the wall which is at dew point UA_i * (Tw - Ti) = UA_o * (To - Tw)
                        T_ac = Tdp + UA_i / UA_o * (Tdp - tri);
                        //Dry effectiveness (minimum capacitance on the air side by definition)
                        epsilon_dry = (tai - T_ac) / (tai - tri);
                        //Dry fraction found by solving epsilon = 1- exp(-f_dry*Ntu) for known epsilon from above equation
                        f_dry = -1.0 / Ntu_dry * Math.Log(1.0 - epsilon_dry);//possible Math.Ln
                        //Enthalpy, using air humidity at the interface between wet and dry surfaces, which is same humidity ratio as inlet
                        //h_ac = 1.0;//= HAProps('H','T',T_ac,'P',pin_a,'W',omega_in) * 1000 J/kg
                        double c_1 = -0.1232369; double c_2 = 0.611334891; double c_3 = 31.34295802; double c_4 = 0.025365477; double c_5 = -29.94267762;
                        double c_6 = -0.456313635; double c_7 = -0.000454003; double c_8 = 14.97634549; double c_9 = 0.290827526; double c_10 = 0.065911723;
                        h_ac = c_1 + c_2 * T_ac + c_3 * RHin_a + c_4 * Math.Pow(T_ac, 2) + c_5 * Math.Pow(RHin_a, 2) + c_6 * T_ac * RHin_a + c_7 * Math.Pow(T_ac, 3) + c_8 * Math.Pow(RHin_a, 3) + c_9 * T_ac * Math.Pow(RHin_a, 2) + c_10 * Math.Pow(T_ac, 2) * RHin_a;


                        //Dry heat transfer
                        Q_dry = ma * cp_a * (tai - T_ac);

                    }

                    //Saturation specific heat at mean water temp
                    double domega_in = b_2 + 2 * b_4 * Tin_a + b_6 * RHin_a + 3 * b_7 * Math.Pow(Tin_a, 2) + b_9 * Math.Pow(RHin_a, 2) + 2 * b_10 * Tin_a * RHin_a;
                    domega_in = domega_in / 1000;
                    c_s = 1.005 + omega_in * 1.86 + (2501 + 1.86 * Tin_a) * domega_in; //cair_sat(Tin_r)*1000 J/kg*K
                    //Find new, effective fin efficiency since cs/cp is changed from wetting
                    //Ratio of specific heats
                    cs_cp = c_s / cp_a;
                    //calculate eta_a_wet
                    double m = Math.Sqrt(2 * ha * cs_cp / (k_fin * Fthickness));
                    eta_a_wet = Math.Tanh(m * h_fin) / (m * h_fin);
                    UA_o = eta_a_wet * ha * (Aa_fin + Aa_tube);
                    Ntu_o = UA_o / (ma * cp_a);

                    //Wet analysis overall Ntu for two-phase refrigerant
                    //Minimum capacitance rate is by definition on the air side
                    //Ntu_wet is the NTU if the entire two-phase region were to be wetted
                    UA_wet = 1 / (c_s / UA_i + cp_a / UA_o);
                    Ntu_wet = UA_wet / ma;
                    //Wet effectiveness
                    epsilon_wet = 1 - Math.Exp(-(1 - f_dry) * Ntu_wet);
                    //Air saturated at refrigerant saturation temp J/kg
                    //h_s_s_o = 1.0;//HAProps('H','T',Tin_r,'P',pin_a,'R',1.0)*1000 kJ/kg
                    //double hri = Refrigerant.TPFLSH(fluid, composition, T_exv + 273.15, P_exv).h / wm - (fluid == "Water" ? 0 : 140);//?
                    //h_s_s_o = 1.0;//HAProps('H','T',Tin_r,'P',pin_a,'R',1.0)*1000 kJ/kg
                    h_s_s_o = 0.0709 * Math.Pow(tri, 2) + 0.6509 * tri + 16.033;
                    //Wet heat transfer W
                    Q_wet = epsilon_wet * ma * (h_ac - h_s_s_o);
                    //Total heat transfer
                    res.Q = Q_dry + Q_wet;
                    //Air exit enthalpy
                    hout_a = h_ac - Q_wet / ma;
                    //Saturated air temp at effective surface temp
                    h_s_s_e = h_ac - (h_ac - hout_a) / (1 - Math.Exp(-(1 - f_dry) * Ntu_o));
                    //Effective surface temperature K
                    //T_s_e = 1.0;//HAProps('T','H',h_s_s_e/1000.0,'P',pin_a,'R',1.0);
                    T_s_e = -0.0016 * Math.Pow(h_s_s_e, 2) + 0.4914 * h_s_s_e - 2.9124;
                    //Outlet dry-bulb temp K
                    Tout_a = T_s_e + (T_ac - T_s_e) * Math.Exp(-(1 - f_dry) * Ntu_o);
                    //Sensible heat transfer rate kW
                    Q_sensible = ma * cp_a * (tai - Tout_a);
                    //Outlet is saturated vapor


                }

                res.hro = hri + Math.Pow(-1, hexType) * res.Q / mr;
                //**********//0:evap, 1:cond*********//

                res.x_o = (res.hro - EnthalpyL) / (EnthalpyV - EnthalpyL); //+ 139.17 for reference state, to be changed
                //*********//res.DP = 0;*********//
                res.Pro = pri - res.DP;
                //res.Tro = Refrigerant.PHFLSH(fluid, composition, res.Pro, (res.hro + (fluid == "Water" ? 0 : 140)) * r.Wm).t; // 
                res.Tro = CoolProp.PropsSI("T", "P", res.Pro * 1000, "H", (res.hro + (fluid == "Water" ? 0 : 140)), fluid);
                double rho_o = CoolProp.PropsSI("D", "T", res.Tro, "H", res.hro * 1000, fluid);
                //double rho_o = Refrigerant.TQFLSH(fluid, composition, res.Tro, res.x_o).D * r.Wm;
                //*********************//

                //*************************//
                res.Tro = res.Tro - 273.15;
                res.Vel_r = g / rho_o;

                //res.Q是整个的Q，而不是Q_sensible
                if (Math.Abs(q - res.Q) / res.Q > err)
                {
                    q = res.Q;
                    flag = true;
                }
                iter++;

            } while (flag && iter < 100);


            //if (iter>=100)
            //{
            //throw new Exception("iter for href > 1000.");
            //}
            return res;

        }


    }
}
