using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Basic
{
    public class Geometry
    {
        public AreaResult[,] ElementArea;
        public AreaResult TotalArea;
        public double Pt;
        public double Pr;
        public double Di;
        public double Do;
        public double Tthickness;
        public double L;
        public double[] FPI;
        public double Fthickness;
        public int Nrow;
        public int N_tube;
    }
    public class AreaResult
    {
        public double A_r_cs;
        public double A_r;
        public double A_fin_cs;
        public double A_hx;
        public double A_face;
        public double A_a;
        public double Aa_tube;
        public double Aa_fin;
        public double A_ratio;
    }

}
