using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Model.Basic;


namespace Model
{
    public class Main
    {
        public static CalcResult main_condenser_inputSC_py(double Tsc_set, RefStateInput refInput, AirStateInput airInput, GeometryInput geoInput, CapiliaryInput capInput, AbstractState coolprop)
        {
            //***几何结构赋值***//
            CalcResult res = new CalcResult();
            int Nrow = geoInput.Nrow;//2
            int[] Ntube = { geoInput.Ntube, geoInput.Ntube };
            int N_tube = Ntube[0];

            double Pt = geoInput.Pt * 0.001;
            double Pr = geoInput.Pr * 0.001;
            double Di = geoInput.Di * 0.001;
            double Do = geoInput.Do * 0.001;
            double L = geoInput.L * 0.001;
            double thickness = 0.5 * (Do - Di);

            double[] FPI = new double[Nrow + 1];
            FPI = new double[] { geoInput.FPI, geoInput.FPI };
            double Fthickness = geoInput.Fthickness * 0.001;//0.095 * 0.001;
            //***几何结构赋值完成***//
            int CirNum = geoInput.CirNum;//流路数目赋值
            int Nelement = 5;//5;单管单元格数赋值

            //流路均分设计
            int[,] CirArrange;

            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { CirNum, CirNum };
            //Avoid invalid Ncir input 
            if (CircuitInfo.number[0] > Ntube[0])
            {
                throw new Exception("circuit number is beyond range.");
            }

            CircuitInfo.TubeofCir = new int[CircuitInfo.number[0]];

            //Get AutoCircuitry
            CircuitInfo = AutoCircuiting.GetTubeofCir(Nrow, N_tube, CircuitInfo);
            CirArrange = new int[CircuitInfo.number[0], CircuitInfo.TubeofCir[CircuitInfo.number[0] - 1]];
            CirArrange = AutoCircuiting.GetCirArrange_2Row(CirArrange, Nrow, N_tube, CircuitInfo);

            CirArr[] cirArr = new CirArr[Nrow * N_tube];
            cirArr = CirArrangement.ReadCirArr(CirArrange, CircuitInfo, Nrow, Ntube).CirArr;
            //CircuitType CirType = new CircuitType();
            CircuitInfo.CirType = CircuitIdentification.CircuitIdentify(CircuitInfo.number, CircuitInfo.TubeofCir, cirArr);

            double[] d_cap = capInput.d_cap;
            double[] lenth_cap = capInput.lenth_cap;

            GeometryInput geoInput_air = new GeometryInput();
            geoInput_air.Pt = Pt;
            geoInput_air.Pr = Pr;
            geoInput_air.Do = Do;
            geoInput_air.Fthickness = Fthickness;
            geoInput_air.FPI = FPI[0];
            geoInput_air.Nrow = Nrow;

            int hexType = 1;
            //******制冷剂、风进口参数输入******//
            string fluid = refInput.FluidName;
            //AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            double mr = refInput.Massflowrate;//initial input 
            //double te = refInput.te;
            //double P_exv = refInput.P_exv;
            //double T_exv = refInput.T_exv;
            double tc = refInput.tc;
            double tri = refInput.tri;

            double Va = airInput.Volumetricflowrate;//0.28317; //m/s
            double tai = airInput.tai;//26.67;
            double RHi = airInput.RHi;//0.469;
            //******制冷剂、风进口参数输入完成******//

            double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
            //double Vel_ave =2.032;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
            AirDistribution VaDistri = new AirDistribution();
            VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
            double[,] ma = new double[N_tube, Nelement];
            double[,] ha = new double[N_tube, Nelement];
            double H = Pt * N_tube;
            double Hx = L * H;
            double rho_a_st = 1.2; //kg/m3

            double Vel_ave = Va / Hx;

            //空气侧几何结构选择
            //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
            //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
            int curve = 1; //
            //double za = 1; //Adjust factor
            for (int i = 0; i < N_tube; i++)
            {
                for (int j = 0; j < Nelement; j++)
                {
                    ma[i, j] = VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st;
                    //ha[i, j] = airInput.ha;// AirHTC.alpha(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve);// *1.5;
                    //ha[i, j] = 79;
                    //ha[i, j] = AirHTC.alpha1(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve, geoInput_air, hexType).ha;
                }
            }
            ha = AirHTC_CAL.alpha_cal(ha, VaDistri.Va, VaDistri.Va_ave, Vel_ave, airInput.za, curve, geoInput_air, hexType, N_tube, Nelement);
            double[,] haw = ha;

            res.DPa = AirHTC.alpha1(Vel_ave, airInput.za, curve, geoInput_air, hexType).dP_a * airInput.zdpa;

            double eta_surface = 1;
            double zh = refInput.zh;
            double zdp = refInput.zdp;
            coolprop.update(input_pairs.QT_INPUTS, 0, tc + 273.15);
            double pri = coolprop.p() / 1000;
            //double pri = CoolProp.PropsSI("P", "T", tc + 273.15, "Q", 0, fluid) / 1000;
            double conductivity = 386;
            double Pwater = 100.0;
            coolprop.update(input_pairs.PT_INPUTS, pri * 1000, tri + 273.15);
            double hri = coolprop.hmass() / 1000;
            //double hri = CoolProp.PropsSI("H", "T", tri + 273.15, "P", pri * 1000, fluid) / 1000;

            double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
            double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

            //string AirDirection="DowntoUp";
            string AirDirection = "逆流";
            ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai, tc, AirDirection);
            RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi, tc, AirDirection);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, tri, pri, hri,
                //mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection);
            
            //double Tsc_set = 5;//define supercooling temp

            double hsc_cal = 0;
            double Tro_set = 0;
            double hsc_set = 0;

            int ii = 0;
            //supercooling temp calculaiton loop
            do
            {

                //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, tri, pri, hri,
                // mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

                res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, tri, pri, hri,
                 mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap, coolprop);


                if (res.x_o >= 0 && res.x_o <= 1)
                {
                    coolprop.update(input_pairs.PQ_INPUTS, res.Pro * 1000, res.x_o);
                    hsc_cal = coolprop.hmass() / 1000;
                    //hsc_cal = CoolProp.PropsSI("H", "P", res.Pro * 1000, "Q", res.x_o, fluid) / 1000;
                }
                else
                {
                    coolprop.update(input_pairs.PT_INPUTS, res.Pro * 1000, res.Tro + 273.15);
                    hsc_cal = coolprop.hmass() / 1000;
                    //hsc_cal = CoolProp.PropsSI("H", "P", res.Pro * 1000, "T", res.Tro + 273.15, fluid) / 1000;
                }

                coolprop.update(input_pairs.PQ_INPUTS, res.Pro * 1000, 0);
                Tro_set = coolprop.T() - 273.15 - Tsc_set;
                //Tro_set = CoolProp.PropsSI("T", "P", res.Pro * 1000, "Q", 0, fluid) - 273.15 - Tsc_set;

                coolprop.update(input_pairs.PT_INPUTS, res.Pro * 1000, Tro_set + 273.15);
                hsc_set = coolprop.hmass() / 1000;
                //hsc_set = CoolProp.PropsSI("H", "P", res.Pro * 1000, "T", Tro_set + 273.15, fluid) / 1000;

                mr = mr * Math.Pow((hsc_set / hsc_cal), 1.8); 

