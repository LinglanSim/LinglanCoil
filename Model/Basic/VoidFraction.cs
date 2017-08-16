using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Basic
{
    public class VoidFraction
    {
        public static double[,] BaroczyVoidFraction()
        {
            double [,] matrix = new double[2,2];
            //double[][] k;
            int[,] values = { { 1, 2 }, { 3, 4 }, { 5, 6 } };
            int[] pins;
            pins = new int[] { 9, 3, 7, 2 };
            double[,] k;
             k=new double[,] {{4.99380E-01, -2.51370E-01, 6.09460E-03, 1.45140E-02, -6.35270E-04, -3.64240E-04, 1.65390E-05},
             {-3.99520E-02,	-1.56330E-02,	6.57060E-03,	2.48090E-03,	-1.38600E-04,	-9.38320E-05,	0},
             {1.02120E-02,	7.12500E-03,	1.50140E-04,	-6.92500E-04,	5.31160E-05,	6.05600E-06,	0},
             {2.88210E-03,	1.75470E-03,	-6.13610E-05,	-2.20430E-04,	2.37400E-05,	2.43830E-06,	0},
             {3.62510E-04,	1.48220E-04,	-1.26320E-06,	-2.10280E-05,	3.22020E-06,	4.75490E-08,	0},
             {2.41000E-05,	4.36860E-06,	1.66240E-07,	-7.35850E-07,	1.58950E-07,	-8.16330E-09,	0},
             {6.63340E-07,	0,	0,	0,	0,	0,	0}};
            return k;
        }
    }
}
