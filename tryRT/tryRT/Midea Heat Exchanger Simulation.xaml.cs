﻿using System;
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

namespace tryRT
{
    /// <summary>
    /// Interaction logic for Midea_Heat_Exchanger_Simulation.xaml
    /// </summary>
    public partial class Midea_Heat_Exchanger_Simulation : Window
    {
        public Midea_Heat_Exchanger_Simulation()
        {
            InitializeComponent();
        }

        private void MenuItem_Click_Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
