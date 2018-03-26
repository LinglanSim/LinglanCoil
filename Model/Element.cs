using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Basic;

namespace Model
{
    public class Element
    {
        public static CalcResult ElementCal(string[] fluid, double[] composition, double dh, double l, 
            double Aa_fin, double Aa_tube, double A_r_cs, double Ar, double tai, double RHi,
            double tri, double pri, double hri, double mr, double g, double ma, double ha,
            double eta_surface, double zh, double zdp, int hexType, double thickness, double conductivity, double Pwater)
        {
            var  r= new Refrigerant.SATTTotalResult();
            double href = 0;
            double gg = 9.8;
            double tsat;
            int phase1 = 1;
            int phase2 = 2;

            string fluidname = "R410A";
            tsat = Refrigerant.SATP(fluid, composition, pri, phase1).Temperature;
            //double tsat1 = CoolProp.PropsSI("T", "P", pri * 1000, "Q", 0, "R410A.mix");
            r = Refrigerant.SATTTotal(fluid, composition, tsat).SATTTotalResult;
            
            double Vol_tubes = A_r_cs * l;   //Tube volume, for charge calculation
            //double h_l = r.EnthalpyL;
            //double h_v = r.EnthalpyV;
            //double hl1 = CoolProp.PropsSI("H", "T", tsat, "Q", 0, "R410A.mix") / 1000 - (fluid[0] == "Water" ? 0 : 140);
            double h_l = CoolProp.PropsSI("H", "T", tsat, "Q", 0, fluidname) / 1000 - (fluid[0] == "Water" ? 0 : 140);
            //double hv1 = CoolProp.PropsSI("H", "T", tsat, "Q", 1, "R410A.mix") / 1000 - (fluid[0] == "Water" ? 0 : 140);
            double h_v = CoolProp.PropsSI("H", "T", tsat, "Q", 1, fluidname) / 1000 - (fluid[0] == "Water" ? 0 : 140);
            double Tri_mod;
            double alpha;
            double M;
            var res_element = new CalcResult();
            // **********Superheated state**********
            if (hri > h_v && fluid[0] != "Water")
            {
                if (hri < 1.02 * h_v)          //ignore modification for now, ruhao,20180226
                    Tri_mod = tri + 0.5 * 0;   //"for Tri modification in the transition region"  
                else
                    Tri_mod = tri;

                res_element = SPElement.ElementCalc(fluid, composition, dh, l, Aa_fin, Aa_tube, A_r_cs, Ar, tai, RHi, Tri_mod, pri, hri, mr, g, ma, ha, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater);
                res_element.x_i = (hri - h_l) / (h_v - h_l);
                res_element.x_o = (res_element.hro - h_l) / (h_v - h_l);
                alpha = 1; //set void fraction to 1 to identify a superheated state
                //{Equations are for charge calculation}
                double T_avg = (tri + res_element.Tro) / 2; //Average temperature of the element
                double P_avg = (pri + res_element.Pro) / 2; //Average pressure of the element
                double rho = Refrigerant.TPFLSH(fluid, composition, T_avg + 273.15, P_avg).D*r.Wm;//density(ref$, T=T_avg, P=P_avg) 
                //double rho1 = CoolProp.PropsSI("D", "P", P_avg * 1000, "T", T_avg + 273.15, "R410A.mix");
                double rho1 = CoolProp.PropsSI("D", "P", P_avg * 1000, "T", T_avg + 273.15, "R410A.mix");//计算结果相差较大
                M = Vol_tubes * rho; //"Mass calculated"
            }
         
            // **********Twophase state**********"
            if (hri <= h_v && hri >= h_l && fluid[0] != "Water")
            {
                res_element = TPElement.ElementCalc(fluid, composition, dh, l, Aa_fin, Aa_tube, A_r_cs, Ar, tai, tri, pri, hri, mr, g, ma, ha, eta_surface, zh, zdp, hexType, thickness, conductivity);
                //x=x_o  "outlet quality of the element" 
                double x_avg = (res_element.x_i + res_element.x_o) / 2; //Average quality of the element
                if (x_avg < 0) //"If negative, set quality to inlet value"
                    x_avg = res_element.x_i;

                double T_avg = (tri + res_element.Tro) / 2;  //Average temperature of the element
                //Call VOIDFRACTION_charge(ref$,x_avg, T_avg, G_r, Dh: alpha) "Average void fraction of the element"
                alpha = 1;
                //{Equations are for charge calculation}
                double P_avg = (pri + res_element.Pro) / 2; //Average pressure of the element
                //double rho_l = Refrigerant.SATP(fluid, composition, P_avg, phase1).DensityL*r.Wm;
                //double rho_v = Refrigerant.SATP(fluid, composition, P_avg, phase2).DensityV*r.Wm;
                //double rho_l1 = CoolProp.PropsSI("D", "P", P_avg * 1000, "Q", 0, "R410A.mix");
                //double rho_v1 = CoolProp.PropsSI("D", "P", P_avg * 1000, "Q", 1, "R410A.mix");
                double rho_l = CoolProp.PropsSI("D", "P", P_avg * 1000, "Q", 0, fluidname);
                double rho_v = CoolProp.PropsSI("D", "P", P_avg * 1000, "Q", 1, fluidname);
                //{Call VOIDFRACTION_pressure(ref$, x_avg, P_avg : alpha_p)  "Baroczy void fraction model"     }
                M = Vol_tubes * (alpha * rho_v + (1 - alpha) * rho_l);  //Mass calculated   
            }
            
            //**********Subcooled state**********
            if (hri < h_l || fluid[0] == "Water")
            {
                if (hri > 0.98 * h_l)            //ignore modification for now, ruhao,20180226
                    Tri_mod = tri - 0.5 * 0;    //"for Tri modification in the transition region"
                else
                    Tri_mod = tri;

                if (fluid[0] == "Water") Tri_mod = tri - 0.0001;

                res_element = SPElement.ElementCalc(fluid, composition, dh, l, Aa_fin, Aa_tube, A_r_cs, Ar, tai, RHi, Tri_mod, pri, hri, mr, g, ma, ha, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater);
                //Call SUBCOOLED(ref$, Dh, L, A_a, A_r, Tai, Tri_mod, Pri, hri, m_r, G_r, m_a, h_air, eta_surface: Tro, Pro, hro, Tao, Q, h_ref, R_1, R_1a, R_1r, DELTAP, Vel_r )
                res_element.x_i = (hri - h_l) / (h_v - h_l);
                res_element.x_o = (res_element.hro - h_l) / (h_v - h_l);
                //x=x_o  "outlet quality of the element" 
                //{x=-1 "set quality to -100 to identify a subcooled state"}
                alpha = -1; //set void fraction to -100 to identify a subcooled state
                //{Equations are for charge calculation}
                double T_avg = (tri + res_element.Tro) / 2; //Average temperature of the element
                double P_avg = (pri + res_element.Pro) / 2; //Average pressure of the element
                //double rho = Refrigerant.TPFLSH(fluid, composition, T_avg, P_avg).D * r.Wm; //density(ref$, T=T_avg, P=P_avg) 
                //double rho1 = CoolProp.PropsSI("D", "P", P_avg * 1000, "T", T_avg + 273.15, "R410A.mix");
                double rho = CoolProp.PropsSI("D", "P", P_avg * 1000, "T", T_avg + 273.15, fluidname);
                M = Vol_tubes * rho; //Mass calculated
            }


            return res_element;
        }
    }
}
