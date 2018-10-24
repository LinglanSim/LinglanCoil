using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Basic
{
    public class AirHTC
    {
         public static double alpha(double vel, double za, int curve)
        {
            DataEntities me = new DataEntities();
            double ha;
            var data = me.AirCoef.First(a => a.Curve == curve);//首先取得字段
            ha = data.creHT.Value * Math.Pow(vel, data.reexpHT.Value);
            ha = ha * za;
            return ha;
        }

         public static AirCoef_res alpha1(double vel, double za, int curve, GeometryInput geoInput, int hexType)
         {
             AirCoef_res res = new AirCoef_res();
             //空气侧换热系数计算
             double Pt = geoInput.Pt ;
             double Pr = geoInput.Pr ;
             double Do = geoInput.Do ;
             double Fthickness = geoInput.Fthickness;
             double P_f = 25.4 / geoInput.FPI / 1000;
             int Nrow = geoInput.Nrow;

             double W_hex = Nrow * Pr;

             double rho_a_st = 1.2; //kg/m3
             double cp_a = (hexType == 0 ? 1.027 * 1000: 1.02 * 1000);//J/kg/K
             double k_a = 0.025;//W/K/m
             double mu_a = 18.2 * Math.Pow(10, -6);//Pa*s

             double De_c = 4 * (P_f - Fthickness) * (Pt * Pr - Math.PI * Math.Pow(Do + 2 * Fthickness, 2.0) / 4) / (2 * (Pt * Pr - Math.PI * Math.Pow(Do + 2 * Fthickness, 2.0) / 4) + Math.PI * (Do + 2 * Fthickness) * (P_f - Fthickness));

             double V_f = vel * (P_f * Pt * Pr) / ((P_f - Fthickness) * (Pt * Pr - Math.PI * Math.Pow(Do + 2 * Fthickness, 2.0) / 4));
             double Re_a = De_c * V_f * rho_a_st / mu_a;
             double Pr_a = mu_a * cp_a / k_a;
             double Nusselt_a = 2.1 * Math.Pow(Re_a * Pr_a * De_c / W_hex, 0.38);
             double ha = Nusselt_a * k_a / De_c * za;

             //空气侧压降计算
             double f = (0.43 + 35.1 * Math.Pow(Re_a * De_c / W_hex, -1.07)) * De_c / W_hex;
             double dP_a = 2 * f * W_hex * rho_a_st * Math.Pow(V_f, 2) / De_c;

             //ha = za * ha;

             return res = new AirCoef_res { ha = ha, dP_a = dP_a };
        }
    }
}
