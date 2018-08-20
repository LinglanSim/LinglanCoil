using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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

        private Object Pro_detail_inter;
        public Object frmPara14
        {
            get { return Pro_detail_inter; }
            set { Pro_detail_inter = value; }
        }

        private Object Q_detail_inter;
        public Object frmPara15
        {
            get { return Q_detail_inter; }
            set { Q_detail_inter = value; }
        }

        private Object href_detail_inter;
        public Object frmPara16
        {
            get { return href_detail_inter; }
            set { href_detail_inter = value; }
        }

        private Object hro_detail_inter;
        public Object frmPara17
        {
            get { return hro_detail_inter; }
            set { hro_detail_inter = value; }
        }

        private Object mr_detail_inter;
        public Object frmPara18
        {
            get { return mr_detail_inter; }
            set { mr_detail_inter = value; }
        }

        private Object Pri_detail_inter;
        public Object frmPara19
        {
            get { return Pri_detail_inter; }
            set { Pri_detail_inter = value; }
        }

        private Object hri_detail_inter;
        public Object frmPara20
        {
            get { return hri_detail_inter; }
            set { hri_detail_inter = value; }
        }

        private Object Tri_detail_inter;
        public Object frmPara21
        {
            get { return Tri_detail_inter; }
            set { Tri_detail_inter = value; }
        }

        //声明用来制作不均匀配风的表格
        List<int[]> list = new List<int[]>();
        ObservableCollection<int[]> showdata = new ObservableCollection<int[]>();

        //是否已经计算
        public bool flag_Calculated;

        ViewModel vm = new ViewModel();
        public MainWindow()
        {
            InitializeComponent();
            
            DataContext = vm;



            Bind1();
            Bind_hextype();
            Bind_reftype();
            Bind_tubetype();
            Bind_fintype();
            Bind_flowtype();

            //不均匀配风表
            GetComboBoxSource();
            GetShowData();

            //初始化按钮取值
            RadioButton_PriTri_Evap.IsChecked = true;
            RadioButton_Tcro_Evap.IsChecked = true;
            RadioButton_mro_Evap.IsChecked = true;
            WetBulbTemperature.IsChecked = true;
            AirVolumnFlowRate.IsChecked = true;
            RadioButton_mro_Cond.IsChecked = true;
            RadioButton_ManualArrange.IsChecked = true;
            CheckBox_UniformWind.IsChecked = true;
            RadioButton_HExType_Condenser.IsChecked = true;
            TreeView_HExType.Focus();

            //数据初始化
            flag_Calculated = false;
        }

        public void Bind1()
        {
            IList<string> customList = new List<string>();
            string[] sss = new string[6];
            //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;

            //sss[0] = "Do=7mm,Row=2,Pt=21mm,Pr=22mm,Fnum=1.2mm";
            //sss[1] = "Do=5mm,Row=2,Pt=14.5mm,Pr=12.56mm,Fnum=1.2mm";
            //sss[2] = "Do=8mm,Row=2,Pt=22mm,Pr=19.05mm,Fnum=1.6mm";
            //sss[3] = "Do=7mm,Row=2,Pt=21mm,Pr=19.4mm,Fnum=1.5mm";
            //sss[4] = "Do=7mm,Row=2,Pt=21mm,Pr=18.19mm,Fnum=1.2mm";
            //sss[5] = "User Defined";

            sss[0] = "7mm,2R,21x22";
            sss[1] = "5mm,2R,14.5x12.56";
            sss[2] = "8mm,2R,22x19.05";
            sss[3] = "7mm,2R,21x19.4";
            sss[4] = "7mm,2R,21x18.19";
            sss[5] = "User Defined";

            customList.Add(sss[0]);
            customList.Add(sss[1]);
            customList.Add(sss[2]);
            customList.Add(sss[3]);
            customList.Add(sss[4]);
            customList.Add(sss[5]);

            ComboBox_TubeVersion.ItemsSource = customList;
            ComboBox_TubeVersion.SelectedValue = customList[0];
        }

        public void Bind_hextype()
        {
            IList<string> customList = new List<string>();
            string[] sss = new string[3];
            sss[0] = "冷凝器";
            sss[1] = "蒸发器";

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

            ComboBox_Refrigerant.ItemsSource = customList;
            ComboBox_Refrigerant.SelectedValue = customList[0];
        }

        public void Bind_tubetype()
        {
            IList<string> customList = new List<string>();
            string[] sss = new string[3];
            //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
            sss[0] = "光管";
            sss[1] = "内螺纹管";
            sss[2] = "波纹管";
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
            sss[0] = "平片";
            sss[1] = "百叶窗片";
            sss[2] = "波纹片";
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
            sss[0] = "逆流";
            sss[1] = "顺流";
            customList.Add(sss[0]);
            customList.Add(sss[1]);

            ComboBox_flowtype.ItemsSource = customList;
            ComboBox_flowtype.SelectedValue = customList[0];
        }

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)//关闭按钮
        {
            this.Close();
        }

        private void MenuItem_Click_0(object sender, RoutedEventArgs e)//关闭按钮
        {
            flag_Calculated = true;

            Model.Main m_Main = new Model.Main();
            Model.Basic.GeometryInput geoInput = new Model.Basic.GeometryInput();
            Model.Basic.RefStateInput refInput = new Model.Basic.RefStateInput();
            Model.Basic.AirStateInput airInput = new Model.Basic.AirStateInput();

            //几何结构输入
            geoInput.Pt = Convert.ToDouble(Pt.Text);
            geoInput.Pr = Convert.ToDouble(Pr.Text);
            geoInput.Nrow = Convert.ToInt16(Row.Text);
            geoInput.Do = Convert.ToDouble(Do.Text);
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
            string flowtype = ComboBox_flowtype.Text;
            //string refri = ComboBox3.Text;
            refInput.FluidName = ComboBox_Refrigerant.Text;
            string fin_type = ComboBox_fintype.Text;
            string tube_type = ComboBox_tubetype.Text;
            string hex_type = ComboBox_hextype.Text;

            //string bb = ComboBox6_SelectionChanged(object sender, SelectionChangedEventArgs e);
            //m_Main.W5(a, b).ha
            Model.Basic.CapiliaryInput capInput = new Model.Basic.CapiliaryInput();
            capInput.d_cap = new double[] { 0.00, 0.00 };//0.006
            capInput.lenth_cap = new double[] { 0.0, 0.0 };//0.5
            var r = m_Main.main_condenser(refInput, airInput, geoInput, flowtype, fin_type, tube_type, hex_type, capInput);

            //***换热器性能输出***//
            Q.Text = r.Q.ToString("f2");
            DP_ref.Text = r.DP.ToString("f2");
            //***制冷剂侧输出***//
            Pro.Text = r.Pro.ToString("f2");
            hro.Text = r.hro.ToString("f2");
            Tro.Text = r.Tro.ToString("f2");
            mro.Text = (1000 * r.mr).ToString("f2");
            //***风侧输出***//
            Tao.Text = r.Tao.ToString("f2");
            RHout.Text = r.RHout.ToString("f2");
            mao.Text = (1000 * r.ma).ToString("f2");
           
            //***热阻输出***//
            ha.Text = r.ha.ToString("f2");
            href.Text = r.href.ToString("f2");
            Ra_ratio.Text = r.Ra_ratio.ToString("f2");
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
            Pro_detail_inter = r.Pro_detail;
            Q_detail_inter = r.Q_detail;
            href_detail_inter = r.href_detail;
            hro_detail_inter = r.hro_detail;
            mr_detail_inter = r.mr_detail;
            Pri_detail_inter = r.Pri_detail;
            hri_detail_inter = r.hri_detail;
            Tri_detail_inter = r.Tri_detail;

            this.TreeView_Result.Focus();
        }


        private void ComboBox_TubeVersion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBox_TubeVersion.SelectedItem.ToString() == "User Defined")
            {
                Pt.IsEnabled = true;
                Pr.IsEnabled = true;
                Row.IsEnabled = true;
                Do.IsEnabled = true;
            }
            else
            {
                string[] sp = ComboBox_TubeVersion.SelectedItem.ToString().Split(new string[] { "," }, StringSplitOptions.None);
                string[] sp_2 = sp[2].Split(new string[] { "x" }, StringSplitOptions.None);
                string[] sp_3 = sp[0].Split(new string[] { "mm" }, StringSplitOptions.None);
                string[] sp_4 = sp[1].Split(new string[] { "R" }, StringSplitOptions.None);

                string sp0 = sp[0].Split(new string[] { "=" }, StringSplitOptions.None).Last();
                Do.Text = sp_3[0];
                string sp1 = sp_4[0];
                Row.Text = sp1;// sp1.Remove((sp1.Length - 2), 2);
                string sp2 = sp_2[0].Split(new string[] { "=" }, StringSplitOptions.None).Last();
                Pt.Text = sp_2[0];
                string sp3 = sp_2[1].Split(new string[] { "=" }, StringSplitOptions.None).Last();
                Pr.Text = sp_2[1];
                //string sp3 = sp[3].Split(new string[] { "=" }, StringSplitOptions.None).Last();
                //Pr.Text = sp3.Remove((sp3.Length - 0), 1);
                //string sp4 = sp[4].Split(new string[] { "=" }, StringSplitOptions.None).Last();
                //Fnum.Text = sp4.Remove((sp4.Length - 2), 2);

                Pt.IsEnabled = false;
                Pr.IsEnabled = false;
                Row.IsEnabled = false;
                Do.IsEnabled = false;

            }   
        }

        private void ComboBox_Refrigerant_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ref_type.Text = ComboBox3.SelectedItem.ToString();
        }

        private void ComboBox_tubetype_SelectionChanged(object sender, SelectionChangedEventArgs e)
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


        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        //TreeView****************************************************************************************************************Start
        private void TreeView_HExType_GotFocus(object sender, RoutedEventArgs e)
        {
            this.TabItem_HExType.IsSelected = true;
            if (RadioButton_HExType_Condenser.IsChecked == true)
            {
                this.TabItem_HExTypr_Picture1.IsSelected = true;
            }
            if (RadioButton_HExType_Evaporator.IsChecked == true)
            {
                this.TabItem_HExTypr_Picture2.IsSelected = true;
            }

            //调显示宽度
            TabControl1.Width = 450;//450
            Canvas_Picture_HEx.Width = 450;//450
            StackPanel_OutPut.Width = 450;

            //调显示高度
            this.Canvas_Picture_HEx.Height = 250;
            this.TabControl_Picuture.Height = 250;
            ListBox_RealTimeInputShow_Condenser.Height = 450;
            ListBox_RealTimeInputShow_Evaporator.Height = 450;
        }

        private void TreeView_Fin_GotFocus(object sender, RoutedEventArgs e)
        {
            this.TabItem_Fin.IsSelected = true;
            this.TabItem_FinType_Picture.IsSelected = true;

            //调显示宽度
            TabControl1.Width = 450;//450
            Canvas_Picture_HEx.Width = 450;//450
            StackPanel_OutPut.Width = 450;

            //调显示高度
            this.Canvas_Picture_HEx.Height = 250;
            this.TabControl_Picuture.Height = 250;
            ListBox_RealTimeInputShow_Condenser.Height = 450;
            ListBox_RealTimeInputShow_Evaporator.Height = 450;
        }

        private void TreeView_Pass_GotFocus(object sender, RoutedEventArgs e)
        {
            this.TabItem_Pass.IsSelected = true;

            if (RadioButton_ManualArrange.IsChecked == true)
            {
                TabItem_Pass_Picture.IsSelected = true;

                //调显示高度
                this.Canvas_Picture_HEx.Height = 450;
                this.TabControl_Picuture.Height = 450;
                ListBox_RealTimeInputShow_Condenser.Height = 350;
                ListBox_RealTimeInputShow_Evaporator.Height = 350;
            }
            else 
            {
                TabItem_Null_Picture.IsSelected = true;

                //调显示高度
                this.Canvas_Picture_HEx.Height = 250;
                this.TabControl_Picuture.Height = 250;
                ListBox_RealTimeInputShow_Condenser.Height = 450;
                ListBox_RealTimeInputShow_Evaporator.Height = 450;
            }


            //调显示宽度
            TabControl1.Width = 450;//450
            Canvas_Picture_HEx.Width = 450;//450
            StackPanel_OutPut.Width = 450;


            //生成手动连流路图
            int _row = Convert.ToInt32(Row.Text);
            int _tube = Convert.ToInt32(tube_per.Text);
            vm.Connectors.Clear();
            vm.Capacities.Clear();
            node1 = null;
            rect = null;
            vm.GenerateNode(_row, _tube);
            vm.Rects[0].X = vm.Nodes[0].X - vm.RowPitch + 5;
            vm.Rects[0].Y = vm.Nodes[0].Y;
            vm.Rects[0].RectHeight = (_tube - 1) * vm.TubePitch + 20;
            vm.Rects[1].X = vm.Nodes[0].X + _row * vm.RowPitch + 5;
            vm.Rects[1].Y = vm.Rects[0].Y - (_row + 1) % 2 * vm.TubePitch / 2;
            vm.Rects[1].RectHeight = vm.Rects[0].RectHeight;
            while (vm.Rects.Count > 2)
            {
                vm.Rects.RemoveAt(2);
            };
        }

        private void TreeView_Ref_GotFocus(object sender, RoutedEventArgs e)
        {
            this.TabItem_Ref.IsSelected = true;
            this.TabItem_Ref_Picture.IsSelected = true;

            //调显示宽度
            TabControl1.Width = 450;//450
            Canvas_Picture_HEx.Width = 450;//450
            StackPanel_OutPut.Width = 450;

            //调显示高度
            this.Canvas_Picture_HEx.Height = 250;
            this.TabControl_Picuture.Height = 250;
            ListBox_RealTimeInputShow_Condenser.Height = 450;
            ListBox_RealTimeInputShow_Evaporator.Height = 450;
        }

        private void TreeView_Wind_GotFocus(object sender, RoutedEventArgs e)
        {
            this.TabItem_Wind.IsSelected = true;
            this.TabItem_Wind_Picture.IsSelected = true;

            //调显示宽度
            TabControl1.Width = 450;//450
            Canvas_Picture_HEx.Width = 450;//450
            StackPanel_OutPut.Width = 450;

            //调显示高度
            this.Canvas_Picture_HEx.Height = 250;
            this.TabControl_Picuture.Height = 250;
            ListBox_RealTimeInputShow_Condenser.Height = 450;
            ListBox_RealTimeInputShow_Evaporator.Height = 450;


            if (RadioButton_HExType_Condenser.IsChecked==true)
            {
                this.tai.Text = "35";
                this.Tai_wet.Text = "24";
            }
            else if (RadioButton_HExType_Evaporator.IsChecked == true)
            {
                this.tai.Text = "27";
                this.Tai_wet.Text = "19";
            }
        }

        private void TreeView_Result_GotFocus(object sender, RoutedEventArgs e)
        {
            this.TabItem_Result.IsSelected = true;
            this.TabItem_Null_Picture.IsSelected = true;

            //调显示宽度
            TabControl1.Width = 450;//450
            Canvas_Picture_HEx.Width = 450;//450
            StackPanel_OutPut.Width = 450;

            //调显示高度
            this.Canvas_Picture_HEx.Height = 250;
            this.TabControl_Picuture.Height = 250;
            ListBox_RealTimeInputShow_Condenser.Height = 450;
            ListBox_RealTimeInputShow_Evaporator.Height = 450;
        }

        private void TreeView_DetailResult_GotFocus(object sender, RoutedEventArgs e)
        {
            this.TabItem_DetailResult.IsSelected = true;
            this.TabItem_Null_Picture.IsSelected = true;

            //调显示宽度
            //TabControl1.Width = 600;//450
            StackPanel_OutPut.Width=550;
            this.TabControl1.Width = 550;

            //调显示高度
            this.Canvas_Picture_HEx.Height = 250;
            this.TabControl_Picuture.Height = 250;
            ListBox_RealTimeInputShow_Condenser.Height = 450;
            ListBox_RealTimeInputShow_Evaporator.Height = 450;

            if (flag_Calculated == true)
            {
                //创建dataGrid数据
                Result_Tube_row = Convert.ToString(tube_inter);
                Result_Row = Convert.ToString(N_row_inter);

                getName_Tro = (double[,])Tro_detail_inter;
                getName_Pro = (double[,])Pro_detail_inter;
                getName_Q = (double[,])Q_detail_inter;
                getName_HTC = (double[,])href_detail_inter;
                getName_Hro = (double[,])hro_detail_inter;
                getName_mr = (double[,])mr_detail_inter;
                getName_Pri = (double[,])Pri_detail_inter;
                getName_Hri = (double[,])hri_detail_inter;
                getName_Tri = (double[,])Tri_detail_inter;

                //准备显示
                int Row_int = Convert.ToInt16(Result_Row);
                int Tube_row_int = Convert.ToInt16(Result_Tube_row);

                for (int i = 0; i < Tube_row_int; i++)
                    for (int j = 0; j < Row_int; j++)
                    {
                        peopleList.Add(new people()
                        {
                            tube = Convert.ToString(i + 1),
                            row = Convert.ToString(j + 1),
                            Pri = getName_Pri[i, j].ToString("f2"),
                            Tri = getName_Tri[i, j].ToString("f2"),
                            Hri = getName_Hri[i, j].ToString("f2"),
                            Pro = getName_Pro[i, j].ToString("f2"),
                            Tro = getName_Tro[i, j].ToString("f2"),
                            Hro = getName_Hro[i, j].ToString("f2"),
                            HTC = getName_HTC[i, j].ToString("f2"),
                            Q = (1000 * getName_Q[i, j]).ToString("f2"),
                            mr = (1000 * getName_mr[i, j]).ToString("f2"),
                        });
                    }

                ((this.FindName("dataGrid_Result")) as DataGrid).ItemsSource = peopleList;
                flag_Calculated = false;
            }

        }


        private void TreeView_AdjustParameter_GotFocus(object sender, RoutedEventArgs e)
        {
            this.TabItem_AdjustParameter.IsSelected = true;
            this.TabItem_Null_Picture.IsSelected = true;

            //调显示宽度
            TabControl1.Width = 450;//450
            Canvas_Picture_HEx.Width = 450;//450
            StackPanel_OutPut.Width = 450;

            //调显示高度
            this.Canvas_Picture_HEx.Height = 250;
            this.TabControl_Picuture.Height = 250;
            ListBox_RealTimeInputShow_Condenser.Height = 450;
            ListBox_RealTimeInputShow_Evaporator.Height = 450;
        }

        private void TreeView_Distributer_GotFocus(object sender, RoutedEventArgs e)
        {
            this.TabItem_Distributer.IsSelected = true;
            this.TabItem_Null_Picture.IsSelected = true;

            //调显示宽度
            TabControl1.Width = 450;//450
            Canvas_Picture_HEx.Width = 450;//450
            StackPanel_OutPut.Width = 450;

            //调显示高度
            this.Canvas_Picture_HEx.Height = 250;
            this.TabControl_Picuture.Height = 250;
            ListBox_RealTimeInputShow_Condenser.Height = 450;
            ListBox_RealTimeInputShow_Evaporator.Height = 450;
        }

        private void TreeView_HExTube_GotFocus(object sender, RoutedEventArgs e)
        {
            this.TabItem_HExTube.IsSelected = true;
            this.TabItem_HExTube_Picture.IsSelected = true;

            //调显示宽度
            TabControl1.Width = 450;//450
            Canvas_Picture_HEx.Width = 450;//450
            StackPanel_OutPut.Width = 450;

            //调显示高度
            this.Canvas_Picture_HEx.Height = 250;
            this.TabControl_Picuture.Height = 250;
            ListBox_RealTimeInputShow_Condenser.Height = 450;
            ListBox_RealTimeInputShow_Evaporator.Height = 450;
        }

        //TreeView****************************************************************************************************************End



        //CheckBox****************************************************************************************************************Start

        private void TubeArrangement_Straight_Checked(object sender, RoutedEventArgs e)
        {
            this.TabItem_TubeArrangement_Straight.IsSelected = true;
        }

        private void TubeArrangement_Cross_High_Checked(object sender, RoutedEventArgs e)
        {
            this.TabItem_TubeArrangement_Crossed_High.IsSelected = true;
        }

        private void TubeArrangement_Cross_Short_Checked(object sender, RoutedEventArgs e)
        {
            this.TabItem_TubeArrangement_Crossed_Short.IsSelected = true;
        }

        private void RadioButton_WetBulbTemperature_Checked(object sender, RoutedEventArgs e)
        {
            this.Tai_wet.IsEnabled = true;
            if (this.RHi.IsEnabled == true)
            {
                this.RHi.IsEnabled = false;
                this.RHi.Text = "0.5";
            }
        }

        private void RadioButton_RelativeHumidity_Checked(object sender, RoutedEventArgs e)
        {
            this.RHi.IsEnabled = true;
            if (this.Tai_wet.IsEnabled == true)
            {
                this.Tai_wet.IsEnabled = false;
                if (RadioButton_HExType_Condenser.IsChecked == true)
                {
                    this.Tai_wet.Text = "24";
                }
                else if (RadioButton_HExType_Evaporator.IsChecked == true)
                {
                    this.Tai_wet.Text = "19";
                }
                
            }
        }

        private void RadioButton_AirVolumnFlowRate_Checked(object sender, RoutedEventArgs e)
        {
            this.Va.IsEnabled = true;
            if (this.q_wind.IsEnabled == true)
            {
                this.q_wind.IsEnabled = false;
                this.q_wind.Text = "1";
            }
        }

        private void RadioButton_AirVelocity_Checked(object sender, RoutedEventArgs e)
        {
            this.q_wind.IsEnabled = true;
            if (this.Va.IsEnabled == true)
            {
                this.Va.IsEnabled = false;
                this.Va.Text = "0.2";
            }
        }

        private void RadioButton_Pri_Evap_Checked(object sender, RoutedEventArgs e)
        {
            this.Pri_Evap.IsEnabled = true;
            if (this.Tcro_Evap.IsEnabled == true)
            {
                this.Tcro_Evap.IsEnabled = false;
                this.Tcro_Evap.Text = "0";
            }
        }

        private void RadioButton_Tcro_Evap_Checked(object sender, RoutedEventArgs e)
        {
            this.Tcro_Evap.IsEnabled = true;
            if (this.Pri_Evap.IsEnabled == true)
            {
                this.Pri_Evap.IsEnabled = false;
                this.Pri_Evap.Text = "0";
            }
        }

        private void RadioButton_xi_Evap_Checked(object sender, RoutedEventArgs e)
        {
            this.xi_Evap.IsEnabled = true;
            if (this.Hri_Evap.IsEnabled == true || this.Tri_ValveBefore.IsEnabled == true || this.Pri_ValveBefore.IsEnabled == true)
            {
                this.Hri_Evap.IsEnabled = false;
                this.Hri_Evap.Text = "0";

                this.Tri_ValveBefore.IsEnabled = false;
                this.Tri_ValveBefore.Text = "0";

                this.Pri_ValveBefore.IsEnabled = false;
                this.Pri_ValveBefore.Text = "0";
            }
        }

        private void RadioButton_Hri_Evap_Checked(object sender, RoutedEventArgs e)
        {
            this.Hri_Evap.IsEnabled = true;
            if (this.xi_Evap.IsEnabled == true || this.Tri_ValveBefore.IsEnabled == true || this.Pri_ValveBefore.IsEnabled == true)
            {
                this.xi_Evap.IsEnabled = false;
                this.xi_Evap.Text = "0";

                this.Tri_ValveBefore.IsEnabled = false;
                this.Tri_ValveBefore.Text = "0";

                this.Pri_ValveBefore.IsEnabled = false;
                this.Pri_ValveBefore.Text = "0";
            }
        }

        private void RadioButton_PriTri_Evap_Checked(object sender, RoutedEventArgs e)
        {
            this.Tri_ValveBefore.IsEnabled = true;
            this.Pri_ValveBefore.IsEnabled = true;
            if (this.Hri_Evap.IsEnabled == true || this.xi_Evap.IsEnabled == true)
            {
                this.Hri_Evap.IsEnabled = false;
                this.Hri_Evap.Text = "0";

                this.xi_Evap.IsEnabled = false;
                this.xi_Evap.Text = "0";
            }
        }

        private void RadioButton_Troex_Evap_Checked(object sender, RoutedEventArgs e)
        {
            this.Troex_Evap.IsEnabled = true;
            if (this.xo_Evap.IsEnabled == true)
            {
                this.xo_Evap.IsEnabled = false;
                this.xo_Evap.Text = "0";
            }
            if (this.mro_Evap.IsEnabled == true)
            {
                this.mro_Evap.IsEnabled = false;
                this.mro_Evap.Text = "0";
            }
        }

        private void RadioButton_xo_Evap_Checked(object sender, RoutedEventArgs e)
        {
            this.xo_Evap.IsEnabled = true;
            if (this.Troex_Evap.IsEnabled == true)
            {
                this.Troex_Evap.IsEnabled = false;
                this.Troex_Evap.Text = "0";
            }
            if (this.mro_Evap.IsEnabled == true)
             {
                this.mro_Evap.IsEnabled = false;
                this.mro_Evap.Text = "0";
            }
        }

        private void RadioButton_mro_Evap_Checked(object sender, RoutedEventArgs e)
        {
            this.mro_Evap.IsEnabled = true;
            if (this.Troex_Evap.IsEnabled == true)
            {
                this.Troex_Evap.IsEnabled = false;
                this.Troex_Evap.Text = "0";
            }
            if (this.xo_Evap.IsEnabled == true)
            {
                this.xo_Evap.IsEnabled = false;
                this.xo_Evap.Text = "0";
            }
        }

        private void RadioButton_mro_Cond_Checked(object sender, RoutedEventArgs e)
        {
            this.mr.IsEnabled = true;
            if (this.x_Cond.IsEnabled == true)
            {
                this.x_Cond.IsEnabled = false;
                this.x_Cond.Text = "0";
            }
            if (this.Tro_ex.IsEnabled == true)
            {
                this.Tro_ex.IsEnabled = false;
                this.Tro_ex.Text = "0";
            }
        }

        private void RadioButton_xo_Cond_Checked(object sender, RoutedEventArgs e)
        {
            this.x_Cond.IsEnabled = true;
            if (this.mr.IsEnabled == true)
            {
                this.mr.IsEnabled = false;
                this.mr.Text = "0.01";
            }
            if (this.Tro_ex.IsEnabled == true)
            {
                this.Tro_ex.IsEnabled = false;
                this.Tro_ex.Text = "0";
            }
        }

        private void RadioButton_Tro_Cond_Checked(object sender, RoutedEventArgs e)
        {
            this.Tro_ex.IsEnabled = true;
            if (this.mr.IsEnabled == true)
            {
                this.mr.IsEnabled = false;
                this.mr.Text = "0.01";
            }
            if (this.x_Cond.IsEnabled == true)
            {
                this.x_Cond.IsEnabled = false;
                this.x_Cond.Text = "0";
            }
        }

        private void RadioButton_ManualArrange_Checked(object sender, RoutedEventArgs e)
        {
            this.GroupBox_AutoArrangeCirnum.Visibility = Visibility.Hidden;
            this.GroupBox_ManualArrangeCirnum.Visibility = Visibility.Visible;
            this.TabItem_Pass_Picture.IsSelected = true;

            //调显示高度
            this.Canvas_Picture_HEx.Height = 450;
            this.TabControl_Picuture.Height = 450;
            

        }

        private void RadioButton_AutoArrange_Checked(object sender, RoutedEventArgs e)
        {
            this.GroupBox_AutoArrangeCirnum.Visibility = Visibility.Visible;
            this.GroupBox_ManualArrangeCirnum.Visibility = Visibility.Hidden;
            this.TabItem_Null_Picture.IsSelected = true;

            //调显示高度
            this.Canvas_Picture_HEx.Height = 250;
            this.TabControl_Picuture.Height = 250;
            ListBox_RealTimeInputShow_Condenser.Height = 450;
            ListBox_RealTimeInputShow_Evaporator.Height = 450;
        }

        //CheckBox*********************************************************************************************************End

        private void HExType_Condenser(object sender, RoutedEventArgs e)
        {
            ComboBox_hextype.Text = "冷凝器";
            this.GroupBox_RefInlet_Evaporator.Visibility = Visibility.Hidden;
            this.GroupBox_RefOutlet_Evaporator.Visibility = Visibility.Hidden;
            this.GroupBox_RefInlet_Condenser.Visibility = Visibility.Visible;
            this.GroupBox_RefOutlet_Condenser.Visibility = Visibility.Visible;
            this.GroupBox_RealTimeInputShow_Condenser.Visibility = Visibility.Visible;
            this.GroupBox_RealTimeInputShow_Evaporator.Visibility = Visibility.Hidden;
            this.TabItem_HExType.IsSelected = true;

            this.TabItem_HExTypr_Picture1.IsSelected = true;
        }

        private void HExType_Evaporator(object sender, RoutedEventArgs e)
        {
            ComboBox_hextype.Text = "蒸发器";
            this.GroupBox_RefInlet_Evaporator.Visibility = Visibility.Visible;
            this.GroupBox_RefOutlet_Evaporator.Visibility = Visibility.Visible;
            this.GroupBox_RefInlet_Condenser.Visibility = Visibility.Hidden;
            this.GroupBox_RefOutlet_Condenser.Visibility = Visibility.Hidden;
            this.GroupBox_RealTimeInputShow_Condenser.Visibility = Visibility.Hidden;
            this.GroupBox_RealTimeInputShow_Evaporator.Visibility = Visibility.Visible;
            this.TabItem_HExType.IsSelected = true;

            this.TabItem_HExTypr_Picture2.IsSelected = true;
        }


        //非均匀风速分布Start*****************************************************************************************************************************Start
        private void CheckBox_UniformWind_Checked(object sender, RoutedEventArgs e)
        {
            GroupBox_AsymmetricalWind.Visibility = Visibility.Hidden;
        }

        private void CheckBox_UniformWind_Unchecked(object sender, RoutedEventArgs e)
        {
            GroupBox_AsymmetricalWind.Visibility = Visibility.Visible;
        }

        //---原始数据源到显示数据源
        private void GetShowData()
        {
            showdata.Clear();
            foreach (var a in list)
            {
                showdata.Add(a);
            }
            dtgShow.ItemsSource = showdata;
        }

        //---显示数据到原始数据
        private void GetRawData()
        {
            list.Clear();
            foreach (var a in showdata)
            {
                list.Add(a);
            }
        }
        //---添加 ComboBox 数据源
        private void GetComboBoxSource()
        {
            cbbSelectMode.Items.Add(DataGridSelectionUnit.Cell);
            cbbSelectMode.SelectedIndex = 0;
        }
        //---DataGrid 选择方式
        private void cbbSelectMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dtgShow.SelectionUnit = (DataGridSelectionUnit)cbbSelectMode.SelectedValue;
        }

        //---自动添加行号
        private void dtgShow_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        //---Enter 达到 Tab 的效果
        private void dtgShow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var uie = e.OriginalSource as UIElement;
            if (e.Key == Key.Enter)
            {
                uie.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                e.Handled = true;
            }
        }

        //---取得选中 Cell 所在的行列
        private bool GetCellXY(DataGrid dg, ref int rowIndex, ref int columnIndex)
        {
            var _cells = dg.SelectedCells;
            if (_cells.Any())
            {
                rowIndex = dg.Items.IndexOf(_cells.First().Item);
                columnIndex = _cells.First().Column.DisplayIndex;
                return true;
            }
            return false;
        }

        //---获取所有的选中cell 的值
        private string GetSelectedCellsValue(DataGrid dg)
        {
            var cells = dg.SelectedCells;
            StringBuilder sb = new StringBuilder();
            if (cells.Any())
            {
                foreach (var cell in cells)
                {
                    sb.Append((cell.Column.GetCellContent(cell.Item) as TextBlock).Text);
                    sb.Append(" ");
                }
            }
            return sb.ToString();
        }

        private void Button_Create_Click(object sender, RoutedEventArgs e)
        {
            this.dtgShow.Visibility = Visibility.Visible;

            while (dtgShow.Columns.Count != 0)
            {
                list.Clear();
                dtgShow.Columns.RemoveAt(0);
            }

            while (showdata.Count != 0)
            {
                showdata.RemoveAt(0);
            }

            int num_col = Convert.ToInt32(Col_Num.Text);
            for (int i = 0; i < num_col; i++)
            {
                dtgShow.Columns.Add(new DataGridTextColumn
                {
                    Width = (Width - 800) / num_col,
                    Header = i + 1,
                    Binding = new Binding("[{i.ToString()}]")
                });
            }

            int num_row = Convert.ToInt32(Row_Num.Text);
            for (int i = 0; i < num_row; i++)
            {
                list.Add(new int[Convert.ToInt32(Col_Num.Text)]);
                showdata.Insert(0, new int[dtgShow.Columns.Count]);
            }

        }

        //非均匀风速分布End*****************************************************************************************************************************End


        //DetailResult******************************************************************************************************Start

        //people类
        public class people
        {
            public string tube { get; set; }
            public string row { get; set; }
            public string Pri { get; set; }
            public string Tri { get; set; }
            public string Hri { get; set; }
            public string Pro { get; set; }
            public string Tro { get; set; }
            public string Hro { get; set; }
            public string HTC { get; set; }
            public string Q { get; set; }
            public string mr { get; set; }
        }

            //创建people数组
            List<people> peopleList = new List<people>();

            //①定义一个可读可写的公用的字符串：getName
            public string Result_Tube_row { get; set; }
            public string Result_Row { get; set; }

            public double[,] getName_tube { get; set; }
            public double[,] getName_row { get; set; }
            public double[,] getName_Pri { get; set; }
            public double[,] getName_Tri { get; set; }
            public double[,] getName_Hri { get; set; }
            public double[,] getName_Pro { get; set; }
            public double[,] getName_Tro { get; set; }
            public double[,] getName_Hro { get; set; }
            public double[,] getName_HTC { get; set; }
            public double[,] getName_Q { get; set; }
            public double[,] getName_mr { get; set; }

        //输出Excel_具体计算结果
            private void TreeView_DetailResultExcelOutput_GotFocus(object sender, RoutedEventArgs e)
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

            //DetailResult******************************************************************************************************End


            private void Button_Click_1(object sender, RoutedEventArgs e)
            {
                //TabControl_Picuture.Height = 300;
                this.Canvas_Picture_HEx.Height=450;
                this.TabControl_Picuture.Height = 450;

                //TabControl_Picuture.Height = 500;
                
                //GroupBox_RealTimeInputShow_Condenser.Style.Add("position", "absolute");
                //button1.Style.Add("left", "0");
                //button1.Style.Add("top", "0");
                //tryRT.Midea_Heat_Exchanger_Simulation Window_Midea_Heat_Exchanger_Simulation = new tryRT.Midea_Heat_Exchanger_Simulation();
                //Window_Midea_Heat_Exchanger_Simulation.Show();

                
            }

            private void Button_Click_Sure(object sender, RoutedEventArgs e)
            {
                int _row = Convert.ToInt32(Row.Text);
                int _tube = Convert.ToInt32(tube_per.Text);
                vm.Connectors.Clear();
                vm.Capacities.Clear();
                node1 = null;
                rect = null;
                vm.GenerateNode(_row, _tube);
                vm.Rects[0].X = vm.Nodes[0].X - vm.RowPitch + 5;
                vm.Rects[0].Y = vm.Nodes[0].Y;
                vm.Rects[0].RectHeight = (_tube - 1) * vm.TubePitch + 20;
                vm.Rects[1].X = vm.Nodes[0].X + _row * vm.RowPitch + 5;
                vm.Rects[1].Y = vm.Rects[0].Y - (_row + 1) % 2 * vm.TubePitch / 2;
                vm.Rects[1].RectHeight = vm.Rects[0].RectHeight;
                while (vm.Rects.Count > 2)
                {
                    vm.Rects.RemoveAt(2);
                };
            }

            private Node node1;
            private Node node2;
            private Rect rect;
            private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                //string msg="Successful";
                //MessageBox.Show(msg);
                if (vm.CreatNewConnector && ListBox_this.SelectedItem != null)
                {
                    //int nodeindex = ListBox.SelectedIndex;
                    var item = ListBox_this.SelectedItem;
                    if (item.GetType().Name == "Node")
                    {
                        node2 = item as Node;
                        if (node1 != null && node1 != node2)
                        {
                            Connector newLine = new Connector();
                            newLine.Start = node1;
                            newLine.End = node2;
                            vm.Connectors.Add(newLine);
                            node1 = node2;
                        }
                        else if (rect != null)
                        {
                            Capacity newcapatity = new Capacity();
                            newcapatity.Start = node2;
                            newcapatity.End = rect;
                            vm.Capacities.Where(x => x.Start == newcapatity.Start && x.End == newcapatity.End).ToList().ForEach(x => vm.Capacities.Remove(x));
                            vm.Capacities.Add(newcapatity);
                            node1 = node2;
                            if (node1.Y < rect.Y)
                            {
                                rect.RectHeight = rect.RectHeight + (rect.Y - node1.Y);
                                rect.Y = node1.Y;
                            }
                            else if (node1.Y > rect.Y)
                            {
                                if (node1.Y + 20 > rect.Y + rect.RectHeight)
                                {
                                    rect.RectHeight = node1.Y + 20 - rect.Y;
                                }
                            }
                        }
                        node1 = node2;
                    }
                    else if (item.GetType().Name == "Rect")
                    {
                        rect = item as Rect;
                        if (node1 != null)
                        {
                            Capacity newcapatity = new Capacity();
                            newcapatity.Start = node1;
                            newcapatity.End = rect;
                            vm.Capacities.Where(x => x.Start == newcapatity.Start && x.End == newcapatity.End).ToList().ForEach(x => vm.Capacities.Remove(x));
                            vm.Capacities.Add(newcapatity);
                            if (node1.Y < rect.Y)
                            {
                                rect.RectHeight = rect.RectHeight + (rect.Y - node1.Y);
                                rect.Y = node1.Y;
                            }
                            else if (node1.Y > rect.Y)
                            {
                                if (node1.Y + 20 > rect.Y + rect.RectHeight)
                                {
                                    rect.RectHeight = node1.Y + 20 - rect.Y;
                                }
                            }
                            node1 = null;
                        }
                    }
                }
                else if (ListBox_this.SelectedItem != null && vm.CreatNewConnector == false)//set capatity
                {
                    var item = ListBox_this.SelectedItem;
                    if (item.GetType().Name == "Capacity")
                    {
                        TextBox_Length.IsEnabled = true;
                        TextBox_Diameter.IsEnabled = true;
                        Button_Capacity.IsEnabled = true;
                        var selectitem = item as Capacity;
                        TextBox_Length.Text = selectitem.Length.ToString();
                        TextBox_Diameter.Text = selectitem.Diameter.ToString();
                    }
                    else
                    {
                        TextBox_Length.IsEnabled = false;
                        TextBox_Diameter.IsEnabled = false;
                        Button_Capacity.IsEnabled = false;
                        TextBox_Length.Text = "";
                        TextBox_Diameter.Text = "";
                    }
                }
                else if (ListBox_this.SelectedItem == null)
                {
                    TextBox_Length.IsEnabled = false;
                    TextBox_Diameter.IsEnabled = false;
                    Button_Capacity.IsEnabled = false;
                    TextBox_Length.Text = "";
                    TextBox_Diameter.Text = "";
                }
            }

            private void ListBox_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
            {
                //string msg = "Right";
                //MessageBox.Show(msg);  
                ListBox_this.SelectedValue = null;
                node1 = null;
                node2 = null;
                rect = null;
            }
            private void Button_Click_Reconnect(object sender, RoutedEventArgs e)
            {
                Button_Click_Sure(sender, e);
            }
            private void Button_Click_zoomout(object sender, RoutedEventArgs e)
            {
                vm.ScaleFactor = 1.2;
                vm.Zoom();
            }
            private void Button_Click_zoomin(object sender, RoutedEventArgs e)
            {
                vm.ScaleFactor = 1.0 / 1.2;
                vm.Zoom();
            }
            private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            {
                if (ElementUnderMouse() && vm.CreatNewConnector)
                {
                    Rect newrect = new Rect();
                    newrect.X = e.GetPosition(ListBox_this).X + 5;
                    newrect.Y = e.GetPosition(ListBox_this).Y;
                    newrect.RectHeight = 20;
                    vm.Rects.Add(newrect);
                }
            }

            private bool ElementUnderMouse()
            {
                bool flag = false;
                string name = Mouse.DirectlyOver.GetType().Name;
                if (name == "Canvas") flag = true;
                return flag;
            }

            private void Button_Click_delete(object sender, RoutedEventArgs e)
            {
                var item = ListBox_this.SelectedItem;
                vm.CreatNewConnector = false;
                rect = null;
                node1 = null;
                if (item != null)
                {
                    if (item.GetType().Name == "Connector")
                    {
                        var selectelement = item as Connector;
                        vm.Connectors.Remove(selectelement);
                    }
                    else if (item.GetType().Name == "Rect")
                    {
                        var selectelement = item as Rect;
                        if (selectelement != vm.Rects[0] && selectelement != vm.Rects[1])
                        {
                            vm.Capacities.Where(x => x.End == selectelement).ToList().ForEach(x => vm.Capacities.Remove(x));
                            vm.Rects.Remove(selectelement);
                        }
                    }
                    else if (item.GetType().Name == "Capacity")
                    {
                        var selectelement = item as Capacity;
                        vm.Capacities.Remove(selectelement);
                    }
                }
            }

            private void Button_Click_Capacity(object sender, RoutedEventArgs e)
            {
                vm.CreatNewConnector = false;
                if (ListBox_this.SelectedItem != null)
                {
                    var item = ListBox_this.SelectedItem;
                    if (item.GetType().Name == "Capacity")
                    {
                        var selectitem = item as Capacity;
                        selectitem.Length = Convert.ToDouble(TextBox_Length.Text);
                        selectitem.Diameter = Convert.ToDouble(TextBox_Diameter.Text);
                    }
                }
            }

















    }

}


