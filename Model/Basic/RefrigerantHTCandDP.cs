using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Basic
{
    public class RefrigerantHTCandDP
    {
        public static RefHTCandDPResult HTCandDP_2p(string fluid, double d, double g, double p, double x, double l, double q, double zh, double zdp, double hexType)
        {
            //Main function for 2-phase flow HTC and calculation
            double href = 0;
            double PressD = 0;
            //********Smoothening HTC & DP at single-to-2 phase transitions********"
            //Use lambda to avoid discontinutities of the two phase-liquid and vapor-two-phase and liquid"
            double x_transition;
            double lambdaL = 0;
            double lambdaV = 0;
            double h_ref_2phase, deltap_2phase;
            double beta = 50; //"Controlling factor for the sharpness of the transition"
            RefHTCandDPResult htc_dp_single_sat = new RefHTCandDPResult();
            if (x <= 0.05 && x >= 0)
            {
                x_transition = 0.05;
                lambdaL = (1 + Math.Tanh(beta * (x - x_transition))) / 2;
            }
            else if (x < 1 && x > 0.9)
            {
                x_transition = 0.9; //"0.95 for pressure drop" 
                lambdaV = (1 + Math.Tanh(beta * (x - x_transition))) / 2;
            }
            //"********Smoothening HTC & DP at single-to-2 phase transitions END********"

            //*************Smoothening HTC & DP at single-to-2 phase transitions****************
            //********divided to 3 region based on quality (0 - 0.05 - 0.95 - 1) to calculated HTC & DP********
            if (hexType == 0)
            {
                if (x < 0.05 && x > 0)
                {
                    h_ref_2phase = RefrigerantTPHTC.Shah_Evap_href(fluid, d, g, p, x, q, l);
                    //href = RefrigerantTPHTC.Kandlikar_Evap_href(fluid, composition, d, g, p, x, q, l);
                    //href = RefrigerantTPHTC.JR_Evap_href(fluid, composition, d, g, p, x, q, l);
                    deltap_2phase = RefrigerantTPDP.deltap_smooth(fluid, d, g, p, x, l);
                    //PressD = RefrigerantTPDP.deltap_JR(fluid, composition, d, g, p, x, l);
                    //PressD = RefrigerantTPDP.deltap_MS(fluid, composition, d, g, p, x, l);
                    htc_dp_single_sat = RefrigerantHTCandDP.HTCandDP_1p_sat(fluid, d, g, p, 0, l); //x=0
                    href = (1 - lambdaL) * htc_dp_single_sat.Href + lambdaL * h_ref_2phase;
                    PressD = (1 - lambdaL) * htc_dp_single_sat.DPref + lambdaL * deltap_2phase;
                }
                else if (x <= 0.9)
                {
                    href = RefrigerantTPHTC.Shah_Evap_href(fluid, d, g, p, x, q, l);
                    //href = RefrigerantTPHTC.Kandlikar_Evap_href(fluid, composition, d, g, p, x, q, l);
                    //href = RefrigerantTPHTC.JR_Evap_href(fluid, composition, d, g, p, x, q, l);
                    PressD = RefrigerantTPDP.deltap_smooth(fluid, d, g, p, x, l);
                    //PressD = RefrigerantTPDP.deltap_JR(fluid, composition, d, g, p, x, l);
                    //PressD = RefrigerantTPDP.deltap_MS(fluid, composition, d, g, p, x, l);
                }
                else //vapor to 2-phase
                {
                    h_ref_2phase = RefrigerantTPHTC.Shah_Evap_href(fluid, d, g, p, x, q, l);
                    //href = RefrigerantTPHTC.Kandlikar_Evap_href(fluid, composition, d, g, p, x, q, l);
                    //href = RefrigerantTPHTC.JR_Evap_href(fluid, composition, d, g, p, x, q, l);
                    deltap_2phase = RefrigerantTPDP.deltap_smooth(fluid, d, g, p, x, l);
                    //PressD = RefrigerantTPDP.deltap_JR(fluid, composition, d, g, p, x, l);
                    //PressD = RefrigerantTPDP.deltap_MS(fluid, composition, d, g, p, x, l);
                    htc_dp_single_sat = RefrigerantHTCandDP.HTCandDP_1p_sat(fluid, d, g, p, 1, l); //x=1
                    href = (1 - lambdaV) * h_ref_2phase + lambdaV * htc_dp_single_sat.Href;
                    PressD = (1 - lambdaV) * deltap_2phase + lambdaV * htc_dp_single_sat.DPref;
                }
            }
            else
            {
                if (x < 0.05 && x >= 0)
                {
                    h_ref_2phase = RefrigerantTPHTC.Shah_Cond_href(fluid, d, g, p, x, q, l);
                    //href = RefrigerantTPHTC.Dobson_Cond_href(fluid, composition, d, g, p, x, Ts, l);//需要考虑Ts的参数传递
                    deltap_2phase = RefrigerantTPDP.deltap_smooth(fluid, d, g, p, x, l);
                    //PressD = RefrigerantTPDP.deltap_JR(fluid, composition, d, g, p, x, l);
                    //PressD = RefrigerantTPDP.deltap_MS(fluid, composition, d, g, p, x, l);
                    htc_dp_single_sat = RefrigerantHTCandDP.HTCandDP_1p_sat(fluid, d, g, p, 0, l); //x=0
                    href = (1 - lambdaL) * htc_dp_single_sat.Href + lambdaL * h_ref_2phase;
                    PressD = (1 - lambdaL) * htc_dp_single_sat.DPref + lambdaL * deltap_2phase;
                }
                else if (x <= 0.9)
                {
                    href = RefrigerantTPHTC.Shah_Cond_href(fluid, d, g, p, x, q, l);
                    //href = RefrigerantTPHTC.Dobson_Cond_href(fluid, composition, d, g, p, x, Ts, l);//需要考虑Ts的参数传递
                    PressD = RefrigerantTPDP.deltap_smooth(fluid, d, g, p, x, l);
                    //PressD = RefrigerantTPDP.deltap_JR(fluid, composition, d, g, p, x, l);
                    //PressD = RefrigerantTPDP.deltap_MS(fluid, composition, d, g, p, x, l);
                }
                else //vapor to 2-phase
                {
                    h_ref_2phase = RefrigerantTPHTC.Shah_Cond_href(fluid, d, g, p, x, q, l);
                    //href = RefrigerantTPHTC.Dobson_Cond_href(fluid, composition, d, g, p, x, Ts, l);//需要考虑Ts的参数传递
                    deltap_2phase = RefrigerantTPDP.deltap_smooth(fluid, d, g, p, x, l);
                    //PressD = RefrigerantTPDP.deltap_JR(fluid, composition, d, g, p, x, l);
                    //PressD = RefrigerantTPDP.deltap_MS(fluid, composition, d, g, p, x, l);
                    htc_dp_single_sat = RefrigerantHTCandDP.HTCandDP_1p_sat(fluid, d, g, p, 1, l); //x=1
                    href = (1 - lambdaV) * h_ref_2phase + lambdaV * htc_dp_single_sat.Href;
                    PressD = (1 - lambdaV) * deltap_2phase + lambdaV * htc_dp_single_sat.DPref;
                }
                //********divided to 3 region based on quality (0 - 0.05 - 0.95 - 1) to calculated HTC & DP END********
                
            }

            href = href * zh;
            PressD = PressD * zdp;

            return new RefHTCandDPResult { Href = href, DPref = PressD };

        }

        public static RefHTCandDPResult HTCandDP_1p_sat(string fluid, double d, double g, double p, double x, double l)
        {
            double mu, k, rho, cp, Pr, Vel, Re, fh, f, Nusselt, href, PressD;
            mu = CoolProp.PropsSI("V", "P", p * 1000, "Q", x, fluid);
            k = CoolProp.PropsSI("L", "P", p * 1000, "Q", x, fluid);
            rho = CoolProp.PropsSI("D", "P", p * 1000, "Q", x, fluid);
            cp = CoolProp.PropsSI("C", "P", p * 1000, "Q", x, fluid);
            Pr = cp * mu / k;
            Vel = g / rho;
            Re = rho * Vel * d / mu;
            fh = RefrigerantSPHTC.ff_CHURCHILL(Re);
            Nusselt = RefrigerantSPHTC.NU_GNIELINSKI(Re, Pr, fh);
            href = Nusselt * k / d; //"Heat transfer coefficient"
            f = RefrigerantSPDP.ff_Friction(Re);
            PressD = f * l / d * Math.Pow(g, 2) / rho / 2000;    //"kPa, for x>0.95"
            return new RefHTCandDPResult { Href = href, DPref = PressD };
        }
    }
}
