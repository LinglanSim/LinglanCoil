using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Data;

namespace Model
{
    public class HumidAirProp
    {
        public double H(double Temp, String Input2, double RHi_or_Omega)
        {
            int FirstRow = 2801;//一般已知干球温度和相对湿度
            if (Input2 == "Omega")//有时已知干球温度和含湿量
            {
                FirstRow = 0;//除了求Omega，其他FirstRow都在表头所在行
            }

            double res = -1000;
            Interpolator interpolate = new Interpolator();
            if (Input2 != "Omega")
            {
                double deta_X = 0.1;
                double deta_Y = 0.01;
                int dig_X = 1;
                int dig_Y = 2;
                res = interpolate.Interpolate(FirstRow, Temp, RHi_or_Omega, deta_X, deta_Y, dig_X, dig_Y);//已知干球温度和相对湿度插值
            }
            else
            {
                double deta_X = 0.05;
                double deta_Y = 0.0002;
                int dig_X = 2;
                int dig_Y = 4;
                res = interpolate.Interpolate(FirstRow, Temp, RHi_or_Omega, deta_X, deta_Y, dig_X, dig_Y);//已知干球温度和相对湿度插值
            }
            return res;
        }

        public double O(double Temp, double RHi)
        {
            int FirstRow = 7007;

            //Model.OpenExcel OpenExcel = new Model.OpenExcel();
            //DataRowCollection rowCollection = OpenExcel.ExcelRowCollection(tableCollection);//取得工作表中所有的行

            double res = -1000;
            double deta_X = 0.1;
            double deta_Y = 0.01;
            int dig_X = 1;
            int dig_Y = 2;
            Interpolator interpolate = new Interpolator();
            res = interpolate.Interpolate(FirstRow, Temp, RHi, deta_X, deta_Y, dig_X, dig_Y);//插值

            return res;
        }

        public double Cp(double Temp, double RHi)
        {
            int FirstRow = 4203;

            //Model.OpenExcel OpenExcel = new Model.OpenExcel();
            //DataRowCollection rowCollection = OpenExcel.ExcelRowCollection(tableCollection);//取得工作表中所有的行

            double res = -1000;
            double deta_X = 0.1;
            double deta_Y = 0.01;
            int dig_X = 1;
            int dig_Y = 2;
            Interpolator interpolate = new Interpolator();
            res = interpolate.Interpolate(FirstRow, Temp, RHi, deta_X, deta_Y, dig_X, dig_Y);//插值

            return res * 1000;
        }

        public double Tdp(double Temp, double RHi)
        {
            int FirstRow = 5605;

            //Model.OpenExcel OpenExcel = new Model.OpenExcel();
            //DataRowCollection rowCollection = OpenExcel.ExcelRowCollection(tableCollection);//取得工作表中所有的行

            double res = -1000;
            double deta_X = 0.1;
            double deta_Y = 0.01;
            int dig_X = 1;
            int dig_Y = 2;
            Interpolator interpolate = new Interpolator();
            res = interpolate.Interpolate(FirstRow, Temp, RHi, deta_X, deta_Y, dig_X, dig_Y);//插值

            return res;
        }

        public double Ts(double H)//求Tdp
        {
            int FirstRow = 14017;

            //Model.OpenExcel OpenExcel = new Model.OpenExcel();
            //DataRowCollection rowCollection = OpenExcel.ExcelRowCollection(tableCollection);//取得工作表中所有的行

            double res = -1000;
            double deta_X = 0.1;
            int dig_X = 1;
            Interpolator interpolate = new Interpolator();
            res = interpolate.Interpolate_Ts(FirstRow, H, deta_X, dig_X);//插值

            return res;
        }
        public double RHI(double Temp, double Tdp)
        {
            int FirstRow;//一般已知干球温度和相对湿度
            if ((-40 < Tdp || -40 == Tdp) && (Tdp < -14.8 || Tdp == -14.8))
            {
                FirstRow = 8409;
            }
            else if ((-14.8 < Tdp || -14.8 == Tdp) && (Tdp < 10.4 || Tdp == 10.4))
            {
                FirstRow = 9811;
            }
            else if ((10.4 < Tdp || 10.4 == Tdp) && (Tdp < 35.6 || Tdp == 35.6))
            {
                FirstRow = 11213;
            }
            else// if ((35.6 < Tdp || 35.6 == Tdp) && (Tdp < 60.7 || Tdp == 60.7))
            {
                FirstRow = 12615;
            }
            double res = -1000;
            Interpolator interpolate = new Interpolator();

            double deta_X = 0.1;
            double deta_Y = 0.1;
            int dig_X = 1;
            int dig_Y = 1;
            res = interpolate.Interpolate(FirstRow, Temp, Tdp, deta_X, deta_Y, dig_X, dig_Y);//已知干球温度和相对湿度插值

            //if (res > 1)
            //{
            //    res = 1;
            //}
            return res;
        }

    }
}
