using AMIClient.HelperClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC57CIM.IEC61970.Core;
using TC57CIM.IEC61970.Wires;

namespace AMIClient.Classes
{
    public class TableItemForAlarm : INotifyPropertyChanged
    {
        private long id;
        private string consumer;
        private Status status;
        private DateTime fromPeriod;
        private DateTime? toPeriod;
        private TypeVoltage typeVoltage;

        public TableItemForAlarm()
        {
        }

        public Status Status
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

        public DateTime FromPeriod
        {
            get
            {
                return fromPeriod;
            }

            set
            {
                fromPeriod = value;
                RaisePropertyChanged("FromPeriod");
            }
        }

        public DateTime? ToPeriod
        {
            get
            {
                return toPeriod;
            }

            set
            {
                toPeriod = value;
                RaisePropertyChanged("ToPeriod");
            }
        }

        public TypeVoltage TypeVoltage
        {
            get
            {
                return typeVoltage;
            }

            set
            {
                typeVoltage = value;
                RaisePropertyChanged("Type");
            }
        }

        public string Consumer
        {
            get
            {
                return consumer;
            }

            set
            {
                consumer = value;
                RaisePropertyChanged("Consumer");
            }
        }

        public long Id
        {
            get
            {
                return id;
            }

            set
            {
                id = value;
                RaisePropertyChanged("Id");
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
