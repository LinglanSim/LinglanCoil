using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace tryRT
{
    public class Connector : INotifyPropertyChanged
    {
        //public static List<Connector> List_Connector = new List<Connector>();

        private Node _start;
        public Node Start
        {
            get { return _start; }
            set
            {
                _start = value;
                OnPropertyChanged("Start");
            }
        }
        private Node _end;
        public Node End
        {
            get { return _end; }
            set
            {
                _end = value;
                OnPropertyChanged("End");
            }
        }
        public double X
        {
            get { return 0; }
            set { }
        }

        public double Y
        {
            get { return 0; }
            set { }
        }

        private bool _fullLine=true;
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
