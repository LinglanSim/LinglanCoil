using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Basic
{
    public class CalcResult
    {
        public double Tro = 0;
        public double Pro = 0;
        public double hro = 0;
        public double Tao = 0; //= new double[1];
        public double[, ,] Tao_Detail = new double[100, 100, 100];
        public double Q = 0;
        public double M = 0;
        public double x_i = 0;
        public double x_o = 0;
        public double href = 0;
        public double ha = 0;
        public double R_1 = 0;
        public double R_1a = 0;
        public double R_1r = 0;
        public double DP = 0;
        public double Vel_r = 0;
        public double Ra_ratio = 0;
        public double mr = 0;
        public double Tri = 0;
        public double Pri = 0;
        public double hri = 0;
        public double ma = 0;
        public double Va = 0;

        public CalcResult ShallowCopy()
        {
            return (CalcResult)this.MemberwiseClone();
        }
    }
}
