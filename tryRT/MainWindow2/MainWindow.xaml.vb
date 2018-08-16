Class MainWindow 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Practice
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// 2016年4月11日 BY 伊甸一点
    /// </summary>

    public partial class MainWindow : Window
    {
            Public MainWindow()
        {
            InitializeComponent();
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)//listview 加载时对gridview实现动态完成
        {
            GridView gridview = new GridView();
            gridview.Columns.Add(new GridViewColumn { Header = "ID", DisplayMemberBinding = new Binding("Id") });
            gridview.Columns.Add(new GridViewColumn { Header = "NAME", DisplayMemberBinding = new Binding("Name") });
            gridview.Columns.Add(new GridViewColumn { Header = "CATEGORY", DisplayMemberBinding = new Binding("Category") });
            listView1.View = gridview;
        }
        private void Button_Click(object sender, RoutedEventArgs e)//完成添加功能
        {
            string text1 = textBox1.Text;
            string text2 = textBox2.Text;
            string text3 = comoboBox1.Text.ToString();
            Boolean flag = false;//进行标记，flag == false 说明ID都不重复， flag == true 说明ID有重复
            if (text1 == "" || text2 == "" || text3 == "")
                MessageBox.Show("Incomplete information", "Tips");//提示信息不完整
            else
            {
                foreach (Book item in listView1.Items)//进行循环判断 item.id( Book的实例 )是否与listView1.Items的某个text1相等
                {
                    if (text1 == item.Id)
                    {
                        MessageBox.Show("Already have same ID number", "Tips");//提醒已经有相同ID存在
                        flag = true;//修改flag 
                    }
                }
            }
            if (!flag)//相当于 if( flag == false )
                listView1.Items.Add(new Book(text1, text2, text3));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)//完成删除功能
        {
            if (listView1.SelectedItem == null)//判断是否选择中ListView中的某行
                MessageBox.Show("Nothing have been choosed ", "Tips");
            else
                listView1.Items.Remove(listView1.SelectedItem);//删除选中的行
        }

    }
            Class Book
    {
        public Book(string ID, string NAME, string CATEGORY)//构造函数
        {
            Id = ID;
            Name = NAME;
            Category = CATEGORY;
        }
        private string id;//封装的要求
       //可以通过{ 右键--->重构--->封装字段 }实现自动完成get set函数
        //下面相同 
        public string Id//再次使用id 时只需要调用Id即可 
        {
            get { return id; }
            set { id = value; }
        }
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        private string category;

        public string Category
        {
            get { return category; }
            set { category = value; }
        }
    }
}
            End Class
