using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Basic
{
    public class CalcResult_CirArr
    {
        public int[,] CirArrange = new int[100 , 100];
        public int AddNumber_1;
        public int AddNumber_2;
        public int AddNumber_3;


        public CalcResult ShallowCopy()
        {
            return (CalcResult)this.MemberwiseClone();
        }
    }
}
