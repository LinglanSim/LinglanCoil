using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Basic
{
    class CircuitIdentification
    {
        public static CircuitType CircuitIdentify(int[] number, int[] TubeofCir, CirArr[] cirArr)
        {
            CircuitType res = new CircuitType();
            //int N_tube = Ntube[0];
            if(number==null)
            {
                res.flag = false;
                return res;
            }
            array[] array0 = new array[number[0]];
            array[] array1 = new array[number[0]];
            for (int i = 0; i < number[0]; i++)
            {
                array0[i] = new array();
                array1[i] = new array();
            }//initialize
            if (number[0] != number[1])
                res.flag = false;
            else
            {
                res.flag = true;
                int k = 0;
                for (int i = 0; i < number[0]; i++)
                {
                    int[] Row = new int[TubeofCir[i]];
                    int[] Tube = new int[TubeofCir[i]];
                    for (int j = 0; j < TubeofCir[i]; j++)
                    {
                        Row[j] = cirArr[k].iRow;
                        Tube[j] = cirArr[k].iTube;
                        k++;
                    }
                    array0[i].Row = Row;
                    array0[i].Tube = Tube;
                }//initialize
                array1 = array0;
                int min = new int();
                for (int i = 0; i < number[0]; i++)
                {
                    min = array1[i].Tube[0];
                    for (int j = 0; j < TubeofCir[i]; j++)
                    {
                        array1[i].Tube[j] = array1[i].Tube[j] - min;
                    }
                }//unity
                int k1 = 1;
                int k2 = 1;
                for (int i = 0; i < number[0]; i++)
                {
                    if (EquOrSym.SelfEqu(array1[i]) == true)
                    {
                        array1[i].Type1 = 0;
                        if (k1 == 1)
                        {
                            array1[i].Type2 = k1;
                        }
                        else
                        {
                            for (int j = 0; j < i + 1; j++)
                            {
                                if (j < i)
                                {
                                    if (EquOrSym.equOrSym(array1[i], array1[j]) == true)
                                    {
                                        array1[i].Type2 = array1[j].Type2;
                                        break;
                                    }
                                }
                                if (j == i)
                                    array1[i].Type2 = k1;
                            }
                        }

                        k1++;
                    }
                    if (EquOrSym.SelfEqu(array1[i]) == false)
                    {
                        array1[i].Type1 = 1;
                        if (k2 == 1)
                        {
                            array1[i].Type2 = k2;
                        }
                        else
                        {
                            for (int j = 0; j < i + 1; j++)
                            {
                                if (j < i)
                                {
                                    if (EquOrSym.EquOnly(array1[i], array1[j]) == true)
                                    {
                                        array1[i].Type2 = array1[j].Type2;
                                        break;
                                    }
                                }
                                if (j == i)
                                    array1[i].Type2 = k2;
                            }
                        }
                        k2++;
                    }
                }//type1 type2 recognize



            }
            res.type = new int[number[0], 2];
            for (int i = 0; i < number[0]; i++)
            {
                res.type[i, 0] = array1[i].Type1;
                res.type[i, 1] = array1[i].Type2;
            }

            return res;
        }

    }
    class array
    {
        public int[] Row;
        public int[] Tube;
        public int Type1;
        public int Type2;
    }
    public class CircuitType
    {
        public int[,] type;
        public bool flag;
    }
    class EquOrSym
    {
        public static bool equOrSym(array array1, array array2)
        {
            bool flag = false;
            int N1 = array1.Row.Length;
            int N2 = array2.Row.Length;
            if (N1 == N2)
            {
                if (Equ(array1.Row, array2.Row) == true)
                {
                    if (Equ(array1.Tube, array2.Tube) == true | Sym(array1.Tube, array2.Tube) == true)
                        flag = true;
                }
            }
            return flag;
        }
        public static bool EquOnly(array array1, array array2)
        {
            bool flag = false;
            int N1 = array1.Row.Length;
            int N2 = array2.Row.Length;
            if (N1 == N2)
            {
                if (Equ(array1.Row, array2.Row) == true && Equ(array1.Tube, array2.Tube) == true)
                {
                    flag = true;
                }
            }
            return flag;
        }
        public static bool Equ(int[] A1, int[] A2)
        {
            bool flag = true;
            int N1 = A1.Length;
            int N2 = A2.Length;
            if (N1 != N2)
                flag = false;
            else
            {
                for (int i = 0; i < N1; i++)
                    if (A1[i] != A2[i])
                    {
                        flag = false;
                        break;
                    }
            }
            return flag;
        }
        public static bool Sym(int[] A1, int[] A2)
        {
            bool flag = true;
            int N1 = A1.Length;
            int N2 = A2.Length;
            if (N1 != N2)
                flag = false;
            else
            {
                for (int i = 0; i < N1; i++)
                    if (A1[i] + A2[i] != 0)
                    {
                        flag = false;
                        break;
                    }
            }
            return flag;
        }
        public static bool SelfEqu(array A1)
        {
            bool flag = false;
            int[] Row = A1.Row;
            int[] Tube = A1.Tube;
            int N = A1.Row.Length;
            int max = new int();
            for (int i = 0; i < N; i++)
            {
                if (max <= A1.Row[i])
                    max = A1.Row[i];
            }
            int[][] A2 = new int[max + 1][];
            int k = new int();
            for (int i = 0; i <= max; i++)
            {
                A2[i] = new int[N];
                k = 0;
                for (int j = 0; j < N; j++)
                {
                    if (Row[j] == i)
                    {
                        A2[i][k] = Tube[j];
                        k++;
                    }
                }
                Array.Sort(A2[i]);
            }
            if (max == 0)
                flag = true;
            if (max >= 1)
            {
                flag = true;
                for (int i = 0; i < max; i++)
                {
                    if (Equ(A2[i], A2[i + 1]) == false)
                    {
                        flag = false;
                        break;
                    }
                }
            }
            return flag;
        }

    }
}

