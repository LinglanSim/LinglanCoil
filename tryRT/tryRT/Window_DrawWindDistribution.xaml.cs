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
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Windows.Navigation;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace GUI
{
    /// <summary>
    /// Interaction logic for Window_DrawWindDistribution.xaml
    /// </summary>
    public partial class Window_DrawWindDistribution : Window
    {
        ViewModel vm = new ViewModel();
        public Window_DrawWindDistribution()
        {
            InitializeComponent();
            DataContext = vm;
        }
        private void Button_Click_Sure(object sender, RoutedEventArgs e)
        {
            int _row = Convert.ToInt32(NRow.Text);
            int _tube = Convert.ToInt32(NTube.Text);
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
