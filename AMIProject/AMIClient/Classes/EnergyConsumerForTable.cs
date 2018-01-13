using AMIClient.HelperClasses;
using FTN.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TC57CIM.IEC61970.Meas;
using TC57CIM.IEC61970.Wires;

namespace AMIClient
{
    public class EnergyConsumerForTable : INotifyPropertyChanged
    {
        private EnergyConsumer ami;
        private float currentP;
        private float currentQ;
        private float currentV;
        private Brush status;
        private DataGridType type;

        public EnergyConsumerForTable(EnergyConsumer ami)
        {
            this.Ami = ami;
            this.CurrentP = 0;
            this.currentQ = 0;
            this.CurrentV = 0;
            this.Status = new SolidColorBrush(Colors.Green);
        }

        public EnergyConsumer Ami
        {
            get
            {
                return ami;
            }

            set
            {
                ami = value;
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
