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
        public double H(double Temp, String Input2, double RHi_or_Omega, DataTableCollection tableCollection)
        {
            int FirstRow = 2204;//一般已知干球温度和相对湿度
            if (Input2 == "Omega")//有时已知干球温度和含湿量
            {
                FirstRow = 0;
            }

            Model.OpenExcel OpenExcel = new Model.OpenExcel();
            DataRowCollection rowCollection = OpenExcel.ExcelRowCollection(tableCollection);//取得工作表中所有的行

            double res = -1000;
            Interpolator interpolate = new Interpolator();
            if (FirstRow == 2204)
            {
                double deta_X = 0.1;
                double deta_Y = 0.01;
                int dig_X = 1;
                int dig_Y = 2;
                res = interpolate.Interpolate(FirstRow, Temp, RHi_or_Omega, deta_X, deta_Y, dig_X, dig_Y, rowCollection);//已知干球温度和相对湿度插值
            }
            else
            {
                double deta_X = 0.05;
                double deta_Y = 0.0002;
                int dig_X = 2;
                int dig_Y = 4;
                res = interpolate.Interpolate(FirstRow, Temp, RHi_or_Omega, deta_X, deta_Y, dig_X, dig_Y, rowCollection);//已知干球温度和相对湿度插值
            }
            return res;
        }

        public double O(double Temp, double RHi, DataTableCollection tableCollection)
        {
            int FirstRow = 5513;

            Model.OpenExcel OpenExcel = new Model.OpenExcel();
            DataRowCollection rowCollection = OpenExcel.ExcelRowCollection(tableCollection);//取得工作表中所有的行

            double res = -1000;
            double deta_X = 0.1;
            double deta_Y = 0.01;
            int dig_X = 1;
            int dig_Y = 2;
            Interpolator interpolate = new Interpolator();
            res = interpolate.Interpolate(FirstRow, Temp, RHi, deta_X, deta_Y, dig_X, dig_Y, rowCollection);//插值

            return res;
        }

        public double Cp(double Temp, double RHi, DataTableCollection tableCollection)
        {
            int FirstRow = 3307;

            Model.OpenExcel OpenExcel = new Model.OpenExcel();
            DataRowCollection rowCollection = OpenExcel.ExcelRowCollection(tableCollection);//取得工作表中所有的行

            double res = -1000;
            double deta_X = 0.1;
            double deta_Y = 0.01;
            int dig_X = 1;
            int dig_Y = 2;
            Interpolator interpolate = new Interpolator();
            res = interpolate.Interpolate(FirstRow, Temp, RHi, deta_X, deta_Y, dig_X, dig_Y, rowCollection);//插值

            return res;
        }

        public double Tdp(double Temp, double RHi, DataTableCollection tableCollection)
        {
            int FirstRow = 4410;

            Model.OpenExcel OpenExcel = new Model.OpenExcel();
            DataRowCollection rowCollection = OpenExcel.ExcelRowCollection(tableCollection);//取得工作表中所有的行

            double res = -1000;
            double deta_X = 0.1;
            double deta_Y = 0.01;
            int dig_X = 1;
            int dig_Y = 2;
            Interpolator interpolate = new Interpolator();
            res = interpolate.Interpolate(FirstRow, Temp, RHi, deta_X, deta_Y, dig_X, dig_Y, rowCollection);//插值

            return res;
        }

        public double Ts(double H, DataTableCollection tableCollection)
        {
            int FirstRow = 6616;

            Model.OpenExcel OpenExcel = new Model.OpenExcel();
            DataRowCollection rowCollection = OpenExcel.ExcelRowCollection(tableCollection);//取得工作表中所有的行

            double res = -1000;
            double deta_X = 0.1;
            int dig_X = 1;
            Interpolator interpolate = new Interpolator();
            res = interpolate.Interpolate_Ts(FirstRow, H, deta_X, dig_X, rowCollection);//插值

            return res;
        }
    }
}
