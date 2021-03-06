﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace tryRT
{
    public class Rect : INotifyPropertyChanged
    {
        //public static List<Rect> List_Rect = new List<Rect>();

        private double _x;
        public double X
        {
            get { return _x; }
            set
            {
                _x = value;
                OnPropertyChanged("X");
            }
        }
        private double _y;
        public double Y
        {
            get { return _y; }
            set
            {
                _y = value;
                OnPropertyChanged("Y");
            }
        }
        private double _rectheight;
        public double RectHeight
        {
            get { return _rectheight; }
            set
            {
                _rectheight = value;
                OnPropertyChanged("RectHeight");
            }
        }

        private bool _fullLine = true;
        public bool FullLine
        {
            get { return _fullLine; }
            set
            {
                _fullLine = value;
                OnPropertyChanged("FullLine");
            }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
