using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Basic
{
    public class SearchNode
    {
        public static int FindNextNode(int in_Node, int index_status, List<NodeInfo> Nodes,int N_Node)
        {
            int r=0;
            if(in_Node==-1)//Find the first node
            {
                for(int i=0;i<N_Node;i++)
                {
                    if (Nodes[i].inlet[0] == -1)
                    {
                        r = i;
                        break;
                    }
                }
            }
            else
            {   
                if(Nodes[in_Node].outType[index_status]==0)//out is node
                {
                    for(int i=0;i<N_Node;i++)
                    {
                        for(int j=0;j<Nodes[i].N_in;j++)
                        {
                            if((Nodes[i].inType[j]==0)&&(i==Nodes[in_Node].outlet[index_status]))//avoid find circuit
                            {
                                r=i;
                                break;
                            }                           
                        }
                        //break;
                    }
                }
                else if(Nodes[in_Node].outType[index_status]==1)//out is circuit
                {
                    for(int i=0;i<N_Node;i++)
                    {
                        for(int j=0;j<Nodes[i].N_in;j++)
                        {
                            if((Nodes[i].inType[j]==1)&&(Nodes[i].inlet[j]==Nodes[in_Node].outlet[index_status]))//avoid find node
                            {
                                r=i;
                                break;
                            }
                        }
                    }
                }                       
            }
            return r;
        }// function
    }//end class
}//end basic

