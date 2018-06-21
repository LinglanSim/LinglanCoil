using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Basic;

namespace Model
{
    public class GeometryCal
    {
        public static AreaResult Areas_element(double L_element, double FPI, double Do, double Di,
            double Pt, double Pr, double Fthickness)
        {
            AreaResult r = new AreaResult();
            double N_fpm = FPI / 25.4 * 1000;//"Number of fins per meer"

            //Air side calculations
            double Aa_tube = Math.PI * Do * L_element*(1 - N_fpm * Fthickness); //"Air side heat transfer area of  tubes"
            double Aa_fin = 2 * N_fpm * L_element * (Pt * Pr - Math.PI * Math.Pow(Do + 2 * Fthickness, 2.0) / 4); //"Air side heat transfer area of fins"
            r.Aa_tube = Aa_tube;
            r.Aa_fin = Aa_fin;
            r.A_a = Aa_tube + Aa_fin; //"Total air side heat transfer area"

            //{Refrigerant side cacluations}
            r.A_r = Math.PI * Di * L_element;  //"Total refrigerant side heat transfer area"
            r.A_r_cs = Math.PI * Math.Pow(Di, 2.0) / 4; //"Refrigerant side cross sectional area"

            //r.A_hx = L * H; //"Front area of the heat exchanger"
            //r.A_face = r.A_hx - A_MCT_cs - r.A_fin_cs; //"Air side face area"

            r.A_ratio = r.A_a/r.A_r; //"Area ratio of air side to refrigerant"
            return r;

        }

        public static Geometry GeoCal(int Nrow, int N_tube, int Nelement, double L, double[] FPI, double Do, double Di,
            double Pt, double Pr, double Fthickness)
        {
            Geometry geo = new Geometry();
            geo.TotalArea = new AreaResult();
            geo.ElementArea = new AreaResult[N_tube, Nrow];
            //GeometryResult[,] r = new GeometryResult();
            for (int k = 0; k < Nrow; k++)
                for (int j = 0; j < N_tube; j++)
                {
                    geo.ElementArea[j, k] = GeometryCal.Areas_element(L / Nelement, FPI[k], Do, Di, Pt, Pr, Fthickness);
                    //geo_element[i] = Areas.Geometry(L / element, FPI[i], Do, Di, Pt, Pr);

                    geo.TotalArea.Aa_tube += geo.ElementArea[j, k].Aa_tube;
                    geo.TotalArea.Aa_fin += geo.ElementArea[j, k].Aa_fin;
                    geo.TotalArea.A_a += geo.ElementArea[j, k].A_a;
                    geo.TotalArea.A_r += geo.ElementArea[j, k].A_r;
                    geo.TotalArea.A_r_cs += geo.ElementArea[j, k].A_r_cs;
                    //geo.A_ratio += geo_element[j,k].A_ratio;
                }
            geo.TotalArea.Aa_tube *= Nelement;
            geo.TotalArea.Aa_fin *= Nelement;
            geo.TotalArea.A_a *= Nelement;
            geo.TotalArea.A_r *= Nelement;
            geo.TotalArea.A_r_cs *= Nelement;
            geo.TotalArea.A_hx = L * N_tube * Pt;
            geo.TotalArea.A_ratio = geo.TotalArea.A_r / geo.TotalArea.A_a;
            geo.Di = Di;
            geo.Do = Do;
            geo.L = L;
            geo.Pt = Pt;
            geo.Pr = Pr;
            geo.FPI = FPI;
            geo.Fthickness = Fthickness;
            geo.Nrow = Nrow;
            geo.N_tube = N_tube;
            return geo;

        }

    }
}
