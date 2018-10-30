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
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO;

namespace tryRT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        //综合结果的定义
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

        private int CircuitIndex = new int();//0:Manual circuiting 1:Auto circuiting
        public int AirFlowDirection = new int();//0:normal 1:reverse
        public string AirFlowString = "-<-<-<-<-<-AirFlow-<-<-<-<-";
        private int RefFlowDirection = new int();//0:normal 1:reverse

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
            this.RadioButton_PriTri_Evap.IsChecked = true;
            this.RadioButton_Tro_Evap.IsChecked = true;
            this.RadioButton_mro_Evap.IsChecked = true;
            this.RadioButton_WetBulbTemperature.IsChecked = true;
            this.RadioButton_AirVolumnFlowRate.IsChecked = true;
            this.RadioButton_mro_Cond.IsChecked = true;
            this.RadioButton_ManualArrange.IsChecked = true;
            this.CheckBox_UniformWind.IsChecked = true;
            this.RadioButton_HExType_Condenser.IsChecked = true;
            this.TubeArrangement_Crossed_High.IsChecked = true;
            this.TreeView_HExType.Focus();

            //数据初始化
            flag_Calculated = false;
            this.Pri_Evap.Text = "0";
            this.xi_Evap.Text = "0";
            this.Hri_Evap.Text = "0";
            this.Tro_sub_Evap.Text = "0";
            this.xo_Evap.Text = "0";
            ViewModel.Connector_Num = 0;
            ViewModel.Start_Capillary_Num = 0;
            ViewModel.End_Capillary_Num = 0;
            ViewModel.Rect_Num = 0;
            ViewModel.Circuit_Num = 0;


            //初始化湿空气数组
            Model.HumidAirSourceData.SourceTableData = Model.HumidAirSourceData.InitializeSourceTableData();

            //#region//测试湿空气物性查表对错用
            ////测试用
            //Model.HumidAirProp humidairprop = new Model.HumidAirProp();
            //double[] Tin_a = new double[] 
            //{
            //    -39.75, -39.75, -39.75, -39.75, -39.75, -39.75, -39.75, -39.75, -39.75, 
            //    -29.75, -29.75, -29.75, -29.75, -29.75, -29.75, -29.75, -29.75, -29.75, 
            //    -19.75, -19.75, -19.75, -19.75, -19.75, -19.75, -19.75, -19.75, -19.75, 
            //    -9.75, -9.75, -9.75, -9.75, -9.75, -9.75, -9.75, -9.75, -9.75, 
            //    -0.75, -0.75, -0.75, -0.75, -0.75, -0.75, -0.75, -0.75, -0.75, 
            //    10.75,10.75,10.75,10.75,10.75,10.75,10.75,10.75,10.75,
            //    20.75,20.75,20.75,20.75,20.75,20.75,20.75,20.75,20.75,
            //    30.75,30.75,30.75,30.75,30.75,30.75,30.75,30.75,30.75,
            //    40.75,40.75,40.75,40.75,40.75,40.75,40.75,40.75,40.75,
            //    50.75,50.75,50.75,50.75,50.75,50.75,50.75,50.75,50.75,
            //    60.75,60.75,60.75,60.75,60.75,60.75,60.75,60.75,60.75
            //};
            //double[] Tin_a2 = new double[] 
            //{
            //    -39.25, -39.25, -39.25, -39.25, -39.25, -39.25, -39.25, -39.25, -39.25, 
            //    -29.25, -29.25, -29.25, -29.25, -29.25, -29.25, -29.25, -29.25, -29.25, 
            //    -19.25, -19.25, -19.25, -19.25, -19.25, -19.25, -19.25, -19.25, -19.25, 
            //    -9.25, -9.25, -9.25, -9.25, -9.25, -9.25, -9.25, -9.25, -9.25, 
            //    -0.25, -0.25, -0.25, -0.25, -0.25, -0.25, -0.25, -0.25, -0.25, 
            //    10.25,10.25,10.25,10.25,10.25,10.25,10.25,10.25,10.25,
            //    20.25,20.25,20.25,20.25,20.25,20.25,20.25,20.25,20.25,
            //    30.25,30.25,30.25,30.25,30.25,30.25,30.25,30.25,30.25,
            //    40.25,40.25,40.25,40.25,40.25,40.25,40.25,40.25,40.25,
            //    50.25,50.25,50.25,50.25,50.25,50.25,50.25,50.25,50.25,
            //    60.25,60.25,60.25,60.25,60.25,60.25,60.25,60.25,60.25
            //};
            //double[] Tin_a3 = new double[] 
            //{   0,0,0,0,0,0,0,0,0,
            //    0.25,0.25,0.25,0.25,0.25,0.25,0.25,0.25,0.25,
            //    0.75,0.75,0.75,0.75,0.75,0.75,0.75,0.75,0.75,                
            //};
            //double[] Tin_a4 = new double[] 
            //{   35,35.075,35.075,35.075,35.075,35.075,35.075,35.075,
            //    45,45.075,45.075,45.075,45.075,45.075,45.075,45.075,
            //    24,24.075,24.075,24.075,24.075,24.075,24.075,24.075,
            //    12,12.075,12.075,12.075,12.075,12.075,12.075,12.075, 
            //    7,7.075,7.075,7.075,7.075,7.075,7.075,7.075
            //};
            //double[] Twet1 = new double[] 
            //{   35,30.075,24.075,20.075,15.075,10.075,5.075,0.075,
            //    45,35.075,30.075,24.075,20.075,15.075,5.075,0.075,
            //    24,20,15.075,10.075,5.075,15,5,0.075, 
            //    12,11,10.075,9.075,8.075,7,5,0.075,  
            //    7,6,5.075,4.075,3.075,2.075,1.075,0.075 
            //};
            //double[] Tin_a5 = new double[] 
            //{   35,35.675,35.675,35.675,35.675,35.675,35.675,35.675,
            //    45,45.675,45.675,45.675,45.675,45.675,45.675,45.675,
            //    24,24.675,24.675,24.675,24.675,24.675,24.675,24.675,
            //    12,12.675,12.675,12.675,12.675,12.675,12.675,12.675, 
            //    7,7.675,7.675,7.675,7.675,7.675,7.675,7.675
            //};
            //double[] Twet2 = new double[] 
            //{   35,30.675,24.675,20.675,15.675,10.675,5.675,0.675,
            //    45,35.675,30.675,24.675,20.675,15.675,5.675,0.675,
            //    24,20,15.675,10.675,5.675,15,5,0.675, 
            //    12,11,10.675,9.675,8.675,7,5,0.675,  
            //    7,6,5.675,4.675,3.675,2.675,1.675,0.675 
            //};
            //double[] H3 = new double[] 
            //{   -0.75,-0.75,-0.75,-0.75,-0.75,-0.75,-0.75,-0.75,-0.75, 
            //    -0.25,-0.25,-0.25,-0.25,-0.25,-0.25,-0.25,-0.25,-0.25,
            //    0,0,0,0,0,0,0,0,0,
            //    0.25,0.25,0.25,0.25,0.25,0.25,0.25,0.25,0.25,
            //    0.75,0.75,0.75,0.75,0.75,0.75,0.75,0.75,0.75             
            //};
            //double[] RHi = new double[] 
            //{
            //    0.1,0.2,0.3,0.4,0.5,0.6,0.7,0.8,0.9,
            //    0.1,0.2,0.3,0.4,0.5,0.6,0.7,0.8,0.9,
            //    0.1,0.2,0.3,0.4,0.5,0.6,0.7,0.8,0.9,
            //    0.1,0.2,0.3,0.4,0.5,0.6,0.7,0.8,0.9,
            //    0.1,0.2,0.3,0.4,0.5,0.6,0.7,0.8,0.9,
            //    0.1,0.2,0.3,0.4,0.5,0.6,0.7,0.8,0.9,
            //    0.1,0.2,0.3,0.4,0.5,0.6,0.7,0.8,0.9,
            //    0.1,0.2,0.3,0.4,0.5,0.6,0.7,0.8,0.9,
            //    0.1,0.2,0.3,0.4,0.5,0.6,0.7,0.8,0.9,
            //    0.1,0.2,0.3,0.4,0.5,0.6,0.7,0.8,0.9,
            //    0.1,0.2,0.3,0.4,0.5,0.6,0.7,0.8,0.9
            //};

            ////double[] RHi = new double[] { 0.05, 0.055, 0.55, 0.555, 0.88, 0.888, 0.05, 0.055, 0.55, 0.555, 0.88, 0.888, 0.05, 0.055, 0.55, 0.555, 0.88, 0.888, 0.05, 0.055, 0.55, 0.555, 0.88, 0.888, 0.05, 0.055, 0.55, 0.555, 0.88, 0.888, 0.05, 0.055, 0.55, 0.555, 0.88, 0.888 };
            //double[] Omega = new double[] 
            //{
            //    0.005,0.01,0.015,0.02,0.025,0.03,0.035,0.04,0.045,
            //    0.005,0.01,0.015,0.02,0.025,0.03,0.035,0.04,0.045,
            //    0.005,0.01,0.015,0.02,0.025,0.03,0.035,0.04,0.045,
            //    0.005,0.01,0.015,0.02,0.025,0.03,0.035,0.04,0.045,
            //    0.005,0.01,0.015,0.02,0.025,0.03,0.035,0.04,0.045,
            //    0.005,0.01,0.015,0.02,0.025,0.03,0.035,0.04,0.045,
            //    0.005,0.01,0.015,0.02,0.025,0.03,0.035,0.04,0.045,
            //    0.005,0.01,0.015,0.02,0.025,0.03,0.035,0.04,0.045,
            //    0.005,0.01,0.015,0.02,0.025,0.03,0.035,0.04,0.045,
            //    0.005,0.01,0.015,0.02,0.025,0.03,0.035,0.04,0.045,
            //    0.005,0.01,0.015,0.02,0.025,0.03,0.035,0.04,0.045
            //};
            //double[] H = new double[]
            //{
            //    -39.75,-29.75,-19.75,-9.75,0.75,
            //    10.75,20.75,30.75,40.75,50.75,
            //    60.75,70.75,80.75,90.75,100.75,
            //    110.75,120.75,130.75,140.75,150.75,
            //    160.75,170.75,180.75,190.75,200.75,
            //    210.75,220.75,230.75,240.75,250.75,
            //    260.75,270.75,280.75,290.75,300.75,
            //    310.75,320.75,330.75,340.75,350.75,
            //    360.75,370.75,380.75,390.75,400.75,
            //    410.75,420.75,430.75,440.75,450.75,
            //    460.75,470.75,480.75,490.75,500.75,
            //    510.75,520.75,530.75,540.75,550.75,
            //    560.75,570.75,580.75,590.75,600.75,
            //    610.75,620.75,630.75,640.75,650.75,
            //    660.75,670.75,680.75,690.75,700.75,
            //    710.75,720.75,730.75,740.75,750.75,
            //    760.75,770.75,780.75,790.75,798.75
            //};
            //double[] H2 = new double[]
            //{
            //    -39.25,-29.25,-19.25,-9.25,0.25,
            //    10.25,20.25,30.25,40.25,50.25,
            //    60.25,70.25,80.25,90.25,100.25,
            //    110.25,120.25,130.25,140.25,150.25,
            //    160.25,170.25,180.25,190.25,200.25,
            //    210.25,220.25,230.25,240.25,250.25,
            //    260.25,270.25,280.25,290.25,300.25,
            //    310.25,320.25,330.25,340.25,350.25,
            //    360.25,370.25,380.25,390.25,400.25,
            //    410.25,420.25,430.25,440.25,450.25,
            //    460.25,470.25,480.25,490.25,500.25,
            //    510.25,520.25,530.25,540.25,550.25,
            //    560.25,570.25,580.25,590.25,600.25,
            //    610.25,620.25,630.25,640.25,650.25,
            //    660.25,670.25,680.25,690.25,700.25,
            //    710.25,720.25,730.25,740.25,750.25,
            //    760.25,770.25,780.25,790.25,798.25
            //};

            //////***************************************************************测试运行速度Start***************************************************************
            //DateTime beforDT = System.DateTime.Now;

            //for (int i = 0; i < Tin_a5.Count(); i++)
            //{
            //    double R = humidairprop.RHI_TwetBulb(Tin_a5[i], Twet2[i], Model.HumidAirSourceData.SourceTableData);

            //    double Z = R;
            //    string arr1 = Convert.ToString(Z);
            //    Console.WriteLine(arr1);
            //}
            ////double R = humidairprop.RHI_TwetBulb(45.075, 35.075, Model.HumidAirSourceData.SourceTableData);
            ////double Z = R;
            ////string arr1 = Convert.ToString(Z);
            ////Console.WriteLine(arr1);

            ////double hin_a = humidairprop.H(Tin_a[i], "R", RHi[i]);     //5.859ms/0.0591818181818182ms
            ////double h_ac = humidairprop.H(Tin_a[i], "Omega", Omega[i]);        //6.83ms/0.00683ms
            ////double omega_in = humidairprop.O(Tin_a[i], RHi[i]);       //5.85ms/0.0590909090909091ms
            ////double cp_da = humidairprop.Cp(Tin_a[i], RHi[i]);     //3.9ms/0.0393939393939394ms 
            ////double Tdp = humidairprop.Tdp(Tin_a[i], RHi[i]);      //5.8ms/0.0591818181818182ms
            ////double T_s_e = humidairprop.Ts(H[i]);     //2.9ms/0.0344682352941176ms
            ////double airInput.RHi = humidairprop.RHI_TwetBulb(Convert.ToDouble(tai.Text), Convert.ToDouble(Tai_wet.Text), Model.HumidAirSourceData.SourceTableData);

            ////double hin_a = CoolProp.HAPropsSI("H", "T", Tin_a[i] + 273.15, "P", 101325, "R", RHi[i]);//93.75ms
            ////double h_ac = CoolProp.HAPropsSI("H", "T", Tin_a[i] + 273.15, "P", 101325, "W", Omega[i]);//78.125ms
            ////double omega_in = CoolProp.HAPropsSI("W", "T", Tin_a[i] + 273.15, "P", 101325, "R", RHi[i]);//293.9453ms
            ////double cp_da = CoolProp.HAPropsSI("C", "T", Tin_a[i] + 273.15, "P", 101325, "R", RHi[i]);//121.086ms
            ////double Tdp = CoolProp.HAPropsSI("D", "T", Tin_a[i] + 273.15, "P", 101325, "R", RHi[i]) - 273.15;//292.98ms
            ////double T_s_e = CoolProp.HAPropsSI("T","H",H[i],"P",101325,"R",1.0)-273.15;//365.2484ms

            //DateTime afterDT = System.DateTime.Now;
            //TimeSpan ts = afterDT.Subtract(beforDT);
            //Console.WriteLine("DateTime总共花费{0}ms.", ts.TotalMilliseconds);
            ////////***************************************************************测试运行速度End***************************************************************

            //#endregion

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
            string[] sss = new string[10];
            sss[0] = "R32";
            sss[1] = "R410A";
            sss[2] = "R290";
            sss[3] = "R22";
            sss[4] = "R600a";
            sss[5] = "R1234YF";
            sss[6] = "R1234ZE";
            sss[7] = "R1233ZD";
            sss[8] = "R744";
            sss[9] = "R502";
            customList.Add(sss[0]);
            customList.Add(sss[1]);
            customList.Add(sss[2]);
            customList.Add(sss[3]);
            customList.Add(sss[4]);
            customList.Add(sss[5]);
            customList.Add(sss[6]);
            customList.Add(sss[7]);
            customList.Add(sss[8]);
            customList.Add(sss[9]);

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

            //ComboBox_flowtype.ItemsSource = customList;
            //ComboBox_flowtype.SelectedValue = customList[0];
        }

        private void MenuItem_Click_Close(object sender, RoutedEventArgs e)//关闭按钮
        {
            this.Close();
        }

        private void MenuItem_Click_Calculate(object sender, RoutedEventArgs e)//Start calculate
        {
            flag_Calculated = true;

            Model.Main m_Main = new Model.Main();
            Model.Basic.GeometryInput geoInput = new Model.Basic.GeometryInput();
            Model.Basic.RefStateInput refInput = new Model.Basic.RefStateInput();
            Model.Basic.AirStateInput airInput = new Model.Basic.AirStateInput();
            Model.HumidAirProp humidairprop = new Model.HumidAirProp();
            
            //几何结构输入
            geoInput.Pt = Convert.ToDouble(Pt.Text);//管间距
            geoInput.Pr = Convert.ToDouble(Pr.Text);//列间距
            geoInput.Nrow = Convert.ToInt16(Row.Text);//管排数
            geoInput.Do = Convert.ToDouble(Do.Text);//管外径
            geoInput.Ntube = Convert.ToInt16(tube_per.Text);//管数/排
            geoInput.Tthickness = Convert.ToDouble(thick_tube.Text);//壁厚
            geoInput.L = Convert.ToDouble(L.Text);//管长
            geoInput.Fthickness = Convert.ToDouble(Fthick.Text);//翅片厚度
            geoInput.FPI = Convert.ToDouble(Fnum.Text);//翅片间距

            refInput.FluidName = ComboBox_Refrigerant.Text;//制冷剂名
            AbstractState coolprop = AbstractState.factory("HEOS", refInput.FluidName);
            if (RadioButton_HExType_Condenser.IsChecked == true) //Cond
            {
                refInput.tc = Convert.ToDouble(this.tc.Text);//Cond_in饱和温度
                refInput.tri = Convert.ToDouble(this.tri.Text);//Cond_in温度

                if (this.RadioButton_mro_Cond.IsChecked == true) 
                { 
                    refInput.Massflowrate = Convert.ToDouble(this.mro_Cond.Text);
                    Model.WindowControls.RadioButton_mro_Cond = true; 
                }//制冷剂流量kg/s
                else 
                { 
                    refInput.Massflowrate = 0.01; 
                     Model.WindowControls.RadioButton_mro_Cond = false; 
                }
                if (this.RadioButton_xo_Cond.IsChecked == true) 
                { 
                    refInput.xo_Cond = Convert.ToDouble(this.xo_Cond.Text);
                     Model.WindowControls.RadioButton_xo_Cond = true; 
                }//Cond_out干度
                else 
                { 
                    refInput.xo_Cond = 0; 
                     Model.WindowControls.RadioButton_xo_Cond = false; 
                }
                if (this.RadioButton_Tro_sub_Cond.IsChecked == true) 
                { 
                    refInput.Tro_sub_Cond = Convert.ToDouble(this.Tro_sub_Cond.Text); 
                     Model.WindowControls.RadioButton_Tro_sub_Cond = true; 
                }//Cond_out过冷度
                else 
                { 
                    refInput.Tro_sub_Cond = 0; 
                     Model.WindowControls.RadioButton_Tro_sub_Cond = false; 
                }

                refInput.te = -1000;//Evap饱和温度
                refInput.xi_Evap = 0;//Evap_in干度
                refInput.H_exv = 0;//Evap_in焓
                refInput.T_exv = -1000;//Evap阀前温度
                refInput.P_exv = 0;//Evap阀前压力
                refInput.Tro_sub_Evap = 0;//Evap_out过热度
                refInput.xo_Evap = 1;//Evap_out干度

                 Model.WindowControls.RadioButton_Pri_Evap = false;
                 Model.WindowControls.RadioButton_xi_Evap = false;
                 Model.WindowControls.RadioButton_Hri_Evap = false;
                 Model.WindowControls.RadioButton_PriTri_Evap = false;
                 Model.WindowControls.RadioButton_Tro_Evap = false;
                 Model.WindowControls.RadioButton_Tro_sub_Evap = false;
                 Model.WindowControls.RadioButton_xo_Evap = false;
                 Model.WindowControls.RadioButton_mro_Evap = false;
                
            }
            else  //Evap
            {
                if (this.RadioButton_Tro_Evap.IsChecked == true) 
                { 
                    refInput.te = Convert.ToDouble(this.Tcro_Evap.Text); 
                     Model.WindowControls.RadioButton_Tro_Evap = true;
                     Model.WindowControls.RadioButton_Pri_Evap = false; 
                }//Evap饱和温度
                else if (this.RadioButton_Pri_Evap.IsChecked == true) 
                { 
                    coolprop.update(input_pairs.PQ_INPUTS, Convert.ToDouble(this.Pri_Evap.Text) * 1000, 1); 
                    refInput.te = coolprop.T() - 273.15; 
                     Model.WindowControls.RadioButton_Tro_Evap = false; 
                     Model.WindowControls.RadioButton_Pri_Evap = true; 
                }//调用CoolProp把压力转成饱和温度
                else 
                { 
                    refInput.te = -1000; 
                     Model.WindowControls.RadioButton_Tro_Evap = false;
                     Model.WindowControls.RadioButton_Pri_Evap = false; 
                }

                if (this.RadioButton_xi_Evap.IsChecked == true) 
                { 
                    refInput.xi_Evap = Convert.ToDouble(this.xi_Evap.Text);
                     Model.WindowControls.RadioButton_xi_Evap = true;
                }//Evap_in干度
                else 
                { 
                    refInput.xi_Evap = 0;
                     Model.WindowControls.RadioButton_xi_Evap = false;
                }
                if (this.RadioButton_Hri_Evap.IsChecked == true) 
                { 
                    refInput.H_exv = Convert.ToDouble(this.Hri_Evap.Text);
                     Model.WindowControls.RadioButton_Hri_Evap = true;
                }//Evap_in焓
                else 
                { 
                    refInput.H_exv = -1000;
                     Model.WindowControls.RadioButton_Hri_Evap = false;
                }
                if (this.RadioButton_PriTri_Evap.IsChecked == true) 
                {
                    refInput.P_exv = Convert.ToDouble(this.Pri_ValveBefore.Text);//Evap阀前压力
                    refInput.T_exv = Convert.ToDouble(this.Tri_ValveBefore.Text);//Evap阀前温度
                     Model.WindowControls.RadioButton_PriTri_Evap = true;
                }
                else
                {
                    refInput.P_exv = 0;
                    refInput.T_exv = -1000;
                     Model.WindowControls.RadioButton_PriTri_Evap = false;
                }

                if (this.RadioButton_Tro_sub_Evap.IsChecked == true) 
                { 
                    refInput.Tro_sub_Evap = Convert.ToDouble(this.Tro_sub_Evap.Text);
                     Model.WindowControls.RadioButton_Tro_sub_Evap = true;
                }//Evap_out过热度
                else 
                { 
                    refInput.Tro_sub_Evap = 0;
                     Model.WindowControls.RadioButton_Tro_sub_Evap = false;
                }
                if (this.RadioButton_xo_Evap.IsChecked == true) 
                { 
                    refInput.xo_Evap = Convert.ToDouble(this.xo_Evap.Text);
                     Model.WindowControls.RadioButton_xo_Evap = true;
                }
                else 
                { 
                    refInput.xo_Evap = 1;
                     Model.WindowControls.RadioButton_xo_Evap = false;
                }
                if (this.RadioButton_mro_Evap.IsChecked == true) 
                { 
                    refInput.Massflowrate = Convert.ToDouble(this.mro_Evap.Text);
                     Model.WindowControls.RadioButton_mro_Evap = true;
                }//制冷剂流量kg/s
                else 
                { 
                    refInput.Massflowrate = 0.01;
                     Model.WindowControls.RadioButton_mro_Evap = false;
                }

                refInput.tc = -1000;//Cond饱和温度
                refInput.tri = -1000;//Cond进口温度
                refInput.Tro_sub_Cond = 0;//Cond_out过冷度

                 Model.WindowControls.RadioButton_mro_Cond = false;
                 Model.WindowControls.RadioButton_xo_Cond = false;
                 Model.WindowControls.RadioButton_Tro_sub_Cond = false;
            }

            if (RadioButton_AirVolumnFlowRate.IsChecked == true) { airInput.Volumetricflowrate = Convert.ToDouble(this.Va.Text); }//空气体积流量m3/s
            else { airInput.Volumetricflowrate = Convert.ToDouble(this.Velocity_ai.Text) * geoInput.L * 0.001 * geoInput.Pt * geoInput.Ntube; }
            airInput.tai = Convert.ToDouble(this.tai.Text);//进风干球温度
            if (RadioButton_RelativeHumidity.IsChecked == true) { airInput.RHi = Convert.ToDouble(this.RHi.Text); }//进风相对湿度
            else { airInput.RHi = humidairprop.RHI_TwetBulb(Convert.ToDouble(this.tai.Text), Convert.ToDouble(this.Tai_wet.Text), Model.HumidAirSourceData.SourceTableData); }
            airInput.AirFlowDirection = AirFlowDirection;//0:normal 1:reverse

            string fin_type = this.ComboBox_fintype.Text;//平片
            string tube_type = this.ComboBox_tubetype.Text;//光管
            string hex_type = this.ComboBox_hextype.Text;//冷凝器

            //string bb = ComboBox6_SelectionChanged(object sender, SelectionChangedEventArgs e);
            //m_Main.W5(a, b).ha
            Model.Basic.CapiliaryInput cap_inlet = new Model.Basic.CapiliaryInput();//进口毛细管
            Model.Basic.CapiliaryInput cap_outlet = new Model.Basic.CapiliaryInput();//出口毛细管

            //流路
            int i = 1;
            int j = 1;
            int k = 1;
            int[,] CirArrange = new int[i,j];
            int[, ,] NodeInfo = new int[i,j,k];
            if(CircuitIndex==0)//Manual connect circuit info
            {
                i = CircuitInput.CircuitConvert(vm.Nodes, vm.Connectors, vm.Capillaries, RefFlowDirection).GetLength(0);
                j = CircuitInput.CircuitConvert(vm.Nodes, vm.Connectors, vm.Capillaries, RefFlowDirection).GetLength(1);
                CirArrange = new int[i, j];
                CirArrange = CircuitInput.CircuitConvert(vm.Nodes, vm.Connectors, vm.Capillaries,RefFlowDirection);
                cap_inlet = CircuitInput.CapillaryConvert_inlet(CirArrange, vm.Capillaries);
                cap_outlet = CircuitInput.CapillaryConvert_outlet(CirArrange, vm.Capillaries);
                i = CircuitInput.NodesConvert(vm.Nodes, vm.Connectors, vm.Capillaries, vm.Rects,RefFlowDirection).GetLength(0);
                j = CircuitInput.NodesConvert(vm.Nodes, vm.Connectors, vm.Capillaries, vm.Rects,RefFlowDirection).GetLength(2);
                NodeInfo = new int[i, 2, j];
                NodeInfo = CircuitInput.NodesConvert(vm.Nodes, vm.Connectors, vm.Capillaries, vm.Rects,RefFlowDirection);
            }
            else if(CircuitIndex==1)//Auto connect circuit info
            {
                Model.Basic.CircuitNumber CircuitInfo=new Model.Basic.CircuitNumber();
                CircuitInfo.number = new int[] { Convert.ToInt32(Cirnum.Text), Convert.ToInt32(Cirnum.Text) };                 
                if (CircuitInfo.number[0] > geoInput.Ntube)//Avoid invalid Ncir input//管排数比管数还多
                {
                    throw new Exception("circuit number is beyond range.");
                }
                CircuitInfo.TubeofCir = new int[CircuitInfo.number[0]];
                //Get AutoCircuitry
                CircuitInfo = Model.Basic.AutoCircuiting.GetTubeofCir(geoInput.Nrow, geoInput.Ntube, CircuitInfo);
                CirArrange = new int[CircuitInfo.number[0], CircuitInfo.TubeofCir[CircuitInfo.number[0] - 1]];
                if(geoInput.Nrow%2==0)
                {
                    CirArrange = Model.Basic.AutoCircuiting.GetCirArrange_2Row(CirArrange, geoInput.Nrow, geoInput.Ntube, CircuitInfo);
                }
                else if(geoInput.Nrow%2==1)
                {
                    CirArrange = Model.Basic.AutoCircuiting.GetCirArrange_3Row(CirArrange, geoInput.Nrow, geoInput.Ntube, CircuitInfo);
                }
                
                //if (geoInput.Nrow % 2 == 0)
                //{
                //    CirArrange = Model.Basic.AutoCircuiting.GetCirArrange_2Row(CirArrange, geoInput.Nrow, geoInput.Ntube, CircuitInfo);
                //}
                //else
                //{
                //    CirArrange = Model.Basic.AutoCircuiting.GetCirArrange_3Row(CirArrange, geoInput.Nrow, geoInput.Ntube, CircuitInfo);
                //}

                bool reverse = RefFlowDirection == 0 ? false : true;
                CirArrange = Model.Basic.CircuitReverse.CirReverse(reverse, CirArrange, CircuitInfo);
                NodeInfo = new int[2, 2, Convert.ToInt32(Cirnum.Text)];
                for(i=0;i<2;i++)//initialize
                    for(j=0;j<2;j++)
                        for(k=0;k<Convert.ToInt32(Cirnum.Text);k++)
                        {
                            NodeInfo[i, j, k] = -100;
                        }
                NodeInfo[0, 0, 0] = -1;
                NodeInfo[1, 1, 0] = -1;
                for (i = 0; i < Convert.ToInt32(Cirnum.Text);i++ )
                {
                    NodeInfo[0, 1, i] = i;
                    NodeInfo[1, 0, i] = i;
                }
                cap_inlet.d_cap = new double[Convert.ToInt32(Cirnum.Text)];
                cap_inlet.lenth_cap = new double[Convert.ToInt32(Cirnum.Text)];
                cap_outlet.d_cap = new double[Convert.ToInt32(Cirnum.Text)];
                cap_outlet.lenth_cap = new double[Convert.ToInt32(Cirnum.Text)];
            }

            Model.Basic.CalcResult r=new Model.Basic.CalcResult();
            if (RadioButton_HExType_Condenser.IsChecked == true)
            {
                if (this.RadioButton_xo_Cond.IsChecked ==true) 
                {
                    r = m_Main.main_condenser_inputQ(refInput, airInput, geoInput, CirArrange, NodeInfo, fin_type, tube_type, hex_type, cap_inlet,cap_outlet, Model.HumidAirSourceData.SourceTableData,
                        Convert.ToDouble(Zha.Text), Convert.ToDouble(Zapa.Text), Convert.ToDouble(Zhr.Text), Convert.ToDouble(Zapr.Text)); 
                }
                else if (this.RadioButton_Tro_sub_Cond.IsChecked == true)
                {
                    r = m_Main.main_condenser_inputSC(refInput, airInput, geoInput, CirArrange, NodeInfo, fin_type, tube_type, hex_type, cap_inlet, cap_outlet, Model.HumidAirSourceData.SourceTableData,
                        Convert.ToDouble(Zha.Text), Convert.ToDouble(Zapa.Text), Convert.ToDouble(Zhr.Text), Convert.ToDouble(Zapr.Text));
                }
                else 
                {
                    r = m_Main.main_condenser(refInput, airInput, geoInput, CirArrange, NodeInfo, fin_type, tube_type, hex_type, cap_inlet, cap_outlet, Model.HumidAirSourceData.SourceTableData,
                        Convert.ToDouble(Zha.Text), Convert.ToDouble(Zapa.Text), Convert.ToDouble(Zhr.Text), Convert.ToDouble(Zapr.Text));
                }
            }
            else 
            {
                if (this.RadioButton_xo_Evap.IsChecked ==true)
                {
                    r = m_Main.main_evaporator_inputQ(refInput, airInput, geoInput, CirArrange, NodeInfo, fin_type, tube_type, hex_type, cap_inlet, cap_outlet, Model.HumidAirSourceData.SourceTableData,
                        Convert.ToDouble(Zha.Text), Convert.ToDouble(Zapa.Text), Convert.ToDouble(Zhr.Text), Convert.ToDouble(Zapr.Text));
                }
                else if (this.RadioButton_Tro_sub_Evap.IsChecked == true)
                {
                    r = m_Main.main_evaporator_inputSH(refInput, airInput, geoInput, CirArrange, NodeInfo, fin_type, tube_type, hex_type, cap_inlet, cap_outlet, Model.HumidAirSourceData.SourceTableData,
                        Convert.ToDouble(Zha.Text), Convert.ToDouble(Zapa.Text), Convert.ToDouble(Zhr.Text), Convert.ToDouble(Zapr.Text));
                    //r = m_Main.main_evaporator_inputSH_py(refInput, airInput, geoInput, capInput, coolprop, Model.HumidAirSourceData.SourceTableData);
                }
                else
                {
                    r = m_Main.main_evaporator(refInput, airInput, geoInput, CirArrange, NodeInfo, fin_type, tube_type, hex_type, cap_inlet, cap_outlet, Model.HumidAirSourceData.SourceTableData,
                        Convert.ToDouble(Zha.Text), Convert.ToDouble(Zapa.Text), Convert.ToDouble(Zhr.Text), Convert.ToDouble(Zapr.Text));
                }
            }

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
            this.Canvas_0.Visibility = Visibility.Visible;
            this.Canvas_1.Visibility = Visibility.Collapsed;
            this.Canvas_2.Visibility = Visibility.Collapsed;
            this.Canvas_3.Visibility = Visibility.Collapsed;
            this.Canvas_4.Visibility = Visibility.Collapsed;
            this.Canvas_5.Visibility = Visibility.Collapsed;
            //this.Canvas_6.Visibility = Visibility.Collapsed;
            this.Canvas_7.Visibility = Visibility.Collapsed;
            this.Canvas_8.Visibility = Visibility.Collapsed;
            this.Canvas_9.Visibility = Visibility.Collapsed;
            this.ScrollViewer_0.Visibility = Visibility.Collapsed;

            this.Picture_HExTube.Visibility = Visibility.Collapsed;
            this.Picture_FinType.Visibility = Visibility.Collapsed;
            this.Picture_HExType.Visibility = Visibility.Visible;
            this.Picture_Ref.Visibility = Visibility.Collapsed;
            this.Picture_Wind.Visibility = Visibility.Collapsed;
            //this.GroupBox_ManualArrangeCirnum.Visibility = Visibility.Collapsed;
            this.DockPanel_Picture.Visibility = Visibility.Collapsed;
            
            //调显示宽度
            //this.Grid_Mid.MaxWidth = 450;
            //this.Grid_Mid.Width = new GridLength(450, GridUnitType.Pixel);

            //调显示高度
            //this.Grid_Picture.Height = new GridLength(290, GridUnitType.Pixel);
            //this.ListBox_RealTimeInputShow_Condenser.Height = 414;
            //this.ListBox_RealTimeInputShow_Evaporator.Height = 414;
        }

        private void TreeView_Fin_GotFocus(object sender, RoutedEventArgs e)
        {
            this.Canvas_0.Visibility = Visibility.Collapsed;
            this.Canvas_1.Visibility = Visibility.Collapsed;
            this.Canvas_2.Visibility = Visibility.Visible;
            this.Canvas_3.Visibility = Visibility.Collapsed;
            this.Canvas_4.Visibility = Visibility.Collapsed;
            this.Canvas_5.Visibility = Visibility.Collapsed;
            //this.Canvas_6.Visibility = Visibility.Collapsed;
            this.Canvas_7.Visibility = Visibility.Collapsed;
            this.Canvas_8.Visibility = Visibility.Collapsed;
            this.Canvas_9.Visibility = Visibility.Collapsed;
            this.ScrollViewer_0.Visibility = Visibility.Collapsed;

            this.Picture_HExTube.Visibility = Visibility.Collapsed;
            this.Picture_FinType.Visibility = Visibility.Visible;
            this.Picture_HExType.Visibility = Visibility.Collapsed;
            this.Picture_Ref.Visibility = Visibility.Collapsed;
            this.Picture_Wind.Visibility = Visibility.Collapsed;
            //this.GroupBox_ManualArrangeCirnum.Visibility = Visibility.Collapsed;
            this.DockPanel_Picture.Visibility = Visibility.Collapsed;

            //调显示宽度
            //this.Grid_Mid.MaxWidth = 450;
            //this.Grid_Mid.Width = new GridLength(450, GridUnitType.Pixel);

            //调显示高度
            //this.Grid_Picture.Height = new GridLength(290, GridUnitType.Pixel);
            //this.ListBox_RealTimeInputShow_Condenser.Height = 414;
            //this.ListBox_RealTimeInputShow_Evaporator.Height = 414;
        }

        private void TreeView_Pass_GotFocus(object sender, RoutedEventArgs e)
        {
            this.Canvas_0.Visibility = Visibility.Collapsed;
            this.Canvas_1.Visibility = Visibility.Collapsed;
            this.Canvas_2.Visibility = Visibility.Collapsed;
            this.Canvas_3.Visibility = Visibility.Collapsed;
            this.Canvas_4.Visibility = Visibility.Collapsed;
            this.Canvas_5.Visibility = Visibility.Visible;
            //this.Canvas_6.Visibility = Visibility.Collapsed;
            this.Canvas_7.Visibility = Visibility.Collapsed;
            this.Canvas_8.Visibility = Visibility.Collapsed;
            this.Canvas_9.Visibility = Visibility.Collapsed;
            this.ScrollViewer_0.Visibility = Visibility.Collapsed;

            if (RadioButton_ManualArrange.IsChecked == true)
            {
                this.Picture_HExTube.Visibility = Visibility.Collapsed;
                this.Picture_FinType.Visibility = Visibility.Collapsed;
                this.Picture_HExType.Visibility = Visibility.Collapsed;
                this.Picture_Ref.Visibility = Visibility.Collapsed;
                this.Picture_Wind.Visibility = Visibility.Collapsed;
                //this.GroupBox_ManualArrangeCirnum.Visibility = Visibility.Collapsed;
                this.DockPanel_Picture.Visibility = Visibility.Visible;

                this.StackPanel_ManualArrangeCirnum.Visibility = Visibility.Visible;
                //this.GroupBox_ManualArrangeCirnum.Header = "手动分配流路";
                TextBlock_AirFlow.Text = AirFlowString;

                //调显示高度
                //this.Grid_Picture.Height = new GridLength(450, GridUnitType.Pixel);
                //this.ListBox_RealTimeInputShow_Condenser.Height = 254;
                //this.ListBox_RealTimeInputShow_Evaporator.Height = 254;
            }
            else 
            {
                this.Picture_HExTube.Visibility = Visibility.Collapsed;
                this.Picture_FinType.Visibility = Visibility.Collapsed;
                this.Picture_HExType.Visibility = Visibility.Hidden;
                this.Picture_Ref.Visibility = Visibility.Collapsed;
                this.Picture_Wind.Visibility = Visibility.Collapsed;
                //this.GroupBox_ManualArrangeCirnum.Visibility = Visibility.Collapsed;
                this.DockPanel_Picture.Visibility = Visibility.Collapsed;

                //调显示高度
                //this.Grid_Picture.Height = new GridLength(290, GridUnitType.Pixel);
                //this.ListBox_RealTimeInputShow_Condenser.Height = 414;
                //this.ListBox_RealTimeInputShow_Evaporator.Height = 414;
            }
            //调显示宽度
            //this.Grid_Mid.MaxWidth = 450;
            //this.Grid_Mid.Width = new GridLength(450, GridUnitType.Pixel);

            //生成手动连流路图
            /*int _row = Convert.ToInt32(Row.Text);
            int _tube = Convert.ToInt32(tube_per.Text);
            vm.Connectors.Clear();
            vm.Capillaries.Clear();
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
            };*/
        }

        private void TreeView_Ref_GotFocus(object sender, RoutedEventArgs e)
        {
            this.Canvas_0.Visibility = Visibility.Collapsed;
            this.Canvas_1.Visibility = Visibility.Collapsed;
            this.Canvas_2.Visibility = Visibility.Collapsed;
            this.Canvas_3.Visibility = Visibility.Visible;
            this.Canvas_4.Visibility = Visibility.Collapsed;
            this.Canvas_5.Visibility = Visibility.Collapsed;
            //this.Canvas_6.Visibility = Visibility.Collapsed;
            this.Canvas_7.Visibility = Visibility.Collapsed;
            this.Canvas_8.Visibility = Visibility.Collapsed;
            this.Canvas_9.Visibility = Visibility.Collapsed;
            this.ScrollViewer_0.Visibility = Visibility.Collapsed;

            this.Picture_HExTube.Visibility = Visibility.Collapsed;
            this.Picture_FinType.Visibility = Visibility.Collapsed;
            this.Picture_HExType.Visibility = Visibility.Collapsed;
            this.Picture_Ref.Visibility = Visibility.Visible;
            this.Picture_Wind.Visibility = Visibility.Collapsed;
            //this.GroupBox_ManualArrangeCirnum.Visibility = Visibility.Collapsed;
            this.DockPanel_Picture.Visibility = Visibility.Collapsed;

            //调显示宽度
            //this.Grid_Mid.MaxWidth = 450;
            //this.Grid_Mid.Width = new GridLength(450, GridUnitType.Pixel);

            //调显示高度
            //this.Grid_Picture.Height = new GridLength(290, GridUnitType.Pixel);
            //this.ListBox_RealTimeInputShow_Condenser.Height = 414;
            //this.ListBox_RealTimeInputShow_Evaporator.Height = 414;
        }

        private void TreeView_Wind_GotFocus(object sender, RoutedEventArgs e)
        {
            this.Canvas_0.Visibility = Visibility.Collapsed;
            this.Canvas_1.Visibility = Visibility.Collapsed;
            this.Canvas_2.Visibility = Visibility.Collapsed;
            this.Canvas_3.Visibility = Visibility.Collapsed;
            this.Canvas_4.Visibility = Visibility.Visible;
            this.Canvas_5.Visibility = Visibility.Collapsed;
            //this.Canvas_6.Visibility = Visibility.Collapsed;
            this.Canvas_7.Visibility = Visibility.Collapsed;
            this.Canvas_8.Visibility = Visibility.Collapsed;
            this.Canvas_9.Visibility = Visibility.Collapsed;
            this.ScrollViewer_0.Visibility = Visibility.Collapsed;

            this.Picture_HExTube.Visibility = Visibility.Collapsed;
            this.Picture_FinType.Visibility = Visibility.Collapsed;
            this.Picture_HExType.Visibility = Visibility.Collapsed;
            this.Picture_Ref.Visibility = Visibility.Collapsed;
            this.Picture_Wind.Visibility = Visibility.Visible;
            //this.GroupBox_ManualArrangeCirnum.Visibility = Visibility.Collapsed;
            this.DockPanel_Picture.Visibility = Visibility.Collapsed;

            //调显示宽度
            //this.Grid_Mid.MaxWidth = 450;
            //this.Grid_Mid.Width = new GridLength(450, GridUnitType.Pixel);        
             

            //调显示高度
            //this.Grid_Picture.Height = new GridLength(290, GridUnitType.Pixel);
            //this.ListBox_RealTimeInputShow_Condenser.Height = 414;
            //this.ListBox_RealTimeInputShow_Evaporator.Height = 414;

        }

        private void TreeView_Result_GotFocus(object sender, RoutedEventArgs e)
        {
            this.Canvas_0.Visibility = Visibility.Collapsed;
            this.Canvas_1.Visibility = Visibility.Collapsed;
            this.Canvas_2.Visibility = Visibility.Collapsed;
            this.Canvas_3.Visibility = Visibility.Collapsed;
            this.Canvas_4.Visibility = Visibility.Collapsed;
            this.Canvas_5.Visibility = Visibility.Collapsed;
            //this.Canvas_6.Visibility = Visibility.Collapsed;
            this.Canvas_7.Visibility = Visibility.Collapsed;
            this.Canvas_8.Visibility = Visibility.Visible;
            this.Canvas_9.Visibility = Visibility.Collapsed;
            this.ScrollViewer_0.Visibility = Visibility.Collapsed;
            

            this.Picture_HExTube.Visibility = Visibility.Collapsed;
            this.Picture_FinType.Visibility = Visibility.Collapsed;
            this.Picture_HExType.Visibility = Visibility.Collapsed;
            this.Picture_Ref.Visibility = Visibility.Collapsed;
            this.Picture_Wind.Visibility = Visibility.Collapsed;
            //this.GroupBox_ManualArrangeCirnum.Visibility = Visibility.Collapsed;
            this.DockPanel_Picture.Visibility = Visibility.Visible;

            //调显示宽度
            //this.Grid_Mid.MaxWidth = 450;
            //this.Grid_Mid.Width = new GridLength(450, GridUnitType.Pixel);                       

            //调显示高度
            //this.Grid_Picture.Height = new GridLength(450, GridUnitType.Pixel);

            //this.ListBox_RealTimeInputShow_Condenser.Height = 254;
            //this.ListBox_RealTimeInputShow_Evaporator.Height = 254;

            //this.GroupBox_ManualArrangeCirnum.Header = "流路分配";
            this.StackPanel_ManualArrangeCirnum.Visibility = Visibility.Collapsed;  
        }

        private void TreeView_DetailResult_GotFocus(object sender, RoutedEventArgs e)
        {
            this.Canvas_0.Visibility = Visibility.Collapsed;
            this.Canvas_1.Visibility = Visibility.Collapsed;
            this.Canvas_2.Visibility = Visibility.Collapsed;
            this.Canvas_3.Visibility = Visibility.Collapsed;
            this.Canvas_4.Visibility = Visibility.Collapsed;
            this.Canvas_5.Visibility = Visibility.Collapsed;
            //this.Canvas_6.Visibility = Visibility.Collapsed;
            this.Canvas_7.Visibility = Visibility.Collapsed;
            this.Canvas_8.Visibility = Visibility.Collapsed;
            this.Canvas_9.Visibility = Visibility.Visible;
            this.ScrollViewer_0.Visibility = Visibility.Visible;

            this.Picture_HExTube.Visibility = Visibility.Collapsed;
            this.Picture_FinType.Visibility = Visibility.Collapsed;
            this.Picture_HExType.Visibility = Visibility.Collapsed;
            this.Picture_Ref.Visibility = Visibility.Collapsed;
            this.Picture_Wind.Visibility = Visibility.Collapsed;
            //this.GroupBox_ManualArrangeCirnum.Visibility = Visibility.Collapsed;
            this.DockPanel_Picture.Visibility = Visibility.Visible;

            //调显示宽度
            //this.Grid_Mid.MaxWidth = 650;
            //this.Grid_Mid.Width = new GridLength(650, GridUnitType.Pixel);

            //调显示高度
            //this.Grid_Picture.Height = new GridLength(450, GridUnitType.Pixel);

            //this.ListBox_RealTimeInputShow_Condenser.Height = 254;
            //this.ListBox_RealTimeInputShow_Evaporator.Height = 254;

            //this.GroupBox_ManualArrangeCirnum.Header = "流路分配";
            this.StackPanel_ManualArrangeCirnum.Visibility = Visibility.Collapsed;

            if (flag_Calculated == true)
            {
                peopleList.Clear();
                dataGrid_Result.ItemsSource = null;
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

                for (int k = 1; k < Tube_row_int * Row_int + 1; k++)
                {
                    int i = k % Tube_row_int == 0 ? Tube_row_int - 1 : k % Tube_row_int - 1;
                    int j = 0;
                    if(AirFlowDirection==0)
                    {
                        j = k % Tube_row_int == 0 ? k / Tube_row_int - 1 : k / Tube_row_int;
                    }
                    else if(AirFlowDirection==1)
                    {
                        j = k % Tube_row_int == 0 ?Row_int- k / Tube_row_int : Row_int-1- k / Tube_row_int;
                    }
                    
                    peopleList.Add(new people()
                    {
                        //tube = Convert.ToString(i + 1),
                        //row = Convert.ToString(j + 1),
                        TubeNumber = k.ToString(),
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

                //((this.FindName("dataGrid_Result")) as DataGrid).ItemsSource = peopleList;
                dataGrid_Result.ItemsSource = peopleList;                
                flag_Calculated = false;
            }

        }


        private void TreeView_AdjustParameter_GotFocus(object sender, RoutedEventArgs e)
        {
            this.Canvas_0.Visibility = Visibility.Collapsed;
            this.Canvas_1.Visibility = Visibility.Collapsed;
            this.Canvas_2.Visibility = Visibility.Collapsed;
            this.Canvas_3.Visibility = Visibility.Collapsed;
            this.Canvas_4.Visibility = Visibility.Collapsed;
            this.Canvas_5.Visibility = Visibility.Collapsed;
            //this.Canvas_6.Visibility = Visibility.Collapsed;
            this.Canvas_7.Visibility = Visibility.Visible;
            this.Canvas_8.Visibility = Visibility.Collapsed;
            this.Canvas_9.Visibility = Visibility.Collapsed;

            this.Picture_HExTube.Visibility = Visibility.Collapsed;
            this.Picture_FinType.Visibility = Visibility.Collapsed;
            this.Picture_HExType.Visibility = Visibility.Hidden;
            this.Picture_Ref.Visibility = Visibility.Collapsed;
            this.Picture_Wind.Visibility = Visibility.Collapsed;
            //this.GroupBox_ManualArrangeCirnum.Visibility = Visibility.Collapsed;
            this.DockPanel_Picture.Visibility = Visibility.Collapsed;
            this.ScrollViewer_0.Visibility = Visibility.Collapsed;

            //调显示宽度
            //this.Grid_Mid.MaxWidth = 450;
            //this.Grid_Mid.Width = new GridLength(450, GridUnitType.Pixel);

            //调显示高度
            //this.Grid_Picture.Height = new GridLength(290, GridUnitType.Pixel);

            //this.ListBox_RealTimeInputShow_Condenser.Height = 414;
            //this.ListBox_RealTimeInputShow_Evaporator.Height = 414;
        }

        private void TreeView_Distributer_GotFocus(object sender, RoutedEventArgs e)
        {
            this.Canvas_0.Visibility = Visibility.Collapsed;
            this.Canvas_1.Visibility = Visibility.Collapsed;
            this.Canvas_2.Visibility = Visibility.Collapsed;
            this.Canvas_3.Visibility = Visibility.Collapsed;
            this.Canvas_4.Visibility = Visibility.Collapsed;
            this.Canvas_5.Visibility = Visibility.Collapsed;
            //this.Canvas_6.Visibility = Visibility.Visible;
            this.Canvas_7.Visibility = Visibility.Collapsed;
            this.Canvas_8.Visibility = Visibility.Collapsed;
            this.Canvas_9.Visibility = Visibility.Collapsed;

            this.Picture_HExTube.Visibility = Visibility.Collapsed;
            this.Picture_FinType.Visibility = Visibility.Collapsed;
            this.Picture_HExType.Visibility = Visibility.Hidden;
            this.Picture_Ref.Visibility = Visibility.Collapsed;
            this.Picture_Wind.Visibility = Visibility.Collapsed;
            //this.GroupBox_ManualArrangeCirnum.Visibility = Visibility.Collapsed;
            this.DockPanel_Picture.Visibility = Visibility.Collapsed;

            //调显示宽度
            //this.Grid_Mid.MaxWidth = 450;
            //this.Grid_Mid.Width = new GridLength(450, GridUnitType.Pixel);

            //调显示高度
            //this.Grid_Picture.Height = new GridLength(450, GridUnitType.Pixel);

            //this.ListBox_RealTimeInputShow_Condenser.Height = 254;
            //this.ListBox_RealTimeInputShow_Evaporator.Height = 254;
        }

        private void TreeView_HExTube_GotFocus(object sender, RoutedEventArgs e)
        {
            this.Canvas_0.Visibility = Visibility.Collapsed;
            this.Canvas_1.Visibility = Visibility.Visible;
            this.Canvas_2.Visibility = Visibility.Collapsed;
            this.Canvas_3.Visibility = Visibility.Collapsed;
            this.Canvas_4.Visibility = Visibility.Collapsed;
            this.Canvas_5.Visibility = Visibility.Collapsed;
            //this.Canvas_6.Visibility = Visibility.Collapsed;
            this.Canvas_7.Visibility = Visibility.Collapsed;
            this.Canvas_8.Visibility = Visibility.Collapsed;
            this.Canvas_9.Visibility = Visibility.Collapsed;

            this.Picture_HExTube.Visibility = Visibility.Visible;
            this.Picture_FinType.Visibility = Visibility.Collapsed;
            this.Picture_HExType.Visibility = Visibility.Collapsed;
            this.Picture_Ref.Visibility = Visibility.Collapsed;
            this.Picture_Wind.Visibility = Visibility.Collapsed;
            //this.GroupBox_ManualArrangeCirnum.Visibility = Visibility.Collapsed;
            this.DockPanel_Picture.Visibility = Visibility.Collapsed;

            //调显示宽度
            //this.Grid_Mid.MaxWidth = 450;
            //this.Grid_Mid.Width = new GridLength(450, GridUnitType.Pixel);

            //调显示高度
            //this.Grid_Picture.Height = new GridLength(290, GridUnitType.Pixel);

            //this.ListBox_RealTimeInputShow_Condenser.Height = 414;
            //this.ListBox_RealTimeInputShow_Evaporator.Height = 414;
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
            if (this.Velocity_ai.IsEnabled == true)
            {
                this.Velocity_ai.IsEnabled = false;
                this.Velocity_ai.Text = "1";
            }
        }

        private void RadioButton_AirVelocity_Checked(object sender, RoutedEventArgs e)
        {
            this.Velocity_ai.IsEnabled = true;
            if (this.Va.IsEnabled == true)
            {
                this.Va.IsEnabled = false;
                this.Va.Text = "0.12";
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

        private void RadioButton_Tcro_Evap_Checked(object sender, RoutedEventArgs e)//saturation temperature
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
            this.Tro_sub_Evap.IsEnabled = true;
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
            if (this.Tro_sub_Evap.IsEnabled == true)
            {
                this.Tro_sub_Evap.IsEnabled = false;
                this.Tro_sub_Evap.Text = "0";
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
            if (this.Tro_sub_Evap.IsEnabled == true)
            {
                this.Tro_sub_Evap.IsEnabled = false;
                this.Tro_sub_Evap.Text = "0";
            }
            if (this.xo_Evap.IsEnabled == true)
            {
                this.xo_Evap.IsEnabled = false;
                this.xo_Evap.Text = "0";
            }
        }

        private void RadioButton_mro_Cond_Checked(object sender, RoutedEventArgs e)
        {
            this.mro_Cond.IsEnabled = true;
            if (this.xo_Cond.IsEnabled == true)
            {
                this.xo_Cond.IsEnabled = false;
                this.xo_Cond.Text = "0";
            }
            if (this.Tro_sub_Cond.IsEnabled == true)
            {
                this.Tro_sub_Cond.IsEnabled = false;
                this.Tro_sub_Cond.Text = "0";
            }
        }

        private void RadioButton_xo_Cond_Checked(object sender, RoutedEventArgs e)
        {
            this.xo_Cond.IsEnabled = true;
            if (this.mro_Cond.IsEnabled == true)
            {
                this.mro_Cond.IsEnabled = false;
                this.mro_Cond.Text = "0.01";
            }
            if (this.Tro_sub_Cond.IsEnabled == true)
            {
                this.Tro_sub_Cond.IsEnabled = false;
                this.Tro_sub_Cond.Text = "0";
            }
        }

        private void RadioButton_Tro_Cond_Checked(object sender, RoutedEventArgs e)
        {
            this.Tro_sub_Cond.IsEnabled = true;
            if (this.mro_Cond.IsEnabled == true)
            {
                this.mro_Cond.IsEnabled = false;
                this.mro_Cond.Text = "0.01";
            }
            if (this.xo_Cond.IsEnabled == true)
            {
                this.xo_Cond.IsEnabled = false;
                this.xo_Cond.Text = "0";
            }
        }

        private void RadioButton_ManualArrange_Checked(object sender, RoutedEventArgs e)
        {
            //this.GroupBox_AutoArrangeCirnum.Visibility = Visibility.Hidden;
            //this.GroupBox_ManualArrangeCirnum.Visibility = Visibility.Visible;

            this.Picture_HExTube.Visibility = Visibility.Collapsed;
            this.Picture_FinType.Visibility = Visibility.Collapsed;
            this.Picture_HExType.Visibility = Visibility.Collapsed;
            this.Picture_Ref.Visibility = Visibility.Collapsed;
            this.Picture_Wind.Visibility = Visibility.Collapsed;
            //this.GroupBox_ManualArrangeCirnum.Visibility = Visibility.Collapsed;
            this.DockPanel_Picture.Visibility = Visibility.Visible;
            this.StackPanel_ManualArrangeCirnum.Visibility = Visibility.Visible;

            this.ToggleButton1.Visibility = Visibility.Visible;
            this.Button_ReArrange.Visibility = Visibility.Visible;
            this.Button_Delete.Visibility = Visibility.Visible;
            this.Button_CopyCircuit.Visibility = Visibility.Visible;

            Cirnum.IsEnabled = false;
            Pass_OK.IsEnabled = false;

            //调显示高度
            CircuitIndex = 0;

        }

        private void RadioButton_AutoArrange_Checked(object sender, RoutedEventArgs e)
        {
            Cirnum.IsEnabled = true;
            Pass_OK.IsEnabled = true;

            this.Picture_HExTube.Visibility = Visibility.Collapsed;
            this.Picture_FinType.Visibility = Visibility.Collapsed;
            this.Picture_HExType.Visibility = Visibility.Collapsed;
            this.Picture_Ref.Visibility = Visibility.Collapsed;
            this.Picture_Wind.Visibility = Visibility.Collapsed;
            //this.DockPanel_Picture.Visibility = Visibility.Hidden;

            this.ToggleButton1.Visibility = Visibility.Hidden;
            this.Button_ReArrange.Visibility = Visibility.Hidden;
            this.Button_Delete.Visibility = Visibility.Hidden;
            this.Button_CopyCircuit.Visibility = Visibility.Hidden;

            //调显示高度
            ListBox_RealTimeInputShow_Condenser.Height = 450;
            ListBox_RealTimeInputShow_Evaporator.Height = 450;
            CircuitIndex = 1;
        }

        //CheckBox*********************************************************************************************************End

        private void HExType_Condenser(object sender, RoutedEventArgs e)
        {
            ComboBox_hextype.Text = "冷凝器";
            this.GroupBox_RefInlet_Evaporator.Visibility = Visibility.Hidden;
            this.GroupBox_RefOutlet_Evaporator.Visibility = Visibility.Hidden;
            this.GroupBox_RefInlet_Condenser.Visibility = Visibility.Visible;
            this.GroupBox_RefOutlet_Condenser.Visibility = Visibility.Visible;
            this.ListBox_RealTimeInputShow_Condenser.Visibility = Visibility.Visible;
            this.ListBox_RealTimeInputShow_Evaporator.Visibility = Visibility.Hidden;
            this.Canvas_0.Visibility = Visibility.Visible;
            this.Tri_ValveBefore.Text="24";
            this.Pri_ValveBefore.Text = "1729.2";

            this.Picture_HExTube.Visibility = Visibility.Collapsed;
            this.Picture_FinType.Visibility = Visibility.Collapsed;
            this.Picture_HExType.Visibility = Visibility.Visible;
            this.Picture_Ref.Visibility = Visibility.Collapsed;
            this.Picture_Wind.Visibility = Visibility.Collapsed;
            //this.GroupBox_ManualArrangeCirnum.Visibility = Visibility.Collapsed;
            this.DockPanel_Picture.Visibility = Visibility.Collapsed;
        }

        private void HExType_Evaporator(object sender, RoutedEventArgs e)
        {
            ComboBox_hextype.Text = "蒸发器";
            this.GroupBox_RefInlet_Evaporator.Visibility = Visibility.Visible;
            this.GroupBox_RefOutlet_Evaporator.Visibility = Visibility.Visible;
            this.GroupBox_RefInlet_Condenser.Visibility = Visibility.Hidden;
            this.GroupBox_RefOutlet_Condenser.Visibility = Visibility.Hidden;
            this.ListBox_RealTimeInputShow_Condenser.Visibility = Visibility.Hidden;
            this.ListBox_RealTimeInputShow_Evaporator.Visibility = Visibility.Visible;
            this.Canvas_0.Visibility = Visibility.Visible;
            this.Tri_ValveBefore.Text = "20";
            this.Pri_ValveBefore.Text = "1842.28";

            this.Picture_HExTube.Visibility = Visibility.Collapsed;
            this.Picture_FinType.Visibility = Visibility.Collapsed;
            this.Picture_HExType.Visibility = Visibility.Visible;
            this.Picture_Ref.Visibility = Visibility.Collapsed;
            this.Picture_Wind.Visibility = Visibility.Collapsed;
            this.Picture_Wind.Visibility = Visibility.Collapsed;
            //this.GroupBox_ManualArrangeCirnum.Visibility = Visibility.Collapsed;
            this.DockPanel_Picture.Visibility = Visibility.Collapsed;
        }

        #region 非均匀风速分布

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
        private void dtgShow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var uie = e.OriginalSource as UIElement;
            if (e.Key == Key.Enter)
            {
                uie.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                e.Handled = true;
            }
        }

        //---取得选中 Cell 所在的行列
        private bool GetCellXY(System.Windows.Controls.DataGrid dg, ref int rowIndex, ref int columnIndex)
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
        private string GetSelectedCellsValue(System.Windows.Controls.DataGrid dg)
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
                    Binding = new System.Windows.Data.Binding("[{i.ToString()}]")
                });
            }

            int num_row = Convert.ToInt32(Row_Num.Text);
            for (int i = 0; i < num_row; i++)
            {
                list.Add(new int[Convert.ToInt32(Col_Num.Text)]);
                showdata.Insert(0, new int[dtgShow.Columns.Count]);
            }

        }


        private void Get_Value(object sender, RoutedEventArgs e)
        {
            int dtg_col = dtgShow.Columns.Count;
            int dtg_row = dtgShow.Items.Count;//DataGrid居然是用Items表示行数
            string ss = (dtgShow.Columns[dtg_col-1].GetCellContent(dtgShow.Items[dtg_row-1]) as TextBlock).Text;//最大行最大列对应数据
            System.Windows.MessageBox.Show(dtg_col.ToString());
            System.Windows.MessageBox.Show(dtg_row.ToString());
            System.Windows.MessageBox.Show(ss);
        }
        //非均匀风速分布End*****************************************************************************************************************************End
        #endregion

        //DetailResult******************************************************************************************************Start

        //people类
        public class people
        {
            public string TubeNumber { get; set; }
            //public string row { get; set; }
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

                //表头
                row["参数"] = "参数";
                row["数值"] = "数值";
                row["单位"] = "单位";
                dt.Rows.Add(row);

                //综合仿真结果
                row = dt.NewRow();
                row["参数"] = "换热量";
                row["数值"] = Q_inter;
                row["单位"] = "kW";
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
                row["数值"] = mr_inter;
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

                string localFilePath, fileNameExt, FilePath;
                SaveFileDialog sfd = new SaveFileDialog();

                //设置文件类型 
                sfd.Filter = "Excel文件(*.xlsx)|*.xlsx|Excel文件(*.xls)|*.xls";

                //默认文件名
                if (this.RadioButton_HExType_Condenser.IsChecked == true)
                {
                    sfd.FileName = "Cond "+DateTime.Now.ToString("yyyyMMddHHmmss") + "_Mcoil输出" + ".xlsx";
                }
                else 
                {
                    sfd.FileName = "Evap " + DateTime.Now.ToString("yyyyMMddHHmmss") + "_Mcoil输出" + ".xlsx";
                }

                //点了保存按钮进入 
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    localFilePath = sfd.FileName.ToString(); //获得文件路径 
                    fileNameExt = localFilePath.Substring(localFilePath.LastIndexOf("\\") + 1); //获取文件名，不带路径

                    //获取文件路径，不带文件名 
                    FilePath = localFilePath.Substring(0, localFilePath.LastIndexOf("\\")) +"\\"+ fileNameExt;
                    excelWB.SaveAs(localFilePath);  //将其进行保存到指定的路径
                    excelWB.Close();
                    excelApp.Quit();  //KillAllExcel(excelApp); 释放可能还没释放的进程
                    System.Windows.MessageBox.Show("文件保存成功！");
                }

            }

            //DetailResult******************************************************************************************************End

            private void Button_Click_Sure(object sender, RoutedEventArgs e)
            {
                int _row = Convert.ToInt32(Row.Text);
                int _tube = Convert.ToInt32(tube_per.Text);
                vm.Connectors.Clear();
                vm.Capillaries.Clear();
                node1 = null;
                rect = null;
                vm.GenerateNode(_row, _tube);
                vm.Rects[0].X = vm.Nodes[0].X - vm.RowPitch + 5;
                vm.Rects[0].Y = vm.Nodes[0].Y;
                vm.Rects[0].RectHeight = (_tube - 1) * vm.TubePitch + 20;
                vm.Rects[0].FullLine = true;
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
            private bool rect_start;
            private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                //string msg="Successful";
                //MessageBox.Show(msg);
                if(vm.CreatNewConnector)
                {
                    this.ToggleButton1.Content = "正在流路连接";
                    var item = ListBox_this.SelectedItem;
                    if(item==null)
                    {
                        rect_start = false;
                    }
                    else if(item!=null)
                    {
                        if (item.GetType().Name == "Rect")
                        {
                            rect = item as Rect;
                            if (rect.X == vm.Rects[0].X && rect.Y == vm.Rects[0].Y)//check selected item is Rect[0] or not
                            {
                                rect_start = true;
                                ViewModel.Circuit_Num++;//防止自动分配流路与手动分配流路切换后，会出现两个流路共用一个流路号
                            }
                            else if (rect_start && node1 != null && node1.ConnectNum==1)
                            {
                                Capillary newcapillary = new Capillary();
                                newcapillary.Start = node1;
                                newcapillary.End = rect;
                                newcapillary.FullLine = node1.FullLine;
                                newcapillary.In = false;
                                rect.FullLine = node1.FullLine;
                                vm.Capillaries.Where(x => x.Start == newcapillary.Start && x.End == newcapillary.End).ToList().ForEach(x => vm.Capillaries.Remove(x));
                                vm.Capillaries.Add(newcapillary);

                                //Canvas_Picture.RegisterName("New_End_Capillary/" + ViewModel.End_Capillary_Num + "/" + ViewModel.Circuit_Num, newcapillary);//注册名字，以便以后使用
                                if (rect.GetHashCode() != vm.Rects[0].GetHashCode() && rect.GetHashCode() != vm.Rects[1].GetHashCode())
                                {
                                    newcapillary.Name = Convert.ToString("New_End_Capillary/CapillaryNum" + ViewModel.End_Capillary_Num + "/CircuitNum" + ViewModel.Circuit_Num);
                                    //System.Windows.MessageBox.Show(newcapillary.Name);
                                    var selectele_rect = rect as Rect;
                                    string circuitnum_str = "";

                                    circuitnum_str = selectele_rect.Name.Substring(0, selectele_rect.Name.LastIndexOf("HasCircuitNum") + 13);
                                    int lenth_circuitnum_str = selectele_rect.Name.Substring(0, selectele_rect.Name.LastIndexOf("HasCircuitNum") + 13).Length - selectele_rect.Name.Length;

                                    if (lenth_circuitnum_str != 0)//第一次连的集管会等于0
                                    {
                                        circuitnum_str = selectele_rect.Name.Substring(selectele_rect.Name.LastIndexOf("HasCircuitNum") + 14, (selectele_rect.Name.Length - (selectele_rect.Name.LastIndexOf("HasCircuitNum") + 14)));

                                        string[] sp = circuitnum_str.ToString().Split(new string[] { "," }, StringSplitOptions.None);

                                        for (int list_circuit = 0; list_circuit < sp.Length; list_circuit++)
                                        {
                                            int circuitnum = Convert.ToInt32(sp[list_circuit]);
                                            if (circuitnum == ViewModel.Circuit_Num)
                                            {
                                                break;
                                            }
                                            else if (list_circuit == sp.Length - 1 && circuitnum != ViewModel.Circuit_Num)
                                            {
                                                rect.Name = rect.Name + Convert.ToString("," + ViewModel.Circuit_Num);
                                            }
                                        }
                                    }
                                    else 
                                    {
                                        rect.Name = rect.Name + Convert.ToString("," + ViewModel.Circuit_Num);
                                    }

                                }
                                else if (rect.GetHashCode() == vm.Rects[1].GetHashCode())
                                {
                                    newcapillary.Name = Convert.ToString("New_End_Capillary/CapillaryNum" + ViewModel.End_Capillary_Num + "/CircuitNum" + ViewModel.Circuit_Num);
                                }

                                ViewModel.List_Controls.Add(newcapillary);
                                //由于先储存了Rect才连线生成Capiliary，Capiliary后储存，所以调换一下位置
                                if (ViewModel.List_Controls[ViewModel.List_Controls.Count - 2].GetType().Name == "Rect")
                                {
                                    object obj = ViewModel.List_Controls[ViewModel.List_Controls.Count - 1];
                                    ViewModel.List_Controls[ViewModel.List_Controls.Count - 1] = ViewModel.List_Controls[ViewModel.List_Controls.Count - 2];
                                    ViewModel.List_Controls[ViewModel.List_Controls.Count - 2] = obj;//Rect存放的位置
                                }

                                ////对一个流路里的Node着色
                                //for (int i_vm_connectors = 0; i_vm_connectors < vm.Connectors.Count; i_vm_connectors++)
                                //{
                                //    string _circuitnum_str = vm.Connectors[i_vm_connectors].Name.Substring(vm.Connectors[i_vm_connectors].Name.LastIndexOf("CircuitNum") + 10, (vm.Connectors[i_vm_connectors].Name.Length - (vm.Connectors[i_vm_connectors].Name.LastIndexOf("CircuitNum") + 10)));//截取Connector所在流路
                                //    int _circuitnum = Convert.ToInt32(_circuitnum_str);

                                //    if (_circuitnum == ViewModel.Circuit_Num)
                                //    {
                                //        ;
                                //    }
                                //}

                                ViewModel.End_Capillary_Num++;
                                //ViewModel.Circuit_Num++;

                                if (node1.Y < rect.Y)//change rect height
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

                                node1.ConnectNum--;
                                node1 = null;
                            }
                            if (rect.X == vm.Rects[1].X && rect.Y == vm.Rects[1].Y)
                            {
                                rect_start = false;
                                rect = null;
                            }
                        }
                        else if(item.GetType().Name=="Node")
                        {
                            node2 = item as Node;
                            //if(rect_start&&node1!=null&&node1!=node2&&node2.Full==false)
                            if (rect_start && node1 != null && node1 != node2 && node1.ConnectNum != 0 && node2.ConnectNum!=0)
                            {
                                Connector newLine = new Connector();
                                newLine.Start = node1;
                                newLine.End = node2;
                                newLine.FullLine = node1.FullLine;
                                vm.Connectors.Add(newLine);

                                //Canvas_Picture.RegisterName("NewLine/" + ViewModel.Connector_Num + "/" + ViewModel.Circuit_Num, newLine);//注册名字，以便以后使用
                                newLine.Name = Convert.ToString("NewConnector/ConnectorNum" + ViewModel.Connector_Num + "/CircuitNum" + ViewModel.Circuit_Num);
                                //System.Windows.MessageBox.Show(newLine.Name);

                                ViewModel.Connector_Num++;
                                //Connector.List_Connector.Add(newLine);
                                ViewModel.List_Controls.Add(newLine);
                                //ViewModel.List_Controls.Add(Connector.List_Connector);

                                node2.FullLine = newLine.FullLine ? false : true;
                                node2.Full = true;
                                node1.ConnectNum--;
                                node2.ConnectNum--;
                                node1 = node2;
                            }
                            //else if(rect_start&&rect!=null&&node2.Full==false)
                            else if (rect_start && rect != null && node2.ConnectNum==2)
                            {
                                Capillary newcapillary = new Capillary();
                                newcapillary.Start = node2;
                                newcapillary.End = rect;
                                vm.Capillaries.Where(x => x.Start == newcapillary.Start && x.End == newcapillary.End).ToList().ForEach(x => vm.Capillaries.Remove(x));//delete same capillary
                                vm.Capillaries.Add(newcapillary);


                                if (rect.GetHashCode() != vm.Rects[0].GetHashCode() && rect.GetHashCode() != vm.Rects[1].GetHashCode())
                                {
                                    ViewModel.Circuit_Num++;/////////////////////////////////////////////////////////////////////////////////////////////////////
                                    newcapillary.Name = Convert.ToString("New_Start_Capillary/StartCapillaryNum" + ViewModel.Start_Capillary_Num + "/CircuitNum" + ViewModel.Circuit_Num);
                                    //System.Windows.MessageBox.Show(newcapillary.Name);
                                    var selectele_rect = rect as Rect;
                                    string circuitnum_str = selectele_rect.Name.Substring(selectele_rect.Name.LastIndexOf("HasCircuitNum") + 14, (selectele_rect.Name.Length - (selectele_rect.Name.LastIndexOf("HasCircuitNum") + 14)));
                                    string[] sp = circuitnum_str.ToString().Split(new string[] { "," }, StringSplitOptions.None);

                                    for (int list_circuit = 0; list_circuit < sp.Length; list_circuit++)
                                    {
                                        int circuitnum = Convert.ToInt32(sp[list_circuit]);
                                        if (circuitnum == ViewModel.Circuit_Num)
                                        {
                                            break;
                                        }
                                        else if (list_circuit == sp.Length - 1 && circuitnum != ViewModel.Circuit_Num)
                                        {
                                            rect.Name = rect.Name + Convert.ToString("," + ViewModel.Circuit_Num);
                                        }
                                    }
                                }
                                else if (rect.GetHashCode() == vm.Rects[0].GetHashCode())
                                {
                                    newcapillary.Name = Convert.ToString("New_Start_Capillary/StartCapillaryNum" + ViewModel.Start_Capillary_Num + "/CircuitNum" + ViewModel.Circuit_Num); 
                                }

                                //Canvas_Picture.RegisterName("New_Start_Capillary," + ViewModel.Start_Capillary_Num + "," + ViewModel.Circuit_Num, newcapillary);//注册名字，以便以后使用
                                //newcapillary.Name = Convert.ToString("New_Start_Capillary/StartCapillaryNum" + ViewModel.Start_Capillary_Num + "/CircuitNum" + ViewModel.Circuit_Num);                                
                                //System.Windows.MessageBox.Show(rect.Name);

                                ViewModel.Start_Capillary_Num++;
                                //Capillary.List_Capillary.Add(newcapillary);
                                ViewModel.List_Controls.Add(newcapillary);

                                newcapillary.FullLine = rect.FullLine;
                                newcapillary.In = true;
                                node2.FullLine = rect.FullLine ? false : true;
                                node2.Full = true;
                                node2.ConnectNum--;
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
                            //if (rect_start) node1 = node2;
                        }
                    }
                }
                else if(vm.CreatNewConnector==false)
                {
                    this.ToggleButton1.Content = "开始流路连接";

                    var item = ListBox_this.SelectedItem;
                    if (item!=null&&item.GetType().Name == "Capillary")
                    {
                        TextBox_Length.IsEnabled = true;
                        TextBox_Diameter.IsEnabled = true;
                        Button_Capillary.IsEnabled = true;
                        var selectitem = item as Capillary;
                        TextBox_Length.Text = selectitem.Length.ToString();
                        TextBox_Diameter.Text = selectitem.Diameter.ToString();
                    }
                    else
                    {
                        TextBox_Length.IsEnabled = false;
                        TextBox_Diameter.IsEnabled = false;
                        Button_Capillary.IsEnabled = false;
                        TextBox_Length.Text = "";
                        TextBox_Diameter.Text = "";
                    }
                }


                /*if (vm.CreatNewConnector && ListBox_this.SelectedItem != null)//creat connector
                {
                    //int nodeindex = ListBox.SelectedIndex;
                    var item = ListBox_this.SelectedItem;
                    //item.GetType().Name==""
                    



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
                            Capillary newcapillary = new Capillary();
                            newcapillary.Start = node2;
                            newcapillary.End = rect;
                            vm.Capillaries.Where(x => x.Start == newcapillary.Start && x.End == newcapillary.End).ToList().ForEach(x => vm.Capillaries.Remove(x));//delete same capillary
                            vm.Capillaries.Add(newcapillary);
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
                            Capillary newcapillary = new Capillary();
                            newcapillary.Start = node1;
                            newcapillary.End = rect;
                            vm.Capillaries.Where(x => x.Start == newcapillary.Start && x.End == newcapillary.End).ToList().ForEach(x => vm.Capillaries.Remove(x));
                            vm.Capillaries.Add(newcapillary);
                            if (node1.Y < rect.Y)//change rect height
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
                else if (ListBox_this.SelectedItem != null && vm.CreatNewConnector == false)//set capillary
                {
                    var item = ListBox_this.SelectedItem;
                    if (item.GetType().Name == "Capillary")
                    {
                        TextBox_Length.IsEnabled = true;
                        TextBox_Diameter.IsEnabled = true;
                        Button_Capillary.IsEnabled = true;
                        var selectitem = item as Capillary;
                        TextBox_Length.Text = selectitem.Length.ToString();
                        TextBox_Diameter.Text = selectitem.Diameter.ToString();
                    }
                    else
                    {
                        TextBox_Length.IsEnabled = false;
                        TextBox_Diameter.IsEnabled = false;
                        Button_Capillary.IsEnabled = false;
                        TextBox_Length.Text = "";
                        TextBox_Diameter.Text = "";
                    }
                }
                else if (ListBox_this.SelectedItem == null)//set capillary
                {
                    TextBox_Length.IsEnabled = false;
                    TextBox_Diameter.IsEnabled = false;
                    Button_Capillary.IsEnabled = false;
                    TextBox_Length.Text = "";
                    TextBox_Diameter.Text = "";
                }*/
            }

            private void ListBox_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
            {
                //ListBox_this.SelectedValue = null;
                //node1 = null;
                //node2 = null;
                //rect = null;
            }
            private void Button_Click_Reconnect(object sender, RoutedEventArgs e)
            {
                Button_Click_Sure(sender, e);

                //删除List_Controls上的所有
                int i_List_Controls = 0;
                while(ViewModel.List_Controls.Count>0)
                {
                    ViewModel.List_Controls.Remove(ViewModel.List_Controls[i_List_Controls]);
                }

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
                    newrect.Name = Convert.ToString("NewRect/RectNum" + ViewModel.Rect_Num + "/HasCircuitNum");
                    vm.Rects.Add(newrect);

                    //Canvas_Picture.RegisterName("newRect" + ViewModel.Rect_Num, newrect);//注册名字，以便以后使用
                    
                    //System.Windows.MessageBox.Show(newrect.Name);
                    ViewModel.Rect_Num++;

                    //Rect.List_Rect.Add(newrect);
                    ViewModel.List_Controls.Add(newrect);
                }
            }

            private bool ElementUnderMouse()
            {
                bool flag = false;
                string name = Mouse.DirectlyOver.GetType().Name;
                if (name == "Canvas") flag = true;
                return flag;
            }

            private void Button_Click_Capillary(object sender, RoutedEventArgs e)
            {
                vm.CreatNewConnector = false;
                if (ListBox_this.SelectedItem != null)
                {
                    var item = ListBox_this.SelectedItem;
                    if (item.GetType().Name == "Capillary")
                    {
                        var selectitem = item as Capillary;
                        selectitem.Length = Convert.ToDouble(TextBox_Length.Text);
                        selectitem.Diameter = Convert.ToDouble(TextBox_Diameter.Text);
                    }
                }
            }

            private void AirFlow_Plus(object sender, RoutedEventArgs e)
            {
                AirFlowString = "-<-<-<-<-<-AirFlow-<-<-<-<-";
                TextBlock_AirFlow.Text = AirFlowString;
                AirFlowDirection = 0;
            }

            private void AirFlow_Minus(object sender, RoutedEventArgs e)
            {
                AirFlowString = "->->->->->-AirFlow->->->->-";
                TextBlock_AirFlow.Text = AirFlowString;
                AirFlowDirection = 1;
            }

            private void RefFlow_Plus(object sender, RoutedEventArgs e)
            {
                RefFlowDirection = 0;
            }

            private void RefFlow_Minus(object sender, RoutedEventArgs e)
            {
                RefFlowDirection = 1;
            }

            #region 界面输入合法性检验

            private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)//只允许负号、小数点、数字
            {
                Regex re = new Regex("[^0-9.-]+");
                e.Handled = re.IsMatch(e.Text);

                System.Windows.Controls.TextBox TextBoxText = (System.Windows.Controls.TextBox)sender;//根据sender引用控件。
                if (TextBoxText.Text == "0")//0开头是小数
                {
                    TextBoxText.Text = TextBoxText.Text + ".";
                    TextBoxText.SelectionStart = TextBoxText.Text.Length;
                    return;
                }
            }
            private void TextBox_TextChanged(object sender, TextChangedEventArgs e)//不允许小数点、0开头
            {
                int i = 0;
                System.Windows.Controls.TextBox TextBoxText = (System.Windows.Controls.TextBox)sender;//根据sender引用控件。
                if (TextBoxText.Text.Contains(Convert.ToString((char)46)))
                {
                    if (TextBoxText.Text == "")
                    {
                        e.Handled = true;
                        return;
                    }
                    else if (TextBoxText.Text == ".")
                    {
                        TextBoxText.Text = "";
                        TextBoxText.SelectionStart = TextBoxText.Text.Length;
                    }
                    else
                    { //小数点不允许出现2次
                        foreach (char ch in TextBoxText.Text)
                        {
                            if (ch == (char)46)
                            {
                                i++;
                                if (i == 2)
                                {
                                    TextBoxText.Text = TextBoxText.Text.Substring(0, TextBoxText.Text.LastIndexOf("."));
                                    TextBoxText.SelectionStart = TextBoxText.Text.Length;
                                }
                            }
                        }
                    }
                }
                i = 0;
                if (TextBoxText.Text.Contains(Convert.ToString((char)45)))
                {
                    if (TextBoxText.Text != "") //允许第一个输入负号
                    {
                        foreach (char ch in TextBoxText.Text)
                        {
                            if (ch == (char)45)
                            {
                                i++;
                                if (i == 2)//不允许负号出现2次
                                {
                                    TextBoxText.Text = TextBoxText.Text.Substring(0, TextBoxText.Text.LastIndexOf("-"));
                                    TextBoxText.SelectionStart = TextBoxText.Text.Length;
                                }
                            }
                        }
                    }
                }

                if (TextBoxText.Name == "tai")
                {
                    if ((TextBoxText.Text != "" && TextBoxText.Text!="-") && (Convert.ToDouble(TextBoxText.Text) < -40 || Convert.ToDouble(TextBoxText.Text) > 90))
                    {
                        MessageBoxResult result = System.Windows.MessageBox.Show("空气干球温度只能在-40~90℃内");
                        TextBoxText.Text = TextBoxText.Text.Substring(0, TextBoxText.Text.Length-1);
                        TextBoxText.SelectionStart = TextBoxText.Text.Length;
                    }
                }
                if (TextBoxText.Name == "Tai_wet")
                {
                    if ((TextBoxText.Text != "" && TextBoxText.Text != "-") && (Convert.ToDouble(TextBoxText.Text) < 0 || Convert.ToDouble(TextBoxText.Text) > 37.5))
                    {
                        MessageBoxResult result = System.Windows.MessageBox.Show("空气湿球温度只能在0~37.5℃内");
                        TextBoxText.Text = TextBoxText.Text.Substring(0, TextBoxText.Text.Length - 1);
                        TextBoxText.SelectionStart = TextBoxText.Text.Length;
                    }
                }
            }
            private void PositiveTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)//只允许小数点、数字
            {
                Regex re = new Regex("[^0-9.]+");
                e.Handled = re.IsMatch(e.Text);

                System.Windows.Controls.TextBox TextBoxText = (System.Windows.Controls.TextBox)sender;//根据sender引用控件。
                if (TextBoxText.Text == "0")//0开头是小数
                {
                    TextBoxText.Text = TextBoxText.Text + ".";
                    TextBoxText.SelectionStart = TextBoxText.Text.Length;
                    return;
                }
            }
            private void PositiveTextBox_TextChanged(object sender, TextChangedEventArgs e)//不允许小数点、0开头
            {
                int i = 0;
                System.Windows.Controls.TextBox TextBoxText = (System.Windows.Controls.TextBox)sender;//根据sender引用控件。
                if (TextBoxText.Text.Contains(Convert.ToString((char)46)))
                {
                    if (TextBoxText.Text == "")
                    {
                        e.Handled = true;
                        return;
                    }
                    else if (TextBoxText.Text == ".")
                    {
                        TextBoxText.Text = "";
                        TextBoxText.SelectionStart = TextBoxText.Text.Length;
                    }
                    else
                    { //小数点不允许出现2次
                        foreach (char ch in TextBoxText.Text)
                        {
                            if (ch == (char)46)
                            {
                                i++;
                                if (i == 2)
                                {
                                    TextBoxText.Text = TextBoxText.Text.Substring(0, TextBoxText.Text.LastIndexOf("."));
                                    TextBoxText.SelectionStart = TextBoxText.Text.Length;
                                }
                            }
                        }
                    }
                }

                if (TextBoxText.Name == "RHi")
                {
                    if ((TextBoxText.Text != "") && (Convert.ToDouble(TextBoxText.Text) < 0 || Convert.ToDouble(TextBoxText.Text) > 1))
                    {
                        MessageBoxResult result = System.Windows.MessageBox.Show("空气相对湿度只能在0~1内");
                        TextBoxText.Text = TextBoxText.Text.Substring(0, TextBoxText.Text.Length - 1);
                        TextBoxText.SelectionStart = TextBoxText.Text.Length;
                    }
                }
                if (TextBoxText.Name == "xo_Cond" || TextBoxText.Name == "xi_Evap" || TextBoxText.Name == "xo_Evap")
                {
                    if ((TextBoxText.Text != "" && TextBoxText.Text != "-") && (Convert.ToDouble(TextBoxText.Text) < 0 || Convert.ToDouble(TextBoxText.Text) > 1))
                    {
                        MessageBoxResult result = System.Windows.MessageBox.Show("制冷剂干度只能在0~1内");
                        TextBoxText.Text = TextBoxText.Text.Substring(0, TextBoxText.Text.Length - 1);
                        TextBoxText.SelectionStart = TextBoxText.Text.Length;
                    }
                }
                //if (TextBoxText.Name == "Tro_sub_Evap" || TextBoxText.Name == "Tro_sub_Cond")
                //{
                //    if ((TextBoxText.Text != "" && TextBoxText.Text != "-") && (Convert.ToDouble(TextBoxText.Text) < 0 || Convert.ToDouble(TextBoxText.Text) > 1))
                //    {
                //        MessageBoxResult result = MessageBox.Show("制冷剂干度只能在0~1内");
                //        TextBoxText.Text = TextBoxText.Text.Substring(0, TextBoxText.Text.Length - 1);
                //        TextBoxText.SelectionStart = TextBoxText.Text.Length;
                //    }
                //}
            }

            private void NumTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)//只允许整数
            {
                //只允许输入数字/小数点/负号
                Regex re = new Regex("[^0-9]+");
                e.Handled = re.IsMatch(e.Text);
            }
            private void NumTextBox_TextChanged(object sender, TextChangedEventArgs e)//不允许0开头
            {
                System.Windows.Controls.TextBox TextBoxText = (System.Windows.Controls.TextBox)sender;//根据sender引用控件。
                if (TextBoxText.Text == "0")
                {
                    TextBoxText.Text = "";
                    return;
                }
            }

            #endregion

            private void MenuItem_Click_DefaultValue(object sender, RoutedEventArgs e)
            {
                //***********************HEx Type***********************
                if (this.RadioButton_HExType_Condenser.IsChecked == true)
                {
                    //***********************HEx Tube***********************
                    //管型号
                    //ComboBox_TubeVersion
                    this.ComboBox_TubeVersion.SelectedItem = "7mm,2R,21x22";

                    ////管外径
                    ////Do

                    ////管间距
                    ////Pt

                    ////列间距
                    ////Pr

                    ////管排
                    ////Row

                    //管数/排
                    //tube_per
                    this.tube_per.Text = "6";

                    //管壁厚
                    //thick_tube
                    this.thick_tube.Text = "0.2";

                    //管长
                    //L
                    this.L.Text = "914.4";

                    //管种类
                    //ComboBox_tubetype
                    this.ComboBox_tubetype.SelectedItem = "光管";

                    //确定
                    //Button1
                    MouseButtonEventArgs args = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left);
                    args.RoutedEvent = System.Windows.Controls.Button.ClickEvent;
                    this.Button1.RaiseEvent(args);

                    //直排
                    //TubeArrangement_Crossed_High
                    this.TubeArrangement_Crossed_High.IsChecked = true;

                    //***********************Fin***********************

                    //翅片间距
                    //Fnum
                    this.Fnum.Text = "1.2";

                    //翅片厚度
                    //Fthick
                    this.Fthick.Text = "0.095";

                    //翅片种类
                    //ComboBox_fintype
                    this.ComboBox_fintype.SelectedItem = "平片";

                    //***********************Ref***********************

                    //制冷剂
                    this.ComboBox_Refrigerant.SelectedItem = "R32";

                    //***********************冷凝器
                    //饱和温度
                    this.tc.Text = "45";

                    //进口温度
                    this.tri.Text = "78";

                    //流量
                    this.RadioButton_mro_Cond.IsChecked = true;
                    this.mro_Cond.Text = "0.02";

                    //***********************Air***********************

                    //进风干球温度
                    this.tai.Text = "35";

                    //大气压力
                    this.Patm.Text = "101.325";

                    //进风湿球温度
                    this.RadioButton_WetBulbTemperature.IsChecked = true;
                    this.Tai_wet.Text = "24";

                    ////进风相对湿度
                    ////RelativeHumidity
                    ////this.RHi.Text="0.5"

                    //体积流量
                    this.RadioButton_AirVolumnFlowRate.IsChecked = true;
                    this.Va.Text = "0.12";

                    ////风速
                    ////AirVelocity
                    ////Velocity_ai.Text="1";

                    //均匀送风
                    this.CheckBox_UniformWind.IsChecked = true;

                    //***********************Pass***********************
                    //手动分配流路
                    //RadioButton_ManualArrange
                    this.RadioButton_ManualArrange.IsChecked = true;

                    ////自动分配流路
                    ////RadioButton_AutoArrange
                    ////流路数
                    ////Cirnum.Text="2";
                    //<Button x:Name="Pass_OK" Content="确定" HorizontalAlignment="Left" VerticalAlignment="Top" Margin="220,85,0,0" Height="Auto" Width="90" IsEnabled="False"/>

                    //空气流向反转
                    //AirReverse
                    this.AirReverse.IsChecked = false;

                    //制冷剂流向反转
                    //RefReverse
                    this.RefReverse.IsChecked = false;

                    //***********************AdjustParameter***********************
                    //***********************制冷剂侧系数
                    //传热系数修正
                    this.Zhr.Text = "1";
                    //压降系数修正
                    this.Zapr.Text = "1";
                    //***********************管外侧修正
                    //传热系数修正
                    this.Zha.Text = "1";
                    //压降系数修正
                    this.Zapa.Text = "1";

                }

                else 
                {
                    //***********************HEx Tube***********************
                    //管型号
                    //ComboBox_TubeVersion
                    this.ComboBox_TubeVersion.SelectedItem = "7mm,2R,21x22";

                    ////管外径
                    ////Do

                    ////管间距
                    ////Pt

                    ////列间距
                    ////Pr

                    ////管排
                    ////Row

                    //管数/排
                    //tube_per
                    this.tube_per.Text = "6";

                    //管壁厚
                    //thick_tube
                    this.thick_tube.Text = "0.2";

                    //管长
                    //L
                    this.L.Text = "914.4";

                    //管种类
                    //ComboBox_tubetype
                    this.ComboBox_tubetype.SelectedItem = "光管";

                    //确定
                    //Button1
                    //后台点击按钮
                    MouseButtonEventArgs args = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left);
                    args.RoutedEvent = System.Windows.Controls.Button.ClickEvent;
                    this.Button1.RaiseEvent(args);

                    //直排
                    //TubeArrangement_Crossed_High
                    this.TubeArrangement_Crossed_High.IsChecked = true;

                    //***********************Fin***********************

                    //翅片间距
                    //Fnum
                    this.Fnum.Text = "1.2";

                    //翅片厚度
                    //Fthick
                    this.Fthick.Text = "0.095";

                    //翅片种类
                    //ComboBox_fintype
                    this.ComboBox_fintype.SelectedItem = "平片";

                    //***********************Ref***********************

                    //制冷剂
                    this.ComboBox_Refrigerant.SelectedItem = "R32";

                    //***********************蒸发器
                    ////压力
                    ////RadioButton_Pri_Evap
                    ////Pri_Evap

                    ////干度
                    ////RadioButton_xi_Evap
                    ////xi_Evap

                    ////焓值
                    ////RadioButton_Hri_Evap
                    ////Hri_Evap

                    //阀前温度
                    //RadioButton_PriTri_Evap
                    //Tri_ValveBefore
                    this.RadioButton_PriTri_Evap.IsChecked = true;
                    this.Pri_Evap.Text = "0";
                    this.xi_Evap.Text = "0";
                    this.Hri_Evap.Text = "0";
                    this.Tri_ValveBefore.Text = "20";

                    //阀前压力
                    //Pri_ValveBefore
                    this.Pri_ValveBefore.Text = "1842.28";

                    //饱和温度
                    this.RadioButton_Tro_Evap.IsChecked = true;
                    Tcro_Evap.Text = "10";

                    ////过热度
                    ////RadioButton_Tro_sub_Evap
                    ////Tro_sub_Evap

                    ////干度
                    ////RadioButton_xo_Evap
                    ////xo_Evap"

                    //流量
                    this.RadioButton_mro_Evap.IsChecked = true;
                    this.mro_Evap.Text = "0.02";

                    //***********************Air***********************

                    //进风干球温度
                    this.tai.Text = "27";

                    //大气压力
                    this.Patm.Text = "101.325";

                    //进风湿球温度
                    this.RadioButton_WetBulbTemperature.IsChecked = true;
                    this.Tai_wet.Text = "12";

                    ////进风相对湿度
                    ////RelativeHumidity
                    ////this.RHi.Text="0.5"

                    //体积流量
                    this.RadioButton_AirVolumnFlowRate.IsChecked = true;
                    this.Va.Text = "0.12";

                    ////风速
                    ////AirVelocity
                    ////Velocity_ai.Text="1";

                    //均匀送风
                    this.CheckBox_UniformWind.IsChecked = true;

                    //***********************Pass***********************
                    //手动分配流路
                    //RadioButton_ManualArrange
                    this.RadioButton_ManualArrange.IsChecked = true;

                    ////自动分配流路
                    ////RadioButton_AutoArrange
                    ////流路数
                    ////Cirnum.Text="2";
                    //<Button x:Name="Pass_OK" Content="确定" HorizontalAlignment="Left" VerticalAlignment="Top" Margin="220,85,0,0" Height="Auto" Width="90" IsEnabled="False"/>

                    //空气流向反转
                    //AirReverse
                    this.AirReverse.IsChecked = false;

                    //制冷剂流向反转
                    //RefReverse
                    this.RefReverse.IsChecked = false;

                    //***********************AdjustParameter***********************
                    //***********************制冷剂侧系数
                    //传热系数修正
                    this.Zhr.Text = "1";
                    //压降系数修正
                    this.Zapr.Text = "1";
                    //***********************管外侧修正
                    //传热系数修正
                    this.Zha.Text = "1";
                    //压降系数修正
                    this.Zapa.Text = "1";

                }
            }

            private void MenuItem_Click_ExportInput(object sender, RoutedEventArgs e)
            {
                //记录按钮、键入信息
                string data =
                    "冷凝器"+
                    "\t"+
                    Convert.ToString(this.RadioButton_HExType_Condenser.IsChecked.Value) +
                    "\r\n" +
                    "蒸发器" + 
                    "\t" +
                    Convert.ToString(this.RadioButton_HExType_Evaporator.IsChecked.Value) +
                    "\r\n" +
                    "管型号" + 
                    "\t" +
                    Convert.ToString(this.ComboBox_TubeVersion.SelectedItem) +
                    "\r\n" +
                    "管外径" + 
                    "\t" +
                    Convert.ToString(this.Do.Text)+
                    "\r\n" +
                    "管间距" + 
                    "\t" +
                    Convert.ToString(this.Pt.Text)+
                    "\r\n" +
                    "列间距" + 
                    "\t" +
                    Convert.ToString(this.Pr.Text)+
                    "\r\n" +
                    "管排" + 
                    "\t" +
                    Convert.ToString(this.Row.Text) +
                    "\r\n" +
                    "管数每排" + 
                    "\t" +
                    Convert.ToString(this.tube_per.Text)+
                    "\r\n" +
                    "管壁厚" + 
                    "\t" +
                    Convert.ToString(this.thick_tube.Text)+
                    "\r\n" +
                    "管长" + 
                    "\t" +
                    Convert.ToString(this.L.Text)+
                    "\r\n" +
                    "管种类" + 
                    "\t" +
                    Convert.ToString(this.ComboBox_tubetype.Text) +
                    "\r\n" +
                    "直排" + 
                    "\t" +
                    Convert.ToString(this.TubeArrangement_Straight.IsChecked.Value) +
                    "\r\n" +
                    "叉排迎风面高" + 
                    "\t" +
                    Convert.ToString(this.TubeArrangement_Crossed_High.IsChecked.Value)+
                    "\r\n" +
                    "叉排迎风面低" + 
                    "\t" +
                    Convert.ToString(this.TubeArrangement_Crossed_Short.IsChecked.Value) +
                    "\r\n" +
                    "翅片间距" + 
                    "\t" +
                    Convert.ToString(this.Fnum.Text) +
                    "\r\n" +
                    "翅片厚度" + 
                    "\t" +
                    Convert.ToString(this.Fthick.Text)+
                    "\r\n" +
                    "翅片种类" + 
                    "\t" +
                    Convert.ToString(this.ComboBox_fintype.SelectedItem) +
                    "\r\n" +
                    "制冷剂种类" + 
                    "\t" +
                    Convert.ToString(this.ComboBox_Refrigerant.SelectedItem) +
                     "\r\n" +
                    "冷凝器饱和温度" + 
                    "\t" +
                    Convert.ToString(this.tc.Text) +
                    "\r\n" +
                    "冷凝器进口温度" + 
                    "\t" +
                    Convert.ToString(this.tri.Text) +
                    "\r\n" +
                    "冷凝器流量" + 
                    "\t" +
                    Convert.ToString(this.RadioButton_mro_Cond.IsChecked.Value) +
                    "\t" +
                    Convert.ToString(this.mro_Cond.Text) +
                    "\r\n" +
                    "冷凝器出口干度" + 
                    "\t" +
                    Convert.ToString(this.RadioButton_xo_Cond.IsChecked.Value) +
                    "\t" +
                    Convert.ToString(this.xo_Cond.Text) +
                    "\r\n" +
                    "冷凝器出口过冷度" +
                    "\t" +
                    Convert.ToString(this.RadioButton_Tro_sub_Cond.IsChecked.Value) +
                    "\t" +
                    Convert.ToString(this.Tro_sub_Cond.Text) +
                    "\r\n" +
                    "蒸发器进口压力" +
                    "\t" +
                    Convert.ToString(this.RadioButton_Pri_Evap.IsChecked.Value) +
                    "\t" +
                    Convert.ToString(this.Pri_Evap.Text) +
                    "\r\n" +
                    "蒸发器进口干度" +
                    "\t" +
                    Convert.ToString(this.RadioButton_xi_Evap.IsChecked.Value) +
                    "\t" +
                    Convert.ToString(this.xi_Evap.Text) +
                    "\r\n" +
                    "蒸发器进口焓" +
                    "\t" +
                    Convert.ToString(this.RadioButton_Hri_Evap.IsChecked.Value) +
                    "\t" +
                    Convert.ToString(this.Hri_Evap.Text) +
                    "\r\n" +
                    "蒸发器阀前温度压力" +
                    "\t" +
                    Convert.ToString(this.RadioButton_PriTri_Evap.IsChecked.Value) +
                    "\t" +
                    Convert.ToString(this.Tri_ValveBefore.Text) +
                    "\t" +
                    Convert.ToString(this.Pri_ValveBefore.Text) +
                    "\r\n" +
                    "蒸发器出口饱和温度" +
                    "\t" +
                    Convert.ToString(this.RadioButton_Tro_Evap.IsChecked.Value) +
                    "\t" +
                    Convert.ToString(this.Tcro_Evap.Text) +
                    "\r\n" +
                    "蒸发器出口过热度" +
                    "\t" +
                    Convert.ToString(this.RadioButton_Tro_sub_Evap.IsChecked.Value) +
                    "\t" +
                    Convert.ToString(this.Tro_sub_Evap.Text) +
                    "\r\n" +
                    "蒸发器出口干度" +
                    "\t" +
                    Convert.ToString(this.RadioButton_xo_Evap.IsChecked.Value) +
                    "\t" +
                    Convert.ToString(this.xo_Evap.Text) +
                    "\r\n" +
                    "蒸发器流量" +
                    "\t" +
                    Convert.ToString(this.RadioButton_mro_Evap.IsChecked.Value) +
                    "\t" +
                    Convert.ToString(this.mro_Evap.Text) +
                    "\r\n" +
                    "进风干球温度" +
                    "\t" +
                    Convert.ToString(this.tai.Text) +
                    "\r\n" +
                    "环境压力" +
                    "\t" +
                    Convert.ToString(this.Patm.Text) +
                    "\r\n" +
                    "进风湿球温度" +
                    "\t" +
                    Convert.ToString(this.RadioButton_WetBulbTemperature.IsChecked.Value) +
                    "\t" +
                    Convert.ToString(this.Tai_wet.Text) +
                    "\r\n" +
                    "进风相对" +
                    "\t" +
                    Convert.ToString(this.RadioButton_RelativeHumidity.IsChecked.Value) +
                    "\t" +
                    Convert.ToString(this.RHi.Text) +
                    "\r\n" +
                    "进风体积流量" +
                    "\t" +
                    Convert.ToString(this.RadioButton_AirVolumnFlowRate.IsChecked.Value) +
                    "\t" +
                    Convert.ToString(this.Va.Text) +
                    "\r\n" +
                    "进风风速" +
                    "\t" +
                    Convert.ToString(this.RadioButton_AirVelocity.IsChecked.Value) +
                    "\t" +
                    Convert.ToString(this.Velocity_ai.Text) +
                    "\r\n" +
                    "均匀送风" +
                    "\t" +
                    Convert.ToString(this.CheckBox_UniformWind.IsChecked.Value) +
                    "\r\n" +
                    "手动分配流路" +
                    "\t" +
                    Convert.ToString(this.RadioButton_ManualArrange.IsChecked.Value) +
                    "\r\n" +
                    "自动分配流路" +
                    "\t" +
                    Convert.ToString(this.RadioButton_AutoArrange.IsChecked.Value) +
                    "\r\n" +
                    "自动分配流路数" +
                    "\t" +
                    Convert.ToString(this.Cirnum.Text) +
                    "\r\n" +
                    "风反向" +
                    "\t" +
                    Convert.ToString(this.AirReverse.IsChecked.Value) +
                    "\r\n" +
                    "制冷剂反向" +
                    "\t" +
                    Convert.ToString(this.RefReverse.IsChecked.Value) +
                    "\r\n" +
                    "制冷剂侧传热系数修正" +
                    "\t" +
                    Convert.ToString(this.Zhr.Text) +
                    "\r\n" +
                    "制冷剂侧压降系数修正" +
                    "\t" +
                    Convert.ToString(this.Zapr.Text) +
                    "\r\n" +
                    "空气侧传热系数修正" +
                    "\t" +
                    Convert.ToString(this.Zha.Text) +
                    "\r\n" +
                    "空气侧压降系数修正" +
                    "\t" +
                    Convert.ToString(this.Zapa.Text)
                    ;

                //开始记录流路信息
                //List_Controls
                int i_List_Controls = 0;//在List_Controls中找选定线的位置

                //List_Connector
                int i_List_Connector = 0;//在List_Connector中找选定线的位置

                //List_Capillary
                int i_List_Capillary = 0;

                //List_Rect
                int i_List_Rect = 0;//在List_Rect中找选定线的位置

                for (i_List_Controls = 0; i_List_Controls < ViewModel.List_Controls.Count; i_List_Controls++)
                {
                    if (ViewModel.List_Controls[i_List_Controls].GetType().Name == "Capillary")
                    {
                        var CurrentItem = ViewModel.List_Controls[i_List_Controls] as Capillary;

                        data = data +
                            "\r\n" +
                            "Capillary" +
                            "\r\n" +
                            "Name" +
                            "\t" +
                            Convert.ToString(CurrentItem.Name) +
                            "\r\n" +
                            "Start.Name" +
                            "\t" +
                            Convert.ToString(CurrentItem.Start.Name) +
                            "\r\n" +
                            "Start.X" +
                            "\t" +
                            Convert.ToString(CurrentItem.Start.X) +
                            "\r\n" +
                            "Start.Y" +
                            "\t" +
                            Convert.ToString(CurrentItem.Start.Y) +
                            "\r\n" +
                            "Start.Full" +
                            "\t" +
                            Convert.ToString(CurrentItem.Start.Full) +
                            "\r\n" +
                            "Start.FullLine" +
                            "\t" +
                            Convert.ToString(CurrentItem.Start.FullLine) +
                            "\r\n" +
                            "End.Name" +
                            "\t" +
                            Convert.ToString(CurrentItem.End.Name) +
                            "\r\n" +
                            "End.X" +
                            "\t" +
                            Convert.ToString(CurrentItem.End.X) +
                            "\r\n" +
                            "End.Y" +
                            "\t" +
                            Convert.ToString(CurrentItem.End.Y) +
                            "\r\n" +
                            "End.FullLine" +
                            "\t" +
                            Convert.ToString(CurrentItem.End.FullLine) +
                            "\r\n" +
                            "End.RectHeight" +
                            "\t" +
                            Convert.ToString(CurrentItem.End.RectHeight) +
                            "\r\n" +
                            "Length" +
                            "\t" +
                            Convert.ToString(CurrentItem.Length) +
                            "\r\n" +
                            "Diameter" +
                            "\t" +
                            Convert.ToString(CurrentItem.Diameter) +
                            "\r\n" +
                            "X" +
                            "\t" +
                            Convert.ToString(CurrentItem.X) +
                            "\r\n" +
                            "Y" +
                            "\t" +
                            Convert.ToString(CurrentItem.Y) +
                            "\r\n" +
                            "FullLine" +
                            "\t" +
                            Convert.ToString(CurrentItem.FullLine) +
                            "\r\n" +
                            "In" +
                            "\t" +
                            Convert.ToString(CurrentItem.In)
                            ;
                    }

                    if (ViewModel.List_Controls[i_List_Controls].GetType().Name == "Rect")
                    {
                        var CurrentItem = ViewModel.List_Controls[i_List_Controls] as Rect;

                        data = data +
                            "\r\n" +
                            "Rect" +
                            "\r\n" +
                            "Name" +
                            "\t" +
                            Convert.ToString(CurrentItem.Name) +
                            "\r\n" +
                            "Rect.X" +
                            "\t" +
                            Convert.ToString(CurrentItem.X) +
                            "\r\n" +
                            "Rect.Y" +
                            "\t" +
                            Convert.ToString(CurrentItem.Y) +
                            "\r\n" +
                            "RectHeight" +
                            "\t" +
                            Convert.ToString(CurrentItem.RectHeight) +
                            "\r\n" +
                            "FullLine" +
                            "\t" +
                            Convert.ToString(CurrentItem.FullLine)
                            ;
                    }
                    if (ViewModel.List_Controls[i_List_Controls].GetType().Name == "Connector")
                    {
                        var CurrentItem = ViewModel.List_Controls[i_List_Controls] as Connector;

                        data = data +
                            "\r\n" +
                            "Connector" +
                            "\r\n" +
                            "Name" +
                            "\t" +
                            Convert.ToString(CurrentItem.Name) +
                            "\r\n" +
                            "Start.Name" +
                            "\t" +
                            Convert.ToString(CurrentItem.Start.Name) +
                            "\r\n" +
                            "Start.X" +
                            "\t" +
                            Convert.ToString(CurrentItem.Start.X) +
                            "\r\n" +
                            "Start.Y" +
                            "\t" +
                            Convert.ToString(CurrentItem.Start.Y) +
                            "\r\n" +
                            "Start.Full" +
                            "\t" +
                            Convert.ToString(CurrentItem.Start.Full) +
                            "\r\n" +
                            "Start.FullLine" +
                            "\t" +
                            Convert.ToString(CurrentItem.Start.FullLine) +
                            "\r\n" +
                            "End.Name" +
                            "\t" +
                            Convert.ToString(CurrentItem.End.Name) +
                            "\r\n" +
                            "End.X" +
                            "\t" +
                            Convert.ToString(CurrentItem.End.X) +
                            "\r\n" +
                            "End.Y" +
                            "\t" +
                            Convert.ToString(CurrentItem.End.Y) +
                            "\r\n" +
                            "End.FullLine" +
                            "\t" +
                            Convert.ToString(CurrentItem.End.FullLine) +
                            "\r\n" +
                            "X" +
                            "\t" +
                            Convert.ToString(CurrentItem.X) +
                            "\r\n" +
                            "Y" +
                            "\t" +
                            Convert.ToString(CurrentItem.Y) +
                            "\r\n" +
                            "FullLine" +
                            "\t" +
                            Convert.ToString(CurrentItem.FullLine)
                            ;
                    }
                }

                //按照日期建立一个文件名
                string FileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_Mcoil输入.txt";

                string localFilePath, fileNameExt, FilePath;
                SaveFileDialog sfd = new SaveFileDialog();

                //设置文件类型 
                sfd.Filter = "Mcoil文件(*.Mcoil)|*.Mcoil|MCOIL文件(*.MCOIL)|*.MCOIL";

                //默认文件名
                sfd.FileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_Mcoil输入.Mcoil";

                try
                { 
                    //点了保存按钮进入 
                    if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        localFilePath = sfd.FileName.ToString(); //获得文件路径 
                        fileNameExt = localFilePath.Substring(localFilePath.LastIndexOf("\\") + 1); //获取文件名，不带路径

                        //获取文件路径，不带文件名 
                        FilePath = localFilePath.Substring(0, localFilePath.LastIndexOf("\\")) + "\\" + fileNameExt;

                        //文件覆盖方式添加内容
                        System.IO.StreamWriter file = new System.IO.StreamWriter(FilePath, false);
                        //保存数据到文件
                        file.Write(data);
                        //关闭文件
                        file.Close();
                        //释放对象
                        file.Dispose();

                        System.Windows.MessageBox.Show("文件保存成功！");
                    }
                }
                catch (IOException ex)
                {
                    Console.WriteLine(ex.ToString());
                }


            }

            private void MenuItem_Click_InportInput(object sender, RoutedEventArgs e)
            {
                //定义一个变量，存储文件所在的路径
                string upStr = "";
                int i = 1;

                OpenFileDialog fdlg = new OpenFileDialog();
                fdlg.Title = "选择需导入的输入信息文件";
                //fdlg.InitialDirectory = @"c:\";   //@是取消转义字符的意思
                fdlg.Filter = "Mcoil文件(*.Mcoil)|*.Mcoil|MCOIL文件(*.MCOIL)|*.MCOIL";
                fdlg.FilterIndex = 1;//FilterIndex 属性用于选择了何种文件类型,缺省设置为0,系统取Filter属性设置第一项,相当于FilterIndex 属性设置为1.如果你编了3个文件类型，当FilterIndex ＝2时是指第2个
                fdlg.RestoreDirectory = false;//如果值为false，那么下一次选择文件的初始目录是上一次你选择的那个目录，不固定；如果值为true，每次打开这个对话框初始目录不随你的选择而改变，是固定的

                try
                {
                    if (fdlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        upStr = File.ReadAllText(fdlg.FileName, Encoding.UTF8);//UTF8万国码 //先读取文本内容，调用File类的ReadAllLines即可读取所有内容
                    }

                    if(upStr!="")
                    {
                        string[] sp = upStr.Split(new string[] { "\r\n", "\t" }, StringSplitOptions.None);

                        this.RadioButton_HExType_Condenser.IsChecked = Convert.ToBoolean(sp[i]); i++; i++;
                        this.RadioButton_HExType_Evaporator.IsChecked = Convert.ToBoolean(sp[i]); i++; i++;
                        this.ComboBox_TubeVersion.SelectedItem = Convert.ToString(sp[i]); i++; i++;
                        this.Do.Text = Convert.ToString(sp[i]); i++; i++;
                        this.Pt.Text = Convert.ToString(sp[i]); i++; i++;
                        this.Pr.Text = Convert.ToString(sp[i]); i++; i++;
                        this.Row.Text = Convert.ToString(sp[i]); i++; i++;
                        this.tube_per.Text = Convert.ToString(sp[i]); i++; i++;
                        this.thick_tube.Text = Convert.ToString(sp[i]); i++; i++;
                        this.L.Text = Convert.ToString(sp[i]); i++; i++;
                        this.ComboBox_tubetype.SelectedItem = Convert.ToString(sp[i]); i++; i++;
                        this.TubeArrangement_Straight.IsChecked = Convert.ToBoolean(sp[i]); i++; i++;
                        this.TubeArrangement_Crossed_High.IsChecked = Convert.ToBoolean(sp[i]); i++; i++;
                        this.TubeArrangement_Crossed_Short.IsChecked = Convert.ToBoolean(sp[i]); i++; i++;
                        this.Fnum.Text = Convert.ToString(sp[i]); i++; i++;
                        this.Fthick.Text = Convert.ToString(sp[i]); i++; i++;
                        this.ComboBox_fintype.SelectedItem = Convert.ToString(sp[i]); i++; i++;
                        this.ComboBox_Refrigerant.SelectedItem = Convert.ToString(sp[i]); i++; i++;
                        this.tc.Text = Convert.ToString(sp[i]); i++; i++;
                        this.tri.Text = Convert.ToString(sp[i]); i++; i++;
                        this.RadioButton_mro_Cond.IsChecked = Convert.ToBoolean(sp[i]); i++;
                        this.mro_Cond.Text = Convert.ToString(sp[i]); i++; i++;
                        this.RadioButton_xo_Cond.IsChecked = Convert.ToBoolean(sp[i]); i++;
                        this.xo_Cond.Text = Convert.ToString(sp[i]); i++; i++;
                        this.RadioButton_Tro_sub_Cond.IsChecked = Convert.ToBoolean(sp[i]); i++;
                        this.Tro_sub_Cond.Text = Convert.ToString(sp[i]); i++; i++;
                        this.RadioButton_Pri_Evap.IsChecked = Convert.ToBoolean(sp[i]); i++;
                        this.Pri_Evap.Text = Convert.ToString(sp[i]); i++; i++;
                        this.RadioButton_xi_Evap.IsChecked = Convert.ToBoolean(sp[i]); i++;
                        this.xi_Evap.Text = Convert.ToString(sp[i]); i++; i++;
                        this.RadioButton_Hri_Evap.IsChecked = Convert.ToBoolean(sp[i]); i++;
                        this.Hri_Evap.Text = Convert.ToString(sp[i]); i++; i++;
                        this.RadioButton_PriTri_Evap.IsChecked = Convert.ToBoolean(sp[i]); i++;
                        this.Tri_ValveBefore.Text = Convert.ToString(sp[i]); i++; 
                        this.Pri_ValveBefore.Text = Convert.ToString(sp[i]); i++; i++;
                        this.RadioButton_Tro_Evap.IsChecked = Convert.ToBoolean(sp[i]); i++;
                        this.Tcro_Evap.Text = Convert.ToString(sp[i]); i++; i++;
                        this.RadioButton_Tro_sub_Evap.IsChecked = Convert.ToBoolean(sp[i]); i++;
                        this.Tro_sub_Evap.Text = Convert.ToString(sp[i]); i++; i++;
                        this.RadioButton_xo_Evap.IsChecked = Convert.ToBoolean(sp[i]); i++;
                        this.xo_Evap.Text = Convert.ToString(sp[i]); i++; i++;
                        this.RadioButton_mro_Evap.IsChecked = Convert.ToBoolean(sp[i]); i++;
                        this.mro_Evap.Text = Convert.ToString(sp[i]); i++; i++;
                        this.tai.Text = Convert.ToString(sp[i]); i++; i++;
                        this.Patm.Text = Convert.ToString(sp[i]); i++; i++;
                        this.RadioButton_WetBulbTemperature.IsChecked = Convert.ToBoolean(sp[i]); i++;
                        this.Tai_wet.Text = Convert.ToString(sp[i]); i++; i++;
                        this.RadioButton_RelativeHumidity.IsChecked = Convert.ToBoolean(sp[i]); i++;
                        this.RHi.Text = Convert.ToString(sp[i]); i++; i++;
                        this.RadioButton_AirVolumnFlowRate.IsChecked = Convert.ToBoolean(sp[i]); i++;
                        this.Va.Text = Convert.ToString(sp[i]); i++; i++;
                        this.RadioButton_AirVelocity.IsChecked = Convert.ToBoolean(sp[i]); i++;
                        this.Velocity_ai.Text = Convert.ToString(sp[i]); i++; i++;
                        this.CheckBox_UniformWind.IsChecked = Convert.ToBoolean(sp[i]); i++; i++;
                        this.RadioButton_ManualArrange.IsChecked = Convert.ToBoolean(sp[i]); i++; i++;
                        this.RadioButton_AutoArrange.IsChecked = Convert.ToBoolean(sp[i]); i++; i++;
                        this.Cirnum.Text = Convert.ToString(sp[i]); i++; i++;
                        this.AirReverse.IsChecked = Convert.ToBoolean(sp[i]); i++; i++;
                        this.RefReverse.IsChecked = Convert.ToBoolean(sp[i]); i++; i++;
                        this.Zhr.Text = Convert.ToString(sp[i]); i++; i++;
                        this.Zapr.Text = Convert.ToString(sp[i]); i++; i++;
                        this.Zha.Text = Convert.ToString(sp[i]); i++; i++;
                        this.Zapa.Text = Convert.ToString(sp[i]); i++;

                        //界面生成管子
                        ////人为后台按Button1
                        //MouseButtonEventArgs args = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left);
                        //args.RoutedEvent = System.Windows.Controls.Button.ClickEvent;
                        //this.Button1.RaiseEvent(args);
                        int _row = Convert.ToInt32(Row.Text);
                        int _tube = Convert.ToInt32(tube_per.Text);
                        vm.Connectors.Clear();
                        vm.Capillaries.Clear();
                        while (vm.Rects.Count > 2)
                        {
                            vm.Rects.RemoveAt(2);
                        }
                        ViewModel.List_Controls.Clear();
                        node1 = null;
                        rect = null;
                        vm.GenerateNode(_row, _tube);

                        //生成显示的流路
                        int list_vm_Nodes = 0;
                        int list_vm_Rects = 0;
                        
                        if (i < sp.Length - 1)//没流路时可能会超数组长度，所以判断后面有没有流路信息
                        {
                            //先生成Rect才能生成Capillary
                            for (int j = i; j < sp.Count(); j++)
                            {
                                if (sp[j] == "Rect")
                                {
                                    Rect newrect = new Rect();
                                    j++; j++;
                                    newrect.Name = sp[j]; j++; j++;
                                    newrect.X = Convert.ToDouble(sp[j]); j++; j++;
                                    newrect.Y = Convert.ToDouble(sp[j]); j++; j++;
                                    newrect.RectHeight = Convert.ToDouble(sp[j]); j++; j++;
                                    newrect.FullLine = Convert.ToBoolean(sp[j]); j++; j++;
                                    vm.Rects.Add(newrect);
                                    ViewModel.List_Controls.Add(newrect);//导入时生成的List_Controls顺序会乱...除非有顺序要求，目前不调顺序了
                                }
                            }

                            //生成Capillary
                            for (int j = i; j < sp.Count(); j++)
                            {
                                if (sp[j] == "Capillary")
                                {
                                    Capillary newcapillary = new Capillary();
                                    j++; j++;
                                    //this.Canvas_Picture.RegisterName("New_Start_Capillary" + ViewModel.Start_Capillary_Num, newcapillary);//注册名字，以便以后使用
                                    newcapillary.Name = sp[j]; j++; j++;

                                    for (list_vm_Nodes = 0; list_vm_Nodes < vm.Nodes.Count; list_vm_Nodes++)
                                    {
                                        if (Convert.ToDouble(sp[j + 2]) == vm.Nodes[list_vm_Nodes].X)
                                        {
                                            if (Convert.ToDouble(sp[j + 4]) == vm.Nodes[list_vm_Nodes].Y)
                                            {
                                                newcapillary.Start = vm.Nodes[list_vm_Nodes];
                                                j++; j++;//Start.Name
                                                j++; j++;//Start.X
                                                j++; j++;//Start.Y
                                                j++; j++;//Start.Full
                                                j++; j++;//Start.FullLine
                                                break;
                                            }

                                        }
                                    }
                                    for (list_vm_Rects = 0; list_vm_Rects < vm.Rects.Count; list_vm_Rects++)
                                    {
                                        if (Convert.ToDouble(sp[j + 2]) == vm.Rects[list_vm_Rects].X)
                                        {
                                            if (list_vm_Rects == 0)//导出文件时vm.Rects[0].Y可能不会和当前已有的vm.Rects[0].Y相等，所以特殊处理
                                            {
                                                newcapillary.End = vm.Rects[list_vm_Rects];
                                                j++; j++;//End.Name
                                                j++; j++;//End.X
                                                j++; j++;//End.Y
                                                j++; j++;//End.FullLine
                                                j++; j++;//End.RectHeight
                                                break;
                                            }
                                            else if (Convert.ToDouble(sp[j + 4]) == vm.Rects[list_vm_Rects].Y)
                                            {
                                                newcapillary.End = vm.Rects[list_vm_Rects];
                                                j++; j++;//End.Name
                                                j++; j++;//End.X
                                                j++; j++;//End.Y
                                                j++; j++;//End.FullLine
                                                j++; j++;//End.RectHeight
                                                break;
                                            }

                                        }
                                    }
                                    newcapillary.Length = Convert.ToDouble(sp[j]); j++; j++;
                                    newcapillary.Diameter = Convert.ToDouble(sp[j]); j++; j++;
                                    newcapillary.X = Convert.ToDouble(sp[j]); j++; j++; j++; j++;
                                    if (sp[j] == "True")
                                    {
                                        sp[j] = "true";
                                    }
                                    else
                                    {
                                        sp[j] = "false";
                                    }
                                    newcapillary.FullLine = Convert.ToBoolean(sp[j]); j++; j++;
                                    if (sp[j] == "True")
                                    {
                                        sp[j] = "true";
                                    }
                                    else
                                    {
                                        sp[j] = "false";
                                    }
                                    newcapillary.In = Convert.ToBoolean(sp[j]);

                                    vm.Capillaries.Add(newcapillary);
                                    ViewModel.List_Controls.Add(newcapillary);
                                }
                            }

                            int _list_vm_Nodes = 0;
                            //先生成Rect才能生成Connector
                            for (int j = i; j < sp.Count(); j++)
                            {
                                if (sp[j] == "Connector")
                                {
                                    Connector newLine = new Connector();
                                    j++; j++;//newLine.Name
                                    newLine.Name = sp[j];
                                    j++; j++;//Start.Name

                                    for (list_vm_Nodes = 0; list_vm_Nodes < vm.Nodes.Count; list_vm_Nodes++)
                                    {
                                        if (Convert.ToDouble(sp[j + 2]) == vm.Nodes[list_vm_Nodes].X)
                                        {
                                            if (Convert.ToDouble(sp[j + 4]) == vm.Nodes[list_vm_Nodes].Y)
                                            {
                                                newLine.Start = vm.Nodes[list_vm_Nodes];
                                                j++; j++;//Start.X
                                                j++; j++;//Start.Y
                                                j++; j++;//Start.Full
                                                j++; j++;//Start.FullLine
                                                j++; j++;//End.Name
                                                break;
                                            }
                                        }
                                    }

                                    for (_list_vm_Nodes = 0; _list_vm_Nodes < vm.Nodes.Count; _list_vm_Nodes++)
                                    {
                                        if (Convert.ToDouble(sp[j + 2]) == vm.Nodes[_list_vm_Nodes].X)
                                        {
                                            if (Convert.ToDouble(sp[j + 4]) == vm.Nodes[_list_vm_Nodes].Y)
                                            {
                                                j++; j++;//End.X
                                                newLine.End = vm.Nodes[_list_vm_Nodes];
                                                j++; j++;//End.Y
                                                j++; j++;//End.FullLine
                                                j++; j++;//X
                                                break;
                                            }
                                        }
                                    }
                                    newLine.X = Convert.ToDouble(sp[j]); j++; j++;
                                    newLine.Y = Convert.ToDouble(sp[j]); j++; j++;
                                    if (sp[j] == "True")
                                    {
                                        sp[j] = "true";
                                    }
                                    else
                                    {
                                        sp[j] = "false";
                                    }
                                    newLine.FullLine = Convert.ToBoolean(sp[j]);

                                    vm.Connectors.Add(newLine);
                                    ViewModel.List_Controls.Add(newLine);
                                }
                            }

                        }

                    }

                }
                catch (IOException ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            }

            private void Button_Delete_Controls(object sender, RoutedEventArgs e)
            {
                vm.CreatNewConnector = false;
                rect = null;
                node1 = null;

                //List_Controls
                int i_List_Controls;//在List_Controls中找选定线的位置

                //List_Connector
                int i_List_Connector;//在List_Connector中找选定线的位置

                //List_Capillary
                int i_List_Capillaries;//在List_Controls中找选定线的位置

                //List_Rect
                int i_List_Rect = 0;//在List_Rect中找选定线的位置
                int list_circuit = 0;//在Rect的Name中存放流路

                //List_Node
                int List_Node = 0;

                var item = ListBox_this.SelectedItem;
                vm.CreatNewConnector = false;

                string circuitnum_str="";//截取选定对象所在流路用
                int circuitnum=0;
                if (item != null)
                {
                    if (item.GetType().Name == "Connector" || item.GetType().Name == "Capillary")
                    {
                        if (item.GetType().Name == "Connector")
                        {
                            var selectelement = item as Connector;
                            circuitnum_str = selectelement.Name.Substring(selectelement.Name.LastIndexOf("CircuitNum") + 10, (selectelement.Name.Length - (selectelement.Name.LastIndexOf("CircuitNum") + 10)));
                            circuitnum = Convert.ToInt32(circuitnum_str);
                        }
                        else
                        {
                            var selectelement = item as Capillary;
                            circuitnum_str = selectelement.Name.Substring(selectelement.Name.LastIndexOf("CircuitNum") + 10, (selectelement.Name.Length - (selectelement.Name.LastIndexOf("CircuitNum") + 10)));//截取Connector所在流路
                            circuitnum = Convert.ToInt32(circuitnum_str);
                        }

                        //删除界面上的线
                        for (i_List_Connector = 0; i_List_Connector < vm.Connectors.Count; )
                        {
                            string _circuitnum_str = vm.Connectors[i_List_Connector].Name.Substring(vm.Connectors[i_List_Connector].Name.LastIndexOf("CircuitNum") + 10, (vm.Connectors[i_List_Connector].Name.Length - (vm.Connectors[i_List_Connector].Name.LastIndexOf("CircuitNum") + 10)));//截取Connector所在流路
                            int _circuitnum = Convert.ToInt32(_circuitnum_str);
                            if (_circuitnum == circuitnum)
                            {
                                for (List_Node = 0; List_Node < vm.Nodes.Count; List_Node++)
                                {
                                    if (vm.Nodes[List_Node].Name == vm.Connectors[i_List_Connector].Start.Name || vm.Nodes[List_Node].Name == vm.Connectors[i_List_Connector].End.Name)
                                    {
                                        vm.Nodes[List_Node].ConnectNum++;
                                    }
                                }
                                vm.Connectors.Remove(vm.Connectors[i_List_Connector]);
                            }
                            else
                            {
                                i_List_Connector++;
                            }
                        }

                        //删除界面上的毛细管
                        for (i_List_Capillaries = 0; i_List_Capillaries < vm.Capillaries.Count; )
                        {
                            string _circuitnum_str = vm.Capillaries[i_List_Capillaries].Name.Substring(vm.Capillaries[i_List_Capillaries].Name.LastIndexOf("CircuitNum") + 10, (vm.Capillaries[i_List_Capillaries].Name.Length - (vm.Capillaries[i_List_Capillaries].Name.LastIndexOf("CircuitNum") + 10)));//截取Connector所在流路
                            int _circuitnum = Convert.ToInt32(_circuitnum_str);
                            if (_circuitnum == circuitnum)
                            {
                                for (List_Node = 0; List_Node < vm.Nodes.Count; List_Node++)
                                {
                                    if (vm.Nodes[List_Node].Name == vm.Capillaries[i_List_Capillaries].Start.Name || vm.Nodes[List_Node].Name == vm.Capillaries[i_List_Capillaries].End.Name)
                                    {
                                        vm.Nodes[List_Node].ConnectNum++;
                                    }
                                }
                                vm.Capillaries.Remove(vm.Capillaries[i_List_Capillaries]);
                            }
                            else
                            {
                                i_List_Capillaries++;
                            }
                        }

                        //删除List_Controls上Connector
                        for (i_List_Controls = 0; i_List_Controls < ViewModel.List_Controls.Count; )
                        {
                            if (ViewModel.List_Controls[i_List_Controls].GetType().Name == "Connector")
                            {
                                var CurrentItem = ViewModel.List_Controls[i_List_Controls] as Connector;
                                string _circuitnum_str = CurrentItem.Name.Substring(CurrentItem.Name.LastIndexOf("CircuitNum") + 10, (CurrentItem.Name.Length - (CurrentItem.Name.LastIndexOf("CircuitNum") + 10)));//截取Connector所在流路
                                int _circuitnum = Convert.ToInt32(_circuitnum_str);
                                if (_circuitnum == circuitnum)
                                {
                                    ViewModel.List_Controls.Remove(ViewModel.List_Controls[i_List_Controls]);
                                }
                                else
                                {
                                    i_List_Controls++;
                                }
                            }
                            else
                            {
                                i_List_Controls++;
                            }
                        }

                        //删除List_Controls上Capillary
                        for (i_List_Controls = 0; i_List_Controls < ViewModel.List_Controls.Count; )
                        {
                            if (ViewModel.List_Controls[i_List_Controls].GetType().Name == "Capillary")
                            {
                                var CurrentItem = ViewModel.List_Controls[i_List_Controls] as Capillary;
                                string _circuitnum_str = CurrentItem.Name.Substring(CurrentItem.Name.LastIndexOf("CircuitNum") + 10, (CurrentItem.Name.Length - (CurrentItem.Name.LastIndexOf("CircuitNum") + 10)));//截取Connector所在流路
                                int _circuitnum = Convert.ToInt32(_circuitnum_str);
                                if (_circuitnum == circuitnum)
                                {
                                    ViewModel.List_Controls.Remove(ViewModel.List_Controls[i_List_Controls]);
                                }
                                else
                                {
                                    i_List_Controls++;
                                }
                            }
                            else
                            {
                                i_List_Controls++;
                            }
                        }
                    }
                    else if (item.GetType().Name == "Rect" && item != vm.Rects[0] && item != vm.Rects[1])
                    {
                        var selectelement = item as Rect;
                        circuitnum_str = selectelement.Name.Substring(selectelement.Name.LastIndexOf("HasCircuitNum") + 14, (selectelement.Name.Length - (selectelement.Name.LastIndexOf("HasCircuitNum") + 14)));

                        string[] sp = circuitnum_str.ToString().Split(new string[] { "," }, StringSplitOptions.None);

                        for (list_circuit = 0; list_circuit < sp.Length; list_circuit++)
                        {
                            circuitnum = Convert.ToInt32(sp[list_circuit]);

                            //删除界面上的线
                            for (i_List_Connector = 0; i_List_Connector < vm.Connectors.Count; )
                            {
                                string _circuitnum_str = vm.Connectors[i_List_Connector].Name.Substring(vm.Connectors[i_List_Connector].Name.LastIndexOf("CircuitNum") + 10, (vm.Connectors[i_List_Connector].Name.Length - (vm.Connectors[i_List_Connector].Name.LastIndexOf("CircuitNum") + 10)));//截取Connector所在流路
                                int _circuitnum = Convert.ToInt32(_circuitnum_str);
                                if (_circuitnum == circuitnum)
                                {
                                    for (List_Node = 0; List_Node < vm.Nodes.Count; List_Node++)
                                    {
                                        if (vm.Nodes[List_Node].Name == vm.Connectors[i_List_Connector].Start.Name || vm.Nodes[List_Node].Name == vm.Connectors[i_List_Connector].End.Name)
                                        {
                                            vm.Nodes[List_Node].ConnectNum++;
                                        }
                                    }
                                    vm.Connectors.Remove(vm.Connectors[i_List_Connector]);
                                }
                                else
                                {
                                    i_List_Connector++;
                                }
                            }

                            //删除界面上的毛细管
                            for (i_List_Capillaries = 0; i_List_Capillaries < vm.Capillaries.Count; )
                            {
                                string _circuitnum_str = vm.Capillaries[i_List_Capillaries].Name.Substring(vm.Capillaries[i_List_Capillaries].Name.LastIndexOf("CircuitNum") + 10, (vm.Capillaries[i_List_Capillaries].Name.Length - (vm.Capillaries[i_List_Capillaries].Name.LastIndexOf("CircuitNum") + 10)));//截取Connector所在流路
                                int _circuitnum = Convert.ToInt32(_circuitnum_str);
                                if (_circuitnum == circuitnum)
                                {
                                    for (List_Node = 0; List_Node < vm.Nodes.Count; List_Node++)
                                    {
                                        if (vm.Nodes[List_Node].Name == vm.Capillaries[i_List_Capillaries].Start.Name || vm.Nodes[List_Node].Name == vm.Capillaries[i_List_Capillaries].End.Name)
                                        {
                                            vm.Nodes[List_Node].ConnectNum++;
                                        }
                                    }
                                    vm.Capillaries.Remove(vm.Capillaries[i_List_Capillaries]);
                                }
                                else
                                {
                                    i_List_Capillaries++;
                                }
                            }

                            //删除List_Controls上Connector
                            for (i_List_Controls = 0; i_List_Controls < ViewModel.List_Controls.Count; )
                            {
                                if (ViewModel.List_Controls[i_List_Controls].GetType().Name == "Connector")
                                {
                                    var CurrentItem = ViewModel.List_Controls[i_List_Controls] as Connector;
                                    string _circuitnum_str = CurrentItem.Name.Substring(CurrentItem.Name.LastIndexOf("CircuitNum") + 10, (CurrentItem.Name.Length - (CurrentItem.Name.LastIndexOf("CircuitNum") + 10)));//截取Connector所在流路
                                    int _circuitnum = Convert.ToInt32(_circuitnum_str);
                                    if (_circuitnum == circuitnum)
                                    {
                                        ViewModel.List_Controls.Remove(ViewModel.List_Controls[i_List_Controls]);
                                    }
                                    else
                                    {
                                        i_List_Controls++;
                                    }
                                }
                                else
                                {
                                    i_List_Controls++;
                                }
                            }

                            //删除List_Controls上Capillary
                            for (i_List_Controls = 0; i_List_Controls < ViewModel.List_Controls.Count; )
                            {
                                if (ViewModel.List_Controls[i_List_Controls].GetType().Name == "Capillary")
                                {
                                    var CurrentItem = ViewModel.List_Controls[i_List_Controls] as Capillary;
                                    string _circuitnum_str = CurrentItem.Name.Substring(CurrentItem.Name.LastIndexOf("CircuitNum") + 10, (CurrentItem.Name.Length - (CurrentItem.Name.LastIndexOf("CircuitNum") + 10)));//截取Connector所在流路
                                    int _circuitnum = Convert.ToInt32(_circuitnum_str);
                                    if (_circuitnum == circuitnum)
                                    {
                                        ViewModel.List_Controls.Remove(ViewModel.List_Controls[i_List_Controls]);
                                    }
                                    else
                                    {
                                        i_List_Controls++;
                                    }
                                }
                                else
                                {
                                    i_List_Controls++;
                                }
                            }

                        }

                        //删除界面上的集管
                        vm.Rects.Remove(selectelement);

                        //删除List_Controls上Rect
                        for (i_List_Controls = 0; i_List_Controls < ViewModel.List_Controls.Count; )
                        {
                            if (selectelement.GetHashCode() == ViewModel.List_Controls[i_List_Controls].GetHashCode())
                            {
                                ViewModel.List_Controls.Remove(ViewModel.List_Controls[i_List_Controls]);
                                break;
                            }
                        }

                    }

                }
            }

            private void Button_Click_CopyCircuit(object sender, RoutedEventArgs e)
            {
                //List_Controls
                int i_List_Controls = 0;//在List_Controls中找选定线的位置

                //List_Connector
                int i_List_Connector = 0;//在List_Connector中找选定线的位置

                //List_Capillary
                int i_List_Capillary = 0;

                //List_Rect
                int i_List_Rect = 0;//在List_Rect中找选定线的位置

                for (i_List_Controls = 0; i_List_Controls < ViewModel.List_Controls.Count; i_List_Controls++)
                {
                    if (ViewModel.List_Controls[i_List_Controls].GetType().Name == "Capillary")
                    {
                        var CurrentItem = ViewModel.List_Controls[i_List_Controls] as Capillary;

                        vm.Capillaries.Add(CurrentItem);
                        i_List_Capillary++;
                    }
                    else if (ViewModel.List_Controls[i_List_Controls].GetType().Name == "Connector")
                    {
                        var CurrentItem = ViewModel.List_Controls[i_List_Controls] as Connector;

                        vm.Connectors.Add(CurrentItem);
                        i_List_Connector++;
                    }
                    else if (ViewModel.List_Controls[i_List_Controls].GetType().Name == "Rect")
                    {
                        var CurrentItem = ViewModel.List_Controls[i_List_Controls] as Rect;

                        vm.Rects.Add(CurrentItem);
                        i_List_Rect++;
                    }
                }
            }

            private void Button_Click_AutoArrange(object sender, RoutedEventArgs e)
            {
                vm.Connectors.Clear();
                vm.Capillaries.Clear();
                while (vm.Rects.Count>2)
                {
                    vm.Rects.RemoveAt(2);
                }
                ViewModel.List_Controls.Clear();
                //ViewModel.Circuit_Num++;
                ViewModel.Start_Capillary_Num = 0;
                ViewModel.End_Capillary_Num = 0;
                ViewModel.Connector_Num = 0;

                for (int List_Node = 0; List_Node < vm.Nodes.Count; List_Node++)
                {
                    vm.Nodes[List_Node].ConnectNum = 2;
                }

                #region CirArrange
                //**************下面这段求CirArrange的方法的代码和点击菜单中计算按钮后求CirArrange的方法的代码一样，为方便维护，以后宜提取出来共用
                Model.Basic.GeometryInput geoInput = new Model.Basic.GeometryInput();
                //几何结构输入
                geoInput.Nrow = Convert.ToInt16(Row.Text);//管排数
                geoInput.Ntube = Convert.ToInt16(tube_per.Text);//管数/排

                //流路
                int i = 1;
                int j = 1;
                int k = 1;
                int[,] CirArrange = new int[i, j];
                int[, ,] NodeInfo = new int[i, j, k];

                Model.Basic.CircuitNumber CircuitInfo = new Model.Basic.CircuitNumber();
                CircuitInfo.number = new int[] { Convert.ToInt32(Cirnum.Text), Convert.ToInt32(Cirnum.Text) };
                if (CircuitInfo.number[0] > geoInput.Ntube)//Avoid invalid Ncir input//管排数比管数还多
                {
                    throw new Exception("circuit number is beyond range.");
                }
                CircuitInfo.TubeofCir = new int[CircuitInfo.number[0]];
                //Get AutoCircuitry
                CircuitInfo = Model.Basic.AutoCircuiting.GetTubeofCir(geoInput.Nrow, geoInput.Ntube, CircuitInfo);
                CirArrange = new int[CircuitInfo.number[0], CircuitInfo.TubeofCir[CircuitInfo.number[0] - 1]];
                if (geoInput.Nrow % 2 == 0)
                {
                    CirArrange = Model.Basic.AutoCircuiting.GetCirArrange_2Row(CirArrange, geoInput.Nrow, geoInput.Ntube, CircuitInfo);
                }
                else if (geoInput.Nrow % 2 == 1)
                {
                    CirArrange = Model.Basic.AutoCircuiting.GetCirArrange_3Row(CirArrange, geoInput.Nrow, geoInput.Ntube, CircuitInfo);
                }
                #endregion

                #region 连线
                bool Line_Start = true;
                bool Line_complet = false;
                int remember_list_vm_Nodes = 0;
                bool remember_FullLine = true;

                for (int i_CirArrange = 0; i_CirArrange < CircuitInfo.number[0]; i_CirArrange++)
                {
                    for (int j_CirArrange = 0; j_CirArrange < CircuitInfo.TubeofCir[CircuitInfo.number[0] - 1]; j_CirArrange++)
                    {
                        for (int list_vm_Nodes = 0; list_vm_Nodes < vm.Nodes.Count; list_vm_Nodes++)
                        {
                            //Start,End
                            if (CirArrange[i_CirArrange, j_CirArrange].ToString() == vm.Nodes[list_vm_Nodes].Name)
                            {
                                #region 连Capillary
                                if (j_CirArrange == 0)
                                {
                                    //连StartCapillary
                                    Capillary newcapillary = new Capillary();

                                    newcapillary.Start = vm.Nodes[list_vm_Nodes];
                                    newcapillary.End = vm.Rects[0];
                                    newcapillary.Length = 0;
                                    newcapillary.Diameter = 0;
                                    newcapillary.X = vm.Nodes[0].X - vm.RowPitch + 5;
                                    newcapillary.FullLine = true;
                                    if (this.RefReverse.IsChecked == false)
                                    {
                                        newcapillary.In = true;
                                    }
                                    else
                                    {
                                        newcapillary.In = false;
                                    }
                                    ViewModel.Start_Capillary_Num++;

                                    //ViewModel.Circuit_Num = ViewModel.Circuit_Num + i_CirArrange + 1;
                                    ViewModel.Circuit_Num++;
                                    
                                    newcapillary.Name = Convert.ToString("New_Start_Capillary/StartCapillaryNum" + ViewModel.Start_Capillary_Num + "/CircuitNum" + ViewModel.Circuit_Num);
                                    vm.Nodes[list_vm_Nodes].ConnectNum--;
                                    vm.Capillaries.Add(newcapillary);
                                    ViewModel.List_Controls.Add(newcapillary);
                                }
                                else if (j_CirArrange == CircuitInfo.TubeofCir[CircuitInfo.number[0] - 1] - 1)
                                {
                                    //连EndCapillary
                                    Capillary newcapillary = new Capillary();

                                    newcapillary.Start = vm.Nodes[list_vm_Nodes];
                                    newcapillary.End = vm.Rects[1];
                                    newcapillary.Length = 0;
                                    newcapillary.Diameter = 0;
                                    newcapillary.X = vm.Nodes[list_vm_Nodes].X - vm.RowPitch + 5;

                                    if (j_CirArrange % 2 != 0)
                                    {
                                        remember_FullLine = false;
                                    }
                                    else
                                    {
                                        remember_FullLine = true;
                                    }

                                    if (this.RefReverse.IsChecked == false)
                                    {
                                        newcapillary.In = true;
                                    }
                                    else
                                    {
                                        newcapillary.In = false;
                                    }
                                    ViewModel.End_Capillary_Num++;
                                    //ViewModel.Circuit_Num = i_CirArrange;
                                    newcapillary.Name = Convert.ToString("New_End_Capillary/CapillaryNum" + ViewModel.End_Capillary_Num + "/CircuitNum" + ViewModel.Circuit_Num);
                                    vm.Nodes[list_vm_Nodes].ConnectNum--;
                                    vm.Capillaries.Add(newcapillary);
                                    ViewModel.List_Controls.Add(newcapillary);
                                }
                                #endregion

                                //连Connector里面有更新Rect的高度的region
                                #region 连Connector
                                Connector newLine = new Connector();

                                if (Line_Start == true)
                                {
                                    Line_Start = false;
                                    remember_list_vm_Nodes = list_vm_Nodes;
                                    
                                    if (j_CirArrange % 2 != 0)
                                    {
                                        remember_FullLine = true;
                                    }
                                    else 
                                    {
                                        remember_FullLine = false;
                                    }
                                    break;
                                }
                                else 
                                {
                                    newLine.Start = vm.Nodes[remember_list_vm_Nodes];
                                    newLine.End = vm.Nodes[list_vm_Nodes];
                                    newLine.FullLine = remember_FullLine;
                                    Line_Start = true;
                                    Line_complet = true;

                                }

                                if (j_CirArrange!= 0)
                                {
                                    if (j_CirArrange != CircuitInfo.TubeofCir[CircuitInfo.number[0] - 1]-1)
                                    {
                                        j_CirArrange--;
                                    }
                                }
                                //X,Y
                                //;;;;;;;;;;;;;;;

                                    #region 更新Rect长度
                                    for (int list_Rect = 0; list_Rect < 2; list_Rect++)
                                    {
                                        if (vm.Nodes[list_vm_Nodes].Y < vm.Rects[list_Rect].Y)
                                        {
                                            vm.Rects[list_Rect].RectHeight = vm.Rects[list_Rect].RectHeight + (vm.Rects[list_Rect].Y - vm.Nodes[list_vm_Nodes].Y);
                                            vm.Rects[list_Rect].Y = vm.Nodes[list_vm_Nodes].Y;
                                            //break;
                                        }
                                        else if (vm.Nodes[list_vm_Nodes].Y > vm.Rects[list_Rect].Y)
                                        {
                                            if (vm.Nodes[list_vm_Nodes].Y + 20 > vm.Rects[list_Rect].Y + vm.Rects[list_Rect].RectHeight)
                                            {
                                                vm.Rects[list_Rect].RectHeight = vm.Nodes[list_vm_Nodes].Y + 20 - vm.Rects[list_Rect].Y;
                                                //break;
                                            }
                                        }
                                    }
                                    #endregion

                                if (Line_complet==true)
                                {
                                    Line_complet = false;
                                    ViewModel.Connector_Num++;
                                    //ViewModel.Circuit_Num = i_CirArrange;
                                    newLine.Name = Convert.ToString("NewConnector/ConnectorNum" + ViewModel.Connector_Num + "/CircuitNum" + ViewModel.Circuit_Num);
                                    vm.Nodes[remember_list_vm_Nodes].ConnectNum--;
                                    vm.Nodes[list_vm_Nodes].ConnectNum--;
                                    vm.Connectors.Add(newLine);
                                    ViewModel.List_Controls.Add(newLine);

                                    //由于先储存了Capiliary才连线生成Connectors，Connectors后储存，所以调换一下位置
                                    if (ViewModel.List_Controls.Count - 2>0)
                                    {
                                        if (ViewModel.List_Controls[ViewModel.List_Controls.Count - 2].GetType().Name == "Capillary")
                                        {
                                            var currentitem = ViewModel.List_Controls[ViewModel.List_Controls.Count - 2] as Capillary;

                                            if (currentitem.Name.Contains("End"))
                                            {
                                                object obj = ViewModel.List_Controls[ViewModel.List_Controls.Count - 1];
                                                ViewModel.List_Controls[ViewModel.List_Controls.Count - 1] = ViewModel.List_Controls[ViewModel.List_Controls.Count - 2];
                                                ViewModel.List_Controls[ViewModel.List_Controls.Count - 2] = obj;//Capiliary存放的位置
                                            }
                                        }
                                    }


                                    break;
                                }
                                #endregion

                            }
                        }
                    }
                }
                #endregion



            }




    }

}