                ii++;

            } while (Math.Abs((hsc_cal - hsc_set) / hsc_set) > 0.005);

            return res;
        }
        public static CalcResult main_evaporator_inputSH_py(double Tsh_set, RefStateInput refInput, AirStateInput airInput, GeometryInput geoInput, CapiliaryInput capInput, AbstractState coolprop)
        {

            CalcResult res = new CalcResult();

            int Nrow = geoInput.Nrow;//2
            int[] Ntube = { geoInput.Ntube, geoInput.Ntube };
            int N_tube = Ntube[0];

            double Pt = geoInput.Pt * 0.001;
            double Pr = geoInput.Pr * 0.001;
            double Di = geoInput.Di * 0.001;
            double Do = geoInput.Do * 0.001;
            double L = geoInput.L * 0.001;
            double thickness = 0.5 * (Do - Di);
            double[] FPI = new double[Nrow + 1];
            FPI = new double[] { geoInput.FPI, geoInput.FPI };
            double Fthickness = geoInput.Fthickness * 0.001;
            int CirNum = geoInput.CirNum;//流路数目赋值
            int Nelement = 5;//单管单元格数赋值

            //流路均分设计
            int[,] CirArrange;

            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { CirNum, CirNum };
            //Avoid invalid Ncir input 
            if (CircuitInfo.number[0] > Ntube[0])
            {
                throw new Exception("circuit number is beyond range.");
            }

            CircuitInfo.TubeofCir = new int[CircuitInfo.number[0]];

            //Get AutoCircuitry
            CircuitInfo = AutoCircuiting.GetTubeofCir(Nrow, N_tube, CircuitInfo);
            CirArrange = new int[CircuitInfo.number[0], CircuitInfo.TubeofCir[CircuitInfo.number[0] - 1]];
            CirArrange = AutoCircuiting.GetCirArrange_2Row(CirArrange, Nrow, N_tube, CircuitInfo);

            CirArr[] cirArr = new CirArr[Nrow * N_tube];
            cirArr = CirArrangement.ReadCirArr(CirArrange, CircuitInfo, Nrow, Ntube).CirArr;
            CircuitInfo.CirType = CircuitIdentification.CircuitIdentify(CircuitInfo.number, CircuitInfo.TubeofCir, cirArr);


            //Circuit-Reverse module
            bool reverse = true; //*********************************false:origin, true:reverse******************************************
            CirArrange = CircuitReverse.CirReverse(reverse, CirArrange, CircuitInfo);

            double[] d_cap = capInput.d_cap;
            double[] lenth_cap = capInput.lenth_cap;

            GeometryInput geoInput_air = new GeometryInput();
            geoInput_air.Pt = Pt;
            geoInput_air.Pr = Pr;
            geoInput_air.Do = Do;
            geoInput_air.Fthickness = Fthickness;
            geoInput_air.FPI = FPI[0];
            geoInput_air.Nrow = Nrow;

            int hexType = 0; //***0 is evap, 1 is cond***//
            //******制冷剂、风进口参数输入******//
            string fluid = refInput.FluidName;
            //AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            double te = refInput.te;//45.0;
            double P_exv = refInput.P_exv;
            double T_exv = refInput.T_exv;

            double Va = airInput.Volumetricflowrate;//0.28317; //m/s
            double tai = airInput.tai;//26.67;
            double RHi = airInput.RHi;//0.469;

            //******制冷剂、风进口参数输入完成******//

            //double mr = 0.02;
            //double Vel_a = 1.8; //m/s
            double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
            //double Vel_ave = 2.032;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
            AirDistribution VaDistri = new AirDistribution();
            VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
            double[,] ma = new double[N_tube, Nelement];
            double[,] ha = new double[N_tube, Nelement];
            double H = Pt * N_tube;
            double Hx = L * H;
            double rho_a_st = 1.2; //kg/m3

            double Vel_ave = Va / Hx;

            //空气侧几何结构选择
            //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
            //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
            int curve = 1; //

            //double za = 1; //Adjust factor
            for (int i = 0; i < N_tube; i++)
            {
                for (int j = 0; j < Nelement; j++)
                {
                    ma[i, j] = VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st;
                    //ha[i, j] = airInput.ha;// AirHTC.alpha(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve) * 1.5;
                    //ha[i, j] = 79;
                    //ha[i, j] = AirHTC.alpha1(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve, geoInput_air, hexType).ha;
                }
            }
            ha = AirHTC_CAL.alpha_cal(ha, VaDistri.Va, VaDistri.Va_ave, Vel_ave, airInput.za, curve, geoInput_air, hexType, N_tube, Nelement);

            double[,] haw = ha;

            res.DPa = AirHTC.alpha1(Vel_ave, airInput.za, curve, geoInput_air, hexType).dP_a * airInput.zdpa;

            double eta_surface = 1;
            double zh = refInput.zh;
            double zdp = refInput.zdp;
            coolprop.update(input_pairs.QT_INPUTS, 0, te + 273.15);
            double pe = coolprop.p() / 1000;
            //double pe = CoolProp.PropsSI("P", "T", te + 273.15, "Q", 0, fluid) / 1000;
            //double P_exv = 1842.28;//kpa
            //double T_exv = 20;//C
            double conductivity = 386; //w/mK for Cu
            double Pwater = 0;
            double hri = 0;
            if (refInput.H_exv != 0)
                hri = refInput.H_exv;
            else if (P_exv != 0)
            {
                coolprop.update(input_pairs.PT_INPUTS, P_exv * 1000, T_exv + 273.15);
                hri = coolprop.hmass() / 1000;
                //hri = CoolProp.PropsSI("H", "T", T_exv + 273.15, "P", P_exv * 1000, fluid) / 1000;
            }
            else hri = 0;

            double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
            double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

            //string AirDirection="DowntoUp";
            string AirDirection = "顺流";
            ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai, te, AirDirection);
            RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi, te, AirDirection);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);

            //double Tsh_set = 5;//define superheating temp
            double mr = refInput.Massflowrate;//assume mr input;
            //double mr = 0.006;//assume mr input

            double hsh_cal = 0;
            double Tro_set = 0;
            double hsh_set = 0;

            int ii = 0;
            //superheating temp calculaiton loop
            do
            {

                //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, te, pe, hri,
                //mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

                res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, te, pe, hri,
                mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap, coolprop);

                if (res.x_o >= 0 && res.x_o <= 1)
                {
                    coolprop.update(input_pairs.PQ_INPUTS, res.Pro * 1000, res.x_o);
                    hsh_cal = coolprop.hmass() / 1000;
                    //hsh_cal = CoolProp.PropsSI("H", "P", res.Pro * 1000, "Q", res.x_o, fluid) / 1000;
                }
                else
                {
                    coolprop.update(input_pairs.PT_INPUTS, res.Pro * 1000, res.Tro + 273.15);
                    hsh_cal = coolprop.hmass() / 1000;
                    //hsh_cal = CoolProp.PropsSI("H", "P", res.Pro * 1000, "T", res.Tro + 273.15, fluid) / 1000;
                }

                coolprop.update(input_pairs.PQ_INPUTS, res.Pro * 1000, 1);
                Tro_set = coolprop.T() - 273.15 + Tsh_set;
                //Tro_set = CoolProp.PropsSI("T", "P", res.Pro * 1000, "Q", 1, fluid) - 273.15 + Tsh_set;

                coolprop.update(input_pairs.PT_INPUTS, res.Pro * 1000, Tro_set + 273.15);
                hsh_set = coolprop.hmass() / 1000;
                //hsh_set = CoolProp.PropsSI("H", "T", Tro_set + 273.15, "P", res.Pro * 1000, fluid) / 1000;

                mr = mr * Math.Pow((hsh_cal / hsh_set), 3.8);

                ii++;

            } while (Math.Abs((hsh_cal - hsh_set) / hsh_set) > 0.0027);
 
            return res;
        }
        public static CalcResult main_evaporator_py(RefStateInput refInput, AirStateInput airInput, GeometryInput geoInput, CapiliaryInput capInput, AbstractState coolprop)
        {
            CalcResult res = new CalcResult();

            int Nrow = geoInput.Nrow;//2
            int[] Ntube = { geoInput.Ntube, geoInput.Ntube };
            int N_tube = Ntube[0];

            double Pt = geoInput.Pt * 0.001;
            double Pr = geoInput.Pr * 0.001;
            double Di = geoInput.Di * 0.001;
            double Do = geoInput.Do * 0.001;
            double L = geoInput.L * 0.001;
            double thickness = 0.5 * (Do - Di);
            double[] FPI = new double[Nrow + 1];
            FPI = new double[] { geoInput.FPI, geoInput.FPI };
            double Fthickness = geoInput.Fthickness * 0.001;
            int CirNum = geoInput.CirNum;//流路数目赋值
            int Nelement = 5;//单管单元格数赋值

            //流路均分设计
            int[,] CirArrange;

            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { CirNum, CirNum };
            //Avoid invalid Ncir input 
            if (CircuitInfo.number[0] > Ntube[0])
            {
                throw new Exception("circuit number is beyond range.");
            }

            CircuitInfo.TubeofCir = new int[CircuitInfo.number[0]];

            //Get AutoCircuitry
            CircuitInfo = AutoCircuiting.GetTubeofCir(Nrow, N_tube, CircuitInfo);
            CirArrange = new int[CircuitInfo.number[0], CircuitInfo.TubeofCir[CircuitInfo.number[0] - 1]];
            CirArrange = AutoCircuiting.GetCirArrange_2Row(CirArrange, Nrow, N_tube, CircuitInfo);

            CirArr[] cirArr = new CirArr[Nrow * N_tube];
            cirArr = CirArrangement.ReadCirArr(CirArrange, CircuitInfo, Nrow, Ntube).CirArr;
            CircuitInfo.CirType = CircuitIdentification.CircuitIdentify(CircuitInfo.number, CircuitInfo.TubeofCir, cirArr);


            //Circuit-Reverse module
            bool reverse = true; //*********************************false:origin, true:reverse******************************************
            CirArrange = CircuitReverse.CirReverse(reverse, CirArrange, CircuitInfo);

            double[] d_cap = capInput.d_cap;
            double[] lenth_cap = capInput.lenth_cap;

            GeometryInput geoInput_air = new GeometryInput();
            geoInput_air.Pt = Pt;
            geoInput_air.Pr = Pr;
            geoInput_air.Do = Do;
            geoInput_air.Fthickness = Fthickness;
            geoInput_air.FPI = FPI[0];
            geoInput_air.Nrow = Nrow;

            int hexType = 0; //***0 is evap, 1 is cond***//
            //******制冷剂、风进口参数输入******//
            string fluid = refInput.FluidName;
            //AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            double mr = refInput.Massflowrate;//0.01;
            double te = refInput.te;//45.0;
            double P_exv = refInput.P_exv;
            double T_exv = refInput.T_exv;

            double Va = airInput.Volumetricflowrate;//0.28317; //m/s
            double tai = airInput.tai;//26.67;
            double RHi = airInput.RHi;//0.469;

            //******制冷剂、风进口参数输入完成******//

            //double mr = 0.02;
            //double Vel_a = 1.8; //m/s
            double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
            //double Vel_ave = 2.032;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
            AirDistribution VaDistri = new AirDistribution();
            VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
            double[,] ma = new double[N_tube, Nelement];
            double[,] ha = new double[N_tube, Nelement];
            double H = Pt * N_tube;
            double Hx = L * H;
            double rho_a_st = 1.2; //kg/m3

            double Vel_ave = Va / Hx;

            //空气侧几何结构选择
            //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
            //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
            int curve = 1; //

            //double za = 1; //Adjust factor
            for (int i = 0; i < N_tube; i++)
            {
                for (int j = 0; j < Nelement; j++)
                {
                    ma[i, j] = VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st;
                    //ha[i, j] = airInput.ha;// AirHTC.alpha(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve) * 1.5;
                    //ha[i, j] = 79;
                    //ha[i, j] = AirHTC.alpha1(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve, geoInput_air, hexType).ha;
                }
            }
            ha = AirHTC_CAL.alpha_cal(ha, VaDistri.Va, VaDistri.Va_ave, Vel_ave, airInput.za, curve, geoInput_air, hexType, N_tube, Nelement);

            double[,] haw = ha;

            res.DPa = AirHTC.alpha1(Vel_ave, airInput.za, curve, geoInput_air, hexType).dP_a * airInput.zdpa;

            double eta_surface = 1;
            double zh = refInput.zh;
            double zdp = refInput.zdp;
            coolprop.update(input_pairs.QT_INPUTS, 0, te + 273.15);
            double pe = coolprop.p() / 1000;
            //double pe = CoolProp.PropsSI("P", "T", te + 273.15, "Q", 0, fluid) / 1000;
            //double P_exv = 1842.28;//kpa
            //double T_exv = 20;//C
            double conductivity = 386; //w/mK for Cu
            double Pwater = 0;
            double hri=0;
            if (refInput.H_exv != 0)
                hri = refInput.H_exv;
            else if (P_exv != 0)
            {
                coolprop.update(input_pairs.PT_INPUTS, P_exv * 1000, T_exv + 273.15);
                hri = coolprop.hmass() / 1000;
                //hri = CoolProp.PropsSI("H", "T", T_exv + 273.15, "P", P_exv * 1000, fluid) / 1000;
            }
            else hri = 0;

            double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
            double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

            //string AirDirection="DowntoUp";
            string AirDirection = "顺流";
            ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai, te, AirDirection);
            RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi, te, AirDirection);


            //AreaResult geo = new AreaResult();
            //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, te, pe, hri,
                //mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, te, pe, hri,
                mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap, coolprop);


            return res;
        }
        public static CalcResult main_condenser_py(RefStateInput refInput, AirStateInput airInput, GeometryInput geoInput, CapiliaryInput capInput, AbstractState coolprop)
        {
            //***几何结构赋值***//
            CalcResult res = new CalcResult();
            int Nrow = geoInput.Nrow;//2
            int[] Ntube = { geoInput.Ntube, geoInput.Ntube };
            int N_tube = Ntube[0];

            double Pt = geoInput.Pt * 0.001;
            double Pr = geoInput.Pr * 0.001;
            double Di = geoInput.Di * 0.001;
            double Do = geoInput.Do * 0.001;
            double L = geoInput.L * 0.001;
            double thickness = 0.5 * (Do - Di);

            double[] FPI = new double[Nrow + 1];
            FPI = new double[] { geoInput.FPI, geoInput.FPI };
            double Fthickness = geoInput.Fthickness * 0.001;//0.095 * 0.001;
            //***几何结构赋值完成***//
            int CirNum = geoInput.CirNum;//流路数目赋值
            int Nelement = 5;//5;单管单元格数赋值

            //流路均分设计
            int[,] CirArrange;

            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { CirNum, CirNum };
            //Avoid invalid Ncir input 
            if (CircuitInfo.number[0] > Ntube[0])
            {
                throw new Exception("circuit number is beyond range.");
            }

            CircuitInfo.TubeofCir = new int[CircuitInfo.number[0]];

            //Get AutoCircuitry
            CircuitInfo = AutoCircuiting.GetTubeofCir(Nrow, N_tube, CircuitInfo);
            CirArrange = new int[CircuitInfo.number[0], CircuitInfo.TubeofCir[CircuitInfo.number[0] - 1]];
            CirArrange = AutoCircuiting.GetCirArrange_2Row(CirArrange, Nrow, N_tube, CircuitInfo);

            CirArr[] cirArr = new CirArr[Nrow * N_tube];
            cirArr = CirArrangement.ReadCirArr(CirArrange, CircuitInfo, Nrow, Ntube).CirArr;
            //CircuitType CirType = new CircuitType();
            CircuitInfo.CirType = CircuitIdentification.CircuitIdentify(CircuitInfo.number, CircuitInfo.TubeofCir, cirArr);

            double[] d_cap = capInput.d_cap;
            double[] lenth_cap = capInput.lenth_cap;

            GeometryInput geoInput_air = new GeometryInput();
            geoInput_air.Pt = Pt;
            geoInput_air.Pr = Pr;
            geoInput_air.Do = Do;
            geoInput_air.Fthickness = Fthickness;
            geoInput_air.FPI = FPI[0];
            geoInput_air.Nrow = Nrow;

            int hexType = 1;
            //******制冷剂、风进口参数输入******//
            string fluid = refInput.FluidName;
            //AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            double mr = refInput.Massflowrate;
            //double te = refInput.te;
            //double P_exv = refInput.P_exv;
            //double T_exv = refInput.T_exv;
            double tc = refInput.tc;
            double tri = refInput.tri;

            double Va = airInput.Volumetricflowrate;//0.28317; //m/s
            double tai = airInput.tai;//26.67;
            double RHi = airInput.RHi;//0.469;
            //******制冷剂、风进口参数输入完成******//

            double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
            //double Vel_ave =2.032;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
            AirDistribution VaDistri = new AirDistribution();
            VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
            double[,] ma = new double[N_tube, Nelement];
            double[,] ha = new double[N_tube, Nelement];
            double H = Pt * N_tube;
            double Hx = L * H;
            double rho_a_st = 1.2; //kg/m3

            double Vel_ave = Va / Hx;

            //空气侧几何结构选择
            //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
            //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
            int curve = 1; //
            //double za = 1; //Adjust factor
            for (int i = 0; i < N_tube; i++)
            {
                for (int j = 0; j < Nelement; j++)
                {
                    ma[i, j] = VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st;
                    //ha[i, j] = airInput.ha;// AirHTC.alpha(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve);// *1.5;
                    //ha[i, j] = 79;
                    //ha[i, j] = AirHTC.alpha1(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve, geoInput_air, hexType).ha;
                }
            }
            ha = AirHTC_CAL.alpha_cal(ha, VaDistri.Va, VaDistri.Va_ave, Vel_ave, airInput.za, curve, geoInput_air, hexType, N_tube, Nelement);
            double[,] haw = ha;

            res.DPa = AirHTC.alpha1(Vel_ave, airInput.za, curve, geoInput_air, hexType).dP_a * airInput.zdpa;

            double eta_surface = 1;
            double zh = refInput.zh;
            double zdp = refInput.zdp;
            coolprop.update(input_pairs.QT_INPUTS, 0, tc + 273.15);
            double pri = coolprop.p() / 1000;
            //double pri = CoolProp.PropsSI("P", "T", tc + 273.15, "Q", 0, fluid) / 1000;
            double conductivity = 386; 
            double Pwater = 100.0;
            coolprop.update(input_pairs.PT_INPUTS, pri * 1000, tri + 273.15);
            double hri = coolprop.hmass() / 1000;
            //double hri = CoolProp.PropsSI("H", "T", tri + 273.15, "P", pri * 1000, fluid) / 1000 ;

            double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
            double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

            //string AirDirection="DowntoUp";
            string AirDirection = "逆流";
            ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai, tc, AirDirection);
            RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi, tc, AirDirection);


            //AreaResult geo = new AreaResult();
            //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, tri, pri, hri,
                //mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, L, geo, ta, RH, tri, pri, hri,
                mr, ma, ha,haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater,AirDirection, d_cap, lenth_cap, coolprop);


            return res;
        }
        public static CalcResult main_evaporator()
        {
            string fluid = "R410A";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            //string fluid = new string { "R410A.mix" };
            //string fluid = new string[] { "ISOBUTAN" };
            CalcResult res = new CalcResult();
            int Nrow = 2;
            double[] FPI = new double[Nrow + 1];
            //FPI = new double[] { 1.27, 1.27, 1.27, 1.27, 1.27, 1.27, 1.27, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 5.2, 5.2, 5.2, 5.2, 5.2, 5.2 };
            FPI = new double[] { 15, 15 };
            double Pt = 1 * 25.4 * 0.001;
            double Pr = 0.75 * 25.4 * 0.001;
            double Di = 8.4074 * 0.001;//8 6.8944
            double Do = 10.0584 * 0.001;//8.4 7.35
            double Fthickness = 0.095 * 0.001;
            double thickness = 0.5 * (Do - Di);
            //double n_tubes = 10;
            //double n_rows = 2;
            //int[] Ntube = { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
            //int[] Ntube = { 2, 2, 2, 2 };
            int[] Ntube = { 6, 6 };
            int N_tube = Ntube[0];
            double L = 914.4 * 0.001;
            int Nelement = 1;
            int[,] CirArrange;
            //CirArrange = new int[,] { { 8, 6, 4, 2, 1, 3, 5, 7 } };//actual, counter-paralle,  Q=83.1
            //CirArrange = new int[,] { { 20, 18, 16, 14, 12, 10, 8, 6, 4, 2, 1, 3, 5, 7, 9, 11, 13, 15, 17, 19 } };//actual, counter-paralle,  Q=83.1
            //CirArrange = new int[,] { { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 1, 3, 5, 7, 9, 11, 13, 15, 17, 19 } }; //paralle-paralle, better Q=85.3
            //CirArrange = new int[,] { { 20, 18, 16, 14, 12, 10, 8, 6, 4, 2, 19, 17, 15, 13, 11, 9, 7, 5, 3, 1 } };//counter-counter,  Q=82.4
            //CirArrange = new int[,] { { 14, 12, 10, 8, 6, 4, 2, 1, 3, 5, 7, 9, 11, 13 } };//actual, counter-paralle,  Q=76
            //CirArrange = new int[,] { {16, 14, 12, 10, 8, 6, 4, 2, 1, 3, 5, 7, 9, 11, 13, 15 } };//actual, counter-paralle,  Q=79
            //CirArrange = new int[,] { {42, 40, 38, 36, 34, 32, 30, 28, 26, 24, 22, 20, 18, 16, 14, 12, 10, 8, 6, 4, 2, 
            //                              1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25, 27, 29, 31, 33, 35, 37, 39, 41 } };//actual, counter-paralle,  Q=79
            //CirArrange = new int[,] { {32, 30, 28, 26, 24, 22, 20, 18, 16, 14, 12, 10, 8, 6, 4, 2, 
            //                              1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25, 27, 29, 31 } };//actual, counter-paralle,  Q=79

            //CirArrange = new int[,] { { 7, 8, 2, 1, 5, 3 }, {5, 8, 9, 10, 11}, {5, 88, 5, 4, 3 } };
            //CirArrange = new int[,] { { 7, 8, 2, 1, 0, 0, 0, 0 }, { 9, 10, 11, 12, 6, 5, 4, 3 } };
            //CirArrange = new int[,] { { 7, 1, 0, 0, 0, 0 }, { 8, 9, 3, 2, 0, 0 }, { 10, 11, 12, 6, 5, 4 } };
            //CirArrange = new int[,] { { 7, 8, 2, 1, 9, 10, 11, 12, 6, 5, 4, 3 } };
            //List<string> productType = new List<string>();
            CirArrange = new int[,] { { 7, 8, 9, 3, 2, 1 }, { 12, 11, 10, 4, 5, 6 } };
            //CirArrange = new int[,] { { 7, 8, 9, 4, 5, 6 }, { 12, 11, 10, 3, 2, 1 } };
            //CirArrange = new int[,] { { 7, 8, 5, 6 }, { 10, 9, 2, 1 }, { 12, 11, 4, 3 } };
            //CirArrange = new int[,] { { 7, 8, 2, 1 }, { 10, 9, 3, 4 }, { 11, 12, 6, 5 } };
            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { 2, 2 };
            CircuitInfo.TubeofCir = new int[] { 6, 6 };  //{ 4, 8 };
            // [19 - 17 - 15 - 13   11   9   7   5   3   1] <====Air
            // [20 - 18 - 16 - 14   12   10  8   6   4   2] <====Air
            //  Ncir=1, 20in, 20->19 1out

            double[] d_cap = new double[] { 0, 0 };
            double[] lenth_cap = new double[] { 0, 0 };

            double mr = 0.02;
            //double Vel_a = 1.8; //m/s
            double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
            double Vel_ave = 2.032;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
            AirDistribution VaDistri = new AirDistribution();
            VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
            double[,] ma = new double[N_tube, Nelement];
            double[,] ha = new double[N_tube, Nelement];
            double H = Pt * N_tube;
            double Hx = L * H;
            double rho_a_st = 1.2; //kg/m3

            int hexType = 0;

            GeometryInput geoInput_air = new GeometryInput();
            geoInput_air.Pt = Pt;
            geoInput_air.Pr = Pr;
            geoInput_air.Do = Do;
            geoInput_air.Fthickness = Fthickness;
            geoInput_air.FPI = FPI[0];
            geoInput_air.Nrow = Nrow;

            //空气侧几何结构选择
            //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
            //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
            int curve = 1; //
            AirCoef_res res1 = new AirCoef_res();
            double za = 1; //Adjust factor
            for (int i = 0; i < N_tube; i++)
            {
                for (int j = 0; j < Nelement; j++)
                {
                    ma[i, j] = VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st;
                    //ha[i, j] = AirHTC.alpha(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve) * 1.5;
                    //ha[i, j] = 79;
                    ha[i, j] = AirHTC.alpha1(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve, geoInput_air, hexType).ha;
                }
            }
            double[,] haw = ha;

            res.DPa = AirHTC.alpha1(Vel_ave, za, curve, geoInput_air, hexType).dP_a;

            double eta_surface = 1;
            double zh = 1;
            double zdp = 1;

            double tai = 26.67;
            double RHi = 0.469;
            double tri = 7.2;
            double te = tri;
            coolprop.update(input_pairs.QT_INPUTS, 0, te + 273.15);
            double pe = coolprop.p() / 1000;
            //double pe = CoolProp.PropsSI("P", "T", te + 273.15, "Q", 0, fluid) / 1000;

            double P_exv = 1842.28;//kpa
            double T_exv = 20;//C
            double conductivity = 386; //w/mK for Cu
            double Pwater = 0;
            //int hexType = 0; //*********************************0 is evap, 1 is cond******************************************
            coolprop.update(input_pairs.PT_INPUTS, P_exv * 1000, T_exv + 273.15);
            double hri = coolprop.hmass() / 1000;
            //double hri = CoolProp.PropsSI("H", "T", T_exv + 273.15, "P", P_exv * 1000, fluid) / 1000 ;
            double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
            double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

            //string AirDirection="DowntoUp";
            string AirDirection = "逆流";
            ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai, te, AirDirection);
            RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi, te, AirDirection);


            //AreaResult geo = new AreaResult();
            //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, te, pe, hri,
            //mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, te, pe, hri,
            mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap, coolprop);

            return res;

        }
        public CalcResult main_condenser(RefStateInput refInput, AirStateInput airInput, GeometryInput geoInput, string flowtype, string fin_type, string tube_type, string hex_type, CapiliaryInput capInput)
        {
            //制冷剂制热模块计算
            //string fluid = new string[] { "Water" };
            string fluid = refInput.FluidName;// refri_in;// "R32";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            //string fluid = new string[] { "ISOBUTAN" };
            CalcResult res = new CalcResult();
            int Nrow = geoInput.Nrow;
            double[] FPI = new double[Nrow + 1];
            //FPI = new double[] { 1.27, 1.27, 1.27, 1.27, 1.27, 1.27, 1.27, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 5.2, 5.2, 5.2, 5.2, 5.2, 5.2 };
            FPI = new double[] { 25.4 / geoInput.FPI, 25.4 / geoInput.FPI };//to be updated
            double Pt = geoInput.Pt * 0.001;//1 * 25.4 * 0.001;
            double Pr = geoInput.Pr * 0.001;//0.75 * 25.4 * 0.001;
            //double Di = 8.4074 * 0.001;//8 6.8944
            double Do = geoInput.Do * 0.001;// 10.0584 * 0.001;//8.4 7.35
            double Fthickness = geoInput.Fthickness * 0.001;// 0.095 * 0.001;
            double thickness = geoInput.Tthickness * 0.001;// 0.5 * (Do - Di);
            double Di = Do - 2 * thickness; //8.4074 * 0.001;//8 6.8944
            //double n_tubes = 10;
            //double n_rows = 2;
            //int[] Ntube = { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
            //int[] Ntube = { 2, 2, 2, 2 };
            int[] Ntube = { geoInput.Ntube, geoInput.Ntube };
            int N_tube = Ntube[0];
            double L = geoInput.L * 0.001;// 914.4 * 0.001;
            int Nelement = 5;
            int CirNum = geoInput.CirNum;// 2;//流路数目赋值
            int[,] CirArrange;

            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { CirNum, CirNum };
            //Avoid invalid Ncir input 
            if (CircuitInfo.number[0] > Ntube[0])
            {
                throw new Exception("circuit number is beyond range.");
            }

            CircuitInfo.TubeofCir = new int[CircuitInfo.number[0]];

            //Get AutoCircuitry
            CircuitInfo = AutoCircuiting.GetTubeofCir(Nrow, N_tube, CircuitInfo);
            CirArrange = new int[CircuitInfo.number[0], CircuitInfo.TubeofCir[CircuitInfo.number[0] - 1]];
            CirArrange = AutoCircuiting.GetCirArrange_2Row(CirArrange, Nrow, N_tube, CircuitInfo);

            //CirArrange = new int[,] { { 25, 26, 27, 28, 4, 3, 2, 1 }, { 29, 30, 31, 32, 8, 7, 6, 5 }, { 33, 34, 35, 36, 12, 11, 10, 9 }, { 37, 38, 39, 40, 16, 15, 14, 13 }, { 41, 42, 43, 44, 20, 19, 18, 17 }, { 48, 47, 46, 45, 24, 23, 22, 21 } };
            //CircuitInfo.number = new int[] { 4, 2 };//4in 2out
            //CircuitInfo.TubeofCir = new int[] { 8, 8, 8, 8, 8, 8 };  //{ 4, 8 };
            //CircuitInfo.UnequalCir = new int[] { 5, 5, 6, 6, 0, 0 };

            CirArr[] cirArr = new CirArr[Nrow * N_tube];
            cirArr = CirArrangement.ReadCirArr(CirArrange, CircuitInfo, Nrow, Ntube).CirArr;
            //CircuitType CirType = new CircuitType();
            CircuitInfo.CirType = CircuitIdentification.CircuitIdentify(CircuitInfo.number, CircuitInfo.TubeofCir, cirArr);

            //Circuit-Reverse module
            bool reverse = false; //*********************************false:origin, true:reverse******************************************
            if (flowtype == "逆流")
                reverse = false;
            else
                reverse = true;

            CirArrange = CircuitReverse.CirReverse(reverse, CirArrange, CircuitInfo);

            double[] d_cap = capInput.d_cap;
            double[] lenth_cap = capInput.lenth_cap;

            GeometryInput geoInput_air = new GeometryInput();
            geoInput_air.Pt = Pt;
            geoInput_air.Pr = Pr;
            geoInput_air.Do = Do;
            geoInput_air.Fthickness = Fthickness;
            geoInput_air.FPI = FPI[0];
            geoInput_air.Nrow = Nrow;

            int hexType = 1;
            if (hex_type == "蒸发器") hexType = 0;
            else hexType = 1;

            double mr = refInput.Massflowrate; //mr_in;//0.01;
            //double Vel_a = 1.8; //m/s
            double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
            //double Vel_ave = 1;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
            AirDistribution VaDistri = new AirDistribution();
            VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
            double[,] ma = new double[N_tube, Nelement];
            double[,] ha = new double[N_tube, Nelement];
            double H = Pt * N_tube;
            double Hx = L * H;
            double rho_a_st = 1.2; //kg/m3
            double Va = airInput.Volumetricflowrate;
            double Vel_ave = Va / Hx;
            //空气侧几何结构选择
            //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
            //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
            int curve = 1; //

            double za = 1; //Adjust factor
            if (fin_type == "平片")
                za = 1.0;
            else if (fin_type == "louver")
                za = 1.3;
            else
                za = 1.1;

            for (int i = 0; i < N_tube; i++)
            {
                for (int j = 0; j < Nelement; j++)
                {
                    ma[i, j] = VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st;
                    //ha[i, j] = AirHTC.alpha(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve);// *1.5;
                    //ha[i, j] = 79;
                    ha[i, j] = AirHTC.alpha1(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve, geoInput_air, hexType).ha;
                }
            }
            double[,] haw = ha;

            res.DPa = AirHTC.alpha1(Vel_ave, za, curve, geoInput_air, hexType).dP_a;

            double eta_surface = 1;
            double zh = 1;
            double zdp = 1;

            if (tube_type == "光管")
            { zh = 1.0; zdp = 1.0; }
            else if (tube_type == "内螺纹管")
            { zh = 1.4; zdp = 1.2; }
            else
            { zh = 1.2; zdp = 1.1; }

            double tai = airInput.tai;// tai_in;//26.67;
            double RHi = airInput.RHi;// RHi_in;//0.469;
            double tc = refInput.tc;// tc_in;//45.0;
            //double pri = Refrigerant.SATT(fluid, composition, tc + 273.15, 1).Pressure;
            coolprop.update(input_pairs.QT_INPUTS, 0, tc + 273.15);
            double pri = coolprop.p() / 1000;
            //double pri = CoolProp.PropsSI("P", "T", tc + 273.15, "Q", 0, fluid) / 1000;
            //double P_exv = 1842.28;//kpa
            double tri = refInput.tri;// tri_in;//78;//C
            double conductivity = 386; //w/mK for Cu
            double Pwater = 100.0;
            //int hexType = 1; //*********************************0 is evap, 1 is cond******************************************
            coolprop.update(input_pairs.PT_INPUTS, pri * 1000, tri + 273.15);
            double hri = coolprop.hmass() / 1000;
            //double hri = CoolProp.PropsSI("H", "T", tri + 273.15, "P", pri * 1000, fluid) / 1000 ;
            //double hri = 354.6;
            //double xin = 0.57;

            double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
            double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

            //string AirDirection="DowntoUp";
            string AirDirection = flowtype;// "Counter";
            ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai, tc, AirDirection);
            RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi, tc, AirDirection);


            //AreaResult geo = new AreaResult();
            //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, tri, pri, hri,
                //mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, tri, pri, hri,
                mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap, coolprop);


            return res;
        }
        public static CalcResult main_condenser_Slab(RefStateInput refInput, AirStateInput airInput, GeometryInput geoInput, string flowtype, string fin_type, string tube_type, string hex_type, CapiliaryInput capInput)
        {
            //制冷剂制热模块计算
            string fluid = refInput.FluidName;// refri_in;// "R32";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            CalcResult res = new CalcResult();
            int Nrow = geoInput.Nrow;
            double[] FPI = new double[Nrow + 1];
            FPI = new double[] { 25.4 / geoInput.FPI, 25.4 / geoInput.FPI };//to be updated
            double Pt = geoInput.Pt * 0.001;//1 * 25.4 * 0.001;
            double Pr = geoInput.Pr * 0.001;//0.75 * 25.4 * 0.001;
            double Do = geoInput.Do * 0.001;// 10.0584 * 0.001;//8.4 7.35
            double Di = geoInput.Di * 0.001;
            double Fthickness = geoInput.Fthickness * 0.001;// 0.095 * 0.001;
            double thickness = 0.5 * (Do - Di); //geoInput.Tthickness * 0.001;// 
            //double Di = Do - 2 * thickness; //8.4074 * 0.001;//8 6.8944
            int[] Ntube = { geoInput.Ntube, geoInput.Ntube };
            int N_tube = Ntube[0];
            double L = geoInput.L * 0.001;// 914.4 * 0.001;
            int Nelement = 5;
            int CirNum = geoInput.CirNum;// 2;//流路数目赋值
            int[,] CirArrange;

            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { CirNum, CirNum };
            //Avoid invalid Ncir input 
            if (CircuitInfo.number[0] > Ntube[0])
            {
                throw new Exception("circuit number is beyond range.");
            }
            CircuitInfo.TubeofCir = new int[CircuitInfo.number[0]];
            //Get AutoCircuitry
            //CircuitInfo = AutoCircuiting.GetTubeofCir(Nrow, N_tube, CircuitInfo);
            //CirArrange = new int[CircuitInfo.number[0], CircuitInfo.TubeofCir[CircuitInfo.number[0] - 1]];
            //CirArrange = AutoCircuiting.GetCirArrange_2Row(CirArrange, Nrow, N_tube, CircuitInfo);

            //CirArrange = new int[,] { { 13, 14, 15, 16, 4, 3, 2, 1 }, { 17, 18, 19, 20, 8, 7, 6, 5 }, { 21, 22, 23, 24, 12, 11, 10, 9 } };
            //CirArrange = new int[,] { { 15, 16, 17, 3, 2, 1 }, { 18, 19, 20, 6, 5, 4 }, { 21, 22, 23, 9, 8, 7 }, { 24, 25, 26, 12, 11, 10 }, { 27, 13, 0, 0, 0, 0 }, { 28, 14, 0, 0, 0, 0 } };
            //CirArrange = new int[,] { { 15, 16, 2, 1, 0, 0, 0, 0 }, { 17, 18, 19, 20, 6, 5, 4, 3 }, { 21, 22, 8, 7, 0, 0, 0, 0 }, { 23, 24, 25, 26, 12, 11, 10, 9 }, { 27, 13, 0, 0, 0, 0, 0, 0 }, { 28, 14, 0, 0, 0, 0, 0, 0 } };
            //CirArrange = new int[,] { { 15, 16, 2, 1, 0, 0, 0, 0 }, { 17, 18, 19, 20, 6, 5, 4, 3 }, { 21, 22, 8, 7, 0, 0, 0, 0 }, { 23, 24, 25, 11, 10, 9, 0, 0 }, { 26, 27, 13, 12, 0, 0, 0, 0 }, { 28, 14, 0, 0, 0, 0, 0, 0 } };
            //CirArrange = new int[,] { { 15, 1, 0, 0, 0, 0, 0, 0 }, { 16, 2, 0, 0, 0, 0, 0, 0 }, { 17, 18, 4, 3, 0, 0, 0, 0 }, { 19, 20, 21, 22, 8, 7, 6, 5 }, { 23, 24, 10, 9, 0, 0, 0, 0 }, { 25, 26, 27, 28, 14, 13, 12, 11 } };
            CirArrange = new int[,] { { 15, 1, 0, 0, 0, 0, 0, 0 }, { 16, 17, 3, 2, 0, 0, 0, 0 }, { 18, 19, 5, 4, 0, 0, 0, 0 }, { 20, 21, 22, 8, 7, 6, 0, 0 }, { 23, 24, 10, 9, 0, 0, 0, 0 }, { 25, 26, 27, 28, 14, 13, 12, 11 } };
            CircuitInfo.number = new int[] { 4, 2 };//4in 2out
            CircuitInfo.TubeofCir = new int[] { 2, 4, 4, 6, 4, 8 };//{ 6, 6, 6, 6, 2, 2 };  //{ 4, 8 };
            CircuitInfo.UnequalCir = new int[] { 5, 5, 6, 6, 0, 0 };

            /*int[,] Node_in = { { -1, -100 }, { 0, -100 }, { 0, -100 }, { 0, 1 }, { 2, 3 }, { 4, 5 } };
            int[,] Node_inType = { { -1, -100 }, { 0, -100 }, { 0, -100 }, { 1, 1 }, { 1, 1 }, { 1, 1 } };//0:Node,1:Circuit,-1:in/out,-100:meaningless
            int[] Node_Nin = { 1, 1, 1, 2, 2, 2 };
            int[,] Node_out = { { 1, 2 }, { 0, 1 }, { 2, 3 }, { 4, -100 }, { 5, -100 },{-1,-100} };
            int[,] Node_outType = { { 0, 0 }, { 1, 1 }, { 1, 1 }, { 1, -100 }, { 1, -100 }, { -1, -100 } };
            int[] Node_Nout = { 2, 2, 2, 1, 1, 1 };
            char[] Node_type = { 'D', 'D' ,'D','C','C','C'};// Diverse/Converge
            int[] Node_couple = { 0, 1, 2, 1, 2, 0 };*/
            /*
            int[,] Node_in = { { -1, -100 }, { 0, -100 }, { 1, -100 }, { 2, 3 }, { 4, 5 }, { 3, 4 } };
            int[,] Node_inType = { { -1, -100 }, { 1, -100 }, { 1, -100 }, { 1, 1 }, { 1, 1 }, { 0, 0 } };//0:Node,1:Circuit,-1:in/out,-100:meaningless
            int[] Node_Nin = { 1, 1, 1, 2, 2, 2 };
            int[,] Node_out = { { 0, 1 }, { 2, 3 }, { 4, 5 }, { 5, -100 }, { 5, -100 }, { -1, -100 } };
            int[,] Node_outType = { { 1, 1 }, { 1, 1 }, { 1, 1 }, { 0, -100 }, { 0, -100 }, { -1, -100 } };
            int[] Node_Nout = { 2, 2, 2, 1, 1, 1 };
            char[] Node_type = { 'D', 'D', 'D', 'C', 'C', 'C' };// Diverse/Converge
            int[] Node_couple = { 0, 1, 2, 1, 2, 0 };
            */
            int[,] Node_in = { { -1, -100 }, { 0, -100 }, { 0, -100 }, { 0, 1 }, { 2, 3 }, { 4, 5 } };
            int[,] Node_inType = { { -1, -100 }, { 0, -100 }, { 0, -100 }, { 1, 1 }, { 1, 1 }, { 1, 1 } };//0:Node,1:Circuit,-1:in/out,-100:meaningless
            int[] Node_Nin = { 1, 1, 1, 2, 2, 2 };
            int[,] Node_out = { { 1, 2 }, { 0, 1 }, { 2, 3 }, { 4, -100 }, { 5, -100 }, { -1, -100 } };
            int[,] Node_outType = { { 0, 0 }, { 1, 1 }, { 1, 1 }, { 1, -100 }, { 1, -100 }, { -1, -100 } };
            int[] Node_Nout = { 2, 2, 2, 1, 1, 1 };
            char[] Node_type = { 'D', 'D', 'D', 'C', 'C', 'C' };// Diverse/Converge
            int[] Node_couple = { 0, 1, 2, 1, 2, 0 };

            int N_Node=Node_in.GetLength(0);
            NodeInfo[] Nodes = new NodeInfo[N_Node];
            for(int i=0;i<N_Node;i++)
            {
                Nodes[i] = new NodeInfo();
                Nodes[i].N_in = Node_Nin[i];
                Nodes[i].N_out = Node_Nout[i];
                Nodes[i].inlet = new int[Nodes[i].N_in];
                Nodes[i].inType = new int[Nodes[i].N_in];
                Nodes[i].outlet = new int[Nodes[i].N_out];
                Nodes[i].outType = new int[Nodes[i].N_out];
                for (int j = 0; j < Nodes[i].N_in;j++ )
                {
                    Nodes[i].inlet[j] = Node_in[i, j];
                    Nodes[i].inType[j] = Node_inType[i, j];
                }
                for (int j = 0; j < Nodes[i].N_out; j++)
                {
                    Nodes[i].outlet[j] = Node_out[i, j];
                    Nodes[i].outType[j] = Node_outType[i, j];
                }
                Nodes[i].type = Node_type[i];
                Nodes[i].couple = Node_couple[i];
                Nodes[i].mri = new double[Nodes[i].N_in];
                Nodes[i].mro = new double[Nodes[i].N_out];
                Nodes[i].pri = new double[Nodes[i].N_in];
                Nodes[i].pro = new double[Nodes[i].N_out];
                Nodes[i].hri = new double[Nodes[i].N_in];
                Nodes[i].hro = new double[Nodes[i].N_out];
                Nodes[i].tri = new double[Nodes[i].N_in];
                Nodes[i].tro = new double[Nodes[i].N_out];
                Nodes[i].mr_ratio = new double[Nodes[i].N_out];
            }

            CirArr[] cirArr = new CirArr[Nrow * N_tube];
            cirArr = CirArrangement.ReadCirArr(CirArrange, CircuitInfo, Nrow, Ntube).CirArr;
            //CircuitType CirType = new CircuitType();
            CircuitInfo.CirType = CircuitIdentification.CircuitIdentify(CircuitInfo.number, CircuitInfo.TubeofCir, cirArr);

            //Circuit-Reverse module
            bool reverse = false; //*********************************false:origin, true:reverse******************************************
            if (flowtype == "逆流")
                reverse = false;
            else
                reverse = true;
            CirArrange = CircuitReverse.CirReverse(reverse, CirArrange, CircuitInfo);

            double[] d_cap = capInput.d_cap;
            double[] lenth_cap = capInput.lenth_cap;

            GeometryInput geoInput_air = new GeometryInput();
            geoInput_air.Pt = Pt;
            geoInput_air.Pr = Pr;
            geoInput_air.Do = Do;
            geoInput_air.Fthickness = Fthickness;
            geoInput_air.FPI = FPI[0];
            geoInput_air.Nrow = Nrow;

            int hexType = 1;
            if (hex_type == "蒸发器") hexType = 0;
            else hexType = 1;

            double mr = refInput.Massflowrate; //mr_in;//0.01;
            //double Vel_a = 1.8; //m/s
            double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
            //double Vel_ave = 1;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
            AirDistribution VaDistri = new AirDistribution();
            VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
            double[,] ma = new double[N_tube, Nelement];
            double[,] ha = new double[N_tube, Nelement];
            double H = Pt * N_tube;
            double Hx = L * H;
            double rho_a_st = 1.2; //kg/m3
            double Va = airInput.Volumetricflowrate;
            double Vel_ave = Va / Hx / 3600;
            double za = 1; //Adjust factor
            if (fin_type == "平片")
                za = 1.0;
            else if (fin_type == "百叶窗片")
                za = 1.3;
            else
                za = 1.1;
            int curve = 1;
            for (int i = 0; i < N_tube; i++)
            {
                for (int j = 0; j < Nelement; j++)
                {
                    ma[i, j] = VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st;
                    //ha[i, j] = AirHTC.alpha(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve);// *1.5;
                    //ha[i, j] = 79;
                    ha[i, j] = AirHTC.alpha1(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve, geoInput_air, hexType).ha;
                }
            }
            double[,] haw = ha;

            res.DPa = AirHTC.alpha1(Vel_ave, za, curve, geoInput_air, hexType).dP_a;

            double eta_surface = 1;
            double zh = 1;
            double zdp = 1;

            if (tube_type == "光管")
            { zh = 1.0; zdp = 1.0; }
            else if (tube_type == "内螺纹管")
            { zh = 1.4; zdp = 1.2; }
            else
            { zh = 1.2; zdp = 1.1; }

            double tai = airInput.tai;// tai_in;//26.67;
            double RHi = airInput.RHi;// RHi_in;//0.469;
            double tc = refInput.tc;// tc_in;//45.0;
            //double pri = Refrigerant.SATT(fluid, composition, tc + 273.15, 1).Pressure;

            coolprop.update(input_pairs.QT_INPUTS, 0, tc + 273.15);
            double pri = coolprop.p() / 1000;
            //double P_exv = 1842.28;//kpa
            double tri = refInput.tri;// tri_in;//78;//C
            double conductivity = 386; //w/mK for Cu
            double Pwater = 100.0;
            //int hexType = 1; //*********************************0 is evap, 1 is cond******************************************
            coolprop.update(input_pairs.PT_INPUTS, pri * 1000, tri + 273.15);
            double hri = coolprop.hmass() / 1000;
            //double hri = CoolProp.PropsSI("H", "T", tri + 273.15, "P", pri * 1000, fluid) / 1000;
            //double hri = 354.6;
            //double xin = 0.57;

            double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
            double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

            //string AirDirection="DowntoUp";
            string AirDirection = flowtype;// "Counter";
            ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai, tc, AirDirection);
            RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi, tc, AirDirection);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, L, geo, ta, RH, tri, pri, hri,
            //    mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap, coolprop);
            res = Slab2.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, L, geo, ta, RH, tri, pri, hri,
                mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection,Nodes,N_Node,d_cap, lenth_cap, coolprop);
            return res;
        }
        
        public static CalcResult main_RACR32condenser_1()
        {
            //制冷剂制热模块计算
            string fluid = "R32";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            CalcResult res = new CalcResult();
            int Nrow = 2;
            double[] FPI = new double[Nrow + 1];
            FPI = new double[] { 19.54, 19.54 };
            double Pt = 14.5 * 0.001;
            double Pr = 20 * 0.001;
            double Di = 4.84 * 0.001;//8 6.8944
            double Do = 5.334 * 0.001;//8.4 7.35
            double Fthickness = 0.095 * 0.001;
            double thickness = 0.5 * (Do - Di);
            int[] Ntube = { 32, 32 };
            int N_tube = Ntube[0];
            double L = 842.5 * 0.001;
            int Nelement = 10;
            int[,] CirArrange;
            //CirArrange = new int[,] { { 38, 37, 36, 35, 1, 2, 3, 4 }, { 42, 41, 40, 39, 5, 6, 7, 8 }, { 46, 45, 44, 43, 9, 10, 11, 12 }, { 50, 49, 48, 47, 13, 14, 15, 16 }, 
            //{ 54, 53, 52, 51, 17, 18, 19, 20 }, { 58, 57, 56, 55, 21, 22, 23, 24 }, { 62, 61, 60, 59, 25, 26, 27, 28 }, { 66, 65, 64, 63, 29, 30, 31, 32 } };
            CirArrange = new int[,] { { 36, 35, 34, 33, 1, 2, 3, 4 }, { 40, 39, 38, 37, 5, 6, 7, 8 }, { 44, 43, 42, 41, 9, 10, 11, 12 }, { 48, 47, 46, 45, 13, 14, 15, 16 }, 
            { 52, 51, 50, 49, 17, 18, 19, 20 }, { 56, 55, 54, 53, 21, 22, 23, 24 }, { 60, 59, 58, 57, 25, 26, 27, 28 }, { 64, 63, 62, 61, 29, 30, 31, 32 } };

            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { 8, 8 }; 
            CircuitInfo.TubeofCir = new int[] { 8, 8, 8, 8, 8, 8, 8, 8 };
            CircuitInfo.UnequalCir = null;

            double[] d_cap = new double[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            double[] lenth_cap = new double[] { 0, 0, 0, 0, 0, 0, 0, 0 };

            GeometryInput geoInput_air = new GeometryInput();
            geoInput_air.Pt = Pt;
            geoInput_air.Pr = Pr;
            geoInput_air.Do = Do;
            geoInput_air.Fthickness = Fthickness;
            geoInput_air.FPI = FPI[0];
            geoInput_air.Nrow = Nrow;

            int hexType = 1;

            double tai = 35;
            double mr = 0.01725;
            double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
            double Vel_ave = 2.032;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
            AirDistribution VaDistri = new AirDistribution();
            VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
            double[,] ma = new double[N_tube, Nelement];
            double[,] ha = new double[N_tube, Nelement];
            double H = Pt * N_tube;
            double Hx = L * H;
            double rho_a_st = 1 / CoolProp.HAProps("Vha", "T", 273.15 + tai, "P", 101.325, "R", 0.469); //kg/m3

            //空气侧几何结构选择
            //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
            //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
            int curve = 1; //

            double za = 1; //Adjust factor
            for (int i = 0; i < N_tube; i++)
            {
                for (int j = 0; j < Nelement; j++)
                {
                    ma[i, j] = VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st;
                    //ha[i, j] = AirHTC.alpha(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve) * 1.5;
                    //ha[i, j] = 79;
                    ha[i, j] = AirHTC.alpha1(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve, geoInput_air, hexType).ha;
                }
            }
            double[,] haw = ha;

            res.DPa = AirHTC.alpha1(Vel_ave, za, curve, geoInput_air, hexType).dP_a;

            double eta_surface = 1;
            double zh = 1;
            double zdp = 3.2;
            double RHi = 0.469;
            double tc = 46.23;
            //double pri = Refrigerant.SATT(fluid, composition, tc + 273.15, 1).Pressure;
            coolprop.update(input_pairs.QT_INPUTS, 0, tc + 273.15);
            double pri = coolprop.p() / 1000;
            //double pri = CoolProp.PropsSI("P", "T", tc + 273.15, "Q", 0, fluid) / 1000;
            double tri = 95;//C
            double conductivity = 386; //w/mK for Cu
            double Pwater = 100.0;
            //int hexType = 1; //*********************************0 is evap, 1 is cond******************************************
            coolprop.update(input_pairs.PT_INPUTS, pri * 1000, tri + 273.15);
            double hri = coolprop.hmass() / 1000;
            //double hri = CoolProp.PropsSI("H", "T", tri + 273.15, "P", pri * 1000, fluid) / 1000 ;

            double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
            double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

            //string AirDirection="DowntoUp";
            string AirDirection = "逆流";
            ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai, tc, AirDirection);
            RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi, tc, AirDirection);


            //AreaResult geo = new AreaResult();
            //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, tri, pri, hri,
                //mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, tri, pri, hri,
                mr, ma, ha,haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap, coolprop);


            return res;
        }
        public static CalcResult main_RACR32condenser()
        {
            //制冷剂制热模块计算
            string fluid = "R32";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            CalcResult res = new CalcResult();
            int Nrow = 2;
            double[] FPI = new double[Nrow + 1];
            FPI = new double[] { 19.54, 19.54 };
            double Pt = 14.5 * 0.001;
            double Pr = 20 * 0.001;
            double Di = 4.84 * 0.001;//8 6.8944
            double Do = 5.334 * 0.001;//8.4 7.35
            double Fthickness = 0.095 * 0.001;
            double thickness = 0.5 * (Do - Di);
            int[] Ntube = { 2, 2 };//****************modified 1
            int N_tube = Ntube[0];
            double L = 842.5 * 0.001;
            int Nelement = 1;
            int[,] CirArrange; //****************modified 2
            CirArrange = new int[,] { { 1, 2 }, { 3, 4 } };
            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { 2, 2 }; //****************modified 3
            CircuitInfo.TubeofCir = new int[] { 2, 2 }; //****************modified 4
            CircuitInfo.UnequalCir = null;

            double[] d_cap = new double[] { 0, 0 };
            double[] lenth_cap = new double[] { 0, 0 };

            GeometryInput geoInput_air = new GeometryInput();
            geoInput_air.Pt = Pt;
            geoInput_air.Pr = Pr;
            geoInput_air.Do = Do;
            geoInput_air.Fthickness = Fthickness;
            geoInput_air.FPI = FPI[0];
            geoInput_air.Nrow = Nrow;

            int hexType = 1; 

            //double mr = 0.01725;  //****************modified 5
            double tai = 35;           
            double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
            double Vel_ave = 2.032;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
            AirDistribution VaDistri = new AirDistribution();
            VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
            double[,] ma = new double[N_tube, Nelement];
            double[,] ha = new double[N_tube, Nelement];
            double H = Pt * N_tube;
            double Hx = L * H;
            double rho_a_st = 1 / CoolProp.HAProps("Vha", "T", 273.15 + tai, "P", 101.325, "R", 0.469); //kg/m3
            
            //空气侧几何结构选择
            //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
            //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
            int curve = 1; //

            double za = 1; //Adjust factor
            for (int i = 0; i < N_tube; i++)
            {
                for (int j = 0; j < Nelement; j++)
                {
                    ma[i, j] = VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st;
                    //ha[i, j] = AirHTC.alpha(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve) * 1.5;
                    //ha[i, j] = 79;
                    ha[i, j] = AirHTC.alpha1(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve, geoInput_air, hexType).ha;
                }
            }
            double[,] haw = ha;

            res.DPa = AirHTC.alpha1(Vel_ave, za, curve, geoInput_air, hexType).dP_a;

            double eta_surface = 1;
            double zh = 1;
            double zdp = 1;
            double RHi = 0.469;
            double tc = 46.23;
            //double pri = Refrigerant.SATT(fluid, composition, tc + 273.15, 1).Pressure; //****************modified 6
            //double tri = 84;//C   //****************modified 7
            double conductivity = 386; //w/mK for Cu
            double Pwater = 100.0;
            //int hexType = 1; //*********************************0 is evap, 1 is cond******************************************
            double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
            double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

            //string AirDirection="DowntoUp";
            string AirDirection = "逆流";
            ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai, tc, AirDirection);
            RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi, tc, AirDirection);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            var r = main_RACR32condenser_1(); //****************modified 9
            //****************modified 9

            //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, r.Tro, r.Pro, r.hro,
                //r.mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

            res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, r.Tro, r.Pro, r.hro,
                r.mr, ma, ha,haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap, coolprop);

            return res;
        }

        public static CalcResult main_condenser_YH200()
        {
            //制冷剂制热模块计算
            //string fluid = new string[] { "Water" };
            string fluid = "R32";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            //string fluid = new string[] { "ISOBUTAN" };
            CalcResult res = new CalcResult();
            int Nrow = 2;
            double[] FPI = new double[] { 19.538, 19.538 };
            double Pt = 21 * 0.001;
            double Pr = 22 * 0.001;
            double Di = 6.8944 * 0.001;//8 6.8944
            double Do = 7.35 * 0.001;//8.4 7.35
            double Fthickness = 0.095 * 0.001;
            double thickness = 0.5 * (Do - Di);
            int[] Ntube = { 24, 24 };
            int N_tube = Ntube[0];
            double L = (825 + 860) / 2 * 0.001;
            int Nelement = 5;
            int[,] CirArrange;
            //CirArrange = new int[,] { { 25, 26, 27, 28, 29, 30, 7, 8, 9, 10, 11, 12 }, { 1, 2, 3, 4, 5, 6, 31, 32, 33, 34, 35, 36 }, { 37, 38, 39, 40, 41, 42, 19, 20, 21, 22, 0, 0 }, { 13, 14, 15, 16, 17, 18, 43, 44, 45, 46, 0, 0 }, { 23, 24, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 47, 48, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } };
            //CirArrange = new int[,] { { 28, 27, 26, 25, 1, 2, 3, 4 }, { 29, 30, 31, 32, 5, 6, 7, 8 }, { 36, 35, 34, 33, 9, 10, 11, 12 }, { 37, 38, 39, 40, 13, 14, 15, 16 }, { 44, 43, 42, 41, 17, 18, 19, 20 }, { 45, 46, 47, 48, 24, 23, 22, 21 } };
            //CirArrange = new int[,] { { 30, 29, 28, 27, 26, 25, 1, 2, 3, 4 }, { 34, 33, 32, 31, 5, 6, 7, 8, 9, 10 }, { 40, 39, 38, 37, 36, 35, 11, 12, 13, 14 }, { 44, 43, 42, 41, 15, 16, 17, 18, 19, 20 }, { 46, 45, 21, 22, 0, 0, 0, 0, 0, 0 }, { 47, 48, 24, 23, 0, 0, 0, 0, 0, 0 } };            
            //CirArrange = new int[,] { { 32, 31, 30, 29, 28, 27, 26, 25, 1, 2, 3, 4, 5, 6, 7, 8 }, { 40, 39, 38, 37, 36, 35, 34, 33, 9, 10, 11, 12, 13, 14, 15, 16 }, { 48, 47, 46, 45, 44, 43, 42, 41, 24, 23, 22, 21, 20, 19, 18, 17 } };
            //CirArrange = new int[,] { { 25, 26, 27, 28, 29, 30, 7, 8, 9, 10, 11, 12 }, { 1, 2, 3, 4, 5, 6, 31, 32, 33, 34, 35, 36 }, { 37, 38, 39, 40, 41, 42, 19, 20, 21, 22, 0, 0 }, { 13, 14, 15, 16, 17, 18, 43, 44, 45, 46, 0, 0 }, { 47, 48, 24, 23, 0, 0, 0, 0, 0, 0, 0, 0 } };
            //CirArrange = new int[,] { { 25, 26, 27, 28, 29, 30, 6, 5, 4, 3, 2, 1 }, { 36, 35, 34, 33, 32, 31, 7, 8, 9, 10, 11, 12 }, { 37, 38, 39, 40, 41, 42, 18, 17, 16, 15, 14, 13 }, { 48, 47, 46, 45, 44, 43, 19, 20, 21, 22, 23, 24 } };
            CirArrange = new int[,] { { 25, 26, 27, 28, 4, 3, 2, 1 }, { 32, 31, 30, 29, 5, 6, 7, 8 }, { 33, 34, 35, 36, 12, 11, 10, 9 }, { 40, 39, 38, 37, 13, 14, 15, 16 }, { 44, 43, 42, 41, 17, 18, 19, 20 }, { 45, 46, 47, 48, 24, 23, 22, 21 } };            
            CircuitNumber CircuitInfo = new CircuitNumber();
            //CircuitInfo.number = new int[] { 4, 4 };//{ 4, 1 };//{ 3, 3 }; //{ 4, 2 }//{4,2}//{4,4}
            CircuitInfo.TubeofCir = new int[] { 8, 8, 8, 8, 8, 8 };//{ 12, 12, 10, 10, 4 };//{ 16,16,16 };  //{ 8,8,8,8,8,8 };//{12,12,10,10,2,2}//{ 12, 12, 12, 12 }//{10,10,10,10,4,4}
            //CircuitInfo.UnequalCir = new int[] { 5, 5, 5, 5, 0 };//{ 5, 5, 5, 5, 0 };//{ 5, 5, 6, 6, 0, 0 };//{5,5,6,6,0,0}
            // [19 - 17 - 15 - 13   11   9   7   5   3   1] <====Air
            // [20 - 18 - 16 - 14   12   10  8   6   4   2] <====Air
            //  Ncir=1, 20in, 20->19 1out

            //4--2 
            /*int[,] Node_in = { { -1, -100 }, { 0, -100 }, { 0, -100 }, { 0, 1 }, { 2, 3 }, { 4, 5 } };
            int[,] Node_inType = { { -1, -100 }, { 0, -100 }, { 0, -100 }, { 1, 1 }, { 1, 1 }, { 1, 1 } };//0:Node,1:Circuit,-1:in/out,-100:meaningless
            int[] Node_Nin = { 1, 1, 1, 2, 2, 2 };
            int[,] Node_out = { { 1, 2 }, { 0, 1 }, { 2, 3 }, { 4, -100 }, { 5, -100 }, { -1, -100 } };
            int[,] Node_outType = { { 0, 0 }, { 1, 1 }, { 1, 1 }, { 1, -100 }, { 1, -100 }, { -1, -100 } };
            int[] Node_Nout = { 2, 2, 2, 1, 1, 1 };
            char[] Node_type = { 'D', 'D', 'D', 'C', 'C', 'C' };// Diverse/Converge
            int[] Node_couple = { 0, 1, 2, 1, 2, 0 };*/

            //4--1--2
            int[,] Node_in = { { -1, -100, -100, -100 }, { 0, 1, 2, 3 }, { 1, -100, -100, -100 }, { 4, 5, -100, -100 } };
            int[,] Node_inType = { { -1, -100, -100, -100 }, { 1, 1, 1, 1 }, { 0, -100, -100, -100 }, { 1, 1, -100, -100 } };//0:Node,1:Circuit,-1:in/out,-100:meaningless
            int[] Node_Nin = { 1, 4, 1, 2 };
            int[,] Node_out = { { 0, 1, 2, 3 }, { 2, -100, -100, -100 }, { 4, 5, -100, -100 }, { -1, -100, -100, -100 } };
            int[,] Node_outType = { { 1, 1, 1, 1 }, { 0, -100, -100, -100 }, { 1, 1, -100, -100 }, { -1, -100, -100, -100 } };
            int[] Node_Nout = { 4, 1, 2, 1 };
            char[] Node_type = { 'D', 'C', 'D', 'C' };// Diverse/Converge
            int[] Node_couple = { 0, 0, 1, 1 };

            //3--2
            /*int[,] Node_in = { { -1, -100, -100 }, { 0, 1, 2 } };
            int[,] Node_inType = { { -1, -100, -100 }, { 1, 1, 1 } };//0:Node,1:Circuit,-1:in/out,-100:meaningless
            int[] Node_Nin = { 1, 3 };
            int[,] Node_out = { { 0, 1, 2 }, { -1, -100, -100 } };
            int[,] Node_outType = { { 1, 1, 1 }, { -1, -100, -100 } };
            int[] Node_Nout = { 3, 1};
            char[] Node_type = { 'D', 'C'};// Diverse/Converge
            int[] Node_couple = { 0, 0 };*/

            //4--1
            /*int[,] Node_in = { { -1, -100, -100, -100 }, { 0, -100, -100, -100 }, { 0, 1, 2, 3 }, { 4, -100, -100, -100 } };
            int[,] Node_inType = { { -1, -100, -100, -100 }, { 0, -100, -100, -100 }, { 1, 1, 1, 1 }, { 1, -100, -100, -100 } };//0:Node,1:Circuit,-1:in/out,-100:meaningless
            int[] Node_Nin = { 1, 1, 4, 1 };
            int[,] Node_out = { { 1, -100, -100, -100 }, { 0, 1, 2, 3 }, { 4, -100, -100, -100 }, { -1, -100, -100, -100 } };
            int[,] Node_outType = { { 0, -100, -100, -100 }, { 1, 1, 1, 1 }, { 1, -100, -100, -100 }, { -1, -100, -100, -100 } };
            int[] Node_Nout = { 1, 4, 1, 1 };
            char[] Node_type = { 'D', 'D', 'C', 'C' };// Diverse/Converge
            int[] Node_couple = { 0, 1, 1, 0 };*/

            //4--4
            /*int[,] Node_in = { { -1, -100, -100, -100 }, { 0, 1, 2, 3 } };
            int[,] Node_inType = { { -1, -100, -100, -100 }, { 1, 1, 1, 1 } };//0:Node,1:Circuit,-1:in/out,-100:meaningless
            int[] Node_Nin = { 1, 4 };
            int[,] Node_out = { { 0, 1, 2, 3 }, { -1, -100, -100, -100 } };
            int[,] Node_outType = { { 1, 1, 1, 1 }, { -1, -100, -100, -100 } };
            int[] Node_Nout = { 4, 1 };
            char[] Node_type = { 'D', 'C' };// Diverse/Converge
            int[] Node_couple = { 0, 0 };*/


            int N_Node = Node_in.GetLength(0);
            NodeInfo[] Nodes = new NodeInfo[N_Node];
            for (int i = 0; i < N_Node; i++)
            {
                Nodes[i] = new NodeInfo();
                Nodes[i].N_in = Node_Nin[i];
                Nodes[i].N_out = Node_Nout[i];
                Nodes[i].inlet = new int[Nodes[i].N_in];
                Nodes[i].inType = new int[Nodes[i].N_in];
                Nodes[i].outlet = new int[Nodes[i].N_out];
                Nodes[i].outType = new int[Nodes[i].N_out];
                for (int j = 0; j < Nodes[i].N_in; j++)
                {
                    Nodes[i].inlet[j] = Node_in[i, j];
                    Nodes[i].inType[j] = Node_inType[i, j];
                }
                for (int j = 0; j < Nodes[i].N_out; j++)
                {
                    Nodes[i].outlet[j] = Node_out[i, j];
                    Nodes[i].outType[j] = Node_outType[i, j];
                }
                Nodes[i].type = Node_type[i];
                Nodes[i].couple = Node_couple[i];
                Nodes[i].mri = new double[Nodes[i].N_in];
                Nodes[i].mro = new double[Nodes[i].N_out];
                Nodes[i].pri = new double[Nodes[i].N_in];
                Nodes[i].pro = new double[Nodes[i].N_out];
                Nodes[i].hri = new double[Nodes[i].N_in];
                Nodes[i].hro = new double[Nodes[i].N_out];
                Nodes[i].tri = new double[Nodes[i].N_in];
                Nodes[i].tro = new double[Nodes[i].N_out];
                Nodes[i].mr_ratio = new double[Nodes[i].N_out];
            }

            double[] d_cap = new double[] { 0, 0, 0, 0, 0, 0 };
            double[] lenth_cap = new double[] { 0, 0, 0, 0, 0, 0 };

            GeometryInput geoInput_air = new GeometryInput();
            geoInput_air.Pt = Pt;
            geoInput_air.Pr = Pr;
            geoInput_air.Do = Do;
            geoInput_air.Fthickness = Fthickness;
            geoInput_air.FPI = FPI[0];
            geoInput_air.Nrow = Nrow;

            int hexType = 1;

            double mr = 0.0277;//0.01826;
            //double Vel_a = 1.8; //m/s
            double H = Pt * N_tube;
            double Hx = L * H;
            double rho_a_st = 1.2; //kg/m3
            double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
            double Va = 2000;// m3/h
            double Vel_ave = Va / 3600 / Hx;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
            AirDistribution VaDistri = new AirDistribution();
            VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
            double[,] ma = new double[N_tube, Nelement];
            double[,] ha = new double[N_tube, Nelement];

            //空气侧几何结构选择
            //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
            //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
            int curve = 1; //

            double za = 1; //Adjust factor
            for (int i = 0; i < N_tube; i++)
            {
                for (int j = 0; j < Nelement; j++)
                {
                    ma[i, j] = VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st;
                    //ha[i, j] = AirHTC.alpha(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve);// *1.5;
                    //ha[i, j] = 79;
                    ha[i, j] = AirHTC.alpha1(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve, geoInput_air, hexType).ha;
                    //ha[i, j] = 54.71;
                }
            }
            double[,] haw = ha;

            res.DPa = AirHTC.alpha1(Vel_ave, za, curve, geoInput_air, hexType).dP_a;

            double eta_surface = 0.819;
            double zh = 3;
            double zdp = 3;

            double tai = 35;
            double RHi = CoolProp.HAPropsSI("R", "T", tai + 273.15, "P", 101325, "B", 24 + 273.15);//0.469
            double tc = 50.775;
            //double pri = Refrigerant.SATT(fluid, composition, tc + 273.15, 1).Pressure;
            coolprop.update(input_pairs.QT_INPUTS, 0, tc + 273.15);
            double pri = coolprop.p() / 1000;
            //double pri = CoolProp.PropsSI("P", "T", tc + 273.15, "Q", 0, fluid) / 1000;
            //double P_exv = 1842.28;//kpa
            double tri = 67;//C
            double conductivity = 386; //w/mK for Cu
            double Pwater = 100.0;
            //int hexType = 1; //*********************************0 is evap, 1 is cond******************************************
            coolprop.update(input_pairs.PT_INPUTS, pri * 1000, tri + 273.15);
            double hri = coolprop.hmass() / 1000;
            //double hri = CoolProp.PropsSI("H", "T", tri + 273.15, "P", pri * 1000, fluid) / 1000;

            double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
            double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

            //string AirDirection="DowntoUp";
            string AirDirection = "逆流";
            ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai, tc, AirDirection);
            RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi, tc, AirDirection);

            Geometry geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, tri, pri, hri,
            //    mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap, coolprop);
            res = Slab2.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, L, geo, ta, RH, tri, pri, hri,
                mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection,Nodes,N_Node,d_cap, lenth_cap, coolprop);           
            return res;
        }
        public static CalcResult main_condenser_YH200_1()
        {
            //制冷剂制热模块计算
            //string fluid = new string[] { "Water" };
            string fluid = "R32";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            //string fluid = new string[] { "ISOBUTAN" };
            CalcResult res = new CalcResult();
            int Nrow = 2;
            //double[] FPI = new double[Nrow + 1];
            //FPI = new double[] { 1.27, 1.27, 1.27, 1.27, 1.27, 1.27, 1.27, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 5.2, 5.2, 5.2, 5.2, 5.2, 5.2 };
            double[] FPI = new double[] { 19.538, 19.538 };
            double Pt = 21 * 0.001;
            double Pr = 22 * 0.001;
            double Di = 6.8944 * 0.001;//8 6.8944
            double Do = 7.35 * 0.001;//8.4 7.35
            double Fthickness = 0.095 * 0.001;
            double thickness = 0.5 * (Do - Di);
            //double n_tubes = 10;
            //double n_rows = 2;
            //int[] Ntube = { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
            //int[] Ntube = { 2, 2, 2, 2 };
            int[] Ntube = { 16, 16 };//{ 20, 20 };
            int N_tube = Ntube[0];
            double L = (825 + 860) / 2 * 0.001;
            int Nelement = 10;
            int[,] CirArrange;
            //CirArrange = new int[,] { { 26, 25, 24, 23, 22, 21, 1, 2, 3, 4 }, { 30, 29, 28, 27, 5, 6, 7, 8, 9, 10 }, { 36, 35, 34, 33, 32, 31, 11, 12, 13, 14 }, { 40, 39, 38, 37, 15, 16, 17, 18, 19, 20 } };
            CirArrange = new int[,] { { 17, 18, 19, 20, 4, 3, 2, 1 }, { 24, 23, 22, 21, 5, 6, 7, 8 }, { 25, 26, 27, 28, 12, 11, 10, 9 }, { 32, 31, 30, 29, 13, 14, 15, 16 } };

            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { 4, 4 };
            CircuitInfo.TubeofCir = new int[] { 8, 8, 8, 8, };//{ 10, 10, 10, 10 }
            //CircuitInfo.UnequalCir = new int[] { 5, 5, 5, 5, 0 };//{ 5, 5, 6, 6, 0, 0 };
            // [19 - 17 - 15 - 13   11   9   7   5   3   1] <====Air
            // [20 - 18 - 16 - 14   12   10  8   6   4   2] <====Air
            //  Ncir=1, 20in, 20->19 1out

            double[] d_cap = new double[] { 0, 0, 0, 0 };
            double[] lenth_cap = new double[] { 0, 0, 0, 0 };

            GeometryInput geoInput_air = new GeometryInput();
            geoInput_air.Pt = Pt;
            geoInput_air.Pr = Pr;
            geoInput_air.Do = Do;
            geoInput_air.Fthickness = Fthickness;
            geoInput_air.FPI = FPI[0];
            geoInput_air.Nrow = Nrow;

            int hexType = 1;

            double mr = 0.0278;
            //double Vel_a = 1.8; //m/s
            double H = Pt * N_tube;
            double Hx = L * H;
            double rho_a_st = 1.2; //kg/m3
            double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
            double Va = 2000 * 16 / 24;//2000*20/24;// m3/h
            double Vel_ave = Va / 3600 / Hx;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
            AirDistribution VaDistri = new AirDistribution();
            VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
            double[,] ma = new double[N_tube, Nelement];
            double[,] ha = new double[N_tube, Nelement];


            //空气侧几何结构选择
            //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
            //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
            int curve = 1; //

            double za = 1; //Adjust factor
            for (int i = 0; i < N_tube; i++)
            {
                for (int j = 0; j < Nelement; j++)
                {
                    ma[i, j] = VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st;
                    //ha[i, j] = AirHTC.alpha(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve);// *1.5;
                    //ha[i, j] = 79;
                    ha[i, j] = AirHTC.alpha1(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve, geoInput_air, hexType).ha;
                    //ha[i, j] = 36.5;
                }
            }
            double[,] haw = ha;

            res.DPa = AirHTC.alpha1(Vel_ave, za, curve, geoInput_air, hexType).dP_a;

            double eta_surface = 0.8;
            double zh = 1.3;
            double zdp = 2;

            double tai = 35;
            double RHi = CoolProp.HAPropsSI("R", "T", tai + 273.15, "P", 101325, "B", 24 + 273.15);//0.469
            double tc = 50.775;
       
            coolprop.update(input_pairs.QT_INPUTS, 0, tc + 273.15);
            double pri = coolprop.p() / 1000;
            //double pri = CoolProp.PropsSI("P", "T", tc + 273.15, "Q", 0, fluid) / 1000;
            //double P_exv = 1842.28;//kpa
            double tri = 78;//C
            double conductivity = 386; //w/mK for Cu
            double Pwater = 100.0;
            //int hexType = 1; //*********************************0 is evap, 1 is cond******************************************
            coolprop.update(input_pairs.PT_INPUTS, pri * 1000, tri + 273.15);
            double hri = coolprop.hmass() / 1000;
            //double hri = CoolProp.PropsSI("H", "T", tri + 273.15, "P", pri * 1000, fluid) / 1000;
            //double hri = 354.6;
            //double xin = 0.57;

            double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
            double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

            //string AirDirection="DowntoUp";
            string AirDirection = "逆流";
            ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai, tc, AirDirection);
            RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi, tc, AirDirection);


            //AreaResult geo = new AreaResult();
            //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, tri, pri, hri,
                //mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, tri, pri, hri,
                mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap, coolprop);


            return res;
        }
        public static CalcResult main_condenser_YH200_2()
        {
            //制冷剂制热模块计算
            //string fluid = new string[] { "Water" };
            string fluid = "R32";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            //string fluid = new string[] { "ISOBUTAN" };
            CalcResult r0 = new CalcResult();
            r0 = main_condenser_YH200_1();
            CalcResult res = new CalcResult();
            int Nrow = 2;
            //double[] FPI = new double[Nrow + 1];
            //FPI = new double[] { 1.27, 1.27, 1.27, 1.27, 1.27, 1.27, 1.27, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 5.2, 5.2, 5.2, 5.2, 5.2, 5.2 };
            double[] FPI = new double[] { 19.538, 19.538 };
            double Pt = 21 * 0.001;
            double Pr = 22 * 0.001;
            double Di = 6.8944 * 0.001;//8 6.8944
            double Do = 7.35 * 0.001;//8.4 7.35
            double Fthickness = 0.095 * 0.001;
            double thickness = 0.5 * (Do - Di);
            //double n_tubes = 10;
            //double n_rows = 2;
            //int[] Ntube = { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
            //int[] Ntube = { 2, 2, 2, 2 };
            int[] Ntube = { 8, 8 };//{ 4, 4 };
            int N_tube = Ntube[0];
            double L = (825 + 860) / 2 * 0.001;
            int Nelement = 10;
            int[,] CirArrange;
            //CirArrange = new int[,] { { 6, 5, 1, 2 }, { 7, 8, 4, 3 } };
            CirArrange = new int[,] { { 12, 11, 10, 9, 1, 2, 3, 4 }, { 16, 15, 14, 13, 8, 7, 6, 5 } };
            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { 2, 2 };
            CircuitInfo.TubeofCir = new int[] { 8, 8 };//{ 4, 4 }
            //CircuitInfo.UnequalCir = new int[] { 5, 5, 5, 5, 0 };//{ 5, 5, 6, 6, 0, 0 };
            // [19 - 17 - 15 - 13   11   9   7   5   3   1] <====Air
            // [20 - 18 - 16 - 14   12   10  8   6   4   2] <====Air
            //  Ncir=1, 20in, 20->19 1out

            double[] d_cap = new double[] {0, 0};
            double[] lenth_cap = new double[] {0, 0};

            GeometryInput geoInput_air = new GeometryInput();
            geoInput_air.Pt = Pt;
            geoInput_air.Pr = Pr;
            geoInput_air.Do = Do;
            geoInput_air.Fthickness = Fthickness;
            geoInput_air.FPI = FPI[0];
            geoInput_air.Nrow = Nrow;

            int hexType = 1;

            double mr = r0.mr;
            //double Vel_a = 1.8; //m/s
            double H = Pt * N_tube;
            double Hx = L * H;
            double rho_a_st = 1.2; //kg/m3
            double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
            double Va = 2000 * 8 / 24;//2000*4/24;// m3/h
            double Vel_ave = Va / 3600 / Hx;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
            AirDistribution VaDistri = new AirDistribution();
            VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
            double[,] ma = new double[N_tube, Nelement];
            double[,] ha = new double[N_tube, Nelement];


            //空气侧几何结构选择
            //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
            //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
            int curve = 1; //

            double za = 1; //Adjust factor
            for (int i = 0; i < N_tube; i++)
            {
                for (int j = 0; j < Nelement; j++)
                {
                    ma[i, j] = VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st;
                    //ha[i, j] = AirHTC.alpha(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve);// *1.5;
                    //ha[i, j] = 79;
                    ha[i, j] = AirHTC.alpha1(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve, geoInput_air, hexType).ha;
                    //ha[i, j] = 36.5;
                }
            }
            double[,] haw = ha;

            res.DPa = AirHTC.alpha1(Vel_ave, za, curve, geoInput_air, hexType).dP_a;

            double eta_surface = 0.8;
            double zh = 1.3;
            double zdp = 2;

            double tai = 35;
            double RHi = CoolProp.HAPropsSI("R", "T", tai + 273.15, "P", 101325, "B", 24 + 273.15);//0.469
            double tc = 50.775;
            //double pri = Refrigerant.SATT(fluid, composition, tc + 273.15, 1).Pressure;

            //double pri = CoolProp.PropsSI("P", "T", tc + 273.15, "Q", 0, fluid) / 1000;
            double pri = r0.Pro;
            //double P_exv = 1842.28;//kpa
            //double tri = 78;//C
            double tri = r0.Tro;
            double conductivity = 386; //w/mK for Cu
            double Pwater = 100.0;
            //int hexType = 1; //*********************************0 is evap, 1 is cond******************************************
            //double hri = CoolProp.PropsSI("H", "T", tri + 273.15, "P", pri * 1000, fluid) / 1000;
            double hri = r0.hro;
            //double hri = 354.6;
            //double xin = 0.57;

            double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
            double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

            //string AirDirection="DowntoUp";
            string AirDirection = "逆流";
            ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai, tc, AirDirection);
            RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi, tc, AirDirection);


            //AreaResult geo = new AreaResult();
            //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, tri, pri, hri,
                //mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, tri, pri, hri,
                mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap, coolprop);
            

            res.Q = res.Q + r0.Q;
            res.DP = res.DP + r0.DP;
            return res;
        }
        public static CalcResult main_evaporator_YH200()
        {
            string fluid = "R32";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            //string fluid = new string { "R410A.mix" };
            //string fluid = new string[] { "ISOBUTAN" };
            CalcResult res = new CalcResult();
            int Nrow = 2;
            double[] FPI = new double[Nrow + 1];
            //FPI = new double[] { 1.27, 1.27, 1.27, 1.27, 1.27, 1.27, 1.27, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 5.2, 5.2, 5.2, 5.2, 5.2, 5.2 };
            FPI = new double[] { 19.538, 19.538 };
            double Pt = 21 * 0.001;
            double Pr = 22 * 0.001;
            double Di = 6.8944 * 0.001;//8 6.8944
            double Do = 7.35 * 0.001;//8.4 7.35
            double Fthickness = 0.095 * 0.001;
            double thickness = 0.5 * (Do - Di);
            //double n_tubes = 10;
            //double n_rows = 2;
            //int[] Ntube = { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
            //int[] Ntube = { 2, 2, 2, 2 };
            int[] Ntube = { 24, 24 };
            int N_tube = Ntube[0];
            double L = (825 + 860) / 2 * 0.001;
            int Nelement =5;
            int[,] CirArrange;
            //CirArrange = new int[,] { { 48, 47, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 24, 23, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 46, 45, 44, 43, 18, 17, 16, 15, 14, 13, 0, 0 }, { 22, 21, 20, 19, 42, 41, 40, 39, 38, 37, 0, 0 }, { 36, 35, 34, 33, 32, 31, 6, 5, 4, 3, 2, 1, }, { 12, 11, 10, 9, 8, 7, 30, 29, 28, 27, 26, 25 } };
            //CirArrange = new int[,] { { 21, 22, 23, 24, 48, 47, 46, 45 }, { 20, 19, 18, 17, 41, 42, 43, 44 }, { 16, 15, 14, 13, 40, 39, 38, 37 }, { 12, 11, 10, 9, 33, 34, 35, 36 }, { 8, 7, 6, 5, 32, 31, 30, 29 }, { 4, 3, 2, 1, 25, 26, 27, 28 } };
            //CirArrange = new int[,] { { 8, 7, 6, 5, 4, 3, 2, 1, 25, 26, 27, 28, 29, 30, 31, 32 }, { 16, 15, 14, 13, 12, 11, 10, 9, 33, 34, 35, 36, 37, 38, 39, 40 }, { 17, 18, 19, 20, 21, 22, 23, 24, 41, 42, 43, 44, 45, 46, 47, 48 } };
            //CirArrange = new int[,] { { 23, 24, 48, 47, 0, 0, 0, 0, 0, 0, 0, 0 }, { 46, 45, 44, 43, 18, 17, 16, 15, 14, 13, 0, 0 }, { 22, 21, 20, 19, 42, 41, 40, 39, 38, 37, 0, 0 }, { 36, 35, 34, 33, 32, 31, 6, 5, 4, 3, 2, 1, }, { 12, 11, 10, 9, 8, 7, 30, 39, 28, 27, 26, 25 } };
            //CirArrange = new int[,] { { 1, 2, 3, 4, 5, 6, 30, 29, 28, 27, 26, 25 }, { 12, 11, 10, 9, 8, 7, 31, 32, 33, 34, 35, 36 }, { 13, 14, 15, 16, 17, 18, 42, 41, 40, 39, 38, 37 }, { 24, 23, 22, 21, 20, 19, 43, 44, 45, 46, 47, 48 } };          

            //CirArrange = new int[,] { { 25, 26, 27, 28, 29, 30, 7, 8, 9, 10, 11, 12 }, { 1, 2, 3, 4, 5, 6, 31, 32, 33, 34, 35, 36 }, { 37, 38, 39, 40, 41, 42, 19, 20, 21, 22, 0, 0 }, { 13, 14, 15, 16, 17, 18, 43, 44, 45, 46, 0, 0 }, { 23, 24, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 47, 48, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } };
            //CirArrange = new int[,] { { 28, 27, 26, 25, 1, 2, 3, 4 }, { 29, 30, 31, 32, 5, 6, 7, 8 }, { 36, 35, 34, 33, 9, 10, 11, 12 }, { 37, 38, 39, 40, 13, 14, 15, 16 }, { 44, 43, 42, 41, 17, 18, 19, 20 }, { 45, 46, 47, 48, 24, 23, 22, 21 } };
            //CirArrange = new int[,] { { 30, 29, 28, 27, 26, 25, 1, 2, 3, 4 }, { 34, 33, 32, 31, 5, 6, 7, 8, 9, 10 }, { 40, 39, 38, 37, 36, 35, 11, 12, 13, 14 }, { 44, 43, 42, 41, 15, 16, 17, 18, 19, 20 }, { 46, 45, 21, 22, 0, 0, 0, 0, 0, 0 }, { 47, 48, 24, 23, 0, 0, 0, 0, 0, 0 } };            
            //CirArrange = new int[,] { { 32, 31, 30, 29, 28, 27, 26, 25, 1, 2, 3, 4, 5, 6, 7, 8 }, { 40, 39, 38, 37, 36, 35, 34, 33, 9, 10, 11, 12, 13, 14, 15, 16 }, { 48, 47, 46, 45, 44, 43, 42, 41, 24, 23, 22, 21, 20, 19, 18, 17 } };
            //CirArrange = new int[,] { { 25, 26, 27, 28, 29, 30, 7, 8, 9, 10, 11, 12 }, { 1, 2, 3, 4, 5, 6, 31, 32, 33, 34, 35, 36 }, { 37, 38, 39, 40, 41, 42, 19, 20, 21, 22, 0, 0 }, { 13, 14, 15, 16, 17, 18, 43, 44, 45, 46, 0, 0 }, { 47, 48, 24, 23, 0, 0, 0, 0, 0, 0, 0, 0 } };
            //CirArrange = new int[,] { { 25, 26, 27, 28, 29, 30, 6, 5, 4, 3, 2, 1 }, { 36, 35, 34, 33, 32, 31, 7, 8, 9, 10, 11, 12 }, { 37, 38, 39, 40, 41, 42, 18, 17, 16, 15, 14, 13 }, { 48, 47, 46, 45, 44, 43, 19, 20, 21, 22, 23, 24 } };
            //CirArrange = new int[,] { { 25, 26, 27, 28, 4, 3, 2, 1 }, { 32, 31, 30, 29, 5, 6, 7, 8 }, { 33, 34, 35, 36, 12, 11, 10, 9 }, { 40, 39, 38, 37, 13, 14, 15, 16 }, { 44, 43, 42, 41, 17, 18, 19, 20 }, { 45, 46, 47, 48, 24, 23, 22, 21 } };            

            CirArrange = new int[,] { { 12, 11, 10, 9, 8, 7, 30, 29, 28, 27, 26, 25 }, { 36, 35, 34, 33, 32, 31, 6, 5, 4, 3, 2, 1 }, { 22, 21, 20, 19, 42, 41, 40, 39, 38, 37, 0, 0 }, { 46, 45, 44, 43, 18, 17, 16, 15, 14, 13, 0, 0 }, { 24, 23, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 48, 47, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } };
            //CirArrange = new int[,] { { 4, 3, 2, 1, 25, 26, 27, 28 }, { 8, 7, 6, 5, 32, 31, 30, 29 }, { 12, 11, 10, 9, 33, 34, 35, 36 }, { 16, 15, 14, 13, 40, 39, 38, 37 }, { 20, 19, 18, 17, 41, 42, 43, 44 }, { 21, 22, 23, 24, 48, 47, 46, 45 } };
            //CirArrange = new int[,] { { 4, 3, 2, 1, 25, 26, 27, 28, 29, 30 }, { 10, 9, 8, 7, 6, 5, 31, 32, 33, 34 }, { 14, 13, 12, 11, 35, 36, 37, 38, 39, 40 }, { 20, 19, 18, 17, 16, 15, 41, 42, 43, 44 }, { 22, 21, 45, 49, 0, 0, 0, 0, 0, 0 }, { 23, 24, 48, 47, 0, 0, 0, 0, 0, 0 } };            
            //CirArrange = new int[,] { { 8, 7, 6, 5, 4, 3, 2, 1, 25, 26, 27, 28, 29, 30, 31, 32 }, { 16, 15, 14, 13, 12, 11, 10, 9, 33, 34, 35, 36, 37, 38, 39, 40 }, { 17, 18, 19, 20, 21, 22, 23, 24, 41, 42, 43, 44, 45, 46, 47, 48 } };
            //CirArrange = new int[,] { { 12, 11, 10, 9, 8, 7, 30, 29, 28, 27, 26, 25 }, { 36, 35, 34, 33, 32, 31, 6, 5, 4, 3, 2, 1 }, { 22, 21, 20, 19, 42, 41, 40, 39, 38, 37, 0, 0 }, { 46, 45, 44, 43, 18, 17, 16, 15, 14, 13, 0, 0 }, { 23, 24, 48, 47, 0, 0, 0, 0, 0, 0, 0, 0 } };
            //CirArrange = new int[,] { { 1, 2, 3, 4, 5, 6, 30, 29, 28, 27, 26, 25 }, { 12, 11, 10, 9, 8, 7, 31, 32, 33, 34, 35, 36 }, { 13, 14, 15, 16, 17, 18, 42, 41, 40, 39, 38, 37 }, { 24, 23, 22, 21, 20, 19, 43, 44, 45, 46, 47, 48 } };
            //CirArrange = new int[,] { { 1, 2, 3, 4, 28, 27, 26, 25 }, { 8, 7, 6, 5, 29, 30, 31, 32 }, { 9, 10, 11, 12, 36, 35, 34, 33 }, { 16, 15, 14, 13, 37, 38, 39, 40 }, { 20, 19, 18, 17, 41, 42, 43, 44 }, { 21, 22, 23, 24, 48, 47, 46, 45 } };          
                        
            CircuitNumber CircuitInfo = new CircuitNumber();
            //CircuitInfo.number = new int[] { 4, 2 };//{ 4, 1 };//{ 3, 3 }; //{ 4, 2 }//{4,2}//{4,4}
            CircuitInfo.TubeofCir = new int[] { 12, 12, 10, 10, 2, 2 };//{ 12, 12, 10, 10, 4 };//{ 16,16,16 };  //{ 8,8,8,8,8,8 };//{12,12,10,10,2,2}//{ 12, 12, 12, 12 }
            CircuitInfo.UnequalCir = new int[] { -5, -6, 5, 5, 6, 6 };//{ 5, 5, 5, 5, 0 };//{ 5, 5, 6, 6, 0, 0 };//{5,5,6,6,0,0}
            // [19 - 17 - 15 - 13   11   9   7   5   3   1] <====Air
            // [20 - 18 - 16 - 14   12   10  8   6   4   2] <====Air
            //  Ncir=1, 20in, 20->19 1out

            //bool reverse = true; //*********************************false:origin, true:reverse******************************************
            //CirArrange = CircuitReverse.CirReverse(reverse, CirArrange, CircuitInfo);
            //4--2 
            /*int[,] Node_in = { { -1, -100 }, { 0, -100 }, { 0, -100 }, { 0, 1 }, { 2, 3 }, { 4, 5 } };
            int[,] Node_inType = { { -1, -100 }, { 0, -100 }, { 0, -100 }, { 1, 1 }, { 1, 1 }, { 1, 1 } };//0:Node,1:Circuit,-1:in/out,-100:meaningless
            int[] Node_Nin = { 1, 1, 1, 2, 2, 2 };
            int[,] Node_out = { { 1, 2 }, { 0, 1 }, { 2, 3 }, { 4, -100 }, { 5, -100 }, { -1, -100 } };
            int[,] Node_outType = { { 0, 0 }, { 1, 1 }, { 1, 1 }, { 1, -100 }, { 1, -100 }, { -1, -100 } };
            int[] Node_Nout = { 2, 2, 2, 1, 1, 1 };
            char[] Node_type = { 'D', 'D', 'D', 'C', 'C', 'C' };// Diverse/Converge
            int[] Node_couple = { 0, 1, 2, 1, 2, 0 };*/

            //2--4 
            int[,] Node_out = { { -1, -100 }, { 0, -100 }, { 0, -100 }, { 0, 1 }, { 2, 3 }, { 4, 5 } };
            int[,] Node_outType = { { -1, -100 }, { 0, -100 }, { 0, -100 }, { 1, 1 }, { 1, 1 }, { 1, 1 } };//0:Node,1:Circuit,-1:in/out,-100:meaningless
            int[] Node_Nout = { 1, 1, 1, 2, 2, 2 };
            int[,] Node_in = { { 1, 2 }, { 0, 1 }, { 2, 3 }, { 4, -100 }, { 5, -100 }, { -1, -100 } };
            int[,] Node_inType = { { 0, 0 }, { 1, 1 }, { 1, 1 }, { 1, -100 }, { 1, -100 }, { -1, -100 } };
            int[] Node_Nin = { 2, 2, 2, 1, 1, 1 };
            char[] Node_type = { 'C', 'C', 'C', 'D', 'D', 'D' };// Diverse/Converge
            int[] Node_couple = { 0, 1, 2, 1, 2, 0 };

            int N_Node = Node_in.GetLength(0);
            NodeInfo[] Nodes = new NodeInfo[N_Node];
            for (int i = 0; i < N_Node; i++)
            {
                Nodes[i] = new NodeInfo();
                Nodes[i].N_in = Node_Nin[i];
                Nodes[i].N_out = Node_Nout[i];
                Nodes[i].inlet = new int[Nodes[i].N_in];
                Nodes[i].inType = new int[Nodes[i].N_in];
                Nodes[i].outlet = new int[Nodes[i].N_out];
                Nodes[i].outType = new int[Nodes[i].N_out];
                for (int j = 0; j < Nodes[i].N_in; j++)
                {
                    Nodes[i].inlet[j] = Node_in[i, j];
                    Nodes[i].inType[j] = Node_inType[i, j];
                }
                for (int j = 0; j < Nodes[i].N_out; j++)
                {
                    Nodes[i].outlet[j] = Node_out[i, j];
                    Nodes[i].outType[j] = Node_outType[i, j];
                }
                Nodes[i].type = Node_type[i];
                Nodes[i].couple = Node_couple[i];
                Nodes[i].mri = new double[Nodes[i].N_in];
                Nodes[i].mro = new double[Nodes[i].N_out];
                Nodes[i].pri = new double[Nodes[i].N_in];
                Nodes[i].pro = new double[Nodes[i].N_out];
                Nodes[i].hri = new double[Nodes[i].N_in];
                Nodes[i].hro = new double[Nodes[i].N_out];
                Nodes[i].tri = new double[Nodes[i].N_in];
                Nodes[i].tro = new double[Nodes[i].N_out];
                Nodes[i].mr_ratio = new double[Nodes[i].N_out];
            }
            
            double[] d_cap = new double[6];
            double[] lenth_cap = new double[6];

            double mr = 0.026;
            //double Vel_a = 1.8; //m/s
            double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
            double H = Pt * N_tube;
            double Hx = L * H;
            double rho_a_st = 1.2; //kg/m3
            double Va = 2000;
            double Vel_ave = Va/3600/Hx;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
            AirDistribution VaDistri = new AirDistribution();
            VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
            double[,] ma = new double[N_tube, Nelement];
            double[,] ha = new double[N_tube, Nelement];


            int hexType = 0;

            GeometryInput geoInput_air = new GeometryInput();
            geoInput_air.Pt = Pt;
            geoInput_air.Pr = Pr;
            geoInput_air.Do = Do;
            geoInput_air.Fthickness = Fthickness;
            geoInput_air.FPI = FPI[0];
            geoInput_air.Nrow = Nrow;

            //空气侧几何结构选择
            //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
            //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
            int curve = 1; //
            AirCoef_res res1 = new AirCoef_res();
            double za = 1; //Adjust factor
            for (int i = 0; i < N_tube; i++)
            {
                for (int j = 0; j < Nelement; j++)
                {
                    ma[i, j] = VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st;
                    //ha[i, j] = AirHTC.alpha(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve) * 1.5;
                    //ha[i, j] = 79;
                    ha[i, j] = AirHTC.alpha1(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve, geoInput_air, hexType).ha;
                    //ha[i, j] = 36.5;
                }
            }
            double[,] haw = ha;

            res.DPa = AirHTC.alpha1(Vel_ave, za, curve, geoInput_air, hexType).dP_a;

            double eta_surface = 0.8;
            double zh = 3;
            double zdp = 1.8;

            double tai = 7;
            double RHi = CoolProp.HAPropsSI("R","T",tai+273.15,"P",101325,"B",6+273.15);
            double tri =-0.3;
            double te = tri;

            coolprop.update(input_pairs.QT_INPUTS, 0, te + 273.15);
            double pe = coolprop.p() / 1000;
            //double pe = CoolProp.PropsSI("P", "T", te + 273.15, "Q", 0, fluid) / 1000;

            double P_exv = 3142.28;//kpa
            double T_exv = 36;//C
            double conductivity = 386; //w/mK for Cu
            double Pwater = 0;
            //int hexType = 0; //*********************************0 is evap, 1 is cond******************************************
            double Tc = 50;
            double Sc = 14;
            coolprop.update(input_pairs.QT_INPUTS, 0, Tc + 273.15);
            double Pc = coolprop.p() / 1000;
            //double Pc = CoolProp.PropsSI("P", "T", Tc + 273.15, "Q", 0, fluid) / 1000;
            coolprop.update(input_pairs.PT_INPUTS, Pc * 1000, Tc - Sc + 273.15);
            double hri = coolprop.hmass() / 1000;
            //double hri = CoolProp.PropsSI("H", "T", Tc-Sc + 273.15, "P", Pc * 1000, fluid) / 1000;

            double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
            double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

            //string AirDirection="DowntoUp";
            string AirDirection = "逆流";
            ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai, te, AirDirection);
            RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi, te, AirDirection);


            //AreaResult geo = new AreaResult();
            //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, te, pe, hri,
                //mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, te, pe, hri,
            //    mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap, coolprop);
            res = Slab2.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, L, geo, ta, RH, tri, pe, hri,
                    mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, Nodes, N_Node, d_cap, lenth_cap, coolprop);           
            
            return res;
        }
        public static CalcResult main_evaporator_YH200_1()
        {
            string fluid = "R32";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            //string fluid = new string { "R410A.mix" };
            //string fluid = new string[] { "ISOBUTAN" };
            CalcResult res = new CalcResult();
            int Nrow = 2;
            double[] FPI = new double[Nrow + 1];
            //FPI = new double[] { 1.27, 1.27, 1.27, 1.27, 1.27, 1.27, 1.27, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 5.2, 5.2, 5.2, 5.2, 5.2, 5.2 };
            FPI = new double[] { 19.538, 19.538 };
            double Pt = 21 * 0.001;
            double Pr = 22 * 0.001;
            double Di = 6.8944 * 0.001;//8 6.8944
            double Do = 7.35 * 0.001;//8.4 7.35
            double Fthickness = 0.095 * 0.001;
            double thickness = 0.5 * (Do - Di);
            //double n_tubes = 10;
            //double n_rows = 2;
            //int[] Ntube = { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
            //int[] Ntube = { 2, 2, 2, 2 };
            int[] Ntube = { 4, 4 };
            int N_tube = Ntube[0];
            double L = (825 + 860) / 2 * 0.001;
            int Nelement = 5;
            int[,] CirArrange;
            //CirArrange = new int[,] { { 48, 47, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 24, 23, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 46, 45, 44, 43, 18, 17, 16, 15, 14, 13, 0, 0 }, { 22, 21, 20, 19, 42, 41, 40, 39, 38, 37, 0, 0 }, { 36, 35, 34, 33, 32, 31, 6, 5, 4, 3, 2, 1, }, { 12, 11, 10, 9, 8, 7, 30, 29, 28, 27, 26, 25 } };
            //CirArrange = new int[,] { { 21, 22, 23, 24, 48, 47, 46, 45 }, { 20, 19, 18, 17, 41, 42, 43, 44 }, { 16, 15, 14, 13, 40, 39, 38, 37 }, { 12, 11, 10, 9, 33, 34, 35, 36 }, { 8, 7, 6, 5, 32, 31, 30, 29 }, { 4, 3, 2, 1, 25, 26, 27, 28 } };
            //CirArrange = new int[,] { { 8, 7, 6, 5, 4, 3, 2, 1, 25, 26, 27, 28, 29, 30, 31, 32 }, { 16, 15, 14, 13, 12, 11, 10, 9, 33, 34, 35, 36, 37, 38, 39, 40 }, { 17, 18, 19, 20, 21, 22, 23, 24, 41, 42, 43, 44, 45, 46, 47, 48 } };
            //CirArrange = new int[,] { { 23, 24, 48, 47, 0, 0, 0, 0, 0, 0, 0, 0 }, { 46, 45, 44, 43, 18, 17, 16, 15, 14, 13, 0, 0 }, { 22, 21, 20, 19, 42, 41, 40, 39, 38, 37, 0, 0 }, { 36, 35, 34, 33, 32, 31, 6, 5, 4, 3, 2, 1, }, { 12, 11, 10, 9, 8, 7, 30, 39, 28, 27, 26, 25 } };
            CirArrange = new int[,] { { 2, 1, 5, 6 }, { 3, 4, 8, 7 } };
            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { 2, 2 };//{ 4, 1 };//{ 3, 3 }; //{ 4, 2 }//{4,2}
            CircuitInfo.TubeofCir = new int[] { 4,4 };//{ 12, 12, 10, 10, 4 };//{ 16,16,16 };  //{ 8,8,8,8,8,8 };//{12,12,10,10,2,2}
            //CircuitInfo.UnequalCir = new int[] { -5, 5, 5, 5, 5 };//{ 5, 5, 5, 5, 0 };//{ 5, 5, 6, 6, 0, 0 };//{5,5,6,6,0,0}
            // [19 - 17 - 15 - 13   11   9   7   5   3   1] <====Air
            // [20 - 18 - 16 - 14   12   10  8   6   4   2] <====Air
            //  Ncir=1, 20in, 20->19 1out

            //bool reverse = true; //*********************************false:origin, true:reverse******************************************
            //CirArrange = CircuitReverse.CirReverse(reverse, CirArrange, CircuitInfo);

            double[] d_cap = new double[] { 0, 0 };
            double[] lenth_cap = new double[] { 0, 0 };

            double mr = 0.017;
            //double Vel_a = 1.8; //m/s
            double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
            double H = Pt * N_tube;
            double Hx = L * H;
            double rho_a_st = 1.2; //kg/m3
            double Va = 2160*4/24;
            double Vel_ave = Va / 3600 / Hx;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
            AirDistribution VaDistri = new AirDistribution();
            VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
            double[,] ma = new double[N_tube, Nelement];
            double[,] ha = new double[N_tube, Nelement];


            int hexType = 0;

            GeometryInput geoInput_air = new GeometryInput();
            geoInput_air.Pt = Pt;
            geoInput_air.Pr = Pr;
            geoInput_air.Do = Do;
            geoInput_air.Fthickness = Fthickness;
            geoInput_air.FPI = FPI[0];
            geoInput_air.Nrow = Nrow;

            //空气侧几何结构选择
            //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
            //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
            int curve = 1; //
            AirCoef_res res1 = new AirCoef_res();
            double za = 1; //Adjust factor
            for (int i = 0; i < N_tube; i++)
            {
                for (int j = 0; j < Nelement; j++)
                {
                    ma[i, j] = VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st;
                    //ha[i, j] = AirHTC.alpha(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve) * 1.5;
                    //ha[i, j] = 79;
                    ha[i, j] = AirHTC.alpha1(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve, geoInput_air, hexType).ha;
                    //ha[i, j] = 36.5;
                }
            }
            double[,] haw = ha;

            res.DP = AirHTC.alpha1(Vel_ave, za, curve, geoInput_air, hexType).dP_a;

            double eta_surface = 0.8;
            double zh = 1.5;
            double zdp = 1.8;

            double tai = 7;
            double RHi = CoolProp.HAPropsSI("R", "T", tai + 273.15, "P", 101325, "B", 6 + 273.15);
            double tri = -0.3;
            double te = tri;

            coolprop.update(input_pairs.QT_INPUTS, 0, te + 273.15);
            double pe = coolprop.p() / 1000;
            //double pe = CoolProp.PropsSI("P", "T", te + 273.15, "Q", 0, fluid) / 1000;

            double P_exv = 3142.28;//kpa
            double T_exv = 36;//C
            double conductivity = 386; //w/mK for Cu
            double Pwater = 0;
            //int hexType = 0; //*********************************0 is evap, 1 is cond******************************************
            double Tc = 50;
            double Sc = 14;
            coolprop.update(input_pairs.QT_INPUTS, 0, Tc + 273.15);
            double Pc = coolprop.p() / 1000;
            //double Pc = CoolProp.PropsSI("P", "T", Tc + 273.15, "Q", 0, fluid) / 1000;
            double hri = CoolProp.PropsSI("H", "T", Tc - Sc + 273.15, "P", Pc * 1000, fluid) / 1000;
            double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
            double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

            //string AirDirection="DowntoUp";
            string AirDirection = "逆流";
            ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai, te, AirDirection);
            RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi, te, AirDirection);


            //AreaResult geo = new AreaResult();
            //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, te, pe, hri,
                //mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, te, pe, hri,
                mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap, coolprop);

            return res;
        }
        public static CalcResult main_evaporator_YH200_2()
        {
            CalcResult r0 = new CalcResult();
            r0 = main_evaporator_YH200_1();
            string fluid = "R32";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            //string fluid = new string { "R410A.mix" };
            //string fluid = new string[] { "ISOBUTAN" };
            CalcResult res = new CalcResult();
            int Nrow = 2;
            double[] FPI = new double[Nrow + 1];
            //FPI = new double[] { 1.27, 1.27, 1.27, 1.27, 1.27, 1.27, 1.27, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 5.2, 5.2, 5.2, 5.2, 5.2, 5.2 };
            FPI = new double[] { 19.538, 19.538 };
            double Pt = 21 * 0.001;
            double Pr = 22 * 0.001;
            double Di = 6.8944 * 0.001;//8 6.8944
            double Do = 7.35 * 0.001;//8.4 7.35
            double Fthickness = 0.095 * 0.001;
            double thickness = 0.5 * (Do - Di);
            //double n_tubes = 10;
            //double n_rows = 2;
            //int[] Ntube = { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
            //int[] Ntube = { 2, 2, 2, 2 };
            int[] Ntube = { 20, 20 };
            int N_tube = Ntube[0];
            double L = (825 + 860) / 2 * 0.001;
            int Nelement = 5;
            int[,] CirArrange;
            //CirArrange = new int[,] { { 48, 47, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 24, 23, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 46, 45, 44, 43, 18, 17, 16, 15, 14, 13, 0, 0 }, { 22, 21, 20, 19, 42, 41, 40, 39, 38, 37, 0, 0 }, { 36, 35, 34, 33, 32, 31, 6, 5, 4, 3, 2, 1, }, { 12, 11, 10, 9, 8, 7, 30, 29, 28, 27, 26, 25 } };
            //CirArrange = new int[,] { { 21, 22, 23, 24, 48, 47, 46, 45 }, { 20, 19, 18, 17, 41, 42, 43, 44 }, { 16, 15, 14, 13, 40, 39, 38, 37 }, { 12, 11, 10, 9, 33, 34, 35, 36 }, { 8, 7, 6, 5, 32, 31, 30, 29 }, { 4, 3, 2, 1, 25, 26, 27, 28 } };
            //CirArrange = new int[,] { { 8, 7, 6, 5, 4, 3, 2, 1, 25, 26, 27, 28, 29, 30, 31, 32 }, { 16, 15, 14, 13, 12, 11, 10, 9, 33, 34, 35, 36, 37, 38, 39, 40 }, { 17, 18, 19, 20, 21, 22, 23, 24, 41, 42, 43, 44, 45, 46, 47, 48 } };
            //CirArrange = new int[,] { { 23, 24, 48, 47, 0, 0, 0, 0, 0, 0, 0, 0 }, { 46, 45, 44, 43, 18, 17, 16, 15, 14, 13, 0, 0 }, { 22, 21, 20, 19, 42, 41, 40, 39, 38, 37, 0, 0 }, { 36, 35, 34, 33, 32, 31, 6, 5, 4, 3, 2, 1, }, { 12, 11, 10, 9, 8, 7, 30, 39, 28, 27, 26, 25 } };
            //CirArrange = new int[,] { { 26, 25, 24, 23, 22, 21, 1, 2, 3, 4 }, { 30, 29, 28, 27, 5, 6, 7, 8, 9, 10 }, { 36, 35, 34, 33, 32, 31, 11, 12, 13, 14 }, { 40, 39, 38, 37, 15, 16, 17, 18, 19, 20 } };
            CirArrange = new int[,] { { 4, 3, 2, 1, 21, 22, 23, 24, 25, 26 }, { 10, 9, 8, 7, 6, 5, 27, 28, 29, 30 }, { 14, 13, 12, 11, 31, 32, 33, 34, 35, 36 }, { 20, 19, 18, 17, 16, 15, 37, 38, 39, 40 } };
            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { 4, 4 };//{ 4, 1 };//{ 3, 3 }; //{ 4, 2 }//{4,2}
            CircuitInfo.TubeofCir = new int[] { 10, 10, 10, 10 };//{ 12, 12, 10, 10, 4 };//{ 16,16,16 };  //{ 8,8,8,8,8,8 };//{12,12,10,10,2,2}
            //CircuitInfo.UnequalCir = new int[] { -5, 5, 5, 5, 5 };//{ 5, 5, 5, 5, 0 };//{ 5, 5, 6, 6, 0, 0 };//{5,5,6,6,0,0}
            // [19 - 17 - 15 - 13   11   9   7   5   3   1] <====Air
            // [20 - 18 - 16 - 14   12   10  8   6   4   2] <====Air
            //  Ncir=1, 20in, 20->19 1out

            //bool reverse = true; //*********************************false:origin, true:reverse******************************************
            //CirArrange = CircuitReverse.CirReverse(reverse, CirArrange, CircuitInfo);

            double[] d_cap = new double[] { 0, 0, 0, 0 };
            double[] lenth_cap = new double[] { 0, 0, 0, 0 };

            double mr = r0.mr;
            //double Vel_a = 1.8; //m/s
            double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
            double H = Pt * N_tube;
            double Hx = L * H;
            double rho_a_st = 1.2; //kg/m3
            double Va = 2160*20/24;
            double Vel_ave = Va / 3600 / Hx;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
            AirDistribution VaDistri = new AirDistribution();
            VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
            double[,] ma = new double[N_tube, Nelement];
            double[,] ha = new double[N_tube, Nelement];


            int hexType = 0;

            GeometryInput geoInput_air = new GeometryInput();
            geoInput_air.Pt = Pt;
            geoInput_air.Pr = Pr;
            geoInput_air.Do = Do;
            geoInput_air.Fthickness = Fthickness;
            geoInput_air.FPI = FPI[0];
            geoInput_air.Nrow = Nrow;

            //空气侧几何结构选择
            //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
            //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
            int curve = 1; //
            AirCoef_res res1 = new AirCoef_res();
            double za = 1; //Adjust factor
            for (int i = 0; i < N_tube; i++)
            {
                for (int j = 0; j < Nelement; j++)
                {
                    ma[i, j] = VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st;
                    //ha[i, j] = AirHTC.alpha(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve) * 1.5;
                    //ha[i, j] = 79;
                    ha[i, j] = AirHTC.alpha1(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve, geoInput_air, hexType).ha;
                    //ha[i, j] = 36.5;
                }
            }
            double[,] haw = ha;

            res.DP = AirHTC.alpha1(Vel_ave, za, curve, geoInput_air, hexType).dP_a;

            double eta_surface = 0.8;
            double zh = 1.5;
            double zdp = 1.8;

            double tai = 7;
            double RHi = CoolProp.HAPropsSI("R", "T", tai + 273.15, "P", 101325, "B", 6 + 273.15);
            double tri = r0.Tro;
            double te = -0.3;
            coolprop.update(input_pairs.QT_INPUTS, 0, te + 273.15);
            double pe = coolprop.p() / 1000;
            //double pe = CoolProp.PropsSI("P", "T", te + 273.15, "Q", 0, fluid) / 1000;

            double P_exv = 3142.28;//kpa
            double T_exv = 36;//C
            double conductivity = 386; //w/mK for Cu
            double Pwater = 0;
            //int hexType = 0; //*********************************0 is evap, 1 is cond******************************************
            double Tc = 50;
            double Sc = 14;
            coolprop.update(input_pairs.QT_INPUTS, 0, Tc + 273.15);
            double Pc = coolprop.p() / 1000;
            //double Pc = CoolProp.PropsSI("P", "T", Tc + 273.15, "Q", 0, fluid) / 1000;
            coolprop.update(input_pairs.PT_INPUTS, Pc * 1000, Tc - Sc + 273.15);
            double hri = coolprop.hmass() / 1000;
            //double hri = CoolProp.PropsSI("H", "T", Tc - Sc + 273.15, "P", Pc * 1000, fluid) / 1000;
            double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
            double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

            //string AirDirection="DowntoUp";
            string AirDirection = "逆流";
            ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai, te, AirDirection);
            RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi, te, AirDirection);


            //AreaResult geo = new AreaResult();
            //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, te, pe, hri,
                //mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, te, pe, hri,
                mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap, coolprop);

            return res;
        }
        public static CalcResult main_condenser_35()
        {
            //制冷剂制热模块计算
            //string fluid = new string[] { "Water" };
            string fluid = "R32";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            //string fluid = new string[] { "ISOBUTAN" };
            CalcResult res = new CalcResult();
            int Nrow = 1;
            //double[] FPI = new double[Nrow + 1];
            //FPI = new double[] { 1.27, 1.27, 1.27, 1.27, 1.27, 1.27, 1.27, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 5.2, 5.2, 5.2, 5.2, 5.2, 5.2 };
            double[] FPI = new double[] { 19.538 };
            double Pt = 21 * 0.001;
            double Pr = 22 * 0.001;
            double Di = 6.8944 * 0.001;//8 6.8944
            double Do = 7.35 * 0.001;//8.4 7.35
            double Fthickness = 0.095 * 0.001;
            double thickness = 0.5 * (Do - Di);
            //double n_tubes = 10;
            //double n_rows = 2;
            //int[] Ntube = { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
            //int[] Ntube = { 2, 2, 2, 2 };
            int[] Ntube = { 24 };
            int N_tube = Ntube[0];
            double L = 870 * 0.001;
            int Nelement = 10;
            int[,] CirArrange;
            //CirArrange = new int[,] { { 9, 10, 0, 0, 0, 0, 0, 0 }, { 12, 11, 0, 0, 0, 0, 0, 0 }, { 13, 14, 0, 0, 0, 0, 0, 0 }, { 15, 16, 0, 0, 0, 0, 0, 0 }, { 8, 7, 6, 5, 4, 3, 2, 1 }, { 17, 18, 19, 20, 21, 22, 23, 24 } };
            //CirArrange = new int[,] { { 5, 6, 7, 8 }, { 12, 11, 10, 9 }, { 13, 14, 15, 16 }, { 20, 19, 18, 17 }, { 4, 3, 2, 1 }, { 21, 22, 23, 24 } };
            CirArrange = new int[,] { { 1, 2, 3, 4, 5, 6 }, { 7, 8, 9, 10, 11, 12 }, { 13, 14, 15, 16, 17, 18 }, { 19, 20, 21, 22, 23, 24 } };           
            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { 3, 1 };//{ 3, 1 };//{ 4, 2 };
            CircuitInfo.TubeofCir = new int[] { 6, 6, 6, 6 };//{ 6, 6, 6, 6 };//{ 4, 4, 4, 4, 4, 4 };//{ 2, 2, 2, 2, 8, 8 };
            CircuitInfo.UnequalCir = new int[] { 4, 4, 4, 0 };//{ 4, 4, 4, 0 };//{ 5, 5, 6, 6, 0, 0 };
            // [19 - 17 - 15 - 13   11   9   7   5   3   1] <====Air
            // [20 - 18 - 16 - 14   12   10  8   6   4   2] <====Air
            //  Ncir=1, 20in, 20->19 1out

            double[] d_cap = new double[] { 0, 0, 0 };
            double[] lenth_cap = new double[] { 0, 0, 0 };

            GeometryInput geoInput_air = new GeometryInput();
            geoInput_air.Pt = Pt;
            geoInput_air.Pr = Pr;
            geoInput_air.Do = Do;
            geoInput_air.Fthickness = Fthickness;
            geoInput_air.FPI = FPI[0];
            geoInput_air.Nrow = Nrow;

            int hexType = 1;

            double mr = 0.01973;//0.01826;
            //double Vel_a = 1.8; //m/s
            double H = Pt * N_tube;
            double Hx = L * H;
            double rho_a_st = 1.2; //kg/m3
            double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
            double Va = 2050;// m3/h
            double Vel_ave = Va / 3600 / Hx;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
            AirDistribution VaDistri = new AirDistribution();
            VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
            double[,] ma = new double[N_tube, Nelement];
            double[,] ha = new double[N_tube, Nelement];


            //空气侧几何结构选择
            //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
            //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
            int curve = 1; //

            double za = 1; //Adjust factor
            for (int i = 0; i < N_tube; i++)
            {
                for (int j = 0; j < Nelement; j++)
                {
                    ma[i, j] = VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st;
                    //ha[i, j] = AirHTC.alpha(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve);// *1.5;
                    //ha[i, j] = 79;
                    //ha[i, j] = AirHTC.alpha1(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve, geoInput_air, hexType).ha;
                    ha[i, j] = 61;
                }
            }
            double[,] haw = ha;

            res.DP = AirHTC.alpha1(Vel_ave, za, curve, geoInput_air, hexType).dP_a;

            double eta_surface = 0.819;
            double zh = 2;
            double zdp = 2;

            double tai = 35;
            double RHi = CoolProp.HAPropsSI("R", "T", tai + 273.15, "P", 101325, "B", 24 + 273.15);//0.469
            double tc = 50;
            //double pri = Refrigerant.SATT(fluid, composition, tc + 273.15, 1).Pressure;
            coolprop.update(input_pairs.QT_INPUTS, 0, tc + 273.15);
            double pri = coolprop.p() / 1000;
            //double pri = CoolProp.PropsSI("P", "T", tc + 273.15, "Q", 0, fluid) / 1000;
            //double P_exv = 1842.28;//kpa
            double tri = 75;//C
            double conductivity = 386; //w/mK for Cu
            double Pwater = 100.0;
            //int hexType = 1; //*********************************0 is evap, 1 is cond******************************************
            coolprop.update(input_pairs.PT_INPUTS, pri * 1000, tri + 273.15);
            double hri = coolprop.hmass() / 1000;
            //double hri = CoolProp.PropsSI("H", "T", tri + 273.15, "P", pri * 1000, fluid) / 1000;
            //double hri = 354.6;
            //double xin = 0.57;

            double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
            double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

            //string AirDirection="DowntoUp";
            string AirDirection = "逆流";
            ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai, tc, AirDirection);
            RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi, tc, AirDirection);


            //AreaResult geo = new AreaResult();
            //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, tri, pri, hri,
                //mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, tri, pri, hri,
                mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap, coolprop);


            return res;
        }
        public static CalcResult main_condenser_35_1()
        {
            //制冷剂制热模块计算
            //string fluid = new string[] { "Water" };
            string fluid = "R32";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            //string fluid = new string[] { "ISOBUTAN" };
            CalcResult res = new CalcResult();
            int Nrow = 1;
            //double[] FPI = new double[Nrow + 1];
            //FPI = new double[] { 1.27, 1.27, 1.27, 1.27, 1.27, 1.27, 1.27, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 5.2, 5.2, 5.2, 5.2, 5.2, 5.2 };
            double[] FPI = new double[] { 19.538 };
            double Pt = 21 * 0.001;
            double Pr = 22 * 0.001;
            double Di = 6.8944 * 0.001;//8 6.8944
            double Do = 7.35 * 0.001;//8.4 7.35
            double Fthickness = 0.095 * 0.001;
            double thickness = 0.5 * (Do - Di);
            //double n_tubes = 10;
            //double n_rows = 2;
            //int[] Ntube = { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
            //int[] Ntube = { 2, 2, 2, 2 };
            int[] Ntube = { 12 };
            int N_tube = Ntube[0];
            double L = 870 * 0.001;
            int Nelement = 10;
            int[,] CirArrange;
            CirArrange = new int[,] { { 1, 2, 3, 4 }, { 5, 6, 7, 8 }, { 9, 10, 11, 12 } };
            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { 3, 3 };//{ 4, 2 };
            CircuitInfo.TubeofCir = new int[] { 4, 4, 4 };//{ 4, 4, 4, 4, 4, 4 };//{ 2, 2, 2, 2, 8, 8 };
            //CircuitInfo.UnequalCir = new int[] { 4, 4, 4, 0 };//{ 5, 5, 6, 6, 0, 0 };
            // [19 - 17 - 15 - 13   11   9   7   5   3   1] <====Air
            // [20 - 18 - 16 - 14   12   10  8   6   4   2] <====Air
            //  Ncir=1, 20in, 20->19 1out

            double[] d_cap = new double[] { 0, 0, 0 };
            double[] lenth_cap = new double[] { 0, 0, 0 };

            GeometryInput geoInput_air = new GeometryInput();
            geoInput_air.Pt = Pt;
            geoInput_air.Pr = Pr;
            geoInput_air.Do = Do;
            geoInput_air.Fthickness = Fthickness;
            geoInput_air.FPI = FPI[0];
            geoInput_air.Nrow = Nrow;

            int hexType = 1;

            double mr = 0.01933;//0.01826;
            //double Vel_a = 1.8; //m/s
            double H = Pt * N_tube;
            double Hx = L * H;
            double rho_a_st = 1.2; //kg/m3
            double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
            double Va = 2050/2;// m3/h
            double Vel_ave = Va / 3600 / Hx;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
            AirDistribution VaDistri = new AirDistribution();
            VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
            double[,] ma = new double[N_tube, Nelement];
            double[,] ha = new double[N_tube, Nelement];


            //空气侧几何结构选择
            //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
            //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
            int curve = 1; //

            double za = 1; //Adjust factor
            for (int i = 0; i < N_tube; i++)
            {
                for (int j = 0; j < Nelement; j++)
                {
                    ma[i, j] = VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st;
                    //ha[i, j] = AirHTC.alpha(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve);// *1.5;
                    //ha[i, j] = 79;
                    //ha[i, j] = AirHTC.alpha1(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve, geoInput_air, hexType).ha;
                    ha[i, j] = 61;
                }
            }
            double[,] haw = ha;

            res.DP = AirHTC.alpha1(Vel_ave, za, curve, geoInput_air, hexType).dP_a;

            double eta_surface = 0.819;
            double zh = 2;
            double zdp = 4.85;

            double tai = 35;
            double RHi = CoolProp.HAPropsSI("R", "T", tai + 273.15, "P", 101325, "B", 24 + 273.15);//0.469
            double tc = 50;
            //double pri = Refrigerant.SATT(fluid, composition, tc + 273.15, 1).Pressure;
            coolprop.update(input_pairs.QT_INPUTS, 0, tc + 273.15);
            double pri = coolprop.p() / 1000;
            //double pri = CoolProp.PropsSI("P", "T", tc + 273.15, "Q", 0, fluid) / 1000;
            //double P_exv = 1842.28;//kpa
            double tri = 75;//C
            double conductivity = 386; //w/mK for Cu
            double Pwater = 100.0;
            //int hexType = 1; //*********************************0 is evap, 1 is cond******************************************
            coolprop.update(input_pairs.PT_INPUTS, pri * 1000, tri + 273.15);
            double hri = coolprop.hmass() / 1000;
            //double hri = CoolProp.PropsSI("H", "T", tri + 273.15, "P", pri * 1000, fluid) / 1000;
            //double hri = 354.6;
            //double xin = 0.57;

            double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
            double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

            //string AirDirection="DowntoUp";
            string AirDirection = "逆流";
            ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai, tc, AirDirection);
            RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi, tc, AirDirection);


            //AreaResult geo = new AreaResult();
            //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, tri, pri, hri,
                //mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, tri, pri, hri,
                mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap, coolprop);


            return res;
        }
        public static CalcResult main_condenser_35_2()
        {
            CalcResult r0 = new CalcResult();
            r0 = main_condenser_35_1();
            //制冷剂制热模块计算
            //string fluid = new string[] { "Water" };
            string fluid = "R32";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            //string fluid = new string[] { "ISOBUTAN" };
            CalcResult res = new CalcResult();
            int Nrow = 1;
            //double[] FPI = new double[Nrow + 1];
            //FPI = new double[] { 1.27, 1.27, 1.27, 1.27, 1.27, 1.27, 1.27, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 5.2, 5.2, 5.2, 5.2, 5.2, 5.2 };
            double[] FPI = new double[] { 19.538 };
            double Pt = 21 * 0.001;
            double Pr = 22 * 0.001;
            double Di = 6.8944 * 0.001;//8 6.8944
            double Do = 7.35 * 0.001;//8.4 7.35
            double Fthickness = 0.095 * 0.001;
            double thickness = 0.5 * (Do - Di);
            //double n_tubes = 10;
            //double n_rows = 2;
            //int[] Ntube = { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
            //int[] Ntube = { 2, 2, 2, 2 };
            int[] Ntube = { 12 };
            int N_tube = Ntube[0];
            double L = 870 * 0.001;
            int Nelement = 10;
            int[,] CirArrange;
            CirArrange = new int[,] { { 1, 2, 3, 4, 5, 6 }, { 7, 8, 9, 10, 11, 12 } };
            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { 2, 2 };//{ 4, 2 };
            CircuitInfo.TubeofCir = new int[] { 6, 6 };//{ 4, 4, 4, 4, 4, 4 };//{ 2, 2, 2, 2, 8, 8 };
            //CircuitInfo.UnequalCir = new int[] { 4, 4, 4, 0 };//{ 5, 5, 6, 6, 0, 0 };
            // [19 - 17 - 15 - 13   11   9   7   5   3   1] <====Air
            // [20 - 18 - 16 - 14   12   10  8   6   4   2] <====Air
            //  Ncir=1, 20in, 20->19 1out

            double[] d_cap = new double[] { 0, 0 };
            double[] lenth_cap = new double[] { 0, 0 };

            GeometryInput geoInput_air = new GeometryInput();
            geoInput_air.Pt = Pt;
            geoInput_air.Pr = Pr;
            geoInput_air.Do = Do;
            geoInput_air.Fthickness = Fthickness;
            geoInput_air.FPI = FPI[0];
            geoInput_air.Nrow = Nrow;

            int hexType = 1;

            double mr = r0.mr;//0.01826;
            //double Vel_a = 1.8; //m/s
            double H = Pt * N_tube;
            double Hx = L * H;
            double rho_a_st = 1.2; //kg/m3
            double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
            double Va = 2050 / 2;// m3/h
            double Vel_ave = Va / 3600 / Hx;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
            AirDistribution VaDistri = new AirDistribution();
            VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
            double[,] ma = new double[N_tube, Nelement];
            double[,] ha = new double[N_tube, Nelement];


            //空气侧几何结构选择
            //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
            //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
            int curve = 1; //

            double za = 1; //Adjust factor
            for (int i = 0; i < N_tube; i++)
            {
                for (int j = 0; j < Nelement; j++)
                {
                    ma[i, j] = VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st;
                    //ha[i, j] = AirHTC.alpha(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve);// *1.5;
                    //ha[i, j] = 79;
                    //ha[i, j] = AirHTC.alpha1(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve, geoInput_air, hexType).ha;
                    ha[i, j] = 61;
                }
            }
            double[,] haw = ha;

            res.DP = AirHTC.alpha1(Vel_ave, za, curve, geoInput_air, hexType).dP_a;

            double eta_surface = 0.819;
            double zh = 2;
            double zdp = 4.85;

            double tai = 35;
            double RHi = CoolProp.HAPropsSI("R", "T", tai + 273.15, "P", 101325, "B", 24 + 273.15);//0.469
            double tc = 50;
            //double pri = Refrigerant.SATT(fluid, composition, tc + 273.15, 1).Pressure;

            double pri = r0.Pro;//CoolProp.PropsSI("P", "T", tc + 273.15, "Q", 0, fluid) / 1000;
            //double P_exv = 1842.28;//kpa
            double tri = r0.Tro;//74.2;//C
            double conductivity = 386; //w/mK for Cu
            double Pwater = 100.0;
            //int hexType = 1; //*********************************0 is evap, 1 is cond******************************************
            double hri = r0.hro;//CoolProp.PropsSI("H", "T", tri + 273.15, "P", pri * 1000, fluid) / 1000;
            //double hri = 354.6;
            //double xin = 0.57;

            double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
            double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

            //string AirDirection="DowntoUp";
            string AirDirection = "逆流";
            ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai, tc, AirDirection);
            RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi, tc, AirDirection);


            //AreaResult geo = new AreaResult();
            //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, tri, pri, hri,
                //mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, tri, pri, hri,
                mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap, coolprop);


            res.Q = res.Q + r0.Q;
            res.DP = res.DP + r0.DP;
            return res;
        }
        public static CalcResult main_evaporator_35()
        {
            string fluid = "R32";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            //string fluid = new string { "R410A.mix" };
            //string fluid = new string[] { "ISOBUTAN" };
            CalcResult res = new CalcResult();
            int Nrow = 1;
            double[] FPI = new double[Nrow + 1];
            //FPI = new double[] { 1.27, 1.27, 1.27, 1.27, 1.27, 1.27, 1.27, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 5.2, 5.2, 5.2, 5.2, 5.2, 5.2 };
            FPI = new double[] { 19.538, 19.538 };
            double Pt = 21 * 0.001;
            double Pr = 22 * 0.001;
            double Di = 6.8944 * 0.001;//8 6.8944
            double Do = 7.35 * 0.001;//8.4 7.35
            double Fthickness = 0.095 * 0.001;
            double thickness = 0.5 * (Do - Di);
            //double n_tubes = 10;
            //double n_rows = 2;
            //int[] Ntube = { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
            //int[] Ntube = { 2, 2, 2, 2 };
            int[] Ntube = { 24 };
            int N_tube = Ntube[0];
            double L = 870 * 0.001;
            int Nelement = 10;
            int[,] CirArrange;
            CirArrange = new int[,] { { 24, 23, 22, 21, 20, 19, 18, 17 }, { 1, 2, 3, 4, 5, 6, 7, 8 }, { 16, 15, 0, 0, 0, 0, 0, 0 }, { 14, 13, 0, 0, 0, 0, 0, 0 }, { 11, 12, 0, 0, 0, 0, 0, 0 }, { 10, 9, 0, 0, 0, 0, 0, 0 } };
            //CirArrange = new int[,] { { 24, 23, 22, 21 }, { 1, 2, 3, 4 }, { 17, 18, 19, 20 }, { 16, 15, 14, 13 }, { 9, 10, 11, 12 }, { 8, 7, 6, 5 } };
            //CirArrange = new int[,] { { 24, 23, 22, 21, 20, 19 }, { 18, 17, 16, 15, 14, 13 }, { 12, 11, 10, 9, 8, 7 }, { 6, 5, 4, 3, 2, 1 } };
            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { 4, 2 };//{ 3, 1 };//{ 4, 2 };
            CircuitInfo.TubeofCir = new int[] { 8, 8, 2, 2, 2, 2 };//{ 6, 6, 6, 6 };//{ 4, 4, 4, 4, 4, 4 };//{ 8, 8, 2, 2, 2, 2 };
            CircuitInfo.UnequalCir = new int[] { -5, -6, 5, 5, 6, 6 };//{ -4, 4, 4, 4 };//{ -5, -6, 5, 5, 6, 6 };
            // [19 - 17 - 15 - 13   11   9   7   5   3   1] <====Air
            // [20 - 18 - 16 - 14   12   10  8   6   4   2] <====Air
            //  Ncir=1, 20in, 20->19 1out

            //bool reverse = true; //*********************************false:origin, true:reverse******************************************
            //CirArrange = CircuitReverse.CirReverse(reverse, CirArrange, CircuitInfo);

            double[] d_cap = new double[6]; //{ 0, 0, 0, 0 };
            double[] lenth_cap = new double[6];// { 0, 0, 0, 0 };

            double mr = 0.012;//0.0125;
            //double Vel_a = 1.8; //m/s
            double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
            double H = Pt * N_tube;
            double Hx = L * H;
            double rho_a_st = 1.2; //kg/m3
            double Va = 2000;
            double Vel_ave = Va / 3600 / Hx;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
            AirDistribution VaDistri = new AirDistribution();
            VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
            double[,] ma = new double[N_tube, Nelement];
            double[,] ha = new double[N_tube, Nelement];


            int hexType = 0;

            GeometryInput geoInput_air = new GeometryInput();
            geoInput_air.Pt = Pt;
            geoInput_air.Pr = Pr;
            geoInput_air.Do = Do;
            geoInput_air.Fthickness = Fthickness;
            geoInput_air.FPI = FPI[0];
            geoInput_air.Nrow = Nrow;

            //空气侧几何结构选择
            //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
            //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
            int curve = 1; //
            AirCoef_res res1 = new AirCoef_res();
            double za = 1; //Adjust factor
            for (int i = 0; i < N_tube; i++)
            {
                for (int j = 0; j < Nelement; j++)
                {
                    ma[i, j] = VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st;
                    //ha[i, j] = AirHTC.alpha(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve) * 1.5;
                    //ha[i, j] = 79;
                    //ha[i, j] = AirHTC.alpha1(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve, geoInput_air, hexType).ha;
                    ha[i, j] = 61;
                }
            }
            double[,] haw = ha;

            res.DP = AirHTC.alpha1(Vel_ave, za, curve, geoInput_air, hexType).dP_a;

            double eta_surface = 0.8;
            double zh = 1.6;//1.6;
            double zdp = 4.85;//4.85;

            double tai = 7;
            double RHi = CoolProp.HAPropsSI("R", "T", tai + 273.15, "P", 101325, "B", 6 + 273.15);
            double tri = -1;
            double te = tri;
            coolprop.update(input_pairs.QT_INPUTS, 0, te + 273.15);
            double pe = coolprop.p() / 1000;
            //double pe = CoolProp.PropsSI("P", "T", te + 273.15, "Q", 0, fluid) / 1000;

            double P_exv = 3142.28;//kpa
            double T_exv = 36;//C
            double conductivity = 386; //w/mK for Cu
            double Pwater = 0;
            //int hexType = 0; //*********************************0 is evap, 1 is cond******************************************
            double Tc = 40;
            double Sc = 9;
            coolprop.update(input_pairs.QT_INPUTS, 0, Tc + 273.1);
            double Pc = coolprop.p() / 1000;
            //double Pc = CoolProp.PropsSI("P", "T", Tc + 273.15, "Q", 0, fluid) / 1000;
            coolprop.update(input_pairs.PT_INPUTS, Pc * 1000, Tc - Sc + 273.15);
            double hri = coolprop.hmass() / 1000;
            //double hri = CoolProp.PropsSI("H", "T", Tc - Sc + 273.15, "P", Pc * 1000, fluid) / 1000;
            double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
            double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

            //string AirDirection="DowntoUp";
            string AirDirection = "逆流";
            ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai, te, AirDirection);
            RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi, te, AirDirection);


            //AreaResult geo = new AreaResult();
            //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, te, pe, hri,
                //mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, te, pe, hri,
                mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap, coolprop);

            return res;
        }
        public static CalcResult Water_Midea5()
        {
            string fluid = "Water";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            //string fluid = new string[] { "R410A.MIX" };
            //string fluid = new string[] { "ISOBUTAN" };
            CalcResult res = new CalcResult();
            int Nrow = 2;
            double[] FPI = new double[Nrow + 1];
            //FPI = new double[] { 1.27, 1.27, 1.27, 1.27, 1.27, 1.27, 1.27, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 5.2, 5.2, 5.2, 5.2, 5.2, 5.2 };
            FPI = new double[] { 21.1667, 21.1667 };
            double Pt = 14.5 * 0.001;
            double Pr = 12.56 * 0.001;
            double Di = 4.6 * 0.001;//8
            double Do = 5.25 * 0.001;//8.4
            double Fthickness = 0.095 * 0.001;
            double thickness = 0.5 * (Do - Di);
            //double n_tubes = 10;
            //double n_rows = 2;
            //int[] Ntube = { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
            //int[] Ntube = { 2, 2, 2, 2 };
            int[] Ntube = { 24, 24 };
            int N_tube = Ntube[0];
            double L = 400 * 0.001;
            int Nelement = 1;
            int[,] CirArrange;
            //CirArrange = new int[,] { { 8, 6, 4, 2, 1, 3, 5, 7 } };//actual, counter-paralle,  Q=83.1
            //CirArrange = new int[,] { { 20, 18, 16, 14, 12, 10, 8, 6, 4, 2, 1, 3, 5, 7, 9, 11, 13, 15, 17, 19 } };//actual, counter-paralle,  Q=83.1
            //CirArrange = new int[,] { { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 1, 3, 5, 7, 9, 11, 13, 15, 17, 19 } }; //paralle-paralle, better Q=85.3
            //CirArrange = new int[,] { { 20, 18, 16, 14, 12, 10, 8, 6, 4, 2, 19, 17, 15, 13, 11, 9, 7, 5, 3, 1 } };//counter-counter,  Q=82.4
            //CirArrange = new int[,] { { 14, 12, 10, 8, 6, 4, 2, 1, 3, 5, 7, 9, 11, 13 } };//actual, counter-paralle,  Q=76
            //CirArrange = new int[,] { {16, 14, 12, 10, 8, 6, 4, 2, 1, 3, 5, 7, 9, 11, 13, 15 } };//actual, counter-paralle,  Q=79
            //CirArrange = new int[,] { {42, 40, 38, 36, 34, 32, 30, 28, 26, 24, 22, 20, 18, 16, 14, 12, 10, 8, 6, 4, 2, 
            //                              1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25, 27, 29, 31, 33, 35, 37, 39, 41 } };//actual, counter-paralle,  Q=79
            //CirArrange = new int[,] { {32, 30, 28, 26, 24, 22, 20, 18, 16, 14, 12, 10, 8, 6, 4, 2, 
            //                              1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25, 27, 29, 31 } };//actual, counter-paralle,  Q=79

            //CirArrange = new int[,] { { 7, 8, 2, 1, 5, 3 }, {5, 8, 9, 10, 11}, {5, 88, 5, 4, 3 } };
            //CirArrange = new int[,] { { 7, 8, 2, 1, 0, 0, 0, 0 }, { 9, 10, 11, 12, 6, 5, 4, 3 } };
            //CirArrange = new int[,] { { 7, 1, 0, 0, 0, 0 }, { 8, 9, 3, 2, 0, 0 }, { 10, 11, 12, 6, 5, 4 } };
            //CirArrange = new int[,] { { 7, 8, 2, 1, 9, 10, 11, 12, 6, 5, 4, 3 } };
            //List<string> productType = new List<string>();
            //CirArrange = new int[,] { { 7, 8, 9, 3, 2, 1 }, { 12, 11, 10, 4, 5, 6 } };
            //CirArrange = new int[,] { { 7, 8, 9, 4, 5, 6 }, { 12, 11, 10, 3, 2, 1 } };
            //CirArrange = new int[,] { { 7, 8, 5, 6 }, { 10, 9, 2, 1 }, { 12, 11, 4, 3 } };
            //CirArrange = new int[,] { { 7, 8, 2, 1 }, { 10, 9, 3, 4 }, { 11, 12, 6, 5 } };
            CirArrange = new int[,] { { 25, 26, 27, 28, 4, 3, 2, 1 }, { 29, 30, 31, 32, 8, 7, 6, 5 }, { 33, 34, 35, 36, 12, 11, 10, 9 }, { 37, 38, 39, 40, 16, 15, 14, 13 }, { 41, 42, 43, 44, 20, 19, 18, 17 }, { 48, 47, 46, 45, 24, 23, 22, 21 } };
            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { 6, 6 };
            CircuitInfo.TubeofCir = new int[] { 8, 8, 8, 8, 8, 8 };  //{ 4, 8 };
            // [19 - 17 - 15 - 13   11   9   7   5   3   1] <====Air
            // [20 - 18 - 16 - 14   12   10  8   6   4   2] <====Air
            //  Ncir=1, 20in, 20->19 1out

            double[] d_cap = new double[] { 0, 0, 0, 0, 0, 0 };
            double[] lenth_cap = new double[] { 0, 0, 0, 0, 0, 0 };

            GeometryInput geoInput_air = new GeometryInput();
            geoInput_air.Pt = Pt;
            geoInput_air.Pr = Pr;
            geoInput_air.Do = Do;
            geoInput_air.Fthickness = Fthickness;
            geoInput_air.FPI = FPI[0];
            geoInput_air.Nrow = Nrow;

            int hexType = 1; 

            double mr = 23.0 / 60;
            //double Vel_a = 1.8; //m/s
            double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
            double Vel_ave = 1.0;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
            AirDistribution VaDistri = new AirDistribution();
            VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
            double[,] ma = new double[N_tube, Nelement];
            double[,] ha = new double[N_tube, Nelement];
            double H = Pt * N_tube;
            double Hx = L * H;
            double rho_a_st = 1.2; //kg/m3

            //空气侧几何结构选择
            //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
            //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
            int curve = 1; //

            double za = 1; //Adjust factor
            for (int i = 0; i < N_tube; i++)
            {
                for (int j = 0; j < Nelement; j++)
                {
                    ma[i, j] = VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st;
                    //ha[i, j] = AirHTC.alpha(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve) * 1.5;
                    //ha[i, j] = 79;
                    ha[i, j] = AirHTC.alpha1(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve, geoInput_air, hexType).ha;
                }
            }
            double[,] haw = ha;

            res.DPa = AirHTC.alpha1(Vel_ave, za, curve, geoInput_air, hexType).dP_a;

            double eta_surface = 0.89;
            double zh = 1;
            double zdp = 1;
                  
            double tai = 20;
            double RHi = 0.469;
            double tri = 45;
            double tc = tri;
            //double pc1 = Refrigerant.SATT(fluid, composition, tc + 273.15, 1).Pressure;
            coolprop.update(input_pairs.QT_INPUTS, 0, tc + 273.15);
            double pc = coolprop.p() / 1000;
            //double pc = CoolProp.PropsSI("P", "T", tc + 273.15, "Q", 0, fluid) / 1000;
            double Pwater = 305;//kpa
            double conductivity = 386; //w/mK for Cu
            //int hexType = 1; //*********************************0 is evap, 1 is cond******************************************
            coolprop.update(input_pairs.PT_INPUTS, Pwater * 1000, tc + 273.15);
            double hri = coolprop.hmass() / 1000;
            //double hri = CoolProp.PropsSI("H", "T", tc + 273.15, "P", Pwater * 1000, fluid) / 1000 ;

            double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
            double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

            //string AirDirection="DowntoUp";
            string AirDirection = "逆流";
            ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai, tc, AirDirection);
            RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi, tc, AirDirection);


            //AreaResult geo = new AreaResult();
            //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, tc, pc, hri,
                //mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, tc, pc, hri,
                mr, ma, ha,haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater,AirDirection, d_cap, lenth_cap, coolprop);

            return res;
        }

        public static CalcResult Water_Midea9()
        {
            string fluid = "Water";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            CalcResult res = new CalcResult();
            int Nrow = 1;
            double[] FPI = new double[Nrow + 1];
            FPI = new double[] { 21 };
            double Pt = 21 * 0.001;
            double Pr = 18.19 * 0.001;
            double Di = 6.8944 * 0.001;//8
            double Do = 7.35 * 0.001;//8.4
            double Fthickness = 0.095 * 0.001;
            double thickness = 0.5 * (Do - Di);
            int[] Ntube = { 12 };
            int N_tube = Ntube[0];
            double L = 410 * 0.001;
            int Nelement = 1;
            int[,] CirArrange;
            CirArrange = new int[,] { { 1, 2, 3, 4 }, { 5, 6, 7, 8 }, { 9, 10, 11, 12 } };
            //CirArrange = new int[,] { { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 } };
            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { 3, 3 };
            CircuitInfo.TubeofCir = new int[] { 4, 4, 4 };  //{ 4, 8 };
            // [19 - 17 - 15 - 13   11   9   7   5   3   1] <====Air
            // [20 - 18 - 16 - 14   12   10  8   6   4   2] <====Air


            double[] d_cap = new double[] { 0, 0, 0 };
            double[] lenth_cap = new double[] { 0, 0, 0 };


            GeometryInput geoInput_air = new GeometryInput();
            geoInput_air.Pt = Pt;
            geoInput_air.Pr = Pr;
            geoInput_air.Do = Do;
            geoInput_air.Fthickness = Fthickness;
            geoInput_air.FPI = FPI[0];
            geoInput_air.Nrow = Nrow;

            int hexType = 1; 

            double mr = 9.99/60;
            //double Vel_a = 1.8; //m/s
            double[,] Vel_distribution = { { 1 }, {2} };//distribution,do not must be real velocity!
            double Vel_ave = 2;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
            AirDistribution VaDistri = new AirDistribution();
            VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
            double[,] ma = new double[N_tube, Nelement];
            double[,] ha = new double[N_tube, Nelement];
            double H = Pt * N_tube;
            double Hx = L * H;
            double rho_a_st = 1.188; //kg/m3

            //空气侧几何结构选择
            //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
            //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
            int curve = 1; //

            double za = 1; //Adjust factor
            for (int i = 0; i < N_tube; i++)
            {
                for (int j = 0; j < Nelement; j++)
                {
                    ma[i, j] = VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st;
                    //ha[i, j] = AirHTC.alpha(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve) * 1.5;
                    //ha[i, j] = AirHTC.alpha1(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve, geoInput_air, hexType).ha;
                }
            }

            ha = AirHTC_CAL.alpha_cal(ha, VaDistri.Va, VaDistri.Va_ave, Vel_ave, za, curve, geoInput_air, hexType, N_tube, Nelement);
            double[,] haw = ha;
     
            res.DPa = AirHTC.alpha1(Vel_ave, za, curve, geoInput_air, hexType).dP_a;

            double eta_surface = 0.8284;
            double zh = 1;
            double zdp = 1;           
            
            double tai = 19.98;
            double RHi = 0.469;
            double tri = 44.98;
            double tc = tri;
            //double pc = Refrigerant.SATT(fluid, composition, tc + 273.15, 1).Pressure;
            coolprop.update(input_pairs.QT_INPUTS, 0, tc + 273.15);
            double pc = coolprop.p() / 1000;
            //double pc = CoolProp.PropsSI("P", "T", tc + 273.15, "Q", 0, fluid) / 1000;
            double Pwater = 395;//kpa
            double conductivity = 386; //w/mK for Cu
            //int hexType = 1; //*********************************0 is evap, 1 is cond******************************************
            coolprop.update(input_pairs.PT_INPUTS, Pwater * 1000, tc + 273.15);
            double hri = coolprop.hmass() / 1000;
            //double hri = CoolProp.PropsSI("H", "T", tc + 273.15, "P", Pwater * 1000, fluid) / 1000 ;

            double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
            double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

            //string AirDirection="DowntoUp";
            string AirDirection = "逆流";
            ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai, tc, AirDirection);
            RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi, tc, AirDirection);


            //AreaResult geo = new AreaResult();
            //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, tc, pc, hri,
                //mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, tc, pc, hri,
                mr, ma, ha,haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater,AirDirection, d_cap, lenth_cap, coolprop);


            return res;
        }

        public static CalcResult Water_Cool_Midea9()
        {
            string fluid = "Water";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            CalcResult res = new CalcResult();
            int Nrow = 1;
            double[] FPI = new double[Nrow + 1];
            FPI = new double[] { 21 };
            double Pt = 21 * 0.001;
            double Pr = 18.19 * 0.001;
            double Di = 6.8944 * 0.001;//8
            double Do = 7.35 * 0.001;//8.4
            double Fthickness = 0.095 * 0.001;
            double thickness = 0.5 * (Do - Di);
            int[] Ntube = { 12 };
            int N_tube = Ntube[0];
            double L = 410 * 0.001;
            int Nelement = 1;
            int[,] CirArrange;
            CirArrange = new int[,] { { 1, 2, 3, 4 }, { 5, 6, 7, 8 }, { 9, 10, 11, 12 } };
            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { 3, 3 };
            CircuitInfo.TubeofCir = new int[] { 4, 4, 4 };  //{ 4, 8 };

            double[] d_cap = new double[] { 0, 0, 0 };
            double[] lenth_cap = new double[] { 0, 0, 0 };

            int N = 28;
            for (int i = 6; i < 7; i++)
            {
                double[] mr = new double[] { 10.0, 14.01, 18.0, 21.01, 25.01, 29.0, 31.01, 10.01, 14.01, 18.01, 21.01, 25.01, 29.01, 31.01, 10.01, 14.01, 18.0, 21.0, 25.01, 29.0, 31.01, 10.01, 14.01, 18.01, 21.01, 25.01, 29.01, 31.0 }; // 60;
                mr[i] = mr[i] / 60;
                double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
                //double Vel_ave = 1.6;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
                double[] Vel_ave = new double[] { 0.8, 0.8, 0.8, 0.8, 0.8, 0.8, 0.8, 1.2, 1.2, 1.2, 1.2, 1.2, 1.2, 1.2, 1.5, 1.5, 1.5, 1.5, 1.5, 1.5, 1.5, 2.0, 2.0, 2.0, 2.0, 2.0, 2.0, 2.0 }; //m/s
                AirDistribution VaDistri = new AirDistribution();
                VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
                double[,] ma = new double[N_tube, Nelement];
                double[,] ha = new double[N_tube, Nelement];
                double H = Pt * N_tube;
                double Hx = L * H;
                double rho_a_st = 1.188;

                //空气侧几何结构选择
                //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
                //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
                //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
                //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
                int curve = 1; //

                double za = 1; //Adjust factor
                double zh = 1;
                double zdp = 1;
                double[] eta_surface = new double[] { 0.845, 0.845, 0.845, 0.845, 0.845, 0.845, 0.845, 0.835, 0.835, 0.835, 0.835, 0.835, 0.835, 0.835, 0.835, 0.835, 0.835, 0.835, 0.835, 0.835, 0.835, 0.83, 0.83, 0.83, 0.83, 0.83, 0.83, 0.83 };
                double[] hai = new double[] { 72.64, 72.64, 72.64, 72.64, 72.64, 72.64, 73.6, 77.2, 77.2, 77.2, 77.2, 77.2, 77.2, 77.2, 78.67, 78.67, 78.67, 78.67, 78.67, 78.67, 78.67, 83.09, 83.09, 83.09, 83.09, 83.09, 83.09, 83.09 };
                for (int j = 0; j < N_tube; j++)
                {
                    for (int k = 0; k < Nelement; k++)
                    {
                        ma[j, k] = VaDistri.Va[j, k] * (Vel_ave[i] / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st;
                        //ha[j, k] = AirHTC.alpha(VaDistri.Va[j, k] * (Vel_ave / VaDistri.Va_ave), za, curve);
                        ha[j, k] = hai[i];
                    }
                }
                double[,] haw = ha;
                
                double[] tai = new double[] { 27.0, 26.99, 26.99, 26.99, 27.01, 27.02, 26.98, 27.01, 27.00, 27.01, 27.00, 27.02, 27.00, 27.00, 27.01, 26.99, 26.99, 27.0, 27.0, 26.99, 27.01, 27.00, 26.99, 26.99, 27.01, 27.0, 27.0, 26.98 };
                double[] RHi = new double[] { 27.0, 26.99, 26.99, 26.99, 27.01, 27.02, 26.98, 27.01, 27.00, 27.01, 27.00, 27.02, 27.00, 27.00, 27.01, 26.99, 26.99, 27.0, 27.0, 26.99, 27.01, 27.00, 26.99, 26.99, 27.01, 27.0, 27.0, 26.98 };

                double[] tri = new double[] { 9.98, 10.0, 9.98, 10.0, 9.99, 10.0, 9.98, 10.0, 9.99, 10.0, 10.01, 9.99, 10.02, 10.0, 10.01, 9.99, 9.99, 9.98, 9.99, 10.0, 10.0, 10.0, 9.99, 9.99, 9.99, 9.99, 9.99, 9.99 };
                double[] tc = tri;
                double[] pc = new double[N];
                //pc[i] = Refrigerant.SATT(fluid, composition, tc[i] + 273.15, 1).Pressure;
                coolprop.update(input_pairs.QT_INPUTS, 0, tc[i] + 273.15);
                pc[i] = coolprop.p() / 1000;
                //pc[i] = CoolProp.PropsSI("P", "T", tc[i] + 273.15, "Q", 0, fluid) / 1000;
                double Pwater = 395;//kpa
                double conductivity = 386; //w/mK for Cu
                int hexType = 0; //*********************************0 is evap, 1 is cond******************************************
                double[] hri = new double[N];
                coolprop.update(input_pairs.PT_INPUTS, Pwater * 1000, tc[i] + 273.15);
                hri[i] = coolprop.hmass() / 1000;
                //hri[i] = CoolProp.PropsSI("H", "T", tc[i] + 273.15, "P", Pwater * 1000, fluid) / 1000 ;

                double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
                double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];
                string AirDirection = "逆流";
                ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai[i], tc[i], AirDirection);
                RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi[i], tc[i], AirDirection);


                //AreaResult geo = new AreaResult();
                //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
                //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, tc[i], pc[i], hri[i],
                    //mr[i], ma, ha, haw, eta_surface[i], zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

                Geometry geo = new Geometry();
                geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
                res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, tc[i], pc[i], hri[i],
                    mr[i], ma, ha,haw, eta_surface[i], zh, zdp, hexType, thickness, conductivity, Pwater,AirDirection, d_cap, lenth_cap, coolprop);

                //using (StreamWriter wr = File.AppendText(@"D:\Work\Simulation\Test\Midea9_cool.txt"))
                //{
                //    wr.WriteLine("Q, {0}, DP, {1}, href, {2}, Ra_ratio, {3}, Tao, {4}, Tro, {5}", res.Q, res.DP, res.href, res.Ra_ratio, res.Tao, res.Tro);
                //}
            }
            return res;
        }

        public static CalcResult Water_Heat_Midea9()
        {
            string fluid = "Water";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            CalcResult res = new CalcResult();
            int Nrow = 1;
            double[] FPI = new double[Nrow + 1];
            FPI = new double[] { 21 };
            double Pt = 21 * 0.001;
            double Pr = 18.19 * 0.001;
            double Di = 6.8944 * 0.001;//8
            double Do = 7.35 * 0.001;//8.4
            double Fthickness = 0.095 * 0.001;
            double thickness = 0.5 * (Do - Di);
            int[] Ntube = { 12 };
            int N_tube = Ntube[0];
            double L = 410 * 0.001;
            int Nelement = 1;
            int[,] CirArrange;
            CirArrange = new int[,] { { 1, 2, 3, 4 }, { 5, 6, 7, 8 }, { 9, 10, 11, 12 } };
            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { 3, 3 };
            CircuitInfo.TubeofCir = new int[] { 4, 4, 4 };  //{ 4, 8 };

            double[] d_cap = new double[] { 0, 0, 0 };
            double[] lenth_cap = new double[] { 0, 0, 0 };

            int N = 28;
            for (int i = 0; i < N; i++)
            {
                double[] mr = new double[] { 10.01, 14.01, 18.0, 21.0, 25.0, 29.01, 31.0, 9.99, 14.01, 18.01, 21.01, 25.0, 29.01, 31.0, 10.01, 14.01, 18.01, 21.01, 25.0, 29.01, 31.01, 9.99, 14.0, 18.0, 21.0, 24.99, 29.0, 31.01 }; // 60;
                mr[i] = mr[i] / 60;
                double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
                //double Vel_ave = 1.6;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
                double[] Vel_ave = new double[] { 0.8, 0.8, 0.8, 0.8, 0.8, 0.8, 0.8, 1.2, 1.2, 1.2, 1.2, 1.2, 1.2, 1.2, 1.5, 1.5, 1.5, 1.5, 1.5, 1.5, 1.5, 2.0, 2.0, 2.0, 2.0, 2.0, 2.0, 2.0 }; //m/s
                AirDistribution VaDistri = new AirDistribution();
                VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
                double[,] ma = new double[N_tube, Nelement];
                double[,] ha = new double[N_tube, Nelement];
                double H = Pt * N_tube;
                double Hx = L * H;
                double rho_a_st = 1.188;

                //空气侧几何结构选择
                //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
                //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
                //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
                //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
                int curve = 1; //

                double za = 1; //Adjust factor
                double zh = 1;
                double zdp = 1;
                double[] eta_surface = new double[] { 0.8764, 0.8764, 0.8764, 0.8764, 0.8764, 0.8764, 0.8764, 0.865, 0.865, 0.865, 0.865, 0.865, 0.865, 0.865, 0.855, 0.855, 0.855, 0.855, 0.855, 0.855, 0.855, 0.846, 0.846, 0.846, 0.846, 0.846, 0.846, 0.846 };
                double[] hai = new double[] { 58.51, 58.51, 58.51, 58.51, 58.51, 58.51, 58.51, 64.92, 64.92, 64.92, 64.92, 64.92, 64.92, 64.92, 70.41, 70.41, 70.41, 70.41, 70.41, 70.41, 70.41, 75.42, 75.42, 75.42, 75.42, 75.42, 75.42, 75.42 };
                for (int j = 0; j < N_tube; j++)
                {
                    for (int k = 0; k < Nelement; k++)
                    {
                        ma[j, k] = VaDistri.Va[j, k] * (Vel_ave[i] / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st;
                        //ha[j, k] = AirHTC.alpha(VaDistri.Va[j, k] * (Vel_ave / VaDistri.Va_ave), za, curve);
                        ha[j, k] = hai[i];
                    }
                }
                double[,] haw = ha;
                
                
                double[] tai = new double[] { 20.01, 20.0, 20.02, 20.0, 19.99, 20.02, 20.0, 19.98, 20.0, 19.99, 19.99, 19.99, 19.98, 20.0, 19.98, 20.01, 19.99, 20.0, 20.0, 19.99, 19.99, 19.98, 20.01, 20.0, 20.0, 20.01, 20.01, 20.02 };
                double[] RHi = new double[] { 20.01, 20.0, 20.02, 20.0, 19.99, 20.02, 20.0, 19.98, 20.0, 19.99, 19.99, 19.99, 19.98, 20.0, 19.98, 20.01, 19.99, 20.0, 20.0, 19.99, 19.99, 19.98, 20.01, 20.0, 20.0, 20.01, 20.01, 20.02 };

                double[] tri = new double[] { 45.01, 45.01, 44.99, 44.99, 45.0, 45.0, 44.99, 44.99, 45.01, 44.99, 45.01, 44.99, 45.01, 45.01, 45.0, 45.01, 44.99, 45.01, 45.01, 44.99, 45.01, 44.98, 45.01, 44.99, 45.0, 45.0, 45.01, 45.0 };
                double[] tc = tri;
                double[] pc = new double[N];
                //pc[i] = Refrigerant.SATT(fluid, composition, tc[i] + 273.15, 1).Pressure;
                coolprop.update(input_pairs.QT_INPUTS, 0, tc[i] + 273.15);
                pc[i] = coolprop.p() / 1000;
                //pc[i] = CoolProp.PropsSI("P", "T", tc[i] + 273.15, "Q", 0, fluid) / 1000;
                double Pwater = 395;//kpa
                double conductivity = 386; //w/mK for Cu
                int hexType = 1; //*********************************0 is evap, 1 is cond******************************************
                double[] hri = new double[N];
                coolprop.update(input_pairs.PT_INPUTS, Pwater * 1000, tc[i] + 273.15);
                hri[i] = coolprop.hmass() / 1000;
                //hri[i] = CoolProp.PropsSI("H", "T", tc[i] + 273.15, "P", Pwater * 1000, fluid) / 1000 ;

                double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
                double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

                string AirDirection = "逆流";
                ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai[i], tc[i], AirDirection);
                RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi[i], tc[i], AirDirection);


                //AreaResult geo = new AreaResult();
                //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
                //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, tc[i], pc[i], hri[i],
                    //mr[i], ma, ha, haw, eta_surface[i], zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

                Geometry geo = new Geometry();
                geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
                res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, tc[i], pc[i], hri[i],
                    mr[i], ma, ha,haw, eta_surface[i], zh, zdp, hexType, thickness, conductivity, Pwater,AirDirection, d_cap, lenth_cap, coolprop);

                //using (StreamWriter wr = File.AppendText(@"D:\Work\Simulation\Test\Midea9_heat.txt"))
                //{
                //    wr.WriteLine("Q, {0}, DP, {1}, href, {2}, Ra_ratio, {3}, Tao, {4}, Tro, {5}", res.Q, res.DP, res.href, res.Ra_ratio, res.Tao, res.Tro);
                //}
            }
            return res;
        }
        public static CalcResult Water_Heat_Jiayong6()
        {
            string fluid = "Water";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            CalcResult res = new CalcResult();
            int Nrow = 2;
            double[] FPI = new double[Nrow + 1];
            FPI = new double[] { 21.167, 21.167 };
            double Pt = 21 * 0.001;
            double Pr = 22 * 0.001;
            double Di = 6.8944 * 0.001;//8
            double Do = 7.35 * 0.001;//8.4
            double Fthickness = 0.095 * 0.001;
            double thickness = 0.5 * (Do - Di);
            int[] Ntube = { 16, 16 };
            int N_tube = Ntube[0];
            double L = 400 * 0.001;
            int Nelement = 1;
            int[,] CirArrange;
            CirArrange = new int[,] { { 17, 18, 19, 20, 4, 3, 2, 1 }, { 21, 22, 23, 24, 8, 7, 6, 5 }, { 25, 26, 27, 28, 12, 11, 10, 9 }, { 29, 30, 31, 32, 16, 15, 14, 13 } };
            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { 4, 4 };
            CircuitInfo.TubeofCir = new int[] { 8, 8, 8, 8 };  //{ 4, 8 };

            CirArr[] cirArr = new CirArr[Nrow * N_tube];
            cirArr = CirArrangement.ReadCirArr(CirArrange, CircuitInfo, Nrow, Ntube).CirArr;
            //CircuitType CirType = new CircuitType();
            CircuitInfo.CirType = CircuitIdentification.CircuitIdentify(CircuitInfo.number, CircuitInfo.TubeofCir, cirArr);



            double[] d_cap = new double[] { 0, 0, 0, 0 };
            double[] lenth_cap = new double[] { 0, 0, 0, 0 };


            //AreaResult geo = new AreaResult();
            //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);

            double[] Q = new double[16];
            int N = 16;
            for (int i = 0; i < N; i++)
            {
                double[] mr = new double[] { 13.01, 17.01, 21.00, 25.00, 13.00, 17.01, 21.00, 25.00, 13.00, 17.00, 21.00, 25.01, 13.01, 17.00, 21.01, 25.00 }; // 60;
                mr[i] = mr[i] / 60;
                double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
                //double Vel_ave = 1.6;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
                double[] Vel_ave = new double[] { 0.70550, 0.70395, 0.70348, 0.70329, 1.00285, 1.00238, 1.00203, 1.00366, 1.30551, 1.30599, 1.30520, 1.30471, 1.58699, 1.58381, 1.58356, 1.58275 }; //m/s
                AirDistribution VaDistri = new AirDistribution();
                VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
                double[,] ma = new double[N_tube, Nelement];
                double[,] ha = new double[N_tube, Nelement];
                double[,] haw = new double[N_tube, Nelement];
                double H = Pt * N_tube;
                double Hx = L * H;
                double[] rho_a_st = { 1.19767, 1.19763, 1.19766, 1.19767, 1.19772, 1.19773, 1.19772, 1.19776, 1.19779, 1.19771, 1.19775, 1.19771, 1.19767, 1.19774, 1.19768, 1.19768 }; //kg/m3
                
                //空气侧几何结构选择
                //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
                //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
                //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
                //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
                int curve = 1; //

                double za = 1; //Adjust factor
                double zh = 1;
                double zdp = 1;
                double[] eta_surface = new double[] { 0.86998, 0.872736, 0.8747438, 0.8761934, 0.84895, 0.851907, 0.8535928, 0.8544665, 0.82776, 0.830934, 0.8327302, 0.8339071, 0.81128, 0.81624, 0.81832, 0.81969 };
                double[] hai = new double[] { 40.297, 39.319, 38.610, 38.100, 47.975, 46.873, 46.248, 45.925, 56.106, 54.862, 54.162, 53.705, 62.722, 60.704, 59.864, 59.315 };
                double[] hawi = new double[N];
                for (int j = 0; j < N_tube; j++)
                {
                    for (int k = 0; k < Nelement; k++)
                    {
                        ma[j, k] = VaDistri.Va[j, k] * (Vel_ave[i] / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st[i];
                        //ha[j, k] = AirHTC.alpha(VaDistri.Va[j, k] * (Vel_ave / VaDistri.Va_ave), za, curve);
                        ha[j, k] = hai[i];
                        haw[j, k] = hawi[i];
                    }
                }
                
                
                double[] tai = new double[] { 20.01, 20.02, 20.01, 20.01, 20.00, 20.00, 20.00, 19.99, 19.98, 20.00, 19.99, 20.00, 20.01, 19.99, 20.01, 20.01 };
                double[] RHi = new double[] { 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469 };

                double[] tri = new double[] { 45.01, 45.00, 44.98, 44.99, 45.01, 44.99, 45.00, 44.99, 45.01, 44.99, 44.99, 44.99, 45.00, 44.99, 44.98, 45.01 };
                double[] tc = tri;
                double[] pc = new double[N];
                //pc[i] = Refrigerant.SATT(fluid, composition, tc[i] + 273.15, 1).Pressure;
                coolprop.update(input_pairs.QT_INPUTS, 0, tc[i] + 273.15);
                pc[i] = coolprop.p() / 1000;
                //pc[i] = CoolProp.PropsSI("P", "T", tc[i] + 273.15, "Q", 0, fluid) / 1000;
                double Pwater = 395;//kpa
                double conductivity = 386; //w/mK for Cu
                int hexType = 1; //*********************************0 is evap, 1 is cond******************************************
                double[] hri = new double[N];
                //hri[i] = Refrigerant.TPFLSH(fluid, composition, tc[i] + 273.15, Pwater).h / wm - 0.5 ;
                coolprop.update(input_pairs.PT_INPUTS, Pwater * 1000, tc[i] + 273.15);
                hri[i] = coolprop.hmass() / 1000;
                //hri[i] = CoolProp.PropsSI("H", "T", tc[i] + 273.15, "P", Pwater * 1000, fluid) / 1000 ;

                double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
                double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

                string AirDirection = "逆流";
                ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai[i], tc[i], AirDirection);
                RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi[i], tc[i], AirDirection);



                //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, tc[i], pc[i], hri[i],
                    //mr[i], ma, ha, haw, eta_surface[i], zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

                res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, tc[i], pc[i], hri[i],
                    mr[i], ma, ha,haw, eta_surface[i], zh, zdp, hexType, thickness, conductivity, Pwater,AirDirection, d_cap, lenth_cap, coolprop);

                //using (StreamWriter wr = File.AppendText(@"D:\Work\Simulation\Test\Midea9_heat.txt"))
                //{
                //    wr.WriteLine("Q, {0}, DP, {1}, href, {2}, Ra_ratio, {3}, Tao, {4}, Tro, {5}", res.Q, res.DP, res.href, res.Ra_ratio, res.Tao, res.Tro);
                //}
                Q[i] = res.Q;
            }
            return res;
        }
        public static CalcResult Water_Cool_Jiayong6()
        {
            string fluid = "Water";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            CalcResult res = new CalcResult();
            int Nrow = 2;
            double[] FPI = new double[Nrow + 1];
            FPI = new double[] { 21.167, 21.167 };
            double Pt = 21 * 0.001;
            double Pr = 22 * 0.001;
            double Di = 6.8944 * 0.001;//8
            double Do = 7.35 * 0.001;//8.4
            double Fthickness = 0.095 * 0.001;
            double thickness = 0.5 * (Do - Di);
            int[] Ntube = { 16, 16 };
            int N_tube = Ntube[0];
            double L = 400 * 0.001;
            int Nelement = 2;
            int[,] CirArrange;
            CirArrange = new int[,] { { 17, 18, 19, 20, 4, 3, 2, 1 }, { 21, 22, 23, 24, 8, 7, 6, 5 }, { 25, 26, 27, 28, 12, 11, 10, 9 }, { 29, 30, 31, 32, 16, 15, 14, 13 } };
            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { 4, 4 };
            CircuitInfo.TubeofCir = new int[] { 8, 8, 8, 8 };  //{ 4, 8 };



            double[] d_cap = new double[] { 0, 0, 0, 0 };
            double[] lenth_cap = new double[] { 0, 0, 0, 0 };


            //AreaResult geo = new AreaResult();
            //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);

            double[] Q = new double[16];
            int N = 16;
            for (int i = 0; i < 1; i++)
            {
                double[] mr = new double[] { 13.01, 17.01, 21.01, 25.00, 13.01, 17.01, 21.00, 25.00, 13.01, 17.01, 21.01, 25.00, 13.01, 17.01, 21.01, 25.00 }; // 60;
                mr[i] = mr[i] / 60;
                double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
                //double Vel_ave = 1.6;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
                double[] Vel_ave = new double[] { 0.70538, 0.70457, 0.70465, 0.70472, 1.00341, 1.00451, 1.00502, 1.00570, 1.30334, 1.30465, 1.30518, 1.30566, 1.58873, 1.58858, 1.58567, 1.58517 }; //m/s
                AirDistribution VaDistri = new AirDistribution();
                VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
                double[,] ma = new double[N_tube, Nelement];
                double[,] ha = new double[N_tube, Nelement];
                double H = Pt * N_tube;
                double Hx = L * H;
                double[] rho_a_st = { 1.16847, 1.16850, 1.16849, 1.16853, 1.16842, 1.16846, 1.16856, 1.16840, 1.16848, 1.16840, 1.16844, 1.16848, 1.16845, 1.16845, 1.16848, 1.16849 }; //kg/m3

                //空气侧几何结构选择
                //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
                //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
                //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
                //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
                int curve = 1; //

                double za = 1; //Adjust factor
                double zh = 1;
                double zdp = 1;
                double[] eta_surface = new double[] { 0.86197, 0.863491, 0.8652783, 0.8663570, 0.83131, 0.830614, 0.8305166, 0.8318977, 0.80813, 0.809165, 0.8107805, 0.8109122, 0.78758, 0.78951, 0.79302, 0.79641 };
                double[] hai = new double[] { 40.297, 39.319, 38.610, 38.100, 47.975, 46.873, 46.248, 45.925, 56.106, 54.862, 54.162, 53.705, 62.722, 60.704, 59.864, 59.315 };
                for (int j = 0; j < N_tube; j++)
                {
                    for (int k = 0; k < Nelement; k++)
                    {
                        ma[j, k] = VaDistri.Va[j, k] * (Vel_ave[i] / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st[i];
                        //ha[j, k] = AirHTC.alpha(VaDistri.Va[j, k] * (Vel_ave / VaDistri.Va_ave), za, curve);
                        ha[j, k] = hai[i];
                    }
                }
                double[,] haw = ha;

                
                double[] tai = new double[] { 27.01, 27.00, 27.00, 26.99, 27.02, 27.01, 26.98, 27.02, 27.00, 27.02, 27.01, 27.00, 27.01, 27.01, 27.00, 27.00 };
                double[] RHi = new double[] { 0.4680, 0.4684, 0.4696, 0.4694, 0.4681, 0.4685, 0.4705, 0.4698, 0.4701, 0.4692, 0.4697, 0.4701, 0.4691, 0.4691, 0.4701, 0.4690 };

                double[] tri = new double[] { 9.98, 10.01, 10.02, 10.01, 9.99, 10.03, 9.99, 10.03, 9.99, 9.99, 10.01, 10.00, 10.00, 10.01, 9.98, 10.02 };
                double[] tc = tri;
                double[] pc = new double[N];
                coolprop.update(input_pairs.QT_INPUTS, 0, tc[i] + 273.15);
                pc[i] = coolprop.p() / 1000;
                //pc[i] = CoolProp.PropsSI("P", "T", tc[i] + 273.15, "Q", 1, fluid) / 1000;
                double Pwater = 395;//kpa
                double conductivity = 386; //w/mK for Cu
                int hexType = 0; //*********************************0 is evap, 1 is cond******************************************
                double[] hri = new double[N];
                coolprop.update(input_pairs.PT_INPUTS, Pwater * 1000, tc[i] + 273.15);
                hri[i] = coolprop.hmass() / 1000;
                //hri[i] = CoolProp.PropsSI("H", "T", tc[i] + 273.15, "P", Pwater * 1000, fluid) / 1000 ;

                double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
                double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

                string AirDirection = "逆流";
                ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai[i], tc[i], AirDirection);
                RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi[i], tc[i], AirDirection);



                //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, tc[i], pc[i], hri[i],
                    //mr[i], ma, ha, haw, eta_surface[i], zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

                res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, tc[i], pc[i], hri[i],
                    mr[i], ma, ha,haw, eta_surface[i], zh, zdp, hexType, thickness, conductivity, Pwater,AirDirection, d_cap, lenth_cap, coolprop);

                //using (StreamWriter wr = File.AppendText(@"D:\Work\Simulation\Test\Midea9_heat.txt"))
                //{
                //    wr.WriteLine("Q, {0}, DP, {1}, href, {2}, Ra_ratio, {3}, Tao, {4}, Tro, {5}", res.Q, res.DP, res.href, res.Ra_ratio, res.Tao, res.Tro);
                //}
                Q[i] = res.Q;
            }
            return res;
        }
        public static CalcResult Water_Heat_Jiayong2()
        {
            string fluid = "Water";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            CalcResult res = new CalcResult();
            int Nrow = 2;
            double[] FPI = new double[Nrow + 1];
            FPI = new double[] { 21.167, 21.167 };
            double Pt = 14.5 * 0.001;
            double Pr = 12.56 * 0.001;
            double Di = 4.8706 * 0.001;//8
            double Do = 5.25 * 0.001;//8.4
            double Fthickness = 0.095 * 0.001;
            double thickness = 0.5 * (Do - Di);
            int[] Ntube = { 24, 24 };
            int N_tube = Ntube[0];
            double L = 400 * 0.001;
            int Nelement = 1;
            int[,] CirArrange;
            CirArrange = new int[,] { { 25, 26, 27, 28, 4, 3, 2, 1 }, { 29, 30, 31, 32, 8, 7, 6, 5 }, { 33, 34, 35, 36, 12, 11, 10, 9 }, { 37, 38, 39, 40, 16, 15, 14, 13 }, { 41, 42, 43, 44, 20, 19, 18, 17 }, { 45, 46, 47, 48, 24, 23, 22, 21 } };
            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { 6, 6 };
            CircuitInfo.TubeofCir = new int[] { 8, 8, 8, 8, 8, 8 };  //{ 4, 8 };



            double[] d_cap = new double[] { 0, 0, 0, 0, 0, 0 };
            double[] lenth_cap = new double[] { 0, 0, 0, 0, 0, 0 };


            //AreaResult geo = new AreaResult();
            //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);

            double[] Q = new double[16];
            int N = 16;
            for (int i = 0; i < N; i++)
            {
                double[] mr = new double[] { 13.00, 17.00, 21.00, 25.00, 13.00, 17.01, 21.01, 25.00, 13.01, 17.00, 21.00, 25.00, 13.01, 17.00, 21.01, 25.00 }; // 60;
                mr[i] = mr[i] / 60;
                double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
                //double Vel_ave = 1.6;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
                double[] Vel_ave = new double[] { 0.702466, 0.70428, 0.70592, 0.70592, 1.00585, 1.00475, 1.00716, 1.00635, 1.31044, 1.30957, 1.30901, 1.30781, 1.58978, 1.58832, 1.58607, 1.59028 }; //m/s
                AirDistribution VaDistri = new AirDistribution();
                VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
                double[,] ma = new double[N_tube, Nelement];
                double[,] ha = new double[N_tube, Nelement];
                double H = Pt * N_tube;
                double Hx = L * H;
                double[] rho_a_st = { 1.19772, 1.19771, 1.19777, 1.19762, 1.19773, 1.19773, 1.19775, 1.19767, 1.19772, 1.19773, 1.19770, 1.19772, 1.19766, 1.19771, 1.19771, 1.19775 }; //kg/m3

                //空气侧几何结构选择
                //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
                //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
                //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
                //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
                int curve = 1; //

                double za = 1; //Adjust factor
                double zh = 1;
                double zdp = 1;
                double[] eta_surface = new double[] { 0.93687, 0.936250, 0.9295073, 0.9333229, 0.92812, 0.927387, 0.9259171, 0.9252950, 0.92324, 0.92239, 0.92222, 0.92211, 0.91866, 0.91823, 0.91817, 0.91800 };
                double[] hai = new double[] { 56.798, 57.390, 63.920, 60.213, 65.277, 65.993, 67.436, 68.048, 70.078, 70.917, 71.086, 71.197, 74.628, 75.059, 75.112, 75.283 };
                for (int j = 0; j < N_tube; j++)
                {
                    for (int k = 0; k < Nelement; k++)
                    {
                        ma[j, k] = VaDistri.Va[j, k] * (Vel_ave[i] / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st[i];
                        //ha[j, k] = AirHTC.alpha(VaDistri.Va[j, k] * (Vel_ave / VaDistri.Va_ave), za, curve);
                        ha[j, k] = hai[i];
                    }
                }
                double[,] haw = ha;

                
                double[] tai = new double[] { 20.00, 20.00, 19.98, 20.02, 20.00, 20.00, 19.99, 20.01, 20.00, 20.00, 20.01, 20.00, 20.01, 20.00, 20.00, 19.99 };
                double[] RHi = new double[] { 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469 };

                double[] tri = new double[] { 44.99, 44.99, 45.01, 44.99, 45.02, 45.02, 44.99, 45.01, 45.01, 45.03, 44.98, 44.99, 45.02, 44.98, 45.03, 44.99 };
                double[] tc = tri;
                double[] pc = new double[N];
                coolprop.update(input_pairs.QT_INPUTS, 0, tc[i] + 273.15);
                pc[i] = coolprop.p() / 1000;
                //pc[i] = CoolProp.PropsSI("P", "T", tc[i] + 273.15, "Q", 1, fluid) / 1000;

                double Pwater = 395;//kpa
                double conductivity = 386; //w/mK for Cu
                int hexType = 1; //*********************************0 is evap, 1 is cond******************************************
                double[] hri = new double[N];
                coolprop.update(input_pairs.PT_INPUTS, Pwater * 1000, tc[i] + 273.15);
                hri[i] = coolprop.hmass() / 1000;
                //hri[i] = CoolProp.PropsSI("H", "T", tc[i] + 273.15, "P", Pwater * 1000, fluid) / 1000 ;

                double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
                double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

                string AirDirection = "逆流";
                ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai[i], tc[i], AirDirection);
                RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi[i], tc[i], AirDirection);



                //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, tc[i], pc[i], hri[i],
                    //mr[i], ma, ha, haw, eta_surface[i], zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

                res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, tc[i], pc[i], hri[i],
                    mr[i], ma, ha,haw, eta_surface[i], zh, zdp, hexType, thickness, conductivity, Pwater,AirDirection, d_cap, lenth_cap, coolprop);

                //using (StreamWriter wr = File.AppendText(@"D:\Work\Simulation\Test\Midea9_heat.txt"))
                //{
                //    wr.WriteLine("Q, {0}, DP, {1}, href, {2}, Ra_ratio, {3}, Tao, {4}, Tro, {5}", res.Q, res.DP, res.href, res.Ra_ratio, res.Tao, res.Tro);
                //}
                Q[i] = res.Q;
            }
            return res;
        }
        public static CalcResult Water_Cool_Jiayong2()
        {
            string fluid = "Water";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            CalcResult res = new CalcResult();
            int Nrow = 2;
            double[] FPI = new double[Nrow + 1];
            FPI = new double[] { 21.167, 21.167 };
            double Pt = 14.5 * 0.001;
            double Pr = 12.56 * 0.001;
            double Di = 4.8706 * 0.001;//8
            double Do = 5.25 * 0.001;//8.4
            double Fthickness = 0.095 * 0.001;
            double thickness = 0.5 * (Do - Di);
            int[] Ntube = { 24, 24 };
            int N_tube = Ntube[0];
            double L = 400 * 0.001;
            int Nelement = 1;
            int[,] CirArrange;
            CirArrange = new int[,] { { 25, 26, 27, 28, 4, 3, 2, 1 }, { 29, 30, 31, 32, 8, 7, 6, 5 }, { 33, 34, 35, 36, 12, 11, 10, 9 }, { 37, 38, 39, 40, 16, 15, 14, 13 }, { 41, 42, 43, 44, 20, 19, 18, 17 }, { 45, 46, 47, 48, 24, 23, 22, 21 } };
            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { 6, 6 };
            CircuitInfo.TubeofCir = new int[] { 8, 8, 8, 8, 8, 8 };  //{ 4, 8 };


            double[] d_cap = new double[] { 0, 0, 0, 0, 0, 0 };
            double[] lenth_cap = new double[] { 0, 0, 0, 0, 0, 0 };

            //AreaResult geo = new AreaResult();
            //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);

            double[] Q = new double[16];
            int N = 16;
            for (int i = 0; i < N; i++)
            {
                double[] mr = new double[] { 13.01, 17.01, 21.01, 25.00, 13.00, 17.00, 21.01, 25.00, 13.00, 17.00, 21.00, 25.01, 13.01, 17.01, 21.00, 25.00 }; // 60;
                mr[i] = mr[i] / 60;
                double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
                //double Vel_ave = 1.6;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
                double[] Vel_ave = new double[] { 0.70813, 0.70656, 0.70680, 0.70728, 1.00764, 1.005388, 1.00521, 1.00649, 1.30721, 1.30791, 1.30833, 1.30897, 1.58697, 1.58870, 1.59016, 1.58798 }; //m/s
                AirDistribution VaDistri = new AirDistribution();
                VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
                double[,] ma = new double[N_tube, Nelement];
                double[,] ha = new double[N_tube, Nelement];
                double H = Pt * N_tube;
                double Hx = L * H;
                double[] rho_a_st = { 1.16850, 1.16845, 1.16841, 1.16846, 1.16850, 1.16849, 1.16849, 1.16845, 1.16848, 1.16846, 1.16848, 1.16850, 1.16848, 1.16846, 1.16842, 1.16847 }; //kg/m3
                
                //空气侧几何结构选择
                //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
                //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
                //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
                //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
                int curve = 1; //

                double za = 1; //Adjust factor
                double zh = 1;
                double zdp = 1;
                double[] eta_surface = new double[] { 0.93114, 0.928620, 0.9280530, 0.9278466, 0.92327, 0.920580, 0.9193262, 0.9188672, 0.91820, 0.91618, 0.91352, 0.90995, 0.91237, 0.90993, 0.90838, 0.90781 };
                double[] hai = new double[] { 56.798, 57.390, 63.920, 60.213, 65.277, 65.993, 67.436, 68.048, 70.078, 70.917, 71.086, 71.197, 74.628, 75.059, 75.112, 75.283 };
                for (int j = 0; j < N_tube; j++)
                {
                    for (int k = 0; k < Nelement; k++)
                    {
                        ma[j, k] = VaDistri.Va[j, k] * (Vel_ave[i] / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st[i];
                        //ha[j, k] = AirHTC.alpha(VaDistri.Va[j, k] * (Vel_ave / VaDistri.Va_ave), za, curve);
                        ha[j, k] = hai[i];
                    }
                }
                double[,] haw = ha;
                
                
                double[] tai = new double[] { 27.00, 27.01, 27.02, 27.01, 27.00, 27.00, 27.00, 27.01, 27.00, 27.01, 27.00, 27.00, 27.00, 27.01, 27.02, 27.01 };
                double[] RHi = new double[] { 0.4684, 0.4691, 0.4686, 0.4685, 0.4684, 0.4690, 0.4696, 0.4691, 0.4701, 0.4685, 0.4701, 0.4684, 0.4701, 0.4685, 0.4681, 0.4680 };

                double[] tri = new double[] { 10.02, 10.00, 9.98, 10.00, 10.01, 10.00, 10.00, 10.01, 9.98, 9.99, 10.02, 10.00, 10.00, 10.02, 10.01, 10.01 };
                double[] tc = tri;
                double[] pc = new double[N];
                coolprop.update(input_pairs.QT_INPUTS, 0, tc[i] + 273.15);
                pc[i] = coolprop.p() / 1000;
                //pc[i] = CoolProp.PropsSI("P", "T", tc[i] + 273.15, "Q", 1, fluid) / 1000;

                double Pwater = 395;//kpa
                double conductivity = 386; //w/mK for Cu
                int hexType = 0; //*********************************0 is evap, 1 is cond******************************************
                double[] hri = new double[N];
                coolprop.update(input_pairs.PT_INPUTS, Pwater * 1000, tc[i] + 273.15);
                hri[i] = coolprop.hmass() / 1000;
                //hri[i] = CoolProp.PropsSI("H", "T", tc[i] + 273.15, "P", Pwater * 1000, fluid) / 1000 ;

                double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
                double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

                string AirDirection = "逆流";
                ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai[i], tc[i], AirDirection);
                RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi[i], tc[i], AirDirection);



                //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, tc[i], pc[i], hri[i],
                    //mr[i], ma, ha, haw, eta_surface[i], zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

                res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, tc[i], pc[i], hri[i],
                    mr[i], ma, ha,haw, eta_surface[i], zh, zdp, hexType, thickness, conductivity, Pwater,AirDirection, d_cap, lenth_cap, coolprop);

                //using (StreamWriter wr = File.AppendText(@"D:\Work\Simulation\Test\Midea9_heat.txt"))
                //{
                //    wr.WriteLine("Q, {0}, DP, {1}, href, {2}, Ra_ratio, {3}, Tao, {4}, Tro, {5}", res.Q, res.DP, res.href, res.Ra_ratio, res.Tao, res.Tro);
                //}
                Q[i] = res.Q;
            }
            return res;
        }

        public static CalcResult Water_Heat_Jiayong6_reverse()
        {
            string fluid = "Water";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            CalcResult res = new CalcResult();
            int Nrow = 2;
            double[] FPI = new double[Nrow + 1];
            FPI = new double[] { 21.167, 21.167 };
            double Pt = 21 * 0.001;
            double Pr = 22 * 0.001;
            double Di = 6.8944 * 0.001;//8
            double Do = 7.35 * 0.001;//8.4
            double Fthickness = 0.095 * 0.001;
            double thickness = 0.5 * (Do - Di);
            int[] Ntube = { 16, 16 };
            int N_tube = Ntube[0];
            double L = 400 * 0.001;
            int Nelement = 1;
            int[,] CirArrange;

            /*int flowType = 0;//*********************************0:origin, 1:reverse******************************************
            if (flowType == 0)
            {
                CirArrange = new int[,] { { 29, 30, 16, 15, 1, 2 }, { 32, 31, 17, 18, 4, 3 }, { 33, 34, 20, 19, 5, 6 }, { 36, 35, 21, 22, 8, 7 }, { 37, 38, 24, 23, 9, 10 }, { 40, 39, 25, 26, 12, 11 }, { 41, 42, 28, 27, 13, 14 } };
            }
            else
            {
                CirArrange = new int[,] { { 2, 1, 15, 16, 30, 29 }, { 3, 4, 18, 17, 31, 32 }, { 6, 5, 19, 20, 34, 33 }, { 7, 8, 22, 21, 35, 36 }, { 10, 9, 23, 24, 38, 37 }, { 11, 12, 26, 25, 39, 40 }, { 14, 13, 27, 28, 42, 41 } };
            }*/

            CirArrange = new int[,] { { 17, 18, 19, 3, 2, 1, 0, 0 }, { 20, 21, 22, 6, 5, 4, 0, 0 }, { 23, 24, 25, 9, 8, 7, 0, 0 }, { 26, 27, 28, 12, 11, 10, 0, 0 }, { 29, 30, 31, 32, 16, 15, 14, 13 } };

            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { 5, 5 };
            CircuitInfo.TubeofCir = new int[] { 6, 6, 6, 6, 8 };  //{ 4, 8 };

            //Circuit-Reverse module
            bool reverse = true; //*********************************false:origin, true:reverse******************************************
            CirArrange = CircuitReverse.CirReverse(reverse, CirArrange, CircuitInfo);


            double[] d_cap = new double[] { 0, 0, 0, 0, 0 };
            double[] lenth_cap = new double[] { 0, 0, 0, 0, 0 };

            //AreaResult geo = new AreaResult();
            //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);

            double[] Q = new double[16];
            int N = 16;
            for (int i = 0; i < N; i++)
            {
                double[] mr = new double[] { 13.01, 17.01, 21.00, 25.00, 13.00, 17.01, 21.00, 25.00, 13.00, 17.00, 21.00, 25.01, 13.01, 17.00, 21.01, 25.00 }; // 60;
                mr[i] = mr[i] / 60;
                double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
                //double Vel_ave = 1.6;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
                double[] Vel_ave = new double[] { 0.70550, 0.70395, 0.70348, 0.70329, 1.00285, 1.00238, 1.00203, 1.00366, 1.30551, 1.30599, 1.30520, 1.30471, 1.58699, 1.58381, 1.58356, 1.58275 }; //m/s
                AirDistribution VaDistri = new AirDistribution();
                VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
                double[,] ma = new double[N_tube, Nelement];
                double[,] ha = new double[N_tube, Nelement];
                double H = Pt * N_tube;
                double Hx = L * H;
                double[] rho_a_st = { 1.19767, 1.19763, 1.19766, 1.19767, 1.19772, 1.19773, 1.19772, 1.19776, 1.19779, 1.19771, 1.19775, 1.19771, 1.19767, 1.19774, 1.19768, 1.19768 }; //kg/m3

                //空气侧几何结构选择
                //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
                //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
                //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
                //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
                int curve = 1; //

                double za = 1; //Adjust factor
                double zh = 1;
                double zdp = 1;
                double[] eta_surface = new double[] { 0.86998, 0.872736, 0.8747438, 0.8761934, 0.84895, 0.851907, 0.8535928, 0.8544665, 0.82776, 0.830934, 0.8327302, 0.8339071, 0.81128, 0.81624, 0.81832, 0.81969 };
                double[] hai = new double[] { 40.297, 39.319, 38.610, 38.100, 47.975, 46.873, 46.248, 45.925, 56.106, 54.862, 54.162, 53.705, 62.722, 60.704, 59.864, 59.315 };
                for (int j = 0; j < N_tube; j++)
                {
                    for (int k = 0; k < Nelement; k++)
                    {
                        ma[j, k] = VaDistri.Va[j, k] * (Vel_ave[i] / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st[i];
                        //ha[j, k] = AirHTC.alpha(VaDistri.Va[j, k] * (Vel_ave / VaDistri.Va_ave), za, curve);
                        ha[j, k] = hai[i];
                    }
                }
                double[,] haw = ha;
                
                double[] tai = new double[] { 20.01, 20.02, 20.01, 20.01, 20.00, 20.00, 20.00, 19.99, 19.98, 20.00, 19.99, 20.00, 20.01, 19.99, 20.01, 20.01 };
                double[] RHi = new double[] { 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469 };

                double[] tri = new double[] { 45.01, 45.00, 44.98, 44.99, 45.01, 44.99, 45.00, 44.99, 45.01, 44.99, 44.99, 44.99, 45.00, 44.99, 44.98, 45.01 };
                double[] tc = tri;
                double[] pc = new double[N];
                coolprop.update(input_pairs.QT_INPUTS, 0, tc[i] + 273.15);
                pc[i] = coolprop.p() / 1000;
                //pc[i] = CoolProp.PropsSI("P", "T", tc[i] + 273.15, "Q", 1, fluid) / 1000;

                double Pwater = 395;//kpa
                double conductivity = 386; //w/mK for Cu
                int hexType = 1; //*********************************0 is evap, 1 is cond******************************************
                double[] hri = new double[N];
                coolprop.update(input_pairs.PT_INPUTS, Pwater * 1000, tc[i] + 273.15);
                hri[i] = coolprop.hmass() / 1000;
                //hri[i] = CoolProp.PropsSI("H", "T", tc[i] + 273.15, "P", Pwater * 1000, fluid) / 1000 ;

                double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
                double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

                string AirDirection = "逆流";
                ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai[i], tc[i], AirDirection);
                RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi[i], tc[i], AirDirection);



                //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, tc[i], pc[i], hri[i],
                    //mr[i], ma, ha, haw, eta_surface[i], zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

                res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, tc[i], pc[i], hri[i],
                    mr[i], ma, ha,haw, eta_surface[i], zh, zdp, hexType, thickness, conductivity, Pwater,AirDirection, d_cap, lenth_cap, coolprop);

                //using (StreamWriter wr = File.AppendText(@"D:\Work\Simulation\Test\Midea9_heat.txt"))
                //{
                //    wr.WriteLine("Q, {0}, DP, {1}, href, {2}, Ra_ratio, {3}, Tao, {4}, Tro, {5}", res.Q, res.DP, res.href, res.Ra_ratio, res.Tao, res.Tro);
                //}
                Q[i] = res.Q;
            }
            return res;
        }
        public static CalcResult Water_Heat_Jiayong2_reverse()
        {
            string fluid = "Water";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            CalcResult res = new CalcResult();
            int Nrow = 2;
            double[] FPI = new double[Nrow + 1];
            FPI = new double[] { 21.167, 21.167 };
            double Pt = 14.5 * 0.001;
            double Pr = 12.56 * 0.001;
            double Di = 4.8706 * 0.001;//8
            double Do = 5.25 * 0.001;//8.4
            double Fthickness = 0.095 * 0.001;
            double thickness = 0.5 * (Do - Di);
            int[] Ntube = { 24, 24 };
            int N_tube = Ntube[0];
            double L = 400 * 0.001;
            int Nelement = 1;
            int[,] CirArrange;
            CirArrange = new int[,] { { 25, 26, 27, 28, 4, 3, 2, 1 }, { 29, 30, 31, 32, 8, 7, 6, 5 }, { 33, 34, 35, 36, 12, 11, 10, 9 }, { 37, 38, 39, 40, 16, 15, 14, 13 }, { 41, 42, 43, 44, 20, 19, 18, 17 }, { 45, 46, 47, 48, 24, 23, 22, 21 } };
            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { 6, 6 };
            CircuitInfo.TubeofCir = new int[] { 8, 8, 8, 8, 8, 8 };  //{ 4, 8 };

            //Circuit-Reverse module
            bool reverse = true; //*********************************false:origin, true:reverse******************************************
            CirArrange = CircuitReverse.CirReverse(reverse, CirArrange, CircuitInfo);


            double[] d_cap = new double[] { 0, 0, 0, 0, 0, 0 };
            double[] lenth_cap = new double[] { 0, 0, 0, 0, 0, 0 };

            //AreaResult geo = new AreaResult();
            //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);

            double[] Q = new double[16];
            int N = 16;
            for (int i = 0; i < N; i++)
            {
                double[] mr = new double[] { 13.00, 17.00, 21.00, 25.00, 13.00, 17.01, 21.01, 25.00, 13.01, 17.00, 21.00, 25.00, 13.01, 17.00, 21.01, 25.00 }; // 60;
                mr[i] = mr[i] / 60;
                double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
                //double Vel_ave = 1.6;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
                double[] Vel_ave = new double[] { 0.702466, 0.70428, 0.70592, 0.70592, 1.00585, 1.00475, 1.00716, 1.00635, 1.31044, 1.30957, 1.30901, 1.30781, 1.58978, 1.58832, 1.58607, 1.59028 }; //m/s
                AirDistribution VaDistri = new AirDistribution();
                VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
                double[,] ma = new double[N_tube, Nelement];
                double[,] ha = new double[N_tube, Nelement];
                double H = Pt * N_tube;
                double Hx = L * H;
                double[] rho_a_st = { 1.19772, 1.19771, 1.19777, 1.19762, 1.19773, 1.19773, 1.19775, 1.19767, 1.19772, 1.19773, 1.19770, 1.19772, 1.19766, 1.19771, 1.19771, 1.19775 }; //kg/m3

                //空气侧几何结构选择
                //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
                //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
                //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
                //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
                int curve = 1; //

                double za = 1; //Adjust factor
                double zh = 1;
                double zdp = 1;
                double[] eta_surface = new double[] { 0.93687, 0.936250, 0.9295073, 0.9333229, 0.92812, 0.927387, 0.9259171, 0.9252950, 0.92324, 0.92239, 0.92222, 0.92211, 0.91866, 0.91823, 0.91817, 0.91800 };
                double[] hai = new double[] { 56.798, 57.390, 63.920, 60.213, 65.277, 65.993, 67.436, 68.048, 70.078, 70.917, 71.086, 71.197, 74.628, 75.059, 75.112, 75.283 };
                for (int j = 0; j < N_tube; j++)
                {
                    for (int k = 0; k < Nelement; k++)
                    {
                        ma[j, k] = VaDistri.Va[j, k] * (Vel_ave[i] / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st[i];
                        //ha[j, k] = AirHTC.alpha(VaDistri.Va[j, k] * (Vel_ave / VaDistri.Va_ave), za, curve);
                        ha[j, k] = hai[i];
                    }
                }
                double[,] haw = ha;
                
                
                double[] tai = new double[] { 20.00, 20.00, 19.98, 20.02, 20.00, 20.00, 19.99, 20.01, 20.00, 20.00, 20.01, 20.00, 20.01, 20.00, 20.00, 19.99 };
                double[] RHi = new double[] { 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469 };

                double[] tri = new double[] { 44.99, 44.99, 45.01, 44.99, 45.02, 45.02, 44.99, 45.01, 45.01, 45.03, 44.98, 44.99, 45.02, 44.98, 45.03, 44.99 };
                double[] tc = tri;
                double[] pc = new double[N];
                coolprop.update(input_pairs.QT_INPUTS, 0, tc[i] + 273.15);
                pc[i] = coolprop.p() / 1000;
                //pc[i] = CoolProp.PropsSI("P", "T", tc[i] + 273.15, "Q", 1, fluid) / 1000;

                double Pwater = 395;//kpa
                double conductivity = 386; //w/mK for Cu
                int hexType = 1; //*********************************0 is evap, 1 is cond******************************************
                double[] hri = new double[N];
                coolprop.update(input_pairs.PT_INPUTS, Pwater * 1000, tc[i] + 273.15);
                hri[i] = coolprop.hmass() / 1000;
                //hri[i] = CoolProp.PropsSI("H", "T", tc[i] + 273.15, "P", Pwater * 1000, fluid) / 1000 ;

                double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
                double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

                string AirDirection = "逆流";
                ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai[i], tc[i], AirDirection);
                RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi[i], tc[i], AirDirection);



                //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, tc[i], pc[i], hri[i],
                    //mr[i], ma, ha, haw, eta_surface[i], zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

                res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, tc[i], pc[i], hri[i],
                    mr[i], ma, ha,haw, eta_surface[i], zh, zdp, hexType, thickness, conductivity, Pwater,AirDirection, d_cap, lenth_cap, coolprop);

                //using (StreamWriter wr = File.AppendText(@"D:\Work\Simulation\Test\Midea9_heat.txt"))
                //{
                //    wr.WriteLine("Q, {0}, DP, {1}, href, {2}, Ra_ratio, {3}, Tao, {4}, Tro, {5}", res.Q, res.DP, res.href, res.Ra_ratio, res.Tao, res.Tro);
                //}
                Q[i] = res.Q;
            }
            return res;
        }
        public static CalcResult Water_Midea9_reverse()
        {
            string fluid = "Water";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            CalcResult res = new CalcResult();
            int Nrow = 1;
            double[] FPI = new double[Nrow + 1];
            FPI = new double[] { 21 };
            double Pt = 21 * 0.001;
            double Pr = 18.19 * 0.001;
            double Di = 6.8944 * 0.001;//8
            double Do = 7.35 * 0.001;//8.4
            double Fthickness = 0.095 * 0.001;
            double thickness = 0.5 * (Do - Di);
            int[] Ntube = { 12 };
            int N_tube = Ntube[0];
            double L = 410 * 0.001;
            int Nelement = 1;
            int[,] CirArrange;
            CirArrange = new int[,] { { 1, 2, 3, 4 }, { 5, 6, 7, 8 }, { 9, 10, 11, 12 } };
            //CirArrange = new int[,] { { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 } };
            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { 3, 3 };
            CircuitInfo.TubeofCir = new int[] { 4, 4, 4 };  //{ 4, 8 };
            // [19 - 17 - 15 - 13   11   9   7   5   3   1] <====Air
            // [20 - 18 - 16 - 14   12   10  8   6   4   2] <====Air
            
            //Circuit-Reverse module
            bool reverse = true; //*********************************false:origin, true:reverse******************************************
            CirArrange = CircuitReverse.CirReverse(reverse, CirArrange, CircuitInfo);

            double[] d_cap = new double[] { 0, 0, 0 };
            double[] lenth_cap = new double[] { 0, 0, 0 };

            GeometryInput geoInput_air = new GeometryInput();
            geoInput_air.Pt = Pt;
            geoInput_air.Pr = Pr;
            geoInput_air.Do = Do;
            geoInput_air.Fthickness = Fthickness;
            geoInput_air.FPI = FPI[0];
            geoInput_air.Nrow = Nrow;

            int hexType = 1;

            double mr = 9.99/60;
            double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
            double Vel_ave = 2;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
            AirDistribution VaDistri = new AirDistribution();
            VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
            double[,] ma = new double[N_tube, Nelement];
            double[,] ha = new double[N_tube, Nelement];
            double H = Pt * N_tube;
            double Hx = L * H;
            double rho_a_st = 1.188;
            double za = 1;

            //空气侧几何结构选择
            //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
            //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
            int curve = 1;

            for (int i = 0; i < N_tube; i++)
            {
                for (int j = 0; j < Nelement; j++)
                {
                    ma[i, j] = VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st;
                    //ha[i, j] = AirHTC.alpha(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve);
                    //ha[i, j] = 79;
                    ha[i, j] = AirHTC.alpha1(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve, geoInput_air, hexType).ha;
                }
            }
            double[,] haw = ha;

            res.DPa = AirHTC.alpha1(Vel_ave, za, curve, geoInput_air, hexType).dP_a;

            double eta_surface = 0.8284;
            double zh = 1;
            double zdp = 1.5;

            double tai = 19.98;
            double RHi = 0.469;
            double tri = 44.98;
            double tc = tri;
            coolprop.update(input_pairs.QT_INPUTS, 1, tc + 273.15);
            double pc = coolprop.p() / 1000;
            //double pc = CoolProp.PropsSI("P", "T", tc+ 273.15, "Q", 1, fluid) / 1000;
            double Pwater = 395;//kpa
            double conductivity = 386; //w/mK for Cu
            //int hexType = 1; //*********************************0 is evap, 1 is cond******************************************
            coolprop.update(input_pairs.PT_INPUTS, Pwater * 1000, tc + 273.15);
            double hri = coolprop.hmass() / 1000;
            //double hri = CoolProp.PropsSI("H", "T", tc + 273.15, "P", Pwater * 1000, fluid) / 1000 ;


            double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
            double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

            //string AirDirection="DowntoUp";
            string AirDirection = "逆流";
            ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai, tc, AirDirection);
            RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi, tc, AirDirection);


            //AreaResult geo = new AreaResult();
            //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, tc, pc, hri,
                //mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, tc, pc, hri,
                mr, ma, ha,haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater,AirDirection, d_cap, lenth_cap,coolprop);


            return res;
        }
        public static CalcResult Water_Heat_Zhongyang1_reverse()
        {
            string fluid = "Water";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            CalcResult res = new CalcResult();
            int Nrow = 3;
            double[] FPI = new double[Nrow + 1];
            FPI = new double[] { 16.933, 16.933, 16.933 };
            double Pt = 21.0 * 0.001;
            double Pr = 19.4 * 0.001;
            double Di = 6.8944 * 0.001;//8
            double Do = 7.35 * 0.001;//8.4
            double Fthickness = 0.1 * 0.001;
            double thickness = 0.5 * (Do - Di);
            int[] Ntube = { 18, 18, 18 };
            int N_tube = Ntube[0];
            double L = 438.0 * 0.001;
            int Nelement = 1;
            int[,] CirArrange;
            CirArrange = new int[,] { { 37, 38, 39, 40, 41, 42, 24, 23, 22, 21, 20, 19, 1, 2, 3, 4, 5, 6 }, { 43, 44, 45, 46, 47, 48, 30, 29, 28, 27, 26, 25, 7, 8, 9, 10, 11, 12 }, { 49, 50, 51, 52, 53, 54, 36, 35, 34, 33, 32, 31, 13, 14, 15, 16, 17, 18 } };
            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { 3, 3 };
            CircuitInfo.TubeofCir = new int[] { 18, 18, 18 };  //{ 4, 8 };

            //Circuit-Reverse module
            bool reverse = true; //*********************************false:origin, true:reverse******************************************
            CirArrange = CircuitReverse.CirReverse(reverse, CirArrange, CircuitInfo);


            double[] d_cap = new double[] { 0, 0, 0 };
            double[] lenth_cap = new double[] { 0, 0, 0 };

            //AreaResult geo = new AreaResult();
            //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);

            double[] Q = new double[16];
            int N = 16;
            for (int i = 0; i < N; i++)
            {
                double[] mr = new double[] { 13.01, 17.01, 21.00, 25.00, 13.01, 17.01, 21.00, 25.00, 13.00, 17.01, 21.01, 25.00, 13.00, 17.00, 21.01, 25.00 }; // 60;
                mr[i] = mr[i] / 60;
                double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
                //double Vel_ave = 1.6;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
                double[] Vel_ave = new double[] { 0.60061, 0.60059, 0.59874, 0.60039, 0.89766, 0.89707, 0.89755, 0.89636, 1.09706, 1.09812, 1.09922, 1.09919, 1.29987, 1.29968, 1.29857, 1.29670 }; //m/s
                AirDistribution VaDistri = new AirDistribution();
                VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
                double[,] ma = new double[N_tube, Nelement];
                double[,] ha = new double[N_tube, Nelement];
                double H = Pt * N_tube;
                double Hx = L * H;
                double[] rho_a_st = { 1.19775, 1.19775, 1.19766, 1.19778, 1.19777, 1.19769, 1.19770, 1.19770, 1.19754, 1.19770, 1.19777, 1.19767, 1.19780, 1.19761, 1.19751, 1.19766 }; //kg/m3

                //空气侧几何结构选择
                //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
                //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
                //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
                //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
                int curve = 1; //

                double za = 1; //Adjust factor
                double zh = 1;
                double zdp = 1;
                double[] eta_surface = new double[] { 0.91638, 0.917989, 0.9188580, 0.9193723, 0.88357, 0.884134, 0.8849341, 0.8834665, 0.86450, 0.865326, 0.8658559, 0.8664762, 0.85036, 0.85115, 0.85198, 0.85284 };
                double[] hai = new double[] { 32.230, 31.553, 31.189, 30.974, 46.541, 46.285, 45.924, 46.587, 55.360, 54.968, 54.718, 54.426, 62.153, 61.768, 61.363, 60.944 };
                for (int j = 0; j < N_tube; j++)
                {
                    for (int k = 0; k < Nelement; k++)
                    {
                        ma[j, k] = VaDistri.Va[j, k] * (Vel_ave[i] / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st[i];
                        //ha[j, k] = AirHTC.alpha(VaDistri.Va[j, k] * (Vel_ave / VaDistri.Va_ave), za, curve);
                        ha[j, k] = hai[i];
                    }
                }
                double[,] haw = ha;
                
                double[] tai = new double[] { 20.00, 20.00, 20.02, 19.99, 19.99, 20.01, 20.01, 20.01, 20.05, 20.01, 19.99, 20.02, 19.98, 20.03, 20.06, 20.02 };
                double[] RHi = new double[] { 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469 };

                double[] tri = new double[] { 45.00, 45.00, 44.99, 45.00, 45.03, 45.02, 45.01, 44.99, 45.01, 45.01, 45.00, 44.99, 45.03, 45.01, 45.01, 45.02 };
                double[] tc = tri;
                double[] pc = new double[N];
                coolprop.update(input_pairs.QT_INPUTS, 1, tc[i] + 273.15);
                pc[i] = coolprop.p() / 1000;
                //pc[i] = CoolProp.PropsSI("P", "T", tc[i] + 273.15, "Q", 1, fluid) / 1000;

                double Pwater = 395;//kpa
                double conductivity = 386; //w/mK for Cu
                int hexType = 1; //*********************************0 is evap, 1 is cond******************************************
                double[] hri = new double[N];
                coolprop.update(input_pairs.PT_INPUTS, Pwater * 1000, tc[i] + 273.15);
                hri[i] = coolprop.hmass() / 1000;
                //hri[i] = CoolProp.PropsSI("H", "T", tc[i] + 273.15, "P", Pwater * 1000, fluid) / 1000 ;


                double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
                double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

                string AirDirection = "逆流";
                ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai[i], tc[i], AirDirection);
                RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi[i], tc[i], AirDirection);

                //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, tc[i], pc[i], hri[i],
                    //mr[i], ma, ha, haw, eta_surface[i], zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

                res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, tc[i], pc[i], hri[i],
                    mr[i], ma, ha,haw, eta_surface[i], zh, zdp, hexType, thickness, conductivity, Pwater,AirDirection, d_cap, lenth_cap,coolprop);

                //using (StreamWriter wr = File.AppendText(@"D:\Work\Simulation\Test\Midea9_heat.txt"))
                //{
                //    wr.WriteLine("Q, {0}, DP, {1}, href, {2}, Ra_ratio, {3}, Tao, {4}, Tro, {5}", res.Q, res.DP, res.href, res.Ra_ratio, res.Tao, res.Tro);
                //}
                Q[i] = res.Q;
            }
            return res;
        }
        public static CalcResult Water_Heat_Jiayong6_autosplitCir_New()
        {
            string fluid = "Water";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            CalcResult res = new CalcResult();
            int Nrow = 2;
            double[] FPI = new double[Nrow + 1];
            FPI = new double[] { 21.167, 21.167 };
            double Pt = 21 * 0.001;
            double Pr = 22 * 0.001;
            double Di = 6.8944 * 0.001;//8
            double Do = 7.35 * 0.001;//8.4
            double Fthickness = 0.095 * 0.001;
            double thickness = 0.5 * (Do - Di);
            int[] Ntube = { 16, 16 };
            int N_tube = Ntube[0];
            double L = 400 * 0.001;
            int Nelement = 1;

            //流路均分设计
            int[,] CirArrange;

            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { 3, 3 };
            CircuitInfo.TubeofCir = new int[CircuitInfo.number[0]];

            //Avoid invalid Ncir input 
            if (CircuitInfo.number[0] > Ntube[0])
            {
                throw new Exception("circuit number is beyond range.");
            }
            //Get AutoCircuitry
            CircuitInfo = AutoCircuiting.GetTubeofCir(Nrow, N_tube, CircuitInfo);
            CirArrange = new int[CircuitInfo.number[0], CircuitInfo.TubeofCir[CircuitInfo.number[0] - 1]];
            CirArrange = AutoCircuiting.GetCirArrange_2Row(CirArrange, Nrow, N_tube, CircuitInfo);


            double[] d_cap = new double[] { 0, 0, 0 };
            double[] lenth_cap = new double[] { 0, 0, 0 };

            //AreaResult geo = new AreaResult();
            //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);

            double[] Q = new double[16];
            double[] href = new double[16];

            int N = 16;
            for (int i = 0; i < N; i++)
            {
                double[] mr = new double[] { 13.01, 17.01, 21.00, 25.00, 13.00, 17.01, 21.00, 25.00, 13.00, 17.00, 21.00, 25.01, 13.01, 17.00, 21.01, 25.00 }; // 60;
                mr[i] = mr[i] / 60;
                double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
                //double Vel_ave = 1.6;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
                double[] Vel_ave = new double[] { 0.70550, 0.70395, 0.70348, 0.70329, 1.00285, 1.00238, 1.00203, 1.00366, 1.30551, 1.30599, 1.30520, 1.30471, 1.58699, 1.58381, 1.58356, 1.58275 }; //m/s
                AirDistribution VaDistri = new AirDistribution();
                VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
                double[,] ma = new double[N_tube, Nelement];
                double[,] ha = new double[N_tube, Nelement];
                double H = Pt * N_tube;
                double Hx = L * H;
                double[] rho_a_st = { 1.19767, 1.19763, 1.19766, 1.19767, 1.19772, 1.19773, 1.19772, 1.19776, 1.19779, 1.19771, 1.19775, 1.19771, 1.19767, 1.19774, 1.19768, 1.19768 }; //kg/m3

                //空气侧几何结构选择
                //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
                //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
                //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
                //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
                int curve = 1; //

                double za = 1; //Adjust factor
                double zh = 1;
                double zdp = 1;
                double[] eta_surface = new double[] { 0.86998, 0.872736, 0.8747438, 0.8761934, 0.84895, 0.851907, 0.8535928, 0.8544665, 0.82776, 0.830934, 0.8327302, 0.8339071, 0.81128, 0.81624, 0.81832, 0.81969 };
                double[] hai = new double[] { 40.297, 39.319, 38.610, 38.100, 47.975, 46.873, 46.248, 45.925, 56.106, 54.862, 54.162, 53.705, 62.722, 60.704, 59.864, 59.315 };
                for (int j = 0; j < N_tube; j++)
                {
                    for (int k = 0; k < Nelement; k++)
                    {
                        ma[j, k] = VaDistri.Va[j, k] * (Vel_ave[i] / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st[i];
                        //ha[j, k] = AirHTC.alpha(VaDistri.Va[j, k] * (Vel_ave / VaDistri.Va_ave), za, curve);
                        ha[j, k] = hai[i];
                    }
                }
                double[,] haw = ha;
                
                double[] tai = new double[] { 20.01, 20.02, 20.01, 20.01, 20.00, 20.00, 20.00, 19.99, 19.98, 20.00, 19.99, 20.00, 20.01, 19.99, 20.01, 20.01 };
                double[] RHi = new double[] { 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469 };

                double[] tri = new double[] { 45.01, 45.00, 44.98, 44.99, 45.01, 44.99, 45.00, 44.99, 45.01, 44.99, 44.99, 44.99, 45.00, 44.99, 44.98, 45.01 };
                double[] tc = tri;
                double[] pc = new double[N];
                coolprop.update(input_pairs.QT_INPUTS, 1, tc[i] + 273.15);
                pc[i] = coolprop.p() / 1000;
                //pc[i] = CoolProp.PropsSI("P", "T", tc[i] + 273.15, "Q", 1, fluid) / 1000;

                double Pwater = 395;//kpa
                double conductivity = 386; //w/mK for Cu
                int hexType = 1; //*********************************0 is evap, 1 is cond******************************************
                double[] hri = new double[N];
                coolprop.update(input_pairs.PT_INPUTS, Pwater * 1000, tc[i] + 273.15);
                hri[i] = coolprop.hmass() / 1000;
                //hri[i] = CoolProp.PropsSI("H", "T", tc[i] + 273.15, "P", Pwater * 1000, fluid) / 1000 ;

                double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
                double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

                string AirDirection = "逆流";
                ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai[i], tc[i], AirDirection);
                RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi[i], tc[i], AirDirection);



                //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, tc[i], pc[i], hri[i],
                    //mr[i], ma, ha, haw, eta_surface[i], zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

                res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, tc[i], pc[i], hri[i],
                    mr[i], ma, ha,haw, eta_surface[i], zh, zdp, hexType, thickness, conductivity, Pwater,AirDirection, d_cap, lenth_cap,coolprop);

                //using (StreamWriter wr = File.AppendText(@"D:\Work\Simulation\Test\Midea9_heat.txt"))
                //{
                //    wr.WriteLine("Q, {0}, DP, {1}, href, {2}, Ra_ratio, {3}, Tao, {4}, Tro, {5}", res.Q, res.DP, res.href, res.Ra_ratio, res.Tao, res.Tro);
                //}
                Q[i] = res.Q;
                href[i] = res.href;
            }
            return res;
        }
        public static CalcResult Water_Heat_Jiayong2_autosplitCir()
        {
            string fluid = "Water";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            CalcResult res = new CalcResult();
            int Nrow = 2;
            double[] FPI = new double[Nrow + 1];
            FPI = new double[] { 21.167, 21.167 };
            double Pt = 14.5 * 0.001;
            double Pr = 12.56 * 0.001;
            double Di = 4.8706 * 0.001;//8
            double Do = 5.25 * 0.001;//8.4
            double Fthickness = 0.095 * 0.001;
            double thickness = 0.5 * (Do - Di);
            int[] Ntube = { 24, 24 };
            int N_tube = Ntube[0];
            double L = 400 * 0.001;
            int Nelement = 1;

            //流路均分设计
            int[,] CirArrange;

            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { 6, 6 };
            //Avoid invalid Ncir input 
            if (CircuitInfo.number[0] > Ntube[0])
            {
                throw new Exception("circuit number is beyond range.");
            }

            CircuitInfo.TubeofCir = new int[CircuitInfo.number[0]];

            //Get AutoCircuitry
            CircuitInfo = AutoCircuiting.GetTubeofCir(Nrow, N_tube, CircuitInfo);
            CirArrange = new int[CircuitInfo.number[0], CircuitInfo.TubeofCir[CircuitInfo.number[0] - 1]];
            CirArrange = AutoCircuiting.GetCirArrange_2Row(CirArrange, Nrow, N_tube, CircuitInfo);


            double[] d_cap = new double[] { 0, 0, 0, 0, 0, 0 };
            double[] lenth_cap = new double[] { 0, 0, 0, 0, 0, 0 };

            //AreaResult geo = new AreaResult();
            //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);

            double[] Q = new double[16];
            int N = 16;
            for (int i = 0; i < N; i++)
            {
                double[] mr = new double[] { 13.00, 17.00, 21.00, 25.00, 13.00, 17.01, 21.01, 25.00, 13.01, 17.00, 21.00, 25.00, 13.01, 17.00, 21.01, 25.00 }; // 60;
                mr[i] = mr[i] / 60;
                double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
                //double Vel_ave = 1.6;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
                double[] Vel_ave = new double[] { 0.702466, 0.70428, 0.70592, 0.70592, 1.00585, 1.00475, 1.00716, 1.00635, 1.31044, 1.30957, 1.30901, 1.30781, 1.58978, 1.58832, 1.58607, 1.59028 }; //m/s
                AirDistribution VaDistri = new AirDistribution();
                VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
                double[,] ma = new double[N_tube, Nelement];
                double[,] ha = new double[N_tube, Nelement];
                double H = Pt * N_tube;
                double Hx = L * H;
                double[] rho_a_st = { 1.19772, 1.19771, 1.19777, 1.19762, 1.19773, 1.19773, 1.19775, 1.19767, 1.19772, 1.19773, 1.19770, 1.19772, 1.19766, 1.19771, 1.19771, 1.19775 }; //kg/m3

                //空气侧几何结构选择
                //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
                //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
                //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
                //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
                int curve = 1; //

                double za = 1; //Adjust factor
                double zh = 1;
                double zdp = 1;
                double[] eta_surface = new double[] { 0.93687, 0.936250, 0.9295073, 0.9333229, 0.92812, 0.927387, 0.9259171, 0.9252950, 0.92324, 0.92239, 0.92222, 0.92211, 0.91866, 0.91823, 0.91817, 0.91800 };
                double[] hai = new double[] { 56.798, 57.390, 63.920, 60.213, 65.277, 65.993, 67.436, 68.048, 70.078, 70.917, 71.086, 71.197, 74.628, 75.059, 75.112, 75.283 };
                for (int j = 0; j < N_tube; j++)
                {
                    for (int k = 0; k < Nelement; k++)
                    {
                        ma[j, k] = VaDistri.Va[j, k] * (Vel_ave[i] / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st[i];
                        //ha[j, k] = AirHTC.alpha(VaDistri.Va[j, k] * (Vel_ave / VaDistri.Va_ave), za, curve);
                        ha[j, k] = hai[i];
                    }
                }
                double[,] haw = ha;
                
                
                double[] tai = new double[] { 20.00, 20.00, 19.98, 20.02, 20.00, 20.00, 19.99, 20.01, 20.00, 20.00, 20.01, 20.00, 20.01, 20.00, 20.00, 19.99 };
                double[] RHi = new double[] { 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469 };

                double[] tri = new double[] { 44.99, 44.99, 45.01, 44.99, 45.02, 45.02, 44.99, 45.01, 45.01, 45.03, 44.98, 44.99, 45.02, 44.98, 45.03, 44.99 };
                double[] tc = tri;
                double[] pc = new double[N];
                coolprop.update(input_pairs.QT_INPUTS, 1, tc[i] + 273.15);
                pc[i] = coolprop.p() / 1000;
                //pc[i] = CoolProp.PropsSI("P", "T", tc[i] + 273.15, "Q", 1, fluid) / 1000;

                double Pwater = 395;//kpa
                double conductivity = 386; //w/mK for Cu
                int hexType = 1; //*********************************0 is evap, 1 is cond******************************************
                double[] hri = new double[N];
                coolprop.update(input_pairs.PT_INPUTS, Pwater * 1000, tc[i] + 273.15);
                hri[i] = coolprop.hmass() / 1000;
                //hri[i] = CoolProp.PropsSI("H", "T", tc[i] + 273.15, "P", Pwater * 1000, fluid) / 1000 ;

                double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
                double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

                string AirDirection = "逆流";
                ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai[i], tc[i], AirDirection);
                RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi[i], tc[i], AirDirection);



                //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, tc[i], pc[i], hri[i],
                    //mr[i], ma, ha, haw, eta_surface[i], zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

                res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, tc[i], pc[i], hri[i],
                    mr[i], ma, ha,haw, eta_surface[i], zh, zdp, hexType, thickness, conductivity, Pwater,AirDirection, d_cap, lenth_cap, coolprop);

                //using (StreamWriter wr = File.AppendText(@"D:\Work\Simulation\Test\Midea9_heat.txt"))
                //{
                //    wr.WriteLine("Q, {0}, DP, {1}, href, {2}, Ra_ratio, {3}, Tao, {4}, Tro, {5}", res.Q, res.DP, res.href, res.Ra_ratio, res.Tao, res.Tro);
                //}
                Q[i] = res.Q;
            }
            return res;
        }
        public static CalcResult Water_Heat_Zhongyang1_autosplitCir()
        {
            string fluid = "Water";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            CalcResult res = new CalcResult();
            int Nrow = 3;
            double[] FPI = new double[Nrow + 1];
            FPI = new double[] { 16.933, 16.933, 16.933 };
            double Pt = 21.0 * 0.001;
            double Pr = 19.4 * 0.001;
            double Di = 6.8944 * 0.001;//8
            double Do = 7.35 * 0.001;//8.4
            double Fthickness = 0.1 * 0.001;
            double thickness = 0.5 * (Do - Di);
            int[] Ntube = { 18, 18, 18 };
            int N_tube = Ntube[0];
            double L = 438.0 * 0.001;
            int Nelement = 1;
            int[,] CirArrange;
            
            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { 11, 11 };
            CircuitInfo.TubeofCir = new int[CircuitInfo.number[0]];
            //Avoid invalid Ncir input 
            if (CircuitInfo.number[0] > Ntube[0])
            {
                throw new Exception("circuit number is beyond range.");
            }

            //Get AutoCircuitry
            CircuitInfo = AutoCircuiting.GetTubeofCir(Nrow, N_tube, CircuitInfo);          
            CirArrange = new int[CircuitInfo.number[0], CircuitInfo.TubeofCir[CircuitInfo.number[0] - 1]];
            CirArrange = AutoCircuiting.GetCirArrange_3Row(CirArrange, Nrow, N_tube, CircuitInfo);


            double[] d_cap = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            double[] lenth_cap = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            //AreaResult geo = new AreaResult();
            //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);

            double[] Q = new double[16];
            int N = 16;
            for (int i = 0; i < N; i++)
            {
                double[] mr = new double[] { 13.01, 17.01, 21.00, 25.00, 13.01, 17.01, 21.00, 25.00, 13.00, 17.01, 21.01, 25.00, 13.00, 17.00, 21.01, 25.00 }; // 60;
                mr[i] = mr[i] / 60;
                double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
                //double Vel_ave = 1.6;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
                double[] Vel_ave = new double[] { 0.60061, 0.60059, 0.59874, 0.60039, 0.89766, 0.89707, 0.89755, 0.89636, 1.09706, 1.09812, 1.09922, 1.09919, 1.29987, 1.29968, 1.29857, 1.29670 }; //m/s
                AirDistribution VaDistri = new AirDistribution();
                VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
                double[,] ma = new double[N_tube, Nelement];
                double[,] ha = new double[N_tube, Nelement];
                double H = Pt * N_tube;
                double Hx = L * H;
                double[] rho_a_st = { 1.19775, 1.19775, 1.19766, 1.19778, 1.19777, 1.19769, 1.19770, 1.19770, 1.19754, 1.19770, 1.19777, 1.19767, 1.19780, 1.19761, 1.19751, 1.19766 }; //kg/m3

                //空气侧几何结构选择
                //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
                //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
                //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
                //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
                int curve = 1; //

                double za = 1; //Adjust factor
                double zh = 1;
                double zdp = 1;
                double[] eta_surface = new double[] { 0.91638, 0.917989, 0.9188580, 0.9193723, 0.88357, 0.884134, 0.8849341, 0.8834665, 0.86450, 0.865326, 0.8658559, 0.8664762, 0.85036, 0.85115, 0.85198, 0.85284 };
                double[] hai = new double[] { 32.230, 31.553, 31.189, 30.974, 46.541, 46.285, 45.924, 46.587, 55.360, 54.968, 54.718, 54.426, 62.153, 61.768, 61.363, 60.944 };
                for (int j = 0; j < N_tube; j++)
                {
                    for (int k = 0; k < Nelement; k++)
                    {
                        ma[j, k] = VaDistri.Va[j, k] * (Vel_ave[i] / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st[i];
                        //ha[j, k] = AirHTC.alpha(VaDistri.Va[j, k] * (Vel_ave / VaDistri.Va_ave), za, curve);
                        ha[j, k] = hai[i];
                    }
                }
                double[,] haw = ha;
                
                double[] tai = new double[] { 20.00, 20.00, 20.02, 19.99, 19.99, 20.01, 20.01, 20.01, 20.05, 20.01, 19.99, 20.02, 19.98, 20.03, 20.06, 20.02 };
                double[] RHi = new double[] { 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469 };

                double[] tri = new double[] { 45.00, 45.00, 44.99, 45.00, 45.03, 45.02, 45.01, 44.99, 45.01, 45.01, 45.00, 44.99, 45.03, 45.01, 45.01, 45.02 };
                double[] tc = tri;
                double[] pc = new double[N];
                coolprop.update(input_pairs.QT_INPUTS, 1, tc[i] + 273.15);
                pc[i] = coolprop.p() / 1000;
                //pc[i] = CoolProp.PropsSI("P", "T", tc[i] + 273.15, "Q", 1, fluid) / 1000;

                double Pwater = 395;//kpa
                double conductivity = 386; //w/mK for Cu
                int hexType = 1; //*********************************0 is evap, 1 is cond******************************************
                double[] hri = new double[N];
                coolprop.update(input_pairs.PT_INPUTS, Pwater * 1000, tc[i] + 273.15);
                hri[i] = coolprop.hmass() / 1000;
                //hri[i] = CoolProp.PropsSI("H", "T", tc[i] + 273.15, "P", Pwater * 1000, fluid) / 1000 ;

                double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
                double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

                string AirDirection = "逆流";
                ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai[i], tc[i], AirDirection);
                RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi[i], tc[i], AirDirection);

                //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, tc[i], pc[i], hri[i],
                    //mr[i], ma, ha, haw, eta_surface[i], zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

                res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, tc[i], pc[i], hri[i],
                    mr[i], ma, ha,haw, eta_surface[i], zh, zdp, hexType, thickness, conductivity, Pwater,AirDirection, d_cap, lenth_cap, coolprop);

                //using (StreamWriter wr = File.AppendText(@"D:\Work\Simulation\Test\Midea9_heat.txt"))
                //{
                //    wr.WriteLine("Q, {0}, DP, {1}, href, {2}, Ra_ratio, {3}, Tao, {4}, Tro, {5}", res.Q, res.DP, res.href, res.Ra_ratio, res.Tao, res.Tro);
                //}
                Q[i] = res.Q;
            }
            return res;
        }

        public static CalcResult Water_Heat_Zhongyang1()
        {
            string fluid = "Water";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            CalcResult res = new CalcResult();
            int Nrow = 3;
            double[] FPI = new double[Nrow + 1];
            FPI = new double[] { 16.933, 16.933, 16.933 };
            double Pt = 21.0 * 0.001;
            double Pr = 19.4 * 0.001;
            double Di = 6.8944 * 0.001;//8
            double Do = 7.35 * 0.001;//8.4
            double Fthickness = 0.1 * 0.001;
            double thickness = 0.5 * (Do - Di);
            int[] Ntube = { 18, 18, 18 };
            int N_tube = Ntube[0];
            double L = 438.0 * 0.001;
            int Nelement = 1;
            int[,] CirArrange;
            CirArrange = new int[,] { { 37, 38, 39, 40, 41, 42, 24, 23, 22, 21, 20, 19, 1, 2, 3, 4, 5, 6 }, { 43, 44, 45, 46, 47, 48, 30, 29, 28, 27, 26, 25, 7, 8, 9, 10, 11, 12 }, { 49, 50, 51, 52, 53, 54, 36, 35, 34, 33, 32, 31, 13, 14, 15, 16, 17, 18 } };
            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { 3, 3 };
            CircuitInfo.TubeofCir = new int[] { 18, 18, 18 };  //{ 4, 8 };


            double[] d_cap = new double[] { 0, 0, 0 };
            double[] lenth_cap = new double[] { 0, 0, 0 };

            //AreaResult geo = new AreaResult();
            //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);

            double[] Q = new double[16];
            int N = 16;
            for (int i = 0; i < N; i++)
            {
                double[] mr = new double[] { 13.01, 17.01, 21.01, 25.01, 13.01, 17.01, 21.01, 25.00, 13.01, 17.01, 21.00, 25.00, 13.01, 17.01, 21.00, 25.00 }; // 60;
                mr[i] = mr[i] / 60;
                double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
                //double Vel_ave = 1.6;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
                double[] Vel_ave = new double[] { 0.60061, 0.60059, 0.59874, 0.60039, 0.89766, 0.89707, 0.89755, 0.89636, 1.09706, 1.09812, 1.09922, 1.09919, 1.29987, 1.29968, 1.29857, 1.29670 }; //m/s
                AirDistribution VaDistri = new AirDistribution();
                VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
                double[,] ma = new double[N_tube, Nelement];
                double[,] ha = new double[N_tube, Nelement];
                double H = Pt * N_tube;
                double Hx = L * H;
                double[] rho_a_st = { 1.19775, 1.19775, 1.19766, 1.19778, 1.19777, 1.19769, 1.19770, 1.19770, 1.19754, 1.19770, 1.19777, 1.19767, 1.19780, 1.19761, 1.19751, 1.19766 }; //kg/m3

                //空气侧几何结构选择
                //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
                //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
                //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
                //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
                int curve = 1; //

                double za = 1; //Adjust factor
                double zh = 1;
                double zdp = 1;
                double[] eta_surface = new double[] { 0.91638, 0.917989, 0.9188580, 0.9193723, 0.88357, 0.884134, 0.8849341, 0.8834665, 0.86450, 0.865326, 0.8658559, 0.8664762, 0.85036, 0.85115, 0.85198, 0.85284 };
                double[] hai = new double[] { 32.230, 31.553, 31.189, 30.974, 46.541, 46.285, 45.924, 46.587, 55.360, 54.968, 54.718, 54.426, 62.153, 61.768, 61.363, 60.944 };
                for (int j = 0; j < N_tube; j++)
                {
                    for (int k = 0; k < Nelement; k++)
                    {
                        ma[j, k] = VaDistri.Va[j, k] * (Vel_ave[i] / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st[i];
                        //ha[j, k] = AirHTC.alpha(VaDistri.Va[j, k] * (Vel_ave / VaDistri.Va_ave), za, curve);
                        ha[j, k] = hai[i];
                    }
                }
                double[,] haw = ha;
                
                
                double[] tai = new double[] { 20.00, 20.00, 20.02, 19.99, 19.99, 20.01, 20.01, 20.01, 20.05, 20.01, 19.99, 20.02, 19.98, 20.03, 20.06, 20.02 };
                double[] RHi = new double[] { 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469, 0.469 };

                double[] tri = new double[] { 45.00, 45.00, 44.99, 45.00, 45.03, 45.02, 45.01, 44.99, 45.01, 45.01, 45.00, 44.99, 45.03, 45.01, 45.01, 45.02 };
                double[] tc = tri;
                double[] pc = new double[N];
                coolprop.update(input_pairs.QT_INPUTS, 1, tc[i] + 273.15);
                pc[i] = coolprop.p() / 1000;
                //pc[i] = CoolProp.PropsSI("P", "T", tc[i] + 273.15, "Q", 1, fluid) / 1000;

                double Pwater = 395;//kpa
                double conductivity = 386; //w/mK for Cu
                int hexType = 1; //*********************************0 is evap, 1 is cond******************************************
                double[] hri = new double[N];
                coolprop.update(input_pairs.PT_INPUTS, Pwater * 1000, tc[i] + 273.15);
                hri[i] = coolprop.hmass() / 1000;
                //hri[i] = CoolProp.PropsSI("H", "T", tc[i] + 273.15, "P", Pwater * 1000, fluid) / 1000 ;

                double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
                double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

                string AirDirection = "逆流";
                ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai[i], tc[i], AirDirection);
                RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi[i], tc[i], AirDirection);

                //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, tc[i], pc[i], hri[i],
                    //mr[i], ma, ha, haw, eta_surface[i], zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

                res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, tc[i], pc[i], hri[i],
                    mr[i], ma, ha,haw, eta_surface[i], zh, zdp, hexType, thickness, conductivity, Pwater,AirDirection, d_cap, lenth_cap,coolprop);

                //using (StreamWriter wr = File.AppendText(@"D:\Work\Simulation\Test\Midea9_heat.txt"))
                //{
                //    wr.WriteLine("Q, {0}, DP, {1}, href, {2}, Ra_ratio, {3}, Tao, {4}, Tro, {5}", res.Q, res.DP, res.href, res.Ra_ratio, res.Tao, res.Tro);
                //}
                Q[i] = res.Q;
            }
            return res;
        }
        public static CalcResult Water_Cool_Zhongyang1()
        {
            string fluid = "Water";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            CalcResult res = new CalcResult();
            int Nrow = 3;
            double[] FPI = new double[Nrow + 1];
            FPI = new double[] { 16.933, 16.933, 16.933 };
            double Pt = 21.0 * 0.001;
            double Pr = 19.4 * 0.001;
            double Di = 6.8944 * 0.001;//8
            double Do = 7.35 * 0.001;//8.4
            double Fthickness = 0.1 * 0.001;
            double thickness = 0.5 * (Do - Di);
            int[] Ntube = { 18, 18, 18 };
            int N_tube = Ntube[0];
            double L = 438.0 * 0.001;
            int Nelement = 1;
            int[,] CirArrange;
            CirArrange = new int[,] { { 37, 38, 39, 40, 41, 42, 24, 23, 22, 21, 20, 19, 1, 2, 3, 4, 5, 6 }, { 43, 44, 45, 46, 47, 48, 30, 29, 28, 27, 26, 25, 7, 8, 9, 10, 11, 12 }, { 49, 50, 51, 52, 53, 54, 36, 35, 34, 33, 32, 31, 13, 14, 15, 16, 17, 18 } };
            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { 3, 3 };
            CircuitInfo.TubeofCir = new int[] { 18, 18, 18 };  //{ 4, 8 };


            double[] d_cap = new double[] { 0, 0, 0 };
            double[] lenth_cap = new double[] { 0, 0, 0 };

            //AreaResult geo = new AreaResult();
            //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);

            double[] Q = new double[16];
            int N = 16;
            for (int i = 0; i < N; i++)
            {
                double[] mr = new double[] { 13.01, 17.01, 21.01, 25.01, 13.01, 17.01, 21.01, 25.00, 13.01, 17.01, 21.00, 25.00, 13.01, 17.01, 21.00, 25.00 }; // 60;
                mr[i] = mr[i] / 60;
                double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
                //double Vel_ave = 1.6;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
                double[] Vel_ave = new double[] { 0.59953, 0.59918, 0.59937, 0.59932, 0.89662, 0.89764, 0.89871, 0.89920, 1.09872, 1.09853, 1.09848, 1.09934, 1.29794, 1.29856, 1.29909, 1.29948 }; //m/s
                AirDistribution VaDistri = new AirDistribution();
                VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
                double[,] ma = new double[N_tube, Nelement];
                double[,] ha = new double[N_tube, Nelement];
                double H = Pt * N_tube;
                double Hx = L * H;
                double[] rho_a_st = { 1.16838, 1.16839, 1.16860, 1.16853, 1.16846, 1.16857, 1.16846, 1.16847, 1.16849, 1.16849, 1.16849, 1.16846, 1.16839, 1.16847, 1.16844, 1.16847 }; //kg/m3

                //空气侧几何结构选择
                //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
                //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
                //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
                //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
                int curve = 1; //

                double za = 1; //Adjust factor
                double zh = 1;
                double zdp = 1;
                double[] eta_surface = new double[] { 0.89596, 0.896814, 0.8966571, 0.8980433, 0.86571, 0.864929, 0.8638292, 0.8638503, 0.84928, 0.846912, 0.8477477, 0.8480225, 0.83348, 0.83280, 0.83285, 0.83317 };
                double[] hai = new double[] { 58.92, 58.92, 58.92, 58.92, 58.92, 58.92, 58.92, 65.61, 65.61, 65.61, 65.61, 65.61, 65.61, 65.61, 71.19, 71.19, 71.19, 71.19, 71.19, 71.19, 71.19, 75.95, 75.95, 75.95, 75.95, 75.95, 75.95, 75.95 };
                for (int j = 0; j < N_tube; j++)
                {
                    for (int k = 0; k < Nelement; k++)
                    {
                        ma[j, k] = VaDistri.Va[j, k] * (Vel_ave[i] / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st[i];
                        //ha[j, k] = AirHTC.alpha(VaDistri.Va[j, k] * (Vel_ave / VaDistri.Va_ave), za, curve);
                        ha[j, k] = hai[i];
                    }
                }
                double[,] haw = ha;

                
                double[] tai = new double[] { 27.03, 27.03, 26.97, 26.99, 27.01, 26.98, 27.01, 27.01, 27.00, 27.00, 27.00, 27.01, 27.03, 27.01, 27.02, 27.01 };
                double[] RHi = new double[] { 0.4682, 0.4676, 0.4704, 0.4694, 0.4685, 0.4699, 0.4685, 0.4680, 0.4690, 0.4690, 0.4696, 0.4685, 0.4676, 0.4680, 0.4669, 0.4680 };

                double[] tri = new double[] { 10.01, 10.01, 10.00, 10.00, 10.01, 10.00, 10.00, 10.02, 9.99, 10.00, 10.00, 10.01, 10.00, 10.02, 10.00, 9.99 };
                double[] tc = tri;
                double[] pc = new double[N];
                coolprop.update(input_pairs.QT_INPUTS, 1, tc[i] + 273.15);
                pc[i] = coolprop.p() / 1000;
                //pc[i] = CoolProp.PropsSI("P", "T", tc[i] + 273.15, "Q", 1, fluid) / 1000;

                double Pwater = 395;//kpa
                double conductivity = 386; //w/mK for Cu
                int hexType = 0; //*********************************0 is evap, 1 is cond******************************************
                double[] hri = new double[N];
                coolprop.update(input_pairs.PT_INPUTS, Pwater * 1000, tc[i] + 273.15);
                hri[i] = coolprop.hmass() / 1000;
                //hri[i] = CoolProp.PropsSI("H", "T", tc[i] + 273.15, "P", Pwater * 1000, fluid) / 1000 ;

                double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
                double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

                string AirDirection = "逆流";
                ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai[i], tc[i], AirDirection);
                RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi[i], tc[i], AirDirection);



                //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, tc[i], pc[i], hri[i],
                    //mr[i], ma, ha, haw, eta_surface[i], zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

                res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, tc[i], pc[i], hri[i],
                    mr[i], ma, ha,haw, eta_surface[i], zh, zdp, hexType, thickness, conductivity, Pwater,AirDirection, d_cap, lenth_cap, coolprop);

                //using (StreamWriter wr = File.AppendText(@"D:\Work\Simulation\Test\Midea9_heat.txt"))
                //{
                //    wr.WriteLine("Q, {0}, DP, {1}, href, {2}, Ra_ratio, {3}, Tao, {4}, Tro, {5}", res.Q, res.DP, res.href, res.Ra_ratio, res.Tao, res.Tro);
                //}
                Q[i] = res.Q;
            }
            return res;
        }
        public static CalcResult Water_Heat_Zhongyang2()
        {
            string fluid = "Water";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            CalcResult res = new CalcResult();
            int Nrow = 3;
            double[] FPI = new double[Nrow + 1];
            FPI = new double[] { 15.875, 15.875, 15.875 };
            double Pt = 22.0 * 0.001;
            double Pr = 19.05 * 0.001;
            double Di = 7.8937 * 0.001;//8
            double Do = 8.4 * 0.001;//8.4
            double Fthickness = 0.1 * 0.001;
            double thickness = 0.5 * (Do - Di);
            int[] Ntube = { 16, 16, 16 };
            int N_tube = Ntube[0];
            double L = 433.0 * 0.001;
            int Nelement = 1;
            int[,] CirArrange;
            CirArrange = new int[,] { { 33, 34, 35, 36, 37, 38, 22, 21, 20, 19, 18, 17, 1, 2, 3, 4, 5, 6 }, { 44, 43, 42, 41, 40, 39, 23, 24, 25, 26, 10, 9, 8, 7, 0, 0, 0, 0 }, { 45, 46, 47, 48, 32, 31, 30, 29, 28, 27, 11, 12, 13, 14, 15, 16, 0, 0 } };
            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { 3, 3 };
             CircuitInfo.TubeofCir = new int[] { 18, 14, 16 };  //{ 4, 8 };

             double[] d_cap = new double[] { 0, 0, 0 };
             double[] lenth_cap = new double[] { 0, 0, 0 };

             GeometryInput geoInput_air = new GeometryInput();
             geoInput_air.Pt = Pt;
             geoInput_air.Pr = Pr;
             geoInput_air.Do = Do;
             geoInput_air.Fthickness = Fthickness;
             geoInput_air.FPI = FPI[0];
             geoInput_air.Nrow = Nrow;

             int hexType = 1; 

             double mr = 9.99/60;
             double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
             double Vel_ave = 2;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
             AirDistribution VaDistri = new AirDistribution();
             VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
             double[,] ma = new double[N_tube, Nelement];
             double[,] ha = new double[N_tube, Nelement];
             double H = Pt * N_tube;
             double Hx = L * H;
             double rho_a_st = 1.188;
             double za = 1;

             //空气侧几何结构选择
             //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
             //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
             //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
             //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
             int curve = 1;

             double eta_surface = 1;
             double zh = 1;
             double zdp = 1.5;
             for (int i = 0; i < N_tube; i++)
             {
                 for (int j = 0; j < Nelement; j++)
                 {
                     ma[i, j] = VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st;
                     //ha[i, j] = AirHTC.alpha(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve) * 1.5;
                     //ha[i, j] = 79;
                     ha[i, j] = AirHTC.alpha1(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve, geoInput_air, hexType).ha;
                 }
             }
             double[,] haw = ha;

             res.DPa = AirHTC.alpha1(Vel_ave, za, curve, geoInput_air, hexType).dP_a;

            double tai = 19.98;
            double RHi = 0.469;
            double tri = 44.98;
            double tc = tri;
            coolprop.update(input_pairs.QT_INPUTS, 1, tc + 273.15);
            double pc = coolprop.p() / 1000;
            //double pc = CoolProp.PropsSI("P", "T", tc + 273.15, "Q", 1, fluid) / 1000;

            double Pwater = 395;//kpa
            double conductivity = 386; //w/mK for Cu
            //int hexType = 1; //*********************************0 is evap, 1 is cond******************************************
            coolprop.update(input_pairs.PT_INPUTS, Pwater * 1000, tc + 273.15);
            double hri = coolprop.hmass() / 1000;
            //double hri = CoolProp.PropsSI("H", "T", tc + 273.15, "P", Pwater * 1000, fluid) / 1000 ;

            double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
            double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

            //string AirDirection="DowntoUp";
            string AirDirection = "逆流";
            ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai, tc, AirDirection);
            RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi, tc, AirDirection);


            //AreaResult geo = new AreaResult();
            //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, tc, pc, hri,
                //mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, tc, pc, hri,
                mr, ma, ha,haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater,AirDirection, d_cap, lenth_cap,coolprop);


            return res;
        }
        public static CalcResult Water_Cool_Zhongyang2()
        {
            string fluid = "Water";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            CalcResult res = new CalcResult();
            int Nrow = 3;
            double[] FPI = new double[Nrow + 1];
            FPI = new double[] { 15.875, 15.875, 15.875 };
            double Pt = 22.0 * 0.001;
            double Pr = 19.05 * 0.001;
            double Di = 7.8937 * 0.001;//8
            double Do = 8.4 * 0.001;//8.4
            double Fthickness = 0.1 * 0.001;
            double thickness = 0.5 * (Do - Di);
            int[] Ntube = { 16, 16, 16 };
            int N_tube = Ntube[0];
            double L = 433.0 * 0.001;
            int Nelement = 1;
            int[,] CirArrange;
            CirArrange = new int[,] { { 33, 34, 35, 36, 37, 38, 22, 21, 20, 19, 18, 17, 1, 2, 3, 4, 5, 6 }, { 44, 43, 42, 41, 40, 39, 23, 24, 25, 26, 10, 9, 8, 7, 0, 0, 0, 0 }, { 45, 46, 47, 48, 32, 31, 30, 29, 28, 27, 11, 12, 13, 14, 15, 16, 0, 0 } };
            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { 3, 3 };
            CircuitInfo.TubeofCir = new int[] { 18, 14, 16 };  //{ 4, 8 };


            double[] d_cap = new double[] { 0, 0, 0 };
            double[] lenth_cap = new double[] { 0, 0, 0 };

            //AreaResult geo = new AreaResult();
            //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);

            double[] Q = new double[20];
            int N = 20;
            for (int i = 0; i < N; i++)
            {
                double[] mr = new double[] { 13.00, 17.01, 21.01, 25.00, 13.01, 17.01, 21.00, 25.00, 13.01, 17.01, 21.01, 25.01, 13.01, 17.01, 21.00, 25.01, 13.01, 17.00, 21.01, 25.00 }; // 60;
                mr[i] = mr[i] / 60;
                double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
                //double Vel_ave = 1.6;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
                double[] Vel_ave = new double[] { 0.65233, 0.65247, 0.65203, 0.65231, 0.97535, 0.97611, 0.97615, 0.97600, 1.19102, 1.19266, 1.19328, 1.19184, 1.41083, 1.41189, 1.41016, 1.41338, 1.45545, 1.45630, 1.45670, 1.45692 };
                AirDistribution VaDistri = new AirDistribution();
                VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
                double[,] ma = new double[N_tube, Nelement];
                double[,] ha = new double[N_tube, Nelement];
                double H = Pt * N_tube;
                double Hx = L * H;
                double[] rho_a_st = { 1.16851, 1.16850, 1.16847, 1.16847, 1.16844, 1.16845, 1.16845, 1.16846, 1.16840, 1.16840, 1.16845, 1.16850, 1.16848, 1.16847, 1.16840, 1.16843, 1.16849, 1.16852, 1.16845, 1.16845 }; //kg/m3

                //空气侧几何结构选择
                //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
                //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
                //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
                //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
                int curve = 1; //

                double za = 1; //Adjust factor
                double zh = 1;
                double zdp = 1;
                double[] eta_surface = new double[] { 0.88953, 0.887951, 0.8871507, 0.8877251, 0.86188, 0.858276, 0.8577662, 0.8573741, 0.84928, 0.845690, 0.8435979, 0.8437980, 0.83914, 0.83524, 0.83448, 0.83279, 0.83796, 0.83381, 0.83292, 0.83561 };
                double[] hai = new double[] { 42.843, 42.385, 41.940, 41.554, 58.081, 58.063, 57.780, 57.606, 66.621, 66.605, 66.459, 66.143, 72.389, 72.531, 72.751, 72.677, 72.605, 72.414, 72.389, 72.491 };
                for (int j = 0; j < N_tube; j++)
                {
                    for (int k = 0; k < Nelement; k++)
                    {
                        ma[j, k] = VaDistri.Va[j, k] * (Vel_ave[i]/ VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st[i];
                        //ha[j, k] = AirHTC.alpha(VaDistri.Va[j, k] * (Vel_ave / VaDistri.Va_ave), za, curve);
                        ha[j, k] = hai[i];
                    }
                }
                double[,] haw = ha;

                double[] tai = new double[] { 27.00, 27.00, 27.01, 27.01, 27.02, 27.01, 27.01, 27.01, 27.02, 27.02, 27.01, 27.00, 27.01, 27.00, 27.02, 27.01, 27.00, 26.99, 27.01, 27.01 };
                double[] RHi = new double[] { 0.4679, 0.4684, 0.4680, 0.4680, 0.4669, 0.4691, 0.4691, 0.4685, 0.4692, 0.4692, 0.4691, 0.4684, 0.4674, 0.4707, 0.4698, 0.4702, 0.4696, 0.4700, 0.4691, 0.4691 };

                double[] tri = new double[] { 9.98, 10.01, 10.01, 10.00, 10.01, 10.00, 10.02, 10.00, 10.01, 10.02, 10.01, 10.00, 10.00, 10.01, 10.02, 10.01, 10.01, 9.99, 10.00, 10.01 };

                double[] tc = tri;
                double[] pc = new double[N];
                coolprop.update(input_pairs.QT_INPUTS, 1, tc[i] + 273.15);
                pc[i] = coolprop.p() / 1000;
                //pc[i] = CoolProp.PropsSI("P", "T", tc[i] + 273.15, "Q", 1, fluid) / 1000;

                double Pwater = 395;//kpa
                double conductivity = 386; //w/mK for Cu
                int hexType = 0; //*********************************0 is evap, 1 is cond******************************************
                double[] hri = new double[N];
                coolprop.update(input_pairs.PT_INPUTS, Pwater * 1000, tc[i] + 273.15);
                hri[i] = coolprop.hmass() / 1000;
                //hri[i] = CoolProp.PropsSI("H", "T", tc[i] + 273.15, "P", Pwater * 1000, fluid) / 1000 ;

                double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
                double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

                string AirDirection = "逆流";
                ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai[i], tc[i], AirDirection);
                RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi[i], tc[i], AirDirection);



                //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, tc[i], pc[i], hri[i],
                    //mr[i], ma, ha, haw, eta_surface[i], zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

                res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, tc[i], pc[i], hri[i],
                    mr[i], ma, ha,haw, eta_surface[i], zh, zdp, hexType, thickness, conductivity, Pwater,AirDirection, d_cap, lenth_cap,coolprop);

                //using (StreamWriter wr = File.AppendText(@"D:\Work\Simulation\Test\Midea9_heat.txt"))
                //{
                //    wr.WriteLine("Q, {0}, DP, {1}, href, {2}, Ra_ratio, {3}, Tao, {4}, Tro, {5}", res.Q, res.DP, res.href, res.Ra_ratio, res.Tao, res.Tro);
                //}
                Q[i] = res.Q;
            }
            return res;
        }

        public static CalcResult MinNout()
        {
            //string fluid = new string[] { "Water" };
            string fluid = "R410A";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            //string fluid = new string[] { "ISOBUTAN" };
            double[] composition = new double[] { 1 };
            CalcResult res = new CalcResult();
            int Nrow = 2;
            double[] FPI = new double[Nrow + 1];
            //FPI = new double[] { 1.27, 1.27, 1.27, 1.27, 1.27, 1.27, 1.27, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 5.2, 5.2, 5.2, 5.2, 5.2, 5.2 };
            FPI = new double[] { 15, 15 };
            double Pt = 1 * 25.4 * 0.001;
            double Pr = 0.75 * 25.4 * 0.001;
            double Di = 8.4074 * 0.001;//8 6.8944
            double Do = 10.0584 * 0.001;//8.4 7.35
            double Fthickness = 0.095 * 0.001;
            double thickness = 0.5 * (Do - Di);
            //double n_tubes = 10;
            //double n_rows = 2;
            //int[] Ntube = { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
            //int[] Ntube = { 2, 2, 2, 2 };
            int[] Ntube = { 24, 24 };
            int N_tube = Ntube[0];
            double L = 914.4 * 0.001;
            int Nelement = 1;
            int[,] CirArrange;

            //CirArrange = new int[,] { { 25, 26, 27, 28, 4, 3, 2, 1, 0, 0 }, { 29, 30, 31, 7, 6, 5, 0, 0, 0, 0 }, { 32, 33, 34, 35, 11, 10, 9, 8, 0, 0 },
            //{ 36, 37, 38, 39, 40, 16, 15, 14, 13, 12 }, { 41, 42, 43, 19, 18, 17, 0, 0, 0, 0 }, { 44, 45, 46, 47, 48, 24, 23, 22, 21, 20 } };
            //CircuitNumber CircuitInfo = new CircuitNumber();
            //CircuitInfo.number = new int[] { 4, 2 }; //4in 2out
            //CircuitInfo.TubeofCir = new int[] { 8, 6, 8, 10, 6, 10 };  //{ 4, 8 };
            //CircuitInfo.UnequalCir = new int[] { 5, 5, 6, 6, 0, 0 };
            //// [19 - 17 - 15 - 13   11   9   7   5   3   1] <====Air
            //// [20 - 18 - 16 - 14   12   10  8   6   4   2] <====Air

            //5in2out, test3
            //CirArrange = new int[,] { { 25, 26, 27, 28, 4, 3, 2, 1, 0, 0 }, { 29, 30, 31, 7, 6, 5, 0, 0, 0, 0 }, { 43, 42, 41, 17, 18, 19, 0, 0, 0, 0 },
            //{40, 39, 15, 16, 0, 0, 0, 0, 0, 0 }, { 44, 45, 46, 47, 48, 24, 23, 22, 21, 20 }, { 36, 37, 38, 14, 13, 12, 0, 0, 0, 0 }, 
            //{32, 33, 34, 35, 11, 10, 9, 8, 0, 0 }};
            //CirArrange = new int[,] { { 25, 26, 27, 28, 4, 3, 2, 1, 0, 0 }, { 29, 30, 31, 7, 6, 5, 0, 0, 0, 0 }, { 32, 33, 34, 35, 11, 10, 9, 8, 0, 0 },
            //{44, 45, 46, 47, 48, 24, 23, 22, 21, 20 }, { 36, 37, 38, 14, 13, 12, 0, 0, 0, 0 }, { 39, 40, 41, 42, 43, 19, 18, 17, 16, 15 } 
            //};
            CirArrange = new int[,] { { 25, 26, 27, 28, 4, 3, 2, 1, 0, 0 }, { 29, 30, 31, 7, 6, 5, 0, 0, 0, 0 }, { 32, 33, 34, 35, 11, 10, 9, 8, 0, 0 },
            { 36, 37, 38, 39, 40, 16, 15, 14, 13, 12 }, {41, 42, 43, 19, 18, 17, 0, 0, 0, 0},{ 44, 45, 46, 47, 48, 24, 23, 22, 21, 20 } 
            };
            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { 4, 2 }; //4in 2out
            CircuitInfo.TubeofCir = new int[] { 8, 6, 8, 10, 6, 10 };  //{ 4, 8 };
            CircuitInfo.UnequalCir = new int[] { 5, 5, 6, 6, 0, 0 };

            double[] d_cap = new double[] { 0, 0, 0, 0 };
            double[] lenth_cap = new double[] { 0, 0, 0, 0 };

            GeometryInput geoInput_air = new GeometryInput();
            geoInput_air.Pt = Pt;
            geoInput_air.Pr = Pr;
            geoInput_air.Do = Do;
            geoInput_air.Fthickness = Fthickness;
            geoInput_air.FPI = FPI[0];
            geoInput_air.Nrow = Nrow;

            int hexType = 0; 

            double mr =0.06;
            double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
            double Vel_ave = 1.6;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
            AirDistribution VaDistri = new AirDistribution();
            VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
            double[,] ma = new double[N_tube, Nelement];
            double[,] ha = new double[N_tube, Nelement];
            double H = Pt * N_tube;
            double Hx = L * H;
            double rho_a_st = 1.2;
            double za = 1;

            //空气侧几何结构选择
            //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
            //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
            int curve = 1;

            for (int i = 0; i < N_tube; i++)
            {
                for (int j = 0; j < Nelement; j++)
                {
                    ma[i, j] = VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st;
                    //ha[i, j] = AirHTC.alpha(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve);
                    //ha[i, j] = 79;
                    ha[i, j] = AirHTC.alpha1(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve, geoInput_air, hexType).ha;
                }
            }
            double[,] haw = ha;

            res.DPa = AirHTC.alpha1(Vel_ave, za, curve, geoInput_air, hexType).dP_a;

            double eta_surface = 1;
            double zh = 1;
            double zdp = 1.5;

            double tai = 26.67;
            double RHi = 0.469;
            double tri = 7.2;
            double te = tri;
            coolprop.update(input_pairs.QT_INPUTS, 0, te + 273.15);
            double pe = coolprop.p() / 1000;
            //double pe = CoolProp.PropsSI("P", "T", te + 273.15, "Q", 0, fluid) / 1000;

            double P_exv = 1842.28;//kpa
            double T_exv = 24;//C
            double conductivity = 386; //w/mK for Cu
            double Pwater = 0;
            //int hexType = 0; //*********************************0 is evap, 1 is cond******************************************
            coolprop.update(input_pairs.PT_INPUTS, P_exv * 1000, T_exv + 273.15);
            double hri = coolprop.hmass() / 1000;
            //double hri = CoolProp.PropsSI("H", "T", T_exv + 273.15, "P", P_exv * 1000, fluid) / 1000 ;

            double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
            double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

            //string AirDirection="DowntoUp";
            string AirDirection = "逆流";
            ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai, te, AirDirection);
            RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi, te, AirDirection);


            //AreaResult geo = new AreaResult();
            //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, te, pe, hri,
                //mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, te, pe, hri,
                mr, ma, ha,haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater,AirDirection, d_cap, lenth_cap, coolprop);


            return res;
        }

        public static CalcResult NinMout()
        {
            //string fluid = new string[] { "Water" };
            string fluid = "R410A";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            //string fluid = new string[] { "ISOBUTAN" };
            CalcResult res = new CalcResult();
            int Nrow = 2;
            double[] FPI = new double[Nrow + 1];
            //FPI = new double[] { 1.27, 1.27, 1.27, 1.27, 1.27, 1.27, 1.27, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 2.6, 5.2, 5.2, 5.2, 5.2, 5.2, 5.2 };
            FPI = new double[] { 15, 15 };
            double Pt = 1 * 25.4 * 0.001;
            double Pr = 0.75 * 25.4 * 0.001;
            double Di = 8.4074 * 0.001;//8 6.8944
            double Do = 10.0584 * 0.001;//8.4 7.35
            double Fthickness = 0.095 * 0.001;
            double thickness = 0.5 * (Do - Di);
            //double n_tubes = 10;
            //double n_rows = 2;
            //int[] Ntube = { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
            //int[] Ntube = { 2, 2, 2, 2 };
            int[] Ntube = { 24, 24 };
            int N_tube = Ntube[0];
            double L = 914.4 * 0.001;
            int Nelement = 1;
            int[,] CirArrange;

            //CirArrange = new int[,] { { 25, 26, 27, 28, 4, 3, 2, 1, 0, 0 }, { 29, 30, 31, 7, 6, 5, 0, 0, 0, 0 }, { 32, 33, 34, 35, 11, 10, 9, 8, 0, 0 },
            //{ 36, 37, 38, 39, 40, 16, 15, 14, 13, 12 }, { 41, 42, 43, 19, 18, 17, 0, 0, 0, 0 }, { 44, 45, 46, 47, 48, 24, 23, 22, 21, 20 } };
            //CircuitNumber CircuitInfo = new CircuitNumber();
            //CircuitInfo.number = new int[] { 4, 2 }; //4in 2out
            //CircuitInfo.TubeofCir = new int[] { 8, 6, 8, 10, 6, 10 };  //{ 4, 8 };
            //CircuitInfo.UnequalCir = new int[] { 5, 5, 6, 6, 0, 0 };
            //// [19 - 17 - 15 - 13   11   9   7   5   3   1] <====Air
            //// [20 - 18 - 16 - 14   12   10  8   6   4   2] <====Air

            //5in2out, test3
            //CirArrange = new int[,] { { 25, 26, 27, 28, 4, 3, 2, 1, 0, 0 }, { 29, 30, 31, 7, 6, 5, 0, 0, 0, 0 }, { 43, 42, 41, 17, 18, 19, 0, 0, 0, 0 },
            //{40, 39, 15, 16, 0, 0, 0, 0, 0, 0 }, { 44, 45, 46, 47, 48, 24, 23, 22, 21, 20 }, { 36, 37, 38, 14, 13, 12, 0, 0, 0, 0 }, 
            //{32, 33, 34, 35, 11, 10, 9, 8, 0, 0 }};
            //CirArrange = new int[,] { { 41, 42, 43, 19, 18, 17, 0, 0, 0, 0 }, { 44, 45, 46, 47, 48, 24, 23, 22, 21, 20 }, { 25, 26, 27, 28, 4, 3, 2, 1, 0, 0 },
            //{29, 30, 31, 7, 6, 5, 0, 0, 0, 0 }, { 32, 33, 34, 35, 11, 10, 9, 8, 0, 0 }, { 36, 37, 38, 39, 40, 16, 15, 14, 13, 12 } 
            //}; 
            //Paralle flow
            CirArrange = new int[,] { { 17, 18, 19, 43, 42, 41, 0, 0, 0, 0 }, { 20, 21, 22, 23, 24, 48, 47, 46, 45, 44 }, { 1, 2, 3, 4, 28, 27, 26, 25, 0, 0 },
            {5, 6, 7, 31, 30, 29, 0, 0, 0, 0 }, { 8, 9, 10, 11, 35, 34, 33, 32, 0, 0 } , { 12, 13, 14, 15, 16, 40, 39, 38, 37, 36 } 
            };
            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { 4, 2 }; //4in 2out
            CircuitInfo.TubeofCir = new int[] { 6, 10, 8, 6, 8, 10 };  //{ 4, 8 };
            CircuitInfo.UnequalCir = new int[] { -5, -6, 5, 5, 6, 6 }; //{ 3, 4, -3, -3, -4, -4 };

            double[] d_cap = new double[] { 0, 0, 0, 0 };
            double[] lenth_cap = new double[] { 0, 0, 0, 0 };

            GeometryInput geoInput_air = new GeometryInput();
            geoInput_air.Pt = Pt;
            geoInput_air.Pr = Pr;
            geoInput_air.Do = Do;
            geoInput_air.Fthickness = Fthickness;
            geoInput_air.FPI = FPI[0];
            geoInput_air.Nrow = Nrow;

            int hexType = 0; 

            double mr = 0.076;
            //double Vel_a = 1.8; //m/s
            double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
            double Vel_ave = 1.8;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
            AirDistribution VaDistri = new AirDistribution();
            VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
            double[,] ma = new double[N_tube, Nelement];
            double[,] ha = new double[N_tube, Nelement];
            double H = Pt * N_tube;
            double Hx = L * H;
            double rho_a_st = 1.2; //kg/m3

            //空气侧几何结构选择
            //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
            //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
            int curve = 1; //

            double za = 1; //Adjust factor
            for (int i = 0; i < N_tube; i++)
            {
                for (int j = 0; j < Nelement; j++)
                {
                    ma[i, j] = VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave) * (Hx / N_tube / Nelement) * rho_a_st;
                    //ha[i, j] = AirHTC.alpha(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve) * 1.5;
                    //ha[i, j] = 79;
                    ha[i, j] = AirHTC.alpha1(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve, geoInput_air, hexType).ha;
                }
            }
            double[,] haw = ha;

            res.DPa = AirHTC.alpha1(Vel_ave, za, curve, geoInput_air, hexType).dP_a;

            double eta_surface = 1;
            double zh = 1;
            double zdp = 1;
            double tai = 26.67;
            double RHi = 0.469;
            double tri = 7.2;
            double te = tri;
            coolprop.update(input_pairs.QT_INPUTS, 0, te + 273.15);
            double pe = coolprop.p() / 1000;
            //double pe = CoolProp.PropsSI("P", "T", te + 273.15, "Q", 0, fluid) / 1000;

            double P_exv = 1842.28;//kpa
            double T_exv = 24;//C
            double conductivity = 386; //w/mK for Cu
            double Pwater = 0;
            //int hexType = 0; //*********************************0 is evap, 1 is cond******************************************
            coolprop.update(input_pairs.PT_INPUTS, P_exv * 1000, T_exv + 273.15);
            double hri = coolprop.hmass() / 1000;
            //double hri = CoolProp.PropsSI("H", "T", T_exv + 273.15, "P", P_exv * 1000, fluid) / 1000 ;

            double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
            double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];

            //string AirDirection="DowntoUp";
            string AirDirection = "逆流";
            ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai, te, AirDirection);
            RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi, te, AirDirection);


            //AreaResult geo = new AreaResult();
            //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, te, pe, hri,
                //mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, te, pe, hri,
                mr, ma, ha,haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater,AirDirection, d_cap, lenth_cap, coolprop);


            //res = Slab.SlabCalc(Npass, N_tubes_pass, fluid, composition, Dh, L, geo.A_a, geo.A_r_cs, geo.A_r, tai, tri, pe, hri,
            //    mr, ma, ha, eta_surface, zh, zdp);
            //Tsh_calc = res.Tro - (Refrigerant.SATP(fluid, composition, res.Pro, 1).Temperature - 273.15);

            // res = Slab.SlabCalc(Npass, N_tubes_pass, fluid, composition, Dh, L, geo.A_a, geo.A_r_cs, geo.A_r, tai, tri, pe, hri,
            //     mr, ma, ha, eta_surface, zh, zdp);
            // Tsh_calc = res.Tro - (Refrigerant.SATP(fluid, composition, res.Pro, 1).Temperature - 273.15);

            return res;
        }

        public static CalcResult Water_Midea_cir6()
        {
            CalcResult res = new CalcResult();
            string fluid = "Water";
            AbstractState coolprop = AbstractState.factory("HEOS", fluid);
            int Nrow = 3;
            int[] Ntube = { 14, 14, 14 };
            int N_tube = Ntube[0];
            double L = 367 * 0.001;
            double[] FPI = new double[Nrow + 1];
            FPI = new double[] { 17, 17, 17 };
            double Pt = 21 *0.001;//??
            double Pr = 13.37 *0.001;
            double Di = 6.8944 *0.001;
            double Do = 7.35 *0.001;
            double Fthickness = 0.095 *0.001;
            double thickness = (Do - Di) / 2;
            int Nelement = 3;//Element
            int[,] CirArrange;
            CirArrange = new int[,] { { 29, 30, 31, 17, 16, 15, 1, 2, 3 }, { 32, 33, 34, 20, 19, 18, 4, 5, 6 }, { 35, 36, 37, 23, 22, 21, 7, 8, 9 }, { 38, 39, 40, 26, 25, 24, 10, 11, 12 }, { 41, 27, 13, 0, 0, 0, 0, 0, 0 }, { 42, 28, 14, 0, 0, 0, 0, 0, 0 } };
            CircuitNumber CircuitInfo = new CircuitNumber();
            CircuitInfo.number = new int[] { 4, 2 };
            CircuitInfo.TubeofCir = new int[] { 9, 9, 9, 9, 3, 3 };
            CircuitInfo.UnequalCir = new int[] { 5, 5, 6, 6, 0, 0 };

            double[] d_cap = new double[] { 0, 0, 0, 0 };
            double[] lenth_cap = new double[] { 0, 0, 0, 0 };

            GeometryInput geoInput_air = new GeometryInput();
            geoInput_air.Pt = Pt;
            geoInput_air.Pr = Pr;
            geoInput_air.Do = Do;
            geoInput_air.Fthickness = Fthickness;
            geoInput_air.FPI = FPI[0];
            geoInput_air.Nrow = Nrow;

            int hexType = 1; 

            double mr = 23.0 / 60;
            //double Vel_a = 1.2;
            double[,] Vel_distribution = { { 1.0 } };//distribution,do not must be real velocity!
            double Vel_ave = 1.2;//average velocity, if Vel_distribution is real, then Vel_ave=1.0
            AirDistribution VaDistri = new AirDistribution();
            VaDistri = DistributionConvert.VaConvert(Vel_distribution, N_tube, Nelement);
            double[,] ma=new double[N_tube,Nelement];
            double[,] ha = new double[N_tube, Nelement];
            double H = Pt * N_tube;
            double Hx = L * H;
            double rho_a_st = 1.2;
            double za = 1;

            //空气侧几何结构选择
            //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 2, geometry parameter is:Do:7mm,Pt:21mm,Pl:22mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 3, geometry parameter is:Do:7mm,Pt:21mm,Pl:19.4mm,Fin_type:plain,Tf:0.1,Pf:1.5mm;
            //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
            int curve=1;

            for(int i=0;i<N_tube;i++)
            {
                for(int j=0;j<Nelement;j++)
                {
                    ma[i, j] = VaDistri.Va[i, j] * (Vel_ave/VaDistri.Va_ave)*(Hx / N_tube / Nelement) * rho_a_st;
                    //ha[i, j] = AirHTC.alpha(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve);
                    ha[i, j] = 79;
                    ha[i, j] = AirHTC.alpha1(VaDistri.Va[i, j] * (Vel_ave / VaDistri.Va_ave), za, curve, geoInput_air, hexType).ha;
                }
            }
            double[,] haw = ha;
            res.DPa = AirHTC.alpha1(Vel_ave, za, curve, geoInput_air, hexType).dP_a;

            //double Va = Vel_a * Hx;
            //double ma = Va * rho_a_st;
            double zh = 1;
            double zdp = 1;
            double eta_surface = 0.89;
            //double ha = 79;
            double tai = 20;
            double RHi = 0.469;
            double tri = 45;
            double tc = tri;
            coolprop.update(input_pairs.QT_INPUTS, 1, tc + 273.15);
            double pc = coolprop.p() / 1000;
            //double pc = CoolProp.PropsSI("P", "T", tc + 273.15, "Q", 1, fluid) / 1000;

            double Pwater = 305;//kPa
            double conductivity = 386;
            //int hexType = 1;//0-eva,1-con
            coolprop.update(input_pairs.PT_INPUTS, Pwater * 1000, tc + 273.15);
            double hri = coolprop.hmass() / 1000;
            //double hri = CoolProp.PropsSI("H", "T", tc + 273.15, "P", Pwater * 1000, fluid) / 1000 ;

            double[, ,] ta = new double[Nelement, N_tube, Nrow + 1];
            double[, ,] RH = new double[Nelement, N_tube, Nrow + 1];
            string AirDirection = "逆流";
            ta = InitialAirProperty.AirTemp(Nelement, Ntube, Nrow, tai, tc, AirDirection);
            RH = InitialAirProperty.RHTemp(Nelement, Ntube, Nrow, RHi, tc, AirDirection);


            //AreaResult geo = new AreaResult();
            //geo = Areas.Area(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            //res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid, Di, L, geo, ta, RH, tc, pc, hri,
                //mr, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, AirDirection, d_cap, lenth_cap);

            Geometry geo = new Geometry();
            geo = GeometryCal.GeoCal(Nrow, N_tube, Nelement, L, FPI, Do, Di, Pt, Pr, Fthickness);
            res = Slab.SlabCalc(CirArrange, CircuitInfo, Nrow, Ntube, Nelement, fluid,L, geo, ta, RH, tc, pc, hri,
                mr, ma, ha, haw,eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater,AirDirection, d_cap, lenth_cap, coolprop);

            return res;
        }
    
    }
}
