using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Data;

namespace Model
{
    class Interpolator
    {
        public double Interpolate(int FirstRow, double FirstColValue, double X, double Y, double deta_X, double deta_Y, int dig_X, int dig_Y)
        {
            double Z1;
            double Z2;
            double Z;
            if ((FirstRow != 8409) && (FirstRow != 11112))
            {
                if (Y < 0)
                {
                    Y = 0;
                }

                if (X < -40)
                {
                    X = -40;
                }
            }

            int rowNum1 = 1 + FirstRow;
            double row = (Math.Round(X, dig_X) - (-40)) / deta_X;//四舍五入获取行位置//0.05是表纵坐标相邻间距
            int rowNum;//行
            if ((0 == X || 0 < X) && X < 1)//要这么做结果才对，原因可能是deta_X是0.1?
            {
                rowNum = (int)row + FirstRow;//行
            }
            else
            {
                rowNum = (int)row + 1 + FirstRow;//行
            }

            int colNum1 = 2;
            double col = (Math.Round(Y, dig_Y) - FirstColValue) / deta_Y;//四舍五入获取行位置//0.0002是表横坐标相邻间距
            //if(Y==1){col = (Math.Round(Y, dig_Y) - 0) / 1;//四舍五入获取行位置//0.0002是表横坐标相邻间距}

            int colNum = (int)col + 2;//列

            if (X != Math.Round(X, dig_X) && Y != Math.Round(Y, dig_Y))
            {
                if (X < Math.Round(X, dig_X))
                {
                    rowNum1 = rowNum - 1;
                    if (Y > Math.Round(Y, dig_Y))
                    {
                        colNum1 = colNum + 1;
                        double X1Y1 = Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum, colNum]);
                        double X2Y1 = Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum1, colNum]);
                        double X1Y2 = Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum, colNum1]);
                        double X2Y2 = Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum1, colNum1]);
                        Z1 = X1Y1 + (X2Y1 - X1Y1) / deta_X * (Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum, 0]) - X);
                        Z2 = X1Y2 + (X2Y2 - X1Y2) / deta_X * (Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum, 0]) - X);
                        //Z = Z1 + (Z2 - Z1) / deta_Y * (Y - Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[FirstRow - 1, colNum]));//0.05是表纵坐标相邻间距
                        Z = Z1 + (Z2 - Z1) / deta_Y * (Y - Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[FirstRow, colNum]));//0.05是表纵坐标相邻间距
                    }
                    else
                    {
                        colNum1 = colNum - 1;
                        double X1Y1 = Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum, colNum]);
                        double X2Y1 = Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum1, colNum]);
                        double X1Y2 = Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum, colNum1]);
                        double X2Y2 = Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum1, colNum1]);
                        Z1 = X1Y1 + (X2Y1 - X1Y1) / deta_X * (Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum, 0]) - X);
                        Z2 = X1Y2 + (X2Y2 - X1Y2) / deta_X * (Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum, 0]) - X);
                        //Z = Z1 + (Z2 - Z1) / deta_Y * (Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[FirstRow - 1, colNum]) - Y);//0.05是表纵坐标相邻间距
                        Z = Z1 + (Z2 - Z1) / deta_Y * (Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[FirstRow, colNum]) - Y);//0.05是表纵坐标相邻间距
                    }

                }
                else
                {
                    rowNum1 = rowNum + 1;
                    if (Y > Math.Round(Y, dig_Y))
                    {
                        colNum1 = colNum + 1;
                        double X1Y1 = Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum, colNum]);
                        double X2Y1 = Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum1, colNum]);
                        double X1Y2 = Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum, colNum1]);
                        double X2Y2 = Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum1, colNum1]);
                        Z1 = X1Y1 + (X2Y1 - X1Y1) / deta_X * (X - Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum, 0]));
                        Z2 = X1Y2 + (X2Y2 - X1Y2) / deta_X * (X - Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum, 0]));
                        //Z = Z1 + (Z2 - Z1) / deta_Y * (Y - Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[FirstRow - 1, colNum]));//0.05是表纵坐标相邻间距
                        Z = Z1 + (Z2 - Z1) / deta_Y * (Y - Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[FirstRow, colNum]));//0.05是表纵坐标相邻间距
                    }
                    else
                    {
                        colNum1 = colNum - 1;
                        double X1Y1 = Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum, colNum]);
                        double X2Y1 = Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum1, colNum]);
                        double X1Y2 = Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum, colNum1]);
                        double X2Y2 = Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum1, colNum1]);
                        Z1 = X1Y1 + (X2Y1 - X1Y1) / deta_X * (X - Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum, 0]));
                        Z2 = X1Y2 + (X2Y2 - X1Y2) / deta_X * (X - Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum, 0]));
                        //Z = Z1 + (Z2 - Z1) / deta_Y * (Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[FirstRow-1, colNum]) - Y);//0.05是表纵坐标相邻间距
                        Z = Z1 + (Z2 - Z1) / deta_Y * (Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[FirstRow, colNum]) - Y);//0.05是表纵坐标相邻间距 
                    }

                }
            }
            else if (X != Math.Round(X, dig_X) && Y == Math.Round(Y, dig_Y))
            {
                if (X < Math.Round(X, dig_X))
                {
                    rowNum1 = rowNum - 1;
                    double X1Y1 = Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum, colNum]);
                    double X2Y1 = Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum1, colNum]);
                    Z1 = X1Y1 + (X2Y1 - X1Y1) / deta_X * (Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum, 0]) - X);//0.05是表纵坐标相邻间距
                }
                else
                {
                    rowNum1 = rowNum + 1;
                    double X1Y1 = Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum, colNum]);
                    double X2Y1 = Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum1, colNum]);
                    Z1 = X1Y1 + (X2Y1 - X1Y1) / deta_X * (X - Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum, 0]));//0.05是表纵坐标相邻间距
                }
                Z = Z1;
            }
            else if (X == Math.Round(X, dig_X) && Y != Math.Round(Y, dig_Y))
            {
                if (Y > Math.Round(Y, dig_Y))
                {
                    colNum1 = colNum + 1;
                    double X1Y1 = Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum, colNum]);
                    double X1Y2 = Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum, colNum1]);
                    //Z1 = X1Y1 + (X1Y2 - X1Y1) / deta_Y * (Y - Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[FirstRow - 1, colNum]));//0.0002是表横坐标相邻间距
                    Z1 = X1Y1 + (X1Y2 - X1Y1) / deta_Y * (Y - Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[FirstRow, colNum]));//0.0002是表横坐标相邻间距
                }
                else
                {
                    colNum1 = colNum - 1;
                    double X1Y1 = Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum, colNum]);
                    double X1Y2 = Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum, colNum1]);
                    //Z1 = X1Y1 + (X1Y2 - X1Y1) / deta_Y * (Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[FirstRow - 1, colNum]) - Y);//0.0002是表横坐标相邻间距
                    Z1 = X1Y1 + (X1Y2 - X1Y1) / deta_Y * (Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[FirstRow, colNum]) - Y);//0.0002是表横坐标相邻间距
                }

                Z = Z1;
            }
            else
            {
                Z = Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum, colNum]);
            }

            return Z;
        }

        public double Interpolate_Ts(int FirstRow, double X, double deta_X, int dig_X)
        {
            double Z1;
            double Z;

            int rowNum1 = 1 + FirstRow;
            double row = (Math.Round(X, dig_X) - (-40)) / deta_X;//四舍五入获取行位置//0.05是表纵坐标相邻间距
            int rowNum;//行
            if ((0 == X || 0 < X) && X < 1)//要这么做结果才对，原因可能是deta_X是0.1?
            {
                rowNum = (int)row + FirstRow;//行
            }
            else
            {
                rowNum = (int)row + 1 + FirstRow;//行
            }

            int colNum = 2;//列

            if (X != Math.Round(X, dig_X))
            {
                if (X < Math.Round(X, dig_X))
                {
                    rowNum1 = rowNum - 1;
                    double X1Y1 = Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum, colNum]);
                    double X2Y1 = Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum1, colNum]);
                    Z1 = X1Y1 + (X2Y1 - X1Y1) / deta_X * (Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum, 0]) - X);//0.05是表纵坐标相邻间距
                }
                else
                {
                    rowNum1 = rowNum + 1;
                    double X1Y1 = Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum, colNum]);
                    double X2Y1 = Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum1, colNum]);
                    Z1 = X1Y1 + (X2Y1 - X1Y1) / deta_X * (X - Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum, 0]));//0.05是表纵坐标相邻间距
                }
                Z = Z1;
            }
            else
            {
                Z = Convert.ToDouble(Model.HumidAirSourceData.SourceTableData[rowNum, colNum]);
            }

            return Z;
        }

    }
}
