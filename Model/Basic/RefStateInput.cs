using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Basic
{
    public class RefStateInput
    {
        public string FluidName;
        public double Massflowrate;
        public double te;
        public double tc;
        public double tri;
        public double P_exv;
        public double T_exv;
        public double H_exv;
        public double zh;
        public double zdp;
        public int RefFlowDirection;
    }
}
