using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Basic;

namespace Model
{
    public class Areas
    {
        public static GeometryResult Areas_element(double L_element, double FPI, double Do, double Di,
            double Pt, double Pr, double Fthickness)
        {
            GeometryResult r = new GeometryResult();
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

        public static AreaResult Area(int Nrow, int N_tube, int Nelement, double L, double[] FPI, double Do, double Di,
            double Pt, double Pr, double Fthickness)
        {
            AreaResult geo = new AreaResult();
            geo.total = new GeometryResult();
            geo.element = new GeometryResult[N_tube, Nrow];
            //GeometryResult[,] r = new GeometryResult();
            for (int k = 0; k < Nrow; k++)
                for (int j = 0; j < N_tube; j++)
                {
                    geo.element[j, k] = Areas.Areas_element(L / Nelement, FPI[k], Do, Di, Pt, Pr, Fthickness);
                    //geo_element[i] = Areas.Geometry(L / element, FPI[i], Do, Di, Pt, Pr);

                    geo.total.Aa_tube += geo.element[j, k].Aa_tube;
                    geo.total.Aa_fin += geo.element[j, k].Aa_fin;
                    geo.total.A_a += geo.element[j, k].A_a;
                    geo.total.A_r += geo.element[j, k].A_r;
                    geo.total.A_r_cs += geo.element[j, k].A_r_cs;
                    //geo.A_ratio += geo_element[j,k].A_ratio;
                }
            geo.total.A_hx = L * N_tube * Pt;
            geo.total.A_ratio = geo.total.A_r / geo.total.A_a;
            return geo;

        }

    }
}
