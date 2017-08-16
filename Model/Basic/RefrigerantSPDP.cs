using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Basic
{
    public class RefrigerantSPDP
    {
        public static double ff_Friction(double Re)
        {
            double f;
            if (Re > 5000)
                f = 4 * (0.0014 + 0.125 * Math.Pow(Re, -0.32));
            else if (Re < 1668.549825)
                f = 64 / Re;
            else //(1668.549825=<Re) and (Re<=5000)
                f = 0.03835666104;
            return f;
        }

    }

}
