using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Basic
{
    public class RefrigerantSPHTC
    {
        //Friction model, for signle-phase heat transfer coefficient only
        public static double ff_CHURCHILL(double Re)
        {
            double relrough = 0; //0.000010/Dh, Relative roughness of condenser material
            double Term1 = 8 / Re;
            double Term = 2.457 * Math.Log(1 / (Math.Pow(7 / Re, 0.9) + 0.27 * relrough));
            double a = Math.Pow(Term, 16);
            double b = Math.Pow(37530 / Re, 16);
            double Term2 = 1 / Math.Pow(a + b, 3.0 / 2);
            double ff = 8 * Math.Pow(Math.Pow(Term1, 12) + Term2, 1.0 / 12);
            
            return ff;
        }

        public static double NU_GNIELINSKI(double Re, double Pr, double f)
        {
            //For signle-phase heat transfer coefficient calculation
            double Nusselt = 0;
            if (Re<=2000)
            {
                Nusselt = 3.65;
            }
            else if (Re>=3000)
            {
                Nusselt = (f / 8) * (Re - 1000) * Pr / (1 + 12.7 * Math.Pow(f / 8, 0.5) * (Math.Pow(Pr, 2.0 / 3) - 1));
            }
            else //(Re<3000) and (Re>2000)  Linear interpolation
            {
                double Nusselt_3000 = (f / 8) * (3000 - 1000) * Pr / (1 + 12.7 * Math.Pow(f / 8, 0.5) * (Math.Pow(Pr, 2.0 / 3) - 1));
                Nusselt = 3.65 + (Nusselt_3000 - 3.65) * (Re - 2000) / 1000;
            }
            
            return Nusselt;
        }

    }
}
