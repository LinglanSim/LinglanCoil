using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Basic
{
    public class NodeInfo
    {
        public int Name;
        public int N_in;
        public int N_out;
        public int[] inlet;
        public int[] inType;
        public int[] outlet;
        public int[] outType;
        public char type;
        public double[] mri;
        public double[] mro;
        public double[] pri;
        public double[] pro;
        public double[] hri;
        public double[] hro;
        public double[] tri;
        public double[] tro;
        public double[] mr_ratio;
        public int couple;
    }
}
