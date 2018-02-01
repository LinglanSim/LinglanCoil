using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Basic
{
    public class CircuitArrange
    {
        public static CalcResult_CirArr CircuitArrange_0(int iCircuit, int Nrow, int Tubeo_Nrow, int N_tube, int[,] CirArrange)
        {
            CalcResult_CirArr res_CirArr = new CalcResult_CirArr();

            int AddNumber_1 = 0;
            int AddNumber_2 = 0;
            int AddNumber_3 = 0;

            for (int iTube_Cir = 0; iTube_Cir < Nrow * Tubeo_Nrow; iTube_Cir++)
            {
                if (iTube_Cir < Tubeo_Nrow)
                {
                    CirArrange[iCircuit, iTube_Cir] = iTube_Cir + 1 + N_tube * (Nrow - 1);
                    AddNumber_1 = AddNumber_1 + 1;
                    
                }
                else if (iTube_Cir >= Tubeo_Nrow && iTube_Cir < (Nrow - 1) * Tubeo_Nrow)
                {
                    CirArrange[iCircuit, iTube_Cir] = 2 * Tubeo_Nrow - iTube_Cir + N_tube * (Nrow - 2);
                    AddNumber_2 += 1;
                }
                else
                {
                    CirArrange[iCircuit, iTube_Cir] = iTube_Cir - 2 * Tubeo_Nrow + 1 + N_tube * (Nrow - 3);
                    AddNumber_3 += 1;
                }
            }

            res_CirArr.AddNumber_1 = AddNumber_1;
            res_CirArr.AddNumber_2 = AddNumber_2;
            res_CirArr.AddNumber_3 = AddNumber_3;
            res_CirArr.CirArrange = CirArrange;
            return res_CirArr;
        }
        public static CalcResult_CirArr CircuitArrange_1(int iCircuit0, int Nrow, int Tubeo_Nrow, int N_tube, int[,] CirArrange)
        {
            CalcResult_CirArr res_CirArr = new CalcResult_CirArr();

            int AddNumber_1 = 0;
            int AddNumber_2 = 0;
            int AddNumber_3 = 0;

            int iCircuit1 = iCircuit0 + 1;
            int iCircuit2 = iCircuit0 + 2;
            
            //第一路
                for (int iTube_Cir = 0; iTube_Cir < Nrow * Tubeo_Nrow + 1; iTube_Cir++)
                {
                    if (iTube_Cir < Tubeo_Nrow)
                    {
                        CirArrange[iCircuit0, iTube_Cir] = iTube_Cir + 1 + N_tube * (Nrow - 1);
                        AddNumber_1 = AddNumber_1 + 1;
                    }
                    else if (iTube_Cir >= Tubeo_Nrow && iTube_Cir < (Nrow - 1) * Tubeo_Nrow)
                    {
                        CirArrange[iCircuit0, iTube_Cir] = 2 * Tubeo_Nrow - iTube_Cir + N_tube * (Nrow - 2);
                        AddNumber_2 += 1;
                    }
                    else
                    {
                        CirArrange[iCircuit0, iTube_Cir] = iTube_Cir - 2 * Tubeo_Nrow + 1 + N_tube * (Nrow - 3);
                        AddNumber_3 += 1;
                    }
                }
            
            //第二路
                for (int iTube_Cir = 0; iTube_Cir < Nrow * Tubeo_Nrow + 1; iTube_Cir++)
                {
                    if (iTube_Cir < Tubeo_Nrow)
                    {
                        CirArrange[iCircuit1, iTube_Cir] = 2 * Tubeo_Nrow - iTube_Cir + N_tube * (Nrow - 1);
                        AddNumber_1 = AddNumber_1 + 1;
                    }
                    else if (iTube_Cir >= Tubeo_Nrow && iTube_Cir < (Nrow - 1) * Tubeo_Nrow + 1)
                    {
                        CirArrange[iCircuit1, iTube_Cir] = iTube_Cir + 1 + N_tube * (Nrow - 2);
                        AddNumber_2 += 1;
                    }
                    else
                    {
                        CirArrange[iCircuit1, iTube_Cir] = 4 * Tubeo_Nrow + 2 - iTube_Cir + N_tube * (Nrow - 3);
                        AddNumber_3 += 1;
                    }
                }
            
            //第三路
                for (int iTube_Cir = 0; iTube_Cir < Nrow * Tubeo_Nrow + 1; iTube_Cir++)
                {
                    if (iTube_Cir < Tubeo_Nrow + 1)
                    {
                        CirArrange[iCircuit2, iTube_Cir] = 2 * Tubeo_Nrow + iTube_Cir + 1 + N_tube * (Nrow - 1);
                        AddNumber_1 = AddNumber_1 + 1;
                    }
                    else if (iTube_Cir >= (Tubeo_Nrow + 1) && iTube_Cir < (Nrow - 1) * Tubeo_Nrow + 1)
                    {
                        CirArrange[iCircuit2, iTube_Cir] = 4 * Tubeo_Nrow + 2 - iTube_Cir + N_tube * (Nrow - 2);
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
        public static CalcResult_CirArr CircuitArrange_2(int iCircuit0, int Nrow, int Tubeo_Nrow, int N_tube, int[,] CirArrange)
        {
            CalcResult_CirArr res_CirArr = new CalcResult_CirArr();

            int AddNumber_1 = 0;
            int AddNumber_2 = 0;
            int AddNumber_3 = 0;

            int iCircuit1 = iCircuit0 + 1;
            int iCircuit2 = iCircuit0 + 2;

            //第一路
            for (int iTube_Cir = 0; iTube_Cir < Nrow * Tubeo_Nrow + 2; iTube_Cir++)
            {
                if (iTube_Cir < Tubeo_Nrow)
                {
                    CirArrange[iCircuit0, iTube_Cir] = Tubeo_Nrow - iTube_Cir + N_tube * (Nrow - 1);
                    AddNumber_1 = AddNumber_1 + 1;
                }
                else if (iTube_Cir >= Tubeo_Nrow && iTube_Cir < ((Nrow - 1) * Tubeo_Nrow + 1))
                {
                    CirArrange[iCircuit0, iTube_Cir] = iTube_Cir - 1 + N_tube * (Nrow - 2);
                    AddNumber_2 += 1;
                }
                else
                {
                    CirArrange[iCircuit0, iTube_Cir] = 3 * Tubeo_Nrow + 2 - iTube_Cir + N_tube * (Nrow - 3);
                    AddNumber_3 += 1;
                }
            }

            //第二路
            for (int iTube_Cir = 0; iTube_Cir < Nrow * Tubeo_Nrow + 2; iTube_Cir++)
            {
                if (iTube_Cir < Tubeo_Nrow + 1)
                {
                    CirArrange[iCircuit1, iTube_Cir] = Tubeo_Nrow + iTube_Cir + 1 + N_tube * (Nrow - 1);
                    AddNumber_1 = AddNumber_1 + 1;
                }
                else if (iTube_Cir >= (Tubeo_Nrow + 1) && iTube_Cir < (Nrow - 1) * Tubeo_Nrow + 1)
                {
                    CirArrange[iCircuit1, iTube_Cir] =  3 * Tubeo_Nrow + 2 - iTube_Cir + N_tube * (Nrow - 2);
                    AddNumber_2 += 1;
                }
                else
                {
                    CirArrange[iCircuit1, iTube_Cir] = iTube_Cir - 1 + N_tube * (Nrow - 3);
                    AddNumber_3 += 1;
                }
            }

            //第三路
            for (int iTube_Cir = 0; iTube_Cir < Nrow * Tubeo_Nrow + 2; iTube_Cir++)
            {
                if (iTube_Cir < Tubeo_Nrow + 1)
                {
                    CirArrange[iCircuit2, iTube_Cir] = 3 * Tubeo_Nrow - iTube_Cir + 2 + N_tube * (Nrow - 1);
                    AddNumber_1 = AddNumber_1 + 1;
                }
                else if (iTube_Cir >= (Tubeo_Nrow + 1) && iTube_Cir < (Nrow - 1) * (Tubeo_Nrow + 1))
                {
                    CirArrange[iCircuit2, iTube_Cir] = Tubeo_Nrow + 1 + iTube_Cir + N_tube * (Nrow - 2);
                    AddNumber_2 += 1;
                }
                else
                {
                    CirArrange[iCircuit2, iTube_Cir] = 5 * Tubeo_Nrow + 4 - iTube_Cir + N_tube * (Nrow - 3);
                    AddNumber_3 += 1;
                }
            }


            res_CirArr.AddNumber_1 = AddNumber_1;
            res_CirArr.AddNumber_2 = AddNumber_2;
            res_CirArr.AddNumber_3 = AddNumber_3;
            res_CirArr.CirArrange = CirArrange;
            return res_CirArr;
        }

    }
}
