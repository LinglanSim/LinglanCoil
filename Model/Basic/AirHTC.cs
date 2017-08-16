using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Basic
{
    public class AirHTC
    {
         public static double alpha(double vel, double za, int curve)
        {
            DataEntities me = new DataEntities();
            double ha;
            var data = me.AirCoef.First(a => a.Curve == curve);
            ha = data.creHT.Value * Math.Pow(vel, data.reexpHT.Value);
            ha = ha * za;
            return ha;

        }
    }
}
