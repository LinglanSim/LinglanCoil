using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace tryRT
{
    public class CircuitInput
    {
        
        public static Model.Basic.CapiliaryInput CapillaryConvert(ObservableCollection<Capillary> Capillaries,int RefFlowDierection)
        {
            Model.Basic.CapiliaryInput res = new Model.Basic.CapiliaryInput();
            int N_circuit = 0;
            for (int i = 0; i < Capillaries.Count; i++)
            {
                if (Capillaries[i].In == true) N_circuit++;
            }
            res.d_cap = new double[N_circuit];
            res.lenth_cap = new double[N_circuit];
            if(RefFlowDierection==0)//normal direction
            {
                int j = 0;
                for(int i=0;i<Capillaries.Count;i++)
                {
                    if(Capillaries[i].In==true)
                    {
                        res.d_cap[j] = Capillaries[i].Diameter;
                        res.lenth_cap[j] = Capillaries[i].Length;
                        j++;
                    }
                }
            }
            else if(RefFlowDierection==1)//reverse direction
            {
                int j = 0;
                for (int i = 0; i < Capillaries.Count; i++)
                {
                    if (Capillaries[i].In == false)
                    {
                        res.d_cap[j] = Capillaries[i].Diameter;
                        res.lenth_cap[j] = Capillaries[i].Length;
                        j++;
                    }
                }
            }
            return res;
        }    
        
        public static int[,] CircuitConvert(ObservableCollection<Node> Nodes, ObservableCollection<Connector> Connectors, ObservableCollection<Capillary> Capillaries,int RefFlowDirection)
        {
            int N_tube=Nodes.Count;
            int N_circuit=0;                     
            for(int i=0;i<Capillaries.Count;i++)
            {
                if (Capillaries[i].In == true) N_circuit++;
            }
            int[,] CirArrange = new int[N_circuit, N_tube];
            int m=0;
            int n=0;
            if(RefFlowDirection==0)// normal direction
            {
                for(int i=0;i<Capillaries.Count;i++)
                {
                    if(Capillaries[i].In==true)
                    {
                        n = 0;
                        CirArrange[m, 0] = Convert.ToInt32(Capillaries[i].Start.Name);
                        for(int j=0;j<Connectors.Count;j++)
                        {
                            if(Convert.ToInt32(Connectors[j].Start.Name)==CirArrange[m,n])
                            {
                                n++;
                                CirArrange[m, n] = Convert.ToInt32(Connectors[j].End.Name);
                            }
                        }
                        m++;
                    }
                }
            }
            else if(RefFlowDirection==1)
            {
                m = 0;
                for(int i=0;i<Capillaries.Count;i++)
                {
                    if(Capillaries[i].In==false)
                    {
                        n=0;
                        CirArrange[m, 0] = Convert.ToInt32(Capillaries[i].Start.Name);
                        for(int j=Connectors.Count-1;j>=0;j--)
                        {
                            if(Convert.ToInt32(Connectors[j].End.Name)==CirArrange[m,n])
                            {
                                n++;
                                CirArrange[m, n] = Convert.ToInt32(Connectors[j].Start.Name);
                            }
                        }
                        m++;
                    }
                }
            }
            int Max = 0;
            int N_element = 0;
            for (int i = 0; i < N_circuit; i++)
            {
                N_element = 0;
                for (int j = 0; j < N_tube; j++)
                {
                    if (CirArrange[i, j] != 0) N_element++;
                }
                Max = Math.Max(Max, N_element);
            }
            int[,] res_CirArrange = new int[N_circuit, Max];
            for (int i = 0; i < N_circuit; i++)
            {
                for (int j = 0; j < Max; j++)
                {
                    res_CirArrange[i, j] = CirArrange[i, j];
                }
            }
            return res_CirArrange;
        }

        public static int[,,] NodesConvert(ObservableCollection<Node> Nodes, ObservableCollection<Connector> Connectors, ObservableCollection<Capillary> Capillaries, ObservableCollection<Rect> Rects,int RefFlowDirection)
        {
            int N_rect = Rects.Count;
            int Max = 0;
            int inlet=0;
            int outlet=0;
            for(int i=0;i<N_rect;i++)
            {
                inlet=0;
                outlet=0;
                for(int j=0;j<Capillaries.Count;j++)
                {
                    if(Capillaries[j].End==Rects[i]&&Capillaries[j].In==true)
                    {
                        inlet++;
                    }
                    else if(Capillaries[j].End==Rects[i]&&Capillaries[j].In==false)
                    {
                        outlet++;
                    }
                }                
                Max=Math.Max(Max,Math.Max(inlet,outlet));
            }// max rect element number
            int[,,] NodesInfo=new int[N_rect,2,Max];
            for(int i=0;i<N_rect;i++)//Initialize
                for(int j=0;j<2;j++)
                    for(int k=0;k<Max;k++)
                    {
                        NodesInfo[i,j,k]=-100;
                    }
            int[,] CirArrange=CircuitConvert(Nodes,Connectors,Capillaries,RefFlowDirection);
            int N_circuit=CirArrange.GetLength(0);
            int N_element=CirArrange.GetLength(1);
            int[] TubeofCir=new int[N_circuit];
            for(int i=0;i<N_circuit;i++)
            {
                for(int j=0;j<N_element;j++)
                {
                    if(CirArrange[i,j]!=0) TubeofCir[i]++;
                }
            }
            if(RefFlowDirection==0)//normal direction
            {
                for (int i = 0; i < Rects.Count; i++)
                {
                    if (i == 0)
                    {
                        NodesInfo[i, 0, 0] = -1;
                    }
                    else
                    {
                        int m = 0;
                        for (int j = 0; j < Capillaries.Count; j++)
                        {
                            if (Capillaries[j].End == Rects[i] && Capillaries[j].In == false)
                            {
                                for (int k = 0; k < N_circuit; k++)
                                {
                                    if (CirArrange[k, TubeofCir[k] - 1] == Convert.ToInt32(Capillaries[j].Start.Name))
                                    {
                                        NodesInfo[i, 0, m] = k;
                                    }
                                }
                                m++;
                            }
                        }
                    }
                    if (i == 1)
                    {
                        NodesInfo[i, 1, 0] = -1;
                    }
                    else
                    {
                        int m = 0;
                        for (int j = 0; j < Capillaries.Count; j++)
                        {
                            if (Capillaries[j].End == Rects[i] && Capillaries[j].In == true)
                            {
                                for (int k = 0; k < N_circuit; k++)
                                {
                                    if (CirArrange[k, 0] == Convert.ToInt32(Capillaries[j].Start.Name))
                                    {
                                        NodesInfo[i, 1, m] = k;
                                    }
                                }
                                m++;
                            }
                        }
                    }
                }
            }
            else if(RefFlowDirection==1)//reverse direction
            {
                for (int i = 0; i < Rects.Count; i++)
                {
                    if (i == 1)
                    {
                        NodesInfo[i, 0, 0] = -1;
                    }
                    else
                    {
                        int m = 0;
                        for (int j = 0; j < Capillaries.Count; j++)
                        {
                            if (Capillaries[j].End == Rects[i] && Capillaries[j].In == true)
                            {
                                for (int k = 0; k < N_circuit; k++)
                                {
                                    if (CirArrange[k, TubeofCir[k] - 1] == Convert.ToInt32(Capillaries[j].Start.Name))
                                    {
                                        NodesInfo[i, 0, m] = k;
                                    }
                                }
                                m++;
                            }
                        }
                    }
                    if (i == 0)
                    {
                        NodesInfo[i, 1, 0] = -1;
                    }
                    else
                    {
                        int m = 0;
                        for (int j = 0; j < Capillaries.Count; j++)
                        {
                            if (Capillaries[j].End == Rects[i] && Capillaries[j].In == false)
                            {
                                for (int k = 0; k < N_circuit; k++)
                                {
                                    if (CirArrange[k, 0] == Convert.ToInt32(Capillaries[j].Start.Name))
                                    {
                                        NodesInfo[i, 1, m] = k;
                                    }
                                }
                                m++;
                            }
                        }
                    }
                }
            }
            return NodesInfo;
        }


    }
}
