using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Meas;
using TC57CIM.IEC61970.Wires;

namespace AMIClient
{
    public class EnergyConsumerForTable : INotifyPropertyChanged
    {
        private EnergyConsumer ami;
        float currentP;
        float currentQ;
        float currentV;

        public EnergyConsumerForTable(EnergyConsumer ami)
        {
            this.Ami = ami;
            this.CurrentP = 0;
            this.currentQ = 0;
            this.CurrentV = 0;
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
                RaisePropertyChanged("P");
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
                RaisePropertyChanged("Q");
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
                RaisePropertyChanged("v");
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
