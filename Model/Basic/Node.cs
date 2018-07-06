using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Basic
{
    class NodeInfo
    {
        public int Name;
        public int inNumber;
        public int outNumber;
        public int[] inlet;
        public int[] outlet;
        public string type;
        public double[] mri;
        public double[] mro;
        public double[] pri;
        public double[] pro;
        public double[] hri;
        public double[] hro;
        public double[] tri;
        public double[] tro;
        public double[] mr_ratio;
    }
}
