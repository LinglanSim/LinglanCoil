using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Basic;

namespace Model
{
    public class CheckPin
    {
        public static CheckPri CheckPriConverge(double te, double te_calc, double pri, double pe, double pro)
        {
            CheckPri res = new CheckPri();
            bool flag = false;//means not converge
            double err = 0.02; //to be updated, need to be 0.02 if err for dp is 0.02...

            if (Math.Abs(te_calc - te) <= err)
                flag = true;
            else
                pri += 1 * (pe - pro);
            //if (Math.Abs(te_calc - te) <= err)
            //    flag = true;
            //else
            //    tri += 0.5 * (te - te_calc);

            //pri = Refrigerant.SATT(fluid, composition, tri + 273.15, 1).Pressure;         
            
            return res = new CheckPri { flag = flag, pri = pri };

        }

    }
}

