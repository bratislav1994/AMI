using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AMIClient.ViewModels
{
    public class DataGridViewModel : INotifyPropertyChanged
    {
        private static DataGridViewModel instance;
        private Model model;
        private object tableItem;

        public DataGridViewModel()
        {

        }

        public static DataGridViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DataGridViewModel();
                }

                return instance;
            }
        }

        public Model Model
        {
            get
            {
                return model;
            }

            set
            {
                model = value;
            }
        }

        public void SetModel(Model model)
        {
            this.Model = model;
        }

        private ICommand individualAmiChartCommand;

        public ICommand IndividualAmiChartCommand
        {
            get
            {
                return this.individualAmiChartCommand ?? (this.individualAmiChartCommand = new DelegateCommand<object>(this.SelectedAMIAction, param => true));
            }
        }

        private ICommand individualAmiHourChartCommand;

        public ICommand IndividualAmiHourChartCommand
        {
            get
            {
                return this.individualAmiHourChartCommand ?? (this.individualAmiHourChartCommand = new DelegateCommand<object>(this.SelectedAMIHourAction, param => true));
            }
        }

        private ICommand individualAmiDayChartCommand;

        public ICommand IndividualAmiDayChartCommand
        {
            get
            {
                return this.individualAmiDayChartCommand ?? (this.individualAmiDayChartCommand = new DelegateCommand<object>(this.SelectedAMIDayAction, param => true));
            }
        }

        public object TableItem
        {
            get
            {
                return tableItem;
            }

            set
            {
                if (((TableItem)value).Type == HelperClasses.DataGridType.ENERGY_CONSUMER)
                {
                    tableItem = value;
                }
                else
                {
                    tableItem = null;
                }
                RaisePropertyChanged("TableItem");
            }
        }

        private void SelectedAMIAction(object ami)
        {
            NetworkPreviewViewModel.Instance.SelectedAMIAction(ami, 1);
        }

        private void SelectedAMIHourAction(object ami)
        {
            NetworkPreviewViewModel.Instance.SelectedAMIAction(ami, 2);
        }

        private void SelectedAMIDayAction(object ami)
        {
            NetworkPreviewViewModel.Instance.SelectedAMIAction(ami, 3);
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
