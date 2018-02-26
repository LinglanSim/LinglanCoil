using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Basic
{
    public class AutoCircuiting
    {
        public static CalcCirArr CircuitArrange_0(int iCircuit, int Nrow, int Tubeof_Nrow, int N_tube, int[,] CirArrange)
        {
            CalcCirArr res_CirArr = new CalcCirArr();

            int AddNumber_1 = 0;
            int AddNumber_2 = 0;
            int AddNumber_3 = 0;

            for (int iTube_Cir = 0; iTube_Cir < Nrow * Tubeof_Nrow; iTube_Cir++)
            {
                if (iTube_Cir < Tubeof_Nrow)
                {
                    CirArrange[iCircuit, iTube_Cir] = iTube_Cir + 1 + N_tube * (Nrow - 1);
                    AddNumber_1 = AddNumber_1 + 1;
                    
                }
                else if (iTube_Cir >= Tubeof_Nrow && iTube_Cir < (Nrow - 1) * Tubeof_Nrow)
                {
                    CirArrange[iCircuit, iTube_Cir] = 2 * Tubeof_Nrow - iTube_Cir + N_tube * (Nrow - 2);
                    AddNumber_2 += 1;
                }
                else
                {
                    CirArrange[iCircuit, iTube_Cir] = iTube_Cir - 2 * Tubeof_Nrow + 1 + N_tube * (Nrow - 3);
                    AddNumber_3 += 1;
                }
            }

            res_CirArr.AddNumber_1 = AddNumber_1;
            res_CirArr.AddNumber_2 = AddNumber_2;
            res_CirArr.AddNumber_3 = AddNumber_3;
            res_CirArr.CirArrange = CirArrange;
            return res_CirArr;
        }
        public static CalcCirArr CircuitArrange_1(int iCircuit, int Nrow, int Tubeof_Nrow, int N_tube, int[,] CirArrange)
        {
            CalcCirArr res_CirArr = new CalcCirArr();

            int AddNumber_1 = 0;
            int AddNumber_2 = 0;
            int AddNumber_3 = 0;

            int iCircuit1 = iCircuit + 1;
            int iCircuit2 = iCircuit + 2;
            
            //第一路
                for (int iTube_Cir = 0; iTube_Cir < Nrow * Tubeof_Nrow + 1; iTube_Cir++)
                {
                    if (iTube_Cir < Tubeof_Nrow)
                    {
                        CirArrange[iCircuit, iTube_Cir] = iTube_Cir + 1 + N_tube * (Nrow - 1);
                        AddNumber_1 = AddNumber_1 + 1;
                    }
                    else if (iTube_Cir >= Tubeof_Nrow && iTube_Cir < (Nrow - 1) * Tubeof_Nrow)
                    {
                        CirArrange[iCircuit, iTube_Cir] = 2 * Tubeof_Nrow - iTube_Cir + N_tube * (Nrow - 2);
                        AddNumber_2 += 1;
                    }
                    else
                    {
                        CirArrange[iCircuit, iTube_Cir] = iTube_Cir - 2 * Tubeof_Nrow + 1 + N_tube * (Nrow - 3);
                        AddNumber_3 += 1;
                    }
                }
            
            //第二路
                for (int iTube_Cir = 0; iTube_Cir < Nrow * Tubeof_Nrow + 1; iTube_Cir++)
                {
                    if (iTube_Cir < Tubeof_Nrow)
                    {
                        CirArrange[iCircuit1, iTube_Cir] = 2 * Tubeof_Nrow - iTube_Cir + N_tube * (Nrow - 1);
                        AddNumber_1 = AddNumber_1 + 1;
                    }
                    else if (iTube_Cir >= Tubeof_Nrow && iTube_Cir < (Nrow - 1) * Tubeof_Nrow + 1)
                    {
                        CirArrange[iCircuit1, iTube_Cir] = iTube_Cir + 1 + N_tube * (Nrow - 2);
                        AddNumber_2 += 1;
                    }
                    else
                    {
                        CirArrange[iCircuit1, iTube_Cir] = 4 * Tubeof_Nrow + 2 - iTube_Cir + N_tube * (Nrow - 3);
                        AddNumber_3 += 1;
                    }
                }
            
            //第三路
                for (int iTube_Cir = 0; iTube_Cir < Nrow * Tubeof_Nrow + 1; iTube_Cir++)
                {
                    if (iTube_Cir < Tubeof_Nrow + 1)
                    {
                        CirArrange[iCircuit2, iTube_Cir] = 2 * Tubeof_Nrow + iTube_Cir + 1 + N_tube * (Nrow - 1);
                        AddNumber_1 = AddNumber_1 + 1;
                    }
                    else if (iTube_Cir >= (Tubeof_Nrow + 1) && iTube_Cir < (Nrow - 1) * Tubeof_Nrow + 1)
                    {
                        CirArrange[iCircuit2, iTube_Cir] = 4 * Tubeof_Nrow + 2 - iTube_Cir + N_tube * (Nrow - 2);
                        AddNumber_2 += 1;
                    }
                    else
                    {
                        CirArrange[iCircuit2, iTube_Cir] = iTube_Cir + 1 + N_tube * (Nrow - 3);
                        AddNumber_3 += 1;
                    }
                }
              

            res_CirArr.AddNumber_1 = AddNumber_1;
            res_CirArr.AddNumber_2 = AddNumber_2;
            res_CirArr.AddNumber_3 = AddNumber_3;
            res_CirArr.CirArrange = CirArrange;
            return res_CirArr;
        }
        public static CalcCirArr CircuitArrange_2(int iCircuit, int Nrow, int Tubeof_Nrow, int N_tube, int[,] CirArrange)
        {
            CalcCirArr res_CirArr = new CalcCirArr();

            int AddNumber_1 = 0;
            int AddNumber_2 = 0;
            int AddNumber_3 = 0;

            int iCircuit1 = iCircuit + 1;
            int iCircuit2 = iCircuit + 2;

            //第一路
            for (int iTube_Cir = 0; iTube_Cir < Nrow * Tubeof_Nrow + 2; iTube_Cir++)
            {
                if (iTube_Cir < Tubeof_Nrow)
                {
                    CirArrange[iCircuit, iTube_Cir] = Tubeof_Nrow - iTube_Cir + N_tube * (Nrow - 1);
                    AddNumber_1 = AddNumber_1 + 1;
                }
                else if (iTube_Cir >= Tubeof_Nrow && iTube_Cir < ((Nrow - 1) * Tubeof_Nrow + 1))
                {
                    CirArrange[iCircuit, iTube_Cir] = iTube_Cir - 1 + N_tube * (Nrow - 2);
                    AddNumber_2 += 1;
                }
                else
                {
                    CirArrange[iCircuit, iTube_Cir] = 3 * Tubeof_Nrow + 2 - iTube_Cir + N_tube * (Nrow - 3);
                    AddNumber_3 += 1;
                }
            }

            //第二路
            for (int iTube_Cir = 0; iTube_Cir < Nrow * Tubeof_Nrow + 2; iTube_Cir++)
            {
                if (iTube_Cir < Tubeof_Nrow + 1)
                {
                    CirArrange[iCircuit1, iTube_Cir] = Tubeof_Nrow + iTube_Cir + 1 + N_tube * (Nrow - 1);
                    AddNumber_1 = AddNumber_1 + 1;
                }
                else if (iTube_Cir >= (Tubeof_Nrow + 1) && iTube_Cir < (Nrow - 1) * Tubeof_Nrow + 1)
                {
                    CirArrange[iCircuit1, iTube_Cir] =  3 * Tubeof_Nrow + 2 - iTube_Cir + N_tube * (Nrow - 2);
                    AddNumber_2 += 1;
                }
                else
                {
                    CirArrange[iCircuit1, iTube_Cir] = iTube_Cir - 1 + N_tube * (Nrow - 3);
                    AddNumber_3 += 1;
                }
            }

            //第三路
            for (int iTube_Cir = 0; iTube_Cir < Nrow * Tubeof_Nrow + 2; iTube_Cir++)
            {
                if (iTube_Cir < Tubeof_Nrow + 1)
                {
                    CirArrange[iCircuit2, iTube_Cir] = 3 * Tubeof_Nrow - iTube_Cir + 2 + N_tube * (Nrow - 1);
                    AddNumber_1 = AddNumber_1 + 1;
                }
                else if (iTube_Cir >= (Tubeof_Nrow + 1) && iTube_Cir < (Nrow - 1) * (Tubeof_Nrow + 1))
                {
                    CirArrange[iCircuit2, iTube_Cir] = Tubeof_Nrow + 1 + iTube_Cir + N_tube * (Nrow - 2);
                    AddNumber_2 += 1;
                }
                else
                {
                    CirArrange[iCircuit2, iTube_Cir] = 5 * Tubeof_Nrow + 4 - iTube_Cir + N_tube * (Nrow - 3);
                    AddNumber_3 += 1;
                }
            }


            res_CirArr.AddNumber_1 = AddNumber_1;
            res_CirArr.AddNumber_2 = AddNumber_2;
            res_CirArr.AddNumber_3 = AddNumber_3;
            res_CirArr.CirArrange = CirArrange;
            return res_CirArr;
        }

        public static CircuitNumber GetTubeofCir(int Nrow, int N_tube, CircuitNumber CircuitInfo)
        {
            int N_total = Nrow * N_tube;
            int a = N_total / CircuitInfo.number[0];
            int b = N_total % CircuitInfo.number[0];
            if (b == 0)
            {
                if (a % 2 == 0)
                {
                    for (int i = 0; i < CircuitInfo.number[0]; i++)
                        CircuitInfo.TubeofCir[i] = a;
                }
                else
                {
                    if (CircuitInfo.number[0] % 2 != 0)
                        throw new Exception("error for exchanger input.");
                    for (int i = 0; i < CircuitInfo.number[0]; i++)
                    {
                        if (i < CircuitInfo.number[0] / 2)
                            CircuitInfo.TubeofCir[i] = a - 1;
                        else
                            CircuitInfo.TubeofCir[i] = a + 1;
                    }
                }
            }
            else
            {
                if (a % 2 == 0)
                {
                    int c = b / 2;
                    for (int i = 0; i < CircuitInfo.number[0]; i++)
                    {
                        if (i < CircuitInfo.number[0] - c)
                            CircuitInfo.TubeofCir[i] = a;
                        else
                            CircuitInfo.TubeofCir[i] = a + 2;
                    }
                }
                else
                {
                    for (int i = 0; i < CircuitInfo.number[0]; i++)
                    {
                        if (i >= CircuitInfo.number[0] - b)
                            CircuitInfo.TubeofCir[i] = a + 1;
                        else
                        {
                            if (i < (CircuitInfo.number[0] - b) / 2)
                                CircuitInfo.TubeofCir[i] = a - 1;
                            else
                                CircuitInfo.TubeofCir[i] = a + 1;
                        }
                    }
                }
            }
            return CircuitInfo;
        }

        public static int[,] GetCirArrange_3Row(int[,] CirArrange, int Nrow, int N_tube, CircuitNumber CircuitInfo)
        {
            CirArrange = new int[CircuitInfo.number[0], CircuitInfo.TubeofCir[CircuitInfo.number[0] - 1]];

            int AddNumber1 = 0;
            int AddNumber2 = 0;
            int AddNumber3 = 0;

            CalcCirArr res_CirArr1 = new CalcCirArr();

            for (int iCircuit = 0; iCircuit < CircuitInfo.number[0]; iCircuit++)
            {
                if (CircuitInfo.TubeofCir[iCircuit] % Nrow == 0)
                {
                    res_CirArr1 = CircuitArrange_0(iCircuit, Nrow, CircuitInfo.TubeofCir[iCircuit] / Nrow, N_tube, CirArrange);
                    CirArrange = res_CirArr1.CirArrange;

                    for (int iTube_Cir = 0; iTube_Cir < Nrow * CircuitInfo.TubeofCir[iCircuit] / Nrow; iTube_Cir++)
                    {
                        if (iTube_Cir < CircuitInfo.TubeofCir[iCircuit] / Nrow)
                        {
                            CirArrange[iCircuit, iTube_Cir] += AddNumber1;
                        }
                        else if (iTube_Cir >= CircuitInfo.TubeofCir[iCircuit] / Nrow && iTube_Cir < (Nrow - 1) * CircuitInfo.TubeofCir[iCircuit] / Nrow)
                        {
                            CirArrange[iCircuit, iTube_Cir] += AddNumber2;
                        }
                        else
                        {
                            CirArrange[iCircuit, iTube_Cir] += AddNumber3;
                        }
                    }

                    AddNumber1 += res_CirArr1.AddNumber_1;
                    AddNumber2 += res_CirArr1.AddNumber_2;
                    AddNumber3 += res_CirArr1.AddNumber_3;
                }
                else if (CircuitInfo.TubeofCir[iCircuit] % Nrow == 1)
                {
                    res_CirArr1 = CircuitArrange_1(iCircuit, Nrow, CircuitInfo.TubeofCir[iCircuit] / Nrow, N_tube, CirArrange);
                    CirArrange = res_CirArr1.CirArrange;

                    for (int iCircuit0 = iCircuit; iCircuit0 < iCircuit + 3; iCircuit0++)
                        for (int iTube_Cir = 0; iTube_Cir < CircuitInfo.TubeofCir[iCircuit0]; iTube_Cir++)
                        {
                            CirArrange[iCircuit0, iTube_Cir] += AddNumber1;
                        }

                    AddNumber1 += res_CirArr1.AddNumber_1;
                    AddNumber2 += res_CirArr1.AddNumber_2;
                    AddNumber3 += res_CirArr1.AddNumber_3;
                    iCircuit = iCircuit + 2;
                }
                else
                {
                    res_CirArr1 = CircuitArrange_2(iCircuit, Nrow, CircuitInfo.TubeofCir[iCircuit] / Nrow, N_tube, CirArrange);
                    CirArrange = res_CirArr1.CirArrange;

                    for (int iCircuit0 = iCircuit; iCircuit0 < iCircuit + 3; iCircuit0++)
                        for (int iTube_Cir = 0; iTube_Cir < CircuitInfo.TubeofCir[iCircuit0]; iTube_Cir++)
                        {
                            CirArrange[iCircuit0, iTube_Cir] += AddNumber1;
                        }

                    AddNumber1 += res_CirArr1.AddNumber_1;
                    AddNumber2 += res_CirArr1.AddNumber_2;
                    AddNumber3 += res_CirArr1.AddNumber_3;
                    iCircuit = iCircuit + 2;
                }
            }
            return CirArrange;
        }

        public static int[,] GetCirArrange_2Row(int[,] CirArrange, int Nrow, int N_tube, CircuitNumber CircuitInfo)
        {
            int AddNumber = 0;
            int iCircuit1 = 0;
            for (int iCircuit = 0; iCircuit < CircuitInfo.number[0]; iCircuit++)
                for (int iTube_Cir = 0; iTube_Cir < CircuitInfo.TubeofCir[iCircuit]; iTube_Cir++)
                {
                    if (iTube_Cir < CircuitInfo.TubeofCir[iCircuit] / Nrow)
                    {
                        if (iCircuit == 0)
                            CirArrange[iCircuit, iTube_Cir] = iTube_Cir + 1 + N_tube;
                        else
                        {
                            for (iCircuit1 = 1; iCircuit1 <= iCircuit; iCircuit1++)
                            {
                                AddNumber = AddNumber + CircuitInfo.TubeofCir[iCircuit1 - 1] / Nrow;
                            }
                            CirArrange[iCircuit, iTube_Cir] = iTube_Cir + 1 + N_tube + AddNumber;
                            AddNumber = 0;
                        }
                    }
                    else
                    {
                        if (iCircuit == 0)
                            CirArrange[iCircuit, iTube_Cir] = CircuitInfo.TubeofCir[iCircuit] - iTube_Cir;
                        else
                        {
                            for (iCircuit1 = 1; iCircuit1 <= iCircuit; iCircuit1++)
                            {
                                AddNumber = AddNumber + CircuitInfo.TubeofCir[iCircuit1 - 1] / Nrow;
                            }
                            CirArrange[iCircuit, iTube_Cir] = CircuitInfo.TubeofCir[iCircuit] - iTube_Cir + AddNumber;
                            AddNumber = 0;
                        }
                    }
                }
        return CirArrange;
        }
    }
}
