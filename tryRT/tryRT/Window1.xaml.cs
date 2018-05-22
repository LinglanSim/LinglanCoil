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
using System.Windows.Shapes;

namespace GUI
{
    //people类
    public class people
    {
        public string row { get; set; }
        public string heat { get; set; }
    }

    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        //创建people数组
        List<people> peopleList = new List<people>();

        public int getName { get; set; }//①定义一个可读可写的公用的整型：getName

        public Window1()
        {
            InitializeComponent();
        }

        //创建dataGrid数据
        private void LoadData(object sender, RoutedEventArgs e)
        {

            int a = getName;

            int row1 = getName;
            double[] Q = { 10, 20 };
            for (int i = 0; i < row1; i++)
            {
                peopleList.Add(new people()
                {
                    row = Convert.ToString(i),
                    heat = Convert.ToString(Q[i]),
                });
            }

            ((this.FindName("dataGrid")) as DataGrid).ItemsSource = peopleList;
            
        }
    }
}
