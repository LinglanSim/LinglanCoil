using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Basic;
using System.IO;

namespace Model
{
    public class Slab
    {
        public static CalcResult SlabCalc(int[,] CirArrange, CircuitNumber CircuitInfo, int Nrow, int[] Ntube, int Nelement, string[] fluid, double[] composition, //double Npass, int[] N_tubes_pass, 
            double dh, double l, GeometryResult[,] geo, double[, ,] ta,
            double te, double pe, double hri, double mr, double ma, double ha,
            double eta_surface, double zh, double zdp, int hexType, double thickness, double conductivity, double Pwater)
   
        {
           //------->        
           // R2   R1

           // [11   1] <====
           // [12   2] <====
           //          <==== Air
           // [13   3] <====
           // [14   4] <====
           // [15   5]  <====
           // [16   6] <====
           // [17   7] <====
           // [18   8]  <====         
           // [19   9] <====
           // [20  10] <====

           //  Ncir=1, 11in, 20->10 1out


            // [19 - 17 - 15 - 13   11   9   7   5   3   1] <====Air
            // [20 - 18 - 16 - 14   12   10  8   6   4   2] <====Air
            //  Ncir=1, 20in, 20->19 1out

           // CirArrange = new int[,] { { 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 } };
           // Nrow=2
           // Ncir=2
            double wm = Refrigerant.WM(fluid, composition).Wm;
            double tri = te; //Refrigerant.SATP(fluid, composition, pri, 1).Temperature - 273.15;
            double pri = pe;
            int Nciri = CircuitInfo.number[0];
            int Nciro = CircuitInfo.number[1];
            int Ncir = (Nciri == Nciro ? Nciri : Nciri + Nciro);
             
            int N_tube = Ntube[0];
            int N_tube_total = 0;
            CirArr[] cirArr = new CirArr[Nrow * N_tube];
            cirArr = CirArrangement.ReadCirArr(CirArrange, CircuitInfo, Nrow, Ntube);
            CalcResult res_slab = new CalcResult();

            double[] pri_cir = new double[Ncir]; //[element, tube, row]
            double[] hri_cir = new double[Ncir];
            double[] tri_cir = new double[Ncir];
            double[] mr_ciri = new double[Nciri];
            List<double[]> mr_ciri_base = new List<double[]>();
         
            double[] mr_ciro = new double[Nciro];
            int[] Ngroupin = new int[Nciro];
            int index = 0;
            int restartDP_index = 0;
            int N_tube2 = 0;
            int[] index_cir = new int[Ncir];
            CalcResult[] r = new CalcResult[Ncir];
            CalcResult[] r1 = new CalcResult[Ncir];
            CalcResult[] r2 = new CalcResult[Ncir]; //for NinMout only
            CalcResult[] res_cir2 = new CalcResult[Nciro + 1];

            int flag_ciro = 0;
            int Ncir_forDP = 0;
            double[] mr_forDP = new double[Nciri];
            int k;
            double te_calc = 0;

            CheckDP dPconverge=new CheckDP();
            CheckPri priconverge = new CheckPri();
            for (int i = 0; i < Nrow; i++) N_tube_total += Ntube[i];
            for (int i = 0; i < Nciro; i++) mr_ciro[i] = mr / Nciro;

            bool index_outbig;
            if (CircuitInfo.UnequalCir == null || CircuitInfo.UnequalCir[0] > 0)
                index_outbig = false;
            else
                index_outbig = true;

            if (CircuitInfo.UnequalCir != null)
            {
                for (int j = 0; j < Nciro; j++)
                {
                    for (int i = 0; i < Ncir; i++)
                        if (CircuitInfo.UnequalCir[i] == Nciri + 1 + j) Ngroupin[j]++;
                    //for (int i = 0; i < Nciri; i++) mr_ciri[i] = mr_ciro[j] / Ngroupin[j];
                }
            }

            int iterforDP = 0;
            int iterforPri = 0;
            //Starting properties
            do
            {
                r = new CalcResult[Ncir];
                r1 = new CalcResult[Ncir];
                res_cir2 = new CalcResult[Nciro + 1];

                flag_ciro = (index_outbig ? 1 : 0);
                tri = Refrigerant.SATP(fluid, composition, pri, 1).Temperature - 273.15;
                for (int j = 0; j < (flag_ciro == 1 ? (index_outbig ? Nciri + 1 : 1) : Nciro + 1); j++)
                {
                    if (j >= Nciro)
                    {
                        j = j - Nciro; //for Nciro
                        flag_ciro = (index_outbig ? 0 : 1);
                    }
                    if (j == 1 && index_outbig && index == 0)
                    {
                        j = j - 1;
                        flag_ciro = 0;
                    }

                    index = 0;
                    do
                    {
                        k = 0;
                        if(!index_outbig)
                        { 
                            for (int i = 0; i < (flag_ciro == 1 ? Nciro : (Nciri == Nciro ? Ncir : Ncir - Nciro)); i++)
                            {
                                if (flag_ciro == 1)
                                {
                                    pri_cir[i + Ncir - Nciro] = res_cir2[i].Pro;
                                    hri_cir[i + Ncir - Nciro] = res_cir2[i].hro;
                                    tri_cir[i + Ncir - Nciro] = res_cir2[i].Tro;
                                }
                                else
                                {
                                    pri_cir[i] = pri;
                                    hri_cir[i] = hri;
                                    tri_cir[i] = tri;
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < (flag_ciro == 1 ? Nciro : Ncir); i++)
                            {
                                if (flag_ciro == 1)
                                {
                                    pri_cir[i] = pri;
                                    hri_cir[i] = hri;
                                    tri_cir[i] = tri;
                                }
                                else
                                {
                                    if (CircuitInfo.UnequalCir[i] == Nciri + 1 + j)
                                    {
                                        pri_cir[i] = r2[j].Pro;
                                        hri_cir[i] = r2[j].hro;
                                        tri_cir[i] = r2[j].Tro;
                                    }
                                }
                            }
                        }

                        for (int i = 0; i < Ncir; i++)
                        {
                            if (flag_ciro == 1)
                            {
                                if(CircuitInfo.UnequalCir[i]<=0)
                                {
                                    //for (int i = 0; i < Ncir; i++)
                                    r[i] = Circuit.CircuitCalc(i, cirArr, CircuitInfo, Nrow, Ntube, Nelement, fluid, composition, dh, l, geo, ta,
                                        tri_cir[i], pri_cir[i], hri_cir[i], mr_ciro[k], ma, ha, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater);
                                    r1[k] = r[i].ShallowCopy();
                                    r2[k] = r[i].ShallowCopy();
                                    if (!index_outbig) r1[k].DP += res_cir2[k].DP;
                                  
                                    index_cir[k] = i;
                                    k++;
                                    Ncir_forDP = Nciro;
                                    mr_forDP = (double[])mr_ciro.Clone(); // mr_forDP = mr_ciro
                                    if (k == Nciro) break;
                                }
                            }
                            else if (Nciri == Nciro || CircuitInfo.UnequalCir[i] == Nciri + 1 + j)
                            {
                                if (index == 0)
                                {
                                    if (Nciri == Nciro)
                                    {
                                        mr_ciro.CopyTo(mr_ciri, 0);
                                    }
                                    else
                                    {
                                        if (restartDP_index == 1 || !priconverge.flag)
                                            mr_ciri[k] = mr_ciri_base[j][k] * mr_ciro[j] / (mr / Nciro);
                                        else
                                            mr_ciri[k] = mr_ciro[j] / Ngroupin[j];
                                    }
                                }
                                //else mr_ciri_base.CopyTo(mr_ciri[k], 0);

                                //for (int i = 0; i < Ncir; i++)
                                r[i] = Circuit.CircuitCalc(i, cirArr, CircuitInfo, Nrow, Ntube, Nelement, fluid, composition, dh, l, geo, ta, 
                                    tri_cir[i], pri_cir[i], hri_cir[i], mr_ciri[k], ma, ha, eta_surface, zh, zdp, hexType, thickness, conductivity, Pwater);
                                r1[k] = r[i].ShallowCopy();
                                index_cir[k] = i;
                                k++;

                                if (k == (Nciri == Nciro ? Ncir : Ngroupin[j]))
                                {
                                    Ncir_forDP = (Nciri == Nciro ? Ncir : Ngroupin[j]);
                                    mr_forDP = (double[])mr_ciri.Clone();
                                    break;
                                }
                            }
                        }

                        if (index_outbig && flag_ciro == 1) break;

                        index++;
                        //dPconverge = CheckDPforCircuits.CheckDPConverge(mr, mr_ciri, r, Ncir);
                        dPconverge = CheckDPforCircuits.CheckDPConverge(res_cir2, iterforPri, flag_ciro, mr_forDP, r1, Ncir_forDP);
                        if (flag_ciro == 0)
                        {
                            restartDP_index = 0;
                            if (!dPconverge.flag) dPconverge.mr.CopyTo(mr_ciri, 0);//mr_ciri = dPconverge.mr;   
                        }
                        else //(flag_ciro == 1)
                        {
                            if (dPconverge.flag)
                            {
                                restartDP_index = 0;
                            }
                            else
                            {
                                restartDP_index = 1;
                                dPconverge.mr.CopyTo(mr_ciro, 0); //mr_ciro = dPconverge.mr;
                                break;
                            }
                        }
                        iterforDP++;
                        

                        N_tube2 = 0;
                        #region //Result print out
                        if (dPconverge.flag)
                        {
                            if (Nciri == Nciro) te_calc = Refrigerant.SATP(fluid, composition, r[j].Pro, 1).Temperature;
                            else
                            {
                                if (mr_ciri_base.Count < Nciro) mr_ciri_base.Add(mr_forDP); //keep original mr ratio for fast iter

                                j = (flag_ciro == 1 ? j + Nciro : j);
                                res_cir2[j] = new CalcResult();
                                for (int i = 0; i < (flag_ciro == 1 ? Nciro : Ngroupin[j]); i++)
                                {
                                    res_cir2[j].Q += r1[i].Q;
                                    res_cir2[j].M += r1[i].M;
                                    res_cir2[j].hro += (flag_ciro == 1 ? mr_ciro[i] : mr_ciri[i]) * r1[i].hro;
                                    if (fluid[0] == "Water")
                                        res_cir2[j].Tro += (flag_ciro == 1 ? mr_ciro[i] : mr_ciri[i]) * r1[i].Tro;

                                    res_cir2[j].Vel_r = r1[i].Vel_r;
                                    res_cir2[j].href += r1[i].href * CircuitInfo.TubeofCir[index_cir[i]];
                                    res_cir2[j].R_1 += r1[i].R_1 * CircuitInfo.TubeofCir[index_cir[i]];
                                    res_cir2[j].R_1a += r1[i].R_1a * CircuitInfo.TubeofCir[index_cir[i]];
                                    res_cir2[j].R_1r += r1[i].R_1r * CircuitInfo.TubeofCir[index_cir[i]];

                                    N_tube2 += CircuitInfo.TubeofCir[index_cir[i]];

                                }
                                res_cir2[j].DP = r1[(flag_ciro == 1 ? Nciro : Ngroupin[j]) - 1].DP;
                                res_cir2[j].Tao_Detail = ta;
                                res_cir2[j].Pro = r1[(flag_ciro == 1 ? Nciro : Ngroupin[j]) - 1].Pro;
                                res_cir2[j].hro = res_cir2[j].hro / (flag_ciro == 1 ? mr : mr_ciro[j]);

                                res_cir2[j].href = res_cir2[j].href / N_tube2;
                                res_cir2[j].R_1 = res_cir2[j].R_1 / N_tube2;
                                res_cir2[j].R_1a = res_cir2[j].R_1a / N_tube2;
                                res_cir2[j].R_1r = res_cir2[j].R_1r / N_tube2;
                                res_cir2[j].Tri = tri;

                                te_calc = Refrigerant.SATP(fluid, composition, res_cir2[j].Pro, 1).Temperature;
                                if (fluid[0] == "Water")
                                    res_cir2[j].Tro = res_cir2[j].Tro / (flag_ciro == 1 ? mr : mr_ciro[j]);
                                else
                                    res_cir2[j].Tro = Refrigerant.PHFLSH(fluid, composition, res_cir2[j].Pro, (res_cir2[j].hro + 140) * wm).t - 273.15;
                            }

                        }
                        #endregion

                    } while (!dPconverge.flag && iterforDP < 100);

                    if (Nciri == Nciro) break;

                    if (index_outbig && j == Nciro - 1)
                    {
                        for (int i = 0; i < Nciro; i++)
                        {
                            r2[i].DP += res_cir2[i].DP;
                        }                       
                        flag_ciro = 1;
                        Ncir_forDP = Nciro;
                        mr_forDP = (double[])mr_ciro.Clone(); // mr_forDP = mr_ciro
                        dPconverge = CheckDPforCircuits.CheckDPConverge(res_cir2, iterforPri, flag_ciro, mr_forDP, r2, Ncir_forDP);
                        if (!dPconverge.flag)
                        {
                            restartDP_index = 1;
                            dPconverge.mr.CopyTo(mr_ciro, 0); //mr_ciro = dPconverge.mr;
                        }
                        break;
                    }
                }

                using (StreamWriter wr = File.AppendText(@"D:\Work\Simulation\Test\MinNout.txt"))
                {
                    for (int i = 0; i < Ncir; i++)
                    {
                        wr.WriteLine("Q, {0}, DP, {1}, href, {2}, Ra_ratio, {3}, Tao, {4}, Tro, {5}, mr, {6}", r[i].Q, r[i].DP, r[i].href, r[i].Ra_ratio, r[i].Tao, r[i].Tro, r[i].mr);
                    }
                }


                if (restartDP_index == 1) priconverge.flag = false;
                else if (hexType == 0 && (fluid[0] != "Water"))
                {
                    priconverge = CheckPin.CheckPriConverge(te, te_calc - 273.15, pri, pe, r[Ncir-1].Pro); //res_slab.Pro
                    iterforPri++;
                    pri = priconverge.pri;
                    if (priconverge.flag && iterforPri == 1 && iterforDP == 1) priconverge.flag = false; //to avoid not even iterate but converge by chance 
                }
                else
                    priconverge.flag = true;

            } while (!priconverge.flag && iterforPri < 20);


            if (iterforDP >= 100)
            {
                throw new Exception("iter for DPConverge > 100.");
            }
            if (iterforPri >= 20)
            {
                throw new Exception("iter for DPPri > 20.");
            }

            #region //Result print out

            for (int i = 0; i < Ncir; i++)
            {
                res_slab.Q += r[i].Q;
                res_slab.M += r[i].M;
                if (Nciri == Nciro) res_slab.hro += mr_ciri[i] * r[i].hro;
                res_slab.href += r[i].href * CircuitInfo.TubeofCir[i];
                res_slab.R_1 += r[i].R_1 * CircuitInfo.TubeofCir[i];
                res_slab.R_1a += r[i].R_1a * CircuitInfo.TubeofCir[i];
                res_slab.R_1r += r[i].R_1r * CircuitInfo.TubeofCir[i];
            }
            if (Nciri == Nciro)
            {
                res_slab.hro = res_slab.hro / mr;
                res_slab.Pro = r[Ncir - 1].Pro;
                res_slab.Vel_r = r[Ncir - 1].Vel_r;
            }
            else if(!index_outbig)
            { 
                res_slab.hro = res_cir2[Nciro].hro;
                res_slab.Pro = res_cir2[Nciro].Pro;
                res_slab.Vel_r = res_cir2[Nciro].Vel_r;
            }
            else
            {
                res_slab.hro = res_cir2[Nciro - 1].hro;
                res_slab.Pro = res_cir2[Nciro - 1].Pro;
                res_slab.Vel_r = res_cir2[Nciro - 1].Vel_r;
            }
            res_slab.Pri = pri;
            res_slab.Tri = tri;
            res_slab.hri = hri;
            res_slab.mr = mr;
            res_slab.DP = pri - res_slab.Pro;
            res_slab.Tao_Detail = ta;
            res_slab.href = res_slab.href / N_tube_total;
            res_slab.ha = ha;
            res_slab.R_1 = res_slab.R_1 / N_tube_total;
            res_slab.R_1a = res_slab.R_1a / N_tube_total;
            res_slab.R_1r = res_slab.R_1r / N_tube_total;

            te_calc = Refrigerant.SATP(fluid, composition, res_slab.Pro, 1).Temperature;            
            double densityLo = Refrigerant.SATT(fluid, composition, te_calc, 1).DensityL;  //mol/L
            double densityVo = Refrigerant.SATT(fluid, composition, te_calc, 2).DensityV;  //mol/L
            //double wm = Refrigerant.WM(fluid, composition).Wm;
            double hlo = Refrigerant.ENTHAL(fluid, composition, te_calc, densityLo).Enthalpy / wm - (fluid[0] == "Water" ? 0 : 140);
            double hvo = Refrigerant.ENTHAL(fluid, composition, te_calc, densityVo).Enthalpy / wm - (fluid[0] == "Water" ? 0 : 140);
            res_slab.x_o = (res_slab.hro - hlo) / (hvo - hlo);
            double densityLi = Refrigerant.SATT(fluid, composition, tri + 273.15, 1).DensityL;  //mol/L
            double densityVi = Refrigerant.SATT(fluid, composition, tri + 273.15, 2).DensityV;  //mol/L
            //double wm = Refrigerant.WM(fluid, composition).Wm;
            double hli = Refrigerant.ENTHAL(fluid, composition, tri + 273.15, densityLi).Enthalpy / wm - (fluid[0] == "Water" ? 0 : 140);
            double hvi = Refrigerant.ENTHAL(fluid, composition, tri + 273.15, densityVi).Enthalpy / wm - (fluid[0] == "Water" ? 0 : 140);
            res_slab.x_i = (res_slab.hri - hli) / (hvi - hli);
            res_slab.Tro = Refrigerant.PHFLSH(fluid, composition, res_slab.Pro, (res_slab.hro + (fluid[0] == "Water" ? 0 : 140)) * wm).t - 273.15;

            for (int j = 0; j < N_tube; j++)
                for (int i = 0; i < Nelement; i++)
                    res_slab.Tao += res_slab.Tao_Detail[i, j, Nrow];
            res_slab.Tao = res_slab.Tao / N_tube;
            res_slab.Ra_ratio = res_slab.R_1a / res_slab.R_1;
            res_slab.ma = ma;
            res_slab.Va = ma / 1.2 * 3600;
            return res_slab;
            #endregion
        }
        
    }
}
