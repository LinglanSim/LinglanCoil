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
        public static int[,] CircuitConvert(ObservableCollection<Node> Nodes, ObservableCollection<Connector> Connectors, ObservableCollection<Capacity> Capacities, ObservableCollection<Rect> Rects)
        {
            int N_tube=Nodes.Count;// Nodes here is tube
            int N_circuit=0;
            for(int i=0;i<Capacities.Count;i++)
            {
                if (Capacities[i].In == true) N_circuit++;
            }
            int[,] CirArrange = new int[N_circuit, N_tube];
            double[] CapLength = new double[N_circuit];
            double[] CapDiameter = new double[N_circuit];

            int m=0;
            int n=0;
            for (int i = 0; i < Capacities.Count; i++)
            {
                if (Capacities[i].In == true)
                {
                    //int j=0;
                    n = 0;
                    CirArrange[m,0]=Convert.ToInt32(Capacities[i].Start.Name);
                    for(int j=0;j<Connectors.Count;j++)
                    {
                        if(Connectors[j].Start.Name==CirArrange[m,n].ToString())
                        {
                            n++;
                            CirArrange[m, n] = Convert.ToInt32(Connectors[j].End.Name);
                        }
                    }
                    m++;
                }
            }
            return CirArrange;
        }
        public static int[,,] NodesConvert(ObservableCollection<Node> Nodes, ObservableCollection<Connector> Connectors, ObservableCollection<Capacity> Capacities, ObservableCollection<Rect> Rects)
        {
            int N_node = Rects.Count;
            int Max = 0;
            int inlet=0;
            int outlet=0;
            for(int i=0;i<N_node;i++)
            {
                inlet=0;
                outlet=0;
                for(int j=0;j<Capacities.Count;j++)
                {
                    if(Capacities[j].End==Rects[i]&&Capacities[j].In==true)
                    {
                        inlet++;
                    }
                    else if(Capacities[j].End==Rects[i]&&Capacities[j].In==false)
                    {
                        outlet++;
                    }
                }                
                Max=Math.Max(Max,Math.Max(inlet,outlet));
            }// max rect element number
            int[,,] NodesInfo=new int[N_node,2,Max];
            for(int i=0;i<N_node;i++)//Initialize
                for(int j=0;j<2;j++)
                    for(int k=0;k<Max;k++)
                    {
                        NodesInfo[i,j,k]=-100;
                    }
            //int[,] NodesOut=new int[N_node,Max];
            int[,] CirArrange=CircuitConvert(Nodes,Connectors,Capacities,Rects);
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
            for(int i=0;i<Rects.Count;i++)
            {                
                if(i==0)
                {
                    NodesInfo[i,0,0]=-1;
                }
                else
                {                   
                    int m=0;
                    for(int j=0;j<Capacities.Count;j++)
                    {
                        if(Capacities[j].End==Rects[i]&&Capacities[j].In==false)
                        {
                            for(int k=0;k<N_circuit;k++)
                            {
                                if(CirArrange[k,TubeofCir[k]-1]==Convert.ToInt32(Capacities[j].Start.Name))
                                {
                                    NodesInfo[i,0,m]=k;
                                }                                    
                            }
                            m++;
                        }
                    }
                }
                if(i==1)
                {
                    NodesInfo[i,1,0]=-1;
                }
                else
                {
                    int m=0;
                    for(int j=0;j<Capacities.Count;j++)
                    {
                        if(Capacities[j].End==Rects[i]&&Capacities[j].In==true)
                        {
                            for(int k=0;k<N_circuit;k++)
                            {
                                if(CirArrange[k,0]==Convert.ToInt32(Capacities[j].Start.Name))
                                {
                                    NodesInfo[i,1,m]=k;
                                }
                            }
                            m++;
                        }
                    }
                }
            }
            return NodesInfo;

        }


    }
}
