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
        public static CalcResult ElementCalc(string[] fluid, double[] composition, double dh, double l, double Aa_fin, double Aa_tube, double A_r_cs, double Ar, double tai, 
            double tri, double pri, double hri, double mr, double g, double ma, double ha,
            double eta_surface, double zh, double zdp, int hexType, double thickness, double conductivity)
        {
            double r_metal = thickness / conductivity / Ar;
            double gg = 9.8;
            //double temperature;
            int phase1 = 1;
            int phase2 = 2;
            double q_initial = 0.01;
            double q = q_initial;
            double err = 0.01;
            bool flag = false;
            int iter = 1;

            CalcResult res=new CalcResult();
            //res.Tao[0] = new double();          
            var r = new Refrigerant.SATTTotalResult();
            //temperature = Refrigerant.SATP(fluid, composition, pri, phase1).Temperature;
            r = Refrigerant.SATTTotal(fluid, composition, tri + 273.15).SATTTotalResult; //temperature
            res.x_i = (hri - r.EnthalpyL) / (r.EnthalpyV - r.EnthalpyL);   //+ 140 for reference state, to be changed

            RefHTCandDPResult htc_dp = new RefHTCandDPResult();

            do
            {
                flag = false;
                htc_dp = RefrigerantHTCandDP.HTCandDP(fluid, composition, dh, g, pri, res.x_i, l, q, zh, zdp);

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

                res.x_o = (res.hro - r.EnthalpyL) / (r.EnthalpyV - r.EnthalpyL); //+ 139.17 for reference state, to be changed
                //res.DP = 0;
                res.Pro = pri - res.DP;
                res.Tro = Refrigerant.PHFLSH(fluid, composition, res.Pro, (res.hro + (fluid[0] == "Water" ? 0 : 140)) * r.Wm).t; // 
                double rho_o = Refrigerant.TQFLSH(fluid, composition, res.Tro, res.x_o).D * r.Wm;
                //    .TPFLSH(fluid, composition, res.Tro, res.Pro).D * r.Wm;//wrong value
                //double rho_o = Refrigerant.PHFLSH(fluid, composition, res.Pro, (res.hro + 140) * r.Wm).D * r.Wm;
                //if (res.x_o > 1 || res.x_o < 0)
                //    rho_o = Refrigerant.PHFLSH(fluid, composition, res.Pro, res.hro).D;
                ////rho_o=density(ref$, P=Pri, T=Tro)
                //else
                //    rho_o = 0;
                //    //rho_o=density(ref$, P=Pri, x=x_o)
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


    }
}
