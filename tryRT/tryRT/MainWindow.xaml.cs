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

namespace tryRT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //public int aq { get; set; }//①定义一个可读可写的公用的整型：getName

        private Object _parameter = null;
        public Object frmPara
        {
            get { return _parameter; }
            set { _parameter = value; }
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
            string[] sss = new string[3];
            //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
            sss[0] = "Do=7mm,Row=2,Pt=22mm,Pr=21mm";
            sss[1] = "Do=5mm,Row=2,Pt=14.5mm,Pr=12.56mm";
            sss[2] = "Do=8mm,Row=2,Pt=22mm,Pr=19.05mm";
            customList.Add(sss[0]);
            customList.Add(sss[1]);
            customList.Add(sss[2]);

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
            _parameter = r.N_row;
            int aaaa = 4;
            //Show("计算结果");

            //this.TabControl1.SelectedItem = this.TabControl1.Items[6];

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
            GUI.Window1 a = new GUI.Window1();
            //double aaa = Convert.ToDouble(Q.Text);
            int aaa = Convert.ToInt16(_parameter);
            a.getName = Convert.ToInt16(_parameter);
            a.Show();

            int aa = 5;


        }

    }
}
