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

        private void SelectedAMIAction(object ami)
        {
            NetworkPreviewViewModel.Instance.SelectedAMIAction(ami);
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
