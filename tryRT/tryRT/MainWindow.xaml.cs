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
        public MainWindow()
        {
            InitializeComponent();
            Bind1();
            Bind2();
            Bind3();
            Bind4();
            Bind5();
            Bind6();
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

        public void Bind2()
        {
            IList<string> customList = new List<string>();
            string[] sss = new string[2];
            sss[0] = "Evaporator";
            sss[1] = "Condenser";
            customList.Add(sss[0]);
            customList.Add(sss[1]);

            ComboBox2.ItemsSource = customList;
            ComboBox2.SelectedValue = customList[0];
        }

        public void Bind3()
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

        public void Bind4()
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

            ComboBox4.ItemsSource = customList;
            ComboBox4.SelectedValue = customList[0];
        }

        public void Bind5()
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

            ComboBox5.ItemsSource = customList;
            ComboBox5.SelectedValue = customList[0];
        }

        public void Bind6()
        {
            IList<string> customList = new List<string>();
            string[] sss = new string[3];
            //if curve = 1, geometry parameter is:Do:5mm,Pt:14.5mm,Pl:12.56mm,Fin_type:plain,Tf:0.095,Pf:1.2mm;
            //if curve = 4, geometry parameter is:Do:8mm,Pt:22mm,Pl:19.05mm,Fin_type:plain,Tf:0.1,Pf:1.6mm;
            sss[0] = "逆流";
            sss[1] = "顺流";
            sss[2] = "交叉流";
            customList.Add(sss[0]);
            customList.Add(sss[1]);
            customList.Add(sss[2]);

            ComboBox6.ItemsSource = customList;
            ComboBox6.SelectedValue = customList[0];
        }

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)//关闭按钮
        {
            this.Close();
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

        private void ComboBox5_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ComboBox6_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
