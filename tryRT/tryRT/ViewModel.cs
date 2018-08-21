using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace tryRT
{
    class ViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Node> _nodes;
        public ObservableCollection<Node> Nodes
        {
            get { return _nodes ?? (_nodes = new ObservableCollection<Node>()); }
        }

        private ObservableCollection<Connector> _connectors;
        public ObservableCollection<Connector> Connectors
        {
            get { return _connectors ?? (_connectors = new ObservableCollection<Connector>()); }
        }
        private ObservableCollection<Rect> _rects;
        public ObservableCollection<Rect> Rects
        {
            get { return _rects ?? (_rects = new ObservableCollection<Rect>()); }
        }
        private ObservableCollection<Capacity> _capacities;
        public ObservableCollection<Capacity> Capacities
        {
            get { return _capacities ?? (_capacities = new ObservableCollection<Capacity>()); }
        }


        private bool _creatNewConnector;
        public bool CreatNewConnector
        {
            get { return _creatNewConnector; }
            set
            {
                _creatNewConnector = value;
                OnPropertyChanged("CreatNewConnector");
            }
        }

        #region generate node and scale
        private double _panelWidth = 500;
        public double PanelWidth
        {
            get { return _panelWidth; }
            set
            {
                _panelWidth = value;
                OnPropertyChanged("PanelWidth");
            }
        }
        private double _panelHeight = 300;
        public double PanelHeight
        {
            get { return _panelHeight; }
            set
            {
                _panelHeight = value;
                OnPropertyChanged("PanelHeight");
            }
        }
        private double _tubePitch = 50;
        public double TubePitch
        {
            get { return _tubePitch; }
            set
            {
                _tubePitch = value;
                OnPropertyChanged("TubePitch");
            }
        }
        private double _rowPitch = 60;
        public double RowPitch
        {
            get { return _rowPitch; }
            set
            {
                _rowPitch = value;
                OnPropertyChanged("RowPitch");
            }
        }
        private double _scaleFactor = 1.0;
        public double ScaleFactor
        {
            get { return _scaleFactor; }
            set
            {
                _scaleFactor = value;
                OnPropertyChanged("ScaleFactor");
            }
        }
        public void GenerateNode(int Nrow, int Ntube)
        {
            PanelHeight = (Ntube + 3) * TubePitch < 500 ? 500 : (Ntube + 3) * TubePitch;
            PanelWidth = (Nrow + 2) * RowPitch < 500 ? 500 : (Nrow + 2) * RowPitch;
            Nodes.Clear();
            for (int i = 0; i < Ntube; i++)
            {
                for (int j = 0; j < Nrow; j++)
                {
                    Nodes.Add(new Node { X = (j + 2) * RowPitch, Y = (i + 2 - 0.5 * (j % 2)) * TubePitch, Name = ((Nrow - j - 1) * Ntube + i + 1).ToString() });
                }
            }
        }
        public void Zoom()
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                Nodes[i].X = Nodes[i].X * ScaleFactor;
                Nodes[i].Y = Nodes[i].Y * ScaleFactor;
            }
            for (int i = 0; i < Rects.Count; i++)
            {
                Rects[i].X = Rects[i].X * ScaleFactor;
                Rects[i].Y = Rects[i].Y * ScaleFactor;
                Rects[i].RectHeight = Rects[i].RectHeight * ScaleFactor;
            }
            PanelHeight = PanelHeight * ScaleFactor;
            PanelWidth = PanelWidth * ScaleFactor;
        }

        #endregion
        public ViewModel()
        {
            _nodes = new ObservableCollection<Node>();
            _rects = new ObservableCollection<Rect>();
            GenerateNode(2, 4);
            Rect inlet = new Rect();
            inlet.X = Nodes[0].X - RowPitch + 5;
            inlet.Y = Nodes[0].Y;
            inlet.RectHeight = 170;
            Rects.Add(inlet);
            Rect outlet = new Rect();
            outlet.X = Nodes[1].X + RowPitch + 5;
            outlet.Y = Nodes[0].Y - TubePitch / 2;
            outlet.RectHeight = inlet.RectHeight;
            Rects.Add(outlet);
        }




        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
