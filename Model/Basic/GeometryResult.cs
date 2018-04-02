using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Basic
{
    public class AreaResult
    {
        public GeometryResult[,] element;
        public GeometryResult total;
    }
    public class GeometryResult
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
