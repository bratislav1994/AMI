using AMIClient.HelperClasses;
using FTN.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TC57CIM.IEC61970.Core;
using TC57CIM.IEC61970.Meas;
using TC57CIM.IEC61970.Wires;

namespace AMIClient
{
    public class TableItem : INotifyPropertyChanged
    {
        private IdentifiedObject io;
        private float currentP;
        private float currentQ;
        private float currentV;
        private Brush status;
        private DataGridType type;

        public TableItem(IdentifiedObject io)
        {
            this.Io = io;
            this.CurrentP = 0;
            this.currentQ = 0;
            this.CurrentV = 0;
        }

        public IdentifiedObject Io
        {
            get
            {
                return io;
            }

            set
            {
                io = value;
            }
        }

        public float CurrentP
        {
            get
            {
                return currentP;
            }

            set
            {
                currentP = value;
                RaisePropertyChanged("CurrentP");
            }
        }

        public float CurrentQ
        {
            get
            {
                return currentQ;
            }

            set
            {
                currentQ = value;
                RaisePropertyChanged("CurrentQ");
            }
        }

        public float CurrentV
        {
            get
            {
                return currentV;
            }

            set
            {
                currentV = value;
                RaisePropertyChanged("CurrentV");
            }
        }

        public Brush Status
        {
            get
            {
                return status;
            }

            set
            {
                status = value;
                RaisePropertyChanged("Status");
            }
        }

        public DataGridType Type
        {
            get
            {
                return type;
            }

            set
            {
                this.Status = value == DataGridType.ENERGY_CONSUMER ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Transparent);
                type = value;
                RaisePropertyChanged("Type");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}
