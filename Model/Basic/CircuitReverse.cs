using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Basic
{
    public class CircuitReverse
    {
        public static int[,] CirReverse(bool reverse, int[,] CirArrange, CircuitNumber CircuitInfo)
        {
            int[,] CirArrangeCopy = new int[CircuitInfo.number[0], CircuitInfo.TubeofCir[CircuitInfo.number[0] - 1]];
            if (reverse)
            {
                for (int i = 0; i < CircuitInfo.number[0]; i++)
                {
                    for (int j = 0; j < CircuitInfo.TubeofCir[i]; j++)
                    {
                        CirArrangeCopy[i, j] = CirArrange[i, CircuitInfo.TubeofCir[i] - 1 - j];
                    }
                    for (int j = 0; j < CircuitInfo.TubeofCir[i]; j++)
                    {
                        CirArrange[i, j] = CirArrangeCopy[i, j];
                    }
                }
            }
            return CirArrange;
        }
    }
}
