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
using Model.Basic;

namespace GUI
{
    //people类
    public class people
    {
        public string tube { get; set; }
        public string element { get; set; }
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

    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        //创建people数组
        List<people> peopleList = new List<people>();
        //①定义一个可读可写的公用的字符串：getName
        public string[, ,] getName_tube { get; set; }
        public string[, ,] getName_element { get; set; }
        public string[, ,] getName_Pri { get; set; }
        public string[, ,] getName_Tri { get; set; }
        public string[, ,] getName_Hri { get; set; }
        public string[, ,] getName_Pro { get; set; }
        public string[, ,] getName_Tro { get; set; }
        public string[, ,] getName_Hro { get; set; }
        public string[, ,] getName_HTC { get; set; }
        public string[, ,] getName_Q { get; set; }
        public string[, ,] getName_mr { get; set; }

        public Window1()
        {
            InitializeComponent();
        }

        //创建dataGrid数据
        private void LoadData(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 5; j++)
                    for (int k = 0; k < 5; k++)
            {
                peopleList.Add(new people()
                {
                    tube = getName_tube[i, j, k],
                    element = getName_element[i, j, k],
                    Pri = getName_Pri[i, j, k],
                    Tri = getName_Tri[i, j, k],
                    Hri = getName_Hri[i, j, k],
                    Pro = getName_Pro[i, j, k],
                    Tro = getName_Tro[i, j, k],
                    Hro = getName_Hro[i, j, k],
                    HTC = getName_HTC[i, j, k],
                    Q = getName_Q[i, j, k],
                    mr = getName_mr[i, j, k],
                });
            }

            ((this.FindName("dataGrid")) as DataGrid).ItemsSource = peopleList;
            
        }
    }
}
