using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Basic
{
    public class CheckDP
    {
        public CheckDP()
        {
            mr = new double[] { 0 };
            flag = true;
        }

        public CheckDP ShallowCopy()
        {
            return (CheckDP)this.MemberwiseClone();
        }

        private double[] _mr;
        public double[] mr
        {
           get
            {
                return _mr;

            }
            set
            {
                _mr = value;
            }
        }
        private bool _flag;
        public bool flag
        {
            get
            {
                return _flag;
            }
            set
            {
                _flag = value;
            }
        }
   }

}
