using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Basic
{
    public class CircuitConvert
    {
        public static int[,] CircuitInputConvert(int[,] CircuitInput)
        {
            int N_circuit = CircuitInput.GetLength(0);
            int N_tube = CircuitInput.GetLength(1);
            int Max = 0;
            int N_element = 0;
            for(int i=0;i<N_circuit;i++)
            {
                N_element=0;
                for(int j=0;j<N_tube;j++)
                {
                    if (CircuitInput[i, j] != 0) N_element++;
                }
                Max = Math.Max(Max, N_element);
            }
            int[,] CirArrange = new int[N_circuit, Max];
            for(int i=0;i<N_circuit;i++)
            {
                for(int j=0;j<Max;j++)
                {
                    CirArrange[i, j] = CircuitInput[i, j];
                }
            }
            return CirArrange;
        }
        public static int[] TubeNumber(int[,] CircuitInput)
        {
            int N_circuit = CircuitInput.GetLength(0);
            int N_tube = CircuitInput.GetLength(1);
            int[] TubeofCircuit = new int[N_circuit];
            int N_element = new int();
            for(int i=0;i<N_circuit;i++)
            {
                N_element = 0;
                for(int j=0;j<N_tube;j++)
                {
                    if (CircuitInput[i, j] != 0) N_element++;
                }
                TubeofCircuit[i]=N_element;
            }
            return TubeofCircuit;
        }

    }
}
