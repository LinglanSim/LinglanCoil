using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Basic
{
    public class CirArr
    {
        public int iRow;
        public int iTube;
        public int iDirection;
        //public int iRow
        //{
        //    get { return IRow; }
        //    set { IRow = value; }
        //}
        //public int iTube
        //{
        //    get { return ITube; }
        //    set { ITube = value; }
        //}


    }
    public class CirArrforAir
    {
        public CirArr[] CirArr;
        public int[,] TotalDirection;

    }
}
