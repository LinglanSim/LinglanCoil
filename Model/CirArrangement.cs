using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Basic;

namespace Model
{
    public class CirArrangement
    {
        public static CirArr[] ReadCirArr(int[,] CirArrange, CircuitNumber CircuitInfo, int Nrow, int[] Ntube)
        {
            int N_tube = Ntube[0];
            int Nciri = CircuitInfo.number[0];
            int Nciro = CircuitInfo.number[1];
            int Ncir = (Nciri == Nciro ? Nciri : Nciri + Nciro);
            //CirArr cirArr2 = new CirArr() { iRow = 0, iTube = 0 };
            CirArr[] cirArr = new CirArr[Nrow * N_tube];
            //int r = Convert.ToInt32(Math.Ceiling(10.0 / 3));
            //string s = CirArrange.ToString();
            for (int i = 0; i < Nrow * N_tube; i++)
            {
                for (int j = 0; j < Ncir; j++)
                    for (int k = 0; k <CircuitInfo.TubeofCir[j]; k++)
                    {
                        cirArr[i] = new CirArr();
                        cirArr[i].iRow = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(CirArrange[j, k]) / N_tube)) - 1;
                        cirArr[i].iTube = CirArrange[j, k] % N_tube == 0 ? N_tube - 1 : CirArrange[j, k] % N_tube - 1;
                        i++;
                    }

            }


            //string caS1 = ca.Split('/')[0];
            //string caS2 = ca.Split('/')[1];
            //if (caS1.Contains(';'))
            //{
            //    this.CollectionP1S1 = new List<ChannelArrangementPart>();
            //    this.CollectionP2S1 = new List<ChannelArrangementPart>();
            //    string[] temp;
            //    temp = caS1.Split(';')[0].Split(',');
            //    for (int i = 0; i < temp.Length; i++)
            //    {
            //        int channelNumber = int.Parse(temp[i].TrimEnd('H', 'M', 'L', 'X'));
            //        Channel channel = channelH;
            //        if (temp[i].Contains('M')) channel = channelM;
            //        if (temp[i].Contains('L')) channel = channelL;
            //        this.CollectionP1S1.Add(new ChannelArrangementPart { Channel = channel, Number = channelNumber });
            //    }
            //    temp = caS1.Split(';')[1].Split(',');
            //    for (int i = 0; i < temp.Length; i++)
            //    {
            //        int channelNumber = int.Parse(temp[i].TrimEnd('H', 'M', 'L', 'X'));
            //        Channel channel = channelH;
            //        if (temp[i].Contains('M')) channel = channelM;
            //        if (temp[i].Contains('L')) channel = channelL;
            //        this.CollectionP2S1.Add(new ChannelArrangementPart { Channel = channel, Number = channelNumber });
            //    }
            //}

            return cirArr;

        }

        //public int TubeofCircuit(int[,] CirArrange)
        //{
        //    get
        //    {
        //        int total = 0;
        //        if (CirArrange != null)
        //        {
        //            foreach (var seg in CirArrange)
        //            {
        //                total += seg.
        //            }
        //        }

        //        return total;
        //    }
        //}



    }
}
