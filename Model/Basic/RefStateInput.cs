using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Basic
{
    public class RefStateInput
    {
        public string FluidName;//(O)
        public double Massflowrate;//Cond_out质量流量(mr)/Evap_out质量流量(mro_Evap)(O)
        
        public double tc;//Cond_in饱和温度(tc)(O)
        public double tri;//Cond_in温度(排气温度)(tri)(O)
        public double xo_Cond;//Cond_out干度
        public double Tro_sub_Cond;//Cond_out过冷度(Tro_sub_Cond)(O)

        public double xi_Evap;//Evap_in干度
        public double H_exv;//Evap_in焓(Hri_Evap)
        public double P_exv;//Evap_in阀前压力(Pri_ValveBefore)
        public double T_exv;//Evap_in阀前温度(Tri_ValveBefore)
        public double te;//Evap_out饱和温度(Tcro_Evap)(O)
        public double Tro_sub_Evap;//Evap_out过热度(Tro_sub_Evap)(O)
        public double xo_Evap;//Evap_out干度

        public double zh;
        public double zdp;
    }
}
