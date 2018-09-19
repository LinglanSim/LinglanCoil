using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using System.Data.OleDb;
using System.Data;

namespace Model
{
    public class HumidAirSourceData
    {
        public static string fileName = "EESWetAirPropertyXLS.xls";
        public static string CurrentDirectory = Convert.ToString(System.AppDomain.CurrentDomain.BaseDirectory);
        public static string SearchStr = "Data Source="+CurrentDirectory.Remove(CurrentDirectory.Length - 15, 15) + "\\Model\\" + fileName;
        
        //public static string connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=D:\\MCoil\\Model\\Excel\\" + fileName + ";" + ";Extended Properties=\"Excel 8.0;HDR=YES;IMEX=1\""; //只能是XLS格式的EXCEL
        public static string connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;" + SearchStr + ";" + ";Extended Properties=\"Excel 8.0;HDR=YES;IMEX=1\""; //只能是XLS格式的EXCEL
        //创建连接到数据源的对象

        public static double[,] SourceTableData = new double[29914, 253];
        //SourceTableData=Model.HumidAirSourceData.InitializeSourceTableData();

         public static void InitializeSourceTableData()//取得工作表中所有的行
        {
            OleDbConnection connection = new OleDbConnection(connectionString);

            //打开连接
            connection.Open();

            string sql = "select * from [EESWetAirPropertyXLS$]";//这个是一个查询命令

            OleDbDataAdapter adapter = new OleDbDataAdapter(sql, connection);

            DataSet dataSet = new DataSet();//用来存放数据 用来存放DataTable

            adapter.Fill(dataSet);//表示把查询的结果(datatable)放到(填充)dataset里面

            connection.Close();//释放连接资源

            //取得数据
            DataTableCollection tableCollection = dataSet.Tables;//获取当前集合中所有的表格

            System.Data.DataTable table = tableCollection[0];//因为我们只往dataset里面放置了一张表格，所以这里取得索引为0的表格就是我们刚刚查询到的表格

            //取得表格中的数据
            //取得table中所有的行
            DataRowCollection rowCollection = table.Rows;//返回了一个行的集合

            for (int i = 0; i < 29914; i++)
            {
                for (int j = 0; j < 253; j++)
                {
                    if (rowCollection[i][j].GetType().Name!="DBNull")
                    {
                        SourceTableData[i,j] = Convert.ToDouble(rowCollection[i][j]);
                        //Console.WriteLine("rowCollection[{0}][{1}]- type {2}", i, j, rowCollection[i][j].GetType().Name);//查看数据类型
                    }                   
                }
            }
        }
    }
}
