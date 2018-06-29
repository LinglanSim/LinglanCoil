using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Basic;

namespace Model
{
    public class Capiliary
    {
        public static Capiliary_res CapiliaryCalc(int index, string fluid, double d_cap, double lenth_cap, double tri, double pri, double hri, double mr, double Pwater, int hexType)
        {
            //毛细管守恒方程模型
            Capiliary_res res_cap = new Capiliary_res();
            
            double Ar_cs = Math.PI * Math.Pow(d_cap, 2) / 4;
            double g = mr / Ar_cs;

            double tsat = CoolProp.PropsSI("T", "P", pri * 1000, "Q", 0, fluid);
            double h_l = CoolProp.PropsSI("H", "T", tsat, "Q", 0, fluid) / 1000;
            double h_v = CoolProp.PropsSI("H", "T", tsat, "Q", 1, fluid) / 1000;
            
            double x_i = (hri - h_l) / (h_v - h_l);
            double rho_gi = CoolProp.PropsSI("D", "Q", 1, "P", (fluid == "Water" ? Pwater : pri) * 1000, fluid);
            double rho_li = CoolProp.PropsSI("D", "Q", 0, "P", (fluid == "Water" ? Pwater : pri) * 1000, fluid);
            double mgi = x_i * mr;
            double mli = (1 - x_i) * mr;
            double esp_i = 1 / (1 + (1 / x_i - 1) * rho_gi / rho_li);
            double Vgi = (x_i * mr) / (rho_gi * esp_i * Ar_cs);
            double Vli = (1 - x_i) * mr / (rho_li * (1 - esp_i) * Ar_cs);

            //初始化出口参数
            double x_o = x_i;
            double mgo = mgi;
            double mlo = mli;
            double rho_go = rho_gi;
            double rho_lo = rho_li;
            double esp_o = esp_i;
            double Vgo_set = Vgi;
            double Vlo = Vli;

            double rho_avg = rho_gi * x_i + rho_li * (1 - x_i);
            double ViscosityL = CoolProp.PropsSI("V", "T", tsat, "Q", 0, fluid);
            double ViscosityV = CoolProp.PropsSI("V", "T", tsat, "Q", 1, fluid);
            double ViscosityTP = x_i * ViscosityV + (1 - x_i) * ViscosityL;
            double ReTP = g * d_cap / ViscosityTP;
            double f_tpi = 0.11 * Math.Pow(0.0014 + 68 / ReTP, 0.25);//0.0014表面粗糙度
            double f_tp = f_tpi;

            double Vgo_cal = 0;
            int i = 0;

            double Pro = 0;
            double hro = 0;
            double tro = 0;
            double tro1 = CoolProp.PropsSI("T", "H", hri * 1000, "P", pri * 1000, fluid) - 273.15;
            double tro2 = 0;

            if (hri <= h_v && hri >= h_l)
            {
                do
                {
                    if (i != 0) Vgo_set = Vgo_cal;
                    //守恒方程
                    double a = f_tp / 4 * Math.Pow(mr, 2) / (2 * rho_avg * Math.Pow(Ar_cs, 2)) * Math.PI * d_cap + mr / lenth_cap * (x_o * Vgo_set + (1 - x_o) * Vlo) - mr / lenth_cap * (x_i * Vgi + (1 - x_i) * Vli);
                    Pro = pri - lenth_cap / Ar_cs * (f_tp / 4 * Math.Pow(mr, 2) / (2 * rho_avg * Math.Pow(Ar_cs, 2)) * Math.PI * d_cap + mr / lenth_cap * (x_o * Vgo_set + (1 - x_o) * Vlo) - mr / lenth_cap * (x_i * Vgi + (1 - x_i) * Vli));
                    hro = 1 / mr * (mr * hri + 1 / 2 * mgi * Math.Pow(Vgi, 2) + 1 / 2 * mli * Math.Pow(Vli, 2) - 1 / 2 * mgo * Math.Pow(Vgo_set, 2) - 1 / 2 * mlo * Math.Pow(Vlo, 2));

                    //新一轮赋值
                    double tsato = CoolProp.PropsSI("T", "P", Pro * 1000, "Q", 0, fluid);
                    double h_lo = CoolProp.PropsSI("H", "T", tsato, "Q", 0, fluid) / 1000;
                    double h_vo = CoolProp.PropsSI("H", "T", tsato, "Q", 1, fluid) / 1000;
                    rho_go = CoolProp.PropsSI("D", "Q", 1, "P", (fluid == "Water" ? Pwater : Pro) * 1000, fluid);
                    rho_lo = CoolProp.PropsSI("D", "Q", 0, "P", (fluid == "Water" ? Pwater : Pro) * 1000, fluid);
                    x_o = (hro - h_lo) / (h_vo - h_lo);
                    esp_o = 1 / (1 + (1 / x_o - 1) * rho_go / rho_lo);
                    Vgo_cal = (x_o * mr) / (rho_go * esp_o * Ar_cs);
                    Vlo = (1 - x_o) * mr / (rho_lo * (1 - esp_o) * Ar_cs);
                    mgo = x_o * mr;
                    mlo = (1 - x_o) * mr;
                    rho_avg = (rho_gi * x_i + rho_li * (1 - x_i) + rho_go * x_o + rho_lo * (1 - x_o)) / 2;

                    double ViscosityLo = CoolProp.PropsSI("V", "T", tsato, "Q", 0, fluid);
                    double ViscosityVo = CoolProp.PropsSI("V", "T", tsato, "Q", 1, fluid);
                    double ViscosityTPo = x_o * ViscosityVo + (1 - x_o) * ViscosityLo;
                    double ReTPo = g * d_cap / ViscosityTPo;
                    double f_tpo = 0.11 * Math.Pow(0.0014 + 68 / ReTPo, 0.25);
                    f_tp = (f_tpi + f_tpo) / 2;

                    i++;
                } while (Math.Abs((Vgo_cal - Vgo_set) / Vgo_cal) > 0.02);
            }
            
            else
            {
                do
                {
                    if (i != 0)  tro1 = tro2;
                    if (i != 0) Vgo_set = Vgo_cal;
                    //守恒方程
                    double a = f_tp / 4 * Math.Pow(mr, 2) / (2 * rho_avg * Math.Pow(Ar_cs, 2)) * Math.PI * d_cap + mr / lenth_cap * (x_o * Vgo_set + (1 - x_o) * Vlo) - mr / lenth_cap * (x_i * Vgi + (1 - x_i) * Vli);
                    Pro = pri - lenth_cap / Ar_cs * (f_tp / 4 * Math.Pow(mr, 2) / (2 * rho_avg * Math.Pow(Ar_cs, 2)) * Math.PI * d_cap + mr / lenth_cap * (x_o * Vgo_set + (1 - x_o) * Vlo) - mr / lenth_cap * (x_i * Vgi + (1 - x_i) * Vli));
                    hro = 1 / mr * (mr * hri + 1 / 2 * mgi * Math.Pow(Vgi, 2) + 1 / 2 * mli * Math.Pow(Vli, 2) - 1 / 2 * mgo * Math.Pow(Vgo_set, 2) - 1 / 2 * mlo * Math.Pow(Vlo, 2));

                    //新一轮赋值
                    double tsato = CoolProp.PropsSI("T", "P", Pro * 1000, "Q", 0, fluid);
                    double h_lo = CoolProp.PropsSI("H", "T", tsato, "Q", 0, fluid) / 1000;
                    double h_vo = CoolProp.PropsSI("H", "T", tsato, "Q", 1, fluid) / 1000;
                    rho_go = CoolProp.PropsSI("D", "Q", 1, "P", (fluid == "Water" ? Pwater : Pro) * 1000, fluid);
                    rho_lo = CoolProp.PropsSI("D", "Q", 0, "P", (fluid == "Water" ? Pwater : Pro) * 1000, fluid);
                    x_o = (hro - h_lo) / (h_vo - h_lo);
                    esp_o = 1 / (1 + (1 / x_o - 1) * rho_go / rho_lo);
                    Vgo_cal = (x_o * mr) / (rho_go * esp_o * Ar_cs);
                    Vlo = (1 - x_o) * mr / (rho_lo * (1 - esp_o) * Ar_cs);
                    mgo = x_o * mr;
                    mlo = (1 - x_o) * mr;
                    rho_avg = (rho_gi * x_i + rho_li * (1 - x_i) + rho_go * x_o + rho_lo * (1 - x_o)) / 2;

                    double ViscosityLo = CoolProp.PropsSI("V", "T", tsato, "Q", 0, fluid);
                    double ViscosityVo = CoolProp.PropsSI("V", "T", tsato, "Q", 1, fluid);
                    double ViscosityTPo = x_o * ViscosityVo + (1 - x_o) * ViscosityLo;
                    double ReTPo = g * d_cap / ViscosityTPo;
                    double f_tpo = 0.11 * Math.Pow(0.0014 + 68 / ReTPo, 0.25);
                    f_tp = (f_tpi + f_tpo) / 2;
                    tro2 = CoolProp.PropsSI("T", "H", hro * 1000, "P", Pro * 1000, fluid) - 273.15;

                    i++;
                } while (Math.Abs(tro2 - tro1) > 0.02);
            }
            
            double DP_cap = pri - Pro;
            tro = CoolProp.PropsSI("T", "H", hro * 1000, "P", Pro * 1000, fluid) - 273.15;

            res_cap.pro = Pro;
            res_cap.hro = hro;
            res_cap.tro = tro;
            res_cap.DP_cap = DP_cap;
            res_cap.x_o = x_o;
    
            return res_cap; 
        }

    }
}
