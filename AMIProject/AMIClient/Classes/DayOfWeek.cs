using AMIClient.HelperClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMIClient.Classes
{
    public class DayOfWeek : INotifyPropertyChanged
    {
        private Days name;
        private bool isChecked;

        public DayOfWeek(Days day)
        {
            this.Name = day;
            IsChecked = false;
        }

        public Days Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
                RaisePropertyChanged("Name");
            }
        }

        public bool IsChecked
        {
            get
            {
                return isChecked;
            }

            set
            {
                isChecked = value;
                RaisePropertyChanged("IsChecked");
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
