using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Basic
{
    public class NodeConvert
    {
        public static List<NodeInfo> NodeInputConvert(int[,] CirArrange, int[,,] NodesInput)
        {
            List<NodeInfo> Nodes=new List<NodeInfo>();             
            int Name=0;
            int N_element_in=0;
            int N_element_out=0;
            for (int i = 0; i < NodesInput.GetLength(0);i++ )//Initial convert
            {
                NodeInfo newNode = new NodeInfo();
                newNode.Name = Name;//0:inlet 1:outlet
                N_element_in = 0;
                N_element_out = 0;
                for (int j = 0; j < NodesInput.GetLength(2); j++)
                {
                    if (NodesInput[i, 0, j] != -100)
                    {
                        N_element_in++;
                    }
                    if (NodesInput[i, 1, j] != -100)
                    {
                        N_element_out++;
                    }
                }
                newNode.inlet = new int[N_element_in];
                newNode.inType = new int[N_element_in];
                newNode.outlet = new int[N_element_out];
                newNode.outType = new int[N_element_out];
                newNode.N_in = N_element_in;
                newNode.N_out = N_element_out;
                for (int j = 0; j < N_element_in; j++)
                {
                    newNode.inlet[j] = NodesInput[i, 0, j];
                    if(i==0)
                    {
                        newNode.inType[j] = -1;
                    }
                    else newNode.inType[j] = 1;
                }
                for (int j = 0; j < N_element_out; j++)
                {
                    newNode.outlet[j] = NodesInput[i, 1, j];
                    if(i==1)
                    {
                        newNode.outType[j] = -1;
                    }
                    else newNode.outType[j] = 1;
                }
                Nodes.Add(newNode);
                Name++;
            }
            
            for (int i = 0; i < Nodes.Count;i++ )//split multi-node
            {
                if(Nodes[i].N_in!=1&&Nodes[i].N_out!=1)
                {
                    NodeInfo newNode = new NodeInfo();
                    newNode.Name = Name;
                    newNode.N_in = 1;
                    newNode.inlet = new int[1]{Nodes[i].Name};
                    newNode.inType = new int[1] { 0 };
                    newNode.N_out = Nodes[i].N_out;
                    newNode.outlet = new int[Nodes[i].N_out];
                    newNode.outlet = Nodes[i].outlet;
                    newNode.outType = new int[Nodes[i].N_out];
                    newNode.outType = Nodes[i].outType;
                    Nodes.Add(newNode);
                    Nodes[i].N_out = 1;
                    Nodes[i].outlet = new int[1] { newNode.Name };
                    Nodes[i].outType = new int[1] { 0 };
                    Name++;
                }
            }

            //give node type
            for (int i = 0; i < Nodes.Count;i++ )
            {
                if(Nodes[i].N_in==1&&Nodes[i].N_out>1)
                {
                    Nodes[i].type = 'D';
                }
                else if(Nodes[i].N_in>1&&Nodes[i].N_out==1)
                {
                    Nodes[i].type = 'C';
                }
                if(i==1)//out
                {
                    if (Nodes[i].N_in == 1)
                    {
                        Nodes[i].type = 'E';
                        Nodes[i].couple=-1;
                    }
                }
            }

            //give simple couple 
            int coupleName=1;
            for (int i = 0; i < Nodes.Count;i++ )
            {
                if(Nodes[i].type=='D'&&Nodes[i].couple==0)
                {
                    for(int j=0;j<Nodes.Count;j++)
                    {
                        if(Nodes[j].type=='C'&&ArrayEqual.IsEqual(Nodes[j].inlet,Nodes[i].outlet)&&ArrayEqual.IsEqual(Nodes[j].inType,Nodes[i].outType))
                        {
                            Nodes[i].couple = coupleName;
                            Nodes[j].couple = coupleName;
                            coupleName++;
                        }
                    }
                }                
            }

            //split un-couple node
            bool ConvertDone = true;
            do
            {
                for (int i = 0; i < Nodes.Count; i++)
                {
                    if (Nodes[i].couple == 0)
                    {
                        if (Nodes[i].type == 'D')
                        {
                            NodeandIndex[] ConvergeNodes = new NodeandIndex[Nodes[i].N_out];
                            ConvergeNodes = ConvergeToSame(Nodes, i);
                            bool Same = true;
                            for (int j = 0; j < Nodes[i].N_out; j++)
                            {
                                if (ConvergeNodes[j].Node == -1 || ConvergeNodes[j].Node != ConvergeNodes[0].Node) Same = false;
                            }
                            if (Same == true)
                            {
                                if (Nodes[i].N_out == Nodes[ConvergeNodes[0].Node].N_in)//give couple name
                                {
                                    Nodes[i].couple = coupleName;
                                    Nodes[ConvergeNodes[0].Node].couple = coupleName;
                                    coupleName++;
                                }
                                else if (Nodes[i].N_out < Nodes[ConvergeNodes[0].Node].N_in)//add converge node
                                {
                                    NodeInfo newNode = new NodeInfo();
                                    newNode.inlet = new int[Nodes[i].N_out];
                                    newNode.inType = new int[Nodes[i].N_out];
                                    newNode.N_in = Nodes[i].N_out;
                                    newNode.N_out = 1;
                                    newNode.Name = Name;
                                    Name++;
                                    newNode.outType = new int[1] { 0 };
                                    newNode.type = 'C';
                                    newNode.outlet = new int[1];
                                    newNode.outlet[0] = Nodes[ConvergeNodes[0].Node].Name;
                                    for (int j = 0; j < Nodes[i].N_out; j++)
                                    {
                                        newNode.inlet[j] = Nodes[ConvergeNodes[0].Node].inlet[ConvergeNodes[j].Index];
                                        newNode.inType[j] = Nodes[ConvergeNodes[0].Node].inType[ConvergeNodes[j].Index];
                                    }
                                    Nodes[i].couple = coupleName;//give couple name
                                    newNode.couple = coupleName;
                                    coupleName++;
                                    int[] oldInlet = Nodes[ConvergeNodes[0].Node].inlet;
                                    int[] oldInType = Nodes[ConvergeNodes[0].Node].inType;
                                    for (int j = 0; j < Nodes[i].N_out; j++)
                                    {
                                        oldInlet[ConvergeNodes[j].Index] = -1;
                                        oldInType[ConvergeNodes[j].Index] = -1;
                                    }
                                    Nodes[ConvergeNodes[0].Node].inlet = new int[Nodes[ConvergeNodes[0].Node].N_in - Nodes[i].N_out + 1];
                                    Nodes[ConvergeNodes[0].Node].inType = new int[Nodes[ConvergeNodes[0].Node].N_in - Nodes[i].N_out + 1];
                                    Nodes[ConvergeNodes[0].Node].N_in = Nodes[ConvergeNodes[0].Node].N_in - Nodes[i].N_out + 1;
                                    int k = 0;
                                    for (int j = 0; j < Nodes[ConvergeNodes[0].Node].N_in + Nodes[i].N_out - 1; j++)
                                    {
                                        if (oldInlet[j] != -1)
                                        {
                                            Nodes[ConvergeNodes[0].Node].inlet[k] = oldInlet[j];
                                            Nodes[ConvergeNodes[0].Node].inType[k] = oldInType[j];
                                            k++;
                                        }
                                        if (k == Nodes[ConvergeNodes[0].Node].N_in - 1)
                                        {
                                            Nodes[ConvergeNodes[0].Node].inlet[k] = newNode.Name;
                                            Nodes[ConvergeNodes[0].Node].inType[k] = 0;
                                        }
                                    }
                                    Nodes.Add(newNode);//Add Node
                                }
                            }// end Same
                        }
                        else if (Nodes[i].type == 'C' && Nodes[i].couple == 0)
                        {
                            NodeandIndex[] DiverseNodes = new NodeandIndex[Nodes[i].N_in];
                            DiverseNodes = DiversefromSame(Nodes, i);// problem to be solved
                            bool Same = true;
                            for (int j = 0; j < Nodes[i].N_in; j++)
                            {
                                if (DiverseNodes[j].Node == -1 || DiverseNodes[j].Node != DiverseNodes[0].Node)
                                {
                                    Same = false;
                                    break;
                                }
                            }
                            if (Same)
                            {
                                if (Nodes[DiverseNodes[0].Node].N_out == Nodes[i].N_in)//give couple name
                                {
                                    Nodes[i].couple = coupleName;
                                    Nodes[DiverseNodes[0].Node].couple = coupleName;
                                    coupleName++;
                                }
                                else if (Nodes[DiverseNodes[0].Node].N_out > Nodes[i].N_in)
                                {
                                    NodeInfo newNode = new NodeInfo();
                                    newNode.outlet = new int[Nodes[i].N_in];
                                    newNode.outType = new int[Nodes[i].N_in];
                                    newNode.N_out = Nodes[i].N_in;
                                    newNode.N_in = 1;
                                    newNode.Name = Name;
                                    Name++;
                                    newNode.inType = new int[1] { 0 };
                                    newNode.type = 'D';
                                    newNode.inlet = new int[1];
                                    newNode.inlet[0] = Nodes[DiverseNodes[0].Node].Name;
                                    for (int j = 0; j < Nodes[i].N_in; j++)
                                    {
                                        newNode.outlet[j] = Nodes[DiverseNodes[0].Node].outlet[DiverseNodes[j].Index];
                                        newNode.outType[j] = Nodes[DiverseNodes[0].Node].outType[DiverseNodes[j].Index];
                                    }
                                    Nodes[i].couple = coupleName;//give couple name
                                    newNode.couple = coupleName;
                                    coupleName++;
                                    int[] oldOutlet = Nodes[DiverseNodes[0].Node].outlet;
                                    int[] oldOutType = Nodes[DiverseNodes[0].Node].outType;
                                    for (int j = 0; j < Nodes[i].N_in; j++)
                                    {
                                        oldOutlet[DiverseNodes[j].Index] = -1;
                                        oldOutType[DiverseNodes[j].Index] = -1;
                                    }
                                    Nodes[DiverseNodes[0].Node].outlet = new int[Nodes[DiverseNodes[0].Node].N_out - Nodes[i].N_in + 1];
                                    Nodes[DiverseNodes[0].Node].outType = new int[Nodes[DiverseNodes[0].Node].N_out - Nodes[i].N_in + 1];
                                    Nodes[DiverseNodes[0].Node].N_out = Nodes[DiverseNodes[0].Node].N_out - Nodes[i].N_in + 1;
                                    int k = 0;
                                    for (int j = 0; j < Nodes[DiverseNodes[0].Node].N_out + Nodes[i].N_in - 1; j++)
                                    {
                                        if (oldOutlet[j] != -1)
                                        {
                                            Nodes[DiverseNodes[0].Node].outlet[k] = oldOutlet[j];
                                            Nodes[DiverseNodes[0].Node].outType[k] = oldOutType[j];
                                            k++;
                                        }
                                        if (k == Nodes[DiverseNodes[0].Node].N_out - 1)
                                        {
                                            Nodes[DiverseNodes[0].Node].outlet[k] = newNode.Name;
                                            Nodes[DiverseNodes[0].Node].outType[k] = 0;
                                        }
                                    }
                                    Nodes.Add(newNode);//Add Node                                
                                }
                            }//end Same
                        }
                    }
                }
                ConvertDone = true;
                for (int i = 0; i < Nodes.Count; i++)
                {
                    if (Nodes[i].couple == 0)
                    {
                        ConvertDone = false;
                        break;
                    }
                }
            } while (ConvertDone == false);

            for (int i = 0; i < Nodes.Count; i++)
            {
                Nodes[i].hri = new double[Nodes[i].N_in];
                Nodes[i].hro = new double[Nodes[i].N_out];
                Nodes[i].mri = new double[Nodes[i].N_in];
                Nodes[i].mro = new double[Nodes[i].N_out];
                Nodes[i].mr_ratio = new double[Nodes[i].N_in];
                Nodes[i].pri = new double[Nodes[i].N_in];
                Nodes[i].pro = new double[Nodes[i].N_out];
                Nodes[i].tri = new double[Nodes[i].N_in];
                Nodes[i].tro = new double[Nodes[i].N_out];
            }           
            return Nodes;
        }


        static NodeandIndex[] ConvergeToSame(List<NodeInfo> Nodes, int NodeIndex)
        {
            NodeandIndex[] ConvergeNodes = new NodeandIndex[Nodes[NodeIndex].N_out];
            for (int i = 0; i < Nodes[NodeIndex].N_out;i++ )
            {
                ConvergeNodes[i]=FindConvergeNode(Nodes,NodeIndex,i);
            }
            return ConvergeNodes;
        }
        static NodeandIndex[] DiversefromSame(List<NodeInfo> Nodes, int NodeIndex)
        {
            NodeandIndex[] DiverseNodes = new NodeandIndex[Nodes[NodeIndex].N_in];
            for (int i = 0; i < Nodes[NodeIndex].N_in; i++)
            {
                DiverseNodes[i] = FindDiverseNode(Nodes, NodeIndex, i);
            }
            return DiverseNodes;
        }


        static NodeandIndex FindConvergeNode(List<NodeInfo> Nodes, int NodeIndex,int Index)
        {
            NodeandIndex ConvergeNode = new NodeandIndex();
            if (Nodes[NodeIndex].type != 'D')
            {
                ConvergeNode.Node = -1;
                return ConvergeNode;
            }
            NodeandIndex NextNode = new NodeandIndex();
            int CoupleNode = -1;
            int N=0;
            NextNode = FindNextNode(Nodes, NodeIndex, Index);
            do
            {
                if (Nodes[NextNode.Node].type == 'D')
                {
                    if (Nodes[NextNode.Node].couple == 0)
                    {
                        ConvergeNode.Node = -1;
                        return ConvergeNode;
                    }
                    else if (Nodes[NextNode.Node].couple != 0)
                    {
                        CoupleNode = FindCoupleNode(Nodes, NextNode.Node);
                        NextNode = FindNextNode(Nodes, CoupleNode, 0);
                        N++;
                        continue;
                    }
                }
                else if (Nodes[NextNode.Node].type == 'C')
                {
                    ConvergeNode = NextNode;
                    break;
                }
            } while (N <= Nodes.Count);
            return ConvergeNode;
        }
        static NodeandIndex FindDiverseNode(List<NodeInfo> Nodes, int NodeIndex,int index)
        {
            NodeandIndex DiverseNode = new NodeandIndex();
            if (Nodes[NodeIndex].type != 'C')
            {
                DiverseNode.Node = -1;
                return DiverseNode;
            }
            NodeandIndex PrevNode = new NodeandIndex();
            int CoupleNode = -1;
            int N = 0;
            PrevNode = FindPrevNode(Nodes, NodeIndex, index);
            do
            {
                if (Nodes[PrevNode.Node].type == 'C')
                {
                    if (Nodes[PrevNode.Node].couple == 0)
                    {
                        DiverseNode.Node = -1;
                        return DiverseNode;
                    }
                    else if (Nodes[PrevNode.Node].couple != 0)
                    {
                        CoupleNode = FindCoupleNode(Nodes, PrevNode.Node);
                        PrevNode = FindPrevNode(Nodes, CoupleNode, 0);
                        N++;
                        continue;
                    }
                }
                else if (Nodes[PrevNode.Node].type == 'D')
                {
                    DiverseNode = PrevNode;
                    break;
                }
            } while (N <= Nodes.Count);
            return DiverseNode;
        }


        static NodeandIndex FindNextNode(List<NodeInfo> Nodes, int NodeIndex,int index)
        {
            NodeandIndex NextNode = new NodeandIndex();
            if(Nodes[NodeIndex].outType[index]==0)
            {
                for (int i = 0; i < Nodes.Count; i++)
                {
                    if (Nodes[i].Name == Nodes[NodeIndex].outlet[index])
                    {
                        NextNode.Node = i;
                        for (int j = 0; j < Nodes[i].N_in;j++ )
                        {
                            if (Nodes[i].inType[j] == 0 && Nodes[i].inlet[j] == Nodes[NodeIndex].Name) NextNode.Index = j;
                        }
                        break;
                    }
                }
            }
            else if(Nodes[NodeIndex].outType[index]==1)
            {
                for (int i = 0; i < Nodes.Count; i++)
                {
                    for(int j=0;j<Nodes[i].N_in;j++)
                    {
                        if(Nodes[i].inlet[j]==Nodes[NodeIndex].outlet[index]&&Nodes[i].inType[j]==1)
                        {
                            NextNode.Node = i;
                            NextNode.Index = j;
                            break;
                        }
                    }
                }
            }
            return NextNode;
        }
        static NodeandIndex FindPrevNode(List<NodeInfo> Nodes, int NodeIndex,int index)
        {
            NodeandIndex PrevNode = new NodeandIndex();
            if(Nodes[NodeIndex].inType[index]==0)
            {
                for(int i=0;i<Nodes.Count;i++)
                {
                    if(Nodes[i].Name==Nodes[NodeIndex].inlet[index])
                    {
                        PrevNode.Node = i;
                        PrevNode.Index = 0;
                        break;
                    }
                }
            }
            else if(Nodes[NodeIndex].inType[index]==1)
            {
                for(int i=0;i<Nodes.Count;i++)
                {
                    for(int j=0;j<Nodes[i].N_out;j++)
                    {
                        if(Nodes[i].outlet[j]==Nodes[NodeIndex].inlet[index]&&Nodes[i].outType[j]==1)
                        {
                            PrevNode.Node = i;
                            PrevNode.Index = j;
                            break;
                        }
                    }
                }
            }
            return PrevNode;
        }
        static int FindCoupleNode(List<NodeInfo> Nodes, int NodeIndex)
        {
            int CoupleNode = 0;
            for (int j = 0; j < Nodes.Count; j++)// find the couple node
            {
                if (Nodes[j].couple == Nodes[NodeIndex].couple)
                {
                    CoupleNode = j;
                    break;
                }
            }
            return CoupleNode;
        }

    }
    public class NodeandIndex
    {
        public int Node;
        public int Index;
    }
}
