using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Basic;
using System.IO;

namespace Model
{
    public class Slab2
    {
        //Start
        public static CalcResult SlabCalc(int[,] CirArrange, CircuitNumber CircuitInfo, int Nrow, int[] Ntube, int Nelement, string fluid, //double Npass, int[] N_tubes_pass, 
           double l, Geometry geo, double[, ,] ta, double[, ,] RH, double te, double pe, double hri, double mr, double[,] ma, double[,] ha, double[,] haw,
           double eta_surface, double zh, double zdp, int hexType, double thickness, double conductivity, double Pwater, string Airdirection, NodeInfo[] Nodes, int N_Node)
         {
            double tri = te; //Refrigerant.SATP(fluid, composition, pri, 1).Temperature - 273.15;
            double pri = pe;
            double te_calc_org = 0;
            int N_tube = Ntube[0];
            CirArr[] cirArr = new CirArr[Nrow * N_tube];
            CirArrforAir cirArrforAir = new CirArrforAir();
            cirArrforAir = CirArrangement.ReadCirArr(CirArrange, CircuitInfo, Nrow, Ntube);
            cirArr = cirArrforAir.CirArr;            
            CheckAir airConverge = new CheckAir();
            CheckDP dPconverge = new CheckDP();
            CheckPri priconverge = new CheckPri();
            int iterforAir = 0;
            int iterforDP = 0;
            int iterforPri = 0;
            CalcResult r = new CalcResult();
            CalcResult res_slab = new CalcResult();
            double[, ,] taout_calc = new double[Nelement, N_tube, Nrow];
            double[, ,] RHout_calc = new double[Nelement, N_tube, Nrow];
            double[,] Q_detail = new double[N_tube,Nrow];//detail output
            double[,] DP_detail = new double[N_tube, Nrow];
            double[,] Tri_detail = new double[N_tube, Nrow];
            double[,] Pri_detail = new double[N_tube, Nrow];
            double[,] hri_detail = new double[N_tube, Nrow];
            double[,] Tro_detail = new double[N_tube, Nrow];
            double[,] Pro_detail = new double[N_tube, Nrow];
            double[,] hro_detail = new double[N_tube, Nrow];//
            double[,] href_detail = new double[N_tube, Nrow];
            double[,] mr_detail = new double[N_tube, Nrow];
            double[,] charge_detail = new double[N_tube, Nrow];


            if (hexType == 0)// need to be under pri iteration
            {
                tri = CoolProp.PropsSI("T", "P", pri * 1000, "Q", 0, fluid) - 273.15;
            }
            else
            {
                te = CoolProp.PropsSI("T", "P", pri * 1000, "Q", 0, fluid) - 273.15;
            }

            //Node[] Nodes=new Node[N_node];
            //Nodes[0]=First Node;
            int index_Node=0;
            int index_couple=0;
            int index_last_Node=0;
            bool index_end=false;
            int[] index_status=new int[N_Node];
            int[] index_FullStatus=new int[N_Node];
            int[] index_DP=new int[N_Node];//
            double mri_cal = 0;
            double pri_cal = 0;
            double hri_cal = 0;
            double tri_cal = 0;
            int index_cir = 0;
            for(int i=0;i<N_Node;i++)
            {
                index_FullStatus[i]=Math.Max(Nodes[i].N_in,Nodes[i].N_out);
                index_status[i]=0;
            }

            index_Node=SearchNode.FindNextNode(-1,0,Nodes,N_Node);//Find the first node
            index_status[index_Node]=0;//initial status
            //index_FullStatus[index_Node]=Nodes[index_Node].N_out;
            index_DP[index_Node]=0;
            for(int i=0;i<1000;i++)//for DP converge
            {    
                if(Nodes[index_Node].inlet[0]==-1)//First Node(diverse)
                {
                    Nodes[index_Node].mri[0]=mr;
                    Nodes[index_Node].pri[0]=pri;
                    Nodes[index_Node].hri[0]=hri;
                    Nodes[index_Node].tri[0]=tri;
                    //index_cir=Node[index].out[i];//?
                }
                if(Nodes[index_Node].type=='D')//Diverse Node Distribution
                {
                    if(index_DP[index_Node]==0)//first time calculation
                    {
                        Nodes[index_Node].mro[index_status[index_Node]]=Nodes[index_Node].mri[0]/Nodes[index_Node].N_out;
                    }
                    else
                    {            
                        Nodes[index_Node].mro[index_status[index_Node]]=Nodes[index_Node].mri[0]*Nodes[index_Node].mr_ratio[index_status[index_Node]];
                    }        
                    Nodes[index_Node].pro[index_status[index_Node]]=Nodes[index_Node].pri[0];
                    Nodes[index_Node].tro[index_status[index_Node]]=Nodes[index_Node].tri[0];
                    Nodes[index_Node].hro[index_status[index_Node]]=Nodes[index_Node].hri[0];
                    if(Nodes[index_Node].outType[index_status[index_Node]]==0)//0:out is Node,1:out is Circuit
                    {           
                        index_Node=SearchNode.FindNextNode(index_Node,index_status[index_Node],Nodes,N_Node);//find node
                        Nodes[index_Node].pri[0]=Nodes[index_Node-1].pro[index_status[index_Node-1]];
                        Nodes[index_Node].tri[0]=Nodes[index_Node-1].tro[index_status[index_Node-1]];
                        Nodes[index_Node].hri[0]=Nodes[index_Node-1].hro[index_status[index_Node-1]];
                        Nodes[index_Node].mri[0]=Nodes[index_Node-1].mro[index_status[index_Node-1]];
                        index_status[index_Node]=0;
                        //i_end=Nodes[index_Node].N_out;
                        continue;
                    }
                    else//out is Circuit
                    {
                        mri_cal=Nodes[index_Node].mro[index_status[index_Node]];
                        pri_cal=Nodes[index_Node].pro[index_status[index_Node]];
                        tri_cal=Nodes[index_Node].tro[index_status[index_Node]];
                        hri_cal=Nodes[index_Node].hro[index_status[index_Node]];
                        index_cir=Nodes[index_Node].outlet[index_status[index_Node]];
                    }
                }
                else if(Nodes[index_Node].type=='C')//Converge Node
                {
                    mri_cal=Nodes[index_Node].mro[0];
                    pri_cal=Nodes[index_Node].pro[0];
                    hri_cal=Nodes[index_Node].hro[0];
                    tri_cal=Nodes[index_Node].tro[0];
                    index_cir=Nodes[index_Node].outlet[0];
                }    
                //index_status[index_Node]=i;//status
                r = Circuit.CircuitCalc(index_cir, cirArr, CircuitInfo, Nrow, Ntube, Nelement, fluid, l, geo, ta, RH,
                tri_cal, pri_cal, hri_cal, mri_cal, ma, ha, haw, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater, Airdirection);
                if (r.Pro < 0) { res_slab.Pro = -10000000; return res_slab; }
                for(int aa=0;aa<Nrow;aa++)// detail result print
                {
                    for(int bb=0;bb<N_tube;bb++)
                    {
                        Q_detail[bb,aa]=r.Q_detail[bb,aa]==0?Q_detail[bb,aa]:r.Q_detail[bb,aa];
                        DP_detail[bb,aa]=r.DP_detail[bb,aa]==0?DP_detail[bb,aa]:r.DP_detail[bb,aa];
                        Pri_detail[bb,aa]=r.Pri_detail[bb,aa]==0?Pri_detail[bb,aa]:r.Pri_detail[bb,aa];
                        Tri_detail[bb,aa]=r.Tri_detail[bb,aa]==0?Tri_detail[bb,aa]:r.Tri_detail[bb,aa];
                        hri_detail[bb,aa]=r.hri_detail[bb,aa]==0?hri_detail[bb,aa]:r.hri_detail[bb,aa];
                        Pro_detail[bb,aa]=r.Pro_detail[bb,aa]==0?Pro_detail[bb,aa]:r.Pro_detail[bb,aa];
                        Tro_detail[bb,aa]=r.Tro_detail[bb,aa]==0?Tro_detail[bb,aa]:r.Tro_detail[bb,aa];
                        hro_detail[bb,aa]=r.hro_detail[bb,aa]==0?hro_detail[bb,aa]:r.hro_detail[bb,aa];
                        href_detail[bb,aa]=r.href_detail[bb,aa]==0?href_detail[bb,aa]:r.href_detail[bb,aa];
                        mr_detail[bb,aa]=r.mr_detail[bb,aa]==0?mr_detail[bb,aa]:r.mr_detail[bb,aa];
                        charge_detail[bb,aa]=r.charge_detail[bb,aa]==0?charge_detail[bb,aa]:r.charge_detail[bb,aa];
                        for(int cc=0;cc<Nelement;cc++)
                        {
                            taout_calc[cc,bb,aa]=r.Tao_Detail[cc,bb,aa]==0?taout_calc[cc,bb,aa]:r.Tao_Detail[cc,bb,aa];
                            RHout_calc[cc,bb,aa]=r.RHo_Detail[cc,bb,aa]==0?RHout_calc[cc,bb,aa]:r.RHo_Detail[cc,bb,aa];
                        }
                    }
                }
                index_Node=SearchNode.FindNextNode(index_Node,index_status[index_Node],Nodes,N_Node);//Find Next Node
    
                if(Nodes[index_Node].type=='D')//Diverse Node cal
                {
                    index_status[index_Node]=0;
                    //i_end=Nodes[index_Node].N_out;
                    Nodes[index_Node].pri[0]=r.Pro;
                    Nodes[index_Node].tri[0]=r.Tro;
                    Nodes[index_Node].hri[0]=r.hro;
                    Nodes[index_Node].mri[0]=r.mr;
                    continue;        
                }
                else if(Nodes[index_Node].type=='C')//Converge Node cal
                {
                    for(int ii=0;ii<N_Node;ii++)
                    {
                        if(Nodes[ii].couple==Nodes[index_Node].couple&&ii!=index_Node)
                        {
                        index_couple=ii;
                        break;
                        }
                    }//Find the Couple Node        
                    Nodes[index_Node].pri[index_status[index_couple]]=r.Pro;
                    Nodes[index_Node].tri[index_status[index_couple]]=r.Tro;
                    Nodes[index_Node].hri[index_status[index_couple]]=r.hro;
                    Nodes[index_Node].mri[index_status[index_couple]]=r.mr;                   
                    for(int k=0;k<100;k++)
                    {
                        if(index_status[index_couple]<index_FullStatus[index_couple]-1)//continue
                        {
                            index_Node=index_couple; 
                            index_status[index_Node]++;
                            //i_end=Nodes[index_couple].N_out; 
                            break;             
                        }
                        else if(index_status[index_couple]==index_FullStatus[index_couple]-1)//dp converge calculation
                        {
                            //DPconverge=DPConverge(Nodes[index_Node].mri,Nodes[index_Node].pri);
                            dPconverge = CheckDPforCircuits.CheckDPConverge2(hexType, iterforPri, Nodes[index_Node].mri,Nodes[index_Node].pri, index_FullStatus[index_couple]);
                            if(dPconverge.flag==false)//need to be modified
                            {
                                Nodes[index_couple].mro=dPconverge.mr;
                                Nodes[index_couple].mr_ratio=dPconverge.mr_ratio;
                                index_DP[index_couple]++;
                                index_Node=index_couple;
                                index_status[index_Node]=0;
                                //i_end=Nodes[index_Node].N_out;
                                break;   
                            }
                            else if(dPconverge.flag==true)
                            {
                                Nodes[index_couple].mr_ratio=dPconverge.mr_ratio;
                                double mr_sum=0;
                                double tro_ave=0;
                                double hro_ave=0;
                                for(int ii=0;ii<index_FullStatus[index_couple];ii++)
                                {
                                    mr_sum+=Nodes[index_Node].mri[ii];
                                }
                                for(int ii=0;ii<index_FullStatus[index_couple];ii++)
                                {
                                    tro_ave+=Nodes[index_Node].mri[ii]*Nodes[index_Node].tri[ii];
                                    hro_ave+=Nodes[index_Node].mri[ii]*Nodes[index_Node].hri[ii];
                                }
                                Nodes[index_Node].mro[0]=mr_sum;
                                Nodes[index_Node].tro[0]=tro_ave/mr_sum;
                                Nodes[index_Node].hro[0]=hro_ave/mr_sum;
                                Nodes[index_Node].pro[0]=Nodes[index_Node].pri[0];                              
                                if(Nodes[index_Node].outType[0]==0)//0:out is Node
                                {
                                    index_last_Node=index_Node;
                                    index_Node=SearchNode.FindNextNode(index_Node,0,Nodes,N_Node);
                                    if(Nodes[index_Node].type=='C')
                                    {
                                        for(int ii=0;ii<N_Node;ii++)
                                        {
                                            if(Nodes[ii].couple==Nodes[index_Node].couple&&ii!=index_couple)
                                            {
                                                index_couple=ii;
                                                break;
                                            }
                                        }//Find the Couple Node
                                        Nodes[index_Node].pri[index_status[index_couple]]=Nodes[index_last_Node].pro[0];
                                        Nodes[index_Node].tri[index_status[index_couple]]=Nodes[index_last_Node].tro[0];
                                        Nodes[index_Node].hri[index_status[index_couple]]=Nodes[index_last_Node].hro[0];
                                        Nodes[index_Node].mri[index_status[index_couple]]=Nodes[index_last_Node].mro[0];                                                         
                                        continue;    
                                    }
                                    else if(Nodes[index_Node].type=='D')
                                    {
                                        index_status[index_Node]=0;
                                        Nodes[index_Node].mri[0]=Nodes[index_last_Node].mro[0];
                                        Nodes[index_Node].tri[0]=Nodes[index_last_Node].tro[0];
                                        Nodes[index_Node].hri[0]=Nodes[index_last_Node].hro[0];
                                        Nodes[index_Node].pri[0]=Nodes[index_last_Node].pro[0];
                                        break;
                                    }                    
                                }
                                else if(Nodes[index_Node].outType[0]==-1)
                                {
                                    index_end=true;
                                    break;
                                }
                                else//out is Circuit
                                {
                                    break;
                                }
                    //index_DP[index_couple]++;
                    //index_Node++;
                    //if(Nodes[index_Node]==null)
                    //{
                        //Finde the Next Node
                        //Nodes[index_Node]=Next Node;
                    //}
                    //i_start=0;
                    //i_end=Nodes[index_Node].N_out
                                continue;
                            }            
                        }//end if
                    }//end for        
                    if(index_end==true) break;
                }//end if          
            }//end out for


            //airConverge=checkairconverge(ta_out,ta)
            //if(airconverge.flag==false)
            //{
            //index_Air==false
            //ta=airconverge.ta
            //}
            //}(while index_Air==false&& iterforAir<50)

            //if(hexType==0)
            //{
            //priconverge=checkpriconverge(Nodes[index_Node].Pro,pe)
            //if(priconverge.flag==false)
            //{
            //pri=priconverge.pri
            //}   
            //}

            //}while(index_Pri==false&&interforPri<20)    

            //result print



            return res_slab;
        }//end function
    }//end class
    
}//end model
