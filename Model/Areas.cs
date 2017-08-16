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
        public static GeometryResult Geometry(double L_element, double FPI, double Do, double Di,
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


    }
}
