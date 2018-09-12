using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Data;


namespace Model
{
    public class OpenExcel
    {
        public static string fileName = "EESWetAirPropertyXLS.xls";
        public string connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + fileName + ";" + ";Extended Properties=\"Excel 8.0;HDR=YES;IMEX=1\""; //只能是XLS格式的EXCEL
        public string sql = "select * from [EESWetAirPropertyXLS$]";//这个是一个查询命令 //目前是查询焓值表

        public DataTableCollection ConnecExcelTable()//取得工作簿
        {
            OleDbConnection connection = new OleDbConnection(connectionString);
            OleDbDataAdapter adapter = new OleDbDataAdapter(sql, connection);
            DataSet dataSet = new DataSet();//用来存放数据 用来存放DataTable
            adapter.Fill(dataSet);//表示把查询的结果(datatable)放到(填充)dataset里面
            connection.Close();//释放连接资源

            //取得工作簿
            DataTableCollection tableCollection = dataSet.Tables;//获取当前集合中所有的表格

            return tableCollection;
        }

        public DataRowCollection ExcelRowCollection(DataTableCollection tableCollection)//取得工作表中所有的行
        {
            //取得工作表
            DataTable table = tableCollection[0];//因为我们只往dataset里面放置了一张表格，所以这里取得索引为0的表格就是第一个表

            //if (SearchProperty == "H" || SearchProperty == "O" || SearchProperty == "Cp" || SearchProperty == "Tdp")
            //{
            //    table = tableCollection[0];//因为我们只往dataset里面放置了一张表格，所以这里取得索引为0的表格就是我们刚刚查询到的表格
            //}
            //else if (SearchProperty == "H1")
            //{
            //    table = tableCollection[1];//因为我们只往dataset里面放置了一张表格，所以这里取得索引为0的表格就是我们刚刚查询到的表格
            //}
            //else if (SearchProperty == "T")
            //{
            //    table = tableCollection[2];//因为我们只往dataset里面放置了一张表格，所以这里取得索引为0的表格就是我们刚刚查询到的表格
            //}

            //取得工作表中所有的行
            DataRowCollection rowCollection = table.Rows;//返回了一个行的集合

            return rowCollection;
        }
    }
}
