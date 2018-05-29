using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Office.Interop.Excel; 

namespace tryRT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        //输出到window1的定义
        private Object N_row_inter;
        public Object frmPara00
        {
            get { return N_row_inter; }
            set { N_row_inter = value; }
        }

        private Object tube_inter;
        public Object frmPara0
        {
            get { return tube_inter; }
            set { tube_inter = value; }
        }
  
        private Object Q_inter;
        public Object frmPara1
        {
            get { return Q_inter; }
            set { Q_inter = value; }
        }

        private Object DP_inter;
        public Object frmPara2
        {
            get { return DP_inter; }
            set { DP_inter = value; }
        }

        private Object ha_inter;
        public Object frmPara3
        {
            get { return ha_inter; }
            set { ha_inter = value; }
        }

        private Object href_inter;
        public Object frmPara4
        {
            get { return href_inter; }
            set { href_inter = value; }
        }

        private Object Ra_ratio_inter;
        public Object frmPara5
        {
            get { return Ra_ratio_inter; }
            set { Ra_ratio_inter = value; }
        }

        private Object Pro_inter;
        public Object frmPara6
        {
            get { return Pro_inter; }
            set { Pro_inter = value; }
        }

        private Object hro_inter;
        public Object frmPara7
        {
            get { return hro_inter; }
            set { hro_inter = value; }
        }

        private Object Tro_inter;
        public Object frmPara8
        {
            get { return Tro_inter; }
            set { Tro_inter = value; }
        }

        private Object mr_inter;
        public Object frmPara9
        {
            get { return mr_inter; }
            set { mr_inter = value; }
        }

        private Object Tao_inter;
        public Object frmPara10
        {
            get { return Tao_inter; }
            set { Tao_inter = value; }
        }

        private Object RHout_inter;
        public Object frmPara11
        {
            get { return RHout_inter; }
            set { RHout_inter = value; }
        }

        private Object ma_inter;
        public Object frmPara12
        {
            get { return ma_inter; }
            set { ma_inter = value; }
        }

        private Object Tro_detail_inter;
        public Object frmPara13
        {
            get { return Tro_detail_inter; }
            set { Tro_detail_inter = value; }
        }

        public MainWindow()
        {
            InitializeComponent();
            Bind1();
            Bind_hextype();
            Bind_reftype();
            Bind_tubetype();
            Bind_fintype();
            Bind_flowtype();
        }

        public void Bind1()
        {
            IList<string> customList = new List<string>();
            string[] sss = new string[4];
            //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
            sss[0] = "Do=7mm,Row=2,Pt=22mm,Pr=21mm";
            sss[1] = "Do=5mm,Row=2,Pt=14.5mm,Pr=12.56mm";
            sss[2] = "Do=8mm,Row=2,Pt=22mm,Pr=19.05mm";
            sss[3] = "User Defined";
            customList.Add(sss[0]);
            customList.Add(sss[1]);
            customList.Add(sss[2]);
            customList.Add(sss[3]);

            ComboBox1.ItemsSource = customList;
            ComboBox1.SelectedValue = customList[0];
        }

        public void Bind_hextype()
        {
            IList<string> customList = new List<string>();
            string[] sss = new string[2];
            sss[0] = "Evaporator";
            sss[1] = "Condenser";
            customList.Add(sss[0]);
            customList.Add(sss[1]);

            ComboBox_hextype.ItemsSource = customList;
            ComboBox_hextype.SelectedValue = customList[0];
        }

        public void Bind_reftype()
        {
            IList<string> customList = new List<string>();
            string[] sss = new string[5];
            sss[0] = "R22";
            sss[1] = "R410A";
            sss[2] = "R290";
            sss[3] = "R32";
            sss[4] = "R600a";
            customList.Add(sss[0]);
            customList.Add(sss[1]);
            customList.Add(sss[2]);
            customList.Add(sss[3]);
            customList.Add(sss[4]);

            ComboBox3.ItemsSource = customList;
            ComboBox3.SelectedValue = customList[0];
        }

        public void Bind_tubetype()
        {
            IList<string> customList = new List<string>();
            string[] sss = new string[3];
            //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
            sss[0] = "smooth";
            sss[1] = "internalthread";
            sss[2] = "wave";
            customList.Add(sss[0]);
            customList.Add(sss[1]);
            customList.Add(sss[2]);

            ComboBox_tubetype.ItemsSource = customList;
            ComboBox_tubetype.SelectedValue = customList[0];
        }

        public void Bind_fintype()
        {
            IList<string> customList = new List<string>();
            string[] sss = new string[3];
            //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
            sss[0] = "plain";
            sss[1] = "louver";
            sss[2] = "wave";
            customList.Add(sss[0]);
            customList.Add(sss[1]);
            customList.Add(sss[2]);

            ComboBox_fintype.ItemsSource = customList;
            ComboBox_fintype.SelectedValue = customList[0];
        }

        public void Bind_flowtype()
        {
            IList<string> customList = new List<string>();
            string[] sss = new string[3];
            //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
            sss[0] = "Counter";
            sss[1] = "Parallel";
            sss[2] = "交叉流";
            customList.Add(sss[0]);
            customList.Add(sss[1]);
            customList.Add(sss[2]);

            ComboBox_flowtype.ItemsSource = customList;
            ComboBox_flowtype.SelectedValue = customList[0];
        }

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)//关闭按钮
        {
            this.Close();
        }

        private void MenuItem_Click_0(object sender, RoutedEventArgs e)//关闭按钮
        {
            Model.Main m_Main = new Model.Main();
            Model.Basic.GeometryInput geoInput = new Model.Basic.GeometryInput();
            Model.Basic.RefStateInput refInput = new Model.Basic.RefStateInput();
            Model.Basic.AirStateInput airInput = new Model.Basic.AirStateInput();

            //几何结构输入
            geoInput.Pt = Convert.ToDouble(Pt.Text);
            geoInput.Pr = Convert.ToDouble(Pr.Text);
            geoInput.Nrow = Convert.ToInt16(Row.Text);
            geoInput.Do = Convert.ToInt16(Do.Text);
            geoInput.Ntube = Convert.ToInt16(tube_per.Text);
            geoInput.Tthickness = Convert.ToDouble(thick_tube.Text);
            geoInput.L = Convert.ToDouble(L.Text);
            geoInput.Fthickness = Convert.ToDouble(Fthick.Text);
            geoInput.FPI = Convert.ToDouble(Fnum.Text);
            geoInput.CirNum = Convert.ToInt16(Cirnum.Text);
            //double mr = Convert.ToDouble(textBox1.Text);
            refInput.Massflowrate = Convert.ToDouble(mr.Text);
            //double tc = Convert.ToDouble(textBox2.Text);
            refInput.tc = Convert.ToDouble(tc.Text);
            //double tri = Convert.ToDouble(textBox3.Text);
            refInput.tri = Convert.ToDouble(tri.Text);
            //double Va = Convert.ToDouble(textBox4.Text);
            airInput.Volumetricflowrate = Convert.ToDouble(Va.Text);
            //double tai = Convert.ToDouble(textBox5.Text);
            airInput.tai = Convert.ToDouble(tai.Text);
            //double RHi = Convert.ToDouble(textBox6.Text);
            airInput.RHi = Convert.ToDouble(RHi.Text);
            string AirDirection = ComboBox_flowtype.Text;
            //string refri = ComboBox3.Text;
            refInput.FluidName = ComboBox3.Text;
            string fin_type = ComboBox_fintype.Text;
            string tube_type = ComboBox_tubetype.Text;
            string hex_type = ComboBox_hextype.Text;

            //string bb = ComboBox6_SelectionChanged(object sender, SelectionChangedEventArgs e);
            //m_Main.W5(a, b).ha
            var r = m_Main.main_condenser(refInput, airInput, geoInput, AirDirection, fin_type, tube_type, hex_type);

            //***换热器性能输出***//
            Q.Text = Convert.ToString(Convert.ToSingle(r.Q));
            DP_ref.Text = Convert.ToString(Convert.ToSingle(r.DP));
            //***制冷剂侧输出***//
            Pro.Text = Convert.ToString(Convert.ToSingle(r.Pro));
            hro.Text = Convert.ToString(Convert.ToSingle(r.hro));
            Tro.Text = Convert.ToString(Convert.ToSingle(r.Tro));
            //***风侧输出***//
            Tao.Text = Convert.ToString(Convert.ToSingle(r.Tao));
            RHout.Text = Convert.ToString(Convert.ToSingle(r.RHout));
            //***热阻输出***//
            R_1a.Text = Convert.ToString(Convert.ToSingle(r.R_1a));
            R_1r.Text = Convert.ToString(Convert.ToSingle(r.R_1r));
            Ra_ratio.Text = Convert.ToString(Convert.ToSingle(r.Ra_ratio));
            //输出到excel
            Q_inter = r.Q;
            DP_inter = r.DP;
            ha_inter = r.ha;
            href_inter = r.href;
            Ra_ratio_inter = r.Ra_ratio;
            Pro_inter = r.Pro;
            hro_inter = r.hro;
            Tro_inter = r.Tro;
            mr_inter = r.mr;
            Tao_inter = r.Tao;
            RHout_inter = r.RHout;
            ma_inter = r.ma;
            
            //输出到window1
            N_row_inter = r.N_row;
            tube_inter = r.tube_row;
            Tro_detail_inter = r.Tro_detail;
            this.TabControl1.SelectedItem = this.TabControl1.Items[6];
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //hex.Text = ComboBox2.SelectedItem.ToString();
        }

        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBox1.SelectedItem.ToString() == "User Defined")
            {
                Pt.IsEnabled = true;
                Pr.IsEnabled = true;
                Row.IsEnabled = true;
                Do.IsEnabled = true;
            }
            else
            {
                string[] sp = ComboBox1.SelectedItem.ToString().Split(new string[] { "," }, StringSplitOptions.None);

                string sp0 = sp[0].Split(new string[] { "=" }, StringSplitOptions.None).Last();
                Do.Text = sp0.Remove((sp0.Length - 2), 2);
                string sp1 = sp[1].Split(new string[] { "=" }, StringSplitOptions.None).Last();
                Row.Text = sp1;// sp1.Remove((sp1.Length - 2), 2);
                string sp2 = sp[2].Split(new string[] { "=" }, StringSplitOptions.None).Last();
                Pt.Text = sp2.Remove((sp2.Length - 2), 2);
                string sp3 = sp[3].Split(new string[] { "=" }, StringSplitOptions.None).Last();
                Pr.Text = sp3.Remove((sp3.Length - 2), 2);
            }   
        }

        private void ComboBox_SelectionChanged_2(object sender, SelectionChangedEventArgs e)
        {
            //ref_type.Text = ComboBox3.SelectedItem.ToString();
        }

        private void ComboBox_SelectionChanged_3(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ComboBox_fintype_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ComboBox_flowtype_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //string aa = ComboBox6.SelectedItem.ToString();
            //return aa;
        }

        private void sgr_Click(object sender, RoutedEventArgs e)
        {
            GUI.Window1 Q_detail = new GUI.Window1();
            Q_detail.Tube_row = Convert.ToString(tube_inter);
            Q_detail.Row = Convert.ToString(N_row_inter);

            double[,] c = (double[,])Tro_detail_inter;
            Q_detail.getName_Tro = (double[,])Tro_detail_inter;
            //Q_detail.getName_Tro = (string[,])Tro_detail_inter[0, 0];

            string[] arraymax = new string[5];
            string[] arraymin = new string[3];
            string[] arraystr = new string[] { "11", "22", "33", "44" };
            arraymax = arraystr;//变成和arraystr一样
            arraymin = arraystr;//变成和arraystr一样


            
            //a.getName[0]= Convert.ToString(tube);
            //a.getName[1] = Convert.ToString(element);
            //a.getName_Pri = Convert.ToString(Pri1);
            //a.getName = Convert.ToString(Tri1);
            //a.getName[4] = Convert.ToString(Hri1);
            //a.getName[5] = Convert.ToString(Pro1);
            //a.getName[6] = Convert.ToString(Tro1);
            //a.getName[7] = Convert.ToString(Hro1);
            //a.getName[8] = Convert.ToString(HTC1);
            //a.getName[9] = Convert.ToString(Q1);
            //a.getName[10] = Convert.ToString(mr1);

            Q_detail.Show();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //首先模拟建立将要导出的数据，这些数据都存于DataTable中

            System.Data.DataTable dt = new System.Data.DataTable();
            dt.Columns.Add("参数", typeof(string));
            dt.Columns.Add("数值", typeof(string));
            dt.Columns.Add("单位", typeof(string));

            //System.Data.DataRow
            System.Data.DataRow row = dt.NewRow();
            row["参数"] = "参数";
            row["数值"] = "数值";
            row["单位"] = "单位";
            dt.Rows.Add(row);

            //综合仿真结果
            row = dt.NewRow();
            row["参数"] = "换热量";
            row["数值"] = Q_inter;
            row["单位"] = "kJ";
            dt.Rows.Add(row);

            row = dt.NewRow();
            row["参数"] = "制冷剂压降";
            row["数值"] = DP_inter;
            row["单位"] = "kPa";
            dt.Rows.Add(row);

            row = dt.NewRow();
            row["参数"] = "空气换热系数";
            row["数值"] = ha_inter;
            row["单位"] = "W/(m2*K)";
            dt.Rows.Add(row);

            row = dt.NewRow();
            row["参数"] = "制冷剂换热系数";
            row["数值"] = href_inter;
            row["单位"] = "W/(m2*K)";
            dt.Rows.Add(row);

            row = dt.NewRow();
            row["参数"] = "风侧热阻占比";
            row["数值"] = Ra_ratio_inter;
            row["单位"] = "1";
            dt.Rows.Add(row);

            //制冷剂
            row = dt.NewRow();
            row["参数"] = "出口压力";
            row["数值"] = Pro_inter;
            row["单位"] = "kPa";
            dt.Rows.Add(row);

            row = dt.NewRow();
            row["参数"] = "出口焓";
            row["数值"] = hro_inter;
            row["单位"] = "kJ";
            dt.Rows.Add(row);

            row = dt.NewRow();
            row["参数"] = "出口温度";
            row["数值"] = Tro_inter;
            row["单位"] = "C";
            dt.Rows.Add(row);

            row = dt.NewRow();
            row["参数"] = "流量";
            row["数值"] = "数值";
            row["单位"] = "kg/s";
            dt.Rows.Add(row);

            //风
            row = dt.NewRow();
            row["参数"] = "出口温度";
            row["数值"] = Tao_inter;
            row["单位"] = "C";
            dt.Rows.Add(row);

            row = dt.NewRow();
            row["参数"] = "出口湿度";
            row["数值"] = RHout_inter;
            row["单位"] = "1";
            dt.Rows.Add(row);

            row = dt.NewRow();
            row["参数"] = "风量";
            row["数值"] = ma_inter;
            row["单位"] = "m3/s";
            dt.Rows.Add(row);


            //创建Excel

            Microsoft.Office.Interop.Excel.Application excelApp = new Microsoft.Office.Interop.Excel.Application();
            Workbook excelWB = excelApp.Workbooks.Add(System.Type.Missing);    //创建工作簿（WorkBook：即Excel文件主体本身）
            Worksheet excelWS = (Worksheet)excelWB.Worksheets[1];   //创建工作表（即Excel里的子表sheet） 1表示在子表sheet1里进行数据导出

            //excelWS.Cells.NumberFormat = "@";     //  如果数据中存在数字类型 可以让它变文本格式显示
            //将数据导入到工作表的单元格
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    excelWS.Cells[i + 1, j + 1] = dt.Rows[i][j].ToString();   //Excel单元格第一个从索引1开始
                }
            }

            excelWB.SaveAs("D:\\sanjiawan.xlsx");  //将其进行保存到指定的路径
            excelWB.Close();
            excelApp.Quit();  //KillAllExcel(excelApp); 释放可能还没释放的进程
        }

    }
}
