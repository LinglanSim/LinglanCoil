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
            double RHi, double tri, double pri, double hri, double mr, double g, double ma, double ha,
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
                res.Tro = tri + Math.Pow(-1, hexType) * epsilon * (tai - tri) * Math.Pow(-1, hexType);//Math.Abs(tai - tri);
                res.Tao = tai + Math.Pow(-1, (hexType + 1)) * C_r * ((res.Tro - tri) / C_a) * Math.Pow(-1, hexType);//Math.Abs(res.Tro - tri)
            }
            else
            {
                res.Tao = tai + Math.Pow(-1, (hexType + 1)) * epsilon * (tai - tri) * Math.Pow(-1, hexType);//Math.Abs(tai - tri)
                res.Tro = tri + Math.Pow(-1, hexType) * C_a * ((tai - res.Tao) / C_r) * Math.Pow(-1, hexType);//(Math.Abs(tai - res.Tao) / C_r)
            }
            double f_sp = RefrigerantSPDP.ff_Friction(Re_r);
            res.DP = zdp * f_sp * l / dh * Math.Pow(g, 2.0) / rho_r / 2000;
            res.Pro = fluid == "Water" ? pri : pri - res.DP;
            res.hro = hri + Math.Pow(-1, hexType) * res.Q / mr;
            res.RHout = 1.1 * RHi;
            return res; 

        }
        public static CalcResult ElementCalc1(string fluid, double dh, double l, double Aa_fin, double Aa_tube, double A_r_cs, double Ar, double tai,
            double RHi, double tri, double pri, double hri, double mr, double g, double ma, double ha,
            double eta_surface, double zh, double zdp, int hexType, double thickness, double conductivity, double Pwater)
        {
            double r_metal = thickness / conductivity / Ar;
            double gg = 9.8;
            CalcResult res = new CalcResult();

            double mu_r = CoolProp.PropsSI("V", "T", tri + 273.15, "P", (fluid == "Water" ? Pwater : pri) * 1000, fluid);
            double k_r = CoolProp.PropsSI("L", "T", tri + 273.15, "P", (fluid == "Water" ? Pwater : pri) * 1000, fluid);
            double rho_r = CoolProp.PropsSI("D", "T", tri + 273.15, "P", (fluid == "Water" ? Pwater : pri) * 1000, fluid);
            double cp_r = CoolProp.PropsSI("C", "T", tri + 273.15, "P", (fluid == "Water" ? Pwater : pri) * 1000, fluid);

            double Pr_r = cp_r * mu_r / k_r;//普朗特数的计算有了
            res.Vel_r = g / rho_r;
            double Re_r = rho_r * res.Vel_r * dh / mu_r;
            double fh = RefrigerantSPHTC.ff_CHURCHILL(Re_r);
            double Nusselt = RefrigerantSPHTC.NU_GNIELINSKI(Re_r, Pr_r, fh);

            res.href = Nusselt * k_r / dh * zh;
            //res.href = 12465;
            double cp_a = 1.0;
            cp_a = (hexType == 0 ? 1.029617 : 1.026082);//1.026077/1.07是析湿的换热加强系数

            double RHo = 0.1;
            double Tin_a = tai;
            //double Tin_a = 25;
            double RHin_a = RHi; //0.4696;
            //double RHin_a = 0.4;
            //**************************************//
            //干工况计算
            double C_r = mr * cp_r / 1000;
            double C_a = ma * cp_a;//cp_a引用干空气热容可能有问题
            //在这里添加空气侧换热系数和翅片效率计算式，采用调用的方式，重点在于调用逻辑，调用语句的编写

            double UA_i = res.href * Ar;
            //External UA between wall and free stream
            double k_fin = 237;
            double Fthickness = 0.095 * 0.001;
            double Pt = 1 * 14.5 * 0.001;
            //double Pr = 0.75 * 25.4 * 0.001;
            //double Di = 8.4074 * 0.001;//8 6.8944
            double Do = 5.25 * 0.001;//8.4 7.35
            double h_fin = (Pt - Do) / 2;
            double m = Math.Sqrt(2 * ha / (k_fin * Fthickness));
            double eta_a_wet1 = Math.Tanh(m * h_fin) / (m * h_fin);
            //eta_surface也不对，要用输入来算
            double UA_o = (eta_surface * Aa_fin + Aa_tube) * ha;
            //Internal Ntu两相换热没有用到
            double Ntu_i = UA_i / (mr * cp_r);
            //External Ntu (multiplied by eta_a since surface is finned and has lower effectiveness)
            double Ntu_o = UA_o / (ma * cp_a * 1000);
            //Overall UA这里可以添加管壁热阻************************//
            double UA = 1 / (1 / (UA_i) + 1 / (UA_o));
            //Min and max capacitance rates W/K
            double C_min = Math.Min(C_a, C_r);
            double C_max = Math.Max(C_a, C_r);
            //Capacitance rate ratio
            double C_ratio = C_min / C_max;
            //Ntu overall
            double Ntu_dry = UA / (C_min * 1000);
            //double Ntu_dry = UA / (C_min );

            //Counterflow effectiveness
            //方程的数学内含规律决定了物理量之间的依变关系

            //纯逆流计算
            double epsilon_dry = (1 - Math.Exp(-Ntu_dry * (1 - C_ratio))) / (1 - C_ratio * Math.Exp(-Ntu_dry * (1 - C_ratio)));

            //交叉流计算
            //double epsilon_dry2 = 1 - Math.Exp(Math.Pow(C_ratio, -1.0) * Math.Pow(Ntu_dry, 0.22)
            //* (Math.Exp(-C_ratio * Math.Pow(Ntu_dry, 0.78)) - 1));
            //**************************************//
            //干工况输出
            //Dry heat transfer W
            //double Q_dry = epsilon_dry * C_min * (tai - tri);
            double Q_dry = epsilon_dry * C_min * Math.Abs(tri - tai);
            //Dry-analysis air outlet temp K
            //double Tout_a_dry = tai - Q_dry / C_a;
            double Tout_a_dry = tai + Math.Pow(-1, (hexType + 1)) * epsilon_dry * Math.Abs(tai - tri);
            //Dry-analysis outlet temp K
            //double Tout_r = tri + Q_dry / C_r;
            double Tout_r = tri + Math.Pow(-1, hexType) * Q_dry / C_r;
            //Dry-analysis air outlet enthalpy from energy balance J/kg
            //double hin_a = 1.0;
            double a_1 = -0.1232369; double a_2 = 0.611334891; double a_3 = 31.34295802; double a_4 = 0.025365477; double a_5 = -29.94267762;
            double a_6 = -0.456313635; double a_7 = -0.000454003; double a_8 = 14.97634549; double a_9 = 0.290827526; double a_10 = 0.065911723;
            double hin_a = a_1 + a_2 * Tin_a + a_3 * RHin_a + a_4 * Math.Pow(Tin_a, 2) + a_5 * Math.Pow(RHin_a, 2) + a_6 * Tin_a * RHin_a + a_7 * Math.Pow(Tin_a, 3) + a_8 * Math.Pow(RHin_a, 3) + a_9 * Tin_a * Math.Pow(RHin_a, 2) + a_10 * Math.Pow(Tin_a, 2) * RHin_a;

            double hout_a = hin_a - Q_dry / ma;
            //Dry-analysis surface outlet temp K
            double Tout_s = (UA_o * Tout_a_dry + UA_i * tri) / (UA_o + UA_i);
            //Dry-analysis surface inlet temp K
            double Tin_s = (UA_o * tai + UA_i * Tout_r) / (UA_o + UA_i);
            //Dry-analysis outlet refirigerant temp K
            double Tout_r_dry = Tout_r;
            //Dry fraction
            double f_dry = 1.0;
            //double omega_in = 1.0;
            double b_1 = 0.0142543; double b_2 = -0.0008854; double b_3 = 5.1791661; double b_4 = -0.0002376; double b_5 = -0.0017752;
            double b_6 = 0.0007217; double b_7 = 8.172 * Math.Pow(10, -6); double b_8 = -0.0029446; double b_9 = 0.0208344; double b_10 = 0.023259933;
            double omega_in = b_1 + b_2 * Tin_a + b_3 * RHin_a + b_4 * Math.Pow(Tin_a, 2) + b_5 * Math.Pow(RHin_a, 2) + b_6 * Tin_a * RHin_a + b_7 * Math.Pow(Tin_a, 3) + b_8 * Math.Pow(RHin_a, 3) + b_9 * Tin_a * Math.Pow(RHin_a, 2) + b_10 * Math.Pow(Tin_a, 2) * RHin_a;
            omega_in = omega_in / 1000;
            //Air outlet humidity ratio
            double omega_out = omega_in;
            //rh=0.5;出口相对湿度计算
            double c_1 = 0.1102508; double c_2 = -0.018691136; double c_3 = 0.220027121; double c_4 = 0.0009904; double c_5 = 1.313 * Math.Pow(10, -6);
            double c_6 = -0.010546516; double c_7 = -1.649 * Math.Pow(10, -5); double c_8 = 6.81712 * Math.Pow(10, -7); double c_9 = -3.49182 * Math.Pow(10, -6); double c_10 = 0.000150734;


            RHo = c_1 + c_2 * Tout_a_dry + c_3 * (omega_out * 1000) + c_4 * Math.Pow(Tout_a_dry, 2) + c_5 * Math.Pow((omega_out * 1000), 2) + c_6 * Tout_a_dry * (omega_out * 1000) + c_7 * Math.Pow(Tout_a_dry, 3) + c_8 * Math.Pow((omega_out * 1000), 3) + c_9 * Tout_a_dry * Math.Pow((omega_out * 1000), 2) + c_10 * Math.Pow(Tout_a_dry, 2) * (omega_out * 1000);
            //通过数据输入和输出，检测到RHo方程有错误。错误原因在于系数符号使用错误 
            //RHo(40.169)数据范围10-30，不包括40.169，偏差大（45.65-32.2）/32.2=41.8%，需要重新拟合经验关联式
            //If inlet surface temp below dewpoint, whole surface is wetted
            //if (Tin_s < Tdp)
            //isFullyWet = true;
            //else
            //isFullyWet = false;
            //omega_in = 10.453 / 1000;
            double Tdp = 10.0;
            //Tdp = -0.0286 * Math.Pow(omega_in, 2) + 1.986 * omega_in + 3.13;//计算很不准确

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


            double A_a = Aa_fin + Aa_tube;
            //**************************************//
            //全湿工况计算
            double h_s_w_i = 0;
            double Tout_a = 1.0;
            double Q = 1.0;
            double Q_sensible = 1.0;



            if (Tout_s < Tdp)
            //There is some wetting, either the coil is fully wetted or partially wetted
            //Loop to get the correct c_s
            //Start with the inlet temp as the outlet temp
            /*x1 = Tin_r + 1;//Lowest possible outlet temp
            double x2 = Tin_a - 1;//Highest possible outlet temp
            double eps = Math.Pow(10, -8);
            double iter = 1;
            double change = 999;

            while ((iter <= 3 & change > eps) && iter < 100)
                if (iter == 1)
                    Tout_r = x1;
            if (iter > 1)
                Tout_r = x2;*/
            {

                //double x1 = tri + 1;//Lowest possible outlet temp
                //double x2 = Tin_a - 1;//Highest possible outlet temp
                double eps = Math.Pow(10, -8);
                double iter = 1;
                //double change = 999;
                double y1 = 1; double y2 = 1; double x3 = 1; //double change = 1;
                double Q_wet = 1.0;
                double Tout_s1 = 1.0;
                double errorToutr = 1.0;

                ma = ma / (1.0 + omega_in);//ma这样修正不对
                //ma = ma;
                //double Tout_r_start = Tout_r;
                //Saturated air enthalpy at the inlet water temp J/kg

                //********************************************//
                h_s_w_i = 0.0709 * Math.Pow((tri + 0.0), 2) + 0.6509 * (tri + 0.0) + 16.033;//HAProps('H','T',Tin_r,'P',pin_a,'R',1.0)*1000;J/kg
                //********************************************//

                //double c_1 = -0.1232369; double c_2 = 0.611334891; double c_3 = 31.34295802; double c_4 = 0.025365477; double c_5 = -29.94267762;
                //double c_6 = -0.456313635; double c_7 = -0.000454003; double c_8 = 14.97634549; double c_9 = 0.290827526; double c_10 = 0.065911723;
                //h_s_w_i = c_1 + c_2 * T_ac + c_3 * RHin_a + c_4 * Math.Pow(T_ac, 2) + c_5 * Math.Pow(RHin_a, 2) + c_6 * T_ac * RHin_a + c_7 * Math.Pow(T_ac, 3) + c_8 * Math.Pow(RHin_a, 3) + c_9 * T_ac * Math.Pow(RHin_a, 2) + c_10 * Math.Pow(T_ac, 2) * RHin_a;  

                //Saturation specific heat at mean water temp J/kg
                double c_s = 2.0;//cair_sat((Tin_r + Tout_r) / 2) * 1000;
                //double domega_in = b_2 + 2 * b_4 * Tin_a + b_6 * RHin_a + 3 * b_7 * Math.Pow(Tin_a, 2) + b_9 * Math.Pow(RHin_a, 2) + 2 * b_10 * Tin_a * RHin_a;
                //c_s = 1.005 + omega_in * 1.86 + (2501 + 1.86 * Tin_a) * domega_in; //cair_sat(Tin_r)*1000 J/kg*K
                //c_s = c_s / 1000;
                c_s = 0.00005 * Math.Pow(Tin_a, 2) - 0.00029 * Tin_a + 1.01882;
                //c_s = 0.00005 * Math.Pow(tri, 2) - 0.00029 * tri + 1.01882;
                //c_s1527,cp_da1.024,错了
                double cp_da = 1.0;
                cp_da = 1.005 + omega_in * 1.86;//1.005 + d * 1.86; //keep it for now as a constant，1.027
                //Ratio of specific heats
                double cs_cp = c_s / cp_da;
                //Find new, effective fin efficiency since cs/cp is changed from wetting
                //double ppp = 1;//WavyLouveredFins(DWS.Fins)
                k_fin = 237;//237
                Fthickness = 0.095 * 0.001;
                Pt = 1 * 21 * 0.001;
                //double Pt = 14.5 * 0.001;
                //double Pr = 12.56 * 0.001;
                //double Pr = 0.75 * 25.4 * 0.001;
                //double Di = 8.4074 * 0.001;//8 6.8944
                Do = 7.35 * 0.001;//8.4 7.35
                //double Do = 5.25 * 0.001;//8.4
                h_fin = (Pt - Do) / 2;
                m = Math.Sqrt(2 * ha * cs_cp / (k_fin * Fthickness));
                double eta_a_wet = Math.Tanh(m * h_fin) / (m * h_fin);

                //Effective humid air mass flow ratio
                double m_start = (ma * c_s) / (mr * cp_r / 1000);
                //double eta_a1 = eta_a_wet;
                double eta_a = 1 - Aa_fin / A_a * (1 - eta_a_wet);
                //compute the new Ntu_owet
                //double Ntu_owet = eta_a * ha * A_a / (ma * c_s * 1000);
                double Ntu_owet = eta_a * ha * A_a / (ma * cp_da * 1000);
                //double Ntu_owet = eta_a * ha * A_a / (ma * cp_da * 1000);
                double m_star = Math.Min(cp_r * mr / 1000, ma * c_s) / Math.Max(cp_r * mr / 1000, ma * c_s);
                double mdot_min = Math.Min(cp_r * mr / (1000 * c_s), ma);
                //Wet-analysis overall Ntu
                //Ntu_owet\Ntu_i\Ntu_o都是按照干工况计算的，可能 不对？
                double Ntu_wet = 1.0;// Ntu_o / (1 + m_star * (Ntu_owet / Ntu_i));
                //double Ntu_wet1 = 1.0;

                if (cp_r * mr / 1000 > c_s * ma)
                {
                    Ntu_wet = Ntu_o / (1 + m_star * (Ntu_owet / Ntu_i));
                    //Ntu_wet = Ntu_o / (1 + m_star * (Ntu_o / Ntu_i));
                    //管壁热阻可以和Ntu_i弄到一起，稍有麻烦
                    //Ntu_wet = Ntu_owet / (1 + m_star * (Ntu_owet / Ntu_i));
                }

                else
                {
                    Ntu_wet = Ntu_i / (1 + m_star * (Ntu_i / Ntu_owet));
                }

                //m_star = 0.035652209385470639;
                //Ntu_wet = 0.86982344382357357; //修改后epsilon_wet = 0.57666264924152688;Q_wet = 0.06335494410307306;
                //Ntu_wet = 0.87727834647216352;//修改前epsilon_wet = 0.57974227442820847;Q_wet = 0.067057695299025735; Q_wet的差别是两次焓的差别引起
                //Counterflow effectiveness for wet analysis
                //纯逆流计算
                //double epsilon_wet = (1 - Math.Exp(-Ntu_wet * (1 - m_star))) / (1 - m_star * Math.Exp(-Ntu_wet * (1 - m_star)));
                //excel处理的是逆流，这里处理成逆流是正确的
                double epsilon_wet = 1 - Math.Exp(Math.Pow(m_star, -1.0) * Math.Pow(Ntu_wet, 0.22)
                * (Math.Exp(-m_star * Math.Pow(Ntu_wet, 0.78)) - 1));

                //Wet-analysis heat transfer rate质量要变成每kg干空气
                Q_wet = epsilon_wet * mdot_min * (hin_a - h_s_w_i);
                //Air outlet enthalpy J/kg
                hout_a = hin_a - Q_wet / ma;
                //Water outlet temp K
                Tout_r = tri + ma / (mr * cp_r / 1000) * (hin_a - hout_a);
                //Water outlet saturated surface enthalpy J/k
                //*************边代码要求做细致，做精致***********
                double h_s_w_o = 1.0;//HAProps('H','T',Tout_r,'P',pin_a,'R',1.0)*1000 J/kg
                //*****************先这样放着***************************//
                h_s_w_o = 0.0709 * Math.Pow((Tout_r + 0.0), 2) + 0.6509 * (Tout_r + 0.0) + 16.033;
                //Local UA and c_s
                //*********这里有简化处理，可能不正确
                //double cair_sat = 1.0;//cair_sat((tai + Tout_r);
                //double UA_star = 1 / (cp_da / eta_a / ha / A_a + c_s / 2.0 * 1000 / res.href / Ar);
                //********************************************//
                double UA_star = 1 / (cp_da / eta_a / ha / A_a + c_s / res.href / Ar);//1 / res.href * Ar; eta_a * ha * A_a
                //Wet-analysis surface temp K
                //从这里看，以制冷剂温度计算壁面空气饱和焓是正确的
                //Tin_s = Tout_r + UA_star / res.href / Ar * (hin_a - h_s_w_o);
                Tin_s = Tout_r + ma / (mr * cp_r / 1000) * Ntu_wet / Ntu_i * (hin_a - h_s_w_o);
                //*****************************************************//
                //Wet-analysis saturation enthalpy
                double h_s_s_e = hin_a + (hout_a - hin_a) / (1 - Math.Exp(-Ntu_owet));//-Ntu_owet
                //Surface effective temp K
                double T_s_e = 1.0;//HAProps('T','H',h_s_s_e/1000,'P',pin_a,'R',1.0)
                T_s_e = -0.0016 * Math.Pow(h_s_s_e, 2) + 0.4914 * h_s_s_e - 2.9124;
                //Air outlet temp based on effective temp K
                Tout_a = T_s_e + (tai - T_s_e) * Math.Exp(-Ntu_o);
                //Sensible heat transfer rate W
                Q_sensible = ma * cp_a * (tai - Tout_a);
                //RHo = 0.7;
                //相对湿度计算
                if (Tout_a < 20)
                {
                    //Tdp = 10;
                    double f_1 = -0.6625838; double f_2 = 0.0286184; double f_3 = 0.1029451; double f_4 = -0.0022826; double f_5 = -0.0001418;
                    double f_6 = -0.0057562; double f_7 = 6.403 * Math.Pow(10, -5); double f_8 = -2.588 * Math.Pow(10, -6); double f_9 = 2.175 * Math.Pow(10, -5); double f_10 = 7.33779 * Math.Pow(10, -5);
                    RHo = f_1 + f_2 * Tout_a + f_3 * hout_a + f_4 * Math.Pow(Tout_a, 2) + f_5 * Math.Pow(hout_a, 2) + f_6 * Tout_a * hout_a + f_7 * Math.Pow(Tout_a, 3) + f_8 * Math.Pow(hout_a, 3) + f_9 * Tout_a * Math.Pow(hout_a, 2) + f_10 * Math.Pow(Tout_a, 2) * hout_a;

                }
                else
                {
                    //Tdp = 10;
                    double g_1 = -1.1598572; double g_2 = 0.046598; double g_3 = 0.0888223; double g_4 = -0.0014229; double g_5 = -0.0001526;
                    double g_6 = -0.0037078; double g_7 = 1.942 * Math.Pow(10, -5); double g_8 = 2.487 * Math.Pow(10, -7); double g_9 = 3.229 * Math.Pow(10, -6); double g_10 = 4.53442 * Math.Pow(10, -5);
                    RHo = g_1 + g_2 * Tout_a + g_3 * hout_a + g_4 * Math.Pow(Tout_a, 2) + g_5 * Math.Pow(hout_a, 2) + g_6 * Tout_a * hout_a + g_7 * Math.Pow(Tout_a, 3) + g_8 * Math.Pow(hout_a, 3) + g_9 * Tout_a * Math.Pow(hout_a, 2) + g_10 * Math.Pow(Tout_a, 2) * hout_a;

                }


                //Fully wetted outlet temp K
                double Tout_r_wet = Tout_r;
                //Dry fraction
                f_dry = 0.0;

                double iter1 = 1;
                double x11 = 1;
                double x21 = 1;
                double eps1 = 1;
                double error = 1;
                double Tout_r_guess = 1;
                double y11 = 1; double y21 = 1; double x31 = 1; double change1 = 1;
                y1 = 1; y2 = 1; double x1 = 1; double x2 = 1; x3 = 1;
                double change = 1;

                //半干半湿计算

                if (Tin_s > Tdp+100.0) //Jingming

                    //if (Tin_s > Tdp && Tout_s < Tdp)
                //Partially wet and Partially dry with single-fase on refrigerant side
                {
                    iter1 = 1;
                    //Now do an iterative solver to find the fraction of the coil that is wetted

                    x1 = 0.01;
                    x2 = 0.99;

                    eps1 = Math.Pow(10, -8);

                    double h_a_x = 0;
                    double T_a_x = 0;


                    while ((iter1 <= 3 | Math.Abs(error) > eps1) && iter1 < 100)
                    {
                        if (iter1 == 1)
                        {
                            f_dry = x1;
                        }

                          //if (iter > 1)
                        else
                        {

                            f_dry = x2;
                        }

                        f_dry = 0.8;// 0.01 * iter1;





                        double K = Ntu_dry * (1.0 - C_ratio);
                        double expk = Math.Exp(-K * f_dry);
                        if (cp_a * ma < cp_r / 1000 * mr)
                        {
                            Tout_r_guess = (Tdp + C_ratio * (Tin_a - Tdp) - expk * (1 - K / Ntu_o) * Tin_a) / (1 - expk * (1 - K / Ntu_o));
                        }

                        else
                        {
                            Tout_r_guess = (expk * (Tin_a + (C_ratio - 1) * Tdp) - C_ratio * (1 + K / Ntu_o) * Tin_a) / (expk * C_ratio - C_ratio * (1 + K / Ntu_o));
                        }

                        //Wet and dry effective effectiveness

                        //纯逆流计算
                        epsilon_dry = (1 - Math.Exp(-f_dry * Ntu_dry * (1 - C_ratio))) / (1 - C_ratio * Math.Exp(-f_dry * Ntu_dry * (1 - C_ratio)));
            
                        //纯逆流计算
                        epsilon_wet = (1 - Math.Exp(-(1 - f_dry) * Ntu_wet * (1 - m_star))) / (1 - m_star * Math.Exp(-(1 - f_dry) * Ntu_wet * (1 - m_star)));
                        //double epsilon_wet = 1 - Math.Exp(Math.Pow(m_star, -1.0) * Math.Pow(Ntu_wet, 0.22)
                        //* (Math.Exp(-m_star * Math.Pow(Ntu_wet, 0.78)) - 1));
                        //mdot_min = 1.0;

                        //Temperature of water where condensation begins
                        double T_w_x = (tri + mdot_min / (cp_r / 1000 * mr) * epsilon_wet * (hin_a - h_s_w_i - epsilon_dry * C_min / ma * Tin_a)) / (1 - C_min * mdot_min / (cp_r * mr * ma) * epsilon_wet * epsilon_dry);
                        //Obtained from energy balance on air side
                        T_a_x = Tin_a - epsilon_dry * C_min * (Tin_a - T_w_x) / (ma * cp_a);
                        //Enthalpy of air where condensation begins
                        h_a_x = hin_a - cp_a * (Tin_a - T_a_x);
                        //New water temperature (stored temporarily to be able to build change)
                        Tout_r = C_min / (cp_r / 1000 * mr) * epsilon_dry * Tin_a + (1 - C_min / (cp_r / 1000 * mr) * epsilon_dry) * T_w_x;
                        //Difference between initial guess and outlet
                        error = Tout_r - Tout_r_guess;


                        if (iter1 > 500)
                        //print Superheated region wet analysis f_dry convergence faiuled
                        {
                            res.Q = Q_dry;
                            //break;
                            //continue;
                            throw new Exception("iter for f_dryConverge > 100.");
                        }


                        /*if (iter1 == 1)
                        {
                            y1 = Math.Abs(error);
>>>>>>> Temp
                        }

                        if (iter1 > 1)
                        {
                            y2 = error;
                            x3 = x2 - y2 / (y2 - y1) * (x2 - x1);
                            change = Math.Abs(y2 / (y2 - y1) * (x2 - x1));
<<<<<<< HEAD
                            y1 = y2; x1 = x2; x2 = x3;
                        }
=======
                            y1 = y2; x1 = x2; x2 = Math.Abs(x3);
                        }*/




                        //if hasattr(DWS,'Verbosity') and DWS.Verbosity > 7
                        //print "Partwet iter %d Toutr %0.5f dT %g" %(iter, Tout_r,errorToutr)
                        //Update loop counter
                        iter1 += 1;

                    }

                    //Ntu_owet = 1.0;
                    //Wet-analysis saturation enthalpy J/kg
                    h_s_s_e = h_a_x + (hout_a - h_a_x) / (1 - Math.Exp(-(1 - f_dry) * Ntu_owet));
                    //Surface effective temperature K

                    //T_s_e = 1.0;//HAProps('T','H',h_s_s_e/1000,'P',pin_a,'R',1.0);

                    T_s_e = -0.0016 * Math.Pow(h_s_s_e, 2) + 0.4914 * h_s_s_e - 2.9124;
                    //Air outlet temp based on effective surface temp K
                    Tout_a = T_s_e + (T_a_x - T_s_e) * Math.Exp(-(1 - f_dry) * Ntu_o);
                    //Heat transferred W
                    Q = mr * cp_r / 1000 * (Tout_r - tri);
                    //Dry-analysis air outlet enthalpy from energy balance J/kg
                    hout_a = hin_a - Q / ma;
                    //Sensible heat transfer rate kW
                    Q_sensible = ma * cp_a * (Tin_a - Tout_a);
                    //RHo = 0.7;

                    //相对湿度计算
                    if (Tout_a < 20)
                    {
                        //Tdp = 10;
                        double f_1 = -0.6625838; double f_2 = 0.0286184; double f_3 = 0.1029451; double f_4 = -0.0022826; double f_5 = -0.0001418;
                        double f_6 = -0.0057562; double f_7 = 6.403 * Math.Pow(10, -5); double f_8 = -2.588 * Math.Pow(10, -6); double f_9 = 2.175 * Math.Pow(10, -5); double f_10 = 7.33779 * Math.Pow(10, -5);
                        RHo = f_1 + f_2 * Tout_a + f_3 * hout_a + f_4 * Math.Pow(Tout_a, 2) + f_5 * Math.Pow(hout_a, 2) + f_6 * Tout_a * hout_a + f_7 * Math.Pow(Tout_a, 3) + f_8 * Math.Pow(hout_a, 3) + f_9 * Tout_a * Math.Pow(hout_a, 2) + f_10 * Math.Pow(Tout_a, 2) * hout_a;

                        if (RHo > 1.0)
                            RHo = 1.0;
                    }
                    else
                    {
                        //Tdp = 10;
                        double g_1 = -1.1598572; double g_2 = 0.046598; double g_3 = 0.0888223; double g_4 = -0.0014229; double g_5 = -0.0001526;
                        double g_6 = -0.0037078; double g_7 = 1.942 * Math.Pow(10, -5); double g_8 = 2.487 * Math.Pow(10, -7); double g_9 = 3.229 * Math.Pow(10, -6); double g_10 = 4.53442 * Math.Pow(10, -5);
                        RHo = g_1 + g_2 * Tout_a + g_3 * hout_a + g_4 * Math.Pow(Tout_a, 2) + g_5 * Math.Pow(hout_a, 2) + g_6 * Tout_a * hout_a + g_7 * Math.Pow(Tout_a, 3) + g_8 * Math.Pow(hout_a, 3) + g_9 * Tout_a * Math.Pow(hout_a, 2) + g_10 * Math.Pow(Tout_a, 2) * hout_a;

                        if (RHo > 1.0)
                            RHo = 1.0;
                    }

                }

                else
                {


                    Q = Q_wet;
                }
                


            }

            else
            //Coil is fully dry
            {

                Tout_a = Tout_a_dry;
                Q = Q_dry;
                Q_sensible = Q_dry;
            }



            res.RHout = RHo;//HAProps('R','T',Tout_a,'P',101.325,'W',res.omega_out)
            res.Q = Q;
            res.Tao = Tout_a;
            res.Tro = Tout_r;
            double f_sp = RefrigerantSPDP.ff_Friction(Re_r);
            res.DP = zdp * f_sp * l / dh * Math.Pow(g, 2.0) / rho_r / 2000;
            res.Pro = fluid == "Water" ? pri : pri - res.DP;
            res.hro = hri + Math.Pow(-1, hexType) * res.Q / mr;



            return res;

        }

    }
}
